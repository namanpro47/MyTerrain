using System;
using System.Collections.Generic;
using System.Linq;
using Terra.Contracts;
namespace Terra.Core
{
    // Immutable baseline DTOs are owned by the loader. This store never writes to them.
    public sealed class WorldStateStore
    {
        readonly WorldData world; readonly Scenario scenario;
        Intervention[] current=Array.Empty<Intervention>(); readonly Stack<Intervention[]> history=new Stack<Intervention[]>();
        public int Revision { get; private set; }
        public bool CanUndo => history.Count>0;
        public WorldResult Result => Calculate(current);
        public string TreatmentAt(string siteId) => current.FirstOrDefault(x=>x.siteId==siteId)?.treatmentId;
        public WorldStateStore(WorldData world, Scenario scenario) { this.world=world; this.scenario=scenario; }
        public Evaluation Preview(EditCommand command)
        {
            if(command.expectedRevision!=Revision) return Invalid("The landscape changed. Preview again.");
            if(command.operation!="add" && command.operation!="replace") return Invalid("This operation arrives after the foundation phase.");
            var site=world.sites.FirstOrDefault(x=>x.id==command.siteId);
            if(site==null) return Invalid("Choose a management site.");
            if(site.protectedSite) return Invalid(site.protectionReason);
            if(!site.allowedTreatments.Contains(command.treatmentId)) return Invalid("This treatment is not available at this site.");
            var existing=current.FirstOrDefault(x=>x.siteId==site.id);
            if(command.operation=="add" && existing!=null) return Invalid("This site already has a proposal.");
            if(command.operation=="replace" && existing==null) return Invalid("There is no proposal to replace.");
            if(existing?.treatmentId==command.treatmentId) return Invalid("This proposal is already placed.");
            var next=current.Where(x=>x.siteId!=site.id).Concat(new[]{new Intervention{siteId=site.id,treatmentId=command.treatmentId}}).ToArray();
            var result=Calculate(next);
            if(result.remainingCredits<0) return Invalid("Not enough credits. Undo a proposal to try this one.");
            return new Evaluation{valid=true,reason="Ready to place",result=result,interventions=Clone(next)};
        }
        public Evaluation Commit(EditCommand command)
        {
            var evaluation=Preview(command); if(!evaluation.valid) return evaluation;
            history.Push(Clone(current)); current=Clone(evaluation.interventions); Revision++;
            evaluation.result=Calculate(current); return evaluation;
        }
        public bool Undo() { if(!CanUndo)return false; current=history.Pop(); Revision++;return true; }
        static Intervention[] Clone(Intervention[] values)=>values.Select(x=>new Intervention{siteId=x.siteId,treatmentId=x.treatmentId}).ToArray();
        Evaluation Invalid(string reason)=>new Evaluation{valid=false,reason=reason,result=Calculate(current),interventions=Clone(current)};
        WorldResult Calculate(Intervention[] proposals)
        {
            var areas=new Dictionary<string,float>(); var results=new List<SiteResult>(); int spent=0;float total=0,baseline=0;
            foreach(var site in world.sites)
            {
                var edit=proposals.FirstOrDefault(x=>x.siteId==site.id); var cover=site.baselineCover; float production=site.baselineProduction;
                if(edit!=null)
                {
                    var offer=site.offers.First(x=>x.treatmentId==edit.treatmentId); var treatment=scenario.treatments.First(x=>x.id==edit.treatmentId);
                    spent+=offer.cost; production=offer.production;cover=treatment.cover;
                }
                baseline+=site.baselineProduction;total+=production;
                foreach(var fraction in cover){if(!areas.ContainsKey(fraction.coverId))areas[fraction.coverId]=0;areas[fraction.coverId]+=site.analysisAreaM2*fraction.fraction;}
                results.Add(new SiteResult{siteId=site.id,production=production,productionDelta=production-site.baselineProduction});
            }
            return new WorldResult{revision=Revision,spentCredits=spent,remainingCredits=scenario.budget-spent,production=total,baselineProduction=baseline,netProduction=total-baseline,
                runoffAvailable=false,coverAreas=areas.OrderBy(x=>x.Key).Select(x=>new CoverArea{coverId=x.Key,areaM2=x.Value}).ToArray(),sites=results.ToArray(),
                explanations=new[]{new ExplanationFact{code="illustrative-production",value=total,unit="game-units",sourceId="scenario-assumptions-v1"}}};
        }
    }
}
