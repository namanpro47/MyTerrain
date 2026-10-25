using System.Linq;
using TMPro;

namespace Terra.Gameplay
{
    public sealed partial class WorldAtlas
    {
        public string NarrationText => notes!=null&&notes.activeInHierarchy
            ? string.Join("\n",notes.GetComponentsInChildren<TextMeshProUGUI>().Where(t=>t.GetComponentInParent<UnityEngine.UI.Button>()==null).Select(t=>t.text)) : "";
    }
}
