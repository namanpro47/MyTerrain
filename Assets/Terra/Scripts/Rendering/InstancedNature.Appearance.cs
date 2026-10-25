using System.Collections.Generic;
using UnityEngine;

namespace Terra.Rendering
{
    public sealed partial class InstancedNature
    {
        Dictionary<Material,Color> originalTints;

        // Drawn batches have no Renderer component for MaterialPropertyBlock overrides.
        // Add already clones these materials, so changing them cannot recolor another site.
        public void SetTint(Color tint,float strength)
        {
            strength=Mathf.Clamp01(strength);
            if(originalTints==null)originalTints=new Dictionary<Material,Color>();
            foreach(var material in ownedMaterials)
            {
                // Some artist shaders use this name for a texture rather than a color.
                if(!material.HasColor("_BaseColor"))continue;
                if(!originalTints.TryGetValue(material,out var original))
                {
                    original=material.GetColor("_BaseColor");originalTints.Add(material,original);
                }
                var color=Color.Lerp(original,tint,strength);
                color.a=original.a; // Status must not change foliage cutout coverage.
                material.SetColor("_BaseColor",color);
            }
        }
    }
}
