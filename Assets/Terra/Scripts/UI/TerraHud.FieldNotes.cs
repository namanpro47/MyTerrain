using System;
using System.Collections.Generic;
using System.Linq;
using Terra.Contracts;
using TMPro;
using UnityEngine;

namespace Terra.UI
{
    public sealed partial class TerraHud
    {
        // TN-06/08, CS-01, AX-01: readable prose, visual change, one budget surface.
        UnityEngine.UI.ScrollRect fieldNotesScroll;
        RectTransform fieldNotesContent;
        Transform fieldNotesControls;
        TextMeshProUGUI fieldNotesBody;
        readonly List<GameObject> fieldNotesRows = new List<GameObject>();
        bool fieldNotesOpen,fieldNotesLayoutDirty;
        Vector2 notesViewport;
        bool notesLarge,notesSecondary;
        string notesCloseCaption;


        void ResetFieldNotes()
        {
            fieldNotesOpen = false;
            if (fieldNotesScroll != null) fieldNotesScroll.gameObject.SetActive(false);
            if (fieldNotesControls != null) fieldNotesControls.gameObject.SetActive(false);
        }

        void FieldNotes(string title, string body, string action, Action callback)
        {
            AnnualDialog(title,body,action,callback);
        }

        void PrepareFieldNotes(string body)
        {
            if (fieldNotesScroll == null)
            {
                var scroll = Rect(modal.transform,"Field notes reading area",new Vector2(0,1),new Vector2(32,-110),new Vector2(736,380));
                fieldNotesScroll = scroll.gameObject.AddComponent<UnityEngine.UI.ScrollRect>();
                var viewport = Rect(scroll,"Viewport",Vector2.zero,Vector2.zero,Vector2.zero);
                viewport.anchorMax = Vector2.one;viewport.offsetMin=viewport.offsetMax=Vector2.zero;
                viewport.gameObject.AddComponent<UnityEngine.UI.Image>().color=Paper;
                viewport.gameObject.AddComponent<UnityEngine.UI.Mask>().showMaskGraphic=false;
                fieldNotesContent=Rect(viewport,"Content",new Vector2(0,1),Vector2.zero,new Vector2(736,0));
                var layout=fieldNotesContent.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
                layout.spacing=18;layout.childControlWidth=true;layout.childControlHeight=true;layout.childForceExpandWidth=true;layout.childForceExpandHeight=false;
                var fitter=fieldNotesContent.gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>();
                fitter.verticalFit=UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
                fieldNotesBody=Label(fieldNotesContent,"",new Vector2(0,1),Vector2.zero,new Vector2(736,0),18,Paper);
                fieldNotesBody.paragraphSpacing=8;fieldNotesBody.lineSpacing=3;
                fieldNotesScroll.viewport=viewport;fieldNotesScroll.content=fieldNotesContent;
                fieldNotesScroll.horizontal=false;fieldNotesScroll.vertical=true;fieldNotesScroll.inertia=false;
                fieldNotesScroll.movementType=UnityEngine.UI.ScrollRect.MovementType.Clamped;
                fieldNotesControls=Rect(modal.transform,"Reading controls",new Vector2(1,0),new Vector2(-32,150),new Vector2(220,30));
                Button(fieldNotesControls,"↑",Vector2.zero,Vector2.zero,new Vector2(40,30),()=>ScrollFieldNotes(-3),18);
                Button(fieldNotesControls,"↓",Vector2.zero,new Vector2(48,0),new Vector2(40,30),()=>ScrollFieldNotes(3),18);
                Label(fieldNotesControls,"Scroll to read",Vector2.zero,new Vector2(100,3),new Vector2(120,24),12,Soft);
            }
            foreach(var row in fieldNotesRows)
            {
                foreach(var label in row.GetComponentsInChildren<TextMeshProUGUI>(true))sizes.Remove(label);
                row.SetActive(false);
                if(Application.isPlaying)UnityEngine.Object.Destroy(row);else UnityEngine.Object.DestroyImmediate(row);
            }
            fieldNotesRows.Clear();fieldNotesLayoutDirty=true;fieldNotesOpen=true;fieldNotesScroll.gameObject.SetActive(true);
            modalText.fontSize=30*(LargeText?1.12f:1);
            fieldNotesBody.text=System.Text.RegularExpressions.Regex.Replace(body,@"</?size[^>]*>","");fieldNotesBody.fontSize=18*(LargeText?1.12f:1);
            LayoutFieldNotes();fieldNotesContent.anchoredPosition=Vector2.zero;fieldNotesScroll.verticalNormalizedPosition=1;
        }

