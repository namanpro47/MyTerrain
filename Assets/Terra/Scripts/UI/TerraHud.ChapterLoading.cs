using UnityEngine;

namespace Terra.UI
{
    public sealed partial class TerraHud
    {
        ChapterLoadingIndicator chapterLoading;
        void ShowChapterLoading()
        {
            if(chapterLoading==null)
            {
                var track=Panel(modal.transform,"AI explanation loading",new Vector2(.5f,0),new Vector2(0,200),new Vector2(640,12),Raised);
                var fill=Panel(track,"Waiting indicator",new Vector2(0,.5f),Vector2.zero,new Vector2(160,12),Accent);
                chapterLoading=track.gameObject.AddComponent<ChapterLoadingIndicator>();
                chapterLoading.track=(RectTransform)track;chapterLoading.fill=(RectTransform)fill;
            }
            chapterLoading.body=modalText;chapterLoading.expectedBody=modalText.text;
            chapterLoading.Begin();
        }
        void HideChapterLoading(){if(chapterLoading!=null)chapterLoading.gameObject.SetActive(false);}
    }
}
