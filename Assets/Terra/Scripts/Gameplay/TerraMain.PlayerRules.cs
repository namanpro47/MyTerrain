using System;
using System.Linq;
using Terra.Core;
using Terra.Contracts;
using UnityEngine;

namespace Terra.Gameplay
{
    public sealed partial class TerraMain
    {
        string annualRegionId;
        void ReviewProjectCancellation(string siteId)
        {
            if(annual.State.phase!=TownPhase.Planning)return;
            int revision=annual.State.revision;
            long refund=annual.Engine.CancellationRefund(siteId);
            var asset=annual.State.assets.Single(a=>a.siteId==siteId&&a.cancelledYear==0);
            var recipe=annualDefinition.actions.Single(a=>a.siteId==siteId&&a.actionId==asset.actionId);
            double progress=recipe.constructionYears==0?1:Math.Min(1,(double)(annual.State.year-asset.startedYear)/recipe.constructionYears);
            hud.AnnualDialog("Cancel "+recipe.title+"?",
                "Construction "+(progress*100).ToString("0")+"% complete.\nRefund at End Year: "+TerraHudMoney(refund)+" of unfinished public capital.\n\nConsumed upkeep is not refunded. Future upkeep stops and the site becomes available next planning year. Cleared land remains disturbed.\n\nQueueing can be undone until End Year.","Queue cancellation",()=>{
                    hud.CloseModal();
                    try
                    {
                        annual.QueueAction(siteId,AnnualPlayerRules.CancelAction,revision);
                        selectedId=siteId;treatmentId="";editMode="place";hud.Selected("");
                        SaveAnnual();RefreshLandscape();Refresh("Cancellation queued");
                    }
                    catch(Exception e){Toast(e.Message);}
                });
        }
        void RenewPlayerScenario()
        {
            string seed=Guid.NewGuid().ToString("N");
            annualDefinition=AnnualPlayerRules.Create(FullAnnualGameplay.Create(world,scenario,annualRegionId,buildActionRules,runoff),annualRegionId,seed);
            PlayerPrefs.SetString(annual.saveKey+".seed",seed);
            annualBaseline=AnnualGameplay.Baseline(annualDefinition);
            hud.SetAnnualRules(annualDefinition);
        }
    }
}
