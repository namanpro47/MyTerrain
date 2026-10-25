using System;
using System.Linq;
using Terra.Contracts;

namespace Terra.Core
{
    // Authored starter recipes. Prices preserve the team table; these are not local estimates.
    public static class AnnualGameplay
    {
        public static readonly string[] Supported = { "cultivate-standard", "conservation-rotation", "cultivate-margin", "restore-grass", "water-swale", "public-park", "low-density-housing" };
        public static AnnualTownDefinition Create(WorldData world, Scenario scenario, string packId, Func<Site,string,string> rejection)
        {
            var definition = AnnualTownDemo.FromWorldPack(world, scenario, packId);
            definition.id = "meadowbrook-annual-playable"; definition.version = "2.0.0";
            definition.startingCashCents = 1000000000L;
            definition.appropriationsCents = Enumerable.Repeat(100000000L, 10).ToArray();
            definition.assumptions = "Authored USD 2026 game balance: $10M opening cash; $1M closing appropriation. Table prices buy one named bundle; all upkeep, service units and weather are scenario assumptions. Private output is not public income. Homes and parks have independent service capacity. Ratings are intent, not score increments.";
            foreach (var site in definition.sites) site.baselineServiceUnits = site.baselineProductionUnits * .25;
            definition.actions = definition.actions.Where(a => Supported.Contains(a.actionId) && rejection(world.sites.First(s => s.id == a.siteId), a.actionId) == null).ToArray();
            foreach (var a in definition.actions)
            {
                var site = world.sites.First(s => s.id == a.siteId);
                a.sourceId = "team-actions-oct24-starter-v2"; a.operatingYears = 10; a.constructionYears = 0;
                a.annualPublicReceiptsCents = 0; a.annualPublicPaymentCents = 0; a.paymentYears = 0;
                a.contributionType = "Authored public project contribution";
                a.ownerOperator = "Fictional consenting land manager / public service operator";
                a.fundedUnit = "m2 of this named field managed under a ten-year agreement";
                a.serviceUnits = a.productionUnits * .25;
                long dollars = 0, upkeep = 0;
                switch (a.actionId)
                {
                    case "cultivate-standard": dollars=100000; upkeep=2000; a.establishmentYears=1; break;
                    case "conservation-rotation": dollars=160000; upkeep=3200; a.establishmentYears=3; a.productionUnits=(site.offers.FirstOrDefault(o=>o.treatmentId=="cultivate-standard")?.production??site.baselineProduction)*1.2; a.serviceUnits=a.productionUnits*.3; a.vegetatedFraction=.3; break;
                    case "cultivate-margin": dollars=125000; upkeep=2500; a.establishmentYears=2; break;
                    case "restore-grass": dollars=75000; upkeep=2000; a.establishmentYears=3; a.productionUnits=0; a.serviceUnits=0; break;
                    case "water-swale": dollars=100000; upkeep=3000; a.constructionYears=1; a.establishmentYears=1; a.serviceUnits=0; break;
                    case "public-park":
                        dollars=50000; upkeep=2000; a.constructionYears=1; a.establishmentYears=1;
                        a.fundedQuantity=1; a.fundedUnit="48 × 40 m small public park, with 5 illustrative recreation-service units";
                        a.productionUnits=0; a.serviceUnits=5; a.servicesFollowProduction=false;
                        a.vegetatedFraction=CoverAfterFootprint(site,1920,.85); break;
                    case "low-density-housing":
                        dollars=1000000; upkeep=10000; a.constructionYears=1; a.establishmentYears=1;
                        a.fundedQuantity=1; a.fundedUnit="one serviced home on a 26 × 32 m lot; 4 illustrative occupied places";
                        a.productionUnits=0; a.serviceUnits=4; a.servicesFollowProduction=false;
                        a.annualPublicReceiptsCents=2000000; a.vegetatedFraction=CoverAfterFootprint(site,832,.35); break;
                }
                var profile=ActionProfiles.ForAction(a.actionId);
                a.upfrontPublicCostCents=profile?.upfrontCents??dollars*100; a.annualPublicOperatingCents=upkeep*100;
            }
            definition.drivers = Enumerable.Range(1,10).Select(y => new TownYearDriver {
                year=y,id="starter-weather-"+y,evidenceLevel="scenario-assumption",productionFactor=y==3?.8:1,
                description=y==3?"Wet-year sensitivity: farm output and food services ×0.8; housing and recreation unchanged":"Reference growing year"
            }).ToArray();
            definition.rubric = new TownScoreRubric {
                version="starter-rubric-v2",productionTargetUnits=40,serviceTargetUnits=18,retentionTargetM3=100,
                explanation="Illustrative 0–100 indicators: Economic 80% productive capacity / 40 + 20% funded upkeep; Social 80% food, occupied-home and recreation services / 18 + 20% undisrupted sites; Environment 80% vegetated share of managed sites + 20% operational storage / 100 m³. Bounded targets prevent unlimited score stacking. This starter rubric omits jobs, energy and pollution; table profiles are not score increments."
            };
            AnnualTown.ValidateDefinition(definition); return definition;
        }
        static double CoverAfterFootprint(Site site, double footprint, double vegetated)
        {
            double baseline=site.baselineCover.Where(c=>c.coverId=="grass"||c.coverId=="woodland").Sum(c=>(double)c.fraction);
            double share=Math.Min(1,footprint/site.analysisAreaM2);return baseline*(1-share)+vegetated*share;
        }
        // Resolve a detached copy for previews; the live run, history and treasury are untouched.
        public static TownYearReport Forecast(AnnualTown town)
        {
            if(town.Definition.simulation!=null&&town.Phase!=TownPhase.Reviewing&&town.Phase!=TownPhase.Completed)
                throw new InvalidOperationException("Numeric-v4 planning requires a conditional range; an unplayed harvest is not a forecast.");
            var copy=AnnualTown.Restore(town.Definition,town.Snapshot());
            if(copy.Phase==TownPhase.Reviewing||copy.Phase==TownPhase.Completed)return copy.History.Last();
            copy.NeutralPlanningDriver();
            return AnnualTownDemo.Advance(copy,copy.PendingRequest?.requestId??copy.RunId+"/preview/"+copy.Revision,new AuthoredAnnualConsequenceProvider());
        }
        public static TownYearOutcomes Baseline(AnnualTownDefinition definition)
            => definition.simulation!=null?AnnualSimulation.OpeningOutcomes(definition):Forecast(new AnnualTown(definition,"baseline-preview")).outcomes;
    }
}
