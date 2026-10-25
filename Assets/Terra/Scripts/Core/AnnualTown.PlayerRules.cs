using System;
using System.Linq;
using Terra.Contracts;

namespace Terra.Core
{
    public sealed partial class AnnualTown
    {
        public bool PlayerRulesActive => AnnualPlayerRules.Enabled(definition);
        public bool BuildingBlockedByDebt => PlayerRulesActive && CashCents < 0;
        public long DebtCents => PlayerRulesActive ? Math.Max(0,-CashCents) : ArrearsCents;
        public long CancellationRefund(string siteId)
        {
            Require(PlayerRulesActive,"This saved rule version does not support cancellation.");
            var asset=assets.SingleOrDefault(a=>a.siteId==siteId&&a.cancelledYear==0);
            Require(asset!=null,"Choose an accepted project that has not already been cancelled.");
            var recipe=Action(siteId,asset.actionId);
            if(recipe.constructionYears<=0)return 0;
            int remaining=Math.Max(0,recipe.constructionYears-Math.Max(0,Year-asset.startedYear));
            return (long)((decimal)recipe.upfrontPublicCostCents*remaining/recipe.constructionYears);
        }
        public void QueueCancellation(string siteId,int revision) => QueueAction(siteId,AnnualPlayerRules.CancelAction,revision);
        double RegionalDamage(TownAsset asset)
        {
            if(!PlayerRulesActive||!IsOperating(asset,Year))return 0;
            var recipe=Action(asset.siteId,asset.actionId);
            // Vegetation and connected retention soften flood damage, never earthquake occurrence.
            return Math.Min(.25,(pending.driver.regionalEvents??Array.Empty<TownRegionalEvent>()).Where(e=>e.affectedSiteIds.Contains(asset.siteId)).Sum(e=>
                e.conditionLoss*(e.id.EndsWith("/flood",StringComparison.Ordinal)?1-Math.Min(.5,recipe.vegetatedFraction*.3+recipe.retentionCapacityM3/(sites[asset.siteId].areaM2*.1)*.2):1)));
        }
        long RegionalRepairs(TownAsset[] nextAssets) => Sum(nextAssets.Select(a=>(long)((decimal)Action(a.siteId,a.actionId).upfrontPublicCostCents*(decimal)RegionalDamage(a))));
        internal void NeutralPlanningDriver()
        {
            if(!PlayerRulesActive||Phase!=TownPhase.Planning)return;
            // Only called on the detached, never-persisted presentation preview.
            definition.drivers[Year-1].regionalEvents=Array.Empty<TownRegionalEvent>();
            definition.drivers[Year-1].productionFactor=1;
            definition.drivers[Year-1].description="Conditional reference estimate; regional disruptions are resolved at End Year.";
        }
    }
}
