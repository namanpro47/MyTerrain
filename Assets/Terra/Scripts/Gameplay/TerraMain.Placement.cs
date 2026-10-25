using System;
using System.Linq;
using Terra.Contracts;
using Terra.UI;

namespace Terra.Gameplay
{
    public sealed partial class TerraMain
    {
        static string TerraHudMoney(long cents)=>TerraHud.Money(cents);

        void PresentPlacement(Site site,TownActionDefinition recipe,AnnualTownSnapshot state,string reason)
        {
            if(site==null||state.phase!=TownPhase.Planning||editMode=="reset"||editMode=="remove")
            {hud.HidePlacementFeedback();return;}
            string queued=QueuedAt(site.id);
            bool active=!string.IsNullOrEmpty(treatmentId);
            var accepted=state.assets.FirstOrDefault(a=>a.siteId==site.id);
            if(!active)
            {
                hud.HidePlacementFeedback();
                hud.PlacementInspection(site,queued,accepted?.actionId);
                return;
            }
            var rule=buildActionRules.Find(treatmentId);
            string title=rule?.title??recipe?.title??treatmentId;
            string status=reason==null?(editMode=="move"?"MOVE PREVIEW":queued!=null?"REPLACE PREVIEW":"PREVIEW"):"BLOCKED";
            string instruction;
            long? delta=null,after=null;
            if(reason==null)
            {
                after=editMode=="move"?annual.Engine.PreviewMove(moveSourceId,site.id):annual.Preview(site.id,treatmentId);
                delta=annual.Engine.AvailableCents-after.Value;
                string funds=delta>0?"Reserve "+TerraHudMoney(delta.Value):delta<0?"Release "+TerraHudMoney(-delta.Value):"Reservation unchanged";
                instruction=funds+" · "+(editMode=="move"?"Click to move":queued!=null?"Choose Replace to confirm":"Click to queue");
            }
            else instruction=PlacementReason(reason,site,accepted!=null,queued==treatmentId);
            string space=rule==null?"":rule.landscape?"Treats this whole field":
                rule.width.ToString("0")+" × "+rule.depth.ToString("0")+" m footprint · reserves this field's project slot";
            hud.PlacementFeedback(status,title,site.name,instruction,space,reason,delta,after,
                landscape.PlacementWorldPoints(site,treatmentId),view);
        }

        string PlacementReason(string reason,Site site,bool accepted,bool sameQueued)
        {
            if(accepted)return "Accepted project · choose another field";
            if(sameQueued)return "Already queued · inspect, move or remove";
            if(site.protectedSite)return "Protected area · choose a marked field";
            if(treatmentId=="water-swale"&&!site.allowedTreatments.Contains("water-swale"))
                return "No modeled swale connection · choose a marked field";
            if(reason.StartsWith("Needs a ",StringComparison.Ordinal))return "Footprint does not fit · choose a larger field";
            if(reason.StartsWith("The best fitting",StringComparison.Ordinal))return "Ground too uneven · choose a flatter field";
            if(reason.StartsWith("Need $",StringComparison.Ordinal))return reason.Split('.')[0];
            return reason;
        }
    }
}
