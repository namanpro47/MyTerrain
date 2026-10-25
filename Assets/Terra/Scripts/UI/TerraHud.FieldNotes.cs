using Terra.Core;
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
        bool fieldNotesOpen;

        void ResetFieldNotes()
        {
            fieldNotesOpen = false;
            if (fieldNotesScroll != null) fieldNotesScroll.gameObject.SetActive(false);
            if (fieldNotesControls != null) fieldNotesControls.gameObject.SetActive(false);
            modalText.rectTransform.anchoredPosition = new Vector2(32,-61);
            modalText.rectTransform.sizeDelta = new Vector2(736,440);
            modalText.fontSize=sizes[modalText]*(LargeText?1.12f:1);
            ((RectTransform)modal.transform).sizeDelta = new Vector2(800,650);
            float width = Screen.width / PixelRatio, height = Screen.height / PixelRatio;
            modal.transform.localScale = Vector3.one * Mathf.Min(1,(height-32)/650,(width-32)/800);
        }

        void FieldNotes(string title, string body, string action, Action callback)
        {
            AnnualDialog(title,"",action,callback);
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
                fieldNotesBody=Label(fieldNotesContent,"",new Vector2(0,1),Vector2.zero,new Vector2(736,0),20,Ink);
                fieldNotesBody.paragraphSpacing=8;fieldNotesBody.lineSpacing=3;
                fieldNotesScroll.viewport=viewport;fieldNotesScroll.content=fieldNotesContent;
                fieldNotesScroll.horizontal=false;fieldNotesScroll.vertical=true;fieldNotesScroll.inertia=false;
                fieldNotesScroll.movementType=UnityEngine.UI.ScrollRect.MovementType.Clamped;
                fieldNotesControls=Rect(modal.transform,"Reading controls",new Vector2(1,0),new Vector2(-32,150),new Vector2(220,30));
                Button(fieldNotesControls,"↑",Vector2.zero,Vector2.zero,new Vector2(40,30),()=>ScrollFieldNotes(-3),18);
                Button(fieldNotesControls,"↓",Vector2.zero,new Vector2(48,0),new Vector2(40,30),()=>ScrollFieldNotes(3),18);
                Label(fieldNotesControls,"Scroll to read",Vector2.zero,new Vector2(100,3),new Vector2(120,24),14,Ink);
            }
            foreach(var row in fieldNotesRows)
            {
                foreach(var label in row.GetComponentsInChildren<TextMeshProUGUI>(true))sizes.Remove(label);
                row.SetActive(false);
                if(Application.isPlaying)UnityEngine.Object.Destroy(row);else UnityEngine.Object.DestroyImmediate(row);
            }
            fieldNotesRows.Clear();fieldNotesOpen=true;fieldNotesScroll.gameObject.SetActive(true);
            modalText.text="<b>"+title+"</b>";modalText.fontSize=26*(LargeText?1.12f:1);
            fieldNotesBody.text=body;fieldNotesBody.fontSize=20*(LargeText?1.12f:1);
            LayoutFieldNotes();fieldNotesScroll.verticalNormalizedPosition=1;
        }

        void LayoutFieldNotes()
        {
            if(!fieldNotesOpen)return;
            float width=Mathf.Min(800,Screen.width/PixelRatio-32),height=Mathf.Min(740,Screen.height/PixelRatio-32);
            modal.transform.localScale=Vector3.one;((RectTransform)modal.transform).sizeDelta=new Vector2(width,height);
            modalText.rectTransform.sizeDelta=new Vector2(width-64,42);
            fieldNotesScroll.GetComponent<RectTransform>().sizeDelta=new Vector2(width-64,height-300);
            fieldNotesContent.sizeDelta=new Vector2(width-64,fieldNotesContent.sizeDelta.y);
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(fieldNotesContent);
            fieldNotesControls.gameObject.SetActive(fieldNotesContent.rect.height>fieldNotesScroll.viewport.rect.height+1);
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
            row.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().preferredHeight=96;fieldNotesRows.Add(row.gameObject);
            Label(row,label+"  / 100",new Vector2(0,1),Vector2.zero,new Vector2(640,24),18,Ink,FontStyles.Bold);
            string[] names={"Before","Now"};double[] values={before,after};
            for(int i=0;i<2;i++)
            {
                Label(row,names[i],new Vector2(0,1),new Vector2(0,-30-i*30),new Vector2(70,24),16,Ink);
                var track=Rect(row,names[i]+" track",new Vector2(0,1),new Vector2(82,-34-i*30),new Vector2(0,16));
                track.anchorMax=new Vector2(1,1);track.offsetMax=new Vector2(-64,track.offsetMax.y);
                track.gameObject.AddComponent<UnityEngine.UI.Image>().color=new Color(.81f,.85f,.82f);
                var fill=Rect(track,names[i]+" score bar",Vector2.zero,Vector2.zero,Vector2.zero);
                fill.anchorMax=new Vector2(Mathf.Clamp01((float)values[i]/100),1);fill.offsetMin=fill.offsetMax=Vector2.zero;
                fill.gameObject.AddComponent<UnityEngine.UI.Image>().color=i==0?new Color(.42f,.49f,.47f):color;
                Label(row,values[i].ToString("0.0"),new Vector2(1,1),new Vector2(0,-30-i*30),new Vector2(54,24),16,Ink);
            }
            foreach(var text in row.GetComponentsInChildren<TextMeshProUGUI>())text.fontSize=sizes[text]*(LargeText?1.12f:1);
            LayoutFieldNotes();
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
            Cost("Capital refund",-ledger.capitalRefundCents);
            Cost("Repairs",ledger.repairPaidCents);Cost("Earlier unpaid bills",ledger.arrearsPaidCents);
            long total=ledger.upfrontPaidCents+ledger.operatingPaidCents+ledger.programPaidCents+ledger.repairPaidCents+ledger.arrearsPaidCents;
            string body="<b>Your plan</b>\n"+string.Join("\n",rows)+"\n\n<b>Pay now   "+NotesMoney(total)+"</b>"+
                "\nTax receipts to collect   +"+NotesMoney(ledger.publicReceiptsCents)+
                "\nAnnual public funding   +"+NotesMoney(ledger.appropriationCents)+
                "\n\n"+NotesMoney(ledger.openingCashCents)+" + "+NotesMoney(ledger.capitalRefundCents)+" − "+NotesMoney(total)+" + "+NotesMoney(ledger.publicReceiptsCents)+" + "+NotesMoney(ledger.appropriationCents)+
                "\n<b>Balance after this year   "+NotesMoney(ledger.closingCashCents)+"</b>"+
                "\n\nReserved for later years   "+NotesMoney(ledger.futureCommitmentsCents)+
                (ledger.closingArrearsCents>0?"\nUnpaid bills carried forward   "+NotesMoney(ledger.closingArrearsCents):"")+
                "\n\n"+mode+" · game estimates. Ending the year commits this plan."+
                (AnnualPlayerRules.Enabled(annualRules)?" Regional hazards use inferred chances, not forecasts. Unexpected repairs may change this estimate. A negative balance blocks new construction. Results save before AI explanation; if AI fails, a local summary uses those same values.":"");
            FieldNotes("Review year "+state.year,body,"End Year",accept);
        }
    }
}
