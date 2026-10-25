"""Data failures, calendar boundaries and cross-language schedule contract."""
import calendar
from datetime import date, timedelta
import importlib.util
import json
from pathlib import Path
import shutil
import tempfile
import unittest


ROOT = Path(__file__).resolve().parents[2]
spec = importlib.util.spec_from_file_location("weather", ROOT / "scripts/prepare-weather.py")
weather = importlib.util.module_from_spec(spec)
spec.loader.exec_module(weather)
STATION = "USW00014920"


def row(year=2024, month=2, element="PRCP", value=10):
    count = calendar.monthrange(year, month)[1]
    return f"{STATION}{year:04}{month:02}{element}" + "".join(
        f"{value:5}  W" if day <= count else "-9999   " for day in range(1, 32))


def fixture(year=2024):
    return {(day, el): (10 if el == "PRCP" else 200 if el == "TMAX" else 100, " ", " ", "W")
            for day in weather.dates(date(year, 1, 1) - timedelta(days=7), date(year, 12, 31))
            for el in weather.ELEMENTS}


class ParserChecks(unittest.TestCase):
    def test_leap_day_and_padding(self):
        self.assertEqual(len(weather.parse_dly(row(), STATION)), 29)
        self.assertEqual(len(weather.parse_dly(row(2023), STATION)), 28)
        self.assertEqual(weather.parse_dly(row(), STATION)[(date(2024, 2, 29), "PRCP")], (10, " ", " ", "W"))

    def test_malformed_and_duplicate_rows_rejected(self):
        for text in [row()[:-1], row() + "\n" + row(), "BAD" + row()[3:], row()[:-8] + "   10  W"]:
            with self.subTest(text=text[:21]), self.assertRaises(ValueError):
                weather.parse_dly(text, STATION)

    def test_missing_quality_and_presumed_zero_are_not_dry_weather(self):
        for observation in [None, (-9999, " ", " ", " "), (0, " ", "S", "W"),
                            (0, "P", " ", "0"), (0, "I", " ", "W"), (-1, " ", " ", "W"), (0, " ", " ", " ")]:
            with self.subTest(observation=observation):
                self.assertIsNotNone(weather.invalid_reason(observation, "PRCP"))
        self.assertIsNone(weather.invalid_reason((0, "T", " ", "W"), "PRCP"))
        self.assertIsNone(weather.invalid_reason((-100, " ", " ", "W"), "TMIN"))


class DerivationChecks(unittest.TestCase):
    def test_missing_day_rejects_entire_year(self):
        data = fixture()
        del data[(date(2024, 7, 4), "TMAX")]
        self.assertEqual(weather.coverage(data, 2024)["invalidByElement"]["TMAX"], {"missing": 1})
        with self.assertRaises(ValueError):
            weather.derive_year(data, 2024, STATION)

    def test_temperature_inconsistency_rejected(self):
        data = fixture()
        data[(date(2024, 8, 1), "TMIN")] = (300, " ", " ", "W")
        self.assertEqual(weather.coverage(data, 2024)["minAboveMaxDays"], 1)
        self.assertFalse(weather.coverage(data, 2024)["eligible"])

    def test_missing_warmup_rejected_instead_of_assumed_zero(self):
        data = fixture()
        del data[(date(2023, 12, 28), "PRCP")]
        self.assertEqual(weather.coverage(data, 2024)["invalidPreceding7Days"], 1)
        with self.assertRaises(ValueError):
            weather.derive_year(data, 2024, STATION)

    def test_event_timing_units_and_cross_year_antecedent(self):
        data = fixture()
        for day in weather.dates(date(2023, 12, 25), date(2023, 12, 31)):
            data[(day, "PRCP")] = (20, " ", " ", "W")
        for day, value in [(date(2024, 1, 1), 250), (date(2024, 1, 2), 300), (date(2024, 1, 3), 249)]:
            data[(day, "PRCP")] = (value, " ", " ", "W")
        result = weather.derive_year(data, 2024, STATION)
        events = result["heavyPrecipitationEvents"]
        self.assertEqual(len(events), 2)
        self.assertEqual([e["dayOfYear"] for e in events], [1, 2])
        self.assertEqual([e["antecedent7DaysTenthsMm"] for e in events], [140, 370])
        self.assertFalse(events[0]["inAnalysisWindow"])
        self.assertEqual(events[0]["precipTenthsMm"], 250)
        self.assertEqual(result["days"], 366)

    def test_analysis_window_dry_spell_heat_and_trace_boundaries(self):
        data = fixture()
        for day in [date(2024, 4, 30), date(2024, 5, 1), date(2024, 5, 2), date(2024, 5, 3), date(2024, 9, 30), date(2024, 10, 1)]:
            data[(day, "PRCP")] = (0, "T", " ", "W")
        data[(date(2024, 5, 4), "TMAX")] = (350, " ", " ", "W")
        data[(date(2024, 5, 5), "TMAX")] = (349, " ", " ", "W")
        data[(date(2024, 10, 1), "TMAX")] = (400, " ", " ", "W")
        result = weather.derive_year(data, 2024, STATION)
        self.assertEqual(result["analysisWindowDays"], 153)
        self.assertEqual(result["growingSeasonPrecipTenthsMm"], 1490)
        self.assertEqual(result["growingSeasonLongestDrySpellDays"], 3)
        self.assertEqual(result["growingSeasonHotDays"], 1)
        self.assertEqual(result["flagCounts"]["PRCP:T W"], 6)

    def test_windows_do_not_wrap_or_bridge_missing_year(self):
        self.assertEqual(weather.contiguous_windows(list(range(2001, 2010)) + list(range(2011, 2021)), STATION),
                         [{"startYear": 2011, "recordIds": [f"{STATION}:{year}" for year in range(2011, 2021)]}])


