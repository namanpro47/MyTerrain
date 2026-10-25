using System.Text.Json;
using Terra.Contracts;
using Terra.Core;

static class PlanningGuidanceChecks
{
    public static int Run(AnnualTownDefinition definition)
    {
        int checks = 0;
        var json = new JsonSerializerOptions { IncludeFields = true };
        string Encode<T>(T value) => JsonSerializer.Serialize(value, json);
        AnnualTownDefinition Copy() => JsonSerializer.Deserialize<AnnualTownDefinition>(Encode(definition), json);
        void Check(bool value, string reason) { checks++; if (!value) throw new Exception(reason); }
        var provider = new AuthoredAnnualConsequenceProvider();
        AnnualPlanningGuidance Advice(AnnualTown town)
        {
            string before = Encode(town.Snapshot());
            var current = town.History.LastOrDefault()?.outcomes ?? AnnualGameplay.Baseline(town.Definition);
            var result = AnnualPlanningGuidance.Create(town, current);
            Check(before == Encode(town.Snapshot()), "Guidance mutated live state");
            Check(!string.IsNullOrWhiteSpace(result.message) || !string.IsNullOrWhiteSpace(result.actionId), "Empty guidance");
            if (result.actionId != null)
            {
                Check(town.Preview(result.siteId, result.actionId) >= 0, "Unaffordable recommendation");
                Check(!town.Assets.Any(a => a.siteId == result.siteId), "Recommendation used accepted site");
                var recipe = town.Definition.actions.First(a => a.siteId == result.siteId && a.actionId == result.actionId);
                Check(town.Year + recipe.constructionYears <= definition.years, "Recommendation cannot open before ending");
            }
            return result;
        }
        var town = new AnnualTown(definition, "guidance-stages");
        Check(Advice(town).message.Contains("inspect"), "Arrival should teach inspection");
        var park = definition.actions.First(a => a.actionId == "public-park");
        town.QueueAction(park.siteId, park.actionId, 0);
        Check(Advice(town).message.Contains("not paid"), "Queued guide omitted commitment boundary");
        town.BeginResolution("guidance-year1", town.Revision);
        Check(Advice(town).message.Contains("pending"), "Pending guide invites editing");
        AnnualTownDemo.Advance(town, "guidance-year1", provider);
        Check(Advice(town).message.Contains("what changed"), "Report should invite understanding");
        town.ContinuePlanning(town.Revision);
        Check(Advice(town).need == "Housing need", "Next plan should identify an unmet need");
        AnnualTownDemo.Finish(town, provider);
        Check(Advice(town).message.Contains("complete"), "Completed guide invites another year");

        var limited = Copy(); limited.startingCashCents = 6000000; // Can buy $50K park, cannot reserve its $70K full term.
        var budget = new AnnualTown(limited, "guidance-full-reserve");
        Check(Advice(budget).actionId == null && Advice(budget).message.Contains("upkeep and debt"), "Guide spent future money or omitted Wait obligations");

        var apartments = new AnnualTown(definition, "guidance-already-building");
        var home = definition.actions.First(a => a.actionId == "high-density-housing");
        apartments.QueueAction(home.siteId, home.actionId, 0);
        AnnualTownDemo.Advance(apartments, "housing-year1", provider);
        apartments.ContinuePlanning(apartments.Revision);
        Check(Advice(apartments).need != "Housing need", "Guide ignored already-funded homes under construction");

        var scarce = Copy(); scarce.economy.baselineWaterSupply = 10;
        var water = new AnnualTown(scarce, "guidance-water");
        AnnualTownDemo.Advance(water, "water-year1", provider); water.ContinuePlanning(water.Revision);
        Check(Advice(water).actionId == null && Advice(water).message.Contains("adds no supply"), "Wastewater or Wait portrayed as new drinking water");

        scarce = Copy(); scarce.economy.baselineEnergySupply = 5; scarce.economy.baselineCleanEnergySupply = 5;
        var power = new AnnualTown(scarce, "guidance-power");
        AnnualTownDemo.Advance(power, "power-year1", provider); power.ContinuePlanning(power.Revision);
        var energyAdvice = Advice(power);
        Check(energyAdvice.need == "Power need", "Guide added demand before addressing power");
        power.QueueAction(energyAdvice.siteId, energyAdvice.actionId, power.Revision);
        AnnualTownDemo.Advance(power, "power-year2", provider); power.ContinuePlanning(power.Revision);
        Check(Advice(power).actionId == null && Advice(power).message.Contains("timing or conditions"), "Guide duplicates power already under construction");

        var allSites = new AnnualTown(definition, "guidance-no-vacancy");
        foreach (var group in definition.actions.GroupBy(a => a.siteId))
        {
            var offer = group.OrderBy(a => a.upfrontPublicCostCents + a.annualPublicOperatingCents * a.operatingYears).First();
            allSites.QueueAction(offer.siteId, offer.actionId, allSites.Revision);
        }
        AnnualTownDemo.Advance(allSites, "full-year1", provider); allSites.ContinuePlanning(allSites.Revision);
        Check(Advice(allSites).message.Contains("sites are committed"), "Guide invites redevelopment/refunds");

        var late = new AnnualTown(definition, "guidance-late");
        while (late.Year < definition.years)
        {
            AnnualTownDemo.Advance(late, "late-" + late.Year, provider); late.ContinuePlanning(late.Revision);
        }
        Advice(late); // Available suggestions must still open by Year 10.

        var latePower = new AnnualTown(scarce, "guidance-late-power");
        while (latePower.Year < definition.years)
        {
            AnnualTownDemo.Advance(latePower, "late-power-" + latePower.Year, provider);
            latePower.ContinuePlanning(latePower.Revision);
        }
        Check(Advice(latePower).actionId == null && Advice(latePower).message.Contains("open by Year 10"), "Late guide confuses affordability with construction time");
        Console.WriteLine("Planning guidance checks passed: " + checks);
        return checks;
    }
}
