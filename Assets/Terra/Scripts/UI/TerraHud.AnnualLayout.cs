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
                Mathf.Min((dock.yMax+padding-canvas.y)/canvas.height,.48f),
                Mathf.Max((right.xMin-padding-canvas.x)/canvas.width,.55f),
                Mathf.Max((feedback.yMin-padding-canvas.y)/canvas.height,.52f));
        }
        // At 125% zoom on a 1280×720 display, keep primary controls outside the map preview.
        // Secondary actions share the catalog footer; prices remain in the selected-site panel.
        void LayoutAnnual(float width,float height,bool compact)
        {
            annualCompact=compact;actionSummary.gameObject.SetActive(!compact);
            ((RectTransform)actionDock).sizeDelta=new Vector2(760,compact?160:222);
            void Place(Button button,Transform parent,Vector2 anchor,Vector2 position,Vector2 size)
            {
                if(button==null)return;button.transform.SetParent(parent,false);
                var r=button.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=r.pivot=anchor;r.anchoredPosition=position;r.sizeDelta=size;
                foreach(var label in button.GetComponentsInChildren<TextMeshProUGUI>())label.rectTransform.sizeDelta=size-new Vector2(16,4);
            }
            var utilities=new[]{undoButton,redoButton,resetProposalsButton,viewButton,guideButton,controlsButton};
            for(int i=0;i<utilities.Length;i++)
            {
                var button=utilities[i];
                if(compact)Place(button,actionDock,new Vector2(0,0),new Vector2(17+i*121,6),new Vector2(116,30));
                else
                {
                    bool right=i>=4;int row=i==0?0:i==1?2:i==2?3:i==3?1:i-4;
                    Place(button,root,new Vector2(right?1:0,0),new Vector2(right?-24:24,20+row*64),new Vector2(right?148:130,56));
                    button.transform.SetSiblingIndex(modalShade.transform.GetSiblingIndex());
                }
                foreach(var label in button.GetComponentsInChildren<TextMeshProUGUI>())label.fontSize=(compact?14:16)*(LargeText?1.12f:1);
            }
            Place(endYearButton,compact?budgetPanel:root,new Vector2(compact?0:1,0),compact?new Vector2(16,8):new Vector2(-24,148),new Vector2(compact?134:148,compact?38:56));
            Place(reportButton,compact?budgetPanel:root,new Vector2(compact?0:1,0),compact?new Vector2(160,8):new Vector2(-24,212),new Vector2(compact?134:148,compact?38:56));
            foreach(var b in new[]{endYearButton,reportButton})if(!compact)b.transform.SetSiblingIndex(modalShade.transform.GetSiblingIndex());
            annualWater.gameObject.SetActive(!compact);
            Place(futureButton,root,compact?new Vector2(0,1):new Vector2(0,0),compact?new Vector2(24,-330):new Vector2(24,328),new Vector2(164,compact?38:44));
            Place(restartButton,root,compact?new Vector2(0,1):new Vector2(0,0),compact?new Vector2(200,-330):new Vector2(24,276),new Vector2(130,compact?38:44));
            foreach(var category in categoryButtons.Keys)
            {var r=category.GetComponent<RectTransform>();r.sizeDelta=new Vector2(240,compact?30:38);r.anchoredPosition=new Vector2(r.anchoredPosition.x,compact?-4:-8);}
            foreach(var row in actionRows.Values){var r=(RectTransform)row.transform;r.anchoredPosition=new Vector2(10,compact?-39:-53);r.sizeDelta=new Vector2(740,compact?82:124);}
            foreach(var button in actionButtons.Keys)
            {
                var r=button.GetComponent<RectTransform>();r.sizeDelta=new Vector2(162,compact?82:124);
                var icon=button.transform.Find("Model icon") as RectTransform;if(icon!=null)icon.sizeDelta=new Vector2(150,compact?38:62);
                foreach(var label in button.GetComponentsInChildren<TextMeshProUGUI>())
                {
                    bool count=label.rectTransform.anchorMin.y==0;label.gameObject.SetActive(!compact||!count);
                    if(!count&&label.rectTransform.anchorMin.y==1)label.rectTransform.anchoredPosition=new Vector2(0,compact?-40:-64);
                }
            }
            foreach(var b in catalogPageButtons)b.GetComponent<RectTransform>().sizeDelta=new Vector2(30,compact?82:124);
            LayoutAnnualPreview();
        }
        void LayoutAnnualPreview()
        {
            if(recipeDetails==null)return;
            var r=(RectTransform)tooltip.transform;r.sizeDelta=new Vector2(292,annualCompact?264:320);r.anchoredPosition=new Vector2(-24,annualCompact?-126:-132);
            previewTitle.rectTransform.anchoredPosition=new Vector2(18,annualCompact?-34:-45);previewTitle.rectTransform.sizeDelta=new Vector2(256,annualCompact?28:52);
            previewKind.rectTransform.anchoredPosition=new Vector2(18,annualCompact?-10:-16);
            previewTitle.fontSize=(annualCompact?19:22)*(LargeText?1.12f:1);
            previewBody.rectTransform.anchoredPosition=new Vector2(18,annualCompact?-67:-95);previewBody.rectTransform.sizeDelta=new Vector2(256,annualCompact?112:120);
            previewBody.fontSize=(annualCompact?14:16)*(LargeText?1.12f:1);
            recipeDetails.GetComponent<RectTransform>().anchoredPosition=new Vector2(0,annualCompact?49:68);
            recipeDetails.GetComponent<RectTransform>().sizeDelta=new Vector2(256,annualCompact?28:34);
            previewAction.rectTransform.anchoredPosition=new Vector2(18,annualCompact?46:68);previewAction.fontSize=(annualCompact?13:14)*(LargeText?1.12f:1);
            foreach(var button in new[]{confirmEditButton,moveButton,removeButton})
            {var br=button.GetComponent<RectTransform>();br.anchoredPosition=new Vector2(br.anchoredPosition.x,annualCompact?8:16);br.sizeDelta=new Vector2(br.sizeDelta.x,annualCompact?34:48);}
        }
    }
}