class PinnedDataChecks(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.products = weather.build(weather.DEFAULT)
        cls.pack_bytes = cls.products["weather-pack.json"]
        cls.pack = json.loads(cls.pack_bytes)

    def test_rebuild_is_byte_exact(self):
        self.assertEqual(self.products, weather.build(weather.DEFAULT))
        for name, content in self.products.items():
            self.assertEqual(content, (weather.DEFAULT / name).read_bytes(), name)

    def test_source_tamper_fails_before_derivation(self):
        with tempfile.TemporaryDirectory() as folder:
            target = Path(folder) / "weather"
            shutil.copytree(weather.DEFAULT, target)
            source = target / "source/USW00014920.2000-2024.dly"
            source.write_bytes(source.read_bytes() + b"\n")
            with self.assertRaisesRegex(ValueError, "Pinned source mismatch"):
                weather.build(target)

    def test_complete_calendar_and_quality_audit(self):
        self.assertEqual(len(self.pack["records"]), 24)
        self.assertEqual(sum(r["days"] for r in self.pack["records"]), 8766)
        self.assertEqual([w["startYear"] for w in self.pack["eligibleWindows"]], list(range(2001, 2016)))
        audit = json.loads(self.products["coverage-audit.json"])
        starts = {entry["station"]["id"]: [w["startYear"] for w in entry["eligibleWindows"]] for entry in audit["stations"]}
        self.assertEqual(starts["USC00474373"], [2015])
        self.assertEqual(starts["USC00473038"], [])
        self.assertEqual(starts["USC00478827"], [])

    def test_independent_observed_event_regression(self):
        record = next(r for r in self.pack["records"] if r["year"] == 2018)
        event = next(e for e in record["heavyPrecipitationEvents"] if e["date"] == "2018-08-27")
        self.assertEqual(event["precipTenthsMm"], 772)
        self.assertEqual(event["antecedent7DaysTenthsMm"], 186)
        self.assertEqual(event["dayOfYear"], 239)
        self.assertEqual(record["growingSeasonPrecipTenthsMm"], 6909)

    def test_schedule_golden_identity_and_saved_records(self):
        selected = weather.schedule(self.pack_bytes, "coon-valley-demo-1")
        self.assertEqual(selected["packSha256"], "5bf2c305473f2d1fb4b3a589f91995032bcd44b4551c0c4278745b08786540de")
        self.assertEqual(selected["startYear"], 2015)
        self.assertEqual(selected["recordIds"], [f"{STATION}:{year}" for year in range(2015, 2025)])
        self.assertEqual(selected, weather.schedule(self.pack_bytes, "coon-valley-demo-1"))
        # The sampler accepts only shared seed and pack bytes, never queue/run/branch identity.
        with self.assertRaises(TypeError):
            weather.schedule(self.pack_bytes, "coon-valley-demo-1", run_id="other-branch")

    def test_different_scenarios_can_select_each_existing_window(self):
        starts = {weather.schedule(self.pack_bytes, f"test-{i}")["startYear"] for i in range(256)}
        self.assertEqual(starts, set(range(2001, 2016)))

    def test_daily_companion_hash_calendar_and_warmup(self):
        content = self.products["daily-observations.json"]
        self.assertEqual(weather.digest(content), self.pack["dailyCompanion"]["sha256"])
        self.assertEqual(len(content), self.pack["dailyCompanion"]["bytes"])
        years = json.loads(content)["years"]
        self.assertEqual([r["year"] for r in years], list(range(2000, 2025)))
        self.assertEqual(sum(len(r["days"]) for r in years), 9132)
        self.assertEqual([r["year"] for r in years if r["warmupOnly"]], [2000])
        for record in years:
            self.assertEqual([d["dayOfYear"] for d in record["days"]], list(range(1, (366 if calendar.isleap(record["year"]) else 365) + 1)))
        self.assertTrue(all(f"{STATION}:2000" not in w["recordIds"] for w in self.pack["eligibleWindows"]))

    def test_daily_companion_keeps_post_harvest_date_and_original_flags(self):
        years = json.loads(self.products["daily-observations.json"])["years"]
        days = next(y["days"] for y in years if y["year"] == 2018)
        # Late-year observed precipitation remains on November 4, not a crop-season multiplier.
        november = days[307]
        self.assertEqual(november["dayOfYear"], 308)
        self.assertEqual(november["precipTenthsMm"], 450)
        self.assertEqual(november["tmaxTenthsC"], 78)
        self.assertTrue(any(d["precipFlags"].startswith("T") for d in days))
        for day in days:
            self.assertEqual(len(day["precipFlags"]), 3)
            self.assertEqual(day["precipFlags"][1], " ")

    def test_invalid_seed_rejected(self):
        for seed in [None, "", "x" * 257, "🌧" * 65]:
            with self.subTest(seed=seed), self.assertRaises(ValueError):
                weather.schedule(self.pack_bytes, seed)

    def test_noncontiguous_or_duplicate_schedule_rejected(self):
        pack = json.loads(self.pack_bytes)
        pack["eligibleWindows"] = [{"startYear": 2001, "recordIds": [f"{STATION}:2001"] * 10}]
        with self.assertRaises(ValueError):
            weather.schedule(weather.encoded(pack), "test")
        pack["eligibleWindows"][0]["recordIds"] = [f"{STATION}:{y}" for y in range(2001, 2010)] + [f"{STATION}:2011"]
        with self.assertRaises(ValueError):
            weather.schedule(weather.encoded(pack), "test")


if __name__ == "__main__":
    unittest.main()
