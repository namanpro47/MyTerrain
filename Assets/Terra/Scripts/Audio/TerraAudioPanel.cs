using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Terra.Audio
{
    // A single reusable uGUI panel for both atlas and landscape. It owns input while open.
    public sealed class TerraAudioPanel : MonoBehaviour
    {
        static TerraAudioPanel instance;
        public static bool IsOpen => instance != null && instance.panel != null && instance.panel.activeSelf;
        public static bool ConsumedInput => IsOpen || (instance != null && instance.closedFrame == Time.frameCount);
        GameObject panel; Canvas canvas; UnityEngine.UI.CanvasScaler scaler;
        readonly List<UnityEngine.UI.Button> buttons = new List<UnityEngine.UI.Button>();
        TextMeshProUGUI[] values; TextMeshProUGUI muteText; UnityEngine.UI.Button down, focused;
        Vector2 pressPoint; int focusIndex, closedFrame = -1, openedFrame = -1;
        GameObject previousSelection;
        static readonly Color Paper = new Color(.94f,.97f,.95f), Slate = new Color(.075f,.13f,.17f), Mint = new Color(.53f,.85f,.69f);
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] static extern float TerraPixelRatio();
#endif
        void Awake() { instance = this; }
        public static void Show()
        {
            TerraAudio.Ensure().Gesture();
            var p = instance; if (p.panel == null) p.Build();
            p.previousSelection = EventSystem.current?.currentSelectedGameObject;
            p.panel.SetActive(true); p.openedFrame = Time.frameCount; p.Refresh(); p.Select(0);
            TerraAudio.Instance.Request("ui-open", 3);
        }
        void Close()
        {
            panel.SetActive(false); closedFrame = Time.frameCount; down = null;
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(previousSelection);
            TerraAudio.Instance.Request("ui-close", 3);
        }
        void Update()
        {
            if (!IsOpen) return;
            float ratio = 1;
#if UNITY_WEBGL && !UNITY_EDITOR
            ratio = Mathf.Max(1, TerraPixelRatio());
#endif
            scaler.scaleFactor = ratio * Mathf.Min(1, (Screen.width / ratio - 24) / 560, (Screen.height / ratio - 24) / 550);
            var k = Keyboard.current;
            if (k != null && openedFrame != Time.frameCount)
            {
                if (k.escapeKey.wasPressedThisFrame || k.uKey.wasPressedThisFrame) { Close(); return; }
                if (k.tabKey.wasPressedThisFrame || k.rightArrowKey.wasPressedThisFrame || k.downArrowKey.wasPressedThisFrame)
                    Select(focusIndex + (k.shiftKey.isPressed ? -1 : 1));
                if (k.leftArrowKey.wasPressedThisFrame || k.upArrowKey.wasPressedThisFrame) Select(focusIndex - 1);
                if (k.enterKey.wasPressedThisFrame || k.spaceKey.wasPressedThisFrame) focused?.onClick.Invoke();
            }
        }
        public static void Pointer(int kind, Vector2 position)
        {
            if (!IsOpen) return;
            var p = instance;
            var hits = new List<RaycastResult>();
            if (EventSystem.current != null) EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = position }, hits);
            var b = hits.Count > 0 ? hits[0].gameObject.GetComponentInParent<UnityEngine.UI.Button>() : null;
            if (b != null && p.buttons.Contains(b) && b != p.focused) p.Select(p.buttons.IndexOf(b));
            if (kind == 0) { p.down = b; p.pressPoint = position; }
#if UNITY_WEBGL && !UNITY_EDITOR
            if (kind == 2 && b != null && b == p.down && Vector2.Distance(position, p.pressPoint) < 8 && b.IsInteractable()) b.onClick.Invoke();
