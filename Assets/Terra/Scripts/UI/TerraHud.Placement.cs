using System.Linq;
using Terra.Contracts;
using Terra.Core;
using TMPro;
using UnityEngine;

namespace Terra.UI
{
    public sealed partial class TerraHud
    {
        Transform placementCapsule;
        TextMeshProUGUI placementHeading,placementInstruction,placementSpace;
        HudPlacementAnchor placementAnchor;
        Transform toolHint;
        TextMeshProUGUI toolHintTitle,toolHintPurpose,toolHintReservation;
        string shownHintTool;
        readonly Color blockedPlacement=new Color(1,.71f,.52f);

        static string PlacementPurpose(string id)
        {
            switch(id)
            {
                case "cultivate-standard":return "Food production";
                case "cultivate-margin":return "Farming with a living edge";
                case "conservation-rotation":return "Crop rotation and cover";
                case "cover-crop":return "Planted cover";
                case "restore-grass":return "Meadow restoration";
                case "water-swale":return "Connected runoff storage";
                case "public-park":return "Community recreation";
                case "low-density-housing":case "high-density-housing":return "Homes for residents";
                case "agricultural-storage":return "Farm produce storage";
                case "data-center":return "Computing and jobs";
                case "village-center":return "Local shops and services";
                case "non-renewable-power":return "Energy supply from fuel";
                case "solar":case "wind":return "Clean energy supply";
                case "stewardship-agreement":return "Stewardship and land care";
                case "wastewater-upgrade":return "Wastewater treatment";
                default:return "Inspect this project's services";
            }
        }

        void ConfigurePlacementFeedback()
        {
            var help=Button(actionDock,"?",new Vector2(1,1),new Vector2(-8,-6),new Vector2(44,44),ShowPlacementHelp,18);
            help.name="Placement help";catalogPageButtons.Add(help);
            Label(budgetPanel,"Available to plan",new Vector2(0,1),new Vector2(46,-74),new Vector2(250,18),14,Soft);
            placementCapsule=Panel(root,"Placement feedback",Vector2.zero,Vector2.zero,new Vector2(320,98),Ink);
            placementCapsule.SetSiblingIndex(modalShade.transform.GetSiblingIndex());
            var group=placementCapsule.gameObject.AddComponent<CanvasGroup>();group.blocksRaycasts=false;group.interactable=false;
            placementHeading=Label(placementCapsule,"",new Vector2(0,1),new Vector2(14,-9),new Vector2(292,23),14,Accent,FontStyles.Bold);
            placementInstruction=Label(placementCapsule,"",new Vector2(0,1),new Vector2(14,-34),new Vector2(292,55),15,Paper);
            placementSpace=Label(projectFacts,"",new Vector2(0,1),new Vector2(18,-310),new Vector2(268,44),14,Soft);
            placementSpace.name="Placement footprint and site";
            placementAnchor=placementCapsule.gameObject.AddComponent<HudPlacementAnchor>();
            placementAnchor.canvas=root.GetComponent<Canvas>();
            placementAnchor.obstacles=new[]{(RectTransform)budgetPanel,(RectTransform)annualIndicators,(RectTransform)tooltip.transform,(RectTransform)actionDock,(RectTransform)commandBar};
            placementAnchor.show=()=>!ModalOpen&&!AnnualOverviewOpen&&!ShowingToolHint();
            placementCapsule.gameObject.SetActive(false);
            toolHint=Panel(root,"Project tool explanation",new Vector2(.5f,0),Vector2.zero,new Vector2(344,128),Ink);
            toolHint.SetSiblingIndex(modalShade.transform.GetSiblingIndex());
            var hintGroup=toolHint.gameObject.AddComponent<CanvasGroup>();hintGroup.blocksRaycasts=false;hintGroup.interactable=false;hintGroup.alpha=0;
            toolHintTitle=Label(toolHint,"",new Vector2(0,1),new Vector2(14,-10),new Vector2(316,24),17,Paper,FontStyles.Bold);
            toolHintPurpose=Label(toolHint,"",new Vector2(0,1),new Vector2(14,-38),new Vector2(316,36),16,Paper);
            toolHintReservation=Label(toolHint,"",new Vector2(0,1),new Vector2(14,-78),new Vector2(316,42),16,Soft);
            var hintAnchor=toolHint.gameObject.AddComponent<HudPlacementAnchor>();hintAnchor.show=ShowingToolHint;hintAnchor.tick=UpdateToolHint;
        }

