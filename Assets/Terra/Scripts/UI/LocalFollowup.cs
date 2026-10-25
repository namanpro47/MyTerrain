using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using Terra.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Terra.UI
{
    // Figma Take it local, browser-inspected 2026-10-25. TN-06/08, AX-01.
    // One native overlay, shared by the atlas and final results. No network dependency.
    public sealed class LocalFollowup : MonoBehaviour
    {
        const string SaveKey = "myterrain.local-action.v1";
        static LocalFollowup instance;
        public static bool IsOpen => instance != null && instance.canvas != null && instance.canvas.gameObject.activeSelf;
        public static bool ConsumedInput => IsOpen || (instance != null && instance.closedFrame == Time.frameCount);
        static readonly Color Paper = Hex("F2F4EF"), Ink = Hex("14201A"), Muted = Hex("4E5A53"), Line = Hex("CBD3C8"), Green = Hex("1F7A4D");
        static readonly Color[] Colors = {Hex("2457C5"),Hex("087285"),Hex("A5501A"),Hex("6E44B8")};
        static readonly Color[] Tints = {Hex("E3EAFA"),Hex("DDF1F3"),Hex("F8E6DA"),Hex("ECE5F7")};
        static readonly string[] Levels = {"Learn", "Habit", "Speak up", "Organize"};
        static readonly string[] Habits = {"walk, cycle or use transit for one suitable trip", "notice what a local trail or park needs", "buy from a locally owned shop", "find a local volunteer opportunity", "plan meals to reduce food waste", "visit a library or community space"};
        Canvas canvas; RectTransform stage; Sprite rounded; Texture2D roundTexture;
        readonly List<Selectable> controls = new List<Selectable>();
        LocalActionPlan plan; Action onClose; string returnLabel, city = "", gamePlace, gameSummary, draft, group = "Friends";
        double[] scores; bool hasResult, showScores = true, showAction = true, showCity, optIn, sendScreen;
        int step, selectedResource, focusIndex, openedFrame, closedFrame = -1; Selectable pressed;
        Vector2 pressPoint; TextMeshProUGUI feedback; string notice = "";
        TMP_InputField cityInput, whenInput, draftInput;
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] static extern void LocalFollowupDownload(string name, string mime, string content);
        [DllImport("__Internal")] static extern void LocalFollowupCopy(string content);
#endif
        public static void Show(string place, string summary, double[] result, string backLabel, Action close)
        {
            Terra.Gameplay.WorldAtlas.Instance?.Pointer(3,Vector2.zero);
            if(instance==null) new GameObject("Take it local").AddComponent<LocalFollowup>();
            var p=instance; p.gamePlace=place; p.gameSummary=summary; p.scores=result; p.hasResult=result!=null;
            p.returnLabel=backLabel; p.onClose=close; p.city=""; p.draft=null; p.showCity=false; p.showScores=p.showAction=true; p.notice=""; p.sendScreen=false;
            p.plan=ReadPlan(); p.optIn=p.plan.reminder; p.step=p.plan.Valid?6:0; p.openedFrame=Time.frameCount;
            if(p.canvas==null)p.Build(); p.canvas.gameObject.SetActive(true); p.Draw();
        }
        void Awake(){instance=this;}
        public static bool CheckInDue => ReadPlan().Due(DateTime.UtcNow);
        static LocalActionPlan ReadPlan()
        {
            try{var p=JsonUtility.FromJson<LocalActionPlan>(PlayerPrefs.GetString(SaveKey,""));if(p==null||!p.Valid)return new LocalActionPlan();if(!DateTime.TryParse(p.checkInUtc,CultureInfo.InvariantCulture,DateTimeStyles.RoundtripKind,out _))p.reminder=false;return p;}
            catch{return new LocalActionPlan();}
        }
        void Persist(){PlayerPrefs.SetString(SaveKey,JsonUtility.ToJson(plan));PlayerPrefs.Save();}
        void Close()
        {
            canvas.gameObject.SetActive(false);closedFrame=Time.frameCount;pressed=null;
            EventSystem.current?.SetSelectedGameObject(null);var callback=onClose;onClose=null;callback?.Invoke();
        }
        void Build()
        {
            var go=new GameObject("Take it local interface",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));go.transform.SetParent(transform,false);
            canvas=go.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=900;
            var scaler=go.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
            var bg=go.AddComponent<Image>();bg.color=Paper;
            stage=Box(go.transform,"Follow-up content",0,0,1920,1080);stage.anchorMin=stage.anchorMax=stage.pivot=Vector2.one*.5f;stage.anchoredPosition=Vector2.zero;
            roundTexture=new Texture2D(64,64,TextureFormat.RGBA32,false);
            for(int y=0;y<64;y++)for(int x=0;x<64;x++){float dx=Mathf.Max(Mathf.Abs(x-31.5f)-15.5f,0),dy=Mathf.Max(Mathf.Abs(y-31.5f)-15.5f,0);roundTexture.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01(16-Mathf.Sqrt(dx*dx+dy*dy))));}
            roundTexture.Apply();rounded=Sprite.Create(roundTexture,new Rect(0,0,64,64),Vector2.one*.5f,100,0,SpriteMeshType.FullRect,new Vector4(17,17,17,17));
        }
        void Draw()
        {
            foreach(Transform child in stage){child.gameObject.SetActive(false);Destroy(child.gameObject);}controls.Clear();cityInput=whenInput=draftInput=null;
            Button(stage,step==0||step==6?"← "+returnLabel:"← Back",64,24,200,64,Back,false);
            Text(stage,"Take it local",300,25,350,60,30,Ink,true);
            string[] names={"Where","Level","Action","Plan","Share","Check-in"};int current=Mathf.Min(step,5);
            for(int i=0;i<6;i++)
            {
                if(i<current)Icon(stage,"check",1000+i*140,42,26,Green);
                else Text(stage,(i+1).ToString(),1000+i*140,35,28,48,21,i==current?Green:Muted,i==current);
                Text(stage,names[i],1034+i*140,35,104,48,21,i<=current?Green:Muted,i==current);
            }
            Panel(stage,"Header rule",0,110,1920,2,Line,false);
            switch(step){case 0:Where();break;case 1:Level();break;case 2:ActionPage();break;case 3:Plan();break;case 4:Share();break;case 5:CheckIn();break;default:Saved();break;}
            feedback=Text(stage,notice,96,1010,1728,40,21,Muted);focusIndex=0;Focus(0);
        }
        void Go(int next){step=next;notice="";Draw();}
        void Back()
        {
            if(step==0||step==6){Close();return;}if(step==2&&sendScreen){sendScreen=false;Draw();return;}
            Go(step-1);
        }
        void Heading(string title,string subtitle){Text(stage,title,96,160,1728,100,60,Ink,true);Text(stage,subtitle,96,270,1728,65,28,Muted);}
        void Where()
        {
            Text(stage,"OPTIONAL · A SMALL NEXT STEP",96,185,1040,40,23,Muted,true);
            Text(stage,hasResult?"Your town was a test run.\nNow try the real one.":"Small steps.\nReal places. Real people.",96,245,1040,190,76,Ink,true);
            Text(stage,"Find people and resources to help you learn, build a habit, ask a question or bring others together.",96,460,1000,115,31,Muted);
            Text(stage,"City or town · optional",96,595,900,38,25,Ink,true);
            cityInput=Input(stage,"Your city or town",city,96,645,920,76,80,false,s=>city=s);
            Button(stage,"Continue",96,765,240,80,()=>{city=cityInput.text.Trim();Go(1);},true);
            Button(stage,"Skip — keep it general",360,765,480,80,()=>{city="";Go(1);},false);
            Text(stage,"Your city stays in this session. A search opens only when you choose it.\nNo account needed. Your plan can be saved on this device.",96,895,1010,85,24,Muted);
            var card=Panel(stage,"Game context",1220,190,600,640,Color.white);
            Text(card,hasResult?"FROM YOUR GAME":"START ANYTIME",44,44,512,40,23,Muted,true);
            Text(card,hasResult?gamePlace:"You don't need to finish a game",44,104,512,100,43,Ink,true);
            Text(card,hasResult?gameSummary:"Explore at your own pace. Every level counts, and you can return from the home screen.",44,220,512,120,28,Ink);
            if(hasResult)
            {
                string[] labels={"Economic","Social","Environmental"};
                for(int i=0;i<3;i++){float y=360+i*74;Text(card,labels[i],44,y,350,40,25,Ink,true);Text(card,scores[i].ToString("0.0",CultureInfo.InvariantCulture)+" / 100",410,y,150,40,25,Green,true);Panel(card,"Indicator track",44,y+45,512,10,Line);Panel(card,"Indicator",44,y+45,Mathf.Max(1,512*Mathf.Clamp01((float)scores[i]/100)),10,Green);}
            }
            else Text(card,"Learn · Habit · Speak up · Organize",44,450,512,100,28,Green,true);
            Text(stage,hasResult?"Saved game indicators · educational scenarios, not forecasts.":"Optional throughout. Leave whenever you want.",1220,870,600,95,23,Muted);
        }
        void Level()
        {
            Heading("How far do you want to take it?","Every level counts. Pick one — you can come back for another.");
            string[] copy={"Understand a local plan and find someone who can help explain it.","Choose one small change tied to how land and shared spaces are used.","Prepare your own question for a planning team or public meeting.","Bring friends, a club or a community group into the conversation."};
            string[] time={"~5 min","~2 min","~10 min","~15 min"},symbols={"guide","redo","more","people"};
            for(int i=0;i<4;i++)
            {
                int index=i;float x=96+i*440;var card=Panel(stage,Levels[i],x,370,408,570,Color.white);
                Panel(card,"Path color",2,2,404,160,Tints[i]);var glyph=Box(card,"Path icon",38,38,72,72).gameObject.AddComponent<HudGlyph>();glyph.symbol=symbols[i];glyph.color=Colors[i];glyph.raycastTarget=false;
                Text(card,time[i],260,42,120,40,24,Colors[i],true);Text(card,Levels[i],36,198,336,60,48,Ink,true);Text(card,copy[i],36,280,336,170,28,Ink);
                Button(card,"Choose "+Levels[i],28,465,352,76,()=>{plan.level=index;selectedResource=0;sendScreen=false;Go(2);},true,Colors[i]);
            }
        }
        bool Marin => city.IndexOf("point reyes",StringComparison.OrdinalIgnoreCase)>=0||city.Equals("94956",StringComparison.OrdinalIgnoreCase)||city.Equals("Marin",StringComparison.OrdinalIgnoreCase);
        string Search(string topic)=>"https://www.google.com/search?q="+Uri.EscapeDataString((string.IsNullOrWhiteSpace(city)?"my local council":city)+" "+topic);
        void Open(string url){Application.OpenURL(url);Tell("Opened in your browser. Come back here when you're ready.");}
        void ChooseAction(string text){plan.action=text;Go(3);}
        void ActionPage()
        {
            if(plan.level==0){Learn();return;}if(plan.level==1){Habit();return;}if(plan.level==2){Speak();return;}Organize();
        }
        void Learn()
        {
            Heading(Marin?"Find help around Point Reyes":"Find help with your local plan","Start with an official source. Check its date, jurisdiction and current opportunities to participate.");
            string[] titles={"Find the plan and the people behind it","Ask about a project or proposal","Explore parks and shared spaces"};
            string[] bodies={"Look for the official planning department, adopted land-use plan and public meeting calendar. Ask staff which plan covers the place you care about.","Ask what need a proposal meets, who it affects, what resources it uses and who pays ongoing costs. Request the underlying evidence and the current public-comment process.","Check the relevant park or local government's current projects and volunteer opportunities. Confirm access, dates and eligibility with the organizer."};
            for(int i=0;i<3;i++){int index=i;Button(stage,titles[i],96,375+i*155,590,125,()=>{selectedResource=index;Draw();},selectedResource==i,Colors[0]);}
            var detail=Panel(stage,"Resource details",735,375,1088,540,Color.white);
            Text(detail,"IN PLAIN WORDS",48,35,992,40,23,Colors[0],true);Text(detail,titles[selectedResource],48,100,992,100,42,Ink,true);Text(detail,bodies[selectedResource],48,225,992,180,30,Ink);
            string url=Marin&&selectedResource<2?"https://www.marincounty.gov/departments/cda/planning":selectedResource==2&&Marin?"https://www.nps.gov/subjects/parkplanning/get-involved.htm":Search(selectedResource==2?"parks volunteer official":"planning department public meetings official");
            Button(detail,Marin?"Open official guidance ↗":"Find official guidance ↗",48,425,480,76,()=>Open(url),false);
            Button(detail,"Make it a plan",552,425,488,76,()=>ChooseAction("read a local plan and ask the planning team one question"),true);
            Text(stage,"Source links are starting points, not a live list of open consultations. No game score is a prediction of local impact.",96,945,1728,50,23,Muted);
        }
        void Habit()
        {
            Heading("Pick one change you'll actually do","Small, repeatable, and connected to how land is used. Choose what fits your circumstances.");
            string[] titles={"Make one trip differently","Notice what a park needs","Support a local shop","Find a volunteer day","Reduce food waste","Use a community space"};
            for(int i=0;i<6;i++){int index=i;float x=96+(i%3)*586,y=365+(i/3)*270;var card=Panel(stage,titles[i],x,y,554,240,Color.white);Text(card,titles[i],32,26,490,60,34,Ink,true);Text(card,Habits[i]+".",32,95,490,65,27,Muted);Button(card,"Make it a plan",30,165,494,60,()=>ChooseAction(Habits[index]),true,Colors[1]);}
        }
        void Speak()
        {
            if(sendScreen){Send();return;}
            Heading("Start a conversation. Make it yours.","A question is a useful first step. Your game is an illustration, not evidence about a real proposal.");
            if(string.IsNullOrEmpty(draft))draft="Hello,\n\nI would like to understand [project or plan] in [place]. What need does it meet, who could be affected, and what evidence supports the proposal?\n\nCould you point me to the current plan, the next public meeting and the right way to share a question or comment?\n\nMy main question is [add your question]. Thank you.";
            draftInput=Input(stage,"Your question or comment",draft,96,385,1100,475,3000,true,s=>draft=s);
            var card=Panel(stage,"Writing help",1250,385,570,350,Color.white);Text(card,"What makes it useful",38,32,494,60,36,Ink,true);Text(card,"Name the project.\n\nAsk one clear question.\n\nReplace words in [brackets].\n\nInclude only details you want to share.",38,110,494,220,28,Ink);
            Button(stage,"Looks good — next",1250,780,570,80,()=>{draft=draftInput.text;sendScreen=true;Draw();},true);
            Text(stage,"Nothing is sent from the game. You review and submit it yourself, if you choose.",96,910,1700,70,25,Muted);
        }
        void Send()
        {
            Heading("Find the right people to ask","Check the official page for the recipient, current agenda and any deadline before submitting.");
            string[] title={"Copy your draft","Find the planning team","Review, then send"},body={"Keep a copy to edit and use in your own email or comment.","Find official contact details and current ways to participate.","Public comments may become public records. Remove details you do not want shared."};
            for(int i=0;i<3;i++){var card=Panel(stage,title[i],96+i*586,385,554,390,Color.white);Text(card,(i+1).ToString(),36,28,90,70,46,Colors[2],true);Text(card,title[i],36,115,482,65,38,Ink,true);Text(card,body[i],36,198,482,140,29,Ink);}
            Button(stage,"Copy draft",132,800,482,80,()=>Copy(draft),true);
            Button(stage,"Find official contact ↗",718,800,482,80,()=>Open(Marin?"https://www.marincounty.gov/departments/cda/planning":Search("planning department contact official")),false);
            Button(stage,"Plan my next step",1304,800,482,80,()=>ChooseAction("ask the local planning team my question"),true);
            Text(stage,"Drafts last for this visit only. Copy yours before leaving; only your next-step plan is saved.",96,930,1728,60,25,Muted);
        }
        void Organize()
        {
            Heading("Run it with your crew","Bring people together, compare ideas, then decide what you'd like to learn next.");
            string[] groups={"Friends","Club or team","Community group","Coworkers","Class"};
            for(int i=0;i<groups.Length;i++){string g=groups[i];Button(stage,g,96+i*235,365,215,72,()=>{group=g;Draw();},group==g,Colors[3]);}
            var kit=Panel(stage,"Discussion kit",96,480,1070,380,Color.white);
            Text(kit,"A simple 30-minute session",40,30,990,70,44,Ink,true);
            Text(kit,"5 min · Pick a place and a question.\n10 min · Play MyTerrain and compare choices.\n10 min · Who benefits? What are the trade-offs?\n5 min · Find an official source and choose one next step.",40,130,990,220,31,Ink);
            var invite=Panel(stage,"Invite preview",1220,365,600,560,Colors[3]);Text(invite,group.ToUpperInvariant()+" · GAME & DISCUSSION",44,44,512,60,24,Color.white,true);Text(invite,"How should our community use its land?",44,140,512,220,56,Color.white,true);Text(invite,"Play MyTerrain together, then explore one real local question.\n\n[Add a date and place]",44,390,512,140,29,Color.white);
            Button(stage,"Copy invitation",96,900,450,76,()=>Copy(Invite()),true);
            Button(stage,"Make it a plan",574,900,592,76,()=>ChooseAction("invite a group to play and discuss one local land-use question"),true);
        }
        string Invite()=>"How should our community use its land?\nJoin me for a 30-minute MyTerrain game and discussion.\n5 minutes: pick a question. 10: play and compare. 10: discuss benefits and trade-offs. 5: find an official source.\nDate and place: [add yours].\n"+GameUrl();
        void Plan()
        {
            Heading("Lock in when it happens","Tie it to a moment you already have. A small, specific step is easier to follow through on.");
            var card=Panel(stage,"Your plan",96,375,1728,235,Hex("DDEFE3"));Text(card,"YOUR PLAN",48,28,1632,45,24,Green,true);Text(card,plan.Sentence,48,95,1632,112,43,Ink,true);
            Text(stage,"Pick your “when”",96,655,1000,40,27,Ink,true);
            string[] when={"after my next meal","during my next lunch break","this weekend"};
            for(int i=0;i<when.Length;i++){string value=when[i];Button(stage,value,96+i*375,715,350,70,()=>{plan.when=value;Draw();},plan.when==value,Green);}
            whenInput=Input(stage,"Or write your own moment",plan.when,96,815,1095,76,200,false,s=>plan.when=s);
            Button(stage,"Lock in my plan",1260,805,564,86,()=>{plan.when=whenInput.text.Trim();if(!plan.Valid){Tell("Add a moment to attach your plan to.");return;}plan.SaveAt(DateTime.UtcNow,false);Persist();Go(4);},true,Green);
            Text(stage,"Saved only on this device. You can return from the home screen and change your plan.",96,930,1728,60,25,Muted);
        }
        string ShareText()
        {
            var text="MyTerrain · My next step";
            if(showScores&&hasResult)text+="\nGame indicators (not real-world impacts): Economic "+scores[0].ToString("0.0")+", Social "+scores[1].ToString("0.0")+", Environmental "+scores[2].ToString("0.0")+" / 100.";
            if(showAction)text+="\nI plan to "+plan.action+".";
            if(showCity&&!string.IsNullOrWhiteSpace(city))text+="\nPlace: "+city;
            return text+"\n"+GameUrl();
        }
        static string GameUrl()=>Application.absoluteURL.StartsWith("https://",StringComparison.OrdinalIgnoreCase)?Application.absoluteURL.Split('?','#')[0]:"MyTerrain";
        void Share()
        {
            var card=Panel(stage,"Share preview",96,190,680,740,Ink);Text(card,"MY NEXT STEP",48,45,584,45,25,Hex("A6D5BB"),true);Text(card,ShareText(),48,140,584,550,36,Color.white,true);
            Text(stage,"Want to share it?",870,260,950,95,64,Ink,true);Text(stage,"Totally optional. You choose what to include.\nNothing is posted automatically.",870,375,950,110,30,Muted);
            if(hasResult)Button(stage,(showScores?"✓ ":"○ ")+"Include game indicators",870,525,950,80,()=>{showScores=!showScores;Draw();},false);
            Button(stage,(showAction?"✓ ":"○ ")+"Include my next step",870,630,950,80,()=>{showAction=!showAction;Draw();},false);
            Button(stage,(showCity?"✓ ":"○ ")+"Include my city · off by default",870,735,950,80,()=>{showCity=!showCity;Draw();},false);
            Button(stage,"Copy share text",870,870,460,80,()=>Copy(ShareText()),false);Button(stage,"Continue / skip",1360,870,460,80,()=>Go(5),true);
        }
        void CheckIn()
        {
            Heading("Check back in two weeks?","One question: how did your plan go? Your plan stays saved here even if you skip the reminder.");
            Button(stage,(optIn?"✓ ":"")+"Yes — remind me in the game",96,390,840,145,()=>{optIn=true;Draw();},optIn,Green);
            Button(stage,(!optIn?"✓ ":"")+"No reminder",966,390,858,145,()=>{optIn=false;Draw();},!optIn,Green);
            Text(stage,"When you return to Take it local after "+DateTime.Today.AddDays(14).ToString("MMM d, yyyy")+", your plan will be ready to review. No email or background notification is sent.",96,595,1728,110,31,Muted);
            Button(stage,"Download calendar check-in",96,765,650,80,()=>Download("myterrain-check-in.ics","text/calendar;charset=utf-8",plan.Calendar(DateTime.UtcNow)),false);
            Button(stage,"Finish",96,900,300,80,()=>{plan.SaveAt(DateTime.UtcNow,optIn);Persist();Go(6);},true);
        }
        void Saved()
        {
            bool due=plan.Due(DateTime.UtcNow);Heading(due?"How did your plan go?":plan.completed?"One step taken. Keep going.":"Your next step is ready.","You can return here anytime from the home screen. Your game and your plan are saved separately.");
            var card=Panel(stage,"Saved plan",96,380,1728,260,Hex("DDEFE3"));Text(card,Levels[plan.level].ToUpperInvariant(),48,30,1632,40,24,Green,true);Text(card,plan.Sentence,48,100,1632,130,45,Ink,true);
            Button(stage,"I did it",96,720,460,85,()=>{plan.completed=true;plan.reminder=false;Persist();Draw();},true,Green);
            Button(stage,"Adjust my plan",590,720,550,85,()=>Go(3),false);Button(stage,"Explore another step",1174,720,650,85,()=>Go(1),false);
            Button(stage,"Back to "+returnLabel.ToLowerInvariant(),96,880,560,80,Close,true);
            Text(stage,plan.reminder?"Check-in set for "+DateTime.Parse(plan.checkInUtc,CultureInfo.InvariantCulture,DateTimeStyles.RoundtripKind).ToLocalTime().ToString("MMM d, yyyy")+" · in-game only":"No reminder set. Come back whenever it works for you.",700,900,1124,70,26,Muted);
        }
        void Tell(string message){notice=message;if(feedback!=null)feedback.text=message;}
        void Copy(string text)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            LocalFollowupCopy(text);Tell("Copy requested. If blocked, your browser shows selectable text to copy.");
