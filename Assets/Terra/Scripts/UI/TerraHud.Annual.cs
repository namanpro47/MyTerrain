using System;
using System.Linq;
using Terra.Contracts;
using Terra.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Terra.UI
{
    // TN-05, TN-06, CS-01, CS-03: quiet landscape HUD, explicit budget, inspectable consequences.
    public sealed partial class TerraHud
    {
        AnnualTownDefinition annualRules;
        TownYearOutcomes acceptedOutcomes;
        Action reviewYearShortcut,reportShortcut,futureShortcut;
        TextMeshProUGUI annualBudget,annualYear,annualWater,annualAvailable;
        Transform annualIndicators,commandBar;
        TextMeshProUGUI[] annualScores=new TextMeshProUGUI[3];
        readonly string[] annualScoreReadout=new string[3];
        Image[] annualBars=new Image[3];
        Button endYearButton,reportButton,restartButton,futureButton,annualPrimary,annualDetails,recipeDetails;
        Transform budgetPanel;
        bool annualCompact,annualCompleted;
        public void MapView(string title){placeSubtitle.text=title??location.Subtitle;if(title!=null)annualYear.text="YOUR SAVED YEAR 10";}
        public void HideAnnualPreview(){tooltip.SetActive(false);ReflowPlanningDock();}
        TownActionDefinition inspectedRecipe;
        string annualPreviewBody,annualPreviewTimeline;
        Action annualPrimaryAction;
        public static string Money(long cents)=>"$"+(cents/100m).ToString("N0",System.Globalization.CultureInfo.InvariantCulture);
        static string CompactMoney(long cents)=>cents>=100000000?"$"+(cents/100000000m).ToString("0.#")+"M":"$"+(cents/100000m).ToString("0")+"K";
        static string Signed(double n)=>n.ToString("+0.0;-0.0;0.0");
        public void ConfigureAnnual(AnnualTownDefinition definition,Action endYear,Action report,Action newRun)
        {
            annualRules=definition;reviewYearShortcut=endYear;reportShortcut=report;ConfigureAnnualCatalog();production.transform.parent.gameObject.SetActive(false);
            var panel=annualIndicators=Panel(root,"Annual indicators",new Vector2(1,1),new Vector2(-24,-24),new Vector2(650,94),PanelColor);
            for(int i=0;i<3;i++)
            {
                string title=new[]{"ECONOMIC","SOCIAL","ENVIRONMENTAL"}[i];
                Label(panel,title,new Vector2(0,1),new Vector2(14+i*174,-10),new Vector2(166,18),13,Soft,FontStyles.Bold);
                annualScores[i]=Label(panel,"—",new Vector2(0,1),new Vector2(14+i*174,-31),new Vector2(166,30),24,Paper,FontStyles.Bold);
                var track=Panel(panel,title+" track",new Vector2(0,0),new Vector2(14+i*174,13),new Vector2(154,5),Raised);
                annualBars[i]=Panel(track,title+" meter",new Vector2(0,0),Vector2.zero,new Vector2(0,5),Accent).GetComponent<Image>();
            }
            var budget=budgetPanel=Panel(root,"Annual budget",new Vector2(0,1),new Vector2(24,-126),new Vector2(310,196),PanelColor);
            annualYear=Label(budget,"",new Vector2(0,1),new Vector2(16,-12),new Vector2(278,22),17,Accent,FontStyles.Bold);
            annualWater=Label(budget,"",new Vector2(0,1),new Vector2(16,-146),new Vector2(278,42),13,Soft);
            annualAvailable=Label(budget,"",new Vector2(0,1),new Vector2(14,-40),new Vector2(282,38),28,Paper,FontStyles.Bold);
            commandBar=Panel(root,"Planning controls",new Vector2(.5f,0),new Vector2(0,10),new Vector2(1248,48),Ink);commandBar.SetSiblingIndex(modalShade.transform.GetSiblingIndex());
            annualBudget=Label(budget,"",new Vector2(0,1),new Vector2(16,-40),new Vector2(278,96),16,Paper);
            budget.SetSiblingIndex(modalShade.transform.GetSiblingIndex());panel.SetSiblingIndex(modalShade.transform.GetSiblingIndex());
            endYearButton=Button(root,"Review year →",new Vector2(1,0),new Vector2(-24,148),new Vector2(148,56),endYear,16);
            reportButton=Button(root,"Last report",new Vector2(1,0),new Vector2(-24,212),new Vector2(148,56),report,16);
            var restart=restartButton=Button(root,"New run",new Vector2(0,0),new Vector2(24,276),new Vector2(130,44),newRun,14);
            foreach(var button in new[]{endYearButton,reportButton,restart})button.transform.SetSiblingIndex(modalShade.transform.GetSiblingIndex());
            annualPrimary=Button(modal.transform,"",new Vector2(.5f,0),new Vector2(0,86),new Vector2(430,56),()=>annualPrimaryAction?.Invoke(),17);annualPrimary.gameObject.SetActive(false);
            annualDetails=Button(modal.transform,"All proposals",new Vector2(0,0),new Vector2(32,86),new Vector2(230,56),()=>{},17);annualDetails.gameObject.SetActive(false);
            recipeDetails=Button(tooltip.transform,"Costs, services & trade-offs",new Vector2(.5f,0),new Vector2(0,68),new Vector2(256,34),()=>AnnualProjectDetails(inspectedRecipe,false),13);
            recipeDetails.gameObject.SetActive(false);
            coverSummary.gameObject.SetActive(false);
            tooltip.GetComponent<RectTransform>().sizeDelta=new Vector2(292,320);
            sizes[previewBody]=16;sizes[previewAction]=14;
            previewBody.rectTransform.anchoredPosition=new Vector2(18,-95);
            foreach(var pair in actionButtons)
            {
                var recipe=annualRules.actions.FirstOrDefault(a=>a.actionId==pair.Value);
                var caption=pair.Key.GetComponentsInChildren<TextMeshProUGUI>().Last();
                caption.text=recipe==null?"—":CompactMoney(recipe.upfrontPublicCostCents);
            }
            ConfigurePlayFirst();UpdateActionSummary();
        }
        int catalogPage,catalogColumns=4;
        readonly System.Collections.Generic.List<Button> catalogPageButtons=new System.Collections.Generic.List<Button>();
        static string AnnualCategory(string id) => new[]{"low-density-housing","high-density-housing","public-park","village-center"}.Contains(id)?"Homes & Community":new[]{"agricultural-storage","data-center","non-renewable-power","solar","wind","wastewater-upgrade"}.Contains(id)?"Work & Utilities":"Land & Nature";
        void ConfigureAnnualCatalog()
        {
            foreach(var row in actionRows.Values)row.SetActive(false);actionRows.Clear();
            var tabs=categoryButtons.Keys.ToArray();categoryButtons.Clear();
            string[] groups={"Land & Nature","Homes & Community","Work & Utilities"};
            for(int i=0;i<tabs.Length;i++)
            {
                var tab=tabs[i];tab.gameObject.SetActive(i<groups.Length);if(i>=groups.Length)continue;
                string group=groups[i];SetButtonText(tab,group);categoryButtons[tab]=group;
                var rect=tab.GetComponent<RectTransform>();rect.anchoredPosition=new Vector2(10+i*248,-8);rect.sizeDelta=new Vector2(240,38);
                tab.GetComponentInChildren<TextMeshProUGUI>().rectTransform.sizeDelta=new Vector2(224,34);
                tab.onClick.RemoveAllListeners();tab.onClick.AddListener(()=>{catalogPage=0;ShowCategory(group);});
                var row=Rect(actionDock,group+" annual actions",new Vector2(0,1),new Vector2(10,-53),new Vector2(740,124));actionRows[group]=row.gameObject;
                var actions=actionButtons.Where(p=>FullAnnualGameplay.Supported.Contains(p.Value)&&AnnualCategory(p.Value)==group).ToArray();
                for(int j=0;j<actions.Length;j++)
                {
                    var button=actions[j].Key;button.transform.SetParent(row,false);
                    var r=button.GetComponent<RectTransform>();r.anchoredPosition=new Vector2(36+(j%4)*168,0);r.sizeDelta=new Vector2(162,124);
                    foreach(var text in button.GetComponentsInChildren<TextMeshProUGUI>())text.rectTransform.sizeDelta=new Vector2(154,text.rectTransform.sizeDelta.y);
                    var icon=button.transform.Find("Model icon")?.GetComponent<RectTransform>();if(icon!=null)icon.sizeDelta=new Vector2(150,62);
                }
                if(actions.Length>3)
                {
                    var prev=Button(row,"‹",new Vector2(0,1),new Vector2(0,0),new Vector2(30,124),()=>{catalogPage=Math.Max(0,catalogPage-1);ApplyCatalogPage();},22);
                    var next=Button(row,"›",new Vector2(1,1),new Vector2(0,0),new Vector2(30,124),()=>{catalogPage=(catalogPage+1)%((actions.Length+catalogColumns-1)/catalogColumns);ApplyCatalogPage();},22);
                    catalogPageButtons.Add(prev);catalogPageButtons.Add(next);
                }
            }
            ShowCategory("Land & Nature");lastScreen=Vector2.zero;
        }
        void ApplyCatalogPage()
        {
            if(annualRules==null)return;
            foreach(var group in actionRows.Keys)
            {
                var buttons=actionButtons.Where(p=>AnnualCategory(p.Value)==group).Select(p=>p.Key).ToArray();
                for(int i=0;i<buttons.Length;i++)buttons[i].gameObject.SetActive(i/catalogColumns==catalogPage);
            }
        }
        public void AnnualState(AnnualTown town,TownYearOutcomes baseline,TownYearOutcomes forecast)
        {
            var state=town.Snapshot();var current=state.history.LastOrDefault()?.outcomes??baseline;
            acceptedOutcomes=current;
            bool completed=state.phase==TownPhase.Completed;
            if(annualCompleted!=completed)
            {
                annualCompleted=completed;catalogKeyboard=false;
                if(focus!=null&&(focus.transform.IsChildOf(actionDock)||focus==undoButton||focus==redoButton||focus==resetProposalsButton))Focus(null);
                lastScreen=Vector2.zero;Layout();
            }
            var previous=state.history.Length>1?state.history[state.history.Length-2].outcomes:baseline;
            bool planning=state.phase==TownPhase.Planning;
            double[] values={current.economicScore,current.socialScore,current.environmentalScore};
            double[] before={previous.economicScore,previous.socialScore,previous.environmentalScore};
            double[] next={forecast.economicScore,forecast.socialScore,forecast.environmentalScore};
            for(int i=0;i<3;i++)
            {
                annualScoreReadout[i]=annualScores[i].text=values[i].ToString("0.0")+" <size=50%>/100</size>"+"\n<size=50%>"+(planning?"Year-end  "+next[i].ToString("0.0"):"Change  "+Signed(values[i]-before[i]))+"</size>";
                annualBars[i].rectTransform.sizeDelta=new Vector2((float)values[i]*1.40f,5);
            }
            annualYear.text="YEAR "+state.year+"/"+annualRules.years+" · "+(state.phase==TownPhase.Completed?"COMPLETE":state.phase==TownPhase.Reviewing?"REVIEW":state.phase.ToString().ToUpperInvariant());
            annualAvailable.text=Money(town.AvailableCents)+" <size=45%>available</size>";
            annualBudget.text="Cash  "+Money(state.cashCents)+"\nCommitted  "+Money(town.CommittedCents)+"   ·   Plan  "+Money(town.QueuedCommitmentCents);
            undoButton.interactable=town.CanUndo;redoButton.interactable=town.CanRedo;resetProposalsButton.interactable=planning&&state.queue.Length>0;
            reportButton.interactable=state.history.Length>0;
            SetButtonText(endYearButton,planning?"End Year":state.phase==TownPhase.Resolving?"Year pending →":"Year report →");
            foreach(var pair in actionButtons)pair.Key.interactable=planning&&annualRules.actions.Any(a=>a.actionId==pair.Value);
            Paint();
        }
        public void AnnualWater(SimulationOutput current,SimulationOutput preview)
        {
            annualWater.text="Fixed storm · discharged "+current.dischargedM3.ToString("N0")+" m³\nYear-end cover: "+preview.dischargedM3.ToString("N0")+" m³ · not flood risk";
        }
        public void AnnualPreview(Site site,TownActionDefinition recipe,AnnualTownSnapshot state,string mode,string rejection,Action confirm,bool queued)
        {
            confirmEdit=confirm;tip.transform.parent.gameObject.SetActive(false);
            var existing=state.queue.FirstOrDefault(q=>q.siteId==site?.id);
            bool replacing=mode=="place"&&existing!=null&&recipe!=null&&existing.actionId!=recipe.actionId&&rejection==null;
            // A blocked replacement still exposes Move/Remove for the real queued project.
            if(mode=="place"&&queued&&rejection!=null&&existing!=null)recipe=annualRules.actions.First(a=>a.siteId==existing.siteId&&a.actionId==existing.actionId);
            inspectedRecipe=recipe;recipeDetails.gameObject.SetActive(recipe?.services!=null&&rejection==null&&mode=="place");
            tooltip.SetActive(site!=null||mode=="reset"||mode=="move");
            if(!tooltip.activeSelf){ReflowPlanningDock();return;}
            previewTitle.text=mode=="reset"?"Clear this year's plan":site?.name??"Choose a destination";
            var asset=state.assets.FirstOrDefault(a=>a.siteId==site?.id);
            var outcome=state.history.LastOrDefault()?.outcomes.sites.FirstOrDefault(s=>s.siteId==site?.id);
            previewKind.text=asset!=null?"ACCEPTED · "+outcome?.phase.ToString().ToUpperInvariant():queued?"QUEUED · NOT YET PAID":state.phase==TownPhase.Completed?"YEAR 10 · READ-ONLY":"YEAR "+state.year+" PROPOSAL";
            previewKind.color=rejection==null?Accent:Soft;
            if(mode=="remove"||mode=="reset")previewBody.text="Release this year's reservation and restore the previous cover. Accepted projects and completed years stay in place.";
            else if(recipe!=null&&asset==null)
                previewBody.text="<b>"+recipe.title+"</b>\n"+Money(recipe.upfrontPublicCostCents)+" upfront\n"+Money(recipe.annualPublicOperatingCents)+" / yr × "+recipe.operatingYears+"\nReserve "+Money(recipe.upfrontPublicCostCents+recipe.annualPublicOperatingCents*recipe.operatingYears+recipe.annualPublicPaymentCents*recipe.paymentYears)+"\nOpens Y"+(state.year+recipe.constructionYears)+" · matures "+recipe.establishmentYears+" yr\n"+(recipe.services==null?Bundle(recipe):(recipe.services.occupiedAreaM2/10000).ToString("0.00")+" ha · "+recipe.services.energyDemand.ToString("0.#")+" energy / "+recipe.services.waterDemand.ToString("0.#")+" water units");

            else if(asset!=null)
                previewBody.text=annualRules.actions.First(a=>a.siteId==asset.siteId&&a.actionId==asset.actionId).title+"\nStarted year "+asset.startedYear+"\nEstablishment "+((outcome?.maturity??0)*100).ToString("0")+"%\n\n"+(state.phase==TownPhase.Completed?"Final accepted project.":"Permanent this run. Use another field.");
            else previewBody.text=site?.protectedSite==true?"Protected by the scenario. No construction or conversion here.":"Choose an annual action. Projects must fit at their real size.";
            bool canEdit=queued&&state.phase==TownPhase.Planning&&mode=="place"&&rejection!=null;
            moveButton.gameObject.SetActive(canEdit);removeButton.gameObject.SetActive(canEdit);
            confirmEditButton.gameObject.SetActive(!canEdit&&state.phase==TownPhase.Planning);confirmEditButton.interactable=rejection==null;
            SetButtonText(confirmEditButton,mode=="reset"?"Clear queued plan":mode=="remove"?"Remove proposal":mode=="move"?"Move proposal":replacing?"Replace · Enter":"Queue · Enter");
            if(state.phase==TownPhase.Completed&&asset==null)
                previewBody.text=site?.protectedSite==true?"Protected scenario area. No project was accepted here.":"Existing land use. No new project was accepted here.\n\nInspect other sites, compare the landscape or start a new run.";
            else if(rejection!=null&&!queued&&asset==null&&site!=null)previewBody.text=rejection;
            previewAction.text=rejection==null?(mode=="place"?"Reserves funds; End Year pays.":"Enter to confirm · Esc cancels"):asset!=null?"Accepted projects stay in place.":queued?"Queued · V to move · Delete to remove":state.phase==TownPhase.Completed?"Future · compare, learn or replay.":"Choose an action and eligible field.";
            previewAction.gameObject.SetActive(!recipeDetails.gameObject.activeSelf);
            previewAction.fontSize=sizes[previewAction]*(LargeText?1.12f:1);previewAction.rectTransform.anchoredPosition=new Vector2(18,68);previewAction.rectTransform.sizeDelta=new Vector2(256,35);
            previewBody.fontSize=sizes[previewBody]*(LargeText?1.12f:1);previewBody.rectTransform.sizeDelta=new Vector2(256,120);
            annualPreviewBody=previewBody.text;
            annualPreviewTimeline=recipe==null?null:"\nOpens Y"+(state.year+recipe.constructionYears)+" · matures "+recipe.establishmentYears+" yr";
            UpdateProjectFacts(site,recipe,state,mode,rejection,queued);
            LayoutAnnualPreview();ReflowPlanningDock();Paint();
        }
        static string Bundle(TownActionDefinition recipe)=>recipe.services!=null?recipe.fundedUnit:recipe.actionId=="low-density-housing"?"1 serviced home · 26 × 32 m lot":recipe.actionId=="public-park"?"1 small park · 48 × 40 m":(recipe.fundedQuantity/10000).ToString("0.00")+" ha management agreement";
        public void AnnualDialog(string title,string body,string action,Action callback)
        {
            ShowModal(title,body);annualDetails.gameObject.SetActive(false);
            var primaryRect=annualPrimary.GetComponent<RectTransform>();primaryRect.anchorMin=primaryRect.anchorMax=primaryRect.pivot=new Vector2(.5f,0);primaryRect.anchoredPosition=new Vector2(0,86);
            motionButton.gameObject.SetActive(false);muteButton.gameObject.SetActive(false);textButton.gameObject.SetActive(false);
            annualPrimaryAction=callback;SetButtonText(annualPrimary,action);annualPrimary.gameObject.SetActive(true);annualPrimary.interactable=callback!=null;
        }
        public void AnnualConfirmation(AnnualTown town,TownYearReport preview,Action accept,bool onlineChapter=false)
        {
            FieldNotesBudget(town.Snapshot(),preview.ledger,onlineChapter?"AI chapter":"Offline chapter",accept);
        }

        public void AnnualReport(AnnualTown town,TownYearOutcomes baseline,Action continuePlanning,Action backToChapter=null)
        {
            var h=town.History;var r=h.Last();var p=h.Length>1?h[h.Length-2].outcomes:baseline;var o=r.outcomes;var l=r.ledger;
            if(o.economy!=null)
            {
                AnnualDialog("Year "+r.year+" · accepted", "<size=88%>"+r.acceptedRequest.driver.description+"\n\nCash "+Money(l.openingCashCents)+" → <b>"+Money(l.closingCashCents)+"</b>\nProjects "+Money(l.upfrontPaidCents)+" · operation "+Money(l.operatingPaidCents)+"\nRepairs "+Money(l.repairPaidCents)+" · old arrears paid "+Money(l.arrearsPaidCents)+"\nReceipts "+Money(l.publicReceiptsCents)+" · appropriation "+Money(l.appropriationCents)+"\nArrears "+Money(l.closingArrearsCents)+" · future commitments "+Money(l.futureCommitmentsCents)+"\n\nEconomic "+o.economicScore.ToString("0.0")+" ("+Signed(o.economicScore-p.economicScore)+") · Social "+o.socialScore.ToString("0.0")+" ("+Signed(o.socialScore-p.socialScore)+")\nEnvironment "+o.environmentalScore.ToString("0.0")+" ("+Signed(o.environmentalScore-p.environmentalScore)+")\n\n"+o.sites.Count(x=>x.phase==TownAssetPhase.Building&&!string.IsNullOrEmpty(x.actionId))+" building · "+o.sites.Count(x=>x.phase==TownAssetPhase.Establishing&&!string.IsNullOrEmpty(x.actionId))+" establishing · "+o.disruptedSites+" disrupted sites\n"+string.Join("\n",o.economy.reasons)+"</size>",town.Phase==TownPhase.Reviewing?"Plan year "+(town.Year+1):"See ten-year future",continuePlanning);
                AnnualSecondary("Meter details",()=>AnnualMetrics(o,()=>AnnualReport(town,baseline,continuePlanning,backToChapter)));
            }
            else AnnualDialog("Year "+r.year+" · accepted","Cash "+Money(l.closingCashCents)+"\n"+o.economicExplanation+"\n"+o.socialExplanation+"\n"+o.environmentalExplanation,"Continue",continuePlanning);
            if(backToChapter!=null){SetButtonText(annualPrimary,AnnualOverviewOpen?"Back to year highlights":"Back to chapter");annualPrimaryAction=backToChapter;}
        }
        void AnnualProjectDetails(TownActionDefinition a,bool effects)
        {
            if(a?.services==null)return;var s=a.services;
            string body=effects?"<b>When operating and funded</b>\nProductive output "+a.productionUnits.ToString("0.0")+" units · sustained jobs "+s.jobs+"\nHomes "+s.homes+" · recreation "+s.recreation+" · local services "+s.localServices+"\nEnergy generated "+s.energySupply+" (clean "+s.cleanEnergySupply+") · energy used "+s.energyDemand+"\nWater used "+s.waterDemand+" · wastewater "+s.wastewaterLoad+"\nWastewater treatment "+s.wastewaterCapacity+" · connected runoff capacity "+a.retentionCapacityM3+" m³\nEmissions pressure "+s.emissionsPressure+" scenario units\n\n<b>Trade-offs</b>"+"\nOccupies "+s.occupiedAreaM2.ToString("N0")+" m²; previous production on that land is displaced. Useful services cap at unmet need. Upkeep and resource shortages limit operation; saved conditions can change output. These are scenario units, not forecasts."
                :a.fundedUnit+"\n\nUpfront "+Money(a.upfrontPublicCostCents)+"\nOperating cost "+Money(a.annualPublicOperatingCents)+" / yr for "+a.operatingYears+" operating years\nProgram payment "+Money(a.annualPublicPaymentCents)+" / yr for "+a.paymentYears+" years\nEligible receipt "+Money(a.annualPublicReceiptsCents)+" / yr; housing needs occupied, supplied homes\nConstruction "+a.constructionYears+" years · establishment "+a.establishmentYears+" years\n\n"+a.ownerOperator+". Fictional consent/access are scenario prerequisites. Total project cost is unknown; this is the public contribution. No historic refund.";
            AnnualDialog(a.title,"<size=86%>"+body+"</size>",effects?"Costs & agreement":"Services & trade-offs",()=>AnnualProjectDetails(a,!effects));
            AnnualSecondary("Return to plan",CloseModal);
        }
        public void AnnualSecondary(string caption,Action callback)
        {
            annualDetails.gameObject.SetActive(true);SetButtonText(annualDetails,caption);
            annualDetails.onClick.RemoveAllListeners();annualDetails.onClick.AddListener(()=>callback());
            var r=annualPrimary.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=r.pivot=new Vector2(1,0);r.anchoredPosition=new Vector2(-32,86);
        }
        public void AnnualMetrics(TownYearOutcomes o,Action back)
        {
            var e=o.economy;
            AnnualDialog("Why these three indicators?","<size=83%>"+o.economicExplanation+"\n\n"+o.socialExplanation+"\n\n"+o.environmentalExplanation+"\n\nOutput "+o.productionUnits.ToString("0.0")+" / "+annualRules.rubric.productionTargetUnits+" · jobs "+e.jobs.ToString("0.0")+" / "+annualRules.economy.jobsTarget+"\nEnergy "+e.energySupplied.ToString("0.0")+" / "+e.energyDemand.ToString("0.0")+" · clean "+e.cleanEnergySupplied.ToString("0.0")+"\nDrinking water "+e.waterSupplied.ToString("0.0")+" / "+e.waterDemand.ToString("0.0")+" · sewage treated "+e.wastewaterTreated.ToString("0.0")+" / "+e.wastewaterLoad.ToString("0.0")+"\nStorm generated / retained / discharged: "+e.generatedM3.ToString("0")+" / "+e.retainedM3.ToString("0")+" / "+e.dischargedM3.ToString("0")+" m³\n\n"+annualRules.rubric.version+" · scenario units, not local forecasts.</size>","Back",back);
        }

    }
}
