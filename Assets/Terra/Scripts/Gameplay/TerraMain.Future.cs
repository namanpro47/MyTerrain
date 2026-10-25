using System;
using System.Collections;
using System.Linq;
using Terra.Contracts;
using Terra.Core;
using UnityEngine;

namespace Terra.Gameplay
{
    public sealed partial class TerraMain
    {
        bool fastForwarding;
        AnnualTown comparisonTown;
        bool introPending;
        bool comparisonMap;
        void ViewComparison(AnnualTownSnapshot snapshot,string title)
        {
            comparisonMap=true;DisplayAnnual(snapshot);hud.MapView(title);UpdatePreview();
        }
        void BeginAnnualArrival()
        {
            if(annual.State.phase==TownPhase.Resolving){ShowPendingChapter();return;}
            if(annual.State.phase==TownPhase.Completed){ShowFuture();return;}
            if(annual.State.phase==TownPhase.Reviewing){ShowAcceptedReport();return;}
            if(!introPending)return;introPending=false;
            hud.AnnualDialog("Ten years. One valley.",(PlayerPrefs.HasKey("terra.meadowbrook.annual-full.v3.1")?"Connected fields use a new run. Your previous-layout save is retained locally.\n\n":"")+"You have $10M in public funds and ten yearly decisions. This fictional community begins with 4 occupied homes against a need for 16, and 4 jobs against a target of 36. Food output, services and land/water condition also matter.\n\nInspect a field, compare a farming option with a community project, then queue a plan. Nothing is paid until End Year. Construction takes time; upkeep reserves money beyond this year.\n\nYou can change this year's plan, but accepted years stay in history. Try to meet needs without leaving unfunded services or exhausting land and water. Year 10 scenario targets: Economic 75, Social 75 and Environmental 60. Mixed outcomes still receive an ending.","Explore the valley",()=>{hud.CloseModal();SaveAnnual();});
        }
        void ReviewFuture()
        {
            if(fastForwarding)return;
            if(annual.State.phase==TownPhase.Completed){ShowFuture();return;}
            var forecast=AnnualGameplay.Forecast(annual.Engine);
            hud.AnnualDialog("See my ten-year future?","Submit the current reviewed plan, then advance the remaining years without new projects. Existing projects still age, need upkeep and encounter the same saved conditions.\n\n"+
                "Current plan: "+annual.State.queue.Length+" projects; expected next closing cash "+Terra.UI.TerraHud.Money(forecast.ledger.closingCashCents)+".\n\nFast-forward uses labelled offline chapters. This cannot undo accepted years. Progress is saved after each year; a failed step can be retried.","Confirm · advance to Year 10",()=>{hud.CloseModal();chapters.StopRequest();StartCoroutine(FinishYears());});
        }
        IEnumerator FinishYears()
        {
            fastForwarding=true;UpdatePreview();var timing=System.Diagnostics.Stopwatch.StartNew();int firstYear=annual.State.year;
            while(annual.State.phase!=TownPhase.Completed)
            {
                int year=annual.State.year;
                hud.AnnualDialog("Advancing your valley","Resolving saved conditions and commitments for year "+year+"…","Please wait",null);
                yield return null;
                string error=null;
                try
                {
                    if(annual.State.phase==TownPhase.Reviewing)chapters.Continue();
                    chapters.ResolveOffline();SaveAnnual();
                }
                catch(Exception e){error=e.Message;}
                if(error!=null)
                {
                    fastForwarding=false;
                    hud.AnnualDialog("Year not completed",error+"\n\nThe frozen request and accepted history are retained. Retry continues the same conditions.","Retry future",()=>{hud.CloseModal();StartCoroutine(FinishYears());});
                    yield break;
                }
            }
            fastForwarding=false;hud.CloseModal();RefreshLandscape();Refresh();ShowFuture();Debug.Log("TERRA_ANNUAL_TIMING: fast-forward from year "+firstYear+" completed in "+timing.Elapsed.TotalMilliseconds.ToString("0.0")+" ms including yielding and final world refresh");
        }
        void ShowFuture()
        {
            if(annual.State.phase!=TownPhase.Completed)return;
            comparisonMap=false;hud.MapView(null);RefreshLandscape();Refresh();
            if(comparisonTown==null)
            {
                comparisonTown=new AnnualTown(annualDefinition,"same-conditions-no-new-action");
                AnnualTownDemo.Finish(comparisonTown,new AuthoredAnnualConsequenceProvider());
            }
            hud.AnnualFuture(annual.Engine,comparisonTown,ShowComparisonChoices,ShowTimeline,ShowTransferQuestion);BrowserPerformance.Memory("future");
        }
        void ShowComparisonChoices()
        {
            hud.AnnualDialog("The same valley, three views","The camera stays where you left it. These views do not change your saved run. The budget and indicators continue to show your accepted result; project tooltips are hidden in reference views.\n\nOpening: original land.\nYour Year 10: accepted projects and conditions.\nNo new action: existing uses under the identical ten-year conditions.\n\nExisting infrastructure continues in the comparison; this is not a frozen no-action simulation.","View my Year 10",()=>{hud.CloseModal();RefreshLandscape();Toast("Your Year 10 · read-only · Future returns to the comparison");});
            hud.AnnualSecondary("Opening / no action",()=>{
                hud.AnnualDialog("Choose comparison land","The no-new-action simulation updates finances and services for ten years. With no new projects, its land cover remains the opening cover under this model. Neither view changes your saved decisions.","View no-new-action Year 10",()=>{hud.CloseModal();ViewComparison(comparisonTown.Snapshot(),"Map: no new action");Toast("No-new-action Year 10 · same conditions · saved run unchanged");});
                hud.AnnualSecondary("View opening",()=>{hud.CloseModal();ViewComparison(new AnnualTown(annualDefinition,"opening-view").Snapshot(),"Map: opening land");Toast("Opening land · saved run unchanged");});
            });
        }
        void ShowTimeline()=>ShowTimelinePage(0);
        void ShowTimelinePage(int page)
        {
            var history=annual.State.history;var rows=history.Skip(page*5).Take(5);
            string text=string.Join("\n\n",rows.Select(r=>"YEAR "+r.year+"  ·  E "+r.outcomes.economicScore.ToString("0.0")+" / S "+r.outcomes.socialScore.ToString("0.0")+" / Env "+r.outcomes.environmentalScore.ToString("0.0")+"\nCash "+Terra.UI.TerraHud.Money(r.ledger.closingCashCents)+" · "+r.acceptedRequest.actions.Length+" new projects · "+r.acceptedRequest.driver.description));
            hud.AnnualDialog("Your ten-year timeline · "+(page+1)+" / 2","<size=86%>"+text+"</size>",page==0?"Years 6–10":"The projects you shaped",page==0?(Action)(()=>ShowTimelinePage(1)):(()=>ShowProjectStory(0)));
            hud.AnnualSecondary("Learning takeaway",ShowTransferQuestion);
        }
        void ShowProjectStory(int index)
        {
            var state=annual.State;var assets=state.assets.OrderBy(a=>a.startedYear).ThenBy(a=>a.siteId).ToArray();
            if(assets.Length==0)
            {
                hud.AnnualDialog("A decade of existing uses","You accepted ten years without adding projects. Existing services and land still faced the saved conditions and their public costs. This is the same no-new-action trajectory used in the comparison.","Return to future",ShowFuture);
                return;
            }
            var asset=assets[index];var site=world.sites.First(s=>s.id==asset.siteId);
            var action=annualDefinition.actions.First(a=>a.siteId==asset.siteId&&a.actionId==asset.actionId);
            var final=state.history.Last().outcomes.sites.First(s=>s.siteId==asset.siteId);
            var firstOperating=state.history.FirstOrDefault(r=>r.outcomes.sites.Any(s=>s.siteId==asset.siteId&&s.actionId==asset.actionId&&s.phase==TownAssetPhase.Operating));
            string text="You committed "+action.title+" at "+site.name+" in Year "+asset.startedYear+".\n\n"+
                (firstOperating==null?"It had not reached the operating stage by Year 10.":"Its first operating year was Year "+firstOperating.year+".")+
                "\n\nFinal state: "+final.phase+" · establishment "+(final.maturity*100).ToString("0")+"%.\n"+final.explanation+
                "\n\nFunded bundle: "+action.fundedQuantity.ToString("0.##")+" × "+action.fundedUnit+".\nTown-wide service delivery depends on resources and demand; capacity is not occupancy or need met.\nConfigured runoff retention: "+final.retentionCapacityM3.ToString("0")+" m³; not a flood-risk estimate.\n\nInspect its real-sized footprint in the final valley. These are saved game outcomes, not a local forecast.";
            hud.AnnualDialog("Your projects · "+(index+1)+" / "+assets.Length,"<size=88%>"+text+"</size>","Inspect this site",()=>{
                comparisonMap=false;hud.MapView(null);RefreshLandscape();hud.CloseModal();selectedId=hoverId=asset.siteId;Refresh();FocusSite();
            });
            hud.AnnualSecondary(index+1<assets.Length?"Next project":"Return to future",index+1<assets.Length?(Action)(()=>ShowProjectStory(index+1)):ShowFuture);
        }
        void ShowTransferQuestion()
        {
            hud.AnnualDialog("Take it to another place · optional","A different community needs homes, but its water supply is already near capacity. Which is the better first planning question?\n\nA. How many buildings can we place with the largest tax receipt?\n\nB. How many homes can the available water and services support, and what will it cost to operate them?\n\nThere is no universally best building. Check needs, resources, timing and commitments together.","B · needs and services",()=>ShowTakeaway(true));
            hud.AnnualSecondary("A · tax receipts",()=>ShowTakeaway(false));
        }
        void ShowTakeaway(bool supported)
        {
            hud.AnnualDialog(supported?"A useful question to take home":"Look beyond the receipt",(supported?"Yes. A building's capacity is different from the service people can actually receive.":"Tax receipts can help the public budget, but a completed home may still lack water or other services.")+"\n\nAsk of a real proposal: Who agrees to it? What need does it meet? What land and resources does it use? Who pays the ongoing costs? What evidence would change the plan?\n\nTerra's geography is sourced. Its public budgets, demands and future outcomes are educational scenarios, not local forecasts.","Return to my future",ShowFuture);
        }
    }
}
