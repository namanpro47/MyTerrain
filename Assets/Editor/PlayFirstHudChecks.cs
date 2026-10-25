using System;
using System.Linq;
using System.Reflection;
using Terra.Contracts;
using Terra.Core;
using Terra.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class PlayFirstHudChecks
{
    public static int Run(TerraHud hud,AnnualTownDefinition definition,WorldData world)
    {
        int checks=0;
        void Check(bool valid,string message){checks++;if(!valid)throw new Exception("Play-first HUD: "+message);}
        var town=new AnnualTown(definition,"play-first-ui-checks");var baseline=AnnualGameplay.Baseline(definition);
        var flags=BindingFlags.Instance|BindingFlags.NonPublic;
        string Text()=>string.Join("\n",UnityEngine.Object.FindObjectsByType<TextMeshProUGUI>().Where(t=>t.gameObject.activeInHierarchy).Select(t=>t.text));
        void Click(string caption)=>UnityEngine.Object.FindObjectsByType<Button>().Single(b=>b.gameObject.activeInHierarchy&&b.GetComponentInChildren<TextMeshProUGUI>()?.text==caption).onClick.Invoke();
        hud.Ready();hud.AnnualState(town,baseline,AnnualGameplay.Forecast(town).outcomes);
        Click("More");
        Check(((Button)typeof(TerraHud).GetField("focus",flags).GetValue(hud)).IsInteractable(),"More focused a disabled action");Click("More");
        foreach(bool large in new[]{false,true})
        {
            if(large)typeof(TerraHud).GetMethod("ToggleText",flags).Invoke(hud,null);
            foreach(bool compact in new[]{false,true})
            foreach(var offer in definition.actions.Where(a=>a.actionId!=AnnualPlayerRules.CancelAction).GroupBy(a=>a.actionId).Select(g=>g.First()))
            {
                hud.AnnualPreview(world.sites.First(s=>s.id==offer.siteId),offer,town.Snapshot(),"place",null,()=>{},false);
                typeof(TerraHud).GetMethod("LayoutAnnual",flags).Invoke(hud,new object[]{compact?1024f:1440f,compact?576f:900f,compact});
                var text=Text();
                Check(text.Contains((string)typeof(TerraHud).GetMethod("StatMoney",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{offer.upfrontPublicCostCents})),"Missing upfront cost for "+offer.actionId);
                Check(text.Contains("Opens Y"+(town.Year+offer.constructionYears)),"Opening year hidden for "+offer.actionId);
                Check(text.Contains(TerraHud.Money(offer.upfrontPublicCostCents+offer.annualPublicOperatingCents*offer.operatingYears+offer.annualPublicPaymentCents*offer.paymentYears)+" lifetime public cost"),"Lifetime cost hidden for "+offer.actionId);
                Check(text.Contains(offer.operatingYears+" yr"),"Upkeep term hidden");
                Canvas.ForceUpdateCanvases();
                foreach(var label in GameObject.Find("Project at a glance").GetComponentsInChildren<TextMeshProUGUI>())
                {label.ForceMeshUpdate();Check(label.preferredHeight<=label.rectTransform.rect.height+2,"Clipped fact for "+offer.actionId+": "+label.text);}
                var visibleCards=UnityEngine.Object.FindObjectsByType<Button>().Where(b=>b.gameObject.activeInHierarchy&&b.name.StartsWith("Build ")).ToArray();
                Check(visibleCards.Length<=(compact?3:4),"Compact catalog did not paginate");
                if(offer.actionId=="public-park")foreach(var label in GameObject.Find("Annual indicators").GetComponentsInChildren<TextMeshProUGUI>())
                {label.ForceMeshUpdate();Check(label.preferredHeight<=label.rectTransform.rect.height+2,"Clipped meter: "+label.text);}
            }
        }
        typeof(TerraHud).GetMethod("ToggleText",flags).Invoke(hud,null);
        Check(town.History.Length==0&&town.CashCents==definition.startingCashCents,"Inspecting projects changed the ledger");
        var park=definition.actions.First(a=>a.actionId=="public-park");town.QueueAction(park.siteId,park.actionId,town.Revision);
        var queued=JsonUtility.ToJson(town.Snapshot());var alternative=definition.actions.First(a=>a.siteId==park.siteId&&a.actionId!=park.actionId);
        hud.AnnualPreview(world.sites.First(s=>s.id==park.siteId),alternative,town.Snapshot(),"place",null,()=>{},true);
        Check(Text().Contains("REPLACE Y1")&&Text().Contains("Replace · Enter"),"Replacement preview was labelled as queued");
        hud.AnnualPreview(world.sites.First(s=>s.id==park.siteId),alternative,town.Snapshot(),"place","This replacement does not fit.",()=>{},true);
        hud.PlacementFeedback("BLOCKED",alternative.title,world.sites.First(s=>s.id==park.siteId).name,
            "Does not fit · choose another field","", "This replacement does not fit.",null,null,Array.Empty<Vector3>(),null);
        Check(Text().Contains(alternative.title)&&Text().Contains("This replacement does not fit."),"Blocked replacement hid the intended project or its rejection");
        Check(Text().Contains("Esc keeps your current plan"),"Blocked replacement did not explain recovery");
        hud.HidePlacementFeedback();
        Check(queued==JsonUtility.ToJson(town.Snapshot()),"Inspecting a replacement mutated the queue");
        var first=AnnualTownDemo.Advance(town,"play-first-year1",new AuthoredAnnualConsequenceProvider());
        var cards=AnnualHighlights.Create(definition,first,null);
        Check(cards.Length<=3&&cards.Any(c=>c.siteId==park.siteId&&c.detail.Contains("Building")),"New construction highlight missing");
        var saved=JsonUtility.ToJson(town.Snapshot());string focused=null;bool continued=false;
        hud.AnnualOverview(town,()=>continued=true,()=>{},()=>{},id=>focused=id);
        Check(hud.AnnualOverviewOpen&&!hud.ModalOpen,"Year overview obscures the landscape with a modal");
        UnityEngine.Object.FindObjectsByType<Button>().Single(b=>b.name=="Year highlight 1"&&b.gameObject.activeInHierarchy).onClick.Invoke();
        Check(focused==park.siteId&&saved==JsonUtility.ToJson(town.Snapshot()),"Highlight did not focus the accepted site without changing state");
        Click("Plan year 2 →");Check(continued,"Continue requires reading the chapter");
        hud.CloseAnnualOverview();
        town.ContinuePlanning(town.Revision);
        var second=AnnualTownDemo.Advance(town,"play-first-year2",new AuthoredAnnualConsequenceProvider());
        Check(AnnualHighlights.Create(definition,second,first).Any(c=>c.siteId==park.siteId&&c.detail.Contains("Operating")),"Opening project highlight missing");
        var empty=new AnnualTown(definition,"quiet-year");var quiet=AnnualTownDemo.Advance(empty,"quiet",new AuthoredAnnualConsequenceProvider());
        Check(AnnualHighlights.Create(definition,quiet,null).All(c=>c.siteId==null),"Quiet year fabricated a project");
        var problem=JsonUtility.FromJson<TownYearReport>(JsonUtility.ToJson(second));problem.ledger.closingArrearsCents=100;
        Check(AnnualHighlights.Create(definition,problem,first)[0].title=="Unpaid commitments","Unpaid commitments did not take priority");
        var program=JsonUtility.FromJson<TownActionDefinition>(JsonUtility.ToJson(park));program.annualPublicPaymentCents=30000;program.paymentYears=5;
        var fresh=new AnnualTown(definition,"program-inspection");
        hud.AnnualPreview(world.sites.First(s=>s.id==program.siteId),program,fresh.Snapshot(),"place",null,()=>{},false);
        Check(Text().Contains("$300/yr program · 5 yr"),"Program payment was folded into upkeep");
        Check(Text().Contains(TerraHud.Money(program.upfrontPublicCostCents+program.annualPublicOperatingCents*program.operatingYears+150000)+" lifetime public cost"),"Program term missing from reservation");
        AnnualTownDemo.Finish(empty,new AuthoredAnnualConsequenceProvider());bool future=false;
        hud.AnnualOverview(empty,()=>future=true,null,()=>{},_=>{});Click("See ten-year future →");
        Check(future&&empty.Year==10,"Completed overview did not hand off without Year 11");hud.CloseAnnualOverview();
        hud.HideAnnualPreview();hud.AnnualState(new AnnualTown(definition,"reset-ui"),baseline,baseline);
        Debug.Log("PLAY_FIRST_HUD_CHECKS_PASSED: "+checks);return checks;
    }
}
