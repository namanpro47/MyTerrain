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
    public sealed class TerraMain : MonoBehaviour
    {
        public Material terrainMaterial;public GameObject treePrefab,cropPrefab,grassPrefab,shrubPrefab;public AudioClip placementSound;
        Camera view;WorldData world;Scenario scenario;WorldStateStore state;LandscapeRenderer landscape;TerraHud hud;AudioSource sound;
        string treatmentId="cultivate-standard",hoverId,selectedId;float yaw=-32,pitch=34,zoom=105;Vector3 target=new Vector3(-90,30,-70);
        bool perspective=true;
        UnityEngine.UI.Button uiDown;
        bool ready,pressed,dragged,overUi,committing;Vector2 down,last;float toastUntil;
        readonly BrowserPerformance performance=new BrowserPerformance();
        float shownYaw=-32,shownPitch=42,shownZoom=58;Vector3 shownTarget=new Vector3(-115,30,10);
        public int Revision=>state?.Revision??-1;
        public string Diagnostic=>state==null?"loading":$"revision={Revision}; production={state.Result.production}; credits={state.Result.remainingCredits}; hover={hoverId}; tool={treatmentId}";
        void Start()
        {
            gameObject.name="TerraMain";Application.runInBackground=true;
            view=Camera.main;view.orthographic=false;view.fieldOfView=36;view.nearClipPlane=1;view.farClipPlane=6000;view.clearFlags=CameraClearFlags.SolidColor;view.backgroundColor=new Color(.78f,.83f,.75f);ApplyView();
            RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor=new Color(.50f,.61f,.72f);RenderSettings.ambientEquatorColor=new Color(.38f,.46f,.40f);RenderSettings.ambientGroundColor=new Color(.22f,.28f,.22f);
            var sun=FindFirstObjectByType<Light>();if(sun!=null){sun.intensity=1.10f;sun.shadowStrength=.72f;sun.color=new Color(1,.96f,.86f);sun.transform.rotation=Quaternion.Euler(52,-100,0);}
            hud=new TerraHud(Choose,Undo,ResetView,Cancel,()=>SceneManager.LoadScene(SceneManager.GetActiveScene().name),UpdatePreview);
            sound=gameObject.AddComponent<AudioSource>();sound.playOnAwake=false;sound.volume=.3f;
#if UNITY_WEBGL && !UNITY_EDITOR
            WebGLInput.captureAllKeyboardInput=true;
#endif
            string root=Application.streamingAssetsPath+"/WorldPacks/meadowbrook-v1";
#if !UNITY_WEBGL || UNITY_EDITOR
            root="file://"+root;
#endif
            StartCoroutine(WorldPackLoader.Load(root,Loaded,Error,hud.Progress));
        }
        void Error(string reason){hud.Error(reason);Debug.LogError("TERRA_PACK_ERROR: "+reason);}
        void Loaded(Manifest m,WorldData w,Scenario s)
        {StartCoroutine(ShapeLandscape(m,w,s));}
        IEnumerator ShapeLandscape(Manifest m,WorldData w,Scenario s)
        {
            hud.Progress("Shaping the terrain and woodland…");yield return null;
            world=w;scenario=s;state=new WorldStateStore(w,s);
            var scenery=new GameObject("Meadowbrook Valley real landscape").transform;
            try{landscape=new LandscapeRenderer(w,terrainMaterial,scenery,treePrefab,cropPrefab,grassPrefab,shrubPrefab);landscape.Build();}
            catch(Exception e){Error("Could not shape the landscape: "+e.Message);yield break;}
            ready=true;ApplyView();hud.Ready();Choose(treatmentId);Refresh();Debug.Log("TERRA_PHASE2_READY: "+m.packId+" "+m.fixtureVersion+" "+Diagnostic);
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
                if(k.f10Key.wasPressedThisFrame)Debug.Log("TERRA_UI_AUDIT: "+hud.Audit());
                if(!uiHandled&&!hud.ModalOpen)
                {
                    if(k.digit1Key.wasPressedThisFrame)Choose("cultivate-standard");if(k.digit2Key.wasPressedThisFrame)Choose("restore-grass");if(k.digit3Key.wasPressedThisFrame)Choose("water-swale");if(k.mKey.wasPressedThisFrame)Choose("cultivate-margin");
                    if(k.zKey.wasPressedThisFrame)Undo();if(k.rKey.wasPressedThisFrame)ResetView();
                    if(k.fKey.wasPressedThisFrame)FocusSite();
                    if(k.tabKey.wasPressedThisFrame){int i=Array.FindIndex(world.sites,s=>s.id==hoverId);SetHover(world.sites[(i+1)%world.sites.Length]);}
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
            if(kind==2 && uiDown!=null && Vector2.Distance(position,down)<6)
            {
                var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=position},hits);
                var up=hits.Count>0?hits[0].gameObject.GetComponentInParent<UnityEngine.UI.Button>():null;
                var button=uiDown;uiDown=null;pressed=false;
                if(up==button&&button.IsInteractable())button.onClick.Invoke();return;
            }
#endif
            if(kind==3){pressed=false;return;}
            if(kind==0){pressed=true;dragged=false;down=last=position;overUi=UiAt(position)||hud.ModalOpen;return;}
            if(!ready)return;
            if(kind==1)
            {
                if(pressed&&!overUi&&!hud.ModalOpen)
                {
                    if(Vector2.Distance(position,down)>6)dragged=true;
                    if(dragged)
                    {Vector2 delta=(position-last)/Screen.height;var right=Quaternion.Euler(0,yaw,0)*Vector3.right;var forward=Quaternion.Euler(0,yaw,0)*Vector3.forward;target-=right*delta.x*zoom*2+forward*delta.y*zoom*2;ApplyView();landscape.ClearGhost();}
                }
                else if(!hud.ModalOpen)SetHover(UiAt(position)?null:landscape.Hit(view.ScreenPointToRay(position)));
                last=position;return;
            }
            if(kind==2)
            {bool place=pressed&&!dragged&&!overUi&&!UiAt(position)&&!hud.ModalOpen;pressed=false;if(place){SetHover(landscape.Hit(view.ScreenPointToRay(position)));Commit();}}
        }
        public void Scroll(float amount){if(!ready||hud.ModalOpen||UiAt(last))return;zoom+=amount*zoom*.075f;ApplyView();}
        public void LoseFocus(string _){pressed=false;dragged=false;}
        void OnApplicationFocus(bool focused){if(!focused)LoseFocus("");}
        void ApplyView()
        {
            // Keep the selected composition while panning; the overview has its own steeper pitch.
            yaw=-32;pitch=Mathf.Clamp(pitch,30,58);zoom=Mathf.Clamp(zoom,32,1000);
            target.x=Mathf.Clamp(target.x,-480,480);target.z=Mathf.Clamp(target.z,-500,500);
            if(ready)
            {
                target.y=landscape.Height(target.x,target.z)+8;
                var offset=Quaternion.Euler(pitch,yaw,0)*Vector3.forward*(zoom/Mathf.Tan(view.fieldOfView*.5f*Mathf.Deg2Rad));
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
            float distance=shownZoom/Mathf.Tan(view.fieldOfView*.5f*Mathf.Deg2Rad);
            var position=shownTarget-view.transform.forward*distance;
            // Also protect the interpolated camera while crossing a wooded hillside.
            if(ready)position.y=Mathf.Max(position.y,landscape.Height(position.x,position.z)+35);
            view.transform.position=position;float displayRatio=hud?.PixelRatio??1;
            InstancedNature.DisplayPixelRatio=displayRatio;Shader.SetGlobalFloat("_TerraDisplayPixelRatio",displayRatio);
            landscape?.ViewScale(2*shownZoom/Screen.height*displayRatio);
        }
        void ResetView(){perspective=true;yaw=-32;pitch=34;zoom=105;target=new Vector3(-90,30,-70);ApplyView();}
        void FocusSite(){var site=world.sites.FirstOrDefault(s=>s.id==(hoverId??selectedId))??world.sites[0];float x=site.polygon.Average(p=>p.x),z=site.polygon.Average(p=>p.z);target=new Vector3(x,landscape.Height(x,z)+8,z);zoom=72;ApplyView();Toast("Close view · Scroll to zoom · R returns to the valley");}
        void OnDestroy(){landscape?.Dispose();}
        void Choose(string id){if(!ready)return;treatmentId=id;hud.Selected(id);UpdatePreview();}
        void Cancel(){if(hud.ModalOpen){hud.CloseModal();return;}treatmentId="";hoverId=null;landscape.ClearGhost();hud.Selected("");UpdatePreview();}
        void SetHover(Site site){string id=site?.id;if(id==hoverId)return;hoverId=id;UpdatePreview();}
        EditCommand Command()=>new EditCommand{operation=state.TreatmentAt(hoverId)==null?"add":"replace",siteId=hoverId,treatmentId=treatmentId,expectedRevision=state.Revision};
        void UpdatePreview()
        {
            if(!ready)return;var timer=System.Diagnostics.Stopwatch.StartNew();var site=world.sites.FirstOrDefault(s=>s.id==hoverId);var treatment=scenario.treatments.FirstOrDefault(t=>t.id==treatmentId);var result=state.Preview(Command());
            bool alreadyPlaced=site!=null&&state.TreatmentAt(site.id)==treatmentId;
            hud.Preview(site,treatment,result,state.Result);landscape.Highlight(hoverId,selectedId,result.valid||alreadyPlaced);landscape.ClearGhost();if(site!=null&&treatment!=null&&!alreadyPlaced)landscape.Ghost(site,result.valid,treatmentId);
            performance.Preview(timer.Elapsed.TotalMilliseconds);
            if(!committing)BrowserPerformance.Feedback(0);
        }
        void Commit()
        {
            if(!ready||hud.ModalOpen||hoverId==null||string.IsNullOrEmpty(treatmentId))return;var timer=System.Diagnostics.Stopwatch.StartNew();var before=state.Result;var result=state.Commit(Command());
            if(!result.valid){Toast(result.reason);return;}
            committing=true;selectedId=hoverId;var site=world.sites.First(s=>s.id==hoverId);var visual=landscape.Apply(site,treatmentId);
            // Reveal through ground colour and planted detail; keep all geometry seated on slopes.
            if(!hud.Muted&&placementSound!=null)sound.PlayOneShot(placementSound);
            float change=result.result.production-before.production;
            Toast(site.name+": "+(change>=0?"+":"")+change.ToString("0.#")+" illustrative units · "+(result.result.spentCredits-before.spentCredits)+" credits");
            hoverId=null;Refresh();landscape.Settle(selectedId,hud.ReducedMotion);performance.Placement(timer.Elapsed.TotalMilliseconds);committing=false;BrowserPerformance.Feedback(1);Debug.Log("TERRA_COMMIT: "+Diagnostic);
        }
        void Undo()
        {
            if(!ready||hud.ModalOpen||!state.Undo())return;
            foreach(var site in world.sites)landscape.Apply(site,state.TreatmentAt(site.id));selectedId=null;Refresh();Toast("Proposal undone. Previous land cover and credits restored.");Debug.Log("TERRA_UNDO: "+Diagnostic);
        }
        void Refresh(){hud.UpdateResult(state.Result,state.CanUndo,scenario.objectives.productionMin);UpdatePreview();}
        void Toast(string message){hud.Toast(message);toastUntil=Time.unscaledTime+3;}
    }
}
