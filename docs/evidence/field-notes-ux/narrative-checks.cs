using System.Text.Json;
using Terra.Core;
using Terra.Contracts;
var json=new JsonSerializerOptions{IncludeFields=true,WriteIndented=true};
var definition=JsonSerializer.Deserialize<AnnualTownDefinition>(File.ReadAllText("Builds/FieldNotes/model/definition.json"),json);
int assertions=0;
void Check(bool value,string message){if(!value)throw new Exception(message);assertions++;}
foreach(var route in new[]{"community-and-services","land-and-livelihoods"})
{
 var state=JsonSerializer.Deserialize<AnnualTownSnapshot>(File.ReadAllText("Builds/FieldNotes/model/"+route+".json"),json);
 var town=AnnualTown.Restore(definition,state);
 var before=JsonSerializer.Serialize(state,json);
 foreach(var report in state.history)
 {
  var chapter=AnnualChapterNarrative.Create(definition,report);
  AnnualChapters.ValidateText(chapter,AnnualChapters.Facts(town));assertions++;
  Check(chapter.cards.Length==3,"Three authored story pages required.");
  Check(!chapter.cards.Any(c=>c.explanation.Contains("vs last year")||c.explanation.Contains("/ 100")||c.explanation.Contains("Output multiplier")),"Numeric score block escaped into prose.");
  Check(!definition.sites.Any(s=>chapter.cards.Any(c=>c.explanation.Contains(s.name))),"Lot names escaped into story.");
  Check(chapter.cards.All(c=>c.explanation.Length>150),"Narrative is underdeveloped.");
 }
 Check(before==JsonSerializer.Serialize(state,json),"Reading changed saved reports.");
}
Console.WriteLine("Narrative checks passed: "+assertions);
File.WriteAllText("docs/evidence/field-notes-ux/narrative-checks.json",JsonSerializer.Serialize(new{assertions,acceptedYears=20,saveUnchanged=true},json));
