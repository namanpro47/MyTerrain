using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Terra.Contracts;

namespace Terra.Core
{
    public static class AnnualPlayerRules
    {
        public const string Version = "annual-player-rules-v5.0.0";
        public const string CancelAction = "cancel-project";
        public static bool Enabled(AnnualTownDefinition d) => d?.playerRules?.version == Version;

        public static AnnualTownDefinition Create(AnnualTownDefinition original,string regionId,string seed)
        {
            AnnualTown.Require(original!=null && original.schemaVersion==2 && original.simulation==null,"Player rules require the existing full annual scenario.");
            AnnualTown.Require(new[]{"meadowbrook-v1","point-reyes-v1","suusamyr-v1"}.Contains(regionId)&&!string.IsNullOrWhiteSpace(seed)&&Encoding.UTF8.GetByteCount(seed)<=256,"Choose a supported region and stable scenario seed.");
            var d=TownCopies.Copy(original);d.version="5.0.0";
            var hazards=Profiles(regionId);
            d.playerRules=new TownPlayerRules {version=Version,regionId=regionId,scenarioSeed=seed,hazards=hazards,
                disclosure="Regional hazard chances, exposure and losses are inferred game estimates informed by regional sources, not measured annual site-damage probabilities. Upkeep is charged to the public budget; a negative balance blocks new construction. Environmental impact is a signed authored indicator."};
            d.assumptions+=" "+d.playerRules.disclosure;
            d.rubric.version="player-rubric-v5";
            d.rubric.explanation="Upkeep funds operations without utility rationing. Economic and Social remain 0–100; Environmental is -100–100 after cumulative authored industrial pressure. Public cash may be negative and is separate from the Economic score.";
            var cancelOffers=d.sites.Where(s=>!s.protectedSite&&d.actions.Any(a=>a.siteId==s.id)).Select(site=>{
                var a=TownCopies.Copy(d.actions.First(x=>x.siteId==site.id));
                a.actionId=CancelAction;a.title="Cancel accepted project";a.visualTreatmentId="";a.upfrontPublicCostCents=0;
                a.annualPublicPaymentCents=a.annualPublicOperatingCents=a.annualPublicReceiptsCents=0;a.paymentYears=0;a.operatingYears=1;a.constructionYears=0;a.establishmentYears=1;
                a.productionUnits=a.serviceUnits=a.retentionCapacityM3=0;a.services.familyId=CancelAction;
                foreach(var field in typeof(TownProjectServices).GetFields().Where(f=>f.FieldType==typeof(double)&&f.Name!="occupiedAreaM2"&&f.Name!="targetCurveNumber"))field.SetValue(a.services,0.0);
                a.sourceId=Version;a.evidenceLevel="authored-game-rule";a.contributionType="Return of unfinished public capital; consumed upkeep is not refunded";
                a.totalProjectCostKnown=false;a.totalProjectCostCents=0;return a;
            }).ToArray();
            d.actions=d.actions.Concat(cancelOffers).ToArray();
            d.drivers=Enumerable.Range(1,d.years).Select(year=>{
                var events=hazards.Where(h=>Draw(seed,regionId,year,h.id,"occurrence")<h.annualChance).Select(h=>new TownRegionalEvent {
                    id=regionId+"/"+year+"/"+h.id,title=h.title,evidence=h.evidence,outputFactor=h.outputFactor,conditionLoss=h.conditionLoss,
                    affectedSiteIds=d.sites.Where(s=>!s.protectedSite&&Draw(seed,regionId,year,h.id,"site/"+s.id)<h.affectedSiteChance).Select(s=>s.id).ToArray()
                }).ToArray();
                return new TownYearDriver {year=year,id=Version+"/"+regionId+"/"+year,productionFactor=1,evidenceLevel="inferred-game-estimate",
                    description=events.Length==0?"No regional disruption in this saved scenario year.":string.Join("; ",events.Select(e=>e.title))+". Inferred scenario, not a forecast.",regionalEvents=events};
            }).ToArray();
            AnnualTown.ValidateDefinition(d);return d;
        }

