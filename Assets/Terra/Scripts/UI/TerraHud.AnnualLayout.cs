using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Terra.UI
{
    public sealed partial class TerraHud
    {
        // Fit a focused project into the actual clear space between the panels, including
        // compact layouts and browser display scaling. Coordinates are camera viewport units.
        public Rect LandscapeFocusViewport()
        {
            Canvas.ForceUpdateCanvases();float padding=16*PixelRatio;var canvas=root.GetComponent<Canvas>().pixelRect;
            Rect ScreenRect(Transform panel)
            {
                var corners=new Vector3[4];((RectTransform)panel).GetWorldCorners(corners);
                return UnityEngine.Rect.MinMaxRect(corners[0].x,corners[0].y,corners[2].x,corners[2].y);
            }
            var left=ScreenRect(budgetPanel);var right=ScreenRect(tooltip.transform);
            var dock=ScreenRect(actionDock);var feedback=ScreenRect(toastPanel.transform);
            return UnityEngine.Rect.MinMaxRect(Mathf.Min((left.xMax+padding-canvas.x)/canvas.width,.45f),
                Mathf.Min(((annualCompleted?canvas.y:dock.yMax)+padding-canvas.y)/canvas.height,.48f),
                Mathf.Max((right.xMin-padding-canvas.x)/canvas.width,.55f),
                Mathf.Max((feedback.yMin-padding-canvas.y)/canvas.height,.52f));
        }
        // At 125% zoom on a 1280×720 display, keep primary controls outside the map preview.
        // Secondary actions share the bottom bar; each catalog card keeps its price badge.
        void LayoutAnnual(float width,float height,bool compact)
        {
            annualCompact=compact;actionDock.gameObject.SetActive(!annualCompleted);
            void Position(Transform t,Vector2 anchor,Vector2 position,Vector2 size)
            {var r=(RectTransform)t;r.anchorMin=r.anchorMax=r.pivot=anchor;r.anchoredPosition=position;r.sizeDelta=size;}
            void Place(Button button,Transform parent,Vector2 position,float w)
            {
                if(button==null)return;button.transform.SetParent(parent,false);
                Position(button.transform,Vector2.zero,position,new Vector2(w,36));
                foreach(var label in button.GetComponentsInChildren<TextMeshProUGUI>(true))
                {label.rectTransform.sizeDelta=new Vector2(w-10,32);label.fontSize=14*(LargeText?1.12f:1);}
            }
            var identity=root.Find("Place");
            Position(identity,new Vector2(0,1),new Vector2(16,-16),new Vector2(310,54));
            var identityLabels=identity.GetComponentsInChildren<TextMeshProUGUI>(true);
            identityLabels[0].gameObject.SetActive(false);
            identityLabels[1].rectTransform.anchoredPosition=new Vector2(14,-7);identityLabels[1].fontSize=20;
            placeSubtitle.rectTransform.anchoredPosition=new Vector2(14,-30);placeSubtitle.fontSize=12;
            Position(budgetPanel,new Vector2(0,1),new Vector2(16,-78),new Vector2(310,124));
            annualYear.rectTransform.anchoredPosition=new Vector2(14,-10);annualYear.fontSize=13;
            annualAvailable.rectTransform.anchoredPosition=new Vector2(14,-32);annualAvailable.fontSize=28*(LargeText?1.12f:1);
            annualBudget.rectTransform.anchoredPosition=new Vector2(14,-77);annualBudget.rectTransform.sizeDelta=new Vector2(286,40);annualBudget.fontSize=12*(LargeText?1.12f:1);
            annualWater.gameObject.SetActive(false); // Full storm facts remain in Meter details.
            Position(annualIndicators,new Vector2(1,1),new Vector2(-16,-16),new Vector2(532,98));
            for(int i=0;i<3;i++)
            {
                annualScores[i].rectTransform.anchoredPosition=new Vector2(14+i*174,-30);
                annualScores[i].rectTransform.sizeDelta=new Vector2(166,56);annualScores[i].fontSize=22*(LargeText?1.12f:1);
                ((RectTransform)annualBars[i].transform.parent).anchoredPosition=new Vector2(14+i*174,7);
                annualBars[i].color=new[]{new Color(.94f,.76f,.43f),new Color(.50f,.76f,.93f),Accent}[i];
            }
            float barWidth=Mathf.Min(width-32,1248);
            Position(commandBar,new Vector2(.5f,0),new Vector2(0,10),new Vector2(barWidth,48));
            var utilities=new[]{undoButton,redoButton,resetProposalsButton,viewButton,guideButton,controlsButton,futureButton,reportButton,endYearButton};
            float unit=(barWidth-26-154)/8;
            for(int i=0;i<utilities.Length;i++)
            {
                var button=utilities[i];if(button==null)continue;
                button.gameObject.SetActive(!annualCompleted||i>=3&&i<8);
                Place(button,commandBar,new Vector2(6+i*(unit+2),6),i==8?154:unit);
            }
            SetButtonText(resetProposalsButton,"Clear plan");SetButtonText(viewButton,"View  R");SetButtonText(guideButton,"Guide  G");SetButtonText(controlsButton,"Settings  ?");SetButtonText(futureButton,"Future  T");SetButtonText(reportButton,"Report  L");
            Place(restartButton,root,new Vector2(16,66),84);SetButtonText(restartButton,"New run");
            restartButton.transform.SetSiblingIndex(modalShade.transform.GetSiblingIndex());
            float dockWidth=688,cardHeight=compact?70:86;
            actionDock.localScale=Vector3.one*Mathf.Min(1,(width-220)/dockWidth);
            Position(actionDock,new Vector2(.5f,0),new Vector2(0,66),new Vector2(dockWidth,compact?140:158));
            actionSummary.gameObject.SetActive(true);actionSummary.rectTransform.anchoredPosition=new Vector2(14,6);actionSummary.rectTransform.sizeDelta=new Vector2(660,20);actionSummary.fontSize=12;
            int ci=0;
            foreach(var category in categoryButtons.Keys)
            {Position(category.transform,new Vector2(0,1),new Vector2(8+ci++*224,-6),new Vector2(220,30));foreach(var label in category.GetComponentsInChildren<TextMeshProUGUI>()){label.rectTransform.sizeDelta=new Vector2(210,28);label.fontSize=14;}}
            foreach(var row in actionRows.Values)Position(row.transform,new Vector2(0,1),new Vector2(8,-42),new Vector2(672,cardHeight));
            foreach(var group in actionRows.Keys)
            {
                int i=0;
                foreach(var pair in actionButtons.Where(p=>AnnualCategory(p.Value)==group))
                {
                    var button=pair.Key;Position(button.transform,new Vector2(0,1),new Vector2(28+(i++%4)*155,0),new Vector2(149,cardHeight));
                    var icon=button.transform.Find("Model icon") as RectTransform;
                    if(icon!=null)Position(icon,new Vector2(0,1),new Vector2(7,-3),new Vector2(84,compact?42:58));
                    foreach(var label in button.GetComponentsInChildren<TextMeshProUGUI>(true))
                    {
                        bool price=label.name=="Action price";
                        label.gameObject.SetActive(!string.IsNullOrEmpty(label.text));
                        if(price){Position(label.transform,new Vector2(1,1),new Vector2(-6,-21),new Vector2(54,25));label.alignment=TextAlignmentOptions.Right;label.fontSize=12;}
                        else if(label.name=="Action title"){Position(label.transform,new Vector2(.5f,1),new Vector2(0,compact?-45:-61),new Vector2(143,24));label.fontSize=13;}
                    }
                }
            }
            foreach(var b in catalogPageButtons)
            {b.GetComponent<RectTransform>().sizeDelta=new Vector2(24,cardHeight);b.GetComponentInChildren<TextMeshProUGUI>().rectTransform.sizeDelta=new Vector2(24,cardHeight-4);}
            float feedbackWidth=Mathf.Clamp(width-680,250,520);
            Position(toastPanel.transform,new Vector2(.5f,1),new Vector2(0,-118),new Vector2(feedbackWidth,52));
            toast.rectTransform.sizeDelta=new Vector2(feedbackWidth-24,46);toast.fontSize=14;
            LayoutAnnualPreview();
        }
        void LayoutAnnualPreview()
        {
            if(recipeDetails==null)return;
            // Reflow existing inspection text too: resizing need not move the pointer.
            if(annualPreviewBody!=null)previewBody.text=annualCompact&&!string.IsNullOrEmpty(annualPreviewTimeline)?annualPreviewBody.Replace(annualPreviewTimeline,""):annualPreviewBody;
            var r=(RectTransform)tooltip.transform;r.sizeDelta=new Vector2(292,annualCompact?248:LargeText?350:320);r.anchoredPosition=new Vector2(-16,-120);
            previewTitle.rectTransform.anchoredPosition=new Vector2(18,annualCompact?-34:-45);previewTitle.rectTransform.sizeDelta=new Vector2(256,annualCompact?28:52);
            previewKind.rectTransform.anchoredPosition=new Vector2(18,annualCompact?-10:-16);
            previewTitle.fontSize=(annualCompact?19:22)*(LargeText?1.12f:1);
            previewBody.rectTransform.anchoredPosition=new Vector2(18,annualCompact?-67:-95);previewBody.rectTransform.sizeDelta=new Vector2(256,annualCompact?104:LargeText?152:120);
            previewBody.fontSize=(annualCompact?13:16)*(LargeText?1.12f:1);
            recipeDetails.GetComponent<RectTransform>().anchoredPosition=new Vector2(0,annualCompact?49:68);
            recipeDetails.GetComponent<RectTransform>().sizeDelta=new Vector2(256,annualCompact?28:34);
            previewAction.rectTransform.anchoredPosition=new Vector2(18,annualCompact?46:68);previewAction.fontSize=(annualCompact?13:14)*(LargeText?1.12f:1);
            foreach(var button in new[]{confirmEditButton,moveButton,removeButton})
            {var br=button.GetComponent<RectTransform>();br.anchoredPosition=new Vector2(br.anchoredPosition.x,annualCompact?8:16);br.sizeDelta=new Vector2(br.sizeDelta.x,annualCompact?34:48);}
        }
    }
}
