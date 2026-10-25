using System;
using System.Linq;
using Terra.Core;
using Terra.Contracts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Terra.UI
{
    public sealed partial class TerraHud
    {
        Transform yearOverview,yearOverviewHeading,yearOverviewActions;
        TextMeshProUGUI yearOverviewTitle;
        readonly Button[] highlightButtons=new Button[3];
        readonly TextMeshProUGUI[] highlightTitles=new TextMeshProUGUI[3],highlightDetails=new TextMeshProUGUI[3];
        readonly HudGlyph[] highlightGlyphs=new HudGlyph[3];
        Button overviewNext,overviewStory,overviewLedger,selectedHighlight;
        Action overviewContinue,overviewReadStory,overviewReadLedger;
        public bool AnnualOverviewOpen=>yearOverview!=null&&yearOverview.gameObject.activeSelf;

        void CreateYearOverview()
        {
            yearOverview=Rect(root,"Year in the landscape",Vector2.zero,Vector2.zero,Vector2.zero);
            var rect=(RectTransform)yearOverview;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
            yearOverview.SetSiblingIndex(modalShade.transform.GetSiblingIndex());
            yearOverviewHeading=Panel(yearOverview,"Year complete",new Vector2(0,1),new Vector2(16,-176),new Vector2(344,54),PanelColor);
            yearOverviewTitle=Label(yearOverviewHeading,"",new Vector2(0,1),new Vector2(14,-8),new Vector2(316,28),22,Paper,FontStyles.Bold);
            Label(yearOverviewHeading,"Select a change to find it on the map",new Vector2(0,1),new Vector2(14,-34),new Vector2(316,18),12,Soft);
            for(int i=0;i<3;i++)
            {
                var b=highlightButtons[i]=Button(yearOverview,"",new Vector2(0,1),new Vector2(16,-238-i*86),new Vector2(344,80),()=>{},14);
                b.name="Year highlight "+(i+1);
                highlightGlyphs[i]=Glyph(b.transform,"check",new Vector2(0,.5f),new Vector2(12,0),24,Accent);
                highlightTitles[i]=Label(b.transform,"",new Vector2(0,1),new Vector2(48,-7),new Vector2(282,34),15,Paper,FontStyles.Bold);
                highlightDetails[i]=Label(b.transform,"",new Vector2(0,1),new Vector2(48,-43),new Vector2(282,32),12,Soft);
            }
            yearOverviewActions=Panel(yearOverview,"Year review actions",new Vector2(.5f,0),new Vector2(0,10),new Vector2(740,48),PanelColor);
            overviewStory=Button(yearOverviewActions,"Year story",Vector2.zero,new Vector2(6,6),new Vector2(172,36),()=>overviewReadStory?.Invoke(),15);
            overviewLedger=Button(yearOverviewActions,"Budget & meters",Vector2.zero,new Vector2(184,6),new Vector2(172,36),()=>overviewReadLedger?.Invoke(),15);
            overviewNext=Button(yearOverviewActions,"",Vector2.zero,new Vector2(362,6),new Vector2(372,36),()=>overviewContinue?.Invoke(),16);
            yearOverview.gameObject.SetActive(false);
        }
        public void AnnualOverview(AnnualTown town,Action continuePlanning,Action story,Action ledger,Action<string> focusSite)
        {
            if(yearOverview==null)CreateYearOverview();
            // Close the pending/chapter modal before making this nonmodal map view visible.
            modal.SetActive(false);modalShade.SetActive(false);tooltip.SetActive(false);moreMenu.gameObject.SetActive(false);
            yearOverview.gameObject.SetActive(true);catalogKeyboard=false;selectedHighlight=null;
            overviewContinue=continuePlanning;overviewReadStory=story;overviewReadLedger=ledger;
            overviewStory.gameObject.SetActive(story!=null);
            var history=town.History;var report=history.Last();
            var cards=AnnualHighlights.Create(annualRules,report,history.Length>1?history[history.Length-2]:null);
            yearOverviewTitle.text="Year "+report.year+" complete";
            SetButtonText(overviewNext,town.Phase==TownPhase.Completed?"See ten-year future →":town.Phase==TownPhase.Reviewing?"Plan year "+(town.Year+1)+" →":"Return to plan →");
            for(int i=0;i<highlightButtons.Length;i++)
            {
                var b=highlightButtons[i];b.gameObject.SetActive(i<cards.Length);if(i>=cards.Length)continue;
                var card=cards[i];highlightTitles[i].text=card.title;highlightDetails[i].text=card.detail;
                highlightGlyphs[i].symbol=card.icon;highlightGlyphs[i].SetVerticesDirty();
                b.onClick.RemoveAllListeners();b.onClick.AddListener(()=>{selectedHighlight=b;if(card.siteId!=null)focusSite?.Invoke(card.siteId);else ledger?.Invoke();Paint();});
            }
            lastScreen=Vector2.zero;Layout();Focus(overviewNext);
        }
        public void CloseAnnualOverview()
        {
            if(yearOverview==null)return;
            yearOverview.gameObject.SetActive(false);selectedHighlight=null;Focus(null);lastScreen=Vector2.zero;Layout();resume?.Invoke();
        }
        void LayoutYearOverview(float width,float height)
        {
            if(!AnnualOverviewOpen)return;
            actionDock.gameObject.SetActive(false);commandBar.gameObject.SetActive(false);moreMenu.gameObject.SetActive(false);
            // Three cards, keyboard reachable, leave the camera free in the remaining map area.
            yearOverviewActions.localScale=Vector3.one*Mathf.Min(1,(width-32)/740);
            int count=highlightButtons.Count(b=>b.gameObject.activeSelf);
            float cardWidth=Mathf.Min(344,(width-172-12*(count-1))/count),cardHeight=LargeText?96:84;
            float start=Mathf.Max(156,(width-(count*cardWidth+12*(count-1)))/2);
            for(int i=0;i<count;i++)
            {
                var r=highlightButtons[i].GetComponent<RectTransform>();r.anchorMin=r.anchorMax=r.pivot=Vector2.zero;r.anchoredPosition=new Vector2(start+i*(cardWidth+12),66);r.sizeDelta=new Vector2(cardWidth,cardHeight);
                highlightTitles[i].rectTransform.sizeDelta=new Vector2(cardWidth-62,LargeText?40:34);highlightTitles[i].fontSize=LargeText?16:15;
                highlightDetails[i].rectTransform.anchoredPosition=new Vector2(48,LargeText?-51:-43);highlightDetails[i].rectTransform.sizeDelta=new Vector2(cardWidth-62,36);highlightDetails[i].fontSize=LargeText?13:12;
            }
        }
        bool OverviewKeyboard(UnityEngine.InputSystem.Keyboard k)
        {
            if(!AnnualOverviewOpen||ModalOpen)return false;
            var available=buttons.Where(b=>b.gameObject.activeInHierarchy&&b.IsInteractable()&&b.transform.IsChildOf(yearOverview)).ToArray();
            if(k.tabKey.wasPressedThisFrame)
            {int i=Array.IndexOf(available,focus),step=k.shiftKey.isPressed?-1:1;Focus(available[(i+step+available.Length)%available.Length]);return true;}
            if((k.enterKey.wasPressedThisFrame||k.spaceKey.wasPressedThisFrame)&&focus!=null&&available.Contains(focus)){focus.onClick.Invoke();return true;}
            if(k.escapeKey.wasPressedThisFrame){CloseAnnualOverview();return true;}
            return false;
        }
    }
}
