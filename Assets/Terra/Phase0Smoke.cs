using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Terra
{
    // Technical browser fixture only. No geography, gameplay, or phase-1 dependencies.
    public sealed class Phase0Smoke : MonoBehaviour
    {
        private Camera view;
        private Vector3 target;
        private float yaw = 35, pitch = 35, distance = 15;
        private bool dragging, panning, focused = true;
        private float nextReport;
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern void TerraCameraStatus(float yaw, float pitch, float distance, float x, float z);
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (SceneManager.GetActiveScene().name == "SampleScene")
                new GameObject("Phase0Smoke").AddComponent<Phase0Smoke>();
        }

        private void Start()
        {
            view = Camera.main;
            view.clearFlags = CameraClearFlags.SolidColor;
            view.backgroundColor = new Color(0.07f, 0.12f, 0.16f);
            view.nearClipPlane = 0.1f;
            view.farClipPlane = 100;
            var material = Resources.Load<Material>("Phase0Material");
            Make(PrimitiveType.Cube, "Base", new Vector3(0, -0.3f, 0), new Vector3(9, 0.6f, 7), new Color(0.3f, 0.47f, 0.46f), material);
            Make(PrimitiveType.Cube, "Coral cube", new Vector3(-2, 0.8f, 0), Vector3.one * 1.6f, new Color(0.95f, 0.46f, 0.3f), material);
            Make(PrimitiveType.Sphere, "Gold sphere", new Vector3(0, 1, 1.2f), Vector3.one * 2, new Color(1, 0.77f, 0.28f), material);
            Make(PrimitiveType.Cylinder, "Teal cylinder", new Vector3(2.3f, 1.3f, -0.6f), new Vector3(1.4f, 1.3f, 1.4f), new Color(0.26f, 0.8f, 0.72f), material);
#if UNITY_WEBGL && !UNITY_EDITOR
            WebGLInput.captureAllKeyboardInput = false;
#endif
            ResetView();
            Debug.Log("TERRA_PHASE0_READY");
        }

        private static void Make(PrimitiveType type, string label, Vector3 position, Vector3 scale, Color color, Material material)
        {
            var item = GameObject.CreatePrimitive(type);
            item.name = label;
            item.transform.position = position;
            item.transform.localScale = scale;
            item.GetComponent<Renderer>().material = new Material(material) { color = color };
        }

        public void ResetView()
        {
            target = Vector3.zero;
            yaw = 35; pitch = 35; distance = 15;
            dragging = panning = false;
            ApplyView();
        }

        // Called by the HTML shell when canvas focus changes, including release outside it.
        public void SetCanvasFocus(string value)
        {
            focused = value == "1";
            dragging = panning = false;
        }

        private void OnApplicationFocus(bool value)
        {
            focused = value;
            dragging = panning = false;
        }

        private void Update()
        {
            if (!focused) return;
            var mouse = Mouse.current;
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.rKey.wasPressedThisFrame) ResetView();
            if (mouse != null)
            {
                if (mouse.leftButton.wasPressedThisFrame) { dragging = true; panning = keyboard != null && keyboard.shiftKey.isPressed; }
                if (mouse.middleButton.wasPressedThisFrame) { dragging = true; panning = true; }
                if (!mouse.leftButton.isPressed && !mouse.middleButton.isPressed) dragging = false;
                if (dragging)
                {
                    var delta = mouse.delta.ReadValue();
                    if (panning)
                    {
                        var right = Quaternion.Euler(0, yaw, 0) * Vector3.right;
                        var forward = Quaternion.Euler(0, yaw, 0) * Vector3.forward;
                        target += (-right * delta.x - forward * delta.y) * distance * 0.0015f;
                    }
                    else { yaw += delta.x * 0.25f; pitch -= delta.y * 0.2f; }
                }
                distance *= Mathf.Exp(-Mathf.Clamp(mouse.scroll.ReadValue().y, -200, 200) * 0.0015f);
            }
            if (keyboard != null)
            {
                float step = Time.unscaledDeltaTime;
                yaw += ((keyboard.rightArrowKey.isPressed ? 1 : 0) - (keyboard.leftArrowKey.isPressed ? 1 : 0)) * step * 65;
                pitch += ((keyboard.downArrowKey.isPressed ? 1 : 0) - (keyboard.upArrowKey.isPressed ? 1 : 0)) * step * 45;
                distance += ((keyboard.minusKey.isPressed ? 1 : 0) - (keyboard.equalsKey.isPressed ? 1 : 0)) * step * 8;
            }
            pitch = Mathf.Clamp(pitch, 15, 75);
            distance = Mathf.Clamp(distance, 8, 24);
            target.x = Mathf.Clamp(target.x, -2, 2);
            target.z = Mathf.Clamp(target.z, -2, 2);
            ApplyView();
        }

        private void ApplyView()
        {
            if (view == null) return;
            view.transform.rotation = Quaternion.Euler(pitch, yaw, 0);
            view.transform.position = target - view.transform.forward * distance;
#if UNITY_WEBGL && !UNITY_EDITOR
            if (Time.unscaledTime >= nextReport)
            {
                TerraCameraStatus(yaw, pitch, distance, target.x, target.z);
                nextReport = Time.unscaledTime + 0.1f;
            }
#endif
        }
    }
}
