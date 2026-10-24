using System;
using System.Linq;
using Terra.Contracts;

namespace Terra.Core
{
    public interface IAnnualConsequenceProvider
    {
        TownYearConsequences Resolve(TownYearRequest request);
    }

    /// <summary>Explicitly authored offline scenario; not a crop, soil or flood prediction.</summary>
    public sealed class AuthoredAnnualConsequenceProvider : IAnnualConsequenceProvider
    {
        public TownYearConsequences Resolve(TownYearRequest request)
        {
            return new TownYearConsequences {
                runId = request.runId, requestId = request.requestId, inputHash = request.inputHash, year = request.year,
                effects = Array.Empty<TownSiteEffect>()
            };
        }
    }

    public static class AnnualTownDemo
    {
        public static AnnualTownDefinition FromWorldPack(WorldData world, Scenario scenario, string worldPackId)
        {
            // Preserve the v1 map/site offers. Costs are a disclosed demo conversion,
            // NOT county construction estimates or the PRD's assistance benchmarks.
            var actions = world.sites.Where(s => !s.protectedSite).SelectMany(site => site.offers.Select(offer => {
                var treatment = scenario.treatments.First(t => t.id == offer.treatmentId);
                return new TownActionDefinition {
                    siteId = site.id, actionId = offer.treatmentId, title = treatment.title, visualTreatmentId = offer.treatmentId,
                    sourceId = "annual-authored-demo-v1", evidenceLevel = "scenario-assumption",
                    contributionType = "Authored public demonstration contribution; total project cost unknown",
                    ownerOperator = "Fictional participating land manager; public swale operator",
                    fundedQuantity = site.analysisAreaM2, fundedUnit = "m2 of hypothetical management site",
                    upfrontPublicCostCents = checked((long)offer.cost * 100000),
                    annualPublicOperatingCents = checked((long)offer.cost * 5000), operatingYears = 10,
                    constructionYears = 1, establishmentYears = offer.treatmentId == "restore-grass" ? 3 : 1,
                    productionUnits = offer.production, serviceUnits = offer.production,
                    vegetatedFraction = treatment.cover.Where(c => c.coverId == "grass" || c.coverId == "woodland").Sum(c => (double)c.fraction),
                    retentionCapacityM3 = offer.retentionCapacityM3
                };
            })).ToArray();
            return new AnnualTownDefinition {
                id = "meadowbrook-annual-demo", version = "1.0.0", worldPackId = worldPackId, currency = "USD", priceYear = 2026, years = 10,
                assumptions = "OFFLINE AUTHORED DEMO. Public prices = v1 offer credits × $1,000; 5% annual operation for ten operating years; $20,000 annual appropriation. All are game assumptions, not measured local public costs. Output/services are illustrative. No LLM, flood routing, observed yields or tax receipts.",
                startingCashCents = checked((long)scenario.budget * 100000),
                appropriationsCents = Enumerable.Repeat(2000000L, 10).ToArray(),
                sites = world.sites.Select(s => new TownSiteDefinition {
                    id = s.id, name = s.name, protectedSite = s.protectedSite, areaM2 = s.analysisAreaM2,
                    baselineProductionUnits = s.baselineProduction, baselineServiceUnits = s.baselineProduction,
                    baselineVegetatedFraction = s.baselineCover.Where(c => c.coverId == "grass" || c.coverId == "woodland").Sum(c => (double)c.fraction)
                }).ToArray(), actions = actions,
                drivers = Enumerable.Range(1, 10).Select(y => new TownYearDriver {
                    year = y, id = "authored-weather-" + y, description = y == 3 ? "Authored wet-year output sensitivity (not flood prediction)" : "Authored reference year",
                    evidenceLevel = "scenario-assumption", productionFactor = y == 3 ? .8 : 1
                }).ToArray(),
                rubric = new TownScoreRubric {
                    version = "illustrative-rubric-v1", productionTargetUnits = 20, serviceTargetUnits = 20, retentionTargetM3 = 100,
                    explanation = "Explicit illustrative targets and 80/20 weights. Service units use production as a provisional community-need proxy. Management-site cover only; retention is configured capacity, not modeled flood protection."
                }
            };
        }

        // Resolve each year through the same transaction; callers can inspect/save after each step.
        public static TownYearReport Advance(AnnualTown town, string requestId, IAnnualConsequenceProvider provider)
        {
            var committed = town.History.FirstOrDefault(r => r.acceptedRequest.requestId == requestId);
            if (committed != null) return committed;
            var request = town.Phase == TownPhase.Resolving ? town.PendingRequest : town.BeginResolution(requestId, town.Revision);
            if (request.requestId != requestId) throw new ArgumentException("Retry must use the pending request ID.");
            var result = provider.Resolve(request); // failure intentionally leaves the frozen request intact.
            return town.CompleteResolution(result);
        }

        public static TownYearReport[] Finish(AnnualTown town, IAnnualConsequenceProvider provider)
        {
            var reports = new System.Collections.Generic.List<TownYearReport>();
            while (town.Phase != TownPhase.Completed)
            {
                if (town.Phase == TownPhase.Reviewing) town.ContinuePlanning(town.Revision);
                string id = town.PendingRequest?.requestId ?? town.RunId + "/finish-year-" + town.Year;
                reports.Add(Advance(town, id, provider));
            }
            return reports.ToArray();
        }
    }
}
