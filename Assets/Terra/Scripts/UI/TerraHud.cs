using System;
using Terra.Contracts;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
namespace Terra.UI
{
    public sealed class TerraHud
    {
        readonly Transform root; readonly Color ink=new Color(.14f,.24f,.20f),cream=new Color(.97f,.96f,.90f),muted=new Color(.37f,.45f,.37f),green=new Color(.22f,.38f,.25f);
        readonly TMP_FontAsset font; readonly Sprite rounded;
        TextMeshProUGUI production,credits,tip,tooltipText,toast,loadText,modalText;
        GameObject tooltip,loading,modal,modalShade,toastPanel; UnityEngine.UI.Button undoButton,motionButton,muteButton;
        UnityEngine.UI.Image[] cards; UnityEngine.UI.Image standard,margin;
        UnityEngine.UI.Button dataPageButton;int dataPage;
        public bool ModalOpen=>modal.activeSelf; public bool ReducedMotion {get;private set;} public bool Muted{get;private set;}
        public TerraHud(Action<string> choose,Action undo,Action reset,Action cancel,Action retry)
        {
            var canvas=new GameObject("Terra HUD",typeof(Canvas),typeof(UnityEngine.UI.CanvasScaler),typeof(UnityEngine.UI.GraphicRaycaster));root=canvas.transform;
            canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;var scaler=canvas.GetComponent<UnityEngine.UI.CanvasScaler>();scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1280,720);scaler.matchWidthOrHeight=.5f;
            if(EventSystem.current==null)
            {
                var events=new GameObject("EventSystem",typeof(EventSystem));
#if !UNITY_WEBGL || UNITY_EDITOR
                events.AddComponent<InputSystemUIInputModule>();
#endif
            }
            font=TMP_Settings.defaultFontAsset;rounded=RoundSprite();
            Panel(root,"Location label backing",new Vector2(0,1),new Vector2(16,-16),new Vector2(370,162),new Color(.97f,.96f,.90f,.96f));
            Label(root,"TERRA",new Vector2(0,1),new Vector2(28,-22),new Vector2(290,42),34,ink,FontStyles.Bold);
            Label(root,"Meadowbrook Valley / Wisconsin",new Vector2(0,1),new Vector2(29,-67),new Vector2(320,25),17,ink);
            Label(root,"REAL GEOGRAPHY · hypothetical choices",new Vector2(0,1),new Vector2(29,-97),new Vector2(340,24),13,muted);
            Button(root,"Data & assumptions",new Vector2(0,1),new Vector2(28,-130),new Vector2(180,34),()=>{dataPage=0;ShowData();},16);
            var stats=Panel(root,"Measures",new Vector2(1,1),new Vector2(-26,-24),new Vector2(620,91),cream);
            Label(stats,"PRODUCTIVE CAPACITY",new Vector2(0,1),new Vector2(20,-13),new Vector2(205,20),12,muted,FontStyles.Bold);
            production=Label(stats,"—",new Vector2(0,1),new Vector2(20,-36),new Vector2(220,36),26,ink,FontStyles.Bold);
            Label(stats,"SAME-STORM RUNOFF",new Vector2(0,1),new Vector2(248,-13),new Vector2(210,20),12,muted,FontStyles.Bold);
            Label(stats,"Pending",new Vector2(0,1),new Vector2(248,-39),new Vector2(180,30),22,muted);
            Label(stats,"CREDITS LEFT",new Vector2(0,1),new Vector2(470,-13),new Vector2(140,20),12,muted,FontStyles.Bold);
            credits=Label(stats,"—",new Vector2(0,1),new Vector2(470,-36),new Vector2(130,36),28,ink,FontStyles.Bold);
            var hint=Panel(root,"Placement hint",new Vector2(.5f,0),new Vector2(0,185),new Vector2(590,34),new Color(.97f,.96f,.90f,.92f));
            tip=Label(hint,"Choose a treatment. Explore the outlined sites.",new Vector2(.5f,.5f),Vector2.zero,new Vector2(570,26),17,ink);tip.alignment=TextAlignmentOptions.Center;
            var dock=Panel(root,"Tool dock",new Vector2(.5f,0),new Vector2(0,24),new Vector2(730,116),cream);
            Label(dock,"SHAPE A PROPOSAL",new Vector2(0,1),new Vector2(18,-11),new Vector2(300,18),12,muted,FontStyles.Bold);
            cards=new UnityEngine.UI.Image[3];string[] names={"1   Cultivate\n<size=14>Make productive space</size>","2   Restore\n<size=14>Bring back meadow</size>","3   Manage water\n<size=14>Reserve a connected swale</size>"};string[] treatments={"cultivate-standard","restore-grass","water-swale"};
            for(int i=0;i<3;i++){string id=treatments[i];var b=Button(dock,names[i],new Vector2(0,1),new Vector2(16+i*236,-36),new Vector2(226,65),()=>choose(id),19);cards[i]=b.GetComponent<UnityEngine.UI.Image>();}
            standard=Button(root,"Open plot",new Vector2(.5f,0),new Vector2(-90,146),new Vector2(170,32),()=>choose("cultivate-standard"),15).GetComponent<UnityEngine.UI.Image>();
            margin=Button(root,"Living margin",new Vector2(.5f,0),new Vector2(90,146),new Vector2(170,32),()=>choose("cultivate-margin"),15).GetComponent<UnityEngine.UI.Image>();
            undoButton=Button(root,"Undo  [Z]",new Vector2(0,0),new Vector2(26,30),new Vector2(146,38),undo,16);
            Button(root,"Reset view  [R]",new Vector2(0,0),new Vector2(26,76),new Vector2(146,38),reset,15);
            Button(root,"Controls  [?]",new Vector2(1,0),new Vector2(-26,30),new Vector2(146,38),ShowHelp,16);
            Button(root,"Cancel  [Esc]",new Vector2(1,0),new Vector2(-26,76),new Vector2(146,38),cancel,15);
            tooltip=Panel(root,"Site preview",new Vector2(1,.5f),new Vector2(-26,-16),new Vector2(290,205),cream).gameObject;
            tooltipText=Label(tooltip.transform,"",new Vector2(0,1),new Vector2(18,-16),new Vector2(254,174),16,ink);tooltip.SetActive(false);
            toastPanel=Panel(root,"Action feedback",new Vector2(.5f,1),new Vector2(0,-128),new Vector2(620,48),cream).gameObject;toastPanel.SetActive(false);
            toast=Label(toastPanel.transform,"",new Vector2(.5f,.5f),Vector2.zero,new Vector2(590,44),17,ink);toast.alignment=TextAlignmentOptions.Center;
            var shade=Rect(root,"Modal backdrop",new Vector2(.5f,.5f),Vector2.zero,Vector2.zero);shade.anchorMin=Vector2.zero;shade.anchorMax=Vector2.one;shade.offsetMin=shade.offsetMax=Vector2.zero;
            shade.gameObject.AddComponent<UnityEngine.UI.Image>().color=new Color(.09f,.17f,.12f,.35f);modalShade=shade.gameObject;modalShade.SetActive(false);
            modal=Panel(root,"Information",new Vector2(.5f,.5f),Vector2.zero,new Vector2(700,540),cream).gameObject;
            modalText=Label(modal.transform,"",new Vector2(0,1),new Vector2(32,-26),new Vector2(636,378),17,ink);
            Button(modal.transform,"Back to landscape",new Vector2(.5f,0),new Vector2(0,24),new Vector2(240,42),CloseModal,17);
            motionButton=Button(modal.transform,"Motion: full",new Vector2(0,0),new Vector2(32,80),new Vector2(270,36),()=>{ReducedMotion=!ReducedMotion;SetButtonText(motionButton,ReducedMotion?"Motion: reduced":"Motion: full");},16);
            muteButton=Button(modal.transform,"Sound: on",new Vector2(1,0),new Vector2(-32,80),new Vector2(270,36),()=>{Muted=!Muted;SetButtonText(muteButton,Muted?"Sound: muted":"Sound: on");},16);
            dataPageButton=Button(modal.transform,"Sources & limits →",new Vector2(.5f,0),new Vector2(0,80),new Vector2(280,36),()=>{dataPage=1-dataPage;ShowData();},16);dataPageButton.gameObject.SetActive(false);
            modal.SetActive(false);
            loading=Panel(root,"Loading",new Vector2(.5f,.5f),Vector2.zero,new Vector2(520,230),cream).gameObject;
            loadText=Label(loading.transform,"<b>Preparing the landscape</b>\n\nLoading the local WorldPack…",new Vector2(0,1),new Vector2(30,-28),new Vector2(460,130),21,ink);
            Button(loading.transform,"Retry",new Vector2(.5f,0),new Vector2(0,24),new Vector2(160,36),retry,17).name="Retry loading";
        }
        public void Ready(){loading.SetActive(false);}
        public void Progress(string message){loadText.text="<b>Preparing Meadowbrook Valley</b>\n\n<size=18>"+message+"</size>";}
        public void Error(string message){loading.SetActive(true);loadText.text="<b>The landscape could not open</b>\n<size=16>"+message+"</size>";}
        public void UpdateResult(WorldResult r,bool canUndo,float target)
        {production.text=r.production.ToString("0.#")+" <size=14>/ "+target.ToString("0.#")+" units*</size>";credits.text=r.remainingCredits.ToString();undoButton.interactable=canUndo;}
        public void Selected(string id)
        {
            int index=id.StartsWith("cultivate")?0:id=="restore-grass"?1:id=="water-swale"?2:-1;
            for(int i=0;i<3;i++){cards[i].color=i==index?green:new Color(.88f,.90f,.81f);cards[i].GetComponentInChildren<TextMeshProUGUI>().color=i==index?cream:ink;}
            standard.gameObject.SetActive(index==0);margin.gameObject.SetActive(index==0);
            standard.color=id=="cultivate-standard"?new Color(.83f,.76f,.48f):cream;margin.color=id=="cultivate-margin"?new Color(.83f,.76f,.48f):cream;
            tip.text=index<0?"Placement cancelled. Choose a tool to start again.":"Hover / Tab: preview  ·  F: close view  ·  Click / Enter: place";
        }
        public void Preview(Site site,Treatment treatment,Evaluation result,WorldResult current)
        {
            tooltip.SetActive(site!=null);if(site==null)return;
            string header="<b>"+site.name+"</b>\n<size=13>"+(site.protectedSite?"PROTECTED LEARNING ZONE":"HYPOTHETICAL MANAGEMENT SITE")+"</size>\n\n";
            if(treatment==null){tooltipText.text=header+"Choose a tool to make a proposal.";return;}
            if(!result.valid&&result.reason=="This proposal is already placed."){tooltipText.text=header+"<b>Current proposal</b>\n"+treatment.title+"\nChoose another treatment or Undo to revise it.";return;}
            if(!result.valid){tooltipText.text=header+"<color=#AA402B>Unavailable</color>\n"+result.reason;return;}
            float delta=result.result.production-current.production;int cost=result.result.spentCredits-current.spentCredits;
            tooltipText.text=header+treatment.title+"  ·  "+cost+" credits\n"+(delta>=0?"+":"")+delta.ToString("0.#")+" illustrative units\n<size=14>Runoff: pending simulation</size>\n<b>Click or Enter to place</b>";
        }
        public void Toast(string text){toast.text=text;toastPanel.SetActive(!string.IsNullOrEmpty(text));}
        public void CloseModal(){modal.SetActive(false);modalShade.SetActive(false);}
        public void ShowHelp()=>ShowModal("Make room for a better balance","Select a tool, inspect a site, then click to place. Every proposal can be undone. *Production units are illustrative game accounting.\n\nDrag: orbit · Shift-drag: pan · Scroll / + / −: zoom\n1 / 2 / 3: tools · Tab: next site · Enter: place\nF: close view of a site · R: return to the valley\nM: living margin · Z: undo · Escape: cancel / close\n\nThe bright outline previews a site. A warm red outline and a reason mark an unavailable action.");
        void ShowData()
        {
            if(dataPage==0)ShowModal("Real land. Hypothetical choices.",
                "<b>Meadowbrook Valley</b> is our fictional game name for the Coon Valley / Brye Lane / Coon Creek area in Wisconsin.\n\n"+
                "<b>Editable WGS84 bounds</b>\n43.697–43.706° N · 91.043–91.031° W (about 0.97 km²)\n\n"+
                "Real elevation, broad land cover, stream and road lines shape this landscape. Trees, crop rows and line widths are decorative. Individual trees and field parcels were not surveyed.\n\n"+
                "Ten management sites and two no-edit zones are authored game assumptions, not ownership or legal designations. Costs, demand, production units, soil B, a 50 mm storm and swale capacity are fictional settings. Runoff is pending; no flood or yield forecast is shown.");
            else ShowModal("Sources, dates & limits",
                "<b>Terrain · USGS 3DEP</b>\nWI_8County_2020_A20, April–May 2020; published January 2024. Bare earth, NAVD88 metres; 1 m data simplified to a 10 m grid without vertical exaggeration.\n\n"+
                "<b>Cover · USGS / MRLC NLCD 2021</b>\n30 m classes, June 2023 release. Narrow margins are unresolved; simplified display groups do not measure ecological condition.\n\n"+
                "<b>Water & roads · USGS National Map</b>\nNHD flowlines and transportation layers 30–32, retrieved October 24, 2026. Feature dates vary; drawn widths are illustrative. Federal source data are public domain.\n\n"+
                "No parcel, structure, tree, ownership or local soil survey is included. This crop is not a whole watershed. No external inflow or downstream flood protection is modeled.");
            motionButton.gameObject.SetActive(false);muteButton.gameObject.SetActive(false);dataPageButton.gameObject.SetActive(true);
            SetButtonText(dataPageButton,dataPage==0?"Sources & limits →":"← Place & assumptions");
        }
        void ShowModal(string title,string body){tooltip.SetActive(false);modalText.text="<size=27><b>"+title+"</b></size>\n\n"+body;modalShade.SetActive(true);modal.SetActive(true);motionButton.gameObject.SetActive(true);muteButton.gameObject.SetActive(true);dataPageButton.gameObject.SetActive(false);}
        static void SetButtonText(UnityEngine.UI.Button b,string s)=>b.GetComponentInChildren<TextMeshProUGUI>().text=s;
        RectTransform Rect(Transform parent,string name,Vector2 anchor,Vector2 pos,Vector2 size)
        {var go=new GameObject(name,typeof(RectTransform));var r=go.GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=anchor;r.pivot=anchor;r.anchoredPosition=pos;r.sizeDelta=size;return r;}
        Transform Panel(Transform parent,string name,Vector2 anchor,Vector2 pos,Vector2 size,Color c)
        {var r=Rect(parent,name,anchor,pos,size);var img=r.gameObject.AddComponent<UnityEngine.UI.Image>();img.sprite=rounded;img.type=UnityEngine.UI.Image.Type.Sliced;img.color=c;return r;}
        TextMeshProUGUI Label(Transform parent,string text,Vector2 anchor,Vector2 pos,Vector2 size,float fontSize,Color c,FontStyles style=FontStyles.Normal)
        {var r=Rect(parent,"Label",anchor,pos,size);var label=r.gameObject.AddComponent<TextMeshProUGUI>();label.font=font;label.text=text;label.fontSize=fontSize;label.color=c;label.fontStyle=style;label.raycastTarget=false;label.textWrappingMode=TextWrappingModes.Normal;label.overflowMode=TextOverflowModes.Overflow;return label;}
        UnityEngine.UI.Button Button(Transform parent,string text,Vector2 anchor,Vector2 pos,Vector2 size,Action action,float textSize)
        {var panel=Panel(parent,"Button "+text,anchor,pos,size,new Color(.88f,.90f,.81f));var button=panel.gameObject.AddComponent<UnityEngine.UI.Button>();button.targetGraphic=panel.GetComponent<UnityEngine.UI.Image>();button.onClick.AddListener(()=>action());var t=Label(panel,text,new Vector2(.5f,.5f),Vector2.zero,size-new Vector2(12,4),textSize,ink);t.alignment=TextAlignmentOptions.Center;return button;}
        Sprite RoundSprite()
        {const int n=32;var t=new Texture2D(n,n,TextureFormat.RGBA32,false);for(int y=0;y<n;y++)for(int x=0;x<n;x++){float dx=Mathf.Max(8-x,x-23),dy=Mathf.Max(8-y,y-23);float d=Mathf.Sqrt(Mathf.Max(dx,0)*Mathf.Max(dx,0)+Mathf.Max(dy,0)*Mathf.Max(dy,0));t.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01(8.5f-d)));}t.Apply();return Sprite.Create(t,new Rect(0,0,n,n),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,new Vector4(9,9,9,9));}
    }
}
