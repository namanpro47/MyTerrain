#if TERRA_TREE_AUDIT
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Scripting;

namespace Terra.Rendering
{
    // Included only by TreeRenderingAudit.BuildExperiment's extra scripting define.
    // Normal builds contain neither this component nor its diagnostic controls.
    [Preserve] public sealed class TreeAuditControl:MonoBehaviour
    {
        string mode;float started;bool sampling;int unfocused;
        readonly List<float> frames=new List<float>();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install(){new GameObject("TreeAuditControl").AddComponent<TreeAuditControl>();}
        [Preserve] public void SetMode(string value)
        {
            var forestObject=GameObject.Find("Textured mesh forest study");
            if(forestObject==null){Debug.Log("TERRA_TREE_ABLATION_NOT_READY");return;}
            if(value!="normal"&&value!="off"&&value!="shadows")throw new ArgumentException("Unknown tree audit mode.");
            var forest=forestObject.GetComponent<InstancedNature>();
            forest.enabled=value!="off";forest.ShadowsOnly=value=="shadows";
            mode=value;frames.Clear();unfocused=0;started=Time.realtimeSinceStartup;sampling=true;
            Debug.Log("TERRA_TREE_ABLATION_STARTED: "+value);
        }
        void Update()
        {
            if(!sampling)return;
            float elapsed=Time.realtimeSinceStartup-started;if(elapsed<2)return;
            frames.Add(Time.unscaledDeltaTime*1000);if(!Application.isFocused)unfocused++;
            if(elapsed<14)return;sampling=false;
            var sorted=frames.OrderBy(x=>x).ToArray();var camera=Camera.main;
            Debug.Log("TERRA_TREE_ABLATION: "+JsonUtility.ToJson(new Sample {
                mode=mode,seconds=elapsed-2,frames=frames.Count,averageFps=frames.Count/(elapsed-2),
                p50Ms=sorted[(int)((sorted.Length-1)*.5f)],p95Ms=sorted[(int)((sorted.Length-1)*.95f)],
                maximumMs=sorted[sorted.Length-1],unfocusedFrames=unfocused,
                bufferWidth=Screen.width,bufferHeight=Screen.height,cameraPosition=camera.transform.position,cameraRotation=camera.transform.eulerAngles,
                fov=camera.fieldOfView,revision=UnityEngine.Object.FindAnyObjectByType<Terra.Gameplay.TerraMain>().Revision
            }));
        }
        [Serializable] sealed class Sample
        {
            public string mode;public float seconds,averageFps,p50Ms,p95Ms,maximumMs,fov;
            public int frames,unfocusedFrames,bufferWidth,bufferHeight,revision;public Vector3 cameraPosition,cameraRotation;
        }
    }
}
#endif