        void LayoutFieldNotes()
        {
            if(!fieldNotesOpen)return;
            var viewportSize=new Vector2(Screen.width/PixelRatio,Screen.height/PixelRatio);
            bool secondaryVisible=annualDetails!=null&&annualDetails.gameObject.activeSelf;
            string closeCaption=closeButton.GetComponentInChildren<TextMeshProUGUI>().text;
            if(!fieldNotesLayoutDirty&&notesViewport==viewportSize&&notesLarge==LargeText&&notesSecondary==secondaryVisible&&notesCloseCaption==closeCaption)return;
            fieldNotesLayoutDirty=false;notesViewport=viewportSize;notesLarge=LargeText;notesSecondary=secondaryVisible;notesCloseCaption=closeCaption;
            float width=Mathf.Min(720,viewportSize.x-32);
            float textScale=LargeText?1.12f:1;
            modalText.fontSize=30*textScale;
            modalText.rectTransform.anchoredPosition=new Vector2(32,-66);
            modalText.rectTransform.sizeDelta=new Vector2(width-64,84);
            modalText.ForceMeshUpdate();
            float heading=Mathf.Max(44,modalText.preferredHeight);
            modalText.rectTransform.sizeDelta=new Vector2(width-64,heading);
            float top=82+heading;
            fieldNotesContent.sizeDelta=new Vector2(width-64,fieldNotesContent.sizeDelta.y);
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(fieldNotesContent);
            float height=Mathf.Min(Screen.height/PixelRatio-32,Mathf.Min(680,Mathf.Max(340,top+fieldNotesContent.rect.height+112)));
            modal.transform.localScale=Vector3.one;((RectTransform)modal.transform).sizeDelta=new Vector2(width,height);
            var area=fieldNotesScroll.GetComponent<RectTransform>();
            area.anchoredPosition=new Vector2(32,-top);area.sizeDelta=new Vector2(width-64,Mathf.Max(80,height-top-104));
            void Place(UnityEngine.UI.Button button,float x,float w)
            {
                var r=button.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,0);
                r.anchoredPosition=new Vector2(x,24);r.sizeDelta=new Vector2(w,48);
                var text=button.GetComponentInChildren<TextMeshProUGUI>();text.rectTransform.sizeDelta=new Vector2(w-20,44);
                text.fontSize=16*textScale;
            }
            var close=closeButton.GetComponent<RectTransform>();close.anchorMin=close.anchorMax=close.pivot=new Vector2(1,1);
            close.anchoredPosition=new Vector2(-24,-20);close.sizeDelta=new Vector2(closeButton.GetComponentInChildren<TextMeshProUGUI>().text=="Close · Esc"?108:206,32);
            closeButton.GetComponentInChildren<TextMeshProUGUI>().rectTransform.sizeDelta=close.sizeDelta-new Vector2(8,0);
            closeButton.GetComponentInChildren<TextMeshProUGUI>().fontSize=13*textScale;
            if(annualPrimary!=null)
            {
                bool secondary=annualDetails.gameObject.activeSelf;
                float w=secondary?(width-76)/2:Mathf.Min(340,width-64);
                Place(annualPrimary,width-32-w,w);Place(annualDetails,32,(width-76)/2);
            }
            Place(dataPageButton,width-32-Mathf.Min(340,width-64),Mathf.Min(340,width-64));
            float third=(width-88)/3;
            Place(motionButton,32,third);Place(muteButton,44+third,third);Place(textButton,56+2*third,third);
            ((RectTransform)fieldNotesControls).anchoredPosition=new Vector2(-32,76);
            bool overflow=fieldNotesContent.rect.height>area.rect.height+1;
            fieldNotesControls.gameObject.SetActive(overflow);
        }

