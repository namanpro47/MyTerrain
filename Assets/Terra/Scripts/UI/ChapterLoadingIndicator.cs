using TMPro;
using UnityEngine;

namespace Terra.UI
{
    // An indeterminate activity indicator: never implies a fabricated completion percent.
    public sealed class ChapterLoadingIndicator : MonoBehaviour
    {
        public RectTransform track,fill;
        public TMP_Text body;
        public string expectedBody;
        bool invalidated;
        public void Begin(){invalidated=false;gameObject.SetActive(true);}
        void LateUpdate()
        {
            if(invalidated||body==null||body.text!=expectedBody){gameObject.SetActive(false);return;}
            float distance=Mathf.Max(0,track.rect.width-fill.rect.width);
            fill.anchoredPosition=new Vector2(Mathf.PingPong(Time.unscaledTime*.65f,1)*distance,0);
        }
        // A closed dialog must not revive its indicator when another modal opens.
        void OnDisable(){invalidated=true;}
    }
}
