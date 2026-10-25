using System;
using System.Collections.Generic;
using UnityEngine;

namespace Terra.Rendering
{
    public sealed partial class InstancedNature
    {
        // Resource materials are immutable inputs. Key the effective construction
        // parameters, not a content hash: distinct artist assets must not alias.
        readonly struct MaterialKey : IEquatable<MaterialKey>
        {
            public readonly Material template;
            public readonly Texture texture;
            public readonly Color tint;
            public readonly Vector4 plantBounds;
            public readonly bool soft,grassTint;
            public MaterialKey(Material source,Color tint,bool soft,Bounds bounds,bool grassTint)
            {
                template=soft?Resources.Load<Material>("Nature/Groundcover"):source;
                texture=soft?source.mainTexture:null;
                this.tint=soft||grassTint?tint:Color.white;
                plantBounds=soft?new Vector4(bounds.min.y,bounds.max.y,0,0):Vector4.zero;
                this.soft=soft;this.grassTint=grassTint;
            }
            public bool Equals(MaterialKey other)=>template==other.template&&texture==other.texture&&
                tint.Equals(other.tint)&&plantBounds.Equals(other.plantBounds)&&soft==other.soft&&grassTint==other.grassTint;
            public override bool Equals(object other)=>other is MaterialKey key&&Equals(key);
            public override int GetHashCode()
            {
                unchecked
                {
                    int hash=template.GetHashCode();hash=hash*31+(texture==null?0:texture.GetHashCode());
                    hash=hash*31+tint.GetHashCode();hash=hash*31+plantBounds.GetHashCode();
                    return (hash*31+soft.GetHashCode())*31+grassTint.GetHashCode();
                }
            }
        }
        sealed class SharedMaterial
        {
            public MaterialKey key;
            public Material material;
            public int users;
        }
        static readonly Dictionary<MaterialKey,SharedMaterial> materialCache=new Dictionary<MaterialKey,SharedMaterial>();
        readonly Dictionary<Material,SharedMaterial> borrowedMaterials=new Dictionary<Material,SharedMaterial>();

        Material BorrowMaterial(Material source,Color tint,bool soft,Bounds bounds,bool grassTint)
        {
            var key=new MaterialKey(source,tint,soft,bounds,grassTint);
            if(!materialCache.TryGetValue(key,out var entry))
            {
                var material=new Material(key.template){enableInstancing=true};
                if(soft)
                {
                    material.SetTexture("_BaseMap",key.texture);
                    material.SetColor("_BaseColor",tint);
                    material.SetFloat("_Cutoff",.5f);
                    material.SetVector("_PlantBounds",key.plantBounds);
                }
                if(grassTint)material.color=tint;
                entry=new SharedMaterial{key=key,material=material};materialCache.Add(key,entry);
            }
            // One lease per component, even when multiple LODs or Add calls reuse it.
            if(!borrowedMaterials.ContainsKey(entry.material))
            {borrowedMaterials.Add(entry.material,entry);entry.users++;}
            return entry.material;
        }

        static void ReleaseMaterial(SharedMaterial entry)
        {
            if(--entry.users!=0)return;
            materialCache.Remove(entry.key);
            Destroy(entry.material);
        }

        void MakeTintMaterialsPrivate()
        {
            var replacements=new Dictionary<Material,Material>();
            foreach(var pair in borrowedMaterials)
            {
                if(!pair.Key.HasColor("_BaseColor"))continue;
                var copy=new Material(pair.Key);ownedMaterials.Add(copy);replacements.Add(pair.Key,copy);
            }
            if(replacements.Count==0)return;
            foreach(var batch in batches)
            {
                if(replacements.TryGetValue(batch.material,out var material))batch.material=material;
                if(replacements.TryGetValue(batch.mediumMaterial,out var medium))batch.mediumMaterial=medium;
                if(replacements.TryGetValue(batch.distantMaterial,out var distant))batch.distantMaterial=distant;
            }
            foreach(var pair in replacements)
            {
                ReleaseMaterial(borrowedMaterials[pair.Key]);borrowedMaterials.Remove(pair.Key);
            }
        }
    }
}
