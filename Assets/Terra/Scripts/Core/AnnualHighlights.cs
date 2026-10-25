using System;
using System.Collections.Generic;
using System.Linq;
using Terra.Contracts;

namespace Terra.Core
{
    public sealed class AnnualHighlight
    {
        public string siteId,icon,title,detail;
    }
    // Presentation of accepted facts only. Reading or focusing a highlight never advances time.
    public static class AnnualHighlights
    {
        public static AnnualHighlight[] Create(AnnualTownDefinition definition,TownYearReport current,TownYearReport previous)
        {
            var cards=new List<AnnualHighlight>();
            string Money(long cents)=>"$"+(cents/100m).ToString("N0",System.Globalization.CultureInfo.InvariantCulture);
            void Add(string icon,string title,string detail,string site=null)=>cards.Add(new AnnualHighlight{icon=icon,title=title,detail=detail,siteId=site});
            if(current.ledger.closingArrearsCents>0)Add("warning","Unpaid commitments",Money(current.ledger.closingArrearsCents)+" remains due. See Budget.");
            foreach(var site in current.outcomes.sites.OrderByDescending(s=>s.disrupted||s.phase==TownAssetPhase.Suspended))
            {
                var before=previous?.outcomes.sites.FirstOrDefault(s=>s.siteId==site.siteId&&s.actionId==site.actionId);
                var action=definition.actions.FirstOrDefault(a=>a.siteId==site.siteId&&a.actionId==site.actionId);
                if(action==null)continue;
                string place=definition.sites.First(s=>s.id==site.siteId).name;
                if(site.phase==TownAssetPhase.Suspended)Add("warning",action.title+" paused",place+" · payments not fully funded",site.siteId);
                else if(site.disrupted)Add("warning",action.title+" disrupted",place+" · review the year’s conditions",site.siteId);
                else if(before==null||before.phase!=site.phase)
                {
                    string phase=site.phase==TownAssetPhase.Building?"Building":site.phase==TownAssetPhase.Establishing?"Growing":site.phase==TownAssetPhase.Operating?"Operating":"Retired";
                    Add(site.phase==TownAssetPhase.Building?"work":"check",action.title,place+" · "+phase,site.siteId);
                }
                else if(site.maturity>before.maturity+.005)Add("leaf",action.title+" growing",place+" · "+(site.maturity*100).ToString("0")+"% established",site.siteId);
                if(cards.Count>=3)break;
            }
            if(cards.Count<3&&current.ledger.repairPaidCents>0)Add("work","Repairs funded",Money(current.ledger.repairPaidCents)+" public cost this year");
            if(cards.Count<3)Add("coin","Year-end balance",Money(current.ledger.closingCashCents)+" cash · "+Money(current.ledger.futureCommitmentsCents)+" committed");
            return cards.Take(3).ToArray();
        }
    }
}
