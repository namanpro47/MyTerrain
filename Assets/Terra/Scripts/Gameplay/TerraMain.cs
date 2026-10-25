using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DG.Tweening;
using Terra.Contracts;
using Terra.Core;
using Terra.Geography;
using Terra.Rendering;
using Terra.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
namespace Terra.Gameplay
{
    public sealed partial class TerraMain : MonoBehaviour
    {
        public Material terrainMaterial;public GameObject treePrefab,cropPrefab,grassPrefab,shrubPrefab;public AudioClip placementSound;
        public Terra.Art.BuildActionCatalog buildActionCatalog;
        BuildActionRules buildActionRules;
        RunoffConfiguration runoffConfiguration;LandscapeRunoff runoff;
        Camera view;WorldData world;Scenario scenario;LandscapeRenderer landscape;TerraHud hud;AudioSource sound;
        string treatmentId="cultivate-standard",hoverId,selectedId;float yaw=-32,pitch=37,zoom=115;Vector3 target=new Vector3(-193,25,-43);
        bool perspective=true;
        UnityEngine.UI.Button uiDown;
        bool ready,pressed,dragged,overUi,committing,panGesture;Vector2 down,last;float toastUntil;
        readonly BrowserPerformance performance=new BrowserPerformance();
        float shownYaw=-32,shownPitch=37,shownZoom=115;Vector3 shownTarget=new Vector3(-193,25,-43);
        public int Revision=>annual?.State.revision??-1;
        public string Diagnostic=>annual==null?"loading":$"year={annual.State.year}; phase={annual.State.phase}; revision={Revision}; cash={annual.State.cashCents}; queued={annual.State.queue.Length}; assets={annual.State.assets.Length}";
        void Start()
        {
            gameObject.name="TerraMain";Application.runInBackground=true;
            view=Camera.main;view.orthographic=false;view.fieldOfView=36;view.nearClipPlane=1;view.farClipPlane=6000;view.clearFlags=CameraClearFlags.SolidColor;view.backgroundColor=new Color(.78f,.83f,.75f);ApplyView();
            RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor=new Color(.41f,.49f,.57f);RenderSettings.ambientEquatorColor=new Color(.30f,.37f,.32f);RenderSettings.ambientGroundColor=new Color(.18f,.23f,.18f);
            var sun=FindFirstObjectByType<Light>();if(sun!=null){sun.intensity=1.20f;sun.color=new Color(1,.96f,.86f);sun.transform.rotation=Quaternion.Euler(38,-105,0);}
            hud=new TerraHud(Choose,Undo,ResetView,Cancel,()=>SceneManager.LoadScene(SceneManager.GetActiveScene().name),UpdatePreview);
            hud.ConfigureEditing(BeginMove,BeginRemove,Redo,BeginReset);
            sound=gameObject.AddComponent<AudioSource>();sound.playOnAwake=false;sound.volume=.3f;
#if UNITY_WEBGL && !UNITY_EDITOR
            WebGLInput.captureAllKeyboardInput=true;
#endif
            string root=Application.streamingAssetsPath+"/WorldPacks/meadowbrook-v1";
#if !UNITY_WEBGL || UNITY_EDITOR
            root="file://"+root;
#endif
            StartCoroutine(WorldPackLoader.Load(root,Loaded,Error,hud.Progress,c=>runoffConfiguration=c));
        }
        void Error(string reason){hud.Error(reason);Debug.LogError("TERRA_PACK_ERROR: "+reason);}
        void Loaded(Manifest m,WorldData w,Scenario s)
        {StartCoroutine(ShapeLandscape(m,w,s));}
        IEnumerator ShapeLandscape(Manifest m,WorldData w,Scenario s)
        {
            hud.Progress("Shaping the terrain and woodland…");yield return null;
            try{AnnualSpatialOpportunities.AddWind(w,runoffConfiguration);buildActionRules=BuildActionRuntime.Expand(buildActionCatalog,ref w,ref s);}
            catch(Exception e){Error(e.Message);yield break;}
            world=w;scenario=s;
            try{runoff=new LandscapeRunoff(w,s,runoffConfiguration);}
            catch(Exception e){Error("Could not prepare runoff estimates: "+e.Message);yield break;}
            hud.ConfigureBuildActions(buildActionCatalog,buildActionRules,w,Choose);
            var scenery=new GameObject("Meadowbrook Valley real landscape").transform;
            try{landscape=new LandscapeRenderer(w,terrainMaterial,scenery,treePrefab,cropPrefab,grassPrefab,shrubPrefab);landscape.ConfigureBuildActions(buildActionCatalog,buildActionRules);landscape.Build();}
            catch(Exception e){Error("Could not shape the landscape: "+e.Message);yield break;}
            InitializeAnnual(m.packId);ready=true;ApplyView();hud.Ready();RefreshLandscape();Choose(treatmentId);Refresh();RestoreChapterView();Debug.Log("TERRA_PHASE2_READY: "+m.packId+" "+m.fixtureVersion+" "+Diagnostic);
            BrowserPerformance.Ready();
        }
        void Update()
        {
            hud?.Layout();if(!ready){hud?.KeyboardInput(Keyboard.current);return;}performance.Tick();var k=Keyboard.current;
            if(k!=null)
            {
                bool uiHandled=hud.KeyboardInput(k);
                if(k.escapeKey.wasPressedThisFrame&&!uiHandled)Cancel();
                if(k.f9Key.wasPressedThisFrame)performance.Start();
                if(k.f5Key.wasPressedThisFrame){perspective=true;yaw=-32;pitch=36;zoom=75;target=new Vector3(40,65,160);ApplyView();}
                if(k.f6Key.wasPressedThisFrame){perspective=false;yaw=-32;pitch=58;zoom=580;target=new Vector3(0,30,0);ApplyView();Toast("Overview study · R returns to the landscape");}
                if(k.f7Key.wasPressedThisFrame){perspective=!perspective;ApplyView();Toast(perspective?"Perspective study":"Orthographic study");}
                if(k.f10Key.wasPressedThisFrame)
                {
                    var audit=hud.Audit();Debug.Log("TERRA_UI_AUDIT: "+audit);
                    int parts=(audit.Length+3999)/4000;
                    for(int part=0;part<parts;part++)Debug.Log("TERRA_UI_PART "+part+"/"+parts+": "+audit.Substring(part*4000,Math.Min(4000,audit.Length-part*4000)));
                    Debug.Log("TERRA_ANNUAL_STATE: "+Diagnostic);
                }
                if(!uiHandled&&!hud.ModalOpen)
                {
                    if(k.digit1Key.wasPressedThisFrame)Choose("cultivate-standard");if(k.digit2Key.wasPressedThisFrame)Choose("restore-grass");if(k.digit3Key.wasPressedThisFrame)Choose("water-swale");if(k.mKey.wasPressedThisFrame)Choose("cultivate-margin");
                    if(k.zKey.wasPressedThisFrame){if(k.shiftKey.isPressed)Redo();else Undo();}if(k.yKey.wasPressedThisFrame)Redo();
                    if(k.xKey.wasPressedThisFrame)BeginReset();if(k.vKey.wasPressedThisFrame)BeginMove();if(k.deleteKey.wasPressedThisFrame||k.backspaceKey.wasPressedThisFrame)BeginRemove();if(k.rKey.wasPressedThisFrame)ResetView();
                    if(k.fKey.wasPressedThisFrame)FocusSite();
                    if(k.tabKey.wasPressedThisFrame){int i=Array.FindIndex(world.sites,s=>s.id==hoverId);SetHover(world.sites[i<0?(k.shiftKey.isPressed?world.sites.Length-1:0):(i+(k.shiftKey.isPressed?world.sites.Length-1:1))%world.sites.Length]);}
                    if(k.enterKey.wasPressedThisFrame)Commit();
                    var movement=new Vector3((k.rightArrowKey.isPressed?1:0)-(k.leftArrowKey.isPressed?1:0),0,(k.upArrowKey.isPressed?1:0)-(k.downArrowKey.isPressed?1:0));
                    if(movement.sqrMagnitude>0){target+=Quaternion.Euler(0,yaw,0)*movement.normalized*zoom*.7f*Time.unscaledDeltaTime;ApplyView();}
                    if(k.equalsKey.isPressed){zoom-=Time.unscaledDeltaTime*240;ApplyView();}if(k.minusKey.isPressed){zoom+=Time.unscaledDeltaTime*240;ApplyView();}
                }
            }
#if !UNITY_WEBGL || UNITY_EDITOR
            var mouse=Mouse.current;
            if(mouse!=null)
            {
                Vector2 p=mouse.position.ReadValue();
                if(mouse.rightButton.wasPressedThisFrame)Cancel();
                if(mouse.leftButton.wasPressedThisFrame||mouse.middleButton.wasPressedThisFrame)Pointer(0,p,mouse.middleButton.isPressed||(k?.shiftKey.isPressed??false));
                if(p!=last)Pointer(1,p,false);
                if(mouse.leftButton.wasReleasedThisFrame||mouse.middleButton.wasReleasedThisFrame)Pointer(2,p,false);
                if(mouse.scroll.ReadValue().y!=0)Scroll(-mouse.scroll.ReadValue().y/120);
            }
#endif
            RenderView();landscape.Tick(Time.unscaledTime,hud.ReducedMotion);
            if(toastUntil>0&&Time.unscaledTime>toastUntil){hud.Toast("");toastUntil=0;}
        }
        public void PointerMessage(string message)
        {
            var p=message.Split(',');if(p.Length!=4)return;
            if(int.TryParse(p[0],out int kind)&&float.TryParse(p[1],NumberStyles.Float,CultureInfo.InvariantCulture,out float x)&&float.TryParse(p[2],NumberStyles.Float,CultureInfo.InvariantCulture,out float y))Pointer(kind,new Vector2(x*Screen.width,(1-y)*Screen.height),p[3]=="1");
        }
        bool UiAt(Vector2 position)
        {
            if(EventSystem.current==null)return false;
            var results=new List<RaycastResult>();EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=position},results);return results.Count>0;
        }
        void Pointer(int kind,Vector2 position,bool panning)
        {
            hud.PointerHover(position);
            if(kind==0)
            {
                var hits=new List<RaycastResult>();
                if(EventSystem.current!=null)EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=position},hits);
                uiDown=hits.Count>0?hits[0].gameObject.GetComponentInParent<UnityEngine.UI.Button>():null;
            }
