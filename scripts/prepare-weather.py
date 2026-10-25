#!/usr/bin/env python3
"""Build the pinned Wisconsin reference from committed GHCN-Daily subsets.

No network, imputation, climate forecast or crop response is performed here.
Python 3.9+, standard library only. See simulation-data/weather/README.md.
"""
import argparse
import calendar
from collections import Counter
from datetime import date, timedelta
import hashlib
import json
import math
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
DEFAULT = ROOT / "simulation-data/weather/coon-valley-v1"
ELEMENTS = ("PRCP", "TMAX", "TMIN")
DERIVATION = "terra-weather-drivers-v1"
SAMPLER = "terra-weather-contiguous-v1"


def encoded(value):
    return (json.dumps(value, indent=2, sort_keys=True, ensure_ascii=True) + "\n").encode("ascii")


def digest(data):
    return hashlib.sha256(data).hexdigest()


def dates(start, end):
    while start <= end:
        yield start
        start += timedelta(days=1)


def parse_dly(text, station):
    """Preserve value and three flags; missing calendar days stay absent."""
    result = {}
    for number, line in enumerate(text.splitlines(), 1):
        if len(line) != 269 or line[:11] != station or line[17:21] not in ELEMENTS:
            raise ValueError(f"Invalid .dly row {number} for {station}")
        year, month, element = int(line[11:15]), int(line[15:17]), line[17:21]
        count = calendar.monthrange(year, month)[1]
        for day in range(1, 32):
            offset = 21 + (day - 1) * 8
            value = int(line[offset:offset + 5])
            flags = tuple(line[offset + 5:offset + 8])
            if day > count:
                if value != -9999:
                    raise ValueError("Nonmissing observation on an impossible calendar date")
                continue
            key = (date(year, month, day), element)
            if key in result:
                raise ValueError(f"Duplicate observation {key}")
            result[key] = (value,) + flags
    return result


def invalid_reason(observation, element):
    if observation is None or observation[0] == -9999:
        return "missing"
    value, measurement, quality, source = observation
    if quality != " ":
        return "quality:" + quality
    if measurement in ("P", "I"):
        return "presumed-or-unreported-zero:" + measurement
    if not source.strip():
        return "missing-source"
    if element == "PRCP" and value < 0:
        return "negative-precipitation"
    return None


def coverage(data, year):
    invalid = {element: Counter() for element in ELEMENTS}
    inconsistent = 0
    for day in dates(date(year, 1, 1), date(year, 12, 31)):
        for element in ELEMENTS:
            reason = invalid_reason(data.get((day, element)), element)
            if reason:
                invalid[element][reason] += 1
        lo, hi = data.get((day, "TMIN")), data.get((day, "TMAX"))
        if not invalid_reason(lo, "TMIN") and not invalid_reason(hi, "TMAX") and lo[0] > hi[0]:
            inconsistent += 1
    prior_missing = sum(bool(invalid_reason(data.get((day, "PRCP")), "PRCP"))
                        for day in dates(date(year, 1, 1) - timedelta(days=7), date(year, 1, 1) - timedelta(days=1)))
    return {"year": year, "days": 366 if calendar.isleap(year) else 365,
            "invalidByElement": {el: dict(sorted(reasons.items())) for el, reasons in invalid.items()},
            "minAboveMaxDays": inconsistent, "invalidPreceding7Days": prior_missing,
            "eligible": not any(invalid.values()) and not inconsistent and not prior_missing}


def contiguous_windows(years, station):
    available = set(years)
    return [{"startYear": year, "recordIds": [f"{station}:{y}" for y in range(year, year + 10)]}
            for year in sorted(available) if all(y in available for y in range(year, year + 10))]


def derive_year(data, year, station):
    if not coverage(data, year)["eligible"]:
        raise ValueError(f"Incomplete or rejected weather year {year}")
    days = list(dates(date(year, 1, 1), date(year, 12, 31)))
    season = [day for day in days if 5 <= day.month <= 9]
    longest = running = 0
    for day in season:
        running = running + 1 if data[(day, "PRCP")][0] < 10 else 0
        longest = max(longest, running)
    events = []
    for day in days:
        precipitation = data[(day, "PRCP")][0]
        if precipitation < 250:
            continue
        events.append({"recordId": f"{station}:{day.isoformat()}:PRCP", "date": day.isoformat(),
                       "dayOfYear": day.timetuple().tm_yday, "month": day.month,
                       "inAnalysisWindow": 5 <= day.month <= 9, "precipTenthsMm": precipitation,
                       "antecedent7DaysTenthsMm": sum(data[(prior, "PRCP")][0] for prior in dates(
                           day - timedelta(days=7), day - timedelta(days=1))),
                       "tminTenthsC": data[(day, "TMIN")][0], "tmaxTenthsC": data[(day, "TMAX")][0]})
    flags = Counter(f"{el}:{''.join(data[(day, el)][1:])}" for day in days for el in ELEMENTS)
    return {"recordId": f"{station}:{year}", "stationId": station, "year": year,
            "days": len(days), "analysisWindowDays": len(season),
            "annualPrecipTenthsMm": sum(data[(day, "PRCP")][0] for day in days),
            "growingSeasonPrecipTenthsMm": sum(data[(day, "PRCP")][0] for day in season),
            "growingSeasonLongestDrySpellDays": longest,
            "growingSeasonHotDays": sum(data[(day, "TMAX")][0] >= 350 for day in season),
            "heavyPrecipitationEvents": events, "flagCounts": dict(sorted(flags.items()))}


