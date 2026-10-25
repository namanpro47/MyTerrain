using System.Linq;
using Terra.Audio;
using TMPro;
using UnityEngine;

namespace Terra.UI
{
    public sealed partial class TerraHud
    {
        // Additive adapter: the dialogue owner can keep changing the layout independently.
        UnityEngine.UI.Button narrationToggle,narrationReplay;
        public string NarrationText
        {
            get
            {
                if(ModalOpen)
                {
                    string text=modalText.text;
                    if(fieldNotesOpen)
                        text+="\n"+fieldNotesBody.text+"\n"+string.Join("\n",fieldNotesRows.SelectMany(r=>r.GetComponentsInChildren<TextMeshProUGUI>()).Select(t=>t.text));
                    return text;
                }
                if(AnnualOverviewOpen)return yearOverviewTitle.text+"\n"+string.Join("\n",highlightTitles.Where(t=>t!=null&&t.gameObject.activeInHierarchy).Select((t,i)=>t.text+". "+highlightDetails[i].text));
                return "";
            }
        }
        public void NarrationControls()
        {
            if(narrationToggle==null)
            {
                narrationToggle=Button(modal.transform,"Voice: on",new Vector2(1,1),new Vector2(-170,-14),new Vector2(130,30),()=>{var n=TerraNarration.Ensure();n.SetEnabled(!n.Enabled);},14);
                narrationReplay=Button(modal.transform,"Replay voice",new Vector2(1,1),new Vector2(-32,-14),new Vector2(130,30),()=>TerraNarration.Ensure().Replay(),14);
            }
            var voice=TerraNarration.Ensure();SetButtonText(narrationToggle,voice.Enabled?"Voice: on":"Voice: off");
            SetButtonText(narrationReplay,voice.Status==4?"Retry voice":voice.Status==1?"Loading voice…":"Replay voice");
            narrationReplay.interactable=voice.Enabled;
        }
    }
}