        void ShowPlacementHelp()
        {
            AnnualDialog("Place a project", "<b>1  Choose a project</b>\nSelect a pictured tool. Quiet outlines mark fields that accept it.\n\n<b>2  Preview the land</b>\nHover a field, or use Tab. The dashed footprint shows what the project uses. Read the reservation and any blocking reason beside it.\n\n<b>3  Click to queue</b>\nA Queued label marks your plan. Inspect it to move or remove it; Undo reverses an unsubmitted change. End Year accepts the plan.\n\nEsc cancels your current tool. Drag to pan, scroll to zoom, F to focus a field. Accepted projects remain in history.","Back to landscape",CloseModal);
        }

        bool ShowingToolHint()=>focus!=null&&actionButtons.ContainsKey(focus)&&!ModalOpen&&!AnnualOverviewOpen&&!annualCompleted;
        void UpdateToolHint()
        {
            if(!ShowingToolHint())return;
            string id=actionButtons[focus];
            if(shownHintTool!=id)
            {
                shownHintTool=id;var rule=actionRules.Find(id);var recipes=annualRules.actions.Where(a=>a.actionId==id).ToArray();
                toolHintTitle.text=rule?.title??id;toolHintPurpose.text=PlacementPurpose(id);
                if(recipes.Length>0)
                {
                    long Total(TownActionDefinition r)=>r.upfrontPublicCostCents+r.annualPublicOperatingCents*r.operatingYears+r.annualPublicPaymentCents*r.paymentYears;
                    long min=recipes.Min(Total),max=recipes.Max(Total);
                    toolHintReservation.text=(min==max?Money(min):Money(min)+"–"+Money(max))+" total reservation\nSelect, then preview a marked field";
                }
                else toolHintReservation.text="Unavailable in this scenario";
            }
            var dock=(RectTransform)actionDock;var hint=(RectTransform)toolHint;
            hint.anchoredPosition=new Vector2(dock.anchoredPosition.x,dock.anchoredPosition.y+dock.sizeDelta.y*dock.localScale.y+12);
        }

        public void HidePlacementFeedback(){if(placementCapsule!=null)placementCapsule.gameObject.SetActive(false);}
        public bool IsPlacementCommand(UnityEngine.UI.Button button)=>button!=null&&button==confirmEditButton;

        public void PlacementFeedback(string status,string title,string site,string instruction,string space,
            string rejection,long? reservationDelta,long? availableAfter,Vector3[] points,Camera camera)
        {
            if(placementCapsule==null)return;
            bool blocked=rejection!=null;
            placementHeading.text=status+" · "+site;
            placementHeading.color=blocked?blockedPlacement:Accent;
            placementInstruction.text=instruction;
            placementInstruction.fontSize=LargeText?16.5f:15;
            placementAnchor.points=points;placementAnchor.view=camera;
            placementCapsule.gameObject.SetActive(true);
            previewKind.text=(status=="REPLACE PREVIEW"?"REPLACE":status=="MOVE PREVIEW"?"MOVE":status)+" · "+site;
            previewKind.color=blocked?blockedPlacement:Accent;
            // Even a failed replacement must identify the intended candidate and its first cause.
            if(blocked)
            {
                projectFacts.gameObject.SetActive(false);projectFactsVisible=false;previewBody.gameObject.SetActive(true);
                previewTitle.text=title;previewBody.text=rejection;annualPreviewBody=rejection;
                previewAction.text="Esc keeps your current plan";previewAction.gameObject.SetActive(true);
                recipeDetails.gameObject.SetActive(false);
                LayoutAnnualPreview();
            }
            else if(projectFactsVisible)
            {
                placementSpace.text=space;
                projectReserve.text=reservationDelta==null?projectReserve.text:
                    reservationDelta>0?"Reserve "+Money(reservationDelta.Value)+" more":
                    reservationDelta<0?"Release "+Money(-reservationDelta.Value):"Reservation unchanged";
                if(availableAfter.HasValue)projectReserve.text+="\n"+Money(availableAfter.Value)+" left to plan";
                projectReserve.rectTransform.sizeDelta=new Vector2(268,40);
                LayoutPlacementFacts();
            }
        }

