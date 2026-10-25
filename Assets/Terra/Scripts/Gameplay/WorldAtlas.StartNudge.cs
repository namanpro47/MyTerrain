using UnityEngine;

namespace Terra.Gameplay
{
    public sealed partial class WorldAtlas
    {
        GameObject startNudge;
        bool startNudgeDismissed;

        // TN-03/06, AX-01: one short invitation, anchored to the actual destination.
        void BuildStartNudge()
        {
            var bubble=Box(choices[0].transform,"Start in Wisconsin",new Vector2(.5f,1),new Vector2(0,30),new Vector2(364,112));
            bubble.pivot=new Vector2(.5f,0);
            startNudge=bubble.gameObject;

            var shadow=Box(bubble,"Bubble shadow",Vector2.one*.5f,new Vector2(0,-5),bubble.sizeDelta+new Vector2(4,4));
            var shadowImage=shadow.gameObject.AddComponent<UnityEngine.UI.Image>();
            shadowImage.sprite=rounded;shadowImage.type=UnityEngine.UI.Image.Type.Sliced;shadowImage.color=new Color(0,.025f,.035f,.55f);shadowImage.raycastTarget=false;

            var face=Box(bubble,"Bubble paper",Vector2.one*.5f,Vector2.zero,bubble.sizeDelta);
            var paper=face.gameObject.AddComponent<UnityEngine.UI.Image>();
            paper.sprite=rounded;paper.type=UnityEngine.UI.Image.Type.Sliced;paper.color=Cream;

            var tail=Box(bubble,"Pointer to Wisconsin",new Vector2(.5f,0),Vector2.zero,new Vector2(40,26));
            tail.gameObject.AddComponent<CanvasRenderer>();
            var triangle=tail.gameObject.AddComponent<AtlasPolygon>();
            triangle.Points=new[]{new Vector2(-18,2),new Vector2(0,-24),new Vector2(18,2)};
            triangle.color=Cream;triangle.raycastTarget=false;

            var enter=startNudge.AddComponent<UnityEngine.UI.Button>();enter.targetGraphic=paper;
            enter.onClick.AddListener(()=>{Terra.Audio.TerraAudio.Ensure().Click();TravelTo(0);});
            var colors=enter.colors;colors.highlightedColor=new Color(1,.96f,.83f);colors.pressedColor=new Color(.91f,.86f,.70f);enter.colors=colors;

            Text(bubble,"Shape a Wisconsin valley over ten years, balancing people, nature and your budget.",new Vector2(0,1),new Vector2(23,-17),new Vector2(305,78),20,Navy);

            var close=Button(bubble,"×",new Vector2(1,1),new Vector2(-2,-2),new Vector2(34,34),DismissStartNudge);
            close.gameObject.name="Dismiss start invitation";close.image.color=Color.clear;
            close.GetComponentInChildren<TMPro.TextMeshProUGUI>().color=new Color(.26f,.39f,.37f);
        }

        void DismissStartNudge(){startNudgeDismissed=true;RefreshStartNudge();}
        void RefreshStartNudge()
        {
            if(startNudge==null)return;
            bool show=!startNudgeDismissed&&!Traveling&&Selected==0&&!notes.activeSelf&&!options.activeSelf;
            startNudge.SetActive(show);
        }
    }
}