def station_metadata(path, reference):
    output = {}
    for line in path.read_text(encoding="utf-8").splitlines():
        lat, lon = float(line[12:20]), float(line[21:30])
        dy, dx = math.radians(lat - reference["latitude"]), math.radians(lon - reference["longitude"])
        h = math.sin(dy / 2) ** 2 + math.cos(math.radians(lat)) * math.cos(math.radians(reference["latitude"])) * math.sin(dx / 2) ** 2
        output[line[:11]] = {"id": line[:11], "name": line[41:71].strip(), "latitude": lat,
                             "longitude": lon, "elevationM": float(line[31:37]),
                             "distanceFromReferenceKm": round(12742 * math.asin(min(1, math.sqrt(h))), 2)}
    return output


def daily_companion(data, lock):
    station = lock["selectedStationId"]
    years = []
    for year in range(lock["warmupYear"], lock["lastYear"] + 1):
        rows = []
        for day in dates(date(year, 1, 1), date(year, 12, 31)):
            observations = {el: data.get((day, el)) for el in ELEMENTS}
            if any(invalid_reason(observations[el], el) for el in ELEMENTS):
                raise ValueError(f"Daily companion requires complete, quality-clear observations: {day}")
            if observations["TMIN"][0] > observations["TMAX"][0]:
                raise ValueError(f"Daily companion minimum exceeds maximum: {day}")
            rows.append({"dayOfYear": day.timetuple().tm_yday,
                         "precipTenthsMm": observations["PRCP"][0],
                         "tminTenthsC": observations["TMIN"][0], "tmaxTenthsC": observations["TMAX"][0],
                         "precipFlags": "".join(observations["PRCP"][1:]),
                         "tminFlags": "".join(observations["TMIN"][1:]), "tmaxFlags": "".join(observations["TMAX"][1:])})
        years.append({"recordId": f"{station}:{year}", "year": year, "warmupOnly": year < lock["firstYear"], "days": rows})
    return {"schemaVersion": 1, "packId": lock["packId"], "stationId": station,
            "units": {"precipTenthsMm": "0.1 mm liquid-equivalent precipitation", "temperatureTenthsC": "0.1 degrees Celsius"},
            "calendar": "Gregorian; dayOfYear is 1-based within year; original station observation-day convention",
            "flags": "Three original characters in measurement, quality, source order; spaces significant",
            "years": years}


