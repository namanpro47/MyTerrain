using System;
using System.Linq;
using Terra.Contracts;
using UnityEngine;

namespace Terra.Gameplay
{
    public sealed partial class TerraMain
    {
        // Interaction intent never mutates proposals. All changes go through one revision-checked command.
        string editMode="place",moveSourceId;
        string PreviewSiteId => editMode=="reset"?null:editMode=="move"?hoverId:editMode=="remove"?selectedId:hoverId??selectedId;
        void Choose(string id)
        {
            if(!ready)return;editMode="place";moveSourceId=null;treatmentId=id;hud.Selected(id);UpdatePreview();
        }
        void Cancel()
        {
            if(hud.ModalOpen){hud.CloseModal();return;}
            if(!ready)return;
            pressed=false;uiDown=null;editMode="place";moveSourceId=null;treatmentId="";hoverId=null;
            landscape.ClearGhost();hud.Selected("");UpdatePreview();
        }
        void SetHover(Site site)
        {
            string id=site?.id;if(id==hoverId)return;hoverId=id;UpdatePreview();
        }
        EditCommand Command()
        {
            var command=new EditCommand{operation=editMode,siteId=PreviewSiteId,treatmentId=treatmentId,expectedRevision=state.Revision};
            if(editMode=="place")command.operation=state.TreatmentAt(command.siteId)==null?"add":"replace";
            if(editMode=="move"){command.siteId=moveSourceId;command.destinationSiteId=hoverId;}
            return command;
        }
        void BeginMove()
        {
            if(!ready||hud.ModalOpen)return;
            if(!SelectEditableProposal())return;
            moveSourceId=selectedId;editMode="move";treatmentId=state.TreatmentAt(selectedId);hoverId=null;
            hud.Selected("");UpdatePreview();Toast("Choose a destination. The source stays unchanged until you confirm.");
        }
        void BeginRemove()
        {
            if(!ready||hud.ModalOpen||!SelectEditableProposal())return;
            editMode="remove";moveSourceId=null;hoverId=selectedId;hud.Selected("");UpdatePreview();
        }
        void BeginReset()
        {
            if(!ready||hud.ModalOpen||!state.HasProposals)return;
            editMode="reset";moveSourceId=null;hud.Selected("");UpdatePreview();
        }
        bool SelectEditableProposal()
        {
            if(hoverId!=null&&state.TreatmentAt(hoverId)!=null)selectedId=hoverId;
            if(selectedId!=null&&state.TreatmentAt(selectedId)!=null)return true;
            Toast("Select a proposal first. Click an occupied site or inspect it with Tab.");return false;
        }
        void UpdatePreview()
        {
            if(!ready)return;
            var timer=System.Diagnostics.Stopwatch.StartNew();var command=Command();
            var site=world.sites.FirstOrDefault(s=>s.id==PreviewSiteId);
            var treatment=scenario.treatments.FirstOrDefault(t=>t.id==treatmentId);
            var result=state.Preview(command);
            bool alreadyPlaced=editMode=="place"&&site!=null&&state.TreatmentAt(site.id)==treatmentId;
            landscape.ClearGhost();
            landscape.Highlight(editMode=="reset"?null:site?.id,selectedId,result.valid||alreadyPlaced||string.IsNullOrEmpty(treatmentId));
            if(editMode=="place"||editMode=="move")
            {
                if(site!=null&&treatment!=null&&!alreadyPlaced&&(editMode!="move"||hoverId!=null))landscape.Ghost(site,result.valid,treatmentId);
            }
            if(editMode=="remove"&&result.valid)landscape.PreviewBaseline(site);
            hud.Preview(site,treatment,result,state.Result);
            var proposalSite=world.sites.FirstOrDefault(s=>s.id==(hoverId!=null&&state.TreatmentAt(hoverId)!=null?hoverId:selectedId));
            hud.EditingPreview(editMode,site,proposalSite,state.TreatmentAt(proposalSite?.id),result,state.Result,
                world.sites.FirstOrDefault(s=>s.id==moveSourceId)?.name,
                state.CanRedo,state.HasProposals,ConfirmEdit);
            performance.Preview(timer.Elapsed.TotalMilliseconds);
            if(!committing)BrowserPerformance.Feedback(0);
        }
        void Commit()
        {
            if(!ready||hud.ModalOpen)return;
            var siteId=PreviewSiteId;
            // Clicking an occupied site first selects it; changing its treatment is explicit on the next confirmation.
            if(editMode=="place"&&siteId!=null&&state.TreatmentAt(siteId)!=null&&selectedId!=siteId)
            {selectedId=siteId;UpdatePreview();return;}
            if(editMode=="place"&&(siteId==null||string.IsNullOrEmpty(treatmentId)))return;
            ConfirmEdit();
        }
        void ConfirmEdit()
        {
            if(!ready||hud.ModalOpen)return;
            var command=Command();var timer=System.Diagnostics.Stopwatch.StartNew();var before=state.Result;
            var previous=world.sites.ToDictionary(s=>s.id,s=>state.TreatmentAt(s.id));
            var result=state.Commit(command);if(!result.valid){Toast(result.reason);return;}
            committing=true;
            landscape.ClearGhost();
            foreach(var site in world.sites)
                if(previous[site.id]!=state.TreatmentAt(site.id))landscape.Apply(site,state.TreatmentAt(site.id));
            selectedId=command.operation=="move"?command.destinationSiteId:command.operation=="reset"?null:command.siteId;
            if(command.operation=="remove")selectedId=null;
            string verb=command.operation=="move"?"Moved":command.operation=="remove"?"Removed":command.operation=="reset"?"Cleared proposals":command.operation=="replace"?"Replaced":"Placed";
            editMode="place";moveSourceId=null;hoverId=null;
            // Keep the selected proposal visible without immediately previewing another edit.
            if(command.operation=="remove"||command.operation=="reset")treatmentId="";
            hud.Selected(treatmentId);Refresh();
            if(selectedId!=null&&state.TreatmentAt(selectedId)!=null)landscape.Settle(selectedId,hud.ReducedMotion);
            if(!hud.Muted&&placementSound!=null)sound.PlayOneShot(placementSound);
            float change=result.result.production-before.production;int cost=result.result.spentCredits-before.spentCredits;
            Toast(verb+": "+(change>=0?"+":"")+change.ToString("0.#")+" illustrative units · "+(cost<0?(-cost)+" credits refunded":cost+" credits spent"));
            performance.Placement(timer.Elapsed.TotalMilliseconds);committing=false;BrowserPerformance.Feedback(1);
            Debug.Log("TERRA_EDIT: "+command.operation+"; "+Diagnostic);
        }
        void Undo()=>History(false);
        void Redo()=>History(true);
        void History(bool redo)
        {
            if(!ready||hud.ModalOpen)return;
            var previous=world.sites.ToDictionary(s=>s.id,s=>state.TreatmentAt(s.id));
            if(!(redo?state.Redo():state.Undo()))return;
            landscape.ClearGhost();
            foreach(var site in world.sites)if(previous[site.id]!=state.TreatmentAt(site.id))landscape.Apply(site,state.TreatmentAt(site.id));
            editMode="place";moveSourceId=null;hoverId=null;treatmentId="";
            if(state.TreatmentAt(selectedId)==null)selectedId=null;
            hud.Selected("");Refresh();Toast(redo?"Proposal redone. Cover and credits restored.":"Proposal undone. Previous cover and credits restored.");
            Debug.Log((redo?"TERRA_REDO: ":"TERRA_UNDO: ")+Diagnostic);
        }
        void Refresh(){hud.UpdateResult(state.Result,state.CanUndo,scenario.objectives.productionMin);UpdatePreview();}
    }
}
