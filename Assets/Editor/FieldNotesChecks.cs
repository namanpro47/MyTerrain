using System;
using System.Linq;
using Terra.Contracts;
using Terra.Core;
using Terra.UI;
using TMPro;
using UnityEngine;

public static class FieldNotesChecks
{
    public static int Run(TerraHud hud,AnnualTownDefinition definition)
    {
        int count=0;
        void Check(bool ok,string message){if(!ok)throw new Exception("Field notes: "+message);count++;}
        string Visible()=>string.Join("\n",JsonUtility.FromJson<TerraHud.UiAudit>(hud.Audit()).labels.Select(l=>l.text));
        var town=new AnnualTown(definition,"field-notes-fixture");
        foreach(var a in definition.actions.Where(a=>a.actionId=="public-park").Take(4))town.QueueAction(a.siteId,a.actionId,town.Revision);
        string snapshot=JsonUtility.ToJson(town.Snapshot());
        var preview=AnnualGameplay.Forecast(town);
        hud.AnnualConfirmation(town,preview,()=>{});
        string budget=Visible();
        Check(!budget.Contains("All 4 proposals"),"Proposal list still opens another screen.");
        Check(budget.Contains("× 4"),"All four projects must be included in review.");
        Check(!definition.sites.Any(s=>budget.Contains(s.name)),"Unmapped lot names remain in review.");
        Check(budget.Contains("Pay now")&&budget.Contains("Tax receipts to collect")&&budget.Contains("Annual public funding"),"Review omits a financial category.");
        Check(budget.Contains(TerraHud.Money(preview.ledger.closingCashCents)),"Closing balance differs from authoritative ledger.");
        Check(snapshot==JsonUtility.ToJson(town.Snapshot()),"Review changed the plan or cash.");
        hud.AnnualState(town,AnnualGameplay.Baseline(definition),preview.outcomes);
        Check(Visible().Contains("End Year")&&!Visible().Contains("End Year ·"),"End Year has a count.");
        var request=town.BeginResolution("field-notes-year-1",town.Revision);
        var input=new AnnualChapterRequest{definition=definition,snapshot=town.Snapshot()};
        var receipt=AnnualChapters.Offline(input);
        town.CompleteResolution(receipt.consequences);
        string accepted=JsonUtility.ToJson(town.Snapshot()),savedReceipt=JsonUtility.ToJson(receipt);
        for(int page=0;page<=3;page++)
        {
            hud.AnnualChapterReport(town,receipt,()=>{},()=>{},page);
            string story=Visible();
            Check(story.Contains("local summary"),"Local narration source label missing.");
            Check(!story.Contains("vs last year")&&!story.Contains("Output multiplier"),"Raw score/technical text remains in narrative.");
            Check(story.Length>200,"Chapter does not contain a developed narrative.");
            var scroll=GameObject.Find("Field notes reading area").GetComponent<UnityEngine.UI.ScrollRect>();
            Check(scroll.viewport.rect.height>0&&scroll.content.rect.height>0,"Reading viewport/content has no size.");
            foreach(var label in scroll.content.GetComponentsInChildren<TextMeshProUGUI>())
            {
                label.ForceMeshUpdate();
                Check(label.preferredHeight<=label.rectTransform.rect.height+2,"Text overflows its content row: "+label.text);
            }
            Check(scroll.content.rect.height<=scroll.viewport.rect.height+1||Mathf.Abs(scroll.content.anchoredPosition.y)<1,"New chapter did not open at top: page "+page+" offset "+scroll.content.anchoredPosition.y);
            if(page==2)Check(scroll.content.Find("Economic progress")!=null&&scroll.content.Find("Social progress")!=null,"People card needs both meter bars.");
            if(page==3)Check(scroll.content.Find("Environmental progress")!=null,"Land card needs an environmental bar.");
        }
        Check(accepted==JsonUtility.ToJson(town.Snapshot())&&savedReceipt==JsonUtility.ToJson(receipt),"Reading mutated accepted history or saved receipt.");
        receipt.chapter.introduction="AI unavailable. "+receipt.chapter.introduction;
        hud.AnnualChapterReport(town,receipt,()=>{},()=>{},0);
        Check(Visible().Contains("AI unavailable."),"Hosted fallback disclosure lost in the readable narrative.");
        hud.ShowHelp();
        Check(!Visible().Contains("AI unavailable.")&&!GameObject.Find("Field notes reading area").GetComponentsInChildren<RectTransform>().Any(r=>r.name=="Economic progress"),"Chapter content leaked into help.");
        Check(GameObject.Find("Field notes reading area").activeInHierarchy,"Help must use the readable dialog surface.");
        bool explored=false;
        hud.AnnualWelcome("Scenario details","Fictional choices on real terrain.",()=>explored=true);
        Check(Visible().Contains("$10M")&&Visible().Contains("4 / 16")&&Visible().Contains("4 / 36"),"Opening resources differ from the definition.");
        Check(!Visible().Contains("Return to landscape"),"Opening repeats its exit action.");
        var modal=GameObject.Find("Field guide");
        Check(modal.GetComponent<UnityEngine.UI.Image>().color.r<.2f&&modal.GetComponent<UnityEngine.UI.Image>().color.a==1,"Dialog reverted to the bright document surface.");
        var primary=modal.GetComponentsInChildren<UnityEngine.UI.Button>().Single(b=>b.GetComponentInChildren<TextMeshProUGUI>().text=="Choose your run  →");
        primary.onClick.Invoke();Check(explored,"Opening primary action is disconnected.");
        hud.AnnualDialog("A long report",string.Join("\n\n",Enumerable.Repeat("A saved result that must remain readable.",35)),"Continue",()=>{});
        var continueButton=modal.GetComponentsInChildren<UnityEngine.UI.Button>().Single(b=>b.GetComponentInChildren<TextMeshProUGUI>().text=="Continue");
        Check(Mathf.Abs(continueButton.GetComponent<RectTransform>().anchoredPosition.y-24)<1,"Single primary action overlaps reading content.");
        var reading=GameObject.Find("Field notes reading area").GetComponent<UnityEngine.UI.ScrollRect>();
        Check(reading.content.rect.height>reading.viewport.rect.height,"Long generic dialog is not scrollable.");
        hud.ScrollFieldNotes(5);Check(reading.verticalNormalizedPosition<.99f,"Reading scroll does not move.");
        Check(accepted==JsonUtility.ToJson(town.Snapshot()),"Dialog navigation changed accepted history.");
        hud.CloseModal();
        return count;
    }
}
