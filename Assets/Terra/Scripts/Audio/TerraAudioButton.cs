using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Terra.Audio
{
    // Native pointer feedback survives dialogs replacing Button.onClick listeners.
    // Web's manual pointer path and the HUD's keyboard dispatcher provide the same cue.
    public sealed class TerraAudioButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
    {
        public Action Hover;
        UnityEngine.UI.Button button;
        void Awake() { button=GetComponent<UnityEngine.UI.Button>(); }
        public void OnPointerClick(PointerEventData e)
        { if(e.button==PointerEventData.InputButton.Left && button!=null && button.IsInteractable()) TerraAudio.Instance?.Click(); }
        public void OnPointerEnter(PointerEventData e)
        { if(button!=null && button.IsInteractable()){TerraAudio.Instance?.Focus();Hover?.Invoke();} }
    }
}
