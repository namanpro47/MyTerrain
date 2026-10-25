using System;
using System.Linq;
using Terra.Contracts;
using Terra.UI;

namespace Terra.Gameplay
{
    public sealed partial class TerraMain
    {
        bool placementIntentPending;
        string placementIntentSite,placementIntentTool,placementIntentMode,placementIntentSource;
        int placementIntentRevision;

        // Capture the command target on pointer-down. Keyboard commands use their current preview.
        void CapturePlacementIntent(string siteId)
        {
            placementIntentPending=ready&&annual!=null;
            if(!placementIntentPending)return;
            placementIntentSite=siteId;placementIntentTool=treatmentId;placementIntentMode=editMode;
            placementIntentSource=moveSourceId;placementIntentRevision=annual.State.revision;
        }
        void ClearPlacementIntent(){placementIntentPending=false;}
        void CancelPlacementPress()
        {
            bool interruptedDrag=dragged;pressed=false;dragged=false;uiDown=null;ClearPlacementIntent();
            if(interruptedDrag&&ready&&!atlasVisible)UpdatePreview();
        }
        bool ConsumePlacementIntent()
        {
            if(!placementIntentPending)return true;placementIntentPending=false;
            return placementIntentRevision==annual.State.revision&&placementIntentSite==PreviewSiteId
                &&placementIntentTool==treatmentId&&placementIntentMode==editMode&&placementIntentSource==moveSourceId;
        }

        void ReviewPlacementReplacement()
        {
            if(!ConsumePlacementIntent()){Toast("The target or plan changed. Preview it again.");return;}
            string id=PreviewSiteId,action=treatmentId;var site=world.sites.FirstOrDefault(s=>s.id==id);
            string reason=Rejection(site);if(reason!=null){Toast(reason);BlockedCue();return;}
            int revision=annual.State.revision;
            string previous=QueuedAt(id);long delta=annual.Engine.AvailableCents-annual.Preview(id,action);
            string funds=delta>=0?"Reserve "+TerraHudMoney(delta)+" more.":"Release "+TerraHudMoney(-delta)+" from the current reservation.";
            hud.AnnualDialog("Replace this queued project?",
                site.name+"\n\n"+buildActionRules.Find(previous).title+" → "+buildActionRules.Find(action).title+"\n\n"+funds+
                "\nCash is unchanged until End Year. Esc keeps the original proposal.","Replace project",()=>
                {
                    hud.CloseModal();
                    if(annual.State.revision!=revision||treatmentId!=action||QueuedAt(id)!=previous)
                    {Toast("The plan changed. Review the replacement again.");return;}
                    hoverId=id;ConfirmEdit();
                });
        }
        void ReviewLandUseChange(Site site)
        {
            string id=site.id,action=treatmentId;
            int revision=annual.State.revision;
            var old=annual.State.assets.Single(a=>a.siteId==id);
            var next=annualDefinition.actions.Single(a=>a.siteId==id&&a.actionId==action);
            long preparation=annual.Engine.ConversionCost(id);
            hud.AnnualDialog("Change this land use?",
                buildActionRules.Find(old.actionId).title+" → "+next.title+
                "\n\nNew project  "+TerraHudMoney(next.upfrontPublicCostCents)+
                "\nRemoval, site preparation and remaining program payments  "+TerraHudMoney(preparation)+
                "\n\nThe previous use stops when you End Year. The replacement needs "+Math.Max(1,next.constructionYears)+
                " year(s) before it opens, then "+next.establishmentYears+" year(s) to establish. Its benefits and ongoing costs replace the old use. Past spending is not refunded."+
                "\n\nThis queues a proposal. You can still undo it before End Year.","Queue land-use change",()=>{
                    hud.CloseModal();
                    if(annual.State.revision!=revision||treatmentId!=action){Toast("The plan changed. Review this conversion again.");return;}
                    hoverId=id;ApplyEdit(true);
                });
        }
        static string TerraHudMoney(long cents)=>TerraHud.Money(cents);
        void PlacementArrivalView()
        {
            if(!location.IsWisconsin)return;
            // Put West and Central fields into the decision area; retain the creek/bridge context.
            // This changes the opening composition only. Existing overview, close-up and pan remain.
            target=new UnityEngine.Vector3(10,30,-145);zoom=165;pitch=42;focusHeightOffset=0;
        }

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
            string status=reason==null?(editMode=="move"?"MOVE PREVIEW":accepted!=null?"CHANGE PREVIEW":queued!=null?"REPLACE PREVIEW":"PREVIEW"):"BLOCKED";
            string instruction;
            long? delta=null,after=null;
            if(reason==null)
            {
                after=editMode=="move"?annual.Engine.PreviewMove(moveSourceId,site.id):annual.Preview(site.id,treatmentId);
                delta=annual.Engine.AvailableCents-after.Value;
                string funds=delta>0?"Reserve "+TerraHudMoney(delta.Value):delta<0?"Release "+TerraHudMoney(-delta.Value):"Reservation unchanged";
                instruction=funds+" · "+(editMode=="move"?"Click to move":accepted!=null?"Click to review land-use change":queued!=null?"Click to review replacement":"Click to queue");
            }
            else instruction=PlacementReason(reason,site,accepted!=null,queued==treatmentId);
            string space=rule==null?"":rule.landscape?"Treats this whole field":
                rule.width.ToString("0")+" × "+rule.depth.ToString("0")+" m footprint · reserves this field's project slot";
            hud.PlacementFeedback(status,title,site.name,instruction,space,reason,delta,after,
                landscape.PlacementWorldPoints(site,treatmentId),view);
        }

        string PlacementReason(string reason,Site site,bool accepted,bool sameQueued)
        {
            if(accepted&&reason.Contains("already has that use"))return "Already this use · choose a different land use";
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
