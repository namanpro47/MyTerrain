using System;
using System.Linq;
using System.Reflection;
using Terra.Contracts;
using Terra.Core;
using Terra.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class AnnualFeedbackWaveChecks
{
    const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
    public static int Run(TerraHud hud,AnnualTownDefinition definition,WorldData world)
    {
        int checks=0;
        void Check(bool value,string message){checks++;if(!value)throw new Exception("Annual feedback: "+message);}
        T Field<T>(string name)=>(T)typeof(TerraHud).GetField(name,Private).GetValue(hud);
        void Invoke(string name,params object[] args)=>typeof(TerraHud).GetMethod(name,Private).Invoke(hud,args);
        string Visible()=>string.Join("\n",JsonUtility.FromJson<TerraHud.UiAudit>(hud.Audit()).labels.Select(l=>l.text));
        void Click(string caption)=>Field<Transform>("root").GetComponentsInChildren<Button>().Single(b=>b.GetComponentInChildren<TextMeshProUGUI>()?.text==caption).onClick.Invoke();
        var baseline=AnnualGameplay.Baseline(definition);
        var town=new AnnualTown(definition,"feedback-ui");
        hud.Ready();hud.AnnualState(town,baseline,baseline);
        var cards=Field<System.Collections.Generic.Dictionary<Button,string>>("actionButtons");
        Check(!cards.Values.Contains("cultivate-margin"),"Living Margin is still a selectable tool.");
        int chosen=0;
        foreach(int years in new[]{1,3,5,10})
        {
            hud.AnnualRunSetup(10,n=>chosen=n);
            Field<Transform>("root").GetComponentsInChildren<Button>().Single(b=>b.name=="Choose "+years+" decision years").onClick.Invoke();
            Check(Visible().Contains(years+(years==1?" year":" years")+" · selected"),"Duration selection has no persistent label.");
            Check(Field<System.Collections.Generic.List<Button>>("buttons").All(b=>b!=null),"Rebuilt duration leaves destroyed navigation buttons.");
            foreach(var label in GameObject.Find("Decision years").GetComponentsInChildren<TextMeshProUGUI>())
            {label.ForceMeshUpdate();Check(label.preferredHeight<=label.rectTransform.rect.height+2,"Duration label clips.");}
            Check(chosen==0,"Selecting a duration started play before Begin.");
            Click("Begin · "+years+(years==1?" decision year":" decision years"));
            Check(chosen==years,"Begin did not retain selected decision years.");chosen=0;
        }
        hud.ShowHelp();Check(Field<System.Collections.Generic.List<Button>>("buttons").All(b=>b!=null),"Leaving setup leaves destroyed navigation buttons.");
        hud.CloseModal();
        foreach(bool large in new[]{false,true})
        {
            if(large)Invoke("ToggleText");
            foreach(var offer in definition.actions.Where(a=>a.actionId!="cultivate-margin"&&a.actionId!=AnnualPlayerRules.CancelAction).GroupBy(a=>a.actionId).Select(g=>g.First()))
            {
                hud.HideAnnualPreview();hud.HidePlacementFeedback();hud.Selected(offer.actionId);Invoke("Focus",new object[]{null});Invoke("UpdateToolHint");
                Check((bool)typeof(TerraHud).GetMethod("ShowingToolHint",Private).Invoke(hud,null),"Selected tool has no details before placement: "+offer.actionId);
                Check(Field<TextMeshProUGUI>("toolHintReservation").text.Contains("upfront"),"Selected tool cost missing.");
                foreach(var label in Field<Transform>("toolHint").GetComponentsInChildren<TextMeshProUGUI>())
                {label.ForceMeshUpdate();Check(label.preferredHeight<=label.rectTransform.rect.height+2,"Selected tool detail clips: "+label.text);}
            }
        }
        Invoke("ToggleText");hud.Selected("");
        var farm=definition.actions.First(a=>a.actionId=="cultivate-standard"&&definition.actions.Any(b=>b.siteId==a.siteId&&b.actionId=="restore-grass"));
        town.ConfigurePlanningYears(3,town.Revision);town.QueueAction(farm.siteId,farm.actionId,town.Revision);
        AnnualTownDemo.Advance(town,"feedback-year1",new AuthoredAnnualConsequenceProvider());town.ContinuePlanning(town.Revision);
        var meadow=definition.actions.First(a=>a.siteId==farm.siteId&&a.actionId=="restore-grass");
        hud.AnnualState(town,baseline,AnnualGameplay.Forecast(town).outcomes);
        hud.AnnualPreview(world.sites.First(s=>s.id==farm.siteId),meadow,town.Snapshot(),"place",null,()=>{},false);
        Check(Visible().Contains("Review change · Enter")&&Visible().Contains("no refund"),"Accepted field does not explain conversion.");
        Check(Visible().Contains(TerraHud.Money(meadow.upfrontPublicCostCents+town.ConversionCost(farm.siteId))),"Conversion preview omits its extra cost.");
        town.QueueAction(farm.siteId,meadow.actionId,town.Revision);
        hud.AnnualConfirmation(town,AnnualGameplay.Forecast(town),()=>{});
        Check(Visible().Contains("Land-use changes")&&Visible().Contains("Future obligations"),"Budget hides conversion or future costs.");hud.CloseModal();
        AnnualTownDemo.Advance(town,"feedback-year2",new AuthoredAnnualConsequenceProvider());town.ContinuePlanning(town.Revision);
        AnnualTownDemo.Advance(town,"feedback-year3",new AuthoredAnnualConsequenceProvider());
        hud.AnnualOverview(town,()=>{},()=>{},()=>{},_=>{});
        Check(Visible().Contains("Review Year 10 →")&&!Visible().Contains("Plan year 4"),"Decision limit still offers a fourth plan.");hud.CloseAnnualOverview();
        AnnualTownDemo.Finish(town,new AuthoredAnnualConsequenceProvider());
        var final=town.History.Last();
        var receipt=new AnnualChapterReceipt{mode="offline",consequences=final.acceptedConsequences,chapter=AnnualChapterNarrative.Create(definition,final)};
        string saved=JsonUtility.ToJson(town.Snapshot());
        hud.AnnualChapterReport(town,receipt,()=>{},()=>{});
        Check(Visible().Contains("The decade you shaped")&&Visible().Contains("changed direction"),"Final notes omit the whole-decade conversion story.");
        Check(saved==JsonUtility.ToJson(town.Snapshot()),"Reading the decade changed saved results.");hud.CloseModal();
        var fullDefinition=JsonUtility.FromJson<AnnualTownDefinition>(JsonUtility.ToJson(definition));fullDefinition.startingCashCents=10000000000;
        var full=new AnnualTown(fullDefinition,"full-landscape");
        bool future=false;hud.ConfigureFuture(()=>future=true);
        foreach(var site in fullDefinition.sites.Where(s=>!s.protectedSite))
        {
            var cheapest=fullDefinition.actions.Where(a=>a.siteId==site.id&&a.actionId!="cultivate-margin"&&a.actionId!=AnnualPlayerRules.CancelAction).OrderBy(a=>a.upfrontPublicCostCents).FirstOrDefault();
            if(cheapest!=null)full.QueueAction(site.id,cheapest.actionId,full.Revision);
        }
        hud.FullLandscapeOption(full);Check(Field<Button>("fullLandscapeFuture").gameObject.activeSelf,"Full landscape has no Year 10 option.");
        Field<Button>("fullLandscapeFuture").onClick.Invoke();Check(future,"Full landscape option has no action.");
        full.UndoPlan(full.Revision);hud.FullLandscapeOption(full);Check(!Field<Button>("fullLandscapeFuture").gameObject.activeSelf,"Undo leaves false full-landscape prompt.");
        hud.HideAnnualPreview();hud.AnnualState(new AnnualTown(definition,"feedback-reset"),baseline,baseline);
        Debug.Log("ANNUAL_FEEDBACK_WAVE_CHECKS_PASSED: "+checks);return checks;
    }
}
