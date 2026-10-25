using UnityEngine;

namespace Terra.Gameplay
{
    public sealed partial class TerraMain
    {
        public AudioClip ambientSound,undoSound,cancelSound,blockedSound;
        AudioSource ambience;
        bool audioGesture;
        float lastBlocked=-10;
        void InitializeAudio()
        {
            sound.volume=1;sound.spatialBlend=0;
            ambience=gameObject.AddComponent<AudioSource>();ambience.playOnAwake=false;ambience.loop=true;ambience.spatialBlend=0;ambience.volume=0;ambience.clip=ambientSound;
        }
        void AudioGesture(){audioGesture=true;if(ambientSound!=null&&ambientSound.loadState==AudioDataLoadState.Unloaded)ambientSound.LoadAudioData();}
        void AudioTick()
        {
            if(ambience==null)return;
            bool audible=audioGesture&&!hud.Muted&&Application.isFocused;
            sound.mute=!audible;
            if(!audible){ambience.Stop();ambience.volume=0;return;}
            if(ambientSound!=null&&ambientSound.loadState==AudioDataLoadState.Loaded&&!ambience.isPlaying)ambience.Play();
            ambience.volume=Mathf.MoveTowards(ambience.volume,.65f,Time.unscaledDeltaTime*.4f);
        }
        void Cue(AudioClip clip)
        {
            AudioGesture();if(hud.Muted||clip==null)return;
            sound.mute=false;sound.PlayOneShot(clip);
        }
        void BlockedCue(){if(Time.unscaledTime-lastBlocked<.35f)return;lastBlocked=Time.unscaledTime;Cue(blockedSound);}
    }
}
