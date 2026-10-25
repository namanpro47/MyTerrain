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
            ConfigurePlacementFeedback();
        }
        void UpdateProjectFacts(Site site,TownActionDefinition recipe,AnnualTownSnapshot state,string mode,string rejection,bool queued)
        {
            projectFactsVisible=recipe!=null&&site!=null&&mode=="place"&&(rejection==null||queued)&&!state.assets.Any(a=>a.siteId==site.id);
            projectFacts.gameObject.SetActive(projectFactsVisible);previewBody.gameObject.SetActive(!projectFactsVisible);
            if(!projectFactsVisible)return;
            bool replacing=queued&&rejection==null&&state.queue.Any(q=>q.siteId==site.id&&q.actionId!=recipe.actionId);
            previewTitle.text=recipe.title;previewKind.text=(replacing?"REPLACE Y"+state.year:queued?"QUEUED":"PLAN Y"+state.year)+" · "+site.name;
            var card=actionButtons.FirstOrDefault(p=>p.Value==recipe.actionId).Key;
            projectArt.sprite=card?.transform.Find("Model icon")?.GetComponent<Image>().sprite;projectArt.enabled=projectArt.sprite!=null;
            projectCost.text=Money(recipe.upfrontPublicCostCents)+"\n<size=80%>Upfront · Opens Y"+(state.year+recipe.constructionYears)+"</size>";
            projectUpkeep.text=Money(recipe.annualPublicOperatingCents)+"/yr upkeep · "+recipe.operatingYears+" yr";
            if(recipe.annualPublicPaymentCents>0)projectUpkeep.text+="\n"+Money(recipe.annualPublicPaymentCents)+"/yr program · "+recipe.paymentYears+" yr";
            var s=recipe.services;string benefit,icon;
            if(recipe.actionId=="stewardship-agreement"){benefit=(recipe.vegetatedFraction*100).ToString("0")+"% target vegetation";icon="leaf";}
            else if(s!=null&&(recipe.actionId=="data-center"||recipe.actionId=="agricultural-storage")){benefit=s.jobs.ToString("0.#")+" jobs at capacity";icon="work";}
            else if(s!=null&&s.homes>0){benefit=s.homes.ToString("0.#")+" home capacity";icon="home";}
            else if(s!=null&&s.cleanEnergySupply>0){benefit=s.cleanEnergySupply.ToString("0.#")+" clean energy units";icon="energy";}
            else if(s!=null&&s.energySupply>0){benefit=s.energySupply.ToString("0.#")+" energy units";icon="energy";}
            else if(s!=null&&s.wastewaterCapacity>0){benefit=s.wastewaterCapacity.ToString("0.#")+" treatment units";icon="water";}
            else if(s!=null&&s.recreation>0){benefit=s.recreation.ToString("0.#")+" recreation units";icon="people";}
            else if(s!=null&&s.localServices>0){benefit=s.localServices.ToString("0.#")+" local service units";icon="people";}
            else if(recipe.retentionCapacityM3>0){benefit=recipe.retentionCapacityM3.ToString("0.#")+" m³ retention capacity";icon="water";}
            else if(recipe.productionUnits>0){benefit=recipe.productionUnits.ToString("0.#")+" production capacity units";icon="work";}
            else{benefit=(recipe.vegetatedFraction*100).ToString("0")+"% target vegetation";icon="leaf";}
            projectBenefit.text=PlacementPurpose(recipe.actionId);benefitGlyph.symbol=icon;benefitGlyph.SetVerticesDirty();
            if(s!=null&&(s.energyDemand>0||s.waterDemand>0))
            {projectTradeoff.text=s.energyDemand>0&&s.waterDemand>0?"Needs energy and water; adds upkeep":s.energyDemand>0?"Needs energy; adds upkeep":"Needs water; adds upkeep";tradeoffGlyph.symbol="energy";}
            else
            {
                var baseline=annualRules.sites.First(a=>a.id==site.id);
                double loss=baseline.baselineProductionUnits-recipe.productionUnits;
                projectTradeoff.text=loss>0?"Replaces productive land; adds upkeep":"Reserves this field for the run";tradeoffGlyph.symbol="warning";
            }
            tradeoffGlyph.SetVerticesDirty();
            projectTiming.text="Opens Y"+(state.year+recipe.constructionYears)+" · grows for "+recipe.establishmentYears+" yr";
            long reserve=recipe.upfrontPublicCostCents+recipe.annualPublicOperatingCents*recipe.operatingYears+recipe.annualPublicPaymentCents*recipe.paymentYears;
            projectReserve.text=Money(reserve)+" total reserved";
            recipeDetails.gameObject.SetActive(recipe.services!=null);SetButtonText(recipeDetails,"Details & assumptions");previewAction.gameObject.SetActive(false);
        }
        void LayoutPlayFirstInspector()
        {
            if(!projectFactsVisible)return;
            float paymentSpace=inspectedRecipe?.annualPublicPaymentCents>0?0:20;
            ((RectTransform)tooltip.transform).sizeDelta=new Vector2(304,400-paymentSpace);
            projectBenefit.rectTransform.anchoredPosition=new Vector2(48,-180+paymentSpace);benefitGlyph.rectTransform.anchoredPosition=new Vector2(18,-180+paymentSpace);
            projectTradeoff.rectTransform.anchoredPosition=new Vector2(48,-218+paymentSpace);tradeoffGlyph.rectTransform.anchoredPosition=new Vector2(18,-218+paymentSpace);
            projectTiming.rectTransform.anchoredPosition=new Vector2(48,-256+paymentSpace);timingGlyph.rectTransform.anchoredPosition=new Vector2(18,-256+paymentSpace);
            projectReserve.rectTransform.anchoredPosition=new Vector2(18,-286+paymentSpace);
            previewTitle.rectTransform.anchoredPosition=new Vector2(18,-34);previewTitle.rectTransform.sizeDelta=new Vector2(268,28);
            previewKind.rectTransform.anchoredPosition=new Vector2(18,-12);previewKind.rectTransform.sizeDelta=new Vector2(268,18);
            previewKind.fontSize=14;previewKind.enableAutoSizing=false;previewKind.textWrappingMode=TextWrappingModes.NoWrap;
            foreach(var text in new[]{projectUpkeep,projectBenefit,projectTradeoff,projectTiming,projectReserve})text.fontSize=LargeText?15.5f:14;
            foreach(var text in new[]{projectBenefit,projectTradeoff})text.rectTransform.sizeDelta=new Vector2(238,38);
            recipeDetails.GetComponent<RectTransform>().anchoredPosition=new Vector2(0,53);recipeDetails.GetComponent<RectTransform>().sizeDelta=new Vector2(268,28);
            foreach(var button in new[]{confirmEditButton,moveButton,removeButton})
            {var rect=button.GetComponent<RectTransform>();rect.anchoredPosition=new Vector2(rect.anchoredPosition.x,10);rect.sizeDelta=new Vector2(rect.sizeDelta.x,36);}
            LayoutPlacementFacts();
        }
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