        public bool ScrollFieldNotes(float amount)
        {
            if(!ModalOpen||!fieldNotesOpen)return false;
            float overflow=fieldNotesContent.rect.height-fieldNotesScroll.viewport.rect.height;
            if(overflow>0)fieldNotesScroll.verticalNormalizedPosition=Mathf.Clamp01(fieldNotesScroll.verticalNormalizedPosition-amount*48/overflow);
            return true;
        }

        void FieldNotesMeter(string label,double before,double after,Color color)
        {
            var row=Rect(fieldNotesContent,label+" progress",new Vector2(0,1),Vector2.zero,new Vector2(736,96));
            row.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().preferredHeight=96;fieldNotesRows.Add(row.gameObject);fieldNotesLayoutDirty=true;
            Label(row,label+"  / 100",new Vector2(0,1),Vector2.zero,new Vector2(640,24),18,Paper,FontStyles.Bold);
            string[] names={"Before","Now"};double[] values={before,after};
            for(int i=0;i<2;i++)
            {
                Label(row,names[i],new Vector2(0,1),new Vector2(0,-30-i*30),new Vector2(70,24),16,Soft);
                var track=Rect(row,names[i]+" track",new Vector2(0,1),new Vector2(82,-34-i*30),new Vector2(0,16));
                track.anchorMax=new Vector2(1,1);track.offsetMax=new Vector2(-64,track.offsetMax.y);
                track.gameObject.AddComponent<UnityEngine.UI.Image>().color=Raised;
                var fill=Rect(track,names[i]+" score bar",Vector2.zero,Vector2.zero,Vector2.zero);
                fill.anchorMax=new Vector2(Mathf.Clamp01((float)values[i]/100),1);fill.offsetMin=fill.offsetMax=Vector2.zero;
                fill.gameObject.AddComponent<UnityEngine.UI.Image>().color=i==0?new Color(.42f,.49f,.47f):color;
                Label(row,values[i].ToString("0.0"),new Vector2(1,1),new Vector2(0,-30-i*30),new Vector2(54,24),16,Soft);
            }
            foreach(var text in row.GetComponentsInChildren<TextMeshProUGUI>())text.fontSize=sizes[text]*(LargeText?1.12f:1);
            LayoutFieldNotes();fieldNotesContent.anchoredPosition=Vector2.zero;fieldNotesScroll.verticalNormalizedPosition=1;
        }

