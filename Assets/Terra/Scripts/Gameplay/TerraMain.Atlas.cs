using System.Runtime.InteropServices;
using Terra.Core;
using UnityEngine;
namespace Terra.Gameplay
{
    public sealed partial class TerraMain
    {
        LocationProfile location=LocationProfile.All[0];
        bool atlasVisible;
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] static extern int TerraLocationIndex();
        [DllImport("__Internal")] static extern int TerraAtlasReducedMotion();
        [DllImport("__Internal")] static extern void TerraLandscapeError(string message);
#endif
        void ConfigureLocation()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            location=LocationProfile.At(TerraLocationIndex());
#else
            location=LocationProfile.Find(PlayerPrefs.GetString("terra.editor.location","meadowbrook-v1"));
#endif
            if(!location.IsWisconsin){zoom=275;pitch=48;target=new Vector3(0,30,0);}
        }
        void ConfigureAtlasMotion()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            hud.SetReducedMotion(TerraAtlasReducedMotion()!=0);
#endif
        }
        // A visible browser atlas must own keyboard input. Async accepted-year work may
        // still finish and save; it cannot be edited from behind the selector.
        public void SetAtlasVisible(int visible)
        {
            atlasVisible=visible!=0;LoseFocus("");
#if UNITY_WEBGL && !UNITY_EDITOR
            WebGLInput.captureAllKeyboardInput=!atlasVisible;
#endif
            if(view!=null)view.enabled=!atlasVisible;
            if(atlasVisible){if(annual!=null)SaveAnnual();AudioListener.pause=true;}
            else {AudioListener.pause=false;ConfigureAtlasMotion();}
        }
        void ReportAtlasError(string message)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            TerraLandscapeError(message);
#endif
        }
    }
}
