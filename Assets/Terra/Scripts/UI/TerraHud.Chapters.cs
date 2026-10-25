using System;
using System.Linq;
using Terra.Contracts;
using Terra.Core;

namespace Terra.UI
{
    public sealed partial class TerraHud
    {
        // TN-06/08, CS-01: short story pages; the full accepted ledger stays one click away.
        public void AnnualChapterReport(AnnualTown town,AnnualChapterReceipt receipt,Action continuePlanning,Action ledger,int page=0)
        {
            var chapter=receipt.chapter;int count=chapter.cards.Length;
            string nextYear=town.Phase==TownPhase.Reviewing?"Plan year "+(town.Year+1)+" →":"See ten-year future";
            if(page==0)
            {
                var report=town.History.Last(r=>r.year==receipt.consequences.year);
                AnnualDialog("Year "+report.year+" · "+(receipt.mode=="gemini"?"AI chapter":"offline chapter"),
                    "<size=92%><b>"+chapter.title+"</b>\n\n"+chapter.introduction+"\n\n<b>Cash  "+Money(report.ledger.closingCashCents)+"</b>\nFuture obligations  "+Money(report.ledger.futureCommitmentsCents)+"\n\nIllustrative game outcomes · not a forecast.</size>",
                    "Read chapter →",()=>AnnualChapterReport(town,receipt,continuePlanning,ledger,1));
            }
            else
            {
                var card=chapter.cards[page-1];
                AnnualDialog("Year "+receipt.consequences.year+" · "+page+" / "+count,
                    "<size=92%><b>"+card.title+"</b>\n\n"+card.explanation+"\n\n<size=80%>"+(receipt.mode=="gemini"?"AI-written from this year's accepted game results.":"Offline story from the authored game rules.")+"</size></size>",
                    page<count?"Next card →":nextYear,page<count?(Action)(()=>AnnualChapterReport(town,receipt,continuePlanning,ledger,page+1)):continuePlanning);
            }
            AnnualSecondary("Budget & meters",ledger);
        }
        public void AnnualChapterWaiting(int year,Action stop)
        {
            AnnualDialog("Closing year "+year+"…","Your plan is submitted. Writing this year's chapter…\n\nThe year and budget will update together when the chapter is ready.\n\nYou can stop waiting and retry the same plan. Your submitted choices are saved.","Stop waiting",stop);
        }
        public void AnnualChapterRecovery(int year,string error,Action retry,Action offline,Action edit)
        {
            AnnualDialog("Year "+year+" is still pending",(string.IsNullOrWhiteSpace(error)?"Your submitted plan is saved and ready to resume.":error)+"\n\n<b>No year has advanced and no payment was taken.</b>\n\nRetry the chapter, use the labelled offline story, or return to editing this year's plan.","Retry chapter",retry);
            AnnualSecondary("Use offline chapter",offline);
            closeButton.onClick.RemoveAllListeners();closeButton.onClick.AddListener(()=>edit());
            SetButtonText(closeButton,"Edit this year's plan");
        }
    }
}
