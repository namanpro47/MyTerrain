using DG.Tweening;
using UnityEngine;

namespace Terra.Toolkit
{
    // An isolated asset smoke test; this is not the game's scene or simulation.
    public sealed class TerraToolkitPreview : MonoBehaviour
    {
        public Transform[] specimens;
        public AudioClip[] sounds;
        private Vector3[] scales;
        private AudioSource speaker;
        private int soundIndex;
        private int completedAnimations;

        public int CompletedAnimations => completedAnimations;

        private void Start()
        {
            speaker = gameObject.AddComponent<AudioSource>();
            speaker.playOnAwake = false;
            speaker.volume = 0.35f;
            scales = new Vector3[specimens.Length];
            for (int i = 0; i < specimens.Length; i++) scales[i] = specimens[i].localScale;
            Replay();
        }

        private void Replay()
        {
            completedAnimations = 0;
            for (int i = 0; i < specimens.Length; i++)
            {
                var specimen = specimens[i];
                specimen.DOKill();
                specimen.localScale = Vector3.zero;
                specimen.DOScale(scales[i], 0.65f).SetDelay(i * 0.035f)
                    .SetEase(Ease.OutBack).SetLink(specimen.gameObject)
                    .OnComplete(() => completedAnimations++);
            }
        }

        private void OnGUI()
        {
            float scale = Mathf.Max(0.5f, Screen.width / 1440f);
            var oldMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * scale);
            var heading = new GUIStyle(GUI.skin.label) { fontSize = 26, fontStyle = FontStyle.Bold };
            var body = new GUIStyle(GUI.skin.label) { fontSize = 16 };
            var button = new GUIStyle(GUI.skin.button) { fontSize = 16 };
            GUI.Box(new Rect(22, 18, 800, 146), "");
            GUI.Label(new Rect(40, 28, 760, 36), "TERRA / HACKATHON TOOLKIT", heading);
            GUI.Label(new Rect(40, 68, 760, 26), "24 nature models • URP materials • DOTween • 10 interface sounds", body);
            if (GUI.Button(new Rect(40, 110, 170, 34), "Replay animation", button)) Replay();
            if (sounds.Length > 0 && GUI.Button(new Rect(226, 110, 260, 34), "Play: " + sounds[soundIndex].name, button))
            {
                speaker.PlayOneShot(sounds[soundIndex]);
                Debug.Log("TOOLKIT_AUDIO_PLAY: " + sounds[soundIndex].name);
                soundIndex = (soundIndex + 1) % sounds.Length;
            }
            GUI.Label(new Rect(504, 114, 290, 26), "Animations complete: " + completedAnimations + "/" + specimens.Length, body);
            GUI.Label(new Rect(28, Screen.height / scale - 38, 1100, 28), "Asset compatibility preview — illustrative display, not a real geographic location. Kenney / CC0.", body);
            GUI.matrix = oldMatrix;
        }
    }
}
