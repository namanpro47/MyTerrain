using System;
using System.Linq;
using System.Text;
using Terra.Core;
class Program {
 static int checks;
 static void Check(bool value,string reason) { checks++;if(!value)throw new Exception(reason); }
 static void Main() {
  var now=new DateTime(2026,10,25,20,0,0,DateTimeKind.Utc);
  var p=new LocalActionPlan();Check(!p.Valid,"empty plan invalid");
  p.action="ask about a local plan";Check(p.Valid,"action and default time valid");
  p.SaveAt(now,true);Check(!p.Due(now.AddDays(13)),"not early");Check(p.Due(now.AddDays(14)),"due at fourteen days");
  p.completed=true;Check(!p.Due(now.AddDays(15)),"completion clears due");
  p.SaveAt(now,false);Check(!p.Due(now.AddDays(40))&&p.checkInUtc=="","opt out removes reminder");
  p.level=4;Check(!p.Valid,"unknown level invalid");p.level=0;
  p.when=" ";Check(!p.Valid,"blank time invalid");p.when="after lunch";
  p.action=new string('a',501);Check(!p.Valid,"bounded action");
  p.action="ask about parks, water; and paths\nBEGIN:VEVENT";
  string ics=p.Calendar(now);Check(ics.Contains("DTSTART;VALUE=DATE:20261108"),"calendar date");
  Check(ics.Contains("DTEND;VALUE=DATE:20261109"),"all day end exclusive");
  Check(ics.Split("\r\nBEGIN:VEVENT").Length==2,"no calendar injection");
  p.action=string.Concat(Enumerable.Repeat("土地🌳",60));
  ics=p.Calendar(now);Check(ics.Split("\r\n").All(s=>Encoding.UTF8.GetByteCount(s)<=75),"UTF8 calendar folding");
  Console.WriteLine(checks+" local action plan checks passed.");
 }
}
