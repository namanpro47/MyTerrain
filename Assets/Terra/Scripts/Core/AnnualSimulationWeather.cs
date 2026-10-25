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
        // Generated from the exact verified pack/daily bytes and the v4 corn/soy
        // calendars. Game-year is zeroed; historical year and all driver contents
        // remain in the canonical digest. Regenerate only with a new rules version.
        static readonly string[] DriverDigests = {
            "c1772bd9a8faa837474d7d35ab07cb4b7ee6531ce220847e7c112c3fc41d77b3", // 2001
            "b1143c56420ee9d6f74d0706bf81a1416ccf9e24b3aa893a8245fe3f60e43b53", // 2002
            "f8641a5289dd8c079c7a8fdfb9f6297748afab9ec6349501687a1b211e9661a3", // 2003
            "0f7fc772a33010f7273bcc44319e021e8300144986e4008ba52646dd9c2486de", // 2004
            "25f0ac250791140d01bd2e37f49880bce22f99370cbec9e5d0345cc734046906", // 2005
            "573a422456a5731aa95797ffe4dce76718bfd24c20d3897270c1eba5b249f663", // 2006
            "076a8a66621a98cd4353f80d4acc10ef16a2cc3dba1bb0a6c5f49aa8b847008f", // 2007
            "42fa9cbd1279288c03a16957061b89e80b1e124fb88506c06e2bf6118785d554", // 2008
            "35dd8a92ac892c3e143febb9dbafda48fe637fcdabe25c1d3cf107cf36c5e287", // 2009
            "4084a636b11b86974b8acfb192e27b9ccad168c2ca522471c4b9e032989e0c3e", // 2010
            "5d1ca0bd5212512cdfc10f9111ca0f3eb8f9bca48105d80b6eaea3bb10071484", // 2011
            "29f9e5e56902c68963fb6c500a389b61b358b2c2774c14f35e8e9d28fc2152ee", // 2012
            "dc3a9fa7ae687a3d7eaf12acf06c49b3483d52f5e1ffe7bd95ef21c991087ec1", // 2013
            "6eadd801b659339ea1e37aec309945706895bdc69130afa0e1055edaf13f74ec", // 2014
            "cf48384ce8933f4a68551a4aa30f48c34b6016f7bd5680653f138d4e59d131d3", // 2015
            "e59ab8af724fafca5ac1759fb4295c9df268690bb2bbdbe4baf0547b4fa48cb9", // 2016
            "7c70c451eb42de1e6ef65cfbeb83e1f371aae9d47679d5ff756a2baabd59f21c", // 2017
            "d9a5ede5211894ebfd08fa14fa59da776c3249443657559d2628fbc59037e0b9", // 2018
            "5ef55753bd3dc1a0e33e9ec8daf80d430bd639cac4e76ce3209a9215b400a810", // 2019
            "db54c647344505d292cf32d08967af87a9489cbaeacbf68f8c200b58ab4db088", // 2020
            "0407e6898c12414fe0ab09123bf31695c8f71a45e6fb8485ebe5525cca337175", // 2021
            "a36aa6908c88b250cf621637b65981eedf9ae85ad2e9017c7fda497022a475e8", // 2022
            "49666c6e58efffc86449632e7fae0052eb7d95fadfa4ba1de79d5e209ffa0728", // 2023
            "ea9102a6ccbe5c029635a8db3a22adbbc2029543f204aa77d9de676422045644", // 2024
        };
        internal static void ValidateDriver(TownSimulationDriver driver)
        {
            AnnualTown.Require(driver != null && driver.historicalYear >= 2001 && driver.historicalYear <= 2024, "Unsupported historical driver.");
            var canonical=TownCopies.Copy(driver);canonical.year=0;
            AnnualTown.Require(TownCopies.Hash(canonical)==DriverDigests[driver.historicalYear-2001], "Driver contents differ from the pinned weather/calendar derivation.");
        }
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

        internal static int ExpectedStartYear(string seed)
        {
            AnnualTown.Require(!string.IsNullOrEmpty(seed) && Utf8.GetByteCount(seed) <= 256, "Scenario seed must contain 1–256 UTF-8 bytes.");
            byte[] digest;
            using (var sha = SHA256.Create()) digest = sha.ComputeHash(Utf8.GetBytes(Sampler + "\n" + PackHash + "\n" + seed));
            uint first = ((uint)digest[0] << 24) | ((uint)digest[1] << 16) | ((uint)digest[2] << 8) | digest[3];
            return 2001 + (int)(first % 15u);
        }
        public TownWeatherSchedule Select(string seed)
        {
            var window = pack.eligibleWindows[ExpectedStartYear(seed)-2001];
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
