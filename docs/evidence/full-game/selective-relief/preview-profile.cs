var main=UnityEngine.Object.FindFirstObjectByType<Terra.Gameplay.TerraMain>();var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
var land=(Terra.Rendering.LandscapeRenderer)main.GetType().GetField("landscape",flags).GetValue(main);var world=(Terra.Contracts.WorldData)main.GetType().GetField("world",flags).GetValue(main);
var flush=land.GetType().GetMethod("FlushSwaleGround",flags);var results=new System.Collections.Generic.List<object>();
try {
 foreach(string id in new[]{"south-07","south-02","south-08"})
 {
  var site=world.sites.First(s=>s.id==id);
  var watch=System.Diagnostics.Stopwatch.StartNew();land.Ghost(site,true,"water-swale");watch.Stop();double ghostMs=watch.Elapsed.TotalMilliseconds;
  watch.Restart();flush.Invoke(land,null);watch.Stop();double groundMs=watch.Elapsed.TotalMilliseconds;
  watch.Restart();land.ViewScale(.25f);watch.Stop();double surfaceAndGroundMs=watch.Elapsed.TotalMilliseconds;
  watch.Restart();land.ClearGhost();land.ViewScale(.25f);watch.Stop();double cancelMs=watch.Elapsed.TotalMilliseconds;
  results.Add(new{id,ghostMs,groundMs,surfaceAndGroundMs,cancelMs});
 }
 return new{results,frame=UnityEngine.Time.frameCount};
} finally{land.ClearGhost();land.ViewScale(.25f);}
