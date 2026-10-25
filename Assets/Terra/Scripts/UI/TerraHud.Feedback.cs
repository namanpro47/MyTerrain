using System;
using System.Collections.Generic;
using System.Linq;
using Terra.Core;
using TMPro;
using UnityEngine;

namespace Terra.UI
{
    public sealed partial class TerraHud
    {
        // TN-03/05/07, CS-01, AX-01: exact signed changes, stable metric anchors,
        // one readable receipt, and identical information with sound/motion disabled.
        Transform changeReceipt;
        CanvasGroup changeOpacity;
        TextMeshProUGUI changeTitle, changeMoney, changeContext, changeScores;
        readonly List<AnnualFeedback> recentChanges = new List<AnnualFeedback>();
        AnnualFeedback activeChange;
        float changeRemaining;
        const float ChangeDuration = 7;
        static readonly Color Decrease = new Color(1f,.73f,.53f);
        static Color DeltaColor(double delta) => delta < 0 ? Decrease : delta > 0 ? Accent : Soft;
        static string DeltaHex(double delta) => ColorUtility.ToHtmlStringRGB(DeltaColor(delta));
        public event Action<AnnualFeedbackTone> FeedbackSound;

        void CreateChangeReceipt()
        {
            changeReceipt = Panel(root,"Action receipt",new Vector2(.5f,1),new Vector2(0,-118),new Vector2(340,214),PanelColor);
            changeReceipt.SetSiblingIndex(modalShade.transform.GetSiblingIndex());
            changeReceipt.GetComponent<UnityEngine.UI.Image>().raycastTarget = false;
            changeOpacity = changeReceipt.gameObject.AddComponent<CanvasGroup>();
            changeOpacity.blocksRaycasts = false; changeOpacity.interactable = false;
            changeTitle = Label(changeReceipt,"",new Vector2(0,1),new Vector2(16,-12),new Vector2(308,40),16,Paper,FontStyles.Bold);
            changeTitle.enableAutoSizing = true; changeTitle.fontSizeMin = 14; changeTitle.fontSizeMax = 16;
            changeMoney = Label(changeReceipt,"",new Vector2(0,1),new Vector2(16,-56),new Vector2(308,36),29,Paper,FontStyles.Bold);
            changeMoney.enableAutoSizing = true; changeMoney.fontSizeMin = 23; changeMoney.fontSizeMax = 29;
            changeMoney.textWrappingMode = TextWrappingModes.NoWrap;
            changeContext = Label(changeReceipt,"",new Vector2(0,1),new Vector2(16,-94),new Vector2(308,36),13,Soft);
            changeScores = Label(changeReceipt,"",new Vector2(0,1),new Vector2(16,-137),new Vector2(308,66),15,Paper);
            changeReceipt.gameObject.SetActive(false);
        }
        public void ClearChangeFeedback()
        {
            recentChanges.Clear(); DismissChange();
        }
        void DismissChange()
        {
            activeChange = null; changeRemaining = 0;
            if(changeReceipt!=null)changeReceipt.gameObject.SetActive(false);
            RestoreMetricFeedback();
        }
        public void ShowChange(AnnualFeedback change)
        {
            if(changeReceipt==null)CreateChangeReceipt();
            activeChange=change;changeRemaining=ChangeDuration;
            recentChanges.Insert(0,change);if(recentChanges.Count>8)recentChanges.RemoveAt(8);
            changeTitle.text=change.title;
            changeMoney.text=change.availableDelta==0?"Available unchanged":AnnualFeedback.Money(change.availableDelta);
            changeMoney.color=DeltaColor(change.availableDelta);
            changeContext.text=change.accepted ? "Available funds · cash "+AnnualFeedback.Money(change.cashDelta)+"\nAccepted change from previous year"
                : (change.availableDelta<0?"Reserved from available funds":change.availableDelta>0?"Released to available funds":"No change to reserved funds")+"\nYear-end forecast changes";
            string[] names={"Economic","Social","Environment"};
            changeScores.text=string.Join("\n",names.Select((name,i)=>name+"  <b><color=#"+DeltaHex(change.scoreDeltas[i])+">"+AnnualFeedback.Points(change.scoreDeltas[i])+"</color></b> <size=85%>→ "+change.scores[i].ToString("0.0",System.Globalization.CultureInfo.InvariantCulture)+"</size>"));
            FeedbackSound?.Invoke(change.tone);
            TickChangeFeedback(0);
        }
        void RestoreMetricFeedback()
        {
            if(annualAvailable!=null)annualAvailable.color=Paper;
            for(int i=0;i<3;i++)if(annualScores[i]!=null&&annualScoreReadout[i]!=null)annualScores[i].text=annualScoreReadout[i];
        }
        public void TickChangeFeedback(float seconds)
        {
            if(activeChange==null||changeReceipt==null)return;
            bool visible=!ModalOpen&&!Terra.Audio.TerraAudioPanel.IsOpen
                &&(placementCapsule==null||!placementCapsule.gameObject.activeSelf);
            changeReceipt.gameObject.SetActive(visible&&changeRemaining>0);
            if(!visible)return; // Reading a dialog or changing tabs must not swallow the receipt.
            // Keep the receipt visible while unfocused, and never let a resumed or
            // stalled frame consume the entire reading window.
            if(Application.isFocused)changeRemaining=Mathf.Max(0,changeRemaining-Mathf.Clamp(seconds,0,.1f));
            if(changeRemaining<=0)
            {
                changeReceipt.gameObject.SetActive(false);activeChange=null;RestoreMetricFeedback();return;
            }
            toastPanel.SetActive(false);
            changeOpacity.alpha=ReducedMotion?1:Mathf.Min(1,changeRemaining/.45f);
            float age=ChangeDuration-changeRemaining;
            ((RectTransform)changeReceipt).anchoredPosition=new Vector2(AnnualOverviewOpen?Mathf.Max(0,546-annualLayoutWidth/2):0,-118+(ReducedMotion?0:8*Mathf.Clamp01(1-age/.22f)));
            annualAvailable.color=age<1.4f?DeltaColor(activeChange.availableDelta):Paper;
            for(int i=0;i<3;i++)
            {
                if(activeChange.scoreDeltas[i]==0)continue;
                string value=annualScoreReadout[i].Split('\n')[0];
                annualScores[i].text=value+"\n<size=65%><b><color=#"+DeltaHex(activeChange.scoreDeltas[i])+">"+AnnualFeedback.Points(activeChange.scoreDeltas[i])+" "+(activeChange.accepted?"this year":"forecast")+"</color></b></size>";
            }
        }
        void ShowRecentChanges(int index=0)
        {
            if(recentChanges.Count==0){AnnualDialog("Recent changes","Your next plan edit or accepted year will appear here. Loading a saved run does not count as a new change.","Back to landscape",CloseModal);return;}
            var change=recentChanges[index];string[] names={"Economic","Social","Environmental"};
            string scores=string.Join("\n",names.Select((name,i)=>name+": "+AnnualFeedback.Points(change.scoreDeltas[i])+" → "+change.scores[i].ToString("0.0")));
            AnnualDialog("Recent changes · "+(index+1)+" / "+recentChanges.Count,"<size=88%><b>"+change.title+"</b>\n\n"+change.detail+"\n\n"+(change.accepted?"Accepted scores":"Year-end forecast")+"\n"+scores+"</size>","Back to landscape",CloseModal);
            if(index+1<recentChanges.Count)AnnualSecondary("Previous change",()=>ShowRecentChanges(index+1));
        }
        void ShowPublicBudget()
        {
            AnnualDialog("Public budget",annualBudget.text+"\n\nAvailable = cash minus accepted commitments and this year's reserved plan.\n\nQueueing reserves the full public term. End Year pays this year's bills.","Back to landscape",CloseModal);
            AnnualSecondary("Recent changes",()=>ShowRecentChanges());
        }
    }
}
