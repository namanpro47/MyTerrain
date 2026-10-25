using System;
using Terra.Core;
using UnityEngine;

namespace Terra.UI
{
    public sealed partial class TerraHud
    {
        UnityEngine.UI.Button fullLandscapeFuture;

        public void AnnualRunSetup(int selected, Action<int> begin)
        {
            FieldNotes("How many years will you make decisions?",
                "Choose how long you want to plan. Every game still follows the community for ten years.\n\n"+
                "After your final decision year, projects keep developing, upkeep is paid and yearly conditions still apply. The remaining years advance to your Year 10 review without more building decisions.",
                "Begin · "+selected+(selected==1?" decision year":" decision years"),()=>begin(selected));
            var row=Rect(fieldNotesContent,"Decision years",new Vector2(0,1),Vector2.zero,new Vector2(656,58));
            row.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().preferredHeight=58;
            var layout=row.gameObject.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            layout.spacing=12;layout.childControlWidth=true;layout.childControlHeight=true;layout.childForceExpandWidth=true;layout.childForceExpandHeight=true;
            fieldNotesRows.Add(row.gameObject);
            foreach(int years in new[]{1,3,5,10})
            {
                int choice=years;
                var button=Button(row,years+(years==1?" year":" years"),Vector2.zero,Vector2.zero,new Vector2(150,58),()=>AnnualRunSetup(choice,begin),18);
                button.name="Choose "+years+" decision years";
                button.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().flexibleWidth=1;
                button.GetComponent<UnityEngine.UI.Image>().color=selected==years?new Color(.17f,.49f,.37f):Raised;
            }
            fieldNotesLayoutDirty=true;LayoutFieldNotes();
        }

        public void FullLandscapeOption(AnnualTown town)
        {
            if(fullLandscapeFuture==null)
            {
                fullLandscapeFuture=Button(root,"All plots assigned · Year 10 →",new Vector2(0,1),new Vector2(16,-190),new Vector2(310,48),()=>futureShortcut?.Invoke(),15);
                fullLandscapeFuture.name="Full landscape future";
                var group=fullLandscapeFuture.gameObject.AddComponent<CanvasGroup>();
                var visibility=fullLandscapeFuture.gameObject.AddComponent<HudPlacementAnchor>();
                visibility.show=()=>!AnnualOverviewOpen&&!ModalOpen;
                visibility.tick=()=>{group.interactable=group.blocksRaycasts=!AnnualOverviewOpen&&!ModalOpen;};
                fullLandscapeFuture.transform.SetSiblingIndex(modalShade.transform.GetSiblingIndex());
            }
            bool show=town.AllPlotsAssigned&&(town.Phase==Terra.Contracts.TownPhase.Planning||town.Phase==Terra.Contracts.TownPhase.Reviewing);
            fullLandscapeFuture.gameObject.SetActive(show);
        }
    }
}
