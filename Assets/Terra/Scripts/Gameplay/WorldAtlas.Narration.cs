using System.Linq;
using TMPro;

namespace Terra.Gameplay
{
    public sealed partial class WorldAtlas
    {
        public string NarrationText
        {
            get
            {
                var surface=notes!=null&&notes.activeInHierarchy?notes.transform:null;
                // The optional invitation is owned by the atlas; avoid a compile dependency
                // while its small UI addition travels through the shared release.
                if(surface==null&&choices!=null&&choices.Length>0&&choices[0]!=null)
                {
                    var invitation=choices[0].transform.Find("Start in Wisconsin");
                    if(invitation!=null&&invitation.gameObject.activeInHierarchy)surface=invitation;
                }
                if(surface==null)return "";
                return string.Join("\n",surface.GetComponentsInChildren<TextMeshProUGUI>().Where(t=>
                {
                    var button=t.GetComponentInParent<UnityEngine.UI.Button>();
                    return button==null||button.transform==surface;
                }).Select(t=>t.text));
            }
        }
    }
}
