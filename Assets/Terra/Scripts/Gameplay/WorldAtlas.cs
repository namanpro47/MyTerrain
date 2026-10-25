using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Terra.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Terra.Gameplay
{
    // TN-01/03/06, AX-01: the atlas is a Unity scene, including every control and transition.
    public sealed partial class WorldAtlas : MonoBehaviour
    {
        public static WorldAtlas Instance {get;private set;}
        public int Selected {get;private set;}
        public bool Visible {get;private set;}=true;
        public bool Traveling {get;private set;}
        public bool ReducedMotion {get;private set;}
        public string Diagnostic=>$"native-atlas visible={Visible} traveling={Traveling} selected={Selected} ready={landscapeReady} lat={latitude:F1} lon={longitude:F1}";
        TerraMain main;Camera globeCamera;Transform earth;GameObject geometry;Canvas canvas;
        CanvasGroup selection;UnityEngine.UI.Image veil;TextMeshProUGUI journeyTitle,journeyStatus;
        UnityEngine.UI.Button retry,back,motion;GameObject notes;RectTransform[] pins=new RectTransform[3];UnityEngine.UI.Button[] choices=new UnityEngine.UI.Button[3];
        TextMeshProUGUI region,nameLabel,description,coordinates;RectTransform uiRoot;
        readonly List<Object> owned=new List<Object>();
        readonly Vector2[] places={new Vector2(43.7015f,-91.037f),new Vector2(38.066f,-122.810f),new Vector2(42.18f,73.96f)};
        readonly string[] regions={"WISCONSIN · UNITED STATES","CALIFORNIA · UNITED STATES","KYRGYZSTAN · CENTRAL ASIA"};
        readonly string[] descriptions={"Wooded ridges, working fields and a winding creek.","Coastal grasslands at the edge of the Pacific.","An open mountain valley beneath the Tian Shan."};
        float latitude=25,longitude=-100,distance=4.65f,targetDistance=4.65f;Vector2 velocity,last,down;
        bool pointerHeld,dragged,landscapeReady,chosen,pointerOverUI;string failure;UnityEngine.UI.Button pressedButton;Coroutine journey;
        static readonly Color Cream=new Color(.98f,.96f,.86f),Mint=new Color(.52f,.85f,.76f),Muted=new Color(.63f,.76f,.76f),Navy=new Color(.025f,.065f,.09f),Gold=new Color(1f,.79f,.38f);
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] static extern void MyTerrainAtlasReady();
        [DllImport("__Internal")] static extern int TerraAtlasReducedMotion();
#endif
        public static WorldAtlas Attach(TerraMain owner)
        {
            if(Instance==null)new GameObject("MyTerrain · World atlas").AddComponent<WorldAtlas>();
            var atlas=Instance;atlas.main=owner;atlas.landscapeReady=false;atlas.failure=null;
            owner.SetAtlasVisible(1);
            if(atlas.chosen)
            {
                if(!atlas.Traveling&&!atlas.Visible)atlas.StartJourney();
                else owner.LoadLocation(LocationProfile.At(atlas.Selected));
            }
            return atlas;
        }
        void Awake()
        {
            Instance=this;DontDestroyOnLoad(gameObject);
            ReducedMotion=PlayerPrefs.GetInt("myterrain.reduced-motion",0)!=0;
#if UNITY_WEBGL && !UNITY_EDITOR
            if(!PlayerPrefs.HasKey("myterrain.reduced-motion"))ReducedMotion=TerraAtlasReducedMotion()!=0;
#endif
            BuildGlobe();BuildUI();Select(0,true);Show();
#if UNITY_WEBGL && !UNITY_EDITOR
            MyTerrainAtlasReady();
#endif
            Debug.Log("MYTERRAIN_NATIVE_ATLAS_READY");
        }
        public void Show()
        {
            notes?.SetActive(false);Visible=true;Traveling=false;velocity=Vector2.zero;pointerHeld=false;
            if(journey!=null){StopCoroutine(journey);journey=null;}
            geometry.SetActive(true);globeCamera.enabled=true;canvas.gameObject.SetActive(true);selection.alpha=1;selection.interactable=true;selection.blocksRaycasts=true;
            veil.color=new Color(Navy.r,Navy.g,Navy.b,0);veil.raycastTarget=false;journeyTitle.gameObject.SetActive(false);journeyStatus.gameObject.SetActive(false);back.gameObject.SetActive(false);retry.gameObject.SetActive(false);
            main?.SetAtlasVisible(1);targetDistance=4.65f;
        }
        public void Select(int index,bool snap=false)
        {
            if(notes!=null&&notes.activeSelf)return;
            if(!snap&&Selected!=(index+3)%3)Terra.Audio.TerraAudio.Ensure().Request("atlas-select",3);
            Selected=(index+3)%3;var place=LocationProfile.At(Selected);region.text=regions[Selected];nameLabel.text=place.Name;description.text=descriptions[Selected];
            var p=places[Selected];coordinates.text=$"{p.x:0.00}° N    {Mathf.Abs(p.y):0.00}° {(p.y<0?"W":"E")}";
            for(int i=0;i<3;i++){choices[i].image.color=i==Selected?new Color(.18f,.32f,.33f,.98f):new Color(.06f,.13f,.17f,.78f);choiceUnderline[i].enabled=i==Selected;}
            if(snap){latitude=p.x-12;longitude=p.y;ApplyGlobe();}else{velocity=Vector2.zero;focusTime=0;focusFrom=new Vector2(latitude,longitude);focusing=true;}
        }
        bool focusing;float focusTime;Vector2 focusFrom;
        public void StartJourney()
        {
            if(Traveling||notes.activeSelf)return;if(!Visible)Show();Traveling=true;Visible=true;chosen=true;failure=null;focusing=false;pointerHeld=false;velocity=Vector2.zero;
            Terra.Audio.TerraAudio.Ensure().Request("travel",6);journey=StartCoroutine(Travel());
        }
        IEnumerator Travel()
        {
            selection.interactable=false;selection.blocksRaycasts=false;back.gameObject.SetActive(true);journeyTitle.gameObject.SetActive(true);journeyStatus.gameObject.SetActive(true);
            journeyTitle.text=LocationProfile.At(Selected).Name;journeyStatus.text="Descending into your landscape…";
            float fromLat=latitude,fromLon=longitude,fromDistance=distance;var p=places[Selected];float duration=ReducedMotion?.25f:2.5f;
            for(float t=0;t<1;t+=Time.unscaledDeltaTime/duration)
            {
                float s=t*t*(3-2*t);latitude=Mathf.Lerp(fromLat,p.x,s);longitude=Mathf.LerpAngle(fromLon,p.y,s);distance=Mathf.Lerp(fromDistance,1.12f,s*s);selection.alpha=1-Mathf.Clamp01(t*2);SetVeil(Mathf.SmoothStep(0,1,Mathf.InverseLerp(.52f,1,t)));ApplyGlobe();yield return null;
            }
            SetVeil(1);selection.alpha=0;yield return null;
            main.EnterSelectedLocation(LocationProfile.At(Selected));
            while(!landscapeReady&&failure==null)yield return null;
            if(failure!=null){journeyStatus.text=failure;retry.gameObject.SetActive(true);yield break;}
            globeCamera.enabled=false;geometry.SetActive(false);main.BeginAtlasDescent();journeyStatus.text="Your next ten years begin here.";
            duration=ReducedMotion?.2f:1.6f;
            for(float t=0;t<1;t+=Time.unscaledDeltaTime/duration){SetVeil(1-Mathf.SmoothStep(0,1,t));main.AtlasDescent(t);yield return null;}
            main.AtlasDescent(1);SetVeil(0);Visible=false;Traveling=false;canvas.gameObject.SetActive(false);main.FinishAtlasDescent();journey=null;
            Terra.Audio.TerraAudio.Ensure().Request("arrival",7);Debug.Log("MYTERRAIN_ARRIVAL: "+LocationProfile.At(Selected).PackId);
        }
        public void BeginLoading(){landscapeReady=false;failure=null;}
        public void LandscapeReady(){landscapeReady=true;}
        public void LoadProgress(string message){if(Traveling)journeyStatus.text=message;}
        public void LoadFailed(string message){Terra.Audio.TerraAudio.Ensure().Request("blocked",9);failure="This landscape could not load. Please try again.\n"+message;}
        void Retry(){retry.gameObject.SetActive(false);failure=null;landscapeReady=false;Traveling=false;Visible=false;distance=targetDistance=4.65f;SceneManager.LoadScene(SceneManager.GetActiveScene().name);}
        void SetVeil(float alpha){veil.color=new Color(Navy.r,Navy.g,Navy.b,alpha);veil.raycastTarget=alpha>.98f;}
        void Update()
        {
            if(Terra.Audio.TerraAudioPanel.ConsumedInput)return;if(!Visible)return;var k=Keyboard.current;
            if(k!=null)
            {
                if(k.escapeKey.wasPressedThisFrame){if(notes.activeSelf){notes.SetActive(false);return;}if(Traveling)Show();}
                if(Traveling&&failure!=null&&k.enterKey.wasPressedThisFrame)Retry();
                if(!Traveling&&k.iKey.wasPressedThisFrame)notes.SetActive(!notes.activeSelf);
                if(!Traveling&&!notes.activeSelf)
                {
                    if(k.mKey.wasPressedThisFrame)motion.onClick.Invoke();
                    if(k.digit1Key.wasPressedThisFrame)Select(0);if(k.digit2Key.wasPressedThisFrame)Select(1);if(k.digit3Key.wasPressedThisFrame)Select(2);
                    if(k.tabKey.wasPressedThisFrame)Select(Selected+(k.shiftKey.isPressed?-1:1));
                    if(k.enterKey.wasPressedThisFrame)StartJourney();
                    Vector2 keys=new Vector2((k.rightArrowKey.isPressed?1:0)-(k.leftArrowKey.isPressed?1:0),(k.upArrowKey.isPressed?1:0)-(k.downArrowKey.isPressed?1:0));
                    if(keys.sqrMagnitude>0){focusing=false;longitude+=keys.x*Time.unscaledDeltaTime*45;latitude+=keys.y*Time.unscaledDeltaTime*35;}
                    if(k.equalsKey.isPressed)Zoom(-Time.unscaledDeltaTime*2);if(k.minusKey.isPressed)Zoom(Time.unscaledDeltaTime*2);if(k.homeKey.wasPressedThisFrame)Select(Selected);
                }
            }
#if !UNITY_WEBGL || UNITY_EDITOR
            var m=Mouse.current;if(m!=null){var pos=m.position.ReadValue();if(m.leftButton.wasPressedThisFrame)Pointer(0,pos);if(pos!=last)Pointer(1,pos);if(m.leftButton.wasReleasedThisFrame)Pointer(2,pos);if(m.scroll.ReadValue().y!=0)Zoom(-m.scroll.ReadValue().y/120);}
#endif
            if(!Traveling)
            {
                if(focusing){focusTime+=Time.unscaledDeltaTime/(ReducedMotion?.05f:1.2f);float t=Mathf.SmoothStep(0,1,focusTime);latitude=Mathf.Lerp(focusFrom.x,places[Selected].x-10,t);longitude=Mathf.LerpAngle(focusFrom.y,places[Selected].y,t);if(focusTime>=1)focusing=false;}
                else if(!pointerHeld&&!ReducedMotion){longitude+=velocity.x*Time.unscaledDeltaTime;latitude+=velocity.y*Time.unscaledDeltaTime;velocity*=Mathf.Exp(-Time.unscaledDeltaTime*5);}
                latitude=Mathf.Clamp(latitude,-75,75);distance=Mathf.Lerp(distance,targetDistance,1-Mathf.Exp(-Time.unscaledDeltaTime*8));ApplyGlobe();
            }
            PositionPins();AnimateScenery();
        }
        public void SetMotion(bool reduced)
        {
            ReducedMotion=reduced;PlayerPrefs.SetInt("myterrain.reduced-motion",reduced?1:0);PlayerPrefs.Save();
            motion.GetComponentInChildren<TextMeshProUGUI>().text=reduced?"Motion: reduced":"Motion: full";
        }
        public void Zoom(float amount){if(!Traveling&&!notes.activeSelf)targetDistance=Mathf.Clamp(targetDistance+amount*.25f,2.5f,5.5f);}
        UnityEngine.UI.Button ButtonAt(Vector2 point)
        {
            if(EventSystem.current==null)return null;var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=point},hits);
            return hits.Count>0?hits[0].gameObject.GetComponentInParent<UnityEngine.UI.Button>():null;
        }
        public void Pointer(int kind,Vector2 point)
        {
            if(kind==0){pressedButton=ButtonAt(point);pointerOverUI=notes.activeSelf;pointerHeld=true;dragged=false;down=last=point;velocity=Vector2.zero;return;}
            if(kind==3){pointerHeld=false;pressedButton=null;return;}
            if(kind==1){if(pointerHeld&&!pointerOverUI&&pressedButton==null&&!Traveling){var delta=point-last;if((point-down).magnitude>5)dragged=true;if(dragged){focusing=false;var rotation=new Vector2(-delta.x,-delta.y)*180/Screen.height;velocity=Vector2.ClampMagnitude(rotation/Mathf.Max(Time.unscaledDeltaTime,.008f),150);longitude+=rotation.x;latitude+=rotation.y;}}last=point;return;}
            if(kind==2){
#if UNITY_WEBGL && !UNITY_EDITOR
                if(pointerHeld&&!dragged&&(point-down).sqrMagnitude<36&&pressedButton!=null&&pressedButton==ButtonAt(point)&&pressedButton.IsInteractable())pressedButton.onClick.Invoke();
#endif
                pointerHeld=false;pressedButton=null;
            }
        }
        void ApplyGlobe(){globeCamera.aspect=(float)Screen.width/Mathf.Max(1,Screen.height);earth.rotation=Quaternion.AngleAxis(-latitude,Vector3.right)*Quaternion.AngleAxis(180+longitude,Vector3.up);globeCamera.transform.localPosition=new Vector3(-distance*globeCamera.aspect*.105f*Mathf.SmoothStep(0,1,Mathf.InverseLerp(1.12f,4.65f,distance)),-.23f*Mathf.SmoothStep(0,1,Mathf.InverseLerp(1.12f,4.65f,distance)),-distance);globeCamera.transform.localRotation=Quaternion.identity;}
        static Vector3 Geo(Vector2 p){float a=p.x*Mathf.Deg2Rad,b=p.y*Mathf.Deg2Rad;return new Vector3(-Mathf.Cos(a)*Mathf.Sin(b),Mathf.Sin(a),Mathf.Cos(a)*Mathf.Cos(b));}
        void PositionPins()
        {
            for(int i=0;i<3;i++){var world=earth.TransformPoint(Geo(places[i])*1.012f);var normal=(world-earth.position).normalized;bool front=Vector3.Dot(normal,(globeCamera.transform.position-world).normalized)>.08f;pins[i].gameObject.SetActive(front&&!Traveling);pins[i].GetComponent<UnityEngine.UI.Image>().color=i==Selected?new Color(1,.79f,.38f,.32f):new Color(.25f,.85f,.85f,.25f);pinLabels[i].transform.parent.gameObject.SetActive(i==Selected);if(front){Vector2 screen=globeCamera.WorldToScreenPoint(world);RectTransformUtility.ScreenPointToLocalPointInRectangle(uiRoot,screen,null,out var point);pins[i].anchoredPosition=point;pins[i].localScale=Vector3.one*(i==Selected&&!ReducedMotion?1+.06f*Mathf.Sin(Time.unscaledTime*2.5f):1);}}
        }
        void OnDestroy(){if(Instance==this)Instance=null;foreach(var item in owned)if(item!=null)Destroy(item);AudioListener.pause=false;}
    }
}
