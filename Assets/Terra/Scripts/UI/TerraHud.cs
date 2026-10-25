using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Terra.Contracts;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
namespace Terra.UI
{
    // TN-03/04/05/06, CS-01/04, AX-01. One visual language for the entire slice.
    public sealed partial class TerraHud
    {
        readonly Terra.Core.LocationProfile location;
        readonly Transform root;readonly UnityEngine.UI.CanvasScaler scaler;
        UnityEngine.UI.Button viewButton,controlsButton;
        Vector2 lastScreen;float lastRatio;
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] static extern float TerraPixelRatio();
#endif
        static readonly Color Ink=new Color(.055f,.09f,.12f),PanelColor=new Color(.085f,.14f,.18f,.97f),Raised=new Color(.13f,.21f,.26f),Paper=new Color(.94f,.97f,.95f),Soft=new Color(.66f,.77f,.80f),Accent=new Color(.53f,.85f,.69f);
        readonly TMP_FontAsset font;readonly Sprite rounded;readonly Action resume,retryLoad;bool loadFailed,compactLayout;
        readonly List<UnityEngine.UI.Button> buttons=new List<UnityEngine.UI.Button>();
        readonly Dictionary<TextMeshProUGUI,float> sizes=new Dictionary<TextMeshProUGUI,float>();
        TextMeshProUGUI placeSubtitle;
        TextMeshProUGUI production,credits,runoffStatus,tip,previewTitle,previewKind,previewBody,previewAction,toast,loadText,modalText;
        GameObject tooltip,loading,loadingShade,modal,modalShade,toastPanel,guideSummary;
        UnityEngine.UI.Button undoButton,motionButton,muteButton,textButton,dataPageButton,guideButton,closeButton,retryButton;
        UnityEngine.UI.Button focus,returnFocus; int dataPage;string activeTool="";
        public void SetVisible(bool visible){root.gameObject.SetActive(visible);}
        Action<bool> atlasMotionChanged;
        public void ConfigureAtlas(Action open,Action<bool> motionChanged)
        {
            atlasMotionChanged=motionChanged;
            var button=Button(root,"World atlas",new Vector2(0,0),new Vector2(16,112),new Vector2(116,36),open,14);
            button.transform.SetSiblingIndex(modalShade.transform.GetSiblingIndex());
        }
        public bool ModalOpen=>modal.activeSelf; public float PixelRatio=>Mathf.Max(1,lastRatio);
        public bool ReducedMotion{get;private set;} public bool Muted{get;private set;} public bool LargeText{get;private set;}
        public TerraHud(Action<string> choose,Action undo,Action reset,Action cancel,Action retry,Action resume,Terra.Core.LocationProfile location=null)
        {
            this.location=location??Terra.Core.LocationProfile.All[0];this.resume=resume;retryLoad=retry;
            var canvas=new GameObject("Terra HUD",typeof(Canvas),typeof(UnityEngine.UI.CanvasScaler),typeof(UnityEngine.UI.GraphicRaycaster));root=canvas.transform;
            canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            scaler=canvas.GetComponent<UnityEngine.UI.CanvasScaler>();scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ConstantPixelSize;
            if(EventSystem.current==null){var events=new GameObject("EventSystem",typeof(EventSystem));
#if !UNITY_WEBGL || UNITY_EDITOR
                events.AddComponent<InputSystemUIInputModule>();
#endif
            }
            EventSystem.current.sendNavigationEvents=false;
            font=TMP_Settings.defaultFontAsset;rounded=RoundSprite();
            var identity=Panel(root,"Place",new Vector2(0,1),new Vector2(24,-24),new Vector2(310,94),PanelColor);
            Label(identity,"MyTerrain",new Vector2(0,1),new Vector2(18,-12),new Vector2(270,24),16,Accent,FontStyles.Bold);
            Label(identity,this.location.Name,new Vector2(0,1),new Vector2(18,-35),new Vector2(278,28),23,Paper,FontStyles.Bold);
            placeSubtitle=Label(identity,this.location.Subtitle,new Vector2(0,1),new Vector2(18,-65),new Vector2(276,24),this.location.IsWisconsin?16:12,Soft);
            var stats=Panel(root,"Measures",new Vector2(1,1),new Vector2(-24,-24),new Vector2(400,94),PanelColor);
            Label(stats,"ILLUSTRATIVE CAPACITY",new Vector2(0,1),new Vector2(18,-14),new Vector2(235,22),14,Soft);
            production=Label(stats,"—",new Vector2(0,1),new Vector2(18,-35),new Vector2(230,32),25,Paper,FontStyles.Bold);
            runoffStatus=Label(stats,"Preparing runoff estimate",new Vector2(0,1),new Vector2(18,-69),new Vector2(250,22),14,Soft);
            Label(stats,"CREDITS",new Vector2(0,1),new Vector2(282,-14),new Vector2(104,22),14,Soft);
            credits=Label(stats,"—",new Vector2(0,1),new Vector2(282,-39),new Vector2(96,36),29,Accent,FontStyles.Bold);
            CreateActionDock(choose);
            var guidance=Panel(root,"Context guidance",new Vector2(0,1),new Vector2(24,-126),new Vector2(310,56),PanelColor);
            tip=Label(guidance,"Choose a treatment\nto shape a proposal",new Vector2(.5f,.5f),Vector2.zero,new Vector2(286,48),16,Paper);tip.alignment=TextAlignmentOptions.Center;
            undoButton=Button(root,"Undo  Z",new Vector2(0,0),new Vector2(24,20),new Vector2(130,56),undo,16);
            viewButton=Button(root,"Valley view  R",new Vector2(0,0),new Vector2(24,84),new Vector2(130,56),reset,16);
            guideButton=Button(root,"Field guide  G",new Vector2(1,0),new Vector2(-24,20),new Vector2(148,56),()=>{dataPage=0;ShowData();},16);
            controlsButton=Button(root,"Controls  ?",new Vector2(1,0),new Vector2(-24,84),new Vector2(148,56),ShowHelp,16);
            tooltip=Panel(root,"Site preview",new Vector2(1,1),new Vector2(-24,-132),new Vector2(292,374),PanelColor).gameObject;
            previewKind=Label(tooltip.transform,"",new Vector2(0,1),new Vector2(18,-16),new Vector2(256,24),14,Accent);
            previewTitle=Label(tooltip.transform,"",new Vector2(0,1),new Vector2(18,-45),new Vector2(256,52),22,Paper,FontStyles.Bold);
            previewBody=Label(tooltip.transform,"",new Vector2(0,1),new Vector2(18,-105),new Vector2(256,160),17,Paper);
            previewAction=Label(tooltip.transform,"",new Vector2(0,0),new Vector2(18,13),new Vector2(256,30),16,Accent);
            tooltip.SetActive(false);
            toastPanel=Panel(root,"Action feedback",new Vector2(.5f,1),new Vector2(0,-132),new Vector2(570,52),PanelColor).gameObject;toastPanel.SetActive(false);
            toast=Label(toastPanel.transform,"",new Vector2(.5f,.5f),Vector2.zero,new Vector2(538,46),16,Paper);toast.alignment=TextAlignmentOptions.Center;
            var shade=Rect(root,"Guide backdrop",new Vector2(.5f,.5f),Vector2.zero,Vector2.zero);shade.anchorMin=Vector2.zero;shade.anchorMax=Vector2.one;shade.offsetMin=shade.offsetMax=Vector2.zero;
            shade.gameObject.AddComponent<UnityEngine.UI.Image>().color=new Color(.04f,.10f,.09f,.72f);modalShade=shade.gameObject;modalShade.SetActive(false);
            modal=Panel(root,"Field guide",new Vector2(.5f,.5f),Vector2.zero,new Vector2(800,650),Paper).gameObject;
            Label(modal.transform,"MyTerrain   /   FIELD NOTES",new Vector2(0,1),new Vector2(32,-22),new Vector2(730,24),14,new Color(.31f,.40f,.31f),FontStyles.Bold);
            modalText=Label(modal.transform,"",new Vector2(0,1),new Vector2(32,-61),new Vector2(736,440),20,Ink);
            guideSummary=Rect(modal.transform,"Evidence distinctions",new Vector2(.5f,0),new Vector2(0,152),new Vector2(736,78)).gameObject;
            string[] categories={"OBSERVED","AUTHORED","NOT A FORECAST"},summaries={"Terrain & broad cover","Sites & productive units","No flood or yield claim"};
            for(int i=0;i<3;i++){var card=Panel(guideSummary.transform,categories[i],new Vector2(0,0),new Vector2(i*248,0),new Vector2(240,78),new Color(.86f,.89f,.79f));Label(card,categories[i],new Vector2(0,1),new Vector2(12,-12),new Vector2(216,24),14,Ink,FontStyles.Bold);Label(card,summaries[i],new Vector2(0,1),new Vector2(12,-38),new Vector2(216,32),16,Ink);}
            guideSummary.SetActive(false);
            closeButton=Button(modal.transform,"Return to landscape  Esc",new Vector2(.5f,0),new Vector2(0,22),new Vector2(300,56),CloseModal,17);
            motionButton=Button(modal.transform,"Motion: full",new Vector2(0,0),new Vector2(32,86),new Vector2(234,56),()=>{ReducedMotion=!ReducedMotion;SetButtonText(motionButton,ReducedMotion?"Motion: reduced":"Motion: full");atlasMotionChanged?.Invoke(ReducedMotion);},16);
            muteButton=Button(modal.transform,"Sound: on",new Vector2(.5f,0),new Vector2(0,86),new Vector2(234,56),()=>{Muted=!Muted;SetButtonText(muteButton,Muted?"Sound: muted":"Sound: on");},16);
            textButton=Button(modal.transform,"Text: standard",new Vector2(1,0),new Vector2(-32,86),new Vector2(234,56),ToggleText,16);
            dataPageButton=Button(modal.transform,"Sources & limits →",new Vector2(.5f,0),new Vector2(0,86),new Vector2(310,56),()=>{dataPage=(dataPage+1)%6;ShowData();},17);dataPageButton.gameObject.SetActive(false);modal.SetActive(false);
            var loadingBackdrop=Rect(root,"Loading backdrop",Vector2.zero,Vector2.zero,Vector2.zero);
            loadingBackdrop.anchorMin=Vector2.zero;loadingBackdrop.anchorMax=Vector2.one;loadingBackdrop.offsetMin=loadingBackdrop.offsetMax=Vector2.zero;
            loadingBackdrop.gameObject.AddComponent<UnityEngine.UI.Image>().color=new Color(.84f,.88f,.77f);loadingShade=loadingBackdrop.gameObject;
            loading=Panel(root,"Loading",new Vector2(.5f,.5f),Vector2.zero,new Vector2(540,250),PanelColor).gameObject;
            loadText=Label(loading.transform,"Preparing the landscape",new Vector2(0,1),new Vector2(28,-28),new Vector2(484,142),23,Paper);
            retryButton=Button(loading.transform,"Try again",new Vector2(.5f,0),new Vector2(0,24),new Vector2(180,56),retry,17);
            retryButton.name="Retry loading";retryButton.gameObject.SetActive(false);
            Layout();
        }
        public void Layout()
        {
            float ratio=1;
#if UNITY_WEBGL && !UNITY_EDITOR
            ratio=Mathf.Max(1,TerraPixelRatio());
#endif
            var screen=new Vector2(Screen.width,Screen.height);if(screen==lastScreen&&Mathf.Approximately(ratio,lastRatio))return;lastScreen=screen;lastRatio=ratio;scaler.scaleFactor=ratio;
            float width=Screen.width/ratio,height=Screen.height/ratio;bool compact=width<1120;compactLayout=compact;tip.transform.parent.gameObject.SetActive(annualRules==null&&!compact&&!tooltip.activeSelf);
            void Utility(UnityEngine.UI.Button b,bool right,int row){var r=b.GetComponent<RectTransform>();if(compact){r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(24+row*(right?156:138),right?-194:-130);}else{r.anchorMin=r.anchorMax=r.pivot=new Vector2(right?1:0,0);r.anchoredPosition=new Vector2(right?-24:24,20+row*64);}}
            Utility(undoButton,false,0);Utility(viewButton,false,1);Utility(guideButton,true,0);Utility(controlsButton,true,1);
            modal.transform.localScale=Vector3.one*Mathf.Min(1,(height-32)/650,(width-32)/800);
            actionDock.localScale=Vector3.one*Mathf.Min(1,(width-48)/760);
            if(annualRules!=null)LayoutAnnual(width,height,compact||height<680);
            LayoutFieldNotes();
        }
        // F10 reports the rendered canvas for evidence; it never changes gameplay state.
        public string Audit()
        {
            Canvas.ForceUpdateCanvases();float ratio=Mathf.Max(1,lastRatio);
            UiRect Bounds(RectTransform r){var corners=new Vector3[4];r.GetWorldCorners(corners);var a=RectTransformUtility.WorldToScreenPoint(null,corners[0])/ratio;var b=RectTransformUtility.WorldToScreenPoint(null,corners[2])/ratio;return new UiRect{x=a.x,y=Screen.height/ratio-b.y,width=b.x-a.x,height=b.y-a.y};}
            var labels=sizes.Keys.Where(t=>t.gameObject.activeInHierarchy&&(!ModalOpen||t.transform.IsChildOf(modal.transform))&&!string.IsNullOrWhiteSpace(t.text)).Select(t=>{t.ForceMeshUpdate();var rect=Bounds(t.rectTransform);float scale=rect.height/t.rectTransform.rect.height;var points=t.textInfo.characterInfo.Take(t.textInfo.characterCount).Where(c=>c.isVisible).Select(c=>c.pointSize).ToArray();return new UiLabel{text=t.text,rect=rect,bodyCssPx=t.fontSize*scale,minGlyphCssPx=(points.Length>0?points.Min():t.fontSize)*scale,preferredHeight=t.preferredHeight,availableHeight=t.rectTransform.rect.height};}).ToArray();
            var controls=buttons.Where(b=>b.gameObject.activeInHierarchy&&(!ModalOpen||b.transform.IsChildOf(modal.transform))).Select(b=>new UiControl{name=b.name,enabled=b.IsInteractable(),rect=Bounds(b.GetComponent<RectTransform>())}).ToArray();
            var panels=new List<UiRect>();foreach(Transform t in root)if(t.gameObject.activeInHierarchy&&t.GetComponent<UnityEngine.UI.Image>()!=null)panels.Add(Bounds((RectTransform)t));
            return JsonUtility.ToJson(new UiAudit{cssWidth=Screen.width/ratio,cssHeight=Screen.height/ratio,pixelRatio=ratio,modal=ModalOpen,largeText=LargeText,reducedMotion=ReducedMotion,muted=Muted,labels=labels,controls=controls,panels=panels.ToArray()});
        }
        [Serializable] public sealed class UiRect{public float x,y,width,height;}
        [Serializable] public sealed class UiLabel{public string text;public UiRect rect;public float bodyCssPx,minGlyphCssPx,preferredHeight,availableHeight;}
        [Serializable] public sealed class UiControl{public string name;public bool enabled;public UiRect rect;}
        [Serializable] public sealed class UiAudit{public float cssWidth,cssHeight,pixelRatio;public bool modal,largeText,reducedMotion,muted;public UiLabel[] labels;public UiControl[] controls;public UiRect[] panels;}
        public void Ready(){loading.SetActive(false);loadingShade.SetActive(false);}
        public void Progress(string message)=>loadText.text="<b>Preparing "+location.Name+"</b>\n\n<size=78%>"+message+"</size>";
        public void Error(string message){loadFailed=true;loadingShade.SetActive(true);loading.SetActive(true);retryButton.gameObject.SetActive(true);loadText.text="<b>The landscape could not open</b>\n\n<size=78%>"+message+"</size>";}
        public void UpdateResult(WorldResult r,bool canUndo,float target){production.text=r.production.ToString("0.#")+" <size=64%>illustrative units</size>";credits.text=r.remainingCredits.ToString();undoButton.interactable=canUndo;RunoffResult(r);Paint();}
        public void Selected(string id){activeTool=id;SelectCatalog(id);tip.text=string.IsNullOrEmpty(id)?"Choose a category\nand an action":"Tab: inspect · Enter: place\nF: focus · Esc: cancel";Paint();}
        public void Preview(Site site,Treatment treatment,Evaluation result,WorldResult current)
        {
            tooltip.SetActive(site!=null);tip.transform.parent.gameObject.SetActive(site==null&&!compactLayout);if(site==null)return;
            previewTitle.text=site.name;previewKind.color=Accent;previewKind.text=site.protectedSite?"PROTECTED LEARNING ZONE":"HYPOTHETICAL SITE";
            if(treatment==null){previewBody.text=LandRules(site);previewAction.text="Choose a category and action";return;}
            if(!result.valid){bool placed=result.reason=="This proposal is already placed.";previewKind.text=placed?"CURRENT PROPOSAL":"UNAVAILABLE";previewKind.color=placed?Accent:new Color(1,.69f,.55f);previewBody.text=placed?treatment.title+"\n\nThis treatment is already here.":result.reason;previewAction.text=placed?"Choose a tool or Z to undo":"Try another site or treatment";return;}
            previewKind.color=Accent;
            float delta=result.result.production-current.production;int cost=result.result.spentCredits-current.spentCredits;
            var action=actionRules?.Find(treatment.id);
            previewBody.text=treatment.title+"\n"+(action==null?"":$"{action.width:0} × {action.depth:0} m "+(action.landscape?"sample":"site")+"\n")+"<b>"+cost+"</b> credits · <b>"+(delta>=0?"+":"")+delta.ToString("0.#")+"</b> farm units\n<size=82%>Other benefits: not modeled</size>";
            previewAction.text="Click / Enter to place   ·   Esc";
        }
        public void Toast(string text){toast.text=text;toastPanel.SetActive(!string.IsNullOrEmpty(text));}
        public void CloseModal(){modal.SetActive(false);modalShade.SetActive(false);Focus(returnFocus!=null&&returnFocus.gameObject.activeInHierarchy&&returnFocus.IsInteractable()?returnFocus:null);resume?.Invoke();}
        public void ShowHelp()=>ShowModal("Make room for a better balance",
            "<b>01   Plan → End Year → adapt</b>\nQueue projects and reserve their public costs. End Year pays once and advances them. Accepted years stay in history. Three categories contain all twelve families; arrows show more options.\n\n"+
            "<b>02   Explore the valley</b>\nDrag or arrow keys to pan · Scroll or + / − to zoom\nF focuses a site · R returns to the valley\n\n"+
            "<b>03   Edit this year’s proposals</b>\nB: keyboard build menu · Tab/arrows: browse\nEnter: queue · Z: undo · Y: redo · Esc: cancel\nE: End Year · L: report · T: future · I: meters · O: more\nG: field guide · ?: controls · Shift+Esc: world atlas");
        void ToggleText(){LargeText=!LargeText;foreach(var p in sizes)p.Key.fontSize=p.Value*(LargeText?1.12f:1);SetButtonText(textButton,LargeText?"Text: larger":"Text: standard");lastScreen=Vector2.zero;Layout();}
        public bool KeyboardInput(Keyboard k)
        {
            if(k==null)return ModalOpen;
            if(loading.activeSelf){if(loadFailed&&k.enterKey.wasPressedThisFrame)retryLoad();return true;}
            if(OverviewKeyboard(k))return true;
            if(!ModalOpen&&!AnnualOverviewOpen&&!(moreMenu?.gameObject.activeSelf??false)&&BuildMenuKeyboard(k))return true;
            if(!ModalOpen && annualRules!=null)
            {
                if(k.oKey.wasPressedThisFrame&&!AnnualOverviewOpen){moreButton.onClick.Invoke();return true;}
                if(k.eKey.wasPressedThisFrame){reviewYearShortcut?.Invoke();return true;}
                if(k.lKey.wasPressedThisFrame){reportShortcut?.Invoke();return true;}
                if(k.tKey.wasPressedThisFrame){CloseAnnualOverview();futureShortcut?.Invoke();return true;}
                if(k.iKey.wasPressedThisFrame&&acceptedOutcomes?.economy!=null){AnnualMetrics(acceptedOutcomes,CloseModal);return true;}
            }
            if(!ModalOpen&&moreMenu!=null&&moreMenu.gameObject.activeSelf)
            {
                if(k.escapeKey.wasPressedThisFrame){moreMenu.gameObject.SetActive(false);Focus(moreButton);return true;}
                var choices=buttons.Where(b=>b.gameObject.activeInHierarchy&&b.IsInteractable()&&b.transform.IsChildOf(moreMenu)).ToArray();
                if(k.tabKey.wasPressedThisFrame){int index=Array.IndexOf(choices,focus),step=k.shiftKey.isPressed?-1:1;Focus(choices[(index+step+choices.Length)%choices.Length]);return true;}
                if((k.enterKey.wasPressedThisFrame||k.spaceKey.wasPressedThisFrame)&&focus!=null&&choices.Contains(focus))focus.onClick.Invoke();
                return true; // A menu key never falls through to a world edit.
            }
            if(k.escapeKey.wasPressedThisFrame&&ModalOpen){CloseModal();return true;}
            if(!ModalOpen)
            {
                // Keyboard activation of a focused UI button must never fall through to a map edit.
                if((k.enterKey.wasPressedThisFrame||k.spaceKey.wasPressedThisFrame)&&focus!=null&&focus.gameObject.activeInHierarchy&&focus.IsInteractable())
                {focus.onClick.Invoke();return true;}
                // Tab outside the catalog is field navigation; leave any stale pointer focus behind.
                if(k.tabKey.wasPressedThisFrame)Focus(null);
            }
            if(!ModalOpen){if(k.gKey.wasPressedThisFrame){dataPage=0;ShowData();return true;}if(k.slashKey.wasPressedThisFrame){ShowHelp();return true;}return false;}
            var available=buttons.Where(b=>b.gameObject.activeInHierarchy&&b.IsInteractable()&&b.transform.IsChildOf(modal.transform)).ToArray();
            if(k.tabKey.wasPressedThisFrame||k.rightArrowKey.wasPressedThisFrame||k.leftArrowKey.wasPressedThisFrame){int i=Array.IndexOf(available,focus),step=k.leftArrowKey.wasPressedThisFrame||k.shiftKey.isPressed?-1:1;Focus(available[(i+step+available.Length)%available.Length]);}
            if((k.enterKey.wasPressedThisFrame||k.spaceKey.wasPressedThisFrame)&&focus!=null&&focus.gameObject.activeInHierarchy)focus.onClick.Invoke();
            return true;
        }
        public void PointerHover(Vector2 point,bool pointerDown=false)
        {
            // A pointer press leaves catalog keyboard navigation; hover alone does not.
            if(pointerDown&&!ModalOpen)catalogKeyboard=false;
            if(EventSystem.current==null)return;var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=point},hits);
            var b=hits.Count>0?hits[0].gameObject.GetComponentInParent<UnityEngine.UI.Button>():null;
            if(b!=null&&b.IsInteractable())Focus(b);else if(!ModalOpen)Focus(null);
        }
        void Focus(UnityEngine.UI.Button button){focus=button;Paint();}
        void Paint()
        {
            foreach(var b in buttons){bool selected=CatalogSelected(b)||b==selectedHighlight;
                b.GetComponent<UnityEngine.UI.Image>().color=!b.IsInteractable()?PanelColor:selected?new Color(.20f,.38f,.38f):(b==endYearButton||b==overviewNext||b==confirmEditButton)?new Color(.20f,.39f,.32f):Raised;
                var line=b.GetComponent<UnityEngine.UI.Outline>();line.enabled=b==focus||selected;line.effectColor=b==focus?(modal!=null&&b.transform.IsChildOf(modal.transform)?Ink:Paper):Accent;
                foreach(var label in b.GetComponentsInChildren<TextMeshProUGUI>())label.alpha=b.IsInteractable()?1:.48f;
                foreach(var glyph in b.GetComponentsInChildren<HudGlyph>())glyph.canvasRenderer.SetAlpha(b.IsInteractable()?1:.48f);
            }
        }
        public void SetReducedMotion(bool value){ReducedMotion=value;SetButtonText(motionButton,value?"Motion: reduced":"Motion: full");}
        void ShowData()
        {
            if(!location.IsWisconsin && dataPage<3)
            {
                string body=dataPage==0 ? "<b>"+location.Name+"</b> · "+location.Region+"\n\nReal elevation anchors this regional landscape. All surface cover, management sites, individual plants, consent and service connections are authored game assumptions. No parcel or land-use survey is claimed.\n\nThe same fictional ten-year USD budget and educational rules are used across regions; they are not local finances, legal permissions, development recommendations or calibrated forecasts. No observed water or road layer is included."
                    : dataPage==1 ? "<b>Elevation · Mapzen / AWS Terrain Tiles</b>\nUSGS 3DEP and global SRTM/GMTED2010 terrain data, retrieved October 25, 2026. Acquisition dates and effective resolution vary by source. Terrarium tiles at zoom 14 are sampled to a 10 m display grid; resampling does not add observed detail. No vertical exaggeration.\n\n<b>Cover · authored scenario</b>\nGrassland, cultivation and any woodland are illustrative polygons. Vegetation is decorative. Site boundaries are hypothetical opportunities, not real parcels."
                    : "<b>Sources & attribution</b>\nMapzen / AWS Terrain Tiles; USGS 3DEP, GMTED2010 and SRTM. USGS data are public domain.\nregistry.opendata.aws/terrain-tiles\ngithub.com/tilezen/joerd/blob/master/docs/attribution.md\n\n<b>What is not included</b>\nNo observed water, roads, buildings, soil, ownership or consent data. Runoff connections are isolated, authored site groups. Budgets and service capacities are educational assumptions; USD figures do not represent local public finances.";
                ShowModal(dataPage==0?"Real terrain. Possible futures.":dataPage==1?"Reading this landscape":"Sources & limits",body);
                motionButton.gameObject.SetActive(false);muteButton.gameObject.SetActive(false);textButton.gameObject.SetActive(false);dataPageButton.gameObject.SetActive(true);guideSummary.SetActive(false);
                SetButtonText(dataPageButton,dataPage==0?"Terrain & cover →":dataPage==1?"Sources & limits →":"Landscape art →");return;
            }
            if(dataPage==0)ShowModal("Real land. Hypothetical choices.",
                "<b>Meadowbrook Valley</b> is a fictional name for Coon Valley / Brye Lane / Coon Creek, Wisconsin.\n\n"+
                "<b>Bounds</b> · 43.697–43.706° N · 91.043–91.031° W (0.97 km²)\n\n"+
                "Real terrain, broad cover and mapped water/roads shape the landscape. Trees, crop rows, softened cover edges, stream banks, water depths and bridge designs are illustrative. Field parcels and individual trees were not surveyed.\n\n"+
                "Eleven management opportunities (including a hypothetical wind site) and two no-edit zones are game assumptions, not ownership or legal designations. Costs, demand, production units, soil B, the 50 mm storm and swale capacity are fictional. Runoff is a simplified same-storm estimate; no flood or yield forecast.");
            else if(dataPage==1)ShowModal("Reading the terrain & cover",
                "<b>01   Elevation · USGS 3DEP</b>\nWI_8County_2020_A20, April–May 2020; published January 2024. Bare earth, NAVD88 metres. The 1 m source is simplified to a 10 m grid, without vertical exaggeration.\n\n"+
                "<b>02   Land cover · USGS / MRLC NLCD 2021</b>\n30 m classes, June 2023 release. Narrow margins are unresolved. Simplified display groups do not measure ecological condition.\n\n"+
                "<b>A miniature, not a survey of every plant</b>\nCanopy shapes, crop rows, surface grain and vegetation clusters are illustrations. They do not add finer observed geographic data.");
            else if(dataPage==2)ShowModal("Mapped lines & the limits",
                "<b>03   Water & roads · USGS National Map</b>\nNHD flowlines and transportation layers 30–32, retrieved October 24, 2026. Feature dates vary; drawn widths are illustrative. Federal source data are public domain.\n\n"+
                "<b>What is not included</b>\nNo parcel, structure, tree, ownership or local soil survey. This crop is not a whole watershed. No external inflow or downstream flood protection is modeled.\n\n"+
                "<b>Use this game to ask better questions</b>\nThe proposals are educational choices, not recommendations for real farms. Production units are illustrative accounting, not measured yields.");
            else if(dataPage==3)ShowModal("Landscape art & acknowledgements",
                "<b>JulioVII · Stylized Grass & Dirt</b>\nCC-BY (version unspecified by creator)\njuliovii.itch.io/ftpgrass-dirt\nAdapted tiling, tint, relief and blending.\n\n"+
                "<b>erces · Free Stylized 3D Tree Pack</b>\nerces.itch.io/free-stylized-3d-tree-pack\nCreator permits free project use. Adapted scale, materials and instancing.\n\n"+
                "<b>Symphonie Studio · Free Oak sample</b>\nsymphonie-studio.itch.io/stylizedforestkit\nAdapted scale, matte materials and instancing.");
            else if(dataPage==4)ShowModal("Plants & sound credits",
                "<b>Elijah Cobden · Stylized Trees & Nature Packs</b>\nCC BY 4.0 · creativecommons.org/licenses/by/4.0\nelijahcobden.itch.io/stylized-trees-pack\nelijahcobden.itch.io/stylized-nature-pack\nAdapted geometry, palette, materials and instancing.\n\n"+
                "<b>Quaternius · Nature MegaKit & Ultimate Crops</b>\nCC0 · quaternius.com · adapted geometry/materials.\n\n"+
                "<b>Kenney · Nature Kit & Interface Sounds</b>\nCC0 · kenney.nl\nThese artists do not endorse the game or its educational assumptions.");
            else ShowModal("One storm. Connected choices.",
                "<b>50 mm rain · assumed soil B</b>\nEducational TR-55 estimate. Cover CN: crops 78, grass 61, woods 55, high-runoff cover 98 (assumed, not measured imperviousness).\n\n"+
                "<b>Each area counts once</b>\nSites + unchanged context: "+(location.IsWisconsin?"1.49":"1.44")+" km² with display margin. Volumes summed. "+runoffContextSummary+"\n\n"+
                "<b>Swales have limits</b>\nEach holds ≤8 m³ beyond its grass-cover effect, only from its highlighted hypothetical group. No measured drainage, outside inflow, flood or water-quality forecast.\n"+
                "<size=80%>Source: USDA TR-55 (1986), chapter 2. Tables reproduced by USACE: hec.usace.army.mil/confluence/hmsdocs/hmstrm/cn-tables</size>");
            motionButton.gameObject.SetActive(false);muteButton.gameObject.SetActive(false);textButton.gameObject.SetActive(false);dataPageButton.gameObject.SetActive(true);
            guideSummary.SetActive(dataPage==0&&!LargeText);
            SetButtonText(dataPageButton,dataPage==0?"Terrain & cover →":dataPage==1?"Water & limits →":dataPage==2?"Landscape art →":dataPage==3?"Plants & sound →":dataPage==4?"Runoff & connections →":"← Place & assumptions");
        }
        void ShowModal(string title,string body)
        {ResetFieldNotes();if(moreMenu!=null)moreMenu.gameObject.SetActive(false);closeButton.onClick.RemoveAllListeners();closeButton.onClick.AddListener(CloseModal);SetButtonText(closeButton,"Return to landscape  Esc");if(!ModalOpen)returnFocus=focus;if(annualPrimary!=null)annualPrimary.gameObject.SetActive(false);if(annualDetails!=null)annualDetails.gameObject.SetActive(false);guideSummary.SetActive(false);tooltip.SetActive(false);modalText.text="<size=140%><b>"+title+"</b></size>\n\n"+body;modalShade.SetActive(true);modal.SetActive(true);motionButton.gameObject.SetActive(true);muteButton.gameObject.SetActive(true);textButton.gameObject.SetActive(true);dataPageButton.gameObject.SetActive(false);Focus(closeButton);}
        static void SetButtonText(UnityEngine.UI.Button b,string text)=>b.GetComponentInChildren<TextMeshProUGUI>().text=text;
        RectTransform Rect(Transform parent,string name,Vector2 anchor,Vector2 pos,Vector2 size){var go=new GameObject(name,typeof(RectTransform));var r=go.GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=r.pivot=anchor;r.anchoredPosition=pos;r.sizeDelta=size;return r;}
        Transform Panel(Transform parent,string name,Vector2 anchor,Vector2 pos,Vector2 size,Color color){var r=Rect(parent,name,anchor,pos,size);var image=r.gameObject.AddComponent<UnityEngine.UI.Image>();image.sprite=rounded;image.type=UnityEngine.UI.Image.Type.Sliced;image.color=color;return r;}
        TextMeshProUGUI Label(Transform parent,string text,Vector2 anchor,Vector2 pos,Vector2 size,float fontSize,Color color,FontStyles style=FontStyles.Normal){var r=Rect(parent,"Label",anchor,pos,size);var label=r.gameObject.AddComponent<TextMeshProUGUI>();label.font=font;label.text=text;label.fontSize=fontSize;label.color=color;label.fontStyle=style;label.raycastTarget=false;label.textWrappingMode=TextWrappingModes.Normal;label.overflowMode=TextOverflowModes.Overflow;sizes[label]=fontSize;return label;}
        UnityEngine.UI.Button Button(Transform parent,string text,Vector2 anchor,Vector2 pos,Vector2 size,Action action,float textSize){var panel=Panel(parent,"Button "+text,anchor,pos,size,Raised);var button=panel.gameObject.AddComponent<UnityEngine.UI.Button>();button.targetGraphic=panel.GetComponent<UnityEngine.UI.Image>();button.transition=UnityEngine.UI.Selectable.Transition.None;button.onClick.AddListener(()=>{action();Paint();});var line=panel.gameObject.AddComponent<UnityEngine.UI.Outline>();line.effectDistance=new Vector2(2,-2);line.enabled=false;var t=Label(panel,text,new Vector2(.5f,.5f),Vector2.zero,size-new Vector2(16,4),textSize,Paper);t.alignment=TextAlignmentOptions.Center;buttons.Add(button);return button;}
        Sprite RoundSprite(){const int n=32;var texture=new Texture2D(n,n,TextureFormat.RGBA32,false);for(int y=0;y<n;y++)for(int x=0;x<n;x++){float dx=Mathf.Max(8-x,x-23),dy=Mathf.Max(8-y,y-23);float d=Mathf.Sqrt(Mathf.Max(dx,0)*Mathf.Max(dx,0)+Mathf.Max(dy,0)*Mathf.Max(dy,0));texture.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01(8.5f-d)));}texture.Apply();return Sprite.Create(texture,new Rect(0,0,n,n),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,new Vector4(9,9,9,9));}
    }
}
