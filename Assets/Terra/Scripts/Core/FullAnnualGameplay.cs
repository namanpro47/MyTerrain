using System;
using System.Linq;
using Terra.Contracts;

namespace Terra.Core
{
    public static class FullAnnualGameplay
    {
        public static readonly string[] Supported = { "cultivate-standard", "conservation-rotation", "cultivate-margin", "cover-crop", "restore-grass", "water-swale", "public-park", "low-density-housing", "high-density-housing", "agricultural-storage", "data-center", "village-center", "non-renewable-power", "solar", "wind", "stewardship-agreement", "wastewater-upgrade" };
        public static AnnualTownDefinition Create(WorldData world, Scenario scenario, string packId, BuildActionRules rules, LandscapeRunoff runoff)
        {
            var d=AnnualTownDemo.FromWorldPack(world,scenario,packId);
            bool parcels=UniformParcels.Applied(world);
            d.schemaVersion=2;d.id=packId=="meadowbrook-v1"?"meadowbrook-full-annual":packId+"-full-annual";d.version=parcels?"4.0.0":AnnualSpatialOpportunities.Connected(world)?"3.2.0":"3.1.0";
            d.startingCashCents=1000000000;d.appropriationsCents=Enumerable.Repeat(100000000L,10).ToArray();
            d.assumptions="USD 2026 authored educational scenario. All capacities, occupancy, service connections, consent, jobs and costs are fictional. Public contributions are not total construction costs; private output is not cash. One project per named management site; no redevelopment refunds.";
            d.economy=new TownEconomyDefinition {version="services-v2",assumptions=parcels?
                "Scenario units, not measured population or utilities. Existing community: 4 homes, 4 jobs, 4 recreation, 4 local-service units. Seven inherited community plots visualize these capacities; they are not new projects or additional rewards. All eligible sites have fictional willing operators, access and utility connections. Drinking-water supply remains fixed; wastewater plants do not produce drinking water.":
                "Scenario units, not measured population or utilities. Existing community outside project footprints: 4 homes, 4 jobs, 4 recreation, 4 local-service units. All eligible sites have fictional willing operators, access and utility connections. Drinking-water supply remains fixed; wastewater plants do not produce drinking water.",
                householdDemand=16,jobsTarget=36,recreationTarget=16,localServicesTarget=16,
                baselineHomes=4,baselineJobs=4,baselineRecreation=4,baselineLocalServices=4,
                baselineEnergySupply=40,baselineCleanEnergySupply=10,baselineWaterSupply=100,baselineWastewaterCapacity=20,
                baselineEnergyDemand=20,baselineWaterDemand=20,baselineWastewaterLoad=16,emissionsLimit=100,
                coverTargetFraction=.65,runoffReductionTarget=.2,finalEconomicTarget=75,finalSocialTarget=75,finalEnvironmentalTarget=60,referenceStorm=runoff.AnnualBaseline()};
            d.rubric=new TownScoreRubric {version="full-rubric-v2",productionTargetUnits=40,serviceTargetUnits=16,retentionTargetM3=100,
                explanation="Economic: output/jobs/funded obligations 40/30/30. Social: occupied housing/essential services/resilience 40/40/20; five equal service subweights. Environment: cover/reference-storm reduction/low-emissions supply/water pressure 40/25/20/15. All components cap at demand; no placement points. Targets and scenario quantities are pinned in this definition."};
            d.actions=d.actions.Where(a=>Supported.Contains(a.actionId)&&rules.Rejection(world.sites.First(s=>s.id==a.siteId),a.actionId)==null).ToArray();
            foreach(var a in d.actions)
            {
                var site=world.sites.First(s=>s.id==a.siteId);var art=rules.Find(a.actionId);
                bool land=art.landscape;
                double occupied=land?site.analysisAreaM2:art.width*art.depth;
                double share=occupied/site.analysisAreaM2;
                double baseGreen=d.sites.First(s=>s.id==a.siteId).baselineVegetatedFraction;
                double conventional=site.offers.FirstOrDefault(o=>o.treatmentId=="cultivate-standard")?.production??site.baselineProduction;
                var s=new TownProjectServices {familyId=ActionProfiles.ForAction(a.actionId)?.id??a.actionId,occupiedAreaM2=occupied,connected=true,consent=true,
                    assumptions="Fictional consenting operator and authored access/utility connection. Capacities and jobs are game units, not engineering output or employment estimates. Explicit zero channels are intentional."};a.services=s;
                a.sourceId="team-chart-full-v3";a.evidenceLevel="scenario-assumption";a.ownerOperator=land?"Fictional consenting land manager":"Fictional public/private service partner";
                a.contributionType="Public contribution; total project cost unknown";a.totalProjectCostKnown=false;a.totalProjectCostCents=0;
                a.fundedQuantity=land?occupied:1;a.fundedUnit=land?"m2 in a ten-year consenting management agreement":"metric project bundle";
                a.operatingYears=10;a.paymentYears=0;a.annualPublicPaymentCents=0;a.annualPublicReceiptsCents=0;a.constructionYears=land?0:1;a.establishmentYears=1;
                a.servicesFollowProduction=false;a.serviceUnits=0;a.productionUnits=site.baselineProduction*(1-share);a.retentionCapacityM3=0;
                double green=.15;long dollars=0,upkeep=0;
                switch(a.actionId)
                {
                    case "cultivate-standard":dollars=100000;upkeep=2000;a.productionUnits=conventional;green=.08;s.jobs=2;s.energyDemand=2;s.waterDemand=4;s.emissionsPressure=2;break;
                    case "conservation-rotation":dollars=160000;upkeep=3200;a.establishmentYears=3;a.productionUnits=conventional*1.15;green=.3;s.jobs=3;s.energyDemand=1;s.waterDemand=2;s.emissionsPressure=1;break;
                    case "cultivate-margin":dollars=125000;upkeep=2500;a.establishmentYears=2;a.productionUnits=conventional*.75;green=.25;s.jobs=2;s.energyDemand=1;s.waterDemand=3;s.emissionsPressure=1;break;
                    case "cover-crop":dollars=120000;upkeep=3000;a.establishmentYears=2;a.productionUnits=conventional*.9;green=.35;s.jobs=2;s.energyDemand=1;s.waterDemand=2;s.emissionsPressure=1;break;
                    case "restore-grass":dollars=75000;upkeep=2000;a.establishmentYears=3;green=1;break;
                    case "water-swale":dollars=100000;upkeep=3000;a.constructionYears=1;green=.9;a.retentionCapacityM3=site.offers.First(o=>o.treatmentId=="water-swale").retentionCapacityM3;break;
                    case "public-park":dollars=50000;upkeep=2000;green=.85;s.recreation=6;s.jobs=1;s.energyDemand=1;s.waterDemand=1;a.fundedUnit="one 48 × 40 m park; six recreation units";break;
                    case "low-density-housing":dollars=1000000;upkeep=10000;green=.35;s.homes=1;s.energyDemand=2;s.waterDemand=2;s.wastewaterLoad=1.6;a.annualPublicReceiptsCents=2000000;a.fundedUnit="one home, one household; 26 × 32 m lot";break;
                    case "high-density-housing":dollars=5000000;upkeep=60000;green=.2;s.homes=12;s.energyDemand=12;s.waterDemand=18;s.wastewaterLoad=14;a.constructionYears=2;a.annualPublicReceiptsCents=15000000;a.fundedUnit="one four-storey block, twelve households; 44 × 38 m lot";break;
                    case "agricultural-storage":dollars=2000000;upkeep=30000;green=.1;a.productionUnits+=8;s.jobs=12;s.energyDemand=4;s.waterDemand=2;s.wastewaterLoad=1;s.emissionsPressure=4;a.fundedUnit="one 48 × 42 m rural warehouse with storage bins";break;
                    case "data-center":dollars=3000000;upkeep=80000;green=.1;a.productionUnits+=24;s.jobs=6;s.energyDemand=35;s.waterDemand=35;s.wastewaterLoad=8;s.emissionsPressure=6;a.constructionYears=2;a.fundedUnit="one 80 × 64 m hall and cooling plant; six ongoing jobs";break;
                    case "village-center":dollars=4000000;upkeep=50000;green=.25;a.productionUnits+=6;s.jobs=20;s.localServices=12;s.recreation=4;s.energyDemand=8;s.waterDemand=6;s.wastewaterLoad=5;s.emissionsPressure=3;a.constructionYears=2;a.fundedUnit="three shops and square; 64 × 52 m site";break;
                    case "non-renewable-power":dollars=5000000;upkeep=120000;green=.1;s.jobs=8;s.energySupply=90;s.waterDemand=10;s.wastewaterLoad=4;s.emissionsPressure=65;a.constructionYears=2;a.fundedUnit="one 72 × 60 m gas-engine facility; 90 energy units";break;
                    case "solar":dollars=3000000;upkeep=25000;green=.6;s.jobs=2;s.energySupply=s.cleanEnergySupply=40;s.waterDemand=1;s.emissionsPressure=2;a.fundedUnit="sixty fixed-tilt tables; 64 × 48 m site; 40 energy units";break;
                    case "wind":dollars=3000000;upkeep=40000;green=.9;s.jobs=2;s.energySupply=s.cleanEnergySupply=55;s.emissionsPressure=3;a.constructionYears=2;a.fundedUnit="one 165 m tip-height turbine; 150 × 150 m clearance; 55 energy units";break;
                    case "stewardship-agreement":dollars=150000;upkeep=5000;green=.95;a.establishmentYears=4;s.jobs=2;s.localServices=2;a.ownerOperator="Fictional consenting stewardship council; agreed restoration and governance work";a.fundedUnit="m2 of specified restoration work under a fictional consenting ten-year agreement; not a purchase of sovereignty";break;
                    case "wastewater-upgrade":dollars=2500000;upkeep=40000;green=.25;s.jobs=4;s.energyDemand=6;s.wastewaterCapacity=50;s.emissionsPressure=2;a.constructionYears=2;a.fundedUnit="one 64 × 52 m wastewater upgrade; 50 treatment units, no drinking-water supply";break;
                    default:throw new ArgumentException("Missing authored recipe: "+a.actionId);
                }
                a.upfrontPublicCostCents=dollars*100;a.annualPublicOperatingCents=upkeep*100;
                a.vegetatedFraction=baseGreen*(1-share)+green*share;
                string other=land&&new[]{"cultivate-standard","conservation-rotation","cultivate-margin","cover-crop"}.Contains(a.actionId)?"cultivated":"sealed";
                double target=runoff.CurveNumber(new[]{new CoverFraction{coverId="grass",fraction=(float)green},new CoverFraction{coverId=other,fraction=(float)(1-green)}});
                s.targetCurveNumber=runoff.CurveNumber(site.baselineCover)*(1-share)+target*share;
            }
            d.drivers=Enumerable.Range(1,10).Select(y=>new TownYearDriver {year=y,id="shared-conditions-v3-"+y,evidenceLevel="scenario-assumption",productionFactor=y==3?.8:y==7?.65:1,
                unexpectedPublicRepairCents=y==7?15000000:0,description=y==3?"Authored wet year: farm output and renewable generation ×0.8":y==7?"Authored disruption: farm output and renewable generation ×0.65; $150K public repair due":"Authored reference conditions"}).ToArray();
            foreach(var id in Supported)AnnualTown.Require(d.actions.Any(a=>a.actionId==id),"Required family has no fitting eligible site: "+id);
            AnnualTown.ValidateDefinition(d);return d;
        }
    }
}
