using System;
using System.Linq;
using Terra.Contracts;

namespace Terra.Core
{
    // Presentation-only reference comparison. Uses the real lifecycle and score evaluator,
    // with funded operation and no new weather events, without resolving or replaying a year.
    public sealed class AnnualProjectPreview
    {
        public int year, openingYear, matureYear;
        public bool available, beyondRun;
        public double economic, social, environmental;
        public string comparison;

        public static AnnualProjectPreview Create(AnnualTownDefinition rules, AnnualTownSnapshot state, TownActionDefinition candidate)
        {
            var result=new AnnualProjectPreview();
            if(rules==null||state==null||candidate==null||candidate.actionId==AnnualPlayerRules.CancelAction)return result;
            result.openingYear=state.year+candidate.constructionYears;
            result.matureYear=result.openingYear+candidate.establishmentYears-1;
            result.year=Math.Min(rules.years,result.matureYear);
            result.beyondRun=result.matureYear>rules.years;
            if(rules.economy==null||rules.simulation!=null||state.phase!=TownPhase.Planning||state.year>rules.years
                ||state.assets.Any(a=>a.siteId==candidate.siteId&&a.cancelledYear==0))return result;
            var queued=state.queue.FirstOrDefault(q=>q.siteId==candidate.siteId);
            if(queued?.actionId==AnnualPlayerRules.CancelAction)return result;
            var before=state.queue.Where(q=>queued?.actionId!=candidate.actionId||q.siteId!=candidate.siteId).ToArray();
            var after=state.queue.Where(q=>q.siteId!=candidate.siteId).Concat(new[]{new TownPlannedAction{siteId=candidate.siteId,actionId=candidate.actionId}}).ToArray();
            var evaluator=AnnualTown.ProjectEvaluator(rules);
            var a=evaluator.ProjectReference(state,before,result.year);
            var b=evaluator.ProjectReference(state,after,result.year);
            result.economic=b.economicScore-a.economicScore;
            result.social=b.socialScore-a.socialScore;
            result.environmental=b.environmentalScore-a.environmentalScore;
            result.comparison=queued==null?"vs current plan":queued.actionId==candidate.actionId?"without this project":"vs queued project";
            result.available=true;return result;
        }

        public static int CompletedConstruction(TownActionDefinition recipe,TownAsset asset,AnnualTownSnapshot state)
        {
            int last=state.history.LastOrDefault()?.year??0;
            return Math.Max(0,Math.Min(recipe.constructionYears,last-asset.startedYear+1));
        }
    }

    public sealed partial class AnnualTown
    {
        // Private read-only evaluator avoids copying/hashing the entire definition on hover.
        // EvaluateOutcomes only reads these dictionaries; all mutable asset state is copied below.
        AnnualTown(AnnualTownDefinition rules)
        {
            definition=rules;
            sites=rules.sites.ToDictionary(s=>s.id,StringComparer.Ordinal);
            actions=rules.actions.ToDictionary(a=>Key(a.siteId,a.actionId),StringComparer.Ordinal);
        }
        internal static AnnualTown ProjectEvaluator(AnnualTownDefinition rules)=>new AnnualTown(rules);

        // Called only on a detached evaluator. Reuse EvaluateOutcomes so crop displacement,
        // maturity, retirement, capped needs and versioned industrial pressure stay identical.
        internal TownYearOutcomes ProjectReference(AnnualTownSnapshot state,TownPlannedAction[] plan,int year)
        {
            Year=year;
            pending=new TownYearRequest{driver=new TownYearDriver{year=year,productionFactor=1,regionalEvents=Array.Empty<TownRegionalEvent>()}};
            var next=TownCopies.Copy(state.assets).ToList();
            foreach(var q in plan)
            {
                var old=next.FirstOrDefault(a=>a.siteId==q.siteId);
                if(q.actionId==AnnualPlayerRules.CancelAction)
                {
                    if(old==null)continue;
                    var previous=state.history.LastOrDefault()?.outcomes.sites.FirstOrDefault(s=>s.siteId==q.siteId);
                    old.cancelledYear=state.year;
                    old.clearedVegetatedFraction=Math.Min(.25,previous==null?sites[q.siteId].baselineVegetatedFraction:previous.vegetatedAreaM2/sites[q.siteId].areaM2);
                }
                else
                {
                    if(old!=null)next.Remove(old);
                    next.Add(new TownAsset{siteId=q.siteId,actionId=q.actionId,startedYear=state.year,condition=1});
                }
            }
            var funded=next.ToDictionary(a=>a.siteId,a=>true);
            return EvaluateOutcomes(next.ToArray(),funded,new TownYearConsequences{effects=Array.Empty<TownSiteEffect>()},new TownYearLedger());
        }
    }
}