        public void PlacementInspection(Site site,string queued,string accepted)
        {
            if(site==null)return;
            if(queued==null&&accepted==null)
            {
                previewKind.text=site.protectedSite?"PROTECTED AREA":"FIELD · INSPECT";
                previewBody.text=LandRules(site);annualPreviewBody=previewBody.text;
                previewAction.text="Choose a project to preview here";
                confirmEditButton.gameObject.SetActive(false);
            }
            if(queued!=null&&placementSpace!=null)
            {
                var rule=actionRules.Find(queued);
                placementSpace.text=rule?.landscape==true?"Queued whole-field treatment":
                    rule==null?"Queued project":rule.width.ToString("0")+" × "+rule.depth.ToString("0")+" m footprint · field slot reserved";
                LayoutPlacementFacts();
            }
        }

        void LayoutPlacementFacts()
        {
            if(!projectFactsVisible)return;
            // Same dimensions for every recipe/state at this viewport; payment rows never move the dock.
            if(placementSpace==null)return;
            var panel=(RectTransform)tooltip.transform;panel.sizeDelta=new Vector2(304,400);
            projectUpkeep.rectTransform.anchoredPosition=new Vector2(48,-114);projectUpkeep.rectTransform.sizeDelta=new Vector2(238,40);
            upkeepGlyph.rectTransform.anchoredPosition=new Vector2(18,-114);
            projectBenefit.rectTransform.anchoredPosition=new Vector2(48,-158);benefitGlyph.rectTransform.anchoredPosition=new Vector2(18,-158);projectBenefit.rectTransform.sizeDelta=new Vector2(238,26);
            projectTradeoff.rectTransform.anchoredPosition=new Vector2(48,-188);tradeoffGlyph.rectTransform.anchoredPosition=new Vector2(18,-188);projectTradeoff.rectTransform.sizeDelta=new Vector2(238,42);
            projectTiming.gameObject.SetActive(false);timingGlyph.gameObject.SetActive(false); // Opening year is beside upfront cost; full timing stays in Details.
            placementSpace.rectTransform.anchoredPosition=new Vector2(18,-234);placementSpace.rectTransform.sizeDelta=new Vector2(268,40);
            placementSpace.fontSize=LargeText?15.5f:14;
            projectReserve.rectTransform.anchoredPosition=new Vector2(18,-276);projectReserve.rectTransform.sizeDelta=new Vector2(268,42);
            foreach(var text in new[]{projectUpkeep,projectBenefit,projectTradeoff,projectReserve})text.fontSize=LargeText?17.5f:16;
            recipeDetails.GetComponent<RectTransform>().anchoredPosition=new Vector2(0,56);recipeDetails.GetComponent<RectTransform>().sizeDelta=new Vector2(268,24);
            foreach(var button in new[]{confirmEditButton,moveButton,removeButton})
            {var rect=button.GetComponent<RectTransform>();rect.anchoredPosition=new Vector2(rect.anchoredPosition.x,8);rect.sizeDelta=new Vector2(rect.sizeDelta.x,44);}
        }

        public void PlacementAcceptedState(AnnualTown town,TownYearOutcomes baseline)
        {
            var state=town.Snapshot();var current=state.history.LastOrDefault()?.outcomes??baseline;
            var previous=state.history.Length>1?state.history[state.history.Length-2].outcomes:baseline;
            double[] values={current.economicScore,current.socialScore,current.environmentalScore};
            double[] before={previous.economicScore,previous.socialScore,previous.environmentalScore};
            for(int i=0;i<3;i++)annualScoreReadout[i]=annualScores[i].text=values[i].ToString("0.0")+" <size=14>/100</size>\n<size=14>"+
                (state.history.Length==0?"Starting condition":"Last year "+Signed(values[i]-before[i]))+"</size>";
            annualAvailable.text=Money(town.AvailableCents);
        }
    }
}