#if UNITY_WEBGL && !UNITY_EDITOR
            if(kind==2 && !panGesture && uiDown!=null && Vector2.Distance(position,down)<6)
            {
                var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=position},hits);
                var up=hits.Count>0?hits[0].gameObject.GetComponentInParent<UnityEngine.UI.Button>():null;
                var button=uiDown;uiDown=null;pressed=false;
                if(up==button&&button.IsInteractable())button.onClick.Invoke();return;
            }
#endif
            if(kind==3){pressed=false;return;}
            if(kind==0){pressed=true;dragged=false;panGesture=panning;down=last=position;overUi=UiAt(position)||hud.ModalOpen;return;}
            if(!ready)return;
            if(kind==1)
            {
                if(pressed&&!overUi&&!hud.ModalOpen)
                {
                    if(Vector2.Distance(position,down)>6)dragged=true;
                    if(dragged)
                    {Vector2 delta=(position-last)/Screen.height;var right=Quaternion.Euler(0,yaw,0)*Vector3.right;var forward=Quaternion.Euler(0,yaw,0)*Vector3.forward;target-=right*delta.x*zoom*2+forward*delta.y*zoom*2;ApplyView();landscape.ClearGhost();}
                }
                else if(!hud.ModalOpen&&!UiAt(position))SetHover(landscape.Hit(view.ScreenPointToRay(position)));
                last=position;return;
            }
            if(kind==2)
            {bool place=pressed&&!panGesture&&!dragged&&!overUi&&!UiAt(position)&&!hud.ModalOpen;pressed=false;if(place&&(editMode=="place"||editMode=="move")){SetHover(landscape.Hit(view.ScreenPointToRay(position)));Commit();}else if(dragged)UpdatePreview();}
        }
        public void Scroll(float amount){if(!ready||hud.ModalOpen||UiAt(last))return;zoom+=amount*zoom*.075f;ApplyView();}
        public void LoseFocus(string _){pressed=false;dragged=false;}
        void OnApplicationFocus(bool focused){if(!focused)LoseFocus("");}
        void ApplyView()
        {
            // Deliberate illustrated camera: pan and zoom keep painted forms and light coherent.
            yaw=-32;pitch=37;zoom=Mathf.Clamp(zoom,55,1000);
            target.x=Mathf.Clamp(target.x,-480,480);target.z=Mathf.Clamp(target.z,-500,500);
            if(ready)
            {
                target.y=landscape.Height(target.x,target.z)+8;
                var offset=Quaternion.Euler(pitch,yaw,0)*Vector3.forward*(perspective?zoom/Mathf.Tan(view.fieldOfView*.5f*Mathf.Deg2Rad):850);
                var position=target-offset;
                target.y=Mathf.Max(target.y,landscape.Height(position.x,position.z)+35+offset.y);
            }
            if(!ready)RenderView();performance.Camera();
        }
        void RenderView()
        {
            float t=!ready||hud.ReducedMotion?1:1-Mathf.Exp(-Time.unscaledDeltaTime*16);
            shownYaw=Mathf.LerpAngle(shownYaw,yaw,t);shownPitch=Mathf.Lerp(shownPitch,pitch,t);shownZoom=Mathf.Lerp(shownZoom,zoom,t);shownTarget=Vector3.Lerp(shownTarget,target,t);
            view.orthographic=!perspective;view.orthographicSize=shownZoom;view.transform.rotation=Quaternion.Euler(shownPitch,shownYaw,0);
            float distance=perspective?shownZoom/Mathf.Tan(view.fieldOfView*.5f*Mathf.Deg2Rad):850;
            var position=shownTarget-view.transform.forward*distance;
            // Also protect the interpolated camera while crossing a wooded hillside.
            if(ready&&perspective)position.y=Mathf.Max(position.y,landscape.Height(position.x,position.z)+35);
            view.transform.position=position;float displayRatio=hud?.PixelRatio??1;
            InstancedNature.DisplayPixelRatio=displayRatio;Shader.SetGlobalFloat("_TerraDisplayPixelRatio",displayRatio);
            landscape?.ViewScale(2*shownZoom/Screen.height*displayRatio);landscape?.ScaleRunoffConnections();landscape?.AnnualViewScale(2*shownZoom/Screen.height*displayRatio);
        }
        void ResetView(){perspective=true;yaw=-32;pitch=37;zoom=115;target=new Vector3(-193,25,-43);ApplyView();}
        void FocusSite(){var site=world.sites.FirstOrDefault(s=>s.id==(hoverId??selectedId))??world.sites[0];float x=site.polygon.Average(p=>p.x),z=site.polygon.Average(p=>p.z);target=new Vector3(x,landscape.Height(x,z)+8,z);zoom=72;ApplyView();Toast("Close view · Scroll to zoom · R returns to the valley");}
        void OnDestroy(){if(chapters!=null)chapters.StateChanged-=ChapterChanged;landscape?.Dispose();}
        void Toast(string message){hud.Toast(message);toastUntil=Time.unscaledTime+3;}
    }
}
