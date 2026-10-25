using UnityEngine;
using Terra.Core;

namespace Terra.Gameplay
{
    public sealed partial class TerraMain
    {
        public AudioClip ambientSound,undoSound,cancelSound,blockedSound;
        AudioSource ambience,changeSound;
        AudioClip[] changeClips;
        bool audioGesture;
        float lastBlocked=-10;
        void InitializeAudio()
        {
            sound.volume=1;sound.spatialBlend=0;
            changeSound=gameObject.AddComponent<AudioSource>();changeSound.playOnAwake=false;changeSound.spatialBlend=0;changeSound.volume=.48f;
            changeClips=new[]{ChangeChime("Plan",520,660),ChangeChime("Reserve",660,440),ChangeChime("Release",440,740),ChangeChime("Outcome",523,784),ChangeChime("Decline",392,294),ChangeChime("Mixed",523,494)};
            ambience=gameObject.AddComponent<AudioSource>();ambience.playOnAwake=false;ambience.loop=true;ambience.spatialBlend=0;ambience.volume=0;ambience.clip=ambientSound;
        }
        void AudioGesture(){audioGesture=true;if(ambientSound!=null&&ambientSound.loadState==AudioDataLoadState.Unloaded)ambientSound.LoadAudioData();}
        void AudioTick()
        {
            if(ambience==null)return;
            bool audible=audioGesture&&!hud.Muted&&Application.isFocused;
            sound.mute=!audible;
            if(changeSound!=null){changeSound.mute=!audible;if(!audible)changeSound.Stop();}
            if(!audible){ambience.Stop();ambience.volume=0;return;}
            if(ambientSound!=null&&ambientSound.loadState==AudioDataLoadState.Loaded&&!ambience.isPlaying)ambience.Play();
            ambience.volume=Mathf.MoveTowards(ambience.volume,.65f,Time.unscaledDeltaTime*.4f);
        }
        void Cue(AudioClip clip)
        {
            AudioGesture();if(hud.Muted||clip==null)return;
            sound.mute=false;sound.PlayOneShot(clip);
        }
        // Original synthesized, softly enveloped notes: no new asset/license dependency.
        static AudioClip ChangeChime(string name,float first,float second)
        {
            const int rate=22050;var samples=new float[(int)(rate*.34f)];
            for(int i=0;i<samples.Length;i++)
            {
                float time=(float)i/rate;
                float Note(float start,float hz)
                {
                    float t=time-start;if(t<0||t>.20f)return 0;
                    float envelope=Mathf.Min(1,t/.009f)*Mathf.Pow(1-t/.20f,2);
                    return envelope*(Mathf.Sin(2*Mathf.PI*hz*t)+.18f*Mathf.Sin(4*Mathf.PI*hz*t))*.35f;
                }
                samples[i]=Note(0,first)+Note(.12f,second);
            }
            var clip=AudioClip.Create("Feedback "+name,samples.Length,1,rate,false);clip.SetData(samples,0);return clip;
        }
        void ChangeCue(AnnualFeedbackTone tone)
        {
            AudioGesture();if(hud.Muted||!Application.isFocused||changeSound==null)return;
            // Replace rapid edits' cue rather than stacking loud overlapping voices.
            changeSound.Stop();changeSound.mute=false;changeSound.clip=changeClips[(int)tone];changeSound.Play();
        }
        void DisposeFeedbackAudio()
        {
            if(changeClips!=null)foreach(var clip in changeClips)if(clip!=null)Destroy(clip);
        }
        void BlockedCue(){if(Time.unscaledTime-lastBlocked<.35f)return;lastBlocked=Time.unscaledTime;Cue(blockedSound);}
    }
}
