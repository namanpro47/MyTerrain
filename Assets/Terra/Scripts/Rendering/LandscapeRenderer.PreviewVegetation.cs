using System.Collections.Generic;
using Terra.Contracts;
using UnityEngine;

namespace Terra.Rendering
{
    public sealed partial class LandscapeRenderer
    {
        readonly Dictionary<string,BaselineVegetation> baselineVegetation=new Dictionary<string,BaselineVegetation>();
        readonly Dictionary<string,Bounds> baselineProjectLots=new Dictionary<string,Bounds>();
        string ghostPreviewSite,ghostPreviewAction;
        bool ghostPreviewValid;

        bool UsesBaselineVegetation(string treatment)
        {
            var rule=buildRules?.Find(treatment);
            // Wind keeps its authored meadow; land treatments still show their own planting.
            return rule!=null&&!rule.landscape&&treatment!="wind";
        }
        Bounds ProjectLot(Site site,string treatment)
        {
            var layout=buildRules.Layout(site,buildRules.Find(treatment));
            var ring=treatment=="wind"?layout.foundation:layout.footprint;
            var a=ring[0];var b=ring[2];
            return new Bounds(new Vector3((a.x+b.x)*.5f,0,(a.z+b.z)*.5f),new Vector3(b.x-a.x+2,10000,b.z-a.z+2));
        }
        BaselineVegetation BaselineVegetationFor(string id)
        {
            if(!baselineVegetation.TryGetValue(id,out var value))
            {value=new BaselineVegetation(baselines[id],meshes);baselineVegetation.Add(id,value);}
            return value;
        }
        void RestoreBaselineVegetation(string id)
        {
            bool shared=baselineProjectLots.TryGetValue(id,out var footprint);
            if(shared)BaselineVegetationFor(id).Exclude(footprint);
            else if(baselineVegetation.TryGetValue(id,out var value))value.Exclude(null);
            baselines[id].gameObject.SetActive(shared||!proposals.ContainsKey(id));
        }

        sealed class BaselineVegetation
        {
            readonly InstancedNature[] plants;
            readonly List<SoilSlice> soil=new List<SoilSlice>();
            public BaselineVegetation(Transform baseline,List<Mesh> owned)
            {
                plants=baseline.GetComponentsInChildren<InstancedNature>(true);
                foreach(var filter in baseline.GetComponentsInChildren<MeshFilter>(true))
                    if(filter.sharedMesh!=null&&filter.name=="Cultivated soil beds")soil.Add(new SoilSlice(filter,owned));
            }
            public void Exclude(Bounds? footprint)
            {
                foreach(var plant in plants)plant.ExcludeFootprint(footprint);
                foreach(var slice in soil)slice.Exclude(footprint);
            }
        }
        sealed class SoilSlice
        {
            readonly MeshFilter filter;
            readonly Mesh original;
            Mesh clipped;
            readonly List<Mesh> owned;
            readonly int[] indices;
            readonly Bounds[] triangles;
            readonly List<int> visible=new List<int>();
            Bounds? cached;
            public SoilSlice(MeshFilter filter,List<Mesh> owned)
            {
                this.filter=filter;this.owned=owned;original=filter.sharedMesh;indices=original.triangles;
                var vertices=original.vertices;
                // Cache world-space triangle bounds once. No terrain sampling or tessellation on hover.
                for(int i=0;i<vertices.Length;i++)vertices[i]=filter.transform.TransformPoint(vertices[i]);
                triangles=new Bounds[indices.Length/3];
                for(int t=0;t<triangles.Length;t++)
                {
                    var bounds=new Bounds(vertices[indices[t*3]],Vector3.zero);
                    bounds.Encapsulate(vertices[indices[t*3+1]]);bounds.Encapsulate(vertices[indices[t*3+2]]);triangles[t]=bounds;
                }
            }
            public void Exclude(Bounds? footprint)
            {
                if(!footprint.HasValue){filter.sharedMesh=original;return;}
                if(!cached.HasValue||cached.Value!=footprint.Value)
                {
                    if(clipped==null){clipped=UnityEngine.Object.Instantiate(original);clipped.name=original.name+" outside project lot";owned.Add(clipped);}
                    cached=footprint;visible.Clear();var lot=footprint.Value;
                    for(int t=0;t<triangles.Length;t++)
                    {
                        var box=triangles[t];
                        if(box.max.x>=lot.min.x&&box.min.x<=lot.max.x&&box.max.z>=lot.min.z&&box.min.z<=lot.max.z)continue;
                        visible.Add(indices[t*3]);visible.Add(indices[t*3+1]);visible.Add(indices[t*3+2]);
                    }
                    clipped.SetTriangles(visible,0,false);
                }
                filter.sharedMesh=clipped;
            }
        }
    }
}