        public void AnnualWelcome(string scenarioTitle,string scenarioDetails,Action explore)
        {
            AnnualDialog("What will you leave behind?",
                "<b>"+annualRules.years+" years. One landscape. Your choices.</b>\n"+
                "Make room for homes and livelihoods while caring for the land and water.\n\n"+
                "<color=#87D9B0><b>Choose a project  →  Find a place  →  End Year</b></color>\n"+
                "Change your plan freely before ending the year. Once accepted, projects take time and upkeep carries forward.",
                "Explore the landscape  →",explore);
            modalEyebrow.text=location.Name.ToUpperInvariant()+"  /  YEAR 1";
            var row=Rect(fieldNotesContent,"Starting resources",new Vector2(0,1),Vector2.zero,new Vector2(656,110));
            row.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().preferredHeight=110;
            var layout=row.gameObject.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            layout.spacing=12;layout.childControlWidth=true;layout.childControlHeight=true;layout.childForceExpandWidth=true;layout.childForceExpandHeight=true;
            fieldNotesRows.Add(row.gameObject);row.SetAsFirstSibling();
            var e=annualRules.economy;
            string[] values={annualRules.startingCashCents%100000000==0?"$"+(annualRules.startingCashCents/100000000)+"M":Money(annualRules.startingCashCents),e.baselineHomes+" / "+e.householdDemand,e.baselineJobs+" / "+e.jobsTarget};
            string[] captions={"PUBLIC FUNDS","HOMES NOW / NEEDED","JOBS NOW / TARGET"},symbols={"coin","home","work"};
            for(int i=0;i<3;i++)
            {
                var card=Panel(row,captions[i],Vector2.zero,Vector2.zero,new Vector2(200,110),Raised);
                card.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().flexibleWidth=1;
                var icon=Rect(card,"Resource icon",new Vector2(0,1),new Vector2(16,-16),new Vector2(22,22)).gameObject.AddComponent<HudGlyph>();
                icon.symbol=symbols[i];icon.color=Accent;icon.raycastTarget=false;
                Label(card,values[i],new Vector2(0,1),new Vector2(16,-44),new Vector2(180,32),26,Paper,FontStyles.Bold);
                Label(card,captions[i],new Vector2(0,0),new Vector2(16,12),new Vector2(184,20),12,Soft,FontStyles.Bold);
            }
            fieldNotesLayoutDirty=true;
            AnnualSecondary("Your brief & goals",()=>
            {
                AnnualDialog(scenarioTitle,"<b>Your year "+annualRules.years+" goals</b>\nEconomic "+e.finalEconomicTarget+" · Social "+e.finalSocialTarget+" · Environmental "+e.finalEnvironmentalTarget+"\nEvery run gets an ending.\n\n"+scenarioDetails,"Explore the landscape  →",explore);
                AnnualSecondary("Back to introduction",()=>AnnualWelcome(scenarioTitle,scenarioDetails,explore));
            });
            LayoutFieldNotes();fieldNotesContent.anchoredPosition=Vector2.zero;fieldNotesScroll.verticalNormalizedPosition=1;
        }

        static string NotesMoney(long cents)=>"$"+(cents/100m).ToString(cents%100==0?"N0":"N2",System.Globalization.CultureInfo.InvariantCulture);

        void FieldNotesBudget(AnnualTownSnapshot state,TownYearLedger ledger,string mode,Action accept)
        {
            var rows=new List<string>();
            foreach(var group in state.queue.Select(q=>annualRules.actions.First(a=>a.siteId==q.siteId&&a.actionId==q.actionId)).GroupBy(a=>a.title))
                rows.Add(group.Key+(group.Count()>1?" × "+group.Count():"")+"   "+NotesMoney(group.Sum(a=>a.upfrontPublicCostCents)));
            if(rows.Count==0)rows.Add("No new projects this year.");
            void Cost(string label,long amount){if(amount!=0)rows.Add(label+"   "+NotesMoney(amount));}
            Cost("Running costs",ledger.operatingPaidCents);Cost("Program payments",ledger.programPaidCents);
            Cost("Repairs",ledger.repairPaidCents);Cost("Earlier unpaid bills",ledger.arrearsPaidCents);
            long total=ledger.upfrontPaidCents+ledger.operatingPaidCents+ledger.programPaidCents+ledger.repairPaidCents+ledger.arrearsPaidCents;
            string body="<b>Your plan</b>\n"+string.Join("\n",rows)+"\n\n<b>Pay now   "+NotesMoney(total)+"</b>"+
                "\nTax receipts to collect   +"+NotesMoney(ledger.publicReceiptsCents)+
                "\nAnnual public funding   +"+NotesMoney(ledger.appropriationCents)+
                "\n\n"+NotesMoney(ledger.openingCashCents)+" − "+NotesMoney(total)+" + "+NotesMoney(ledger.publicReceiptsCents)+" + "+NotesMoney(ledger.appropriationCents)+
                "\n<b>Balance after this year   "+NotesMoney(ledger.closingCashCents)+"</b>"+
                "\n\nReserved for later years   "+NotesMoney(ledger.futureCommitmentsCents)+
                (ledger.closingArrearsCents>0?"\nUnpaid bills carried forward   "+NotesMoney(ledger.closingArrearsCents):"")+
                "\n\n"+mode+" · game estimates. Ending the year commits this plan.";
            FieldNotes("Review year "+state.year,body,"End Year",accept);
        }
    }
}
