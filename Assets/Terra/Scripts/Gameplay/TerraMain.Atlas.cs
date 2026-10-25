using Terra.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Terra.Gameplay
{
    public sealed partial class TerraMain
    {
        LocationProfile location=LocationProfile.All[0];bool atlasVisible,loadingLocation,arrivalPending;WorldAtlas atlas;
        public bool LandscapeIsReady=>ready;
        public void EnterSelectedLocation(LocationProfile selected)
        {
            if(loadingLocation&&location.PackId!=selected.PackId)
            {WorldAtlas.Instance.BeginLoading();if(annual!=null)SaveAnnual();SceneManager.LoadScene(SceneManager.GetActiveScene().name);return;}
            if(!loadingLocation)LoadLocation(selected);
            else if(ready)WorldAtlas.Instance.LandscapeReady();
        }
        public void LoadLocation(LocationProfile selected)
        {
            WorldAtlas.Instance.BeginLoading();location=selected;loadingLocation=true;arrivalPending=true;
            if(!location.IsWisconsin){zoom=275;pitch=48;target=new Vector3(0,30,0);}
            InitializeLandscape();hud.SetVisible(false);view.enabled=false;
        }
        void ConfigureAtlasMotion(){hud.SetReducedMotion(WorldAtlas.Instance.ReducedMotion);}
        public void SetAtlasVisible(int visible)
        {
            atlasVisible=visible!=0;LoseFocus("");if(view!=null)view.enabled=!atlasVisible;hud?.SetVisible(!atlasVisible);
            if(atlasVisible&&annual!=null)SaveAnnual();AudioListener.pause=atlasVisible;
        }
        public void BeginAtlasDescent(){view.enabled=true;ConfigureAtlasMotion();shownZoom=zoom*(hud.ReducedMotion?1:1.8f);shownPitch=55;shownTarget=target;}
        public void AtlasDescent(float progress)
        {shownZoom=Mathf.Lerp(zoom*1.8f,zoom,Mathf.SmoothStep(0,1,progress));shownPitch=Mathf.Lerp(55,pitch,Mathf.SmoothStep(0,1,progress));RenderView();}
        public void FinishAtlasDescent(){SetAtlasVisible(0);if(arrivalPending){arrivalPending=false;BeginAnnualArrival();}}
        void ReportAtlasError(string message){WorldAtlas.Instance.LoadFailed(message);}
    }
}
