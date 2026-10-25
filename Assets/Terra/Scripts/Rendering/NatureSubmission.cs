using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Terra.Rendering
{
    // Combine small neighbouring submissions only after each source batch has
    // selected its existing LOD and passed its existing conservative culling.
    [DefaultExecutionOrder(10000)]
    public sealed class NatureSubmission : MonoBehaviour
    {
        const int Capacity=500;
        const float CellSize=192;
        static NatureSubmission instance;
        readonly Dictionary<Key,int> active=new Dictionary<Key,int>();
        readonly List<Draw> pool=new List<Draw>();
        int frame=-1,used;
        public int LastSourceBatches{get;private set;}
        public int LastDrawCalls{get;private set;}
#if UNITY_EDITOR
        public static bool SeparateForComparison{get;set;}
        public static float ComparisonCellSize{get;set;}=CellSize;
#endif
        readonly struct Key : IEquatable<Key>
        {
            readonly Material material;readonly Mesh mesh;
            readonly int submesh,x,z;readonly ShadowCastingMode shadows;
            public Key(RenderParams p,Mesh mesh,int submesh)
            {
                material=p.material;this.mesh=mesh;this.submesh=submesh;shadows=p.shadowCastingMode;
                float cellSize=CellSize;
#if UNITY_EDITOR
                cellSize=ComparisonCellSize;
#endif
                x=Mathf.FloorToInt(p.worldBounds.center.x/cellSize);z=Mathf.FloorToInt(p.worldBounds.center.z/cellSize);
            }
            public bool Equals(Key k)=>material==k.material&&mesh==k.mesh&&submesh==k.submesh&&x==k.x&&z==k.z&&shadows==k.shadows;
            public override bool Equals(object obj)=>obj is Key k&&Equals(k);
            public override int GetHashCode(){unchecked{return (((((material.GetHashCode()*397)^mesh.GetHashCode())*397^submesh)*397^x)*397^z)*397^(int)shadows;}}
        }
        sealed class Draw
        {
            public Matrix4x4[] matrices,source;
            public RenderParams parameters;public Mesh mesh;public int submesh,count;
        }
        internal static void Ensure()
        {
            if(instance!=null)return;
            instance=FindFirstObjectByType<NatureSubmission>();if(instance!=null)return;
            var go=new GameObject("Vegetation submissions");
            instance=go.AddComponent<NatureSubmission>();
        }
        internal static void Submit(RenderParams parameters,Mesh mesh,int submesh,Matrix4x4[] matrices)
        {
            // Already-large batches gain little from copying. Transparent draws
            // retain their original bounds/sort order. All current cutouts are opaque.
            if(instance==null||!instance.isActiveAndEnabled||matrices.Length>=128||parameters.material.renderQueue>2500
#if UNITY_EDITOR
                ||SeparateForComparison
#endif
                )
            {Graphics.RenderMeshInstanced(parameters,mesh,submesh,matrices);return;}
            instance.Add(parameters,mesh,submesh,matrices);
        }
        void Add(RenderParams parameters,Mesh mesh,int submesh,Matrix4x4[] matrices)
        {
            if(frame!=Time.frameCount)
            {Clear();frame=Time.frameCount;LastSourceBatches=0;LastDrawCalls=0;}
            LastSourceBatches++;
            var key=new Key(parameters,mesh,submesh);
            if(!active.TryGetValue(key,out int index)||pool[index].count+matrices.Length>Capacity)
            {
                index=used++;if(index==pool.Count)pool.Add(new Draw());
                var next=pool[index];next.parameters=parameters;next.mesh=mesh;next.submesh=submesh;next.count=0;
                active[key]=index;
            }
            var draw=pool[index];
            // Most spatial keys have only one source. Keep its existing array;
            // allocate/copy a pooled buffer only when a second source joins it.
            if(draw.count==0){draw.source=matrices;draw.count=matrices.Length;return;}
            if(draw.source!=null)
            {
                if(draw.matrices==null)draw.matrices=new Matrix4x4[Capacity];
                Array.Copy(draw.source,draw.matrices,draw.count);draw.source=null;
            }
            if(draw.count>0){var bounds=draw.parameters.worldBounds;bounds.Encapsulate(parameters.worldBounds);draw.parameters.worldBounds=bounds;}
            Array.Copy(matrices,0,draw.matrices,draw.count,matrices.Length);draw.count+=matrices.Length;
        }
        void LateUpdate()
        {
            if(frame!=Time.frameCount){LastSourceBatches=0;LastDrawCalls=0;return;}
            try
            {
                for(int i=0;i<used;i++)
                {
                    var draw=pool[i];
                    Graphics.RenderMeshInstanced(draw.parameters,draw.mesh,draw.submesh,draw.source??draw.matrices,draw.count);
                    LastDrawCalls++;
                }
            }
            finally{Clear();}
        }
        void Clear()
        {
            active.Clear();
            for(int i=0;i<used;i++)
            {pool[i].parameters=default;pool[i].mesh=null;pool[i].source=null;pool[i].count=0;}
            used=0;
        }
        void OnDisable(){Clear();}
        void OnDestroy(){if(instance==this)instance=null;}
    }
}