        // Independent keyed draws are stable across queue order, retry and comparison run IDs.
        static double Draw(string seed,string region,int year,string hazard,string channel)
        {
            using(var sha=SHA256.Create())
            {
                var b=sha.ComputeHash(Encoding.UTF8.GetBytes(Version+"\n"+seed+"\n"+region+"\n"+year+"\n"+hazard+"\n"+channel));
                uint n=((uint)b[0]<<24)|((uint)b[1]<<16)|((uint)b[2]<<8)|b[3];return n/4294967296.0;
            }
        }
        static TownHazardDefinition H(string id,string title,double chance,double output,double damage,double exposure,string source)
            =>new TownHazardDefinition {id=id,title=title,annualChance=chance,outputFactor=output,conditionLoss=damage,affectedSiteChance=exposure,sourceUrl=source,
                evidence="Inferred game estimate; regional source supports hazard identity, not these exact probabilities or loss coefficients."};
        public static TownHazardDefinition[] Profiles(string region)
        {
            AnnualTown.Require(new[]{"meadowbrook-v1","point-reyes-v1","suusamyr-v1"}.Contains(region),"Unknown regional hazard profile.");
            if(region=="meadowbrook-v1")return new[]{
                H("flood","Heavy-rain / flood disruption",.20,.75,.10,.35,"https://www.weather.gov/arx/aug2818"),
                H("drought","Dry growing season",.15,.80,.04,.70,"https://cms8.revize.com/revize/vernoncountywi/Vernon%20County%20HMP%20-%20WEM-%20FEMA%20Approved%20v2-%20FINAL.pdf"),
                H("storm","Severe storm / hail",.25,.90,.05,.25,"https://www.weather.gov/arx/vernon")};
            if(region=="point-reyes-v1")return new[]{
                H("wildfire","Wildfire disruption",.08,.60,.18,.20,"https://www.nps.gov/pore/learn/nature/wildlandfire_woodwardfire.htm"),
                H("drought","Dry growing season",.25,.80,.04,.70,"https://www.marincounty.gov/departments/fire/office-emergency-management/disaster-preparedness/local-hazard-maps"),
                H("earthquake","Earthquake damage",.02,.85,.12,.35,"https://www.usgs.gov/publications/earthquake-outlook-san-francisco-bay-region-2014-2043")};
            return new[]{
                H("earthquake","Earthquake damage",.015,.85,.12,.35,"https://thinkhazard.org/en/report/147294-kyrgyz-republic-chuy/EQ"),
                H("slope-snow","Slope / snow disruption",.15,.80,.10,.25,"https://www.mchs.gov.kg/ru/structures_old/kr-okm-chui-oblusu-boyuncha-bashkarmalygy/"),
                H("flood","Snowmelt / heavy-rain disruption",.12,.80,.08,.30,"https://www.mchs.gov.kg/ru/structures_old/kr-okm-chui-oblusu-boyuncha-bashkarmalygy/")};
        }
        public static TownSiteEffect[] Effects(TownYearRequest request)
        {
            return (request.driver.regionalEvents??Array.Empty<TownRegionalEvent>()).SelectMany(e=>e.affectedSiteIds.Select(id=>new {e,id})).GroupBy(x=>x.id).Select(g=>{
                return new TownSiteEffect {siteId=g.Key,sourceId=request.driver.id,productionFactor=g.Aggregate(1.0,(value,x)=>value*x.e.outputFactor),
                    conditionDelta=0,disrupted=true,reason=string.Join("; ",g.Select(x=>x.e.title))+" reduced this year's illustrative output. Regional chance and exposure are inferred game estimates."};
            }).ToArray();
        }
        public static double IndustrialPressure(string actionId) => actionId=="data-center"?18:actionId=="non-renewable-power"?30:actionId=="agricultural-storage"?6:0;
        internal static void ValidateEvents(AnnualTownDefinition d)
        {
            AnnualTown.Require(d.playerRules.hazards.Select(h=>h.id).Distinct().Count()==d.playerRules.hazards.Length,"Duplicate hazard profile.");
            Profiles(d.playerRules.regionId);
            foreach(var driver in d.drivers)
            {
                AnnualTown.Require(driver.regionalEvents!=null,"Regional drivers require an explicit saved event array.");
                var used=new System.Collections.Generic.HashSet<string>(StringComparer.Ordinal);
                foreach(var e in driver.regionalEvents)
                {
                    AnnualTown.Require(e!=null&&used.Add(e.id),"Missing or duplicate regional event.");
                    var h=d.playerRules.hazards.SingleOrDefault(hazard=>e.id==d.playerRules.regionId+"/"+driver.year+"/"+hazard.id);
                    AnnualTown.Require(h!=null&&e.title==h.title&&e.evidence==h.evidence&&e.outputFactor==h.outputFactor&&e.conditionLoss==h.conditionLoss,"Event differs from its regional profile.");
                    AnnualTown.Require(e.affectedSiteIds!=null&&e.affectedSiteIds.Distinct().Count()==e.affectedSiteIds.Length&&e.affectedSiteIds.All(id=>d.sites.Any(s=>s.id==id&&!s.protectedSite)),"Invalid regional exposure sites.");
                }
            }
        }
    }
}
