using System;
using System.Collections;
using Terra.Contracts;
using UnityEngine;
using UnityEngine.Networking;
namespace Terra.Geography
{
    public static class WorldPackLoader
    {
        public static IEnumerator Load(string root,Action<Manifest,WorldData,Scenario> ready,Action<string> fail,Action<string> progress=null)
        {
            string manifestText=null,worldText=null,scenarioText=null,error=null;
            progress?.Invoke("Reading the location and source manifest…");
            yield return Read(root+"/manifest.json",x=>manifestText=x,x=>error=x);
            if(error!=null){fail(error);yield break;}
            Manifest m=null;
            try { m=JsonUtility.FromJson<Manifest>(manifestText); if(m.schemaVersion!=1)throw new Exception("Unsupported WorldPack schema. Expected 1.");
                if(m.worldDataPath!="world.json"||m.scenarioPath!="scenario.json")throw new Exception("Invalid v1 layer paths."); }
            catch(Exception e){fail(e.Message);yield break;}
            progress?.Invoke("Loading terrain, land cover and mapped features…");
            yield return Read(root+"/world.json",x=>worldText=x,x=>error=x);
            if(error!=null){fail(error);yield break;}
            progress?.Invoke("Loading the hypothetical management scenario…");
            yield return Read(root+"/scenario.json",x=>scenarioText=x,x=>error=x);
            if(error!=null){fail(error);yield break;}
            WorldData w=null;Scenario s=null;
            try{w=JsonUtility.FromJson<WorldData>(worldText);s=JsonUtility.FromJson<Scenario>(scenarioText);PackValidator.Validate(m,w,s);}
            catch(Exception e){fail("WorldPack rejected: "+e.Message);yield break;}
            ready(m,w,s);
        }
        static IEnumerator Read(string url,Action<string> done,Action<string> fail)
        {
            using(var request=UnityWebRequest.Get(url))
            {request.timeout=20;yield return request.SendWebRequest();if(request.result!=UnityWebRequest.Result.Success)fail("Could not load the landscape. "+request.error);else done(request.downloadHandler.text);}
        }
    }
}
