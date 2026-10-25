using System.Linq;
using Terra.Audio;
using Terra.Contracts;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Terra.Gameplay
{
    public sealed partial class TerraMain
    {
        // Retain serialized legacy fields so historical scenes remain readable. The new bank owns playback.
        public AudioClip ambientSound, undoSound, cancelSound, blockedSound;
        TownPhase audioPhase;
        string audioRun;
        int audioYear;
        bool audioBound, audioModal;
        AnnualTownSession audioSession;
        Feature[] audioWater; float audioWaterGain, nextAudioWater; string audioChapterError;
        void InitializeAudio() { TerraAudio.Ensure(); }
        void AudioGesture() { TerraAudio.Ensure().Gesture(); }
        public void AudioVisibility(string value) { TerraAudio.Ensure().BrowserFocus(value == "1"); }
        public void AudioAudit(string _) { Debug.Log("TERRA_AUDIO_AUDIT: " + TerraAudio.Ensure().Audit()); }
        void AudioTick()
        {
            var audio = TerraAudio.Ensure();
            var k = Keyboard.current;
            if (k?.anyKey.wasPressedThisFrame ?? false) audio.Gesture();
            if ((k?.uKey.wasPressedThisFrame ?? false) && !TerraAudioPanel.ConsumedInput) TerraAudioPanel.Show();
            if (k?.f8Key.wasPressedThisFrame ?? false) AudioAudit("");
            if (annual != null && annual.Initialized && audioSession != annual)
            {
                if (audioSession != null) audioSession.Changed -= AnnualAudioChanged;
                audioSession = annual; audioSession.Changed += AnnualAudioChanged;
                var state = annual.State; audioPhase = state.phase; audioRun = state.runId; audioYear = state.year; audioBound = true;
            }
            if(chapters!=null && chapters.Error!=audioChapterError)
            {audioChapterError=chapters.Error;if(!string.IsNullOrEmpty(audioChapterError))audio.Request("blocked",9);}
            bool modal = hud != null && (hud.ModalOpen || hud.AnnualOverviewOpen);
            if (hud != null && modal != audioModal)
            { audio.Request(modal ? "ui-open" : "ui-close", 2); audioModal = modal; }
            if(ready && Time.unscaledTime >= nextAudioWater)
            {
                nextAudioWater=Time.unscaledTime+.25f;
                if(audioWater==null)audioWater=world.features.Where(f=>f.kind=="water").ToArray();
                float nearest=10000;var focusPoint=new Vector2(shownTarget.x,shownTarget.z);
                foreach(var feature in audioWater)for(int i=1;i<feature.points.Length;i++)
                {
                    var a=new Vector2(feature.points[i-1].x,feature.points[i-1].z);
                    var b=new Vector2(feature.points[i].x,feature.points[i].z);var ab=b-a;
                    var closest=a+ab*Mathf.Clamp01(Vector2.Dot(focusPoint-a,ab)/Mathf.Max(.001f,ab.sqrMagnitude));
                    nearest=Mathf.Min(nearest,Vector2.Distance(focusPoint,closest));
                }
                audioWaterGain=Mathf.InverseLerp(280,25,nearest);
            }
            audio.Context(atlasVisible, location.PackId, modal || fastForwarding, audioBound && audioPhase == TownPhase.Completed, Mathf.InverseLerp(600,60,shownZoom),audioWaterGain);
        }
        void AnnualAudioChanged(AnnualTownSnapshot state)
        {
            var audio = TerraAudio.Ensure();
            if (audioBound && state.runId != audioRun) audio.Request("new-run", 7);
            else if (audioBound && (state.phase != audioPhase || state.year != audioYear))
            {
                if (state.phase == TownPhase.Completed) audio.Request("decade-complete", 10);
                else if (!fastForwarding)
                {
                    if (state.phase == TownPhase.Resolving) audio.Request("year-submit", 7);
                    else if (state.phase == TownPhase.Reviewing)
                    {
                        var result = state.history.LastOrDefault();
                        bool attention = result != null && (result.outcomes.disruptedSites > 0 || state.arrearsCents > 0 || result.outcomes.sites.Any(s => s.phase == TownAssetPhase.Suspended));
                        audio.Request(attention ? "year-attention" : "year-report", 8);
                    }
                    else if (state.year > audioYear) audio.Request("year-continue", 7);
                    else if (audioPhase == TownPhase.Resolving) audio.Request("cancel", 6);
                }
            }
            audioBound = true; audioPhase = state.phase; audioRun = state.runId; audioYear = state.year;
        }
        void Cue(AudioClip clip)
        {
            if (clip == cancelSound) TerraAudio.Ensure().Request("cancel", 4);
            else if (clip == undoSound) TerraAudio.Ensure().Request("undo", 5);
            else if (clip == blockedSound) BlockedCue();
            else TerraAudio.Ensure().Request("ui-confirm", 4);
        }
        void ChangeCue(Terra.Core.AnnualFeedbackTone tone)
        {
            // Model notifications never unlock audio; category cues keep priority.
            string cue=tone==Terra.Core.AnnualFeedbackTone.Decline||tone==Terra.Core.AnnualFeedbackTone.Mixed?"year-attention":
                tone==Terra.Core.AnnualFeedbackTone.Outcome?"year-report":tone==Terra.Core.AnnualFeedbackTone.Release?"undo":"ui-confirm";
            TerraAudio.Ensure().Request(cue,1);
        }
        void BlockedCue() { TerraAudio.Ensure().Request("blocked", 9); }
        void PlacementCue(string action, string mode, Site site)
        {
            var audio = TerraAudio.Ensure();
            if (mode == "remove" || mode == "reset") { audio.Request(mode == "reset" ? "clear" : "remove", 6); return; }
            if (mode == "move") { audio.Request("move", 6); return; }
            float pan = 0;
            if (site != null && site.polygon.Length > 0)
            {
                var point = site.polygon[0]; pan = (view.WorldToViewportPoint(new Vector3(point.x,0,point.z)).x - .5f) * .65f;
            }
            audio.Placement(action, pan);
        }
    }
}
