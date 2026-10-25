using System;
using System.Collections.Generic;
using System.Linq;
using Terra.Art;
using Terra.Contracts;
using Terra.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Terra.UI
{
    public sealed partial class TerraHud
    {
        Transform actionDock;
        BuildActionRules actionRules;
        readonly Dictionary<Button,string> actionButtons=new Dictionary<Button,string>();
        readonly Dictionary<Button,string> categoryButtons=new Dictionary<Button,string>();
        readonly Dictionary<string,GameObject> actionRows=new Dictionary<string,GameObject>();
        readonly Dictionary<string,int> availableSites=new Dictionary<string,int>();
        readonly Dictionary<string,TextMeshProUGUI> selectedToolBadges=new Dictionary<string,TextMeshProUGUI>();
        TextMeshProUGUI actionSummary;
        string activeCategory="Farming";
        bool catalogKeyboard;
        int? highlightedFieldCount;
        string annualPlanningHint;

        public void BuildAvailability(int? count)
        {
            highlightedFieldCount=count;UpdateActionSummary();
        }

        void CreateActionDock(Action<string> choose)
        {
            actionDock=Panel(root,"Build catalog",new Vector2(.5f,0),new Vector2(0,20),new Vector2(760,222),Ink);
            for(int i=0;i<BuildActionRules.Categories.Length;i++)
            {
                string category=BuildActionRules.Categories[i];
                var button=Button(actionDock,category,new Vector2(0,1),new Vector2(10+i*148,-8),new Vector2(144,38),()=>ShowCategory(category),16);
                categoryButtons[button]=category;
            }
            actionSummary=Label(actionDock,"Preparing land-use choices…",new Vector2(0,0),new Vector2(16,6),new Vector2(728,38),14,Soft);
        }

        public void ConfigureBuildActions(BuildActionCatalog catalog,BuildActionRules rules,WorldData world,Action<string> choose)
        {
            actionRules=rules;
            foreach(string category in BuildActionRules.Categories)
            {
                var row=Rect(actionDock,category+" actions",new Vector2(0,1),new Vector2(10,-53),new Vector2(740,124));actionRows[category]=row.gameObject;
                var actions=rules.actions.Where(a=>a.category==category&&a.id!="cultivate-margin").ToArray();
                for(int i=0;i<actions.Length;i++)
                {
                    var action=actions[i];availableSites[action.id]=world.sites.Count(s=>rules.Rejection(s,action.id)==null);
                    float width=732f/Math.Max(4,actions.Length);
                    var button=Button(row,"",new Vector2(0,1),new Vector2(i*width,0),new Vector2(width-6,124),()=>choose(action.id),14);button.name="Build "+action.title;actionButtons[button]=action.id;
                    var icon=Rect(button.transform,"Model icon",new Vector2(.5f,1),new Vector2(0,-1),new Vector2(width-15,62)).gameObject.AddComponent<Image>();icon.sprite=catalog.Find(action.artId).icon;icon.preserveAspect=true;icon.raycastTarget=false;
                    var title=Label(button.transform,ShortTitle(action),new Vector2(.5f,1),new Vector2(0,-64),new Vector2(width-12,36),14,Paper,FontStyles.Bold);title.name="Action title";title.alignment=TextAlignmentOptions.Center;
                    var count=Label(button.transform,availableSites[action.id]==0?"No fitting parcel":availableSites[action.id]+" eligible fields",new Vector2(.5f,0),new Vector2(0,3),new Vector2(width-12,18),11,availableSites[action.id]==0?new Color(1,.69f,.55f):Soft);count.name="Action price";count.alignment=TextAlignmentOptions.Center;
                    var badge=Label(button.transform,"Selected",new Vector2(1,1),new Vector2(-5,-3),new Vector2(57,16),10,Accent,FontStyles.Bold);
                    badge.name="Selected tool badge";badge.alignment=TextAlignmentOptions.Right;badge.gameObject.SetActive(false);selectedToolBadges[action.id]=badge;
                    if(action.category=="Farming")
                    {
                        string motif=action.id=="conservation-rotation"?"redo":action.id=="cultivate-margin"?"map":action.id=="cover-crop"?"leaf":"work";
                        Glyph(icon.transform,motif,new Vector2(0,1),new Vector2(1,-1),18,Paper);
                    }
                }
            }
            ShowCategory(activeCategory);
        }
        static string ShortTitle(BuildActionRule a)
        {
            switch(a.id){case "low-density-housing":return "Homes";case "high-density-housing":return "Apartments";case "stewardship-agreement":return "Stewardship";case "restore-grass":return "Restore meadow";case "conservation-rotation":return "Crop rotation";case "agricultural-storage":return "Farm storage";case "wastewater-upgrade":return "Sewage works";case "non-renewable-power":return "Gas power";case "water-swale":return "Swale";default:return a.title;}
        }
        void ShowCategory(string category)
        {
            activeCategory=category;foreach(var row in actionRows)row.Value.SetActive(row.Key==category);
            ApplyCatalogPage();UpdateActionSummary();Paint();
        }
        void UpdateActionSummary()
        {
            if(actionRules==null)return;var action=actionRules.Find(activeTool);
            if(annualRules!=null)
            {
                var recipe=annualRules.actions.FirstOrDefault(a=>a.actionId==activeTool);
                actionSummary.text=recipe==null?(annualPlanningHint??"Choose a project, then a field · B to browse"):
                    (action==null?recipe.title:ShortTitle(action))+" · "+(highlightedFieldCount==0?"No available fields":"Hover a marked field");
                if(highlightedFieldCount>0)actionSummary.text+=" · "+highlightedFieldCount.Value+" available";
                return;
            }

            actionSummary.text=action==null?"Choose an action, then a field. Costs are prototype credits.":ShortTitle(action)+$" · {action.width:0} × {action.depth:0} m "+(action.landscape?"sample":"site")+" · "+(availableSites[action.id]==0?"No parcel fits at real scale":availableSites[action.id]+" eligible fields")+"\nPrototype costs · effects beyond farmland are not modeled";
        }
        bool CatalogSelected(Button button) => actionButtons.TryGetValue(button,out var id)?id==activeTool:categoryButtons.TryGetValue(button,out var category)&&category==activeCategory;
        bool BuildMenuKeyboard(UnityEngine.InputSystem.Keyboard keyboard)
        {
            if(annualCompleted)return false;
            if(keyboard.bKey.wasPressedThisFrame){catalogKeyboard=!catalogKeyboard;Focus(catalogKeyboard?categoryButtons.Keys.First():null);return true;}
            if(!catalogKeyboard)return false;
            if(keyboard.escapeKey.wasPressedThisFrame){catalogKeyboard=false;Focus(null);return true;}
            // Undo/redo remain available while browsing the nonmodal catalog.
            if(keyboard.zKey.wasPressedThisFrame||keyboard.yKey.wasPressedThisFrame)
            {catalogKeyboard=false;Focus(null);return false;}
            var available=categoryButtons.Keys.Concat(actionButtons.Keys).Concat(catalogPageButtons).Where(b=>b.gameObject.activeInHierarchy).ToArray();
            if(keyboard.tabKey.wasPressedThisFrame||keyboard.rightArrowKey.wasPressedThisFrame||keyboard.leftArrowKey.wasPressedThisFrame)
            {int i=Array.IndexOf(available,focus),step=keyboard.leftArrowKey.wasPressedThisFrame||keyboard.shiftKey.isPressed?-1:1;Focus(available[(i+step+available.Length)%available.Length]);}
            if((keyboard.enterKey.wasPressedThisFrame||keyboard.spaceKey.wasPressedThisFrame)&&focus!=null)
            {bool action=actionButtons.ContainsKey(focus);focus.onClick.Invoke();if(action){catalogKeyboard=false;Focus(null);}}
            return true;
        }
        void SelectCatalog(string id)
        {
            foreach(var badge in selectedToolBadges)badge.Value.gameObject.SetActive(badge.Key==id);
            var action=actionRules?.Find(id);if(action!=null && annualRules!=null)catalogPage=Array.IndexOf(actionButtons.Where(p=>AnnualCategory(p.Value)==AnnualCategory(id)).Select(p=>p.Value).ToArray(),id)/catalogColumns;
            if(action!=null)ShowCategory(annualRules==null?action.category:AnnualCategory(id));else UpdateActionSummary();
        }
        public string LandRules(Site site)
        {
            if(site.protectedSite)return site.name+" is protected by the scenario. No actions are allowed here.";
            bool water=site.allowedTreatments.Contains("water-swale");
            return "Editable field · "+site.analysisAreaM2.ToString("N0")+" m²\n\n"+(water?"Swale connection enabled here.":"No swale connection in this scenario.")+"\n\nBuildings must fit the parcel and meet the foundation limit.";
        }
    }
}
