using System;
using System.Collections.Generic;
using UnityEngine;

namespace Terra.Audio
{
    // All sound is presentation-only. This director never writes town state or uses UnityEngine.Random.
    // TN-07/08: quiet, bounded voices; no score-up reward or sound on every hovered field.
    public sealed class TerraAudio : MonoBehaviour
    {
        public static TerraAudio Instance { get; private set; }
        public const string PreferencePrefix = "myterrain.audio.v1.";
        public static readonly string[] Buses = { "master", "music", "ambience", "effects", "narration" };
        readonly float[] levels = { .8f, .55f, .65f, .8f, 1f };
        readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
        readonly Dictionary<string, float> cooldowns = new Dictionary<string, float>();
        readonly Dictionary<string, int> variations = new Dictionary<string, int>();
        readonly System.Random random = new System.Random(271828);
        readonly Queue<string> recent = new Queue<string>();
        AudioSource[] voices, beds, music;
        readonly float[] voiceGains = new float[6];
        readonly float[] bedTargets = new float[3];
        int voiceIndex, musicIndex;
        float musicAge, musicFade, focusGain, duckUntil, nextBird, birdRemaining, requestedAt, musicDuck = 1;
        bool gesture, focused = true, browserVisible = true, suspended;
        string requested; int requestedPriority; float requestedPan;
        string region = "meadowbrook-v1";
        bool atlas = true, report, ending;
        float detail = 1, riverProximity;
        public bool Muted { get; private set; }
        public bool Unlocked => gesture;
        public string LastCue { get; private set; } = "none";
        public int PlayedCount { get; private set; }
        public int DroppedCount { get; private set; }
        public bool Audible => gesture && !Muted && focused && browserVisible && levels[0] > 0;
        public static TerraAudio Ensure()
        {
            if (Instance == null) new GameObject("Terra Audio").AddComponent<TerraAudio>();
            return Instance;
        }
        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this; DontDestroyOnLoad(gameObject);
            Muted = PlayerPrefs.GetInt(PreferencePrefix + "muted", 0) != 0;
            for (int i = 0; i < levels.Length; i++) levels[i] = Mathf.Clamp01(PlayerPrefs.GetFloat(PreferencePrefix + Buses[i], levels[i]));
            voices = new AudioSource[6]; for (int i = 0; i < voices.Length; i++) voices[i] = Source(false);
            beds = new AudioSource[3];
            string[] ids = { "ambient-wind", "ambient-river", "ambient-birds" };
            for (int i = 0; i < beds.Length; i++) { beds[i] = Source(true); beds[i].clip = Clip(ids[i]); }
            music = new[] { Source(false), Source(false) };
            music[0].clip = Clip("music-mystical"); music[1].clip = Clip("music-vaporware");
            musicFade = 1; nextBird = 9;
            foreach (var clip in Resources.LoadAll<AudioClip>("Audio"))
            {
                clips[clip.name] = clip;
                if (!clip.name.StartsWith("music-") && !clip.name.StartsWith("ambient-")) clip.LoadAudioData();
            }
            gameObject.AddComponent<TerraAudioPanel>();
        }
        AudioSource Source(bool loop)
        {
            var s = gameObject.AddComponent<AudioSource>();
            s.playOnAwake = false; s.loop = loop; s.spatialBlend = 0; s.volume = 0;
            // Atlas intentionally pauses the scene listener. This independent nonspatial bus owns its own pause policy.
            s.ignoreListenerPause = true; s.dopplerLevel = 0; return s;
        }
        AudioClip Clip(string id)
        {
            if (!clips.TryGetValue(id, out var clip))
            {
                clip = Resources.Load<AudioClip>("Audio/" + id); clips[id] = clip;
                if (clip == null) Debug.LogWarning("TERRA_AUDIO_MISSING: " + id);
            }
            return clip;
        }
        public float Level(int bus) => levels[bus];
        public void SetLevel(int bus, float value)
        {
            if (bus < 0 || bus >= levels.Length) return;
            levels[bus] = float.IsNaN(value) ? 0 : Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(PreferencePrefix + Buses[bus], levels[bus]); PlayerPrefs.Save();
            if (!Audible) Silence();
        }
        public void SetMuted(bool value)
        {
            Muted = value; PlayerPrefs.SetInt(PreferencePrefix + "muted", value ? 1 : 0); PlayerPrefs.Save();
            if (value) Silence();
            else Request("ui-confirm", 2);
        }
        public void Gesture() { gesture = true; }
        public void BrowserFocus(bool value) { browserVisible = value; if (!value) Silence(); }
        void OnApplicationFocus(bool value) { focused = value; if (!value) Silence(); }
        void OnApplicationPause(bool value) { focused = !value; if (value) Silence(); }
        void Silence()
        {
            requested = null; requestedPriority = 0; focusGain = 0;
            if (voices == null) return;
            foreach (var s in voices) s.Stop();
            foreach (var s in beds) { s.Pause(); s.volume = 0; }
            foreach (var s in music) { s.Pause(); s.volume = 0; }
            suspended = true;
        }
        public void Context(bool inAtlas, string location, bool reviewing, bool complete, float cameraDetail, float waterProximity)
        { atlas = inAtlas; region = location; report = reviewing; ending = complete; detail = Mathf.Clamp01(cameraDetail); riverProximity = Mathf.Clamp01(waterProximity); }
        public static string PlacementFamily(string id)
        {
            switch (id)
            {
                case "cultivate-standard": return "farm";
                case "conservation-rotation": return "rotation";
                case "cultivate-margin": return "margin";
                case "cover-crop": return "cover";
                case "restore-grass": return "meadow";
                case "water-swale": return "swale";
                case "public-park": return "park";
                case "low-density-housing": return "home";
                case "high-density-housing": return "housing";
                case "agricultural-storage": return "warehouse";
                case "data-center": return "data";
                case "village-center": return "village";
                case "non-renewable-power": return "power";
                case "solar": return "solar";
                case "wind": return "wind";
                case "stewardship-agreement": return "stewardship";
                case "wastewater-upgrade": return "water";
                default: return null;
            }
        }
        public void Placement(string id, float pan = 0)
        {
            string family = PlacementFamily(id);
            if (family == null) { Request("ui-confirm", 5, pan); return; }
            variations.TryGetValue(family, out int variant); variations[family] = (variant + 1) % 3;
            Request("place-" + family + "-" + variant, 6, pan);
        }
        // Coalesce synchronous UI + transaction notifications to the strongest meaningful cue.
        public void Request(string id, int priority = 2, float pan = 0)
        {
            if (!Audible || levels[3] <= 0) { DroppedCount++; return; }
            float interval = id == "ui-focus" ? .11f : id == "blocked" ? .4f : .075f;
            if (cooldowns.TryGetValue(id, out var time) && Time.unscaledTime - time < interval) return;
            if (requested == null || priority >= requestedPriority)
            { requested = id; requestedPriority = priority; requestedPan = Mathf.Clamp(pan, -.4f, .4f); requestedAt = Time.unscaledTime; }
        }
        public void Click() { Gesture(); Request("ui-click", 1); }
        public void Focus() { Request("ui-focus", 0); }
        bool Ready(AudioClip clip)
        {
            if (clip == null) return false;
            if (clip.loadState == AudioDataLoadState.Unloaded) clip.LoadAudioData();
            return clip.loadState == AudioDataLoadState.Loaded;
        }
        void LateUpdate()
        {
            float dt = Mathf.Min(Time.unscaledDeltaTime, .1f);
            if (!Audible) { if (!suspended) Silence(); return; }
            if (suspended)
            {
                foreach (var s in beds) s.UnPause(); foreach (var s in music) s.UnPause(); suspended = false;
            }
            focusGain = Mathf.MoveTowards(focusGain, 1, dt * .65f);
            if (requested != null)
            {
                var clip = Clip(requested);
                if (Ready(clip))
                {
                    // Prefer an unused voice; fixed pool caps rapid interactions, oldest voice is replaced if full.
                    int index = Array.FindIndex(voices, s => !s.isPlaying);
                    if (index < 0) index = voiceIndex++ % voices.Length;
                    var s = voices[index]; s.Stop(); s.clip = clip; s.panStereo = requestedPan;
                    s.pitch = requested.StartsWith("place-", StringComparison.Ordinal) ? .98f + (float)random.NextDouble() * .04f : 1;
                    voiceGains[index] = requested == "ui-focus" ? .25f : requested == "ui-click" ? .65f : 1;
                    s.volume = levels[0] * levels[3] * .7f * voiceGains[index]; s.Play();
                    LastCue = requested; PlayedCount++; cooldowns[requested] = Time.unscaledTime;
                    recent.Enqueue(requested); while (recent.Count > 16) recent.Dequeue();
                    if (requestedPriority >= 5) duckUntil = Time.unscaledTime + Mathf.Min(clip.length, 2.5f);
                }
                if (Ready(clip) || Time.unscaledTime - requestedAt > .25f) { requested = null; requestedPriority = 0; }
            }
            // Apply changed controls to already playing tails too.
            int active = 0; foreach (var s in voices) if (s.isPlaying) active++;
            float headroom = .7f / Mathf.Sqrt(Mathf.Max(1, active));
            for (int i = 0; i < voices.Length; i++) voices[i].volume = levels[0] * levels[3] * voiceGains[i] * headroom;
            float scene = atlas ? .16f : report || TerraAudioPanel.IsOpen ? .65f : 1;
            float ambient = levels[0] * levels[2] * focusGain * scene;
            nextBird -= dt;
            if (nextBird <= 0) { birdRemaining = 10 + (float)random.NextDouble() * 8; nextBird = birdRemaining + 18 + (float)random.NextDouble() * 20; }
            birdRemaining = Mathf.Max(0, birdRemaining - dt);
            bedTargets[0] = ambient * (region == "suusamyr-v1" ? .85f : .65f);
            bedTargets[1] = ambient * (region == "meadowbrook-v1" ? .35f : 0) * Mathf.Lerp(.25f, 1, detail) * riverProximity;
            bedTargets[2] = ambient * (region == "suusamyr-v1" ? .18f : .38f) * (birdRemaining > 0 && !atlas ? 1 : 0);
            for (int i = 0; i < beds.Length; i++)
            {
                var s = beds[i];
                if (Ready(s.clip) && !s.isPlaying) s.Play();
                s.volume = Mathf.MoveTowards(s.volume, bedTargets[i], dt * .12f);
            }
            TickMusic(dt);
        }
        void TickMusic(float dt)
        {
            var current = music[musicIndex];
            if (!Ready(current.clip)) return;
            if (!current.isPlaying) current.Play();
            musicAge = current.time;
            float duration = current.clip.length;
            if (musicAge >= duration - 16) Ready(music[1 - musicIndex].clip);
            // Two full pieces alternate, with a six-second fade; no chopped, repeatedly restarted notes.
            if (musicAge >= duration - 6)
            {
                musicIndex = 1 - musicIndex;
                var next = music[musicIndex]; next.Stop();
                if (Ready(next.clip)) next.Play();
                musicAge = 0; musicFade = 0;
            }
            musicFade = Mathf.MoveTowards(musicFade, 1, dt / 6);
            musicDuck = Mathf.MoveTowards(musicDuck, TerraNarration.Instance?.Speaking == true ? .25f : Time.unscaledTime < duckUntil ? .55f : 1, dt * 1.5f);
            float gain = levels[0] * levels[1] * focusGain * (report ? .65f : ending ? .9f : .8f) * musicDuck;
            music[musicIndex].volume = gain * Mathf.Sin(musicFade * Mathf.PI * .5f);
            music[1 - musicIndex].volume = gain * Mathf.Cos(musicFade * Mathf.PI * .5f);
            if (musicFade >= 1 && music[1 - musicIndex].isPlaying) music[1 - musicIndex].Stop();
        }
        public string Audit() => JsonUtility.ToJson(new AudioAudit {
            unlocked = gesture, audible = Audible, muted = Muted, focused = focused && browserVisible,
            atlas = atlas, region = region, levels = (float[])levels.Clone(), lastCue = LastCue,
            played = PlayedCount, dropped = DroppedCount, activeVoices = Array.FindAll(voices, s => s.isPlaying).Length,
            musicPlaying = music[musicIndex].isPlaying, musicSeconds = musicAge, track = music[musicIndex].clip?.name,
            musicGain = music[musicIndex].volume, ambienceGain = beds[0].volume, recent = recent.ToArray(), settingsOpen = TerraAudioPanel.IsOpen,
            narrationEnabled = TerraNarration.Instance?.Enabled ?? false,
            narrationStatus = TerraNarration.Instance?.Status ?? 0,
            narrationCharacters = TerraNarration.Instance?.Text.Length ?? 0
        });
        [Serializable] public sealed class AudioAudit
        {
            public bool unlocked, audible, muted, focused, atlas, musicPlaying, settingsOpen;
            public string region, lastCue, track; public float[] levels; public string[] recent;
            public int played, dropped, activeVoices; public float musicSeconds, musicGain, ambienceGain;
            public bool narrationEnabled; public int narrationStatus, narrationCharacters;
        }
        void OnDestroy() { if (Instance == this) Instance = null; }
    }
}
