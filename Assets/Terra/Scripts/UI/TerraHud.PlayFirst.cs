using System;
using System.Linq;
using Terra.Contracts;
using Terra.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Terra.UI
{
    public sealed partial class TerraHud
    {
        Transform projectFacts,moreMenu;
        Image projectArt;
        TextMeshProUGUI projectCost,projectUpkeep,projectBenefit,projectTradeoff,projectTiming,projectReserve;
        HudGlyph benefitGlyph,tradeoffGlyph,timingGlyph,upkeepGlyph;
        Button moreButton,budgetButton;
        bool projectFactsVisible;float annualLayoutWidth;

        HudGlyph Glyph(Transform parent,string symbol,Vector2 anchor,Vector2 position,float size,Color color)
        {
            var glyph=Rect(parent,"Icon "+symbol,anchor,position,new Vector2(size,size)).gameObject.AddComponent<HudGlyph>();
            glyph.symbol=symbol;glyph.color=color;glyph.raycastTarget=false;return glyph;
        }
        void ButtonGlyph(Button button,string symbol)
        {
            Glyph(button.transform,symbol,new Vector2(0,.5f),new Vector2(10,0),19,Paper);
            var text=button.GetComponentInChildren<TextMeshProUGUI>();text.rectTransform.anchoredPosition=new Vector2(12,0);
        }
        void ConfigurePlayFirst()
        {
            moreMenu=Panel(root,"More planning tools",Vector2.zero,new Vector2(16,62),new Vector2(250,202),PanelColor);
            moreMenu.SetSiblingIndex(modalShade.transform.GetSiblingIndex());moreMenu.gameObject.SetActive(false);
            resetProposalsButton.onClick.AddListener(()=>moreMenu.gameObject.SetActive(false));
            moreButton=Button(commandBar,"More",Vector2.zero,Vector2.zero,new Vector2(94,36),()=>{catalogKeyboard=false;moreMenu.gameObject.SetActive(!moreMenu.gameObject.activeSelf);Focus(moreMenu.gameObject.activeSelf?new[]{reportButton,futureButton,resetProposalsButton,restartButton}.FirstOrDefault(b=>b!=null&&b.gameObject.activeInHierarchy&&b.IsInteractable()):moreButton);},14);
            foreach(var pair in new[]{(undoButton,"undo"),(redoButton,"redo"),(viewButton,"map"),(guideButton,"guide"),(controlsButton,"settings"),(moreButton,"more")})ButtonGlyph(pair.Item1,pair.Item2);
            budgetButton=Button(budgetPanel,"Budget",new Vector2(1,1),new Vector2(-10,-8),new Vector2(82,24),ShowPublicBudget,14);
            Glyph(budgetPanel,"coin",new Vector2(0,1),new Vector2(14,-36),24,Accent);
            for(int i=0;i<3;i++)Glyph(annualIndicators,new[]{"work","people","leaf"}[i],new Vector2(0,1),new Vector2(12+i*160,-11),18,new[]{new Color(.94f,.76f,.43f),new Color(.50f,.76f,.93f),Accent}[i]);
            projectFacts=Rect(tooltip.transform,"Project at a glance",new Vector2(0,1),Vector2.zero,new Vector2(304,340));
            projectArt=Rect(projectFacts,"Project miniature",new Vector2(0,1),new Vector2(16,-64),new Vector2(90,54)).gameObject.AddComponent<Image>();projectArt.preserveAspect=true;projectArt.raycastTarget=false;
            projectCost=Label(projectFacts,"",new Vector2(0,1),new Vector2(120,-66),new Vector2(166,52),18,Paper,FontStyles.Bold);
            TextMeshProUGUI Row(string symbol,float y,out HudGlyph glyph)
            {
                glyph=Glyph(projectFacts,symbol,new Vector2(0,1),new Vector2(18,-y),20,Accent);
                return Label(projectFacts,"",new Vector2(0,1),new Vector2(48,-y),new Vector2(238,44),14,Paper);
            }
            projectUpkeep=Row("coin",126,out upkeepGlyph);
            projectBenefit=Row("leaf",180,out benefitGlyph);
            projectTradeoff=Row("warning",218,out tradeoffGlyph);
            projectTiming=Row("calendar",256,out timingGlyph);
            projectReserve=Label(projectFacts,"",new Vector2(0,1),new Vector2(18,-286),new Vector2(268,24),14,Soft);
            projectFacts.gameObject.SetActive(false);
            ConfigurePlacementFeedback();ConfigureProjectStats();
        }
        void UpdateProjectFacts(Site site,TownActionDefinition recipe,AnnualTownSnapshot state,string mode,string rejection,bool queued)
            => UpdateVisualProjectStats(site,recipe,state,mode,rejection,queued);
        void LayoutPlayFirstInspector() => LayoutVisualProjectStats();
        void ReflowPlanningDock()
        {
            if(annualRules==null)return;
            float width=annualLayoutWidth>0?annualLayoutWidth:Screen.width/PixelRatio;
            // The native World atlas control owns the bottom-left 146px. Keep it clear too.
            // Reserve the inspector lane in every state. Hover must never move a tool.
            float left=156,right=width-336;
            int columns=right-left<688?3:4;
            if(columns!=catalogColumns)
            {
                int first=catalogPage*catalogColumns;catalogColumns=columns;
                var ids=actionButtons.Where(p=>AnnualCategory(p.Value)==activeCategory).Select(p=>p.Value).ToArray();
                int selected=Array.IndexOf(ids,activeTool);catalogPage=(selected>=0?selected:first)/columns;
                catalogPage=Mathf.Clamp(catalogPage,0,Mathf.Max(0,(ids.Length-1)/columns));ApplyCatalogPage();
            }
            float dockWidth=columns==3?520:688,cardHeight=annualCompact?96:110;
            float scale=Mathf.Min(1,(right-left)/dockWidth);
            actionDock.localScale=Vector3.one*scale;
            void Position(Transform t,Vector2 anchor,Vector2 position,Vector2 size)
            {var r=(RectTransform)t;r.anchorMin=r.anchorMax=r.pivot=anchor;r.anchoredPosition=position;r.sizeDelta=size;}
            float center=(left+right)/2;
            // Reserve a separate instruction line below the cards, including larger text.
            Position(actionDock,new Vector2(.5f,0),new Vector2(center-width/2,74),new Vector2(dockWidth,annualCompact?184:198));
            actionSummary.rectTransform.sizeDelta=new Vector2(dockWidth-28,20);
            int ci=0;float tabWidth=(dockWidth-72)/3;
            foreach(var category in categoryButtons.Keys)
            {
                Position(category.transform,new Vector2(0,1),new Vector2(8+ci++*(tabWidth+4),-6),new Vector2(tabWidth,44));
                category.GetComponentInChildren<TextMeshProUGUI>().rectTransform.sizeDelta=new Vector2(tabWidth-8,40);
            }
            foreach(var group in actionRows.Keys)
            {
                var row=actionRows[group];Position(row.transform,new Vector2(0,1),new Vector2(8,-56),new Vector2(dockWidth-16,cardHeight));
                var cards=actionButtons.Where(p=>AnnualCategory(p.Value)==group).ToArray();
                for(int i=0;i<cards.Length;i++)
                {
                    var button=cards[i].Key;float w=(dockWidth-112-(columns-1)*6)/columns;
                    Position(button.transform,new Vector2(0,1),new Vector2(48+i%columns*(w+6),0),new Vector2(w,cardHeight));
                    var art=button.transform.Find("Model icon");if(art!=null)Position(art,new Vector2(0,1),new Vector2(5,-3),new Vector2(columns==3?68:76,annualCompact?42:58));
                    var title=button.GetComponentsInChildren<TextMeshProUGUI>(true).First(t=>t.name=="Action title");title.rectTransform.anchoredPosition=new Vector2(0,annualCompact?-50:-64);title.rectTransform.sizeDelta=new Vector2(w-6,38);title.fontSize=LargeText?15.5f:14;
                }
                foreach(var pageButton in catalogPageButtons.Where(b=>b.transform.parent==row.transform))
                {pageButton.GetComponent<RectTransform>().sizeDelta=new Vector2(44,cardHeight);pageButton.GetComponentInChildren<TextMeshProUGUI>().rectTransform.sizeDelta=new Vector2(40,cardHeight-4);pageButton.gameObject.SetActive(cards.Length>columns);}
            }
        }
    }
}
