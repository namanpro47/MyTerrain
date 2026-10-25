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
        TextMeshProUGUI annualBudget,annualYear,annualWater;
        TextMeshProUGUI[] annualScores=new TextMeshProUGUI[3];
        Image[] annualBars=new Image[3];
        Button endYearButton,reportButton,annualPrimary,annualDetails;
        Action annualPrimaryAction;
        public static string Money(long cents)=>"$"+(cents/100m).ToString("N0",System.Globalization.CultureInfo.InvariantCulture);
        static string Signed(double n)=>n.ToString("+0.0;-0.0;0.0");
        public void ConfigureAnnual(AnnualTownDefinition definition,Action endYear,Action report,Action newRun)
        {
            annualRules=definition;ConfigureAnnualCatalog();production.transform.parent.gameObject.SetActive(false);
            var panel=Panel(root,"Annual indicators",new Vector2(1,1),new Vector2(-24,-24),new Vector2(650,94),PanelColor);
            for(int i=0;i<3;i++)
            {
                string title=new[]{"ECONOMIC","SOCIAL","ENVIRONMENTAL"}[i];
                Label(panel,title,new Vector2(0,1),new Vector2(16+i*212,-10),new Vector2(202,18),13,Soft,FontStyles.Bold);
                annualScores[i]=Label(panel,"—",new Vector2(0,1),new Vector2(16+i*212,-31),new Vector2(200,28),20,Paper);
                var track=Panel(panel,title+" track",new Vector2(0,0),new Vector2(16+i*212,13),new Vector2(196,7),Raised);
                annualBars[i]=Panel(track,title+" meter",new Vector2(0,0),Vector2.zero,new Vector2(0,7),Accent).GetComponent<Image>();
            }
            var budget=Panel(root,"Annual budget",new Vector2(0,1),new Vector2(24,-126),new Vector2(310,196),PanelColor);
            annualYear=Label(budget,"",new Vector2(0,1),new Vector2(16,-12),new Vector2(278,22),17,Accent,FontStyles.Bold);
            annualWater=Label(budget,"",new Vector2(0,1),new Vector2(16,-146),new Vector2(278,42),13,Soft);
            annualBudget=Label(budget,"",new Vector2(0,1),new Vector2(16,-40),new Vector2(278,96),16,Paper);
            budget.SetSiblingIndex(modalShade.transform.GetSiblingIndex());panel.SetSiblingIndex(modalShade.transform.GetSiblingIndex());
            endYearButton=Button(root,"Review year →",new Vector2(1,0),new Vector2(-24,148),new Vector2(148,56),endYear,16);
            reportButton=Button(root,"Last report",new Vector2(1,0),new Vector2(-24,212),new Vector2(148,56),report,16);
            var restart=Button(root,"New run",new Vector2(0,0),new Vector2(24,276),new Vector2(130,44),newRun,14);
            foreach(var button in new[]{endYearButton,reportButton,restart})button.transform.SetSiblingIndex(modalShade.transform.GetSiblingIndex());
            annualPrimary=Button(modal.transform,"",new Vector2(.5f,0),new Vector2(0,86),new Vector2(430,56),()=>annualPrimaryAction?.Invoke(),17);annualPrimary.gameObject.SetActive(false);
            annualDetails=Button(modal.transform,"All proposals",new Vector2(0,0),new Vector2(32,86),new Vector2(230,56),()=>{},17);annualDetails.gameObject.SetActive(false);
            coverSummary.gameObject.SetActive(false);
            tooltip.GetComponent<RectTransform>().sizeDelta=new Vector2(292,320);
            sizes[previewBody]=16;sizes[previewAction]=14;
            previewBody.rectTransform.anchoredPosition=new Vector2(18,-95);
            foreach(var pair in actionButtons)
            {
                if(!annualRules.actions.Any(a=>a.actionId==pair.Value))
                {var caption=pair.Key.GetComponentsInChildren<TextMeshProUGUI>().Last();caption.text="Recipe pending";}
            }
            UpdateActionSummary();
        }
        static string AnnualCategory(string id)=>id=="restore-grass"||id=="water-swale"?"Nature & water":id=="cultivate-standard"||id=="cultivate-margin"||id=="conservation-rotation"?"Farming":"Community";
        void ConfigureAnnualCatalog()
        {
            foreach(var row in actionRows.Values)row.SetActive(false);actionRows.Clear();
            var tabs=categoryButtons.Keys.ToArray();categoryButtons.Clear();
            string[] groups={"Farming","Nature & water","Community"};
            for(int i=0;i<tabs.Length;i++)
            {
                var tab=tabs[i];tab.gameObject.SetActive(i<groups.Length);if(i>=groups.Length)continue;
                string group=groups[i];SetButtonText(tab,group);categoryButtons[tab]=group;
                var rect=tab.GetComponent<RectTransform>();rect.anchoredPosition=new Vector2(10+i*248,-8);rect.sizeDelta=new Vector2(240,38);
                tab.onClick.RemoveAllListeners();tab.onClick.AddListener(()=>ShowCategory(group));
                var row=Rect(actionDock,group+" annual actions",new Vector2(0,1),new Vector2(10,-53),new Vector2(740,124));actionRows[group]=row.gameObject;
                var actions=actionButtons.Where(p=>AnnualGameplay.Supported.Contains(p.Value)&&AnnualCategory(p.Value)==group).ToArray();
                for(int j=0;j<actions.Length;j++)
                {
                    var button=actions[j].Key;button.transform.SetParent(row,false);
                    var r=button.GetComponent<RectTransform>();r.anchoredPosition=new Vector2((732-actions.Length*244)/2+j*244,0);r.sizeDelta=new Vector2(238,124);
                }
            }
            ShowCategory("Farming");
        }
        public void AnnualState(AnnualTown town,TownYearOutcomes baseline,TownYearOutcomes forecast)
        {
            var state=town.Snapshot();var current=state.history.LastOrDefault()?.outcomes??baseline;
            var previous=state.history.Length>1?state.history[state.history.Length-2].outcomes:baseline;
            bool planning=state.phase==TownPhase.Planning;
            double[] values={current.economicScore,current.socialScore,current.environmentalScore};
            double[] before={previous.economicScore,previous.socialScore,previous.environmentalScore};
            double[] next={forecast.economicScore,forecast.socialScore,forecast.environmentalScore};
            for(int i=0;i<3;i++)
            {
                annualScores[i].text=values[i].ToString("0.0")+" <size=65%>/100 · "+(planning?"year-end "+next[i].ToString("0.0"):Signed(values[i]-before[i]))+"</size>";
                annualBars[i].rectTransform.sizeDelta=new Vector2((float)values[i]*1.96f,7);
            }
            annualYear.text="YEAR "+state.year+" / "+annualRules.years+"  ·  "+state.phase.ToString().ToUpperInvariant();
            annualBudget.text="Cash  "+Money(state.cashCents)+"\nExisting obligations  "+Money(town.CommittedCents)+"\nQueued reserve  "+Money(town.QueuedCommitmentCents)+"\n<b>Available  "+Money(town.AvailableCents)+"</b>";
            undoButton.interactable=town.CanUndo;redoButton.interactable=town.CanRedo;resetProposalsButton.interactable=planning&&state.queue.Length>0;
            reportButton.interactable=state.history.Length>0;
            SetButtonText(endYearButton,planning?(state.queue.Length==0?"Wait one year →":"Review year →"):"Year report →");
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
            tooltip.SetActive(site!=null||mode=="reset"||mode=="move");
            if(!tooltip.activeSelf)return;
            previewTitle.text=mode=="reset"?"Clear this year's plan":site?.name??"Choose a destination";
            var asset=state.assets.FirstOrDefault(a=>a.siteId==site?.id);
            var outcome=state.history.LastOrDefault()?.outcomes.sites.FirstOrDefault(s=>s.siteId==site?.id);
            previewKind.text=asset!=null?"ACCEPTED · "+outcome?.phase.ToString().ToUpperInvariant():queued?"QUEUED · NOT YET PAID":"YEAR "+state.year+" PROPOSAL";
            previewKind.color=rejection==null?Accent:Soft;
            if(mode=="remove"||mode=="reset")previewBody.text="Release this year's reservation and restore the previous cover. Accepted projects and completed years stay in place.";
            else if(recipe!=null&&asset==null)
                previewBody.text="<b>"+recipe.title+"</b>\nUpfront "+Money(recipe.upfrontPublicCostCents)+"\nUpkeep "+Money(recipe.annualPublicOperatingCents)+" / yr × "+recipe.operatingYears+"\n"+(recipe.annualPublicReceiptsCents>0?"Tax "+Money(recipe.annualPublicReceiptsCents)+" / yr (tentative)\n":"")+"Opens Y"+(state.year+recipe.constructionYears)+" · matures "+recipe.establishmentYears+" yr\n"+Bundle(recipe);

            else if(asset!=null)
                previewBody.text=annualRules.actions.First(a=>a.siteId==asset.siteId&&a.actionId==asset.actionId).title+"\nStarted year "+asset.startedYear+"\nEstablishment "+((outcome?.maturity??0)*100).ToString("0")+"%\n\nAccepted projects remain for this run. Plan on another field.";
            else previewBody.text=site?.protectedSite==true?"Protected by the scenario. No construction or conversion here.":"Choose an annual action. Eligible footprints use Peter's metric models.";
            bool canEdit=queued&&state.phase==TownPhase.Planning&&mode=="place"&&rejection!=null;
            moveButton.gameObject.SetActive(canEdit);removeButton.gameObject.SetActive(canEdit);
            confirmEditButton.gameObject.SetActive(!canEdit);confirmEditButton.interactable=rejection==null;
            SetButtonText(confirmEditButton,mode=="reset"?"Clear queued plan":mode=="remove"?"Remove proposal":mode=="move"?"Move proposal":"Queue · Enter");
            if(rejection!=null&&!queued&&asset==null&&site!=null)previewBody.text=rejection;
            previewAction.text=rejection==null?(mode=="place"?"Reserves funds; End Year pays.":"Enter to confirm · Esc cancels"):asset!=null?"Accepted projects stay in place.":queued?"Queued · V to move · Delete to remove":"Choose an action and eligible field.";
            previewAction.fontSize=sizes[previewAction]*(LargeText?1.12f:1);previewAction.rectTransform.anchoredPosition=new Vector2(18,68);previewAction.rectTransform.sizeDelta=new Vector2(256,35);
            previewBody.fontSize=sizes[previewBody]*(LargeText?1.12f:1);previewBody.rectTransform.sizeDelta=new Vector2(256,120);
            Paint();
        }
        static string Bundle(TownActionDefinition recipe)=>recipe.actionId=="low-density-housing"?"1 serviced home · 26 × 32 m lot":recipe.actionId=="public-park"?"1 small park · 48 × 40 m":(recipe.fundedQuantity/10000).ToString("0.00")+" ha management agreement";
        public void AnnualDialog(string title,string body,string action,Action callback)
        {
            ShowModal(title,body);annualDetails.gameObject.SetActive(false);
            var primaryRect=annualPrimary.GetComponent<RectTransform>();primaryRect.anchorMin=primaryRect.anchorMax=primaryRect.pivot=new Vector2(.5f,0);primaryRect.anchoredPosition=new Vector2(0,86);
            motionButton.gameObject.SetActive(false);muteButton.gameObject.SetActive(false);textButton.gameObject.SetActive(false);
            annualPrimaryAction=callback;SetButtonText(annualPrimary,action);annualPrimary.gameObject.SetActive(true);
        }
        public void AnnualConfirmation(AnnualTown town,TownYearReport preview,Action accept)
        {
            var s=town.Snapshot();var l=preview.ledger;
            string projects=s.queue.Length==0?"Wait: no new projects. Existing projects still age and receive upkeep.":string.Join("\n",s.queue.Take(3).Select(q=>"• "+annualRules.sites.First(x=>x.id==q.siteId).name+": "+annualRules.actions.First(a=>a.siteId==q.siteId&&a.actionId==q.actionId).title));
            if(s.queue.Length>3)projects+="\n+ "+(s.queue.Length-3)+" more proposals · all included in the total";
            AnnualDialog("Submit year "+s.year+"?", "<size=88%>"+projects+"\n\n<b>Pay now</b>  "+Money(l.upfrontPaidCents+l.operatingPaidCents+l.programPaidCents)+"\nClosing appropriation  "+Money(l.appropriationCents)+"   ·   Receipts  "+Money(l.publicReceiptsCents)+"\nExpected closing cash  "+Money(l.closingCashCents)+"\nFuture obligations  "+Money(l.futureCommitmentsCents)+"\n\n"+preview.acceptedRequest.driver.description+"\n\nAccepted years cannot be undone. Authored rules; no forecast.</size>","End year "+s.year,accept);
            if(s.queue.Length>0)
            {
                annualDetails.gameObject.SetActive(true);SetButtonText(annualDetails,"All "+s.queue.Length+" proposals");
                var primaryRect=annualPrimary.GetComponent<RectTransform>();primaryRect.anchorMin=primaryRect.anchorMax=primaryRect.pivot=new Vector2(1,0);primaryRect.anchoredPosition=new Vector2(-32,86);
                annualDetails.onClick.RemoveAllListeners();annualDetails.onClick.AddListener(()=>AnnualDialog("Queued projects · year "+s.year,"<size=88%>"+string.Join("\n",s.queue.Select(q=>annualRules.sites.First(x=>x.id==q.siteId).name+" — "+annualRules.actions.First(a=>a.siteId==q.siteId&&a.actionId==q.actionId).title))+"\n\nTotal reserved "+Money(town.QueuedCommitmentCents)+". Cash is unchanged until End Year.</size>","Return to budget review",()=>AnnualConfirmation(town,preview,accept)));
            }
        }
        public void AnnualReport(AnnualTown town,TownYearOutcomes baseline,Action continuePlanning)
        {
            var h=town.History;var r=h.Last();var p=h.Length>1?h[h.Length-2].outcomes:baseline;var o=r.outcomes;var l=r.ledger;
            string changes=string.Join(" · ",o.sites.Where(s=>s.actionId!=null).GroupBy(s=>s.phase).Select(g=>g.Count()+" "+g.Key.ToString().ToLowerInvariant()));
            AnnualDialog("Year "+r.year+" · accepted", "<size=88%>"+r.acceptedRequest.driver.description+"\n"+changes+"\n\nCash  "+Money(l.openingCashCents)+" → <b>"+Money(l.closingCashCents)+"</b>\nProjects "+Money(l.upfrontPaidCents)+" · upkeep "+Money(l.operatingPaidCents)+"\nAppropriation "+Money(l.appropriationCents)+" · receipts "+Money(l.publicReceiptsCents)+"\nFuture obligations "+Money(l.futureCommitmentsCents)+" · arrears "+Money(l.closingArrearsCents)+"\n\n<b>Economic "+o.economicScore.ToString("0.0")+" ("+Signed(o.economicScore-p.economicScore)+")</b> · output "+o.productionUnits.ToString("0.0")+" / 40 + upkeep funded\n<b>Social "+o.socialScore.ToString("0.0")+" ("+Signed(o.socialScore-p.socialScore)+")</b> · food / home / recreation services "+o.serviceUnits.ToString("0.0")+" / 18\n<b>Environment "+o.environmentalScore.ToString("0.0")+" ("+Signed(o.environmentalScore-p.environmentalScore)+")</b> · cover "+(100*o.vegetatedAreaM2/o.managedAreaM2).ToString("0")+"% + storage "+o.retentionCapacityM3.ToString("0")+" m³\n\n<size=82%>Starter rubric v2: 80/20 weights, capped 0–100. Authored effects, not yield, wellbeing or flood forecasts. Adapt on uncommitted fields.</size></size>",town.Phase==TownPhase.Reviewing?"Plan year "+(town.Year+1):"Return to landscape",continuePlanning);
        }
    }
}
