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
            chapters.endpoint=annualChapterEndpoint;chapters.saveKey=location.IsWisconsin?"terra.meadowbrook.annual-full.chapters.v3.2":location.ChapterSaveKey;
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
            if(chapters.Busy)hud.AnnualChapterWaiting(annual.State.year,()=>chapters.StopRequest());
            else if(annual.State.phase==TownPhase.Resolving)ShowPendingChapter();
            else if(annual.State.phase==TownPhase.Reviewing||annual.State.phase==TownPhase.Completed)ShowAcceptedReport();
        }
        void ShowPendingChapter()
        {
            if(chapters.Busy){hud.AnnualChapterWaiting(annual.State.year,()=>chapters.StopRequest());return;}
            hud.AnnualChapterRecovery(annual.State.year,chapters.Error,()=>chapters.ResolveOnline(),()=>chapters.ResolveOffline(),()=>{
                hud.CloseModal();chapters.CancelResolution();
            });
        }
        void ContinueAfterChapter()
        {
            hud.CloseModal();
            if(annual.State.phase==TownPhase.Completed){ShowFuture();return;}
            if(annual.State.phase==TownPhase.Reviewing)chapters.Continue();
            SaveAnnual();Refresh();
        }
        void ShowAnnualLedger()
        {
            bool hasChapter=chapters.ForYear(annual.State.history[annual.State.history.Length-1].year)!=null;
            hud.AnnualReport(annual.Engine,annualBaseline,ContinueAfterChapter,hasChapter?(Action)ShowAcceptedReport:null);
        }
    }
}
