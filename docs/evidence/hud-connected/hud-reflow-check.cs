// Run against the isolated Editor in Play mode; changes only the disposable runtime HUD.
var main=UnityEngine.Object.FindFirstObjectByType<Terra.Gameplay.TerraMain>();
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
var hud=(Terra.UI.TerraHud)main.GetType().GetField("hud",flags).GetValue(main);
var world=(Terra.Contracts.WorldData)main.GetType().GetField("world",flags).GetValue(main);
var session=(Terra.Gameplay.AnnualTownSession)main.GetType().GetField("annual",flags).GetValue(main);
hud.CloseModal();
var ui=typeof(Terra.UI.TerraHud);
var root=(UnityEngine.Transform)ui.GetField("root",flags).GetValue(hud);
var layout=ui.GetMethod("LayoutAnnual",flags);
int checks=0;var failures=new System.Collections.Generic.List<string>();
void Check(bool ok,string reason){if(!ok)failures.Add(reason);checks++;}
var cases=new System.Collections.Generic.List<object>();
foreach(bool large in new[]{false,true})
{
    ui.GetProperty("LargeText").SetValue(hud,large);
    foreach(var size in new[]{new UnityEngine.Vector2(1440,900),new UnityEngine.Vector2(1024,576),new UnityEngine.Vector2(1280,720),new UnityEngine.Vector2(1440,900)})
    {
        bool compact=size.x<1120||size.y<680;
        for(int repeat=0;repeat<3;repeat++)layout.Invoke(hud,new object[]{size.x,size.y,compact});
        var existingBody=(TMPro.TextMeshProUGUI)ui.GetField("previewBody",flags).GetValue(hud);
        if(!string.IsNullOrEmpty(existingBody.text))
        {existingBody.ForceMeshUpdate(true,true);Check(existingBody.preferredHeight<=existingBody.rectTransform.rect.height+.1f,"Existing inspection overflows after resize: "+size+" larger="+large);}
        var identity=root.Find("Place").GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
        Check(identity.Any(t=>t.text.Contains("Meadowbrook")&&t.gameObject.activeSelf),"Location disappeared after reflow");
        foreach(var title in root.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true).Where(t=>t.name=="Action title"))
        {
            var price=title.transform.parent.Find("Action price").GetComponent<TMPro.TextMeshProUGUI>();
            Check(price.rectTransform.anchoredPosition.y>title.rectTransform.anchoredPosition.y,"Price overlaps title: "+title.text);
        }
        foreach(var offer in session.Definition.actions.GroupBy(a=>a.actionId).Select(g=>g.First()))
        {
            hud.AnnualPreview(world.sites.First(s=>s.id==offer.siteId),offer,session.State,"place",null,()=>{},false);
            var body=(TMPro.TextMeshProUGUI)ui.GetField("previewBody",flags).GetValue(hud);body.ForceMeshUpdate(true,true);
            Check(body.preferredHeight<=body.rectTransform.rect.height+.1f,"Preview overflow: "+offer.actionId+" "+size+" large="+large+" needs "+body.preferredHeight+" has "+body.rectTransform.rect.height);
        }
        var available=(TMPro.TextMeshProUGUI)ui.GetField("annualAvailable",flags).GetValue(hud);
        foreach(var amount in new[]{"$10,000,000","$10,810,000","$19,850,000"})
        {
            available.text=amount+" <size=45%>available</size>";available.ForceMeshUpdate(true,true);
            Check(available.textInfo.lineCount==1&&available.preferredHeight<=available.rectTransform.rect.height+.1f,"Available balance wraps: "+amount+" larger="+large);
        }
        foreach(var label in root.GetComponentsInChildren<TMPro.TextMeshProUGUI>().Where(t=>!string.IsNullOrWhiteSpace(t.text)))
        {
            label.ForceMeshUpdate(true,true);
            Check(label.preferredHeight<=label.rectTransform.rect.height+.1f,"Visible text overflows: "+label.text+" at "+size+" larger="+large);
        }
        var subtitle=(TMPro.TextMeshProUGUI)ui.GetField("placeSubtitle",flags).GetValue(hud);
        foreach(var profile in Terra.Core.LocationProfile.All)
        {
            identity[1].text=profile.Name;subtitle.text=profile.Subtitle;
            foreach(var label in new[]{identity[1],subtitle})
            {label.ForceMeshUpdate(true,true);Check(label.preferredHeight<=label.rectTransform.rect.height+.1f&&label.GetRenderedValues(false).x<=label.rectTransform.rect.width+.1f,"Regional identity overflows: "+label.text);}
        }
        identity[1].text=Terra.Core.LocationProfile.All[0].Name;subtitle.text=Terra.Core.LocationProfile.All[0].Subtitle;
        cases.Add(new{width=size.x,height=size.y,large,compact});
    }
}
// A returning 3.1 player sees the retained-save notice with the new arrival text.
var arrivalSource=System.IO.File.ReadAllText("Assets/Terra/Scripts/Gameplay/TerraMain.Future.cs");
var arrivalLiteral=System.Text.RegularExpressions.Regex.Match(arrivalSource,@"""You have \$10M[^""]*""").Value.Trim('"');
var arrival=System.Text.RegularExpressions.Regex.Unescape(arrivalLiteral);
Check(arrival.StartsWith("You have $10M")&&arrival.Length>400,"Arrival text extraction failed");
foreach(bool large in new[]{false,true})
{
    hud.AnnualDialog("Ten years in Meadowbrook Valley","Connected fields use a new run. Your previous-layout save is retained locally.\n\n"+arrival,"Explore the landscape",()=>{});
    var text=(TMPro.TextMeshProUGUI)ui.GetField("modalText",flags).GetValue(hud);text.fontSize=20*(large?1.12f:1);text.ForceMeshUpdate(true,true);
    Check(text.preferredHeight<=text.rectTransform.rect.height+.1f,"Retained-save arrival overflows larger="+large+" needs "+text.preferredHeight+" has "+text.rectTransform.rect.height);
}
return new{checks,failures,cases};
