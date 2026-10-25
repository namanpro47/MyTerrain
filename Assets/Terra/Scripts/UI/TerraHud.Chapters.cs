using System;
using System.Linq;
using Terra.Contracts;
using Terra.Core;
using UnityEngine;

namespace Terra.UI
{
    public sealed partial class TerraHud
    {
        // TN-06/08, CS-01: short story pages; the full accepted ledger stays one click away.
        public void AnnualChapterReport(AnnualTown town,AnnualChapterReceipt receipt,Action continuePlanning,Action ledger,int page=0)
        {
            var report=town.History.Last(r=>r.year==receipt.consequences.year);
            var chapter=receipt.mode=="offline"?AnnualChapterNarrative.Create(town.Definition,report):receipt.chapter;
            if(receipt.mode=="offline"&&receipt.chapter?.introduction?.StartsWith("AI unavailable.")==true)
                chapter.introduction="AI unavailable. "+chapter.introduction;
            int count=chapter.cards.Length;
            string nextYear=town.Phase==TownPhase.Completed?"See ten-year future":town.Phase==TownPhase.Reviewing?"Plan year "+(town.Year+1)+" →":"Return to plan";
            if(page==0)
            {
                FieldNotes(chapter.title,
                    chapter.introduction+"\n\n<b>Cash  "+Money(report.ledger.closingCashCents)+"</b>\nReserved for later years  "+Money(report.ledger.futureCommitmentsCents)+"\n\nIllustrative game outcomes · not a forecast.",
                    "Read chapter →",()=>AnnualChapterReport(town,receipt,continuePlanning,ledger,1));
            }
            else
            {
                var card=chapter.cards[page-1];
                FieldNotes(card.title,
                    card.explanation+"\n\n"+(receipt.mode=="gemini"?"AI-written from this year's accepted game results.":"Local summary from saved game results. No AI was used."),
                    page<count?"Next card →":nextYear,page<count?(Action)(()=>AnnualChapterReport(town,receipt,continuePlanning,ledger,page+1)):continuePlanning);
                var before=town.History.LastOrDefault(r=>r.year==report.year-1)?.outcomes??AnnualGameplay.Baseline(town.Definition);
                if(card.factIds.Contains("economic"))FieldNotesMeter("Economic",before.economicScore,report.outcomes.economicScore,new Color(.94f,.76f,.43f));
                if(card.factIds.Contains("social"))FieldNotesMeter("Social",before.socialScore,report.outcomes.socialScore,new Color(.50f,.76f,.93f));
                if(card.factIds.Contains("environmental"))FieldNotesMeter("Environmental",before.environmentalScore,report.outcomes.environmentalScore,Accent);
            }
            modalEyebrow.text="YEAR "+report.year+"  /  "+(receipt.mode=="gemini"?"AI chapter":"local summary")+(page==0?"":"  ·  "+page+" / "+count);
            AnnualSecondary("Budget & meters",ledger);
        }
        public void AnnualChapterWaiting(int year,Action stop)
        {
            AnnualDialog("Closing year "+year+"…","Your plan is submitted. Writing this year's chapter…\n\nIf AI is unavailable or its limit is reached, a labelled local summary will use the same saved game results.\n\nYou can stop waiting and retry the same plan. Your submitted choices are saved.","Stop waiting",stop);
        }
        public void AnnualChapterRecovery(int year,string error,Action retry,Action offline,Action edit)
        {
            AnnualDialog("Year "+year+" is still pending",(string.IsNullOrWhiteSpace(error)?"Your submitted plan is saved and ready to resume.":error)+"\n\n<b>No year has advanced and no payment was taken.</b>\n\nRetry the chapter, use the labelled offline story, or return to editing this year's plan.","Retry chapter",retry);
            AnnualSecondary("Use local summary",offline);
            closeButton.onClick.RemoveAllListeners();closeButton.onClick.AddListener(()=>edit());
            SetButtonText(closeButton,"Edit this year's plan");
        }
    }
}
