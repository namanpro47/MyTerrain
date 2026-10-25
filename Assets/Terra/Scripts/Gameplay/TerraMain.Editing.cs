using System;
using System.Linq;
using Terra.Contracts;
using Terra.Core;
using UnityEngine;

namespace Terra.Gameplay
{
    public sealed partial class TerraMain
    {
        readonly System.Collections.Generic.Dictionary<string,string> displayedAnnual=new System.Collections.Generic.Dictionary<string,string>();
        string editMode="place",moveSourceId;
        AnnualTownSession annual;
        AnnualTownDefinition annualDefinition;
        TownYearOutcomes annualBaseline;
        string availabilityKey;
        string[] availableFieldIds=Array.Empty<string>();
        string PreviewSiteId => editMode=="reset"?null:editMode=="move"?hoverId:editMode=="remove"?selectedId:hoverId??selectedId;
        string QueuedAt(string siteId)=>annual?.State.queue.FirstOrDefault(q=>q.siteId==siteId)?.actionId;
        void Choose(string id)
        {
            if(!ready||hud.AnnualOverviewOpen)return;if(treatmentId!=id)Terra.Audio.TerraAudio.Ensure().Request("tool-select",3);editMode="place";moveSourceId=null;treatmentId=id;hud.Selected(id);UpdatePreview();BrowserPerformance.Feedback(0);
        }
        void Cancel()
        {
            if(hud.ModalOpen){hud.CloseModal();return;}if(hud.AnnualOverviewOpen){hud.CloseAnnualOverview();return;}if(!ready)return;
            if(!string.IsNullOrEmpty(treatmentId)||editMode!="place")Cue(cancelSound);
            pressed=false;uiDown=null;editMode="place";moveSourceId=null;treatmentId="";hoverId=null;
            landscape.ClearGhost();hud.Selected("");UpdatePreview();
        }
        void SetHover(Site site){string id=site?.id;if(id==hoverId)return;hoverId=id;UpdatePreview();BrowserPerformance.Feedback(0);}
        bool SelectEditableProposal()
        {
            if(hoverId!=null&&QueuedAt(hoverId)!=null)selectedId=hoverId;
            if(selectedId!=null&&QueuedAt(selectedId)!=null)return true;
            BlockedCue();Toast("Select a queued project. Accepted projects cannot be refunded or moved.");return false;
        }
        void BeginMove()
        {
            if(!ready||hud.ModalOpen||hud.AnnualOverviewOpen||!SelectEditableProposal())return;
            Terra.Audio.TerraAudio.Ensure().Request("tool-select",3);moveSourceId=selectedId;editMode="move";treatmentId=QueuedAt(selectedId);hoverId=null;
            hud.Selected("");UpdatePreview();Toast("Choose an empty destination for this year's proposal.");
        }
        void BeginRemove()
        {if(!ready||hud.ModalOpen||hud.AnnualOverviewOpen||!SelectEditableProposal())return;Terra.Audio.TerraAudio.Ensure().Request("tool-select",3);editMode="remove";hoverId=selectedId;hud.Selected("");UpdatePreview();}
        void BeginReset()
        {if(!ready||hud.ModalOpen||hud.AnnualOverviewOpen||annual.State.queue.Length==0)return;Terra.Audio.TerraAudio.Ensure().Request("tool-select",3);editMode="reset";hud.Selected("");UpdatePreview();}
        string Rejection(Site site)
        {
            if(hud.AnnualOverviewOpen)return "Return to the plan before editing.";
            if(fastForwarding)return "Years are advancing; wait for the accepted future.";
            if(annual.State.phase==TownPhase.Completed)return "Year 10 is complete. Inspect your future or start a new run.";
            if(annual.State.phase!=TownPhase.Planning)return "Review the accepted year, then continue planning.";
            if(editMode=="reset")return annual.State.queue.Length==0?"No queued projects to clear.":null;
            if(site==null)return "Choose a field on the map or use Tab to inspect.";
            if(editMode=="remove")return QueuedAt(site.id)==null?"Only queued projects can be removed.":null;
            if(string.IsNullOrEmpty(treatmentId))return "Choose an action to propose here.";
            if(annual.State.assets.Any(a=>a.siteId==site.id))return "Accepted project: permanent for this run. Choose another field.";
            if(!FullAnnualGameplay.Supported.Contains(treatmentId))return "This asset's annual recipe is not ready for play yet.";
            string physical=buildActionRules.Rejection(site,treatmentId);if(physical!=null)return physical;
            if(editMode=="place"&&QueuedAt(site.id)==treatmentId)return "Queued for this year. Move, remove or replace before End Year.";
            try
            {
                long available=editMode=="move"?annual.Engine.PreviewMove(moveSourceId,site.id):annual.Preview(site.id,treatmentId);
                return available<0?"Not enough available money for construction and the full upkeep term.":null;
            }
            catch(ArgumentException e){return e.Message;}
        }
        void UpdatePreview()
        {
            if(!ready||annual==null)return;
            var timer=System.Diagnostics.Stopwatch.StartNew();
            // Re-evaluate only when the plan/tool changes, not for every pointer or camera move.
            var state=annual.State;
            bool showAvailable=!hud.AnnualOverviewOpen&&!comparisonMap&&!fastForwarding&&state.phase==TownPhase.Planning
                &&(editMode=="place"||editMode=="move")&&!string.IsNullOrEmpty(treatmentId);
            string nextKey=state.runId+"/"+state.revision+"/"+state.phase+"/"+editMode+"/"+moveSourceId+"/"+treatmentId+"/"+showAvailable;
            if(nextKey!=availabilityKey)
            {
                availabilityKey=nextKey;
                availableFieldIds=showAvailable?world.sites.Where(s=>Rejection(s)==null).Select(s=>s.id).ToArray():Array.Empty<string>();
                hud.BuildAvailability(showAvailable?(int?)availableFieldIds.Length:null);
            }
            if(hud.AnnualOverviewOpen){landscape.ClearGhost();hud.HideAnnualPreview();return;}
            if(comparisonMap){landscape.ClearGhost();landscape.Highlight(null,null,true);hud.HideAnnualPreview();return;}
            var site=world.sites.FirstOrDefault(s=>s.id==PreviewSiteId);
            string reason=Rejection(site);var recipe=annualDefinition.actions.FirstOrDefault(a=>a.siteId==site?.id&&a.actionId==(string.IsNullOrEmpty(treatmentId)?QueuedAt(site?.id):treatmentId));
            landscape.ClearGhost();landscape.Highlight(site?.id,selectedId,state.phase==TownPhase.Completed||reason==null||string.IsNullOrEmpty(treatmentId),availableFieldIds);
            if(site!=null&&(editMode=="place"||editMode=="move")&&reason==null)landscape.Ghost(site,true,treatmentId);
            if(site!=null&&editMode=="remove"&&reason==null)landscape.PreviewBaseline(site);
            bool water=treatmentId=="water-swale"&&site!=null&&site.allowedTreatments.Contains("water-swale");
            landscape.ShowRunoffConnections(water?runoff.ConnectedSites(site.id):Array.Empty<string>());
            hud.AnnualPreview(site,recipe,annual.State,editMode,reason,ConfirmEdit,QueuedAt(site?.id)!=null);
            performance.Preview(timer.Elapsed.TotalMilliseconds);
        }
        void Commit(){if(!ready||hud.ModalOpen||hud.AnnualOverviewOpen)return;ConfirmEdit();}
        void ConfirmEdit()
        {
            if(!ready||hud.ModalOpen||hud.AnnualOverviewOpen)return;
            var site=world.sites.FirstOrDefault(s=>s.id==PreviewSiteId);string reason=Rejection(site);
            if(reason!=null){Toast(reason);BlockedCue();return;}
            var timer=System.Diagnostics.Stopwatch.StartNew();int revision=annual.State.revision;
            try
            {
                if(editMode=="reset")annual.ClearPlan(revision);
                else if(editMode=="remove")annual.RemoveQueuedAction(site.id,revision);
                else if(editMode=="move")annual.MovePlan(moveSourceId,site.id,revision);
                else annual.QueueAction(site.id,treatmentId,revision);
                PlacementCue(treatmentId,editMode,site);
                selectedId=site?.id;editMode="place";moveSourceId=null;hoverId=null;treatmentId="";hud.Selected("");
                SaveAnnual();RefreshLandscape();Refresh();Toast("Plan updated. Money reserved; payment happens at End Year.");
                performance.Placement(timer.Elapsed.TotalMilliseconds);BrowserPerformance.Feedback(1);
            }
            catch(Exception e){BlockedCue();Toast(e.Message);}
        }
        void Undo()=>History(false);
        void Redo()=>History(true);
        void History(bool redo)
        {
            if(!ready||hud.ModalOpen||hud.AnnualOverviewOpen||annual.State.phase!=TownPhase.Planning)return;
            int beforeRevision=annual.State.revision;
            if(redo)annual.RedoPlan(annual.State.revision);else annual.UndoPlan(annual.State.revision);
            if(annual.State.revision!=beforeRevision)Terra.Audio.TerraAudio.Ensure().Request(redo?"redo":"undo",5);
            editMode="place";moveSourceId=null;hoverId=null;treatmentId="";hud.Selected("");
            SaveAnnual();RefreshLandscape();Refresh();Toast(redo?"Queued plan restored.":"Queued edit undone. Accepted years unchanged.");
        }
        void RefreshLandscape()
        {
            DisplayAnnual(annual.State);
        }
        void DisplayAnnual(AnnualTownSnapshot snapshot)
        {
            landscape.ClearGhost();
            foreach(var site in world.sites)
            {
                var queued=snapshot.queue.FirstOrDefault(q=>q.siteId==site.id);
                var asset=snapshot.assets.FirstOrDefault(a=>a.siteId==site.id);
                var result=snapshot.history.LastOrDefault()?.outcomes.sites.FirstOrDefault(s=>s.siteId==site.id);
                string visualKey=(queued?.actionId??asset?.actionId??"baseline")+"/"+(queued!=null?"PROPOSED":result?.phase.ToString())+"/"+(result?.maturity??0).ToString("R")+"/"+(result?.condition??1).ToString("R");
                if(displayedAnnual.TryGetValue(site.id,out var previous)&&previous==visualKey)continue;
                displayedAnnual[site.id]=visualKey;
                // Proposals show intended planting; accepted vegetation follows the saved annual outcome.
                landscape.Apply(site,queued?.actionId??asset?.actionId,queued!=null?1:(float)(result?.maturity??1));
                landscape.AnnualStage(site,queued!=null?"PROPOSED":asset==null?null:result?.phase.ToString().ToUpperInvariant(),result?.maturity??0);
            }
        }
        void Refresh()
        {
            var forecast=AnnualGameplay.Forecast(annual.Engine).outcomes;
            hud.AnnualState(annual.Engine,annualBaseline,forecast);
            hud.AnnualWater(AnnualEconomy.Water(annualDefinition,annual.State.history.LastOrDefault()?.outcomes??annualBaseline),AnnualEconomy.Water(annualDefinition,forecast));
            UpdatePreview();
        }
        void InitializeAnnual(string packId)
        {
            annualDefinition=FullAnnualGameplay.Create(world,scenario,packId,buildActionRules,runoff);
            annualBaseline=AnnualGameplay.Baseline(annualDefinition);
            annual=gameObject.AddComponent<AnnualTownSession>();annual.saveKey=location.IsWisconsin?"terra.meadowbrook.annual-full.v3.2":location.AnnualSaveKey;
            string recovery=null;introPending=!PlayerPrefs.HasKey(annual.saveKey);
            try{annual.Initialize(annualDefinition,Guid.NewGuid().ToString("N"),true);}
            catch(Exception e)
            {
                // Preserve incompatible data for recovery; never silently overwrite it with a fresh run.
                PlayerPrefs.SetString(annual.saveKey+".recovery",PlayerPrefs.GetString(annual.saveKey));
                annual.Initialize(annualDefinition,Guid.NewGuid().ToString("N"));recovery=e.Message;
            }
            hud.ConfigureAnnual(annualDefinition,ReviewYear,ShowAcceptedReport,NewRun);
            hud.ConfigureFuture(ReviewFuture);
            InitializeChapters(ref recovery);
            if(recovery!=null)Toast("Saved run could not be restored; a recovery copy was kept. "+recovery);
        }
        void SaveAnnual(){try{annual.Save();chapters?.Save();}catch(Exception e){BlockedCue();Debug.LogWarning("Annual save failed: "+e.Message);Toast("This session is playable, but the browser could not save progress.");}}
        void ReviewYear()
        {
            if(fastForwarding)return;
            if(annual.State.phase==TownPhase.Resolving){ShowPendingChapter();return;}
            if(annual.State.phase==TownPhase.Reviewing||annual.State.phase==TownPhase.Completed){ShowAcceptedReport();return;}
            hud.AnnualConfirmation(annual.Engine,AnnualGameplay.Forecast(annual.Engine),()=>{
                hud.CloseModal();
                try{if(OnlineChapters)chapters.ResolveOnline();else chapters.ResolveOffline();}
                catch(Exception e){BlockedCue();Toast("Year has not completed: "+e.Message);}
            },OnlineChapters);
        }
        void ShowAcceptedReport()
        {
            if(annual.State.history.Length==0)return;
            var chapter=chapters.ForYear(annual.State.history.Last().year);
            comparisonMap=false;hud.MapView(null);RefreshLandscape();
            landscape.ClearGhost();landscape.Highlight(null,null,true);landscape.ShowRunoffConnections(Array.Empty<string>());
            hud.AnnualOverview(annual.Engine,ContinueAfterChapter,chapter==null?null:(Action)(()=>hud.AnnualChapterReport(annual.Engine,chapter,ContinueAfterChapter,ShowAnnualLedger)),ShowAnnualLedger,id=>{
                selectedId=hoverId=id;landscape.Highlight(id,id,true);FocusSite();
            });
        }
        void NewRun()
        {
            if(fastForwarding)return;
            hud.AnnualDialog("Start a new run?","This replaces this browser's current run. Accepted years cannot be undone. A copy of this run will be kept locally.","Start new run",()=>{
                hud.CloseAnnualOverview();comparisonTown=null;comparisonMap=false;hud.MapView(null);introPending=true;
                PlayerPrefs.SetString(annual.saveKey+".previous",annual.ExportSave());
                PlayerPrefs.SetString(chapters.saveKey+".previous",chapters.ExportSave());
                chapters.StateChanged-=ChapterChanged;chapters.StopRequest();annual.Initialize(annualDefinition,Guid.NewGuid().ToString("N"));
                chapters.ResetForRun();chapters.StateChanged+=ChapterChanged;
                hud.CloseModal();selectedId=hoverId=null;treatmentId="";editMode="place";hud.Selected("");SaveAnnual();RefreshLandscape();Refresh();BeginAnnualArrival();BrowserPerformance.Memory("new-run");
            });
        }
    }
}
