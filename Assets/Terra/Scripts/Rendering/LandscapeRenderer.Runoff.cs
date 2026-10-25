using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Terra.Rendering
{
    public sealed partial class LandscapeRenderer
    {
        readonly Dictionary<string,LineRenderer> runoffOutlines=new Dictionary<string,LineRenderer>();
        // This is an explicitly hypothetical membership overlay, not a simulated flow path or flood extent.
        public void ShowRunoffConnections(IEnumerable<string> sites)
        {
            var members=new HashSet<string>(sites);
            foreach(var id in members)
                if(!runoffOutlines.ContainsKey(id))
                {
                    var line=Object.Instantiate(outlines[id],root);line.name="Assumed connection "+id;
                    line.sharedMaterial=BoundaryMat(new Color(.55f,.84f,.90f));runoffOutlines[id]=line;
                }
            foreach(var pair in runoffOutlines)pair.Value.enabled=members.Contains(pair.Key)&&pair.Key!=activeOutline;
            ScaleRunoffConnections();
        }
        public void ScaleRunoffConnections(){foreach(var line in runoffOutlines.Values)line.startWidth=line.endWidth=2.6f*boundaryScale;}
    }
}
