using System;
using System.Collections.Generic;
using System.Globalization;
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
        sealed class ProjectStatRow
        {
            public RectTransform root,track;
            public Image fill,zero;
            public Image[] ticks;
            public TextMeshProUGUI label,value;
            public HudGlyph icon;
            public double amount;
            public bool signed;
        }
        ProjectStatRow[] projectStatRows;
        TextMeshProUGUI projectEffectHeading,projectCapacity;
        bool projectStatsAccepted,projectStatsQueued,projectStatsCatalog;
        float annualLayoutHeight=720;
        string projectPreviewRevision;
        readonly Dictionary<string,AnnualProjectPreview> projectPreviewCache=new Dictionary<string,AnnualProjectPreview>();
        readonly Color projectCostColor=new Color(.94f,.76f,.43f);
        readonly Color projectSocialColor=new Color(.50f,.76f,.93f);
        readonly Color projectLossColor=new Color(1,.56f,.43f);
        Transform townNeeds;
        readonly Image[] townNeedBars=new Image[3];
        readonly TextMeshProUGUI[] townNeedValues=new TextMeshProUGUI[3];
        static string StatNumber(double n)=>n.ToString("#,0.#",CultureInfo.InvariantCulture);
        static string StatMoney(long cents)
        {
            decimal dollars=cents/100m;
            return "$"+(dollars>=1000000?(dollars/1000000m).ToString("0.##",CultureInfo.InvariantCulture)+"M":
                dollars>=1000?(dollars/1000m).ToString("0.##",CultureInfo.InvariantCulture)+"K":dollars.ToString("0.##",CultureInfo.InvariantCulture));
        }

        void ConfigureProjectStats()
        {
            projectPreviewCache.Clear();projectPreviewRevision=null;
            projectStatRows=new ProjectStatRow[7];
            for(int i=0;i<projectStatRows.Length;i++)
            {
                var row=new ProjectStatRow();projectStatRows[i]=row;
                row.root=Rect(projectFacts,"Project stat "+i,new Vector2(0,1),Vector2.zero,new Vector2(304,24));
                row.icon=Glyph(row.root,"coin",new Vector2(0,.5f),new Vector2(14,0),15,Soft);
                row.label=Label(row.root,"",new Vector2(0,.5f),new Vector2(35,0),new Vector2(77,24),13,Soft);
                row.track=(RectTransform)Panel(row.root,"Amount track",new Vector2(0,.5f),new Vector2(114,0),new Vector2(86,8),Raised);
                row.fill=Panel(row.track,"Amount",new Vector2(0,.5f),Vector2.zero,new Vector2(0,8),Accent).GetComponent<Image>();
                row.zero=Panel(row.track,"Zero",new Vector2(0,.5f),new Vector2(43,0),new Vector2(1,12),Soft).GetComponent<Image>();
                row.ticks=new Image[9];
                for(int t=0;t<9;t++)row.ticks[t]=Panel(row.track,"Year tick "+t,new Vector2(0,.5f),new Vector2((t+1)*8.6f,0),new Vector2(2,8),PanelColor).GetComponent<Image>();
                row.value=Label(row.root,"",new Vector2(1,.5f),new Vector2(-14,0),new Vector2(86,24),14,Paper,FontStyles.Bold);
                row.value.alignment=TextAlignmentOptions.Right;row.value.textWrappingMode=TextWrappingModes.NoWrap;
                row.label.textWrappingMode=TextWrappingModes.NoWrap;
            }
            projectEffectHeading=Label(projectFacts,"",new Vector2(0,1),Vector2.zero,new Vector2(276,20),13,Soft,FontStyles.Bold);
            projectCapacity=Label(projectFacts,"",new Vector2(0,1),Vector2.zero,new Vector2(276,20),13,Paper);
            townNeeds=Panel(budgetPanel,"Town needs",new Vector2(0,1),new Vector2(0,-112),new Vector2(310,106),PanelColor);
            Label(townNeeds,"TOWN GOALS · CURRENT / TARGET",new Vector2(0,1),new Vector2(14,-9),new Vector2(280,18),12,Soft,FontStyles.Bold);
            for(int i=0;i<3;i++)
            {
                Color goalColor=i==0?projectSocialColor:i==1?projectCostColor:Accent;
                Glyph(townNeeds,new[]{"home","work","leaf"}[i],new Vector2(0,1),new Vector2(14,-32-i*23),15,goalColor);
                Label(townNeeds,new[]{"Homes filled","Local jobs","Green land"}[i],new Vector2(0,1),new Vector2(36,-30-i*23),new Vector2(84,20),13,Soft);
                var track=Panel(townNeeds,"Need track "+i,new Vector2(0,1),new Vector2(122,-36-i*23),new Vector2(82,7),Raised);
                townNeedBars[i]=Panel(track,"Delivered",new Vector2(0,1),Vector2.zero,new Vector2(0,7),goalColor).GetComponent<Image>();
                townNeedValues[i]=Label(townNeeds,"",new Vector2(1,1),new Vector2(-14,-30-i*23),new Vector2(90,20),13,Paper,FontStyles.Bold);
                townNeedValues[i].alignment=TextAlignmentOptions.Right;townNeedValues[i].textWrappingMode=TextWrappingModes.NoWrap;townNeedValues[i].enableAutoSizing=true;townNeedValues[i].fontSizeMin=11.5f;townNeedValues[i].fontSizeMax=13;
            }
        }

        void UpdateTownNeeds(TownYearOutcomes current)
        {
            if(townNeeds==null)return;
            bool show=current?.economy!=null&&annualRules?.economy!=null;
            townNeeds.gameObject.SetActive(show);if(!show)return;
            var e=annualRules.economy;
            double[] value={current.economy.occupiedHomes,current.economy.jobs,current.managedAreaM2==0?0:100*current.vegetatedAreaM2/current.managedAreaM2};
            double[] target={e.householdDemand,e.jobsTarget,e.coverTargetFraction*100};
            for(int i=0;i<3;i++)
            {
                townNeedBars[i].rectTransform.sizeDelta=new Vector2(target[i]<=0?0:(float)Math.Min(1,value[i]/target[i])*82,7);
                townNeedValues[i].text=StatNumber(value[i])+" / "+StatNumber(target[i])+(i==2?"%":"");
            }
        }

        void SetProjectStat(int index,string label,string icon,double fraction,string value,Color color,bool signed=false,bool years=false,int yearSteps=10)
        {
            var row=projectStatRows[index];row.root.gameObject.SetActive(true);row.label.text=label;row.value.text=value;
            row.icon.symbol=icon;row.icon.color=color;row.icon.SetVerticesDirty();row.signed=signed;row.amount=fraction;
            row.zero.gameObject.SetActive(signed);
            for(int i=0;i<row.ticks.Length;i++)
            {
                row.ticks[i].gameObject.SetActive(years&&i<yearSteps-1);
                row.ticks[i].rectTransform.anchoredPosition=new Vector2((i+1)*86f/Math.Max(1,yearSteps),0);
            }
            float width=(float)Math.Min(1,Math.Abs(fraction))*(signed?43:86);
            row.fill.rectTransform.anchoredPosition=new Vector2(signed?(fraction<0?43-width:43):0,0);
            row.fill.rectTransform.sizeDelta=new Vector2(width,8);
            row.fill.color=color;row.value.color=signed&&Math.Abs(fraction)>.00001?color:Paper;
        }

        void UpdateVisualProjectStats(Site site,TownActionDefinition recipe,AnnualTownSnapshot state,string mode,string rejection,bool queued)
        {
            var asset=state.assets.FirstOrDefault(a=>a.siteId==site?.id&&a.cancelledYear==0);
            var proposal=state.queue.FirstOrDefault(q=>q.siteId==site?.id);
            projectStatsAccepted=asset!=null&&string.IsNullOrEmpty(activeTool)&&(proposal==null||proposal.actionId==AnnualPlayerRules.CancelAction);
            projectStatsCatalog=site==null&&mode=="place"&&!string.IsNullOrEmpty(activeTool)&&state.phase==TownPhase.Planning;
            if(projectStatsAccepted)recipe=annualRules.actions.First(a=>a.siteId==asset.siteId&&a.actionId==asset.actionId);
            else if(recipe==null&&proposal!=null&&proposal.actionId!=AnnualPlayerRules.CancelAction&&string.IsNullOrEmpty(activeTool))
                recipe=annualRules.actions.First(a=>a.siteId==proposal.siteId&&a.actionId==proposal.actionId);
            else if(projectStatsCatalog)recipe=annualRules.actions.FirstOrDefault(a=>a.actionId==activeTool);
            projectStatsQueued=recipe!=null&&proposal?.actionId==recipe.actionId&&(string.IsNullOrEmpty(activeTool)||activeTool==recipe.actionId);
            projectFactsVisible=recipe?.services!=null&&recipe.actionId!=AnnualPlayerRules.CancelAction&&mode=="place"
                &&(projectStatsAccepted||projectStatsCatalog||(rejection==null||projectStatsQueued));
            projectFacts.gameObject.SetActive(projectFactsVisible);previewBody.gameObject.SetActive(!projectFactsVisible);
            if(!projectFactsVisible)return;
            inspectedRecipe=recipe;tooltip.SetActive(true);
            var outcome=state.history.LastOrDefault()?.outcomes.sites.FirstOrDefault(s=>s.siteId==site?.id);
            bool replacing=!projectStatsAccepted&&proposal!=null&&proposal.actionId!=recipe.actionId;
            previewTitle.text=recipe.title;
            previewKind.text=projectStatsAccepted?(outcome?.phase.ToString().ToUpperInvariant()??"ACCEPTED")+" · "+site.name:
                projectStatsCatalog?"CHOOSE A FIELD":(replacing?"REPLACE Y"+state.year:projectStatsQueued?"QUEUED":"PLAN Y"+state.year)+" · "+site?.name;
            projectBenefit.text=ProjectScorePurpose(recipe.actionId);
            var card=actionButtons.FirstOrDefault(p=>p.Value==recipe.actionId).Key;
            projectArt.sprite=card?.transform.Find("Model icon")?.GetComponent<Image>().sprite;projectArt.enabled=projectArt.sprite!=null;
            bool converting=!projectStatsAccepted&&asset!=null&&asset.actionId!=recipe.actionId;
            long upfront=recipe.upfrontPublicCostCents+(converting?(annualTown?.ConversionCost(site.id)??0):0);
            int construction=recipe.constructionYears+(projectStatsAccepted?asset.conversionDelayYears:converting?Math.Max(0,1-recipe.constructionYears):0);
            long maxCost=annualRules.actions.Max(a=>a.upfrontPublicCostCents),maxUpkeep=annualRules.actions.Max(a=>a.annualPublicOperatingCents);
            SetProjectStat(0,projectStatsAccepted?"Base upfront":"Upfront","coin",maxCost==0?0:(double)upfront/maxCost,StatMoney(upfront),projectCostColor);
            SetProjectStat(1,"Yearly cost","coin",maxUpkeep==0?0:(double)recipe.annualPublicOperatingCents/maxUpkeep,StatMoney(recipe.annualPublicOperatingCents),projectCostColor);
            int start=projectStatsAccepted?asset.startedYear:state.year,opens=start+construction;
            int full=opens+recipe.establishmentYears-1;
            if(projectStatsAccepted)
            {
                int done=AnnualProjectPreview.CompletedConstruction(recipe,asset,state);
                if(done==construction&&outcome?.phase==TownAssetPhase.Building)previewKind.text="BUILT · OPENS Y"+opens+" · "+site.name;
                SetProjectStat(2,"Built","calendar",construction==0?1:(double)done/construction,construction==0?"Complete":done+"/"+construction+" yr",Accent,false,true,construction);
                double maturity=outcome?.maturity??0;
                double working=outcome!=null&&(outcome.phase==TownAssetPhase.Operating||outcome.phase==TownAssetPhase.Establishing)?maturity*outcome.condition:0;
                SetProjectStat(3,"Working at","leaf",working,(working*100).ToString("0",CultureInfo.InvariantCulture)+"%",Accent);
            }
            else
            {
                SetProjectStat(2,"Build time","calendar",construction/10.0,construction==0?"This year":construction+" yr",projectCostColor,false,true);
                SetProjectStat(3,"Full benefit","leaf",(construction+recipe.establishmentYears-1)/10.0,full==state.year?"This year":"Y"+full,full>annualRules.years?projectLossColor:Accent,false,true);
            }
            var capacities=ProjectCapacities(recipe);
            projectCapacity.text=ProjectConcreteBenefit(recipe)+" · Opens Y"+opens;
            if(projectStatsCatalog)projectCapacity.text="Select an outlined field for its impact";
            if(opens>annualRules.years)projectCapacity.text="Opens Y"+opens+" · after this run";
            else if(full>annualRules.years)projectCapacity.text="Full effect Y"+full+" · after this run";
            if(projectStatsAccepted)
            {
                projectEffectHeading.text="CURRENT BENEFITS · Y"+(state.history.LastOrDefault()?.year??state.year);
                double active=outcome!=null&&(outcome.phase==TownAssetPhase.Operating||outcome.phase==TownAssetPhase.Establishing)?outcome.maturity*outcome.condition:0;
                for(int i=0;i<3;i++)
                {
                    if(i>=capacities.Count){projectStatRows[4+i].root.gameObject.SetActive(false);continue;}
                    var c=capacities[i];double fraction=active;
                    if(c.label=="Green land")fraction=(outcome?.vegetatedAreaM2??0)/annualRules.sites.First(s=>s.id==site.id).areaM2;
                    if(c.label=="Env. cost")fraction=1;
                    double value=c.maximum*fraction;
                    string number=c.label=="Green land"?StatNumber(fraction*100)+"%":c.label=="Env. cost"?"−"+StatNumber(c.maximum)+" pts":StatNumber(value)+" / "+StatNumber(c.maximum)+c.unit;
                    SetProjectStat(4+i,c.label,c.icon,c.label=="Env. cost"?c.maximum/30:fraction,number,c.label=="Env. cost"?projectLossColor:c.label=="Local jobs"?projectCostColor:c.label=="Green land"||c.label=="Rain storage"?Accent:projectSocialColor);
                }
            }
            else
            {
                AnnualProjectPreview estimate=null;
                if(!projectStatsCatalog&&site!=null)
                {
                    string revision=state.runId+"/"+state.definitionHash+"/"+state.revision+"/"+state.phase;
                    if(projectPreviewRevision!=revision){projectPreviewCache.Clear();projectPreviewRevision=revision;}
                    string key=site.id+"/"+recipe.actionId;
                    if(!projectPreviewCache.TryGetValue(key,out estimate)){estimate=AnnualProjectPreview.Create(annualRules,state,recipe);projectPreviewCache[key]=estimate;}
                }
                projectEffectHeading.text=estimate?.available==true?"SCORE IMPACT · Y"+estimate.year+(estimate.beyondRun?" · PARTIAL":" · ESTIMATE"):"SCORE IMPACT · CHOOSE FIELD";
                if(!projectStatsCatalog&&estimate?.available!=true)projectEffectHeading.text="SCORE IMPACT · UNAVAILABLE";
                double[] delta={estimate?.economic??0,estimate?.social??0,estimate?.environmental??0};
                for(int i=0;i<3;i++)SetProjectStat(4+i,new[]{"Economic","Social","Environment"}[i],new[]{"work","people","leaf"}[i],delta[i]/25,
                    estimate?.available==true?(Math.Abs(delta[i])<.05?0:delta[i]).ToString("+0.0;-0.0;0.0",CultureInfo.InvariantCulture)+" pts":"—",delta[i]<0?projectLossColor:i==0?projectCostColor:i==1?projectSocialColor:Accent,true);
            }
            long reserve=upfront+recipe.annualPublicOperatingCents*recipe.operatingYears+recipe.annualPublicPaymentCents*recipe.paymentYears;
            projectReserve.text=projectStatsAccepted?"Upkeep term · "+recipe.operatingYears+" operating years":Money(reserve)+" lifetime public cost";
            if(recipe.annualPublicPaymentCents>0)projectReserve.text+="\n"+StatMoney(recipe.annualPublicPaymentCents)+"/yr program · "+recipe.paymentYears+" yr";
            else if(converting)projectReserve.text+="\nIncludes "+StatMoney(annualTown?.ConversionCost(site.id)??0)+" change cost";
            else if(!projectStatsAccepted)projectReserve.text+="\nIncludes "+recipe.operatingYears+" yr upkeep";
            recipeDetails.gameObject.SetActive(true);SetButtonText(recipeDetails,"Details & assumptions");previewAction.gameObject.SetActive(false);
            if(projectStatsCatalog)confirmEditButton.gameObject.SetActive(false);
        }

        sealed class ProjectCapacity
        {
            public string label,icon,unit;
            public double maximum;
        }
        List<ProjectCapacity> ProjectCapacities(TownActionDefinition recipe)
        {
            var result=new List<ProjectCapacity>();var s=recipe.services;
            void Add(string label,string icon,double value,string unit="")
            {if(value>0)result.Add(new ProjectCapacity{label=label,icon=icon,maximum=value,unit=unit});}
            Add("Homes ready","home",s.homes);
            Add("Rain storage","water",recipe.retentionCapacityM3," m³");
            Add("Local jobs","work",s.jobs);
            if(AnnualPlayerRules.Enabled(annualRules))Add("Env. cost","leaf",AnnualPlayerRules.IndustrialPressure(recipe.actionId));
            // The green-land bar is the share of this field covered by vegetation, not an abstract output unit.
            Add("Green land","leaf",1);return result;
        }
        static string ProjectScorePurpose(string action)
        {
            switch(action)
            {
                case "cultivate-standard":case "conservation-rotation":case "cultivate-margin":case "cover-crop":return "Food & farm jobs → Economic";
                case "data-center":case "agricultural-storage":return "Jobs & business → Economic";
                case "low-density-housing":case "high-density-housing":return "Housing → Social";
                case "public-park":return "Recreation → Social";
                case "village-center":return "Shops → Social · Jobs → Economic";
                case "restore-grass":case "stewardship-agreement":return "Greener land → Environment";
                case "water-swale":return "Less runoff → Environment";
                case "non-renewable-power":return "Electricity → Social";
                case "solar":case "wind":return "Clean power → Social + Environment";
                case "wastewater-upgrade":return "Sanitation → Social + Environment";
                default:return PlacementPurpose(action);
            }
        }
        static string ProjectConcreteBenefit(TownActionDefinition recipe)
        {
            var s=recipe.services;
            if(s.homes>0)return StatNumber(s.homes)+(s.homes==1?" home":" homes");
            switch(recipe.actionId)
            {
                case "cultivate-standard":case "conservation-rotation":case "cultivate-margin":case "cover-crop":return "Food harvest";
                case "public-park":return "Recreation space";
                case "restore-grass":case "stewardship-agreement":return "Grasses & plants";
                case "water-swale":return "Stores rainwater";
                case "solar":case "wind":return "Clean electricity";
                case "non-renewable-power":return "Fuel-powered electricity";
                case "wastewater-upgrade":return "Cleans wastewater";
                default:return StatNumber(s.jobs)+" local jobs";
            }
        }

        void LayoutVisualProjectStats()
        {
            if(!projectFactsVisible||projectStatRows==null)return;
            bool compact=annualCompact||annualLayoutHeight<680;
            float height=compact?390:426,step=compact?22:26,start=compact?76:84;
            ((RectTransform)tooltip.transform).sizeDelta=new Vector2(304,height);
            ((RectTransform)tooltip.transform).anchoredPosition=new Vector2(-16,-Mathf.Min(114,annualLayoutHeight-height-72));
            previewTitle.rectTransform.anchoredPosition=new Vector2(14,-29);previewTitle.rectTransform.sizeDelta=new Vector2(234,27);previewTitle.fontSize=LargeText?21:20;previewTitle.enableAutoSizing=true;previewTitle.fontSizeMin=14;previewTitle.fontSizeMax=previewTitle.fontSize;previewTitle.textWrappingMode=TextWrappingModes.NoWrap;
            previewKind.rectTransform.anchoredPosition=new Vector2(14,-8);previewKind.rectTransform.sizeDelta=new Vector2(276,18);previewKind.fontSize=12.5f;previewKind.enableAutoSizing=true;previewKind.fontSizeMin=11;previewKind.fontSizeMax=13;
            projectArt.rectTransform.anchoredPosition=new Vector2(250,-28);projectArt.rectTransform.sizeDelta=new Vector2(40,40);
            projectBenefit.rectTransform.anchoredPosition=new Vector2(14,-55);projectBenefit.rectTransform.sizeDelta=new Vector2(234,20);projectBenefit.fontSize=LargeText?14:13;projectBenefit.enableAutoSizing=true;projectBenefit.fontSizeMin=11.5f;projectBenefit.fontSizeMax=projectBenefit.fontSize;projectBenefit.textWrappingMode=TextWrappingModes.NoWrap;
            foreach(var t in new[]{projectCost,projectUpkeep,projectTradeoff,projectTiming,placementSpace})if(t!=null)t.gameObject.SetActive(false);
            foreach(var g in new[]{upkeepGlyph,benefitGlyph,tradeoffGlyph,timingGlyph})g.gameObject.SetActive(false);
            for(int i=0;i<7;i++)
            {
                var row=projectStatRows[i];row.root.anchoredPosition=new Vector2(0,-(start+i*step+(i>=4?22:0)));row.root.sizeDelta=new Vector2(304,22);
                row.label.fontSize=LargeText?14:13;row.value.fontSize=LargeText?15:14;
                row.label.enableAutoSizing=true;row.label.fontSizeMin=12;row.label.fontSizeMax=row.label.fontSize;
                row.value.enableAutoSizing=true;row.value.fontSizeMin=12;row.value.fontSizeMax=row.value.fontSize;
            }
            projectEffectHeading.rectTransform.anchoredPosition=new Vector2(14,-(start+4*step));projectEffectHeading.fontSize=LargeText?13.5f:12.5f;
            float bottom=start+7*step+24;
            projectCapacity.rectTransform.anchoredPosition=new Vector2(14,-bottom);projectCapacity.rectTransform.sizeDelta=new Vector2(276,20);projectCapacity.fontSize=LargeText?13.5f:12.5f;
            projectCapacity.enableAutoSizing=true;projectCapacity.fontSizeMin=11.5f;projectCapacity.fontSizeMax=projectCapacity.fontSize;
            projectReserve.rectTransform.anchoredPosition=new Vector2(14,-(bottom+22));projectReserve.rectTransform.sizeDelta=new Vector2(276,34);projectReserve.fontSize=LargeText?13.5f:12.5f;
            recipeDetails.GetComponent<RectTransform>().anchoredPosition=new Vector2(0,48);recipeDetails.GetComponent<RectTransform>().sizeDelta=new Vector2(276,24);
            foreach(var b in new[]{confirmEditButton,moveButton,removeButton}){var r=b.GetComponent<RectTransform>();r.anchoredPosition=new Vector2(r.anchoredPosition.x,6);r.sizeDelta=new Vector2(r.sizeDelta.x,36);}
        }
    }
}
