using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Terra.Contracts;
using Terra.Core;
using Terra.Gameplay;
class NumericCodecProbe {
 static int checks;
 static void Check(bool okay,string why){checks++;if(!okay)throw new Exception(why);}
 sealed class Store:IAnnualNumericStore {
  public string Json;
  public Task<AnnualNumericSave> Read(CancellationToken cancellation){return Task.FromResult(AnnualNumericJson.ReadSave(Json));}
  public Task Write(string expected,AnnualNumericSave candidate,CancellationToken cancellation){throw new Exception("Read-only probe must not write");}
 }
 static void Main(){
  const string folder="Builds/AnnualNumericClientVectors/";
  foreach(string prefix in new[]{"", "dense-"}) {
  var definition=AnnualNumericJson.ReadDefinition(File.ReadAllText(folder+prefix+"definition.json"));
  var original=AnnualNumericJson.ReadSave(File.ReadAllText(folder+prefix+"save.json"));
  var run=AnnualNumericTransactions.Open(definition,original.town.runId,new Store{Json=File.ReadAllText(folder+prefix+"save.json")}).GetAwaiter().GetResult();
  Check(run.State.history.Length==10&&run.State.phase==TownPhase.Completed,"Complete decade");
  Check(run.SaveState.saveHash==original.saveHash,"Exact .NET-to-Mono envelope hash");
  Check(new AnnualTown(definition,"probe").DefinitionHash==original.town.definitionHash,"Exact definition hash");
  for(int i=0;i<10;i++){
   Check(run.State.history[i].simulation.transitionHash==original.town.history[i].simulation.transitionHash,"Report identity year"+(i+1));
   var actual=run.ForYear(i+1);var expected=original.narration[i];
   Check(actual.status==expected.status && actual.receipt?.requestHash==expected.receipt?.requestHash
    && ReportNarration.RequestHash(actual.request)==ReportNarration.RequestHash(expected.request),"Narration identity year"+(i+1));
  }
  var copy=AnnualNumericJson.ReadSave(AnnualNumericJson.WriteSave(run.SaveState));
  Check(AnnualNumericTransactions.Hash(copy)==original.saveHash,"Mono own complete envelope round trip");
  var rules=AnnualNumericJson.ReadDefinition(AnnualNumericJson.WriteDefinition(definition));
  Check(new AnnualTown(rules,"again").DefinitionHash==original.town.definitionHash,"Mono own definition round trip");
  Console.WriteLine((prefix==""?"Sparse":"Dense")+" PASS "+checks+" exact codec assertions on Unity bundled Mono; save "+original.saveHash+". No Editor or browser.");
  }
 }
}
