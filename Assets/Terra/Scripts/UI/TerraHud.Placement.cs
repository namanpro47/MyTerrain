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
        readonly Color blockedPlacement=new Color(1,.71f,.52f);

        static string PlacementPurpose(string id)
        {
            switch(id)
            {
                case "cultivate-standard":return "Food production from this field";
                case "cultivate-margin":return "Food production with a living edge";
                case "conservation-rotation":return "Varied crops and planted cover";
                case "cover-crop":return "Planted cover between crops";
                case "restore-grass":return "Vegetated meadow cover";
                case "water-swale":return "Stores runoff from connected land";
                case "public-park":return "Community recreation space";
                case "low-density-housing":case "high-density-housing":return "Homes for residents";
                case "agricultural-storage":return "Storage space for farm produce";
                case "data-center":return "Computing capacity and jobs";
                case "village-center":return "Local shops and community services";
                case "non-renewable-power":return "Energy supply from fuel";
                case "solar":case "wind":return "Clean energy supply";
                case "stewardship-agreement":return "Agreed stewardship and land care";
                case "wastewater-upgrade":return "Wastewater treatment capacity";
                default:return "Inspect this project's services";
            }
        }

        void ConfigurePlacementFeedback()
        {
            Label(budgetPanel,"Available to plan",new Vector2(0,1),new Vector2(46,-74),new Vector2(250,18),12,Soft);
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
            placementAnchor.show=()=>!ModalOpen&&!AnnualOverviewOpen;
            placementCapsule.gameObject.SetActive(false);
        }

        public void HidePlacementFeedback(){if(placementCapsule!=null)placementCapsule.gameObject.SetActive(false);}

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
            previewKind.text=status+" · "+site;
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
                    reservationDelta>0?Money(reservationDelta.Value)+" additional reserved":
                    reservationDelta<0?Money(-reservationDelta.Value)+" reservation released":"Reservation unchanged";
                if(availableAfter.HasValue)projectReserve.text+="\n"+Money(availableAfter.Value)+" available after queue";
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
            projectUpkeep.rectTransform.anchoredPosition=new Vector2(48,-114);projectUpkeep.rectTransform.sizeDelta=new Vector2(238,36);
            projectBenefit.rectTransform.anchoredPosition=new Vector2(48,-154);benefitGlyph.rectTransform.anchoredPosition=new Vector2(18,-154);
            projectTradeoff.rectTransform.anchoredPosition=new Vector2(48,-192);tradeoffGlyph.rectTransform.anchoredPosition=new Vector2(18,-192);
            projectTiming.rectTransform.anchoredPosition=new Vector2(48,-266);timingGlyph.rectTransform.anchoredPosition=new Vector2(18,-266);
            projectTiming.rectTransform.sizeDelta=new Vector2(238,20);projectTiming.fontSize=13;
            placementSpace.rectTransform.anchoredPosition=new Vector2(18,-230);placementSpace.rectTransform.sizeDelta=new Vector2(268,36);
            placementSpace.fontSize=LargeText?15:14;
            projectReserve.rectTransform.anchoredPosition=new Vector2(18,-288);projectReserve.rectTransform.sizeDelta=new Vector2(268,36);
            projectReserve.fontSize=LargeText?15.5f:14;
            recipeDetails.GetComponent<RectTransform>().anchoredPosition=new Vector2(0,48);recipeDetails.GetComponent<RectTransform>().sizeDelta=new Vector2(268,26);
        }

        public void PlacementAcceptedState(AnnualTown town,TownYearOutcomes baseline)
        {
            var state=town.Snapshot();var current=state.history.LastOrDefault()?.outcomes??baseline;
            var previous=state.history.Length>1?state.history[state.history.Length-2].outcomes:baseline;
            double[] values={current.economicScore,current.socialScore,current.environmentalScore};
            double[] before={previous.economicScore,previous.socialScore,previous.environmentalScore};
            for(int i=0;i<3;i++)annualScoreReadout[i]=annualScores[i].text=values[i].ToString("0.0")+" <size=50%>/100</size>\n<size=50%>"+
                (state.history.Length==0?"Starting condition":"Last year "+Signed(values[i]-before[i]))+"</size>";
            annualAvailable.text=Money(town.AvailableCents);
        }
    }
}
