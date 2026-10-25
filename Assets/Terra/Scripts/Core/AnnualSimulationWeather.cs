using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Terra.Contracts;

namespace Terra.Core
{
    // Immutable, offline reference consumer. No choice/run/branch ID enters the sampler.
    public sealed class AnnualSimulationWeather
    {
        public const string PackId = "coon-valley-weather-v1";
        public const string PackHash = "5bf2c305473f2d1fb4b3a589f91995032bcd44b4551c0c4278745b08786540de";
        public const string DailyHash = "ce1128972981490c28c74bb4e291725345e7c0a4674781c460a4d987fbbbc840";
        public const string Sampler = "terra-weather-contiguous-v1";
        public const string Derivation = "terra-weather-drivers-v1";
        public const string Disclosure = "Fictional scenario · weather and economic references: Coon Valley, Wisconsin · other parameters are authored game assumptions.";
        static readonly UTF8Encoding Utf8 = new UTF8Encoding(false,true);
        readonly TownWeatherPack pack;
        readonly TownDailyWeather daily;

        public AnnualSimulationWeather(byte[] packBytes, byte[] dailyBytes,
            Func<string,TownWeatherPack> readPack, Func<string,TownDailyWeather> readDaily)
        {
            AnnualTown.Require(packBytes != null && Sha(packBytes) == PackHash, "Unknown or changed weather pack bytes.");
            AnnualTown.Require(dailyBytes != null && Sha(dailyBytes) == DailyHash, "Unknown or changed daily weather bytes.");
            pack = TownCopies.Copy(readPack(Encoding.UTF8.GetString(packBytes)));
            daily = TownCopies.Copy(readDaily(Encoding.UTF8.GetString(dailyBytes)));
            AnnualTown.Require(pack != null && pack.schemaVersion == 1 && pack.packId == PackId && pack.derivationVersion == Derivation
                && pack.eligibleWindows?.Length == 15 && pack.records?.Length == 24, "Unsupported weather derivation/schema.");
            AnnualTown.Require(daily != null && daily.schemaVersion == 1 && daily.packId == PackId && daily.stationId == "USW00014920"
                && daily.years?.Length == 25, "Unsupported daily weather schema.");
            for (int i = 0; i < pack.eligibleWindows.Length; i++)
            {
                var window = pack.eligibleWindows[i];
                AnnualTown.Require(window.startYear == 2001 + i && window.recordIds != null
                    && window.recordIds.SequenceEqual(Enumerable.Range(window.startYear,10).Select(y => daily.stationId + ":" + y)), "Noncontiguous weather schedule.");
            }
            foreach (var year in daily.years)
            {
                AnnualTown.Require(year.days != null && year.days.Length == (DateTime.IsLeapYear(year.year) ? 366 : 365), "Incomplete weather year.");
                for (int i = 0; i < year.days.Length; i++)
                {
                    var day = year.days[i];
                    AnnualTown.Require(day.dayOfYear == i + 1 && day.precipTenthsMm >= 0 && day.tminTenthsC <= day.tmaxTenthsC
                        && ValidFlag(day.precipFlags) && ValidFlag(day.tminFlags) && ValidFlag(day.tmaxFlags), "Missing or rejected weather day.");
                }
            }
        }
        static bool ValidFlag(string flag) => flag != null && flag.Length == 3 && flag[1] == ' ' && flag[2] != ' ' && flag[0] != 'P' && flag[0] != 'I';
        static string Sha(byte[] bytes)
        { using (var sha = SHA256.Create()) return string.Concat(sha.ComputeHash(bytes).Select(b => b.ToString("x2"))); }

        public TownWeatherSchedule Select(string seed)
        {
            AnnualTown.Require(!string.IsNullOrEmpty(seed) && Utf8.GetByteCount(seed) <= 256, "Scenario seed must contain 1–256 UTF-8 bytes.");
            byte[] digest;
            using (var sha = SHA256.Create()) digest = sha.ComputeHash(Utf8.GetBytes(Sampler + "\n" + PackHash + "\n" + seed));
            uint first = ((uint)digest[0] << 24) | ((uint)digest[1] << 16) | ((uint)digest[2] << 8) | digest[3];
            var window = pack.eligibleWindows[first % (uint)pack.eligibleWindows.Length];
            return new TownWeatherSchedule { packId = PackId, packSha256 = PackHash, dailySha256 = DailyHash,
                samplerVersion = Sampler, sharedScenarioSeed = seed, startYear = window.startYear, recordIds = window.recordIds.ToArray() };
        }
        public void ValidateSchedule(TownWeatherSchedule saved)
        {
            AnnualTown.Require(saved != null && TownCopies.Hash(saved) == TownCopies.Hash(Select(saved.sharedScenarioSeed)), "Saved weather schedule or identity changed.");
        }
        public TownSimulationDriver Driver(TownWeatherSchedule schedule, int year, TownCropDefinition[] crops)
        {
            ValidateSchedule(schedule);
            AnnualTown.Require(year >= 1 && year <= 10 && crops != null && crops.Length > 0 && crops.Select(c => c.id).Distinct().Count() == crops.Length, "Invalid weather driver request.");
            var record = pack.records.Single(r => r.recordId == schedule.recordIds[year - 1]);
            var days = daily.years.Single(y => y.year == record.year).days;
            return new TownSimulationDriver { year = year, historicalYear = record.year, recordId = record.recordId, packSha256 = PackHash,
                derivationVersion = "terra-crop-calendar-weather-v1", crops = crops.Select(c => Summarize(days,record.year,c)).ToArray(),
                events = TownCopies.Copy(record.heavyPrecipitationEvents), solarFactor = 1, windFactor = 1,
                generationRuleId = "authored-constant-generation-v1-no-irradiance-or-wind-data" };
        }
        static TownCropWeather Summarize(TownWeatherDay[] days, int year, TownCropDefinition crop)
        {
            int first = new DateTime(year,crop.sowMonth,crop.sowDay).DayOfYear;
            int last = new DateTime(year,crop.harvestMonth,crop.harvestDay).DayOfYear;
            AnnualTown.Require(first <= last && !string.IsNullOrWhiteSpace(crop.id), "Only explicit same-calendar-year crops are supported.");
            var result = new TownCropWeather { cropId = crop.id, ruleId = "authored-crop-calendar-v1", firstDay = first, harvestDay = last };
            int dry = 0;
            foreach (var day in days.Where(d => d.dayOfYear >= first && d.dayOfYear <= last))
            {
                result.precipTenthsMm += day.precipTenthsMm;
                dry = day.precipTenthsMm < 10 ? dry + 1 : 0;
                result.longestDrySpellDays = Math.Max(result.longestDrySpellDays,dry);
                if (day.tmaxTenthsC >= 350) result.hotDays++;
            }
            return result;
        }
    }
}
