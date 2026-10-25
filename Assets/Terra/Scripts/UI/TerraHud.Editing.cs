using System;
using System.Linq;
using Terra.Contracts;
using TMPro;
using UnityEngine;

namespace Terra.UI
{
    // TN-05 / CS-03: selected proposal actions stay contextual, preview precedes every edit.
    public sealed partial class TerraHud
    {
        UnityEngine.UI.Button moveButton,removeButton,redoButton,resetProposalsButton,confirmEditButton;
        TextMeshProUGUI coverSummary;
        Action confirmEdit;
        public void ConfigureEditing(Action move,Action remove,Action redo,Action resetProposals)
        {
            redoButton=Button(root,"Redo  Y",new Vector2(0,0),new Vector2(24,148),new Vector2(130,56),redo,16);
            resetProposalsButton=Button(root,"Clear all  X",new Vector2(0,0),new Vector2(24,212),new Vector2(130,56),resetProposals,16);
            redoButton.transform.SetSiblingIndex(modalShade.transform.GetSiblingIndex());
            resetProposalsButton.transform.SetSiblingIndex(modalShade.transform.GetSiblingIndex());
            moveButton=Button(tooltip.transform,"Move  V",new Vector2(0,0),new Vector2(18,16),new Vector2(122,48),move,16);
            removeButton=Button(tooltip.transform,"Remove",new Vector2(1,0),new Vector2(-18,16),new Vector2(122,48),remove,16);
            confirmEditButton=Button(tooltip.transform,"Confirm",new Vector2(.5f,0),new Vector2(0,16),new Vector2(256,48),()=>confirmEdit?.Invoke(),16);
            previewAction.rectTransform.anchoredPosition=new Vector2(18,76);
            coverSummary=Label(tooltip.transform,"",new Vector2(0,1),new Vector2(18,-225),new Vector2(256,38),14,Soft);
            moveButton.gameObject.SetActive(false);removeButton.gameObject.SetActive(false);confirmEditButton.gameObject.SetActive(false);
        }
        public void EditingPreview(string mode,Site site,Site proposalSite,string currentTreatment,Evaluation result,WorldResult current,string sourceName,bool canRedo,bool hasProposals,Action confirm)
        {
            confirmEdit=confirm;redoButton.interactable=canRedo;resetProposalsButton.interactable=hasProposals;
            bool editing=mode!="place";
            bool canEdit=proposalSite!=null&&currentTreatment!=null;
            moveButton.gameObject.SetActive(!editing&&canEdit&&!result.valid);removeButton.gameObject.SetActive(!editing&&canEdit&&!result.valid);
            confirmEditButton.gameObject.SetActive(editing||result.valid);
            if(!editing)SetButtonText(confirmEditButton,result.reason=="Ready to replace"?"Replace · Enter":"Place · Enter");confirmEditButton.interactable=result.valid;
            if(editing)
            {
                tooltip.SetActive(true);tip.transform.parent.gameObject.SetActive(false);
                previewKind.text=mode=="move"?"MOVE PROPOSAL":mode=="remove"?"REMOVE PROPOSAL":"CLEAR ALL PROPOSALS";
                previewKind.color=result.valid?Accent:new Color(1,.69f,.55f);
                previewTitle.text=mode=="reset"?"Return to baseline":mode=="move"?(site?.name??"Choose a destination"):site?.name??"Select a proposal";
                if(result.valid)
                {
                    int cost=result.result.spentCredits-current.spentCredits;float productionDelta=result.result.production-current.production;
                    previewBody.text=(mode=="move"?"From "+sourceName:mode=="reset"?"Restore all original cover.":"Restore original cover.")+
                        "\n\n"+(cost<0?(-cost)+" credits refunded":cost+" credits spent")+"\n"+(productionDelta>=0?"+":"")+productionDelta.ToString("0.#")+" illustrative units";
                }
                else previewBody.text=result.reason;
                previewAction.text="Enter to confirm · Esc to cancel";
                SetButtonText(confirmEditButton,mode=="move"?"Confirm move":mode=="remove"?"Confirm removal":"Clear proposals");
            }
            else if(site!=null&&currentTreatment!=null&&proposalSite?.id==site.id&&string.IsNullOrEmpty(activeTool))
            {
                previewKind.text="CURRENT PROPOSAL";previewBody.text="Move or remove this proposal, or choose a treatment to replace it.";previewAction.text="V: move · Delete: remove";
            }
            if(!editing&&canEdit&&site?.id!=proposalSite.id)
            {moveButton.gameObject.SetActive(false);removeButton.gameObject.SetActive(false);}
            var preview=result.valid?result.result:current;
            float Area(WorldResult r,string cover)=>r.coverAreas.Where(a=>a.coverId==cover).Sum(a=>a.areaM2)/10000;
            float vegetated=Area(preview,"grass")+Area(preview,"woodland");
            float delta=vegetated-Area(current,"grass")-Area(current,"woodland");
            coverSummary.text="Managed-site vegetation: "+vegetated.ToString("0.00")+" ha"+(result.valid?"\n"+(delta>=0?"+":"")+delta.ToString("0.00")+" ha from this edit":"");
            Paint();
        }
    }
}
