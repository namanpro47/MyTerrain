using System;
using Terra.Contracts;
using UnityEngine;

namespace Terra.Gameplay
{
    public sealed partial class TerraMain
    {
        public string annualChapterEndpoint = "http://127.0.0.1:8787/chapter";
        AnnualChapterSession chapters;
        bool OnlineChapters => Uri.TryCreate(annualChapterEndpoint,UriKind.Absolute,out var endpoint)
            && (endpoint.Scheme=="https" || Application.platform!=RuntimePlatform.WebGLPlayer
                || (Uri.TryCreate(Application.absoluteURL,UriKind.Absolute,out var page)&&page.IsLoopback));

        void InitializeChapters(ref string recovery)
        {
            chapters=gameObject.AddComponent<AnnualChapterSession>();chapters.town=annual;
            chapters.endpoint=annualChapterEndpoint;chapters.saveKey=Terra.Core.UniformParcels.Applied(world)?location.ParcelChapterSaveKey:location.IsWisconsin?"terra.meadowbrook.annual-full.chapters.v3.2":location.ChapterSaveKey;
            // The loopback C# service accepts a frozen game request; the hosted JS
            // service accepts projected facts and uses guest sessions.
            chapters.useJavaScriptBackend=AnnualChapterSession.UsesHostedProtocol(annualChapterEndpoint);
            chapters.saveKey+=".player-rules-v5";
            try{chapters.RestoreSaved();}
            catch(Exception e)
            {
                PlayerPrefs.SetString(chapters.saveKey+".recovery",PlayerPrefs.GetString(chapters.saveKey));
                recovery="Chapter save could not be restored; the annual ledger is still available. "+e.Message;
            }
            chapters.StateChanged+=ChapterChanged;
        }
        void RestoreChapterView()
        {
            if(annual.State.phase==TownPhase.Resolving)ShowPendingChapter();
            else if(annual.State.phase==TownPhase.Reviewing||annual.State.phase==TownPhase.Completed)ShowAcceptedReport();
        }
        void ChapterChanged()
        {
            if(!ready)return;
            SaveAnnual();
            if(fastForwarding)return;
            RefreshLandscape();Refresh();
            if(chapters.Busy)ShowChapterWaiting();
            else if(annual.State.phase==TownPhase.Resolving)ShowPendingChapter();
            else if(annual.State.phase==TownPhase.Reviewing||annual.State.phase==TownPhase.Completed)ShowChapterResult();
        }
        void ShowChapterWaiting()
        {
            hud.CloseAnnualOverview();
            bool saved=annual.Engine.PlayerRulesActive&&annual.State.history.Length>0
                &&(annual.State.phase==TownPhase.Reviewing||annual.State.phase==TownPhase.Completed);
            hud.AnnualChapterWaiting(annual.State.year,()=>chapters.StopRequest(),saved);
        }
        void ShowChapterResult()
        {
            ShowAcceptedReport();
            var chapter=chapters.ForYear(annual.State.year);
            if(chapter!=null)hud.AnnualChapterReport(annual.Engine,chapter,ContinueAfterChapter,ShowAnnualLedger);
        }
        void ShowPendingChapter()
        {
            if(chapters.Busy){ShowChapterWaiting();return;}
            hud.AnnualChapterRecovery(annual.State.year,chapters.Error,()=>chapters.ResolveOnline(),()=>chapters.ResolveOffline(),()=>{
                hud.CloseModal();chapters.CancelResolution();
            });
        }
        void ContinueAfterChapter()
        {
            hud.CloseAnnualOverview();hud.CloseModal();
            if(annual.State.phase==TownPhase.Completed){ShowFuture();return;}
            if(annual.State.phase==TownPhase.Reviewing&&annual.Engine.DecisionsFinished)
            { chapters.StopRequest();StartCoroutine(FinishYears());return; }
            if(annual.State.phase==TownPhase.Reviewing)chapters.Continue();
            SaveAnnual();Refresh();
        }
        void ShowAnnualLedger()
        {
            hud.AnnualReport(annual.Engine,annualBaseline,ContinueAfterChapter,ShowAcceptedReport);
        }
    }
}