#endif
            if (kind == 2 || kind == 3) p.down = null;
        }
        void Select(int index)
        {
            focusIndex = (index + buttons.Count) % buttons.Count; focused = buttons[focusIndex];
            foreach (var b in buttons) b.GetComponent<UnityEngine.UI.Outline>().enabled = b == focused;
            TerraAudio.Instance.Focus();
        }
        void Refresh()
        {
            for (int i = 0; i < values.Length; i++) values[i].text = Mathf.RoundToInt(TerraAudio.Instance.Level(i) * 100) + "%";
            muteText.text = TerraAudio.Instance.Muted ? "Unmute all sound" : "Mute all sound";
        }
        void Adjust(int index, float amount)
        {
            TerraAudio.Instance.SetLevel(index, TerraAudio.Instance.Level(index) + amount); Refresh();
            TerraAudio.Instance.Request("ui-confirm", 3);
        }
        void Build()
        {
            panel = new GameObject("Sound settings", typeof(RectTransform), typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
            panel.transform.SetParent(transform, false); canvas = panel.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 1000;
            scaler = panel.GetComponent<UnityEngine.UI.CanvasScaler>(); scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ConstantPixelSize;
            var shade = Box(panel.transform, "Sound backdrop", Vector2.zero, Vector2.zero);
            shade.anchorMin = Vector2.zero; shade.anchorMax = Vector2.one; shade.offsetMin = shade.offsetMax = Vector2.zero;
            shade.gameObject.AddComponent<UnityEngine.UI.Image>().color = new Color(.02f,.04f,.06f,.8f);
            var card = Box(panel.transform, "Sound card", Vector2.zero, new Vector2(560,550));
            card.gameObject.AddComponent<UnityEngine.UI.Image>().color = Slate;
            Text(card, "Sound & music", new Vector2(-244,218), new Vector2(488,38), 28, Paper);
            Text(card, "A quiet landscape, mixed your way.", new Vector2(-244,179), new Vector2(488,30), 18, Mint);
            values = new TextMeshProUGUI[4];
            string[] labels = { "Overall volume", "Music", "Nature ambience", "Actions & interface" };
            for (int i = 0; i < 4; i++)
            {
                int bus = i; float y = 120 - i * 58;
                Text(card, labels[i], new Vector2(-244,y), new Vector2(236,44), 20, Paper);
                Button(card, "−", new Vector2(42,y), new Vector2(48,44), () => Adjust(bus,-.1f));
                values[i] = Text(card, "", new Vector2(82,y), new Vector2(80,44), 20, Mint); values[i].alignment = TextAlignmentOptions.Center;
                Button(card, "+", new Vector2(202,y), new Vector2(48,44), () => Adjust(bus,.1f));
            }
            var mute = Button(card, "", new Vector2(0,-136), new Vector2(488,46), () => { TerraAudio.Instance.SetMuted(!TerraAudio.Instance.Muted); Refresh(); });
            muteText = mute.GetComponentInChildren<TextMeshProUGUI>();
            Text(card, "Saved on this device · Audio pauses when you leave the tab.\nU: sound settings · Tab / arrows: choose · Enter: adjust", new Vector2(-244,-184), new Vector2(488,48), 16, Paper);
            Button(card, "Done  ·  Esc", new Vector2(0,-239), new Vector2(220,44), Close);
        }
        RectTransform Box(Transform parent, string name, Vector2 position, Vector2 size)
        {
            var r = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            r.SetParent(parent,false); r.anchorMin = r.anchorMax = r.pivot = new Vector2(.5f,.5f); r.anchoredPosition = position; r.sizeDelta = size; return r;
        }
        TextMeshProUGUI Text(Transform parent, string value, Vector2 position, Vector2 size, float font, Color color)
        {
            var r = Box(parent, "Label", position, size); r.pivot = new Vector2(0,.5f);
            var t = r.gameObject.AddComponent<TextMeshProUGUI>(); t.font = TMP_Settings.defaultFontAsset; t.text = value; t.fontSize = font;
            t.color = color; t.raycastTarget = false; t.alignment = TextAlignmentOptions.MidlineLeft; return t;
        }
        UnityEngine.UI.Button Button(Transform parent, string label, Vector2 position, Vector2 size, Action action)
        {
            var r = Box(parent, label, position, size); var image = r.gameObject.AddComponent<UnityEngine.UI.Image>(); image.color = new Color(.14f,.24f,.28f);
            var b = r.gameObject.AddComponent<UnityEngine.UI.Button>(); b.targetGraphic = image;
            var line = r.gameObject.AddComponent<UnityEngine.UI.Outline>(); line.effectDistance = new Vector2(2,2); line.effectColor = Mint;
            b.onClick.AddListener(() => { TerraAudio.Instance.Click(); action(); });
            var t = Text(r,label,new Vector2(-size.x*.5f,0),size,18,Paper); t.alignment = TextAlignmentOptions.Center;
            int index=buttons.Count;var feedback=r.gameObject.AddComponent<TerraAudioButton>();feedback.Hover=()=>Select(index);
            buttons.Add(b); return b;
        }
        void OnDestroy() { if (instance == this) instance = null; }
    }
}