def build(directory):
    lock_bytes = (directory / "source-lock.json").read_bytes()
    lock = json.loads(lock_bytes)
    if lock["schemaVersion"] != 1:
        raise ValueError("Unknown source-lock schema")
    for source in lock["sources"]:
        path = (directory / source["path"]).resolve()
        if directory.resolve() not in path.parents:
            raise ValueError("Source path leaves the pack")
        data = path.read_bytes()
        if len(data) != source["bytes"] or digest(data) != source["sha256"]:
            raise ValueError(f"Pinned source mismatch: {source['path']}")
    metadata = station_metadata(directory / "source/nearby-stations.txt", lock["reference"])
    audit = {"schemaVersion": 1, "sourceLockSha256": digest(lock_bytes), "stations": []}
    parsed = {}
    years = range(lock["firstYear"], lock["lastYear"] + 1)
    for station in lock["candidateStationIds"]:
        source = next(item for item in lock["sources"] if item.get("stationId") == station)
        parsed[station] = parse_dly((directory / source["path"]).read_text(encoding="ascii"), station)
        checks = [coverage(parsed[station], year) for year in years]
        audit["stations"].append({"station": metadata[station], "years": checks,
                                  "eligibleWindows": contiguous_windows([c["year"] for c in checks if c["eligible"]], station)})
    selected = lock["selectedStationId"]
    selected_audit = next(s for s in audit["stations"] if s["station"]["id"] == selected)
    if not selected_audit["eligibleWindows"]:
        raise ValueError("No complete ten-year sequence")
    daily_bytes = encoded(daily_companion(parsed[selected], lock))
    pack = {"schemaVersion": 1, "packId": lock["packId"], "sourceLockSha256": digest(lock_bytes),
            "derivationVersion": DERIVATION, "reference": lock["reference"], "station": metadata[selected],
            "dailyCompanion": {"path": "daily-observations.json", "bytes": len(daily_bytes), "sha256": digest(daily_bytes),
                               "purpose": "Exact ordered observations, including warmup year, for separately versioned crop calendars and event timing; no post-harvest inference from annual aggregates"},
            "evidenceLevel": "derived-from-observed-daily-weather",
            "disclosure": "Fictional scenario · weather and economic references: Coon Valley, Wisconsin · other parameters are authored game assumptions.",
            "rules": {
                "analysisWindow": {"startMonthDay": "05-01", "endMonthDay": "09-30", "inclusive": True,
                                   "status": "authored fixed analysis calendar, not observed crop phenology"},
                "units": {"precipTenthsMm": "0.1 mm liquid-equivalent precipitation", "temperatureTenthsC": "0.1 degrees Celsius"},
                "dryDay": {"precipLessThanTenthsMm": 10, "longestSpellBoundary": "reset at May 1; count through September 30"},
                "hotDay": {"tmaxAtLeastTenthsC": 350},
                "eventSelection": {"precipAtLeastTenthsMm": 250, "period": "all calendar days", "grouping": "one event per qualifying day; consecutive events remain ordered",
                                   "antecedentDays": 7, "antecedentIncludesEventDay": False,
                                   "status": "authored extraction threshold, not a flood or rain-phase classification"},
                "completeness": "Every actual calendar day must have PRCP/TMAX/TMIN, no -9999, blank QFLAG, nonblank SFLAG, no P/I measurement flags, nonnegative PRCP and TMIN<=TMAX; preceding seven PRCP days also required. Reject the full year otherwise; no infill.",
                "trace": "Retain source flag T; numeric zero represents an unquantified trace, not measured no precipitation.",
                "continuity": "Ten adjacent historical calendar years at one station; preceding observed December supplies January antecedent. No repeated/wrapped or spliced years.",
                "limitations": ["Station weather is a shared regional reference, not parcel or other-terrain measurement.",
                                "Daily precipitation does not establish rain/snow phase, subdaily intensity, flood depth, frequency or damage.",
                                "Warm-season rain interpretation and crop/exposure/recovery response belong to separately versioned authored rules.",
                                "No soil-water stock, solar or wind generation driver is inferred.",
                                "Historical windows are illustrative scenarios, not future climate predictions or independent probability samples."]},
            "records": [derive_year(parsed[selected], check["year"], selected) for check in selected_audit["years"] if check["eligible"]],
            "eligibleWindows": selected_audit["eligibleWindows"]}
    pack_bytes = encoded(pack)
    receipt = {"schemaVersion": 1, "packId": pack["packId"], "path": "weather-pack.json", "bytes": len(pack_bytes),
               "sha256": digest(pack_bytes), "sourceLockSha256": digest(lock_bytes), "derivationVersion": DERIVATION}
    return {"weather-pack.json": pack_bytes, "daily-observations.json": daily_bytes,
            "coverage-audit.json": encoded(audit), "pack-lock.json": encoded(receipt)}


def schedule(pack_bytes, shared_seed):
    """Pin records once; seed must exclude mutable queue and branch/run identities."""
    if not isinstance(shared_seed, str) or not shared_seed or len(shared_seed.encode("utf-8")) > 256:
        raise ValueError("Shared scenario seed must be 1–256 UTF-8 bytes")
    pack = json.loads(pack_bytes)
    if pack.get("schemaVersion") != 1 or pack.get("derivationVersion") != DERIVATION:
        raise ValueError("Unsupported weather pack")
    windows = pack["eligibleWindows"]
    if not windows or windows != sorted(windows, key=lambda w: w["startYear"]):
        raise ValueError("Weather windows missing or not in canonical order")
    checksum = digest(pack_bytes)
    key = (SAMPLER + "\n" + checksum + "\n" + shared_seed).encode("utf-8")
    choice = int.from_bytes(hashlib.sha256(key).digest()[:4], "big") % len(windows)
    window = windows[choice]
    if len(window["recordIds"]) != 10 or len(set(window["recordIds"])) != 10:
        raise ValueError("A weather window must have ten distinct records")
    by_id = {record["recordId"]: record for record in pack["records"]}
    for index, record_id in enumerate(window["recordIds"]):
        record = by_id[record_id]
        if record["year"] != window["startYear"] + index or record["stationId"] != pack["station"]["id"]:
            raise ValueError("Weather window is not contiguous at the selected station")
    return {"schemaVersion": 1, "packId": pack["packId"], "packSha256": checksum,
            "samplerVersion": SAMPLER, "sharedScenarioSeed": shared_seed,
            "startYear": window["startYear"], "recordIds": list(window["recordIds"])}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--source", type=Path, default=DEFAULT)
    parser.add_argument("--output", type=Path, help="Default: source pack directory")
    parser.add_argument("--check", action="store_true", help="Verify committed products without writing")
    parser.add_argument("--schedule", metavar="SHARED_SEED", help="Print a pinned ten-year schedule")
    args = parser.parse_args()
    products = build(args.source)
    target = args.output or args.source
    if args.schedule is not None:
        print(encoded(schedule(products["weather-pack.json"], args.schedule)).decode("ascii"), end="")
    elif args.check:
        for name, content in products.items():
            if (target / name).read_bytes() != content:
                raise ValueError(f"Derived output differs: {name}")
        print("Verified pinned inputs and all four deterministic weather products")
    else:
        target.mkdir(parents=True, exist_ok=True)
        for name, content in products.items():
            (target / name).write_bytes(content)
        print(f"Prepared {len(products)} weather products at {target}")


if __name__ == "__main__":
    main()