#else
            GUIUtility.systemCopyBuffer=text;Tell("Copied. Paste it wherever you choose; nothing was sent.");
#endif
        }
        void Download(string name,string mime,string text)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            LocalFollowupDownload(name,mime,text);Tell("Calendar file requested. Open it to review and add the check-in.");
#else
            string path=Path.Combine(Application.persistentDataPath,name);File.WriteAllText(path,text);Tell("Calendar saved: "+path);
#endif
        }
        void Update()
        {
            if(!IsOpen||openedFrame==Time.frameCount)return;var k=Keyboard.current;
#if UNITY_WEBGL && !UNITY_EDITOR
            if(EventSystem.current?.currentSelectedGameObject!=null)ExecuteEvents.Execute(EventSystem.current.currentSelectedGameObject,new BaseEventData(EventSystem.current),ExecuteEvents.updateSelectedHandler);
#endif
            if(k!=null&&k.escapeKey.wasPressedThisFrame){Back();return;}
            var selected=EventSystem.current?.currentSelectedGameObject?.GetComponent<TMP_InputField>();
            if(k!=null&&k.tabKey.wasPressedThisFrame){selected?.DeactivateInputField();Focus(focusIndex+(k.shiftKey.isPressed?-1:1));return;}
            if(selected!=null&&selected.isFocused)return;
            var pad=Gamepad.current;
            if(k?.rightArrowKey.wasPressedThisFrame==true||k?.downArrowKey.wasPressedThisFrame==true||pad?.dpad.down.wasPressedThisFrame==true||pad?.dpad.right.wasPressedThisFrame==true)Focus(focusIndex+1);
            if(k?.leftArrowKey.wasPressedThisFrame==true||k?.upArrowKey.wasPressedThisFrame==true||pad?.dpad.up.wasPressedThisFrame==true||pad?.dpad.left.wasPressedThisFrame==true)Focus(focusIndex-1);
            if(pad?.buttonEast.wasPressedThisFrame==true){Back();return;}
            if(k?.enterKey.wasPressedThisFrame==true||k?.spaceKey.wasPressedThisFrame==true||pad?.buttonSouth.wasPressedThisFrame==true)
            {if(controls[focusIndex] is Button b)b.onClick.Invoke();else if(controls[focusIndex] is TMP_InputField input)input.ActivateInputField();}
        }
        void Focus(int index)
        {
            if(controls.Count==0)return;focusIndex=(index+controls.Count)%controls.Count;
            for(int i=0;i<controls.Count;i++)controls[i].GetComponent<Outline>().enabled=i==focusIndex;
            EventSystem.current?.SetSelectedGameObject(controls[focusIndex].gameObject);
        }
        public static void Pointer(int kind,Vector2 position)
        {
            if(!IsOpen)return;var p=instance;var hits=new List<RaycastResult>();var data=new PointerEventData(EventSystem.current){position=position};EventSystem.current?.RaycastAll(data,hits);
            var target=hits.Count>0?hits[0].gameObject.GetComponentInParent<Selectable>():null;
            if(kind==0){p.pressed=target;p.pressPoint=position;if(target!=null&&p.controls.Contains(target))p.Focus(p.controls.IndexOf(target));}
#if UNITY_WEBGL && !UNITY_EDITOR
            if(kind==2&&target!=null&&target==p.pressed&&Vector2.Distance(position,p.pressPoint)<8)
            {if(target is Button b&&b.IsInteractable())b.onClick.Invoke();else if(target is TMP_InputField input){input.OnPointerClick(data);input.ActivateInputField();}}
#endif
            if(kind==2||kind==3)p.pressed=null;
        }
        static Color Hex(string value){ColorUtility.TryParseHtmlString("#"+value,out var c);return c;}
        RectTransform Box(Transform parent,string name,float x,float y,float w,float h)
        {var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);return r;}
        RectTransform Panel(Transform parent,string name,float x,float y,float w,float h,Color color,bool round=true)
        {var r=Box(parent,name,x,y,w,h);var image=r.gameObject.AddComponent<Image>();image.color=color;if(round){image.sprite=rounded;image.type=Image.Type.Sliced;}return r;}
        TextMeshProUGUI Text(Transform parent,string value,float x,float y,float w,float h,float size,Color color,bool bold=false)
        {var r=Box(parent,"Label",x,y,w,h);var t=r.gameObject.AddComponent<TextMeshProUGUI>();t.font=TMP_Settings.defaultFontAsset;t.text=value;t.fontSize=size;t.color=color;t.fontStyle=bold?FontStyles.Bold:FontStyles.Normal;t.raycastTarget=false;t.richText=false;t.textWrappingMode=TextWrappingModes.Normal;return t;}
        Button Button(Transform parent,string title,float x,float y,float w,float h,Action action,bool primary,Color? accent=null)
        {
            string mark=null;
            if(title.StartsWith("✓ ")){mark="check";title=title.Substring(2);}
            else if(title.StartsWith("○ ")){mark="circle";title=title.Substring(2);}
            else if(title.StartsWith("← ")){mark="back";title=title.Substring(2);}
            else if(title.EndsWith(" ↗")){mark="external";title=title.Substring(0,title.Length-2);}
            var r=Panel(parent,title,x,y,w,h,primary?(accent??Ink):Color.white);var b=r.gameObject.AddComponent<Button>();b.targetGraphic=r.GetComponent<Image>();
            var outline=r.gameObject.AddComponent<Outline>();outline.effectColor=Hex("2457C5");outline.effectDistance=new Vector2(3,-3);outline.enabled=false;
            if(mark!=null)Icon(r,mark,22,(h-30)*.5f,30,primary?Color.white:Ink);
            var label=Text(r,title,mark==null?18:66,12,w-(mark==null?36:84),h-24,27,primary?Color.white:Ink,true);label.alignment=TextAlignmentOptions.Center;
            b.navigation=new Navigation{mode=Navigation.Mode.None};b.onClick.AddListener(()=>{Terra.Audio.TerraAudio.Ensure().Click();action();});controls.Add(b);return b;
        }
        void Icon(Transform parent,string symbol,float x,float y,float size,Color color)
        {
            var glyph=Box(parent,"Icon · "+symbol,x,y,size,size).gameObject.AddComponent<HudGlyph>();
            glyph.symbol=symbol;glyph.color=color;glyph.raycastTarget=false;
        }
        TMP_InputField Input(Transform parent,string hint,string value,float x,float y,float w,float h,int limit,bool multi,Action<string> changed)
        {
            var r=Panel(parent,hint,x,y,w,h,Color.white);var field=r.gameObject.AddComponent<TMP_InputField>();field.targetGraphic=r.GetComponent<Image>();field.characterLimit=limit;
            var viewport=Box(r,"Input viewport",24,18,w-48,h-36);viewport.gameObject.AddComponent<RectMask2D>();
            var text=Text(viewport,"",0,0,w-48,h-36,30,Ink);text.raycastTarget=true;var placeholder=Text(viewport,hint,0,0,w-48,h-36,30,Muted);
            field.textViewport=viewport;field.textComponent=text;field.placeholder=placeholder;field.lineType=multi?TMP_InputField.LineType.MultiLineNewline:TMP_InputField.LineType.SingleLine;field.text=value;
            field.onValueChanged.AddListener(s=>changed(s));var outline=r.gameObject.AddComponent<Outline>();outline.effectColor=Colors[0];outline.effectDistance=new Vector2(3,-3);outline.enabled=false;controls.Add(field);return field;
        }
        void OnDestroy(){if(instance==this)instance=null;if(rounded!=null)Destroy(rounded);if(roundTexture!=null)Destroy(roundTexture);}
    }
}
