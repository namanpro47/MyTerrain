using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

namespace Terra.Audio
{
    // One voice follows the visible dialogue. It never changes the annual simulation.
    public sealed class TerraNarration : MonoBehaviour
    {
        public static TerraNarration Instance { get; private set; }
        public bool Enabled { get; private set; }
        public string Text { get; private set; } = "";
        public int Status { get; private set; }
        public bool Speaking => Status == 2;
        public string StatusLabel => !Enabled ? "Voice off" : Status==4 ? "Voice unavailable · retry" : Status==1 ? "Preparing voice…" : Status==2 ? "Reading aloud" : Status==3 ? "Voice paused" : "Read aloud";
        AudioSource voice; Coroutine routine; UnityWebRequest request;
        readonly Dictionary<string,AudioClip> cache = new Dictionary<string,AudioClip>();
        int generation; bool paused;
        static readonly Regex Tags = new Regex("<[^>]*>");
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] static extern void TerraNarrationSet(string text, int enabled, float volume);
        [DllImport("__Internal")] static extern void TerraNarrationReplay();
        [DllImport("__Internal")] static extern int TerraNarrationStatus();
#endif
        public static TerraNarration Ensure()
        {
            if(Instance==null)TerraAudio.Ensure().gameObject.AddComponent<TerraNarration>();
            return Instance;
        }
        void Awake()
        {
            Instance=this;Enabled=PlayerPrefs.GetInt(TerraAudio.PreferencePrefix+"narration-enabled",1)!=0;
            voice=gameObject.AddComponent<AudioSource>();voice.playOnAwake=false;voice.spatialBlend=0;voice.ignoreListenerPause=true;
        }
        public static string PlainText(string rich) => Regex.Replace(Tags.Replace(rich??"", "").Replace("\n", ". "),@"\s+"," ").Trim()
            .Replace("→",", then ").Replace("·", ". ").Replace("m³","cubic metres").Replace("m²","square metres").Replace("&", "and");
        public void Show(string rich)
        {
            string value=PlainText(rich);if(value==Text)return;
            Stop();Text=value;
#if !UNITY_WEBGL || UNITY_EDITOR
            if(Text.Length>0)routine=StartCoroutine(Read(generation));
#endif
        }
        public void SetEnabled(bool value)
        {Enabled=value;PlayerPrefs.SetInt(TerraAudio.PreferencePrefix+"narration-enabled",value?1:0);PlayerPrefs.Save();}
        public void Replay()
        {
            TerraAudio.Ensure().Gesture();
#if UNITY_WEBGL && !UNITY_EDITOR
            TerraNarrationReplay();
#else
            Stop();if(Text.Length>0)routine=StartCoroutine(Read(generation));
#endif
        }
        void Stop()
        {
            generation++;request?.Abort();request=null;if(routine!=null)StopCoroutine(routine);routine=null;
            if(voice!=null)voice.Stop();paused=false;Status=0;
        }
        bool Audible => Enabled && TerraAudio.Instance.Audible && TerraAudio.Instance.Level(4)>0;
        void Update()
        {
            var audio=TerraAudio.Instance;float gain=audio.Audible?audio.Level(0)*audio.Level(4):0;
#if UNITY_WEBGL && !UNITY_EDITOR
            TerraNarrationSet(Text,Enabled?1:0,gain);Status=TerraNarrationStatus();
#else
            voice.volume=gain;
            if(!Audible&&!paused){voice.Pause();paused=true;}
            else if(Audible&&paused){voice.UnPause();paused=false;}
            if(paused&&Text.Length>0&&Status!=4)Status=3;
            else if(voice.isPlaying)Status=2;
#endif
        }
#if !UNITY_WEBGL || UNITY_EDITOR
        [Serializable] sealed class SpeechRequest { public string text; }
        IEnumerator Read(int token)
        {
            yield return new WaitForSecondsRealtime(.35f);
            foreach(var part in Segments(Text))
            {
                while(!Audible)yield return null;
                if(token!=generation)yield break;
                if(!cache.TryGetValue(part,out var clip))
                {
                    Status=1;
                    using(var req=new UnityWebRequest("http://127.0.0.1:8788/api/narration","POST"))
                    {
                        request=req;req.uploadHandler=new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonUtility.ToJson(new SpeechRequest{text=part})));
                        req.downloadHandler=new DownloadHandlerAudioClip(req.url,AudioType.MPEG);req.SetRequestHeader("Content-Type","application/json");req.timeout=25;
                        yield return req.SendWebRequest();request=null;
                        if(token!=generation)yield break;
                        if(req.result!=UnityWebRequest.Result.Success){Status=4;yield break;}
                        clip=DownloadHandlerAudioClip.GetContent(req);if(clip==null){Status=4;yield break;}
                        if(cache.Count>=30){var old=cache.First();cache.Remove(old.Key);Destroy(old.Value);}
                        cache[part]=clip;
                    }
                }
                while(!Audible)yield return null;
                voice.clip=clip;voice.Play();Status=2;
                while(voice.isPlaying||paused)yield return null;
            }
            Status=5;routine=null;
        }
        static IEnumerable<string> Segments(string value)
        {
            while(value.Length>600){int end=value.LastIndexOf(' ',599,600);if(end<=0)end=600;yield return value.Substring(0,end);value=value.Substring(end).TrimStart();}
            if(value.Length>0)yield return value;
        }
#endif
        void OnDestroy(){Stop();foreach(var clip in cache.Values)Destroy(clip);if(Instance==this)Instance=null;}
    }
}
