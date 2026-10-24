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
        Camera view;WorldData world;Scenario scenario;WorldStateStore state;FixtureLandscape landscape;TerraHud hud;AudioSource sound;
        string treatmentId="cultivate-standard",hoverId,selectedId;float yaw=-22,pitch=48,zoom=70;Vector3 target=new Vector3(4,-8,0);
        UnityEngine.UI.Button uiDown;
        bool ready,pressed,dragged,overUi,pan;Vector2 down,last;float toastUntil;
        public int Revision=>state?.Revision??-1;
        public string Diagnostic=>state==null?"loading":$"revision={Revision}; production={state.Result.production}; credits={state.Result.remainingCredits}; hover={hoverId}; tool={treatmentId}";
        void Start()
        {
            gameObject.name="TerraMain";Application.runInBackground=true;
            view=Camera.main;view.orthographic=true;view.nearClipPlane=.1f;view.farClipPlane=500;view.clearFlags=CameraClearFlags.SolidColor;view.backgroundColor=new Color(.84f,.88f,.77f);ApplyView();
            hud=new TerraHud(Choose,Undo,ResetView,Cancel,()=>SceneManager.LoadScene(SceneManager.GetActiveScene().name));
            sound=gameObject.AddComponent<AudioSource>();sound.playOnAwake=false;sound.volume=.3f;
#if UNITY_WEBGL && !UNITY_EDITOR
            WebGLInput.captureAllKeyboardInput=true;
#endif
            string root=Application.streamingAssetsPath+"/WorldPacks/fixture-v1";
#if !UNITY_WEBGL || UNITY_EDITOR
            root="file://"+root;
#endif
            StartCoroutine(WorldPackLoader.Load(root,Loaded,Error));
        }
        void Error(string reason){hud.Error(reason);Debug.LogError("TERRA_PACK_ERROR: "+reason);}
        void Loaded(Manifest m,WorldData w,Scenario s)
        {
            world=w;scenario=s;state=new WorldStateStore(w,s);
            var scenery=new GameObject("Synthetic development landscape").transform;
            landscape=new FixtureLandscape(w,terrainMaterial,scenery,treePrefab,cropPrefab,grassPrefab,shrubPrefab);landscape.Build();
            ready=true;hud.Ready();Choose(treatmentId);Refresh();Debug.Log("TERRA_PHASE1_READY: "+m.packId+" "+m.fixtureVersion+" "+Diagnostic);
        }
        void Update()
        {
            if(!ready)return;var k=Keyboard.current;
            if(k!=null)
            {
                if(k.escapeKey.wasPressedThisFrame)Cancel();
                if(!hud.ModalOpen)
                {
                    if(k.digit1Key.wasPressedThisFrame)Choose("cultivate-standard");if(k.digit2Key.wasPressedThisFrame)Choose("restore-grass");if(k.digit3Key.wasPressedThisFrame)Choose("water-swale");if(k.mKey.wasPressedThisFrame)Choose("cultivate-margin");
                    if(k.zKey.wasPressedThisFrame)Undo();if(k.rKey.wasPressedThisFrame)ResetView();
                    if(k.tabKey.wasPressedThisFrame){int i=Array.FindIndex(world.sites,s=>s.id==hoverId);SetHover(world.sites[(i+1)%world.sites.Length]);}
                    if(k.enterKey.wasPressedThisFrame)Commit();
                    float turn=(k.rightArrowKey.isPressed?1:0)-(k.leftArrowKey.isPressed?1:0);if(turn!=0){yaw+=turn*Time.unscaledDeltaTime*50;ApplyView();}
                    if(k.equalsKey.isPressed){zoom-=Time.unscaledDeltaTime*25;ApplyView();}if(k.minusKey.isPressed){zoom+=Time.unscaledDeltaTime*25;ApplyView();}
                }
                if(k.slashKey.wasPressedThisFrame)hud.ShowHelp();
            }
#if !UNITY_WEBGL || UNITY_EDITOR
            var mouse=Mouse.current;
            if(mouse!=null)
            {
                Vector2 p=mouse.position.ReadValue();
                if(mouse.leftButton.wasPressedThisFrame||mouse.middleButton.wasPressedThisFrame)Pointer(0,p,mouse.middleButton.isPressed||(k?.shiftKey.isPressed??false));
                if(p!=last)Pointer(1,p,pan);
                if(mouse.leftButton.wasReleasedThisFrame||mouse.middleButton.wasReleasedThisFrame)Pointer(2,p,pan);
                if(mouse.scroll.ReadValue().y!=0)Scroll(-mouse.scroll.ReadValue().y/120);
            }
#endif
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
            if(kind==0){pressed=true;dragged=false;down=last=position;overUi=UiAt(position)||hud.ModalOpen;pan=panning;return;}
            if(!ready)return;
            if(kind==1)
            {
                if(pressed&&!overUi&&!hud.ModalOpen)
                {
                    if(Vector2.Distance(position,down)>6)dragged=true;
                    if(dragged)
                    {Vector2 delta=(position-last)/Screen.height;if(pan){var right=Quaternion.Euler(0,yaw,0)*Vector3.right;var forward=Quaternion.Euler(0,yaw,0)*Vector3.forward;target-=right*delta.x*zoom*2+forward*delta.y*zoom*2;}else{yaw+=delta.x*160;pitch-=delta.y*100;}ApplyView();landscape.ClearGhost();}
                }
                else if(!hud.ModalOpen)SetHover(UiAt(position)?null:landscape.Hit(view.ScreenPointToRay(position)));
                last=position;return;
            }
            if(kind==2)
            {bool place=pressed&&!dragged&&!overUi&&!UiAt(position)&&!hud.ModalOpen;pressed=false;if(place){SetHover(landscape.Hit(view.ScreenPointToRay(position)));Commit();}}
        }
        public void Scroll(float amount){if(!ready||hud.ModalOpen)return;zoom+=amount*2;ApplyView();}
        public void LoseFocus(string _){pressed=false;dragged=false;}
        void OnApplicationFocus(bool focused){if(!focused)LoseFocus("");}
        void ApplyView(){pitch=Mathf.Clamp(pitch,30,70);zoom=Mathf.Clamp(zoom,38,82);target.x=Mathf.Clamp(target.x,-18,18);target.z=Mathf.Clamp(target.z,-16,16);view.orthographicSize=zoom;view.transform.rotation=Quaternion.Euler(pitch,yaw,0);view.transform.position=target-view.transform.forward*180;}
        void ResetView(){yaw=-22;pitch=48;zoom=70;target=new Vector3(4,-8,0);ApplyView();}
        void Choose(string id){if(!ready)return;treatmentId=id;hud.Selected(id);UpdatePreview();}
        void Cancel(){if(hud.ModalOpen){hud.CloseModal();return;}treatmentId="";hoverId=null;landscape.ClearGhost();hud.Selected("");UpdatePreview();}
        void SetHover(Site site){string id=site?.id;if(id==hoverId)return;hoverId=id;UpdatePreview();}
        EditCommand Command()=>new EditCommand{operation=state.TreatmentAt(hoverId)==null?"add":"replace",siteId=hoverId,treatmentId=treatmentId,expectedRevision=state.Revision};
        void UpdatePreview()
        {
            if(!ready)return;var site=world.sites.FirstOrDefault(s=>s.id==hoverId);var treatment=scenario.treatments.FirstOrDefault(t=>t.id==treatmentId);var result=state.Preview(Command());
            bool alreadyPlaced=site!=null&&state.TreatmentAt(site.id)==treatmentId;
            hud.Preview(site,treatment,result,state.Result);landscape.Highlight(hoverId,selectedId,result.valid||alreadyPlaced);landscape.ClearGhost();if(site!=null&&treatment!=null&&!alreadyPlaced)landscape.Ghost(site,result.valid);
        }
        void Commit()
        {
            if(!ready||hud.ModalOpen||hoverId==null||string.IsNullOrEmpty(treatmentId))return;var before=state.Result;var result=state.Commit(Command());
            if(!result.valid){Toast(result.reason);return;}
            selectedId=hoverId;var site=world.sites.First(s=>s.id==hoverId);var visual=landscape.Apply(site,treatmentId);
            if(visual!=null&&!hud.ReducedMotion){visual.localPosition=Vector3.up*2;visual.DOLocalMoveY(0,.55f).SetEase(Ease.OutCubic).SetLink(visual.gameObject);}
            if(!hud.Muted&&placementSound!=null)sound.PlayOneShot(placementSound);
            float change=result.result.production-before.production;
            Toast(site.name+": "+(change>=0?"+":"")+change.ToString("0.#")+" illustrative units · "+(result.result.spentCredits-before.spentCredits)+" credits");
            hoverId=null;Refresh();Debug.Log("TERRA_COMMIT: "+Diagnostic);
        }
        void Undo()
        {
            if(!ready||hud.ModalOpen||!state.Undo())return;
            foreach(var site in world.sites)landscape.Apply(site,state.TreatmentAt(site.id));selectedId=null;Refresh();Toast("Proposal undone. Previous land cover and credits restored.");Debug.Log("TERRA_UNDO: "+Diagnostic);
        }
        void Refresh(){hud.UpdateResult(state.Result,state.CanUndo,scenario.objectives.productionMin);UpdatePreview();}
        void Toast(string message){hud.Toast(message);toastUntil=Time.unscaledTime+5;}
    }
}
