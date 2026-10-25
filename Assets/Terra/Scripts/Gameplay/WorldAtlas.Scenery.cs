using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Terra.Gameplay
{
    public sealed partial class WorldAtlas
    {
        // TN-01/03/08: the world is the stage. Art here is native geometry, not a webpage.
        readonly TextMeshProUGUI[] pinLabels=new TextMeshProUGUI[3];
        readonly UnityEngine.UI.Image[] choiceUnderline=new UnityEngine.UI.Image[3];
        Transform stars;
        Material ink;
        void BuildSky(Transform panel)
        {
            var skyMaterial=new Material(Resources.Load<Shader>("Atlas/Sky"));owned.Add(skyMaterial);
            var skyMesh=new Mesh{name="Orbital backdrop"};skyMesh.vertices=new[]{new Vector3(-40,-30,10),new Vector3(40,-30,10),new Vector3(40,30,10),new Vector3(-40,30,10)};skyMesh.triangles=new[]{0,2,1,0,3,2};skyMesh.RecalculateBounds();owned.Add(skyMesh);
            var sky=new GameObject("Orbital glow",typeof(MeshFilter),typeof(MeshRenderer));sky.layer=30;sky.transform.SetParent(geometry.transform,false);sky.GetComponent<MeshFilter>().sharedMesh=skyMesh;sky.GetComponent<MeshRenderer>().sharedMaterial=skyMaterial;
            ink=new Material(Resources.Load<Shader>("Atlas/Ink"));owned.Add(ink);
            stars=new GameObject("Quiet star field").transform;stars.SetParent(geometry.transform,false);
            var random=new System.Random(9147);
            var mesh=new Mesh{name="Atlas stars"};var vertices=new Vector3[360*4];var colors=new Color[vertices.Length];var triangles=new int[360*6];
            for(int i=0;i<360;i++)
            {
                float x=(float)random.NextDouble()*24-12,y=(float)random.NextDouble()*16-8;
                float radius=i%19==0?.018f:.005f+(float)random.NextDouble()*.006f;
                var color=i%7==0?new Color(.62f,.67f,.40f):new Color(.20f,.35f,.38f);
                for(int j=0;j<4;j++){vertices[i*4+j]=new Vector3(x+(j==0||j==3?-radius:radius),y+(j<2?-radius:radius),6);colors[i*4+j]=color;}
                int n=i*6,v=i*4;triangles[n]=v;triangles[n+1]=v+2;triangles[n+2]=v+1;triangles[n+3]=v;triangles[n+4]=v+3;triangles[n+5]=v+2;
            }
            mesh.vertices=vertices;mesh.colors=colors;mesh.triangles=triangles;mesh.RecalculateBounds();owned.Add(mesh);
            stars.gameObject.layer=30;stars.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;stars.gameObject.AddComponent<MeshRenderer>().sharedMaterial=ink;
            for(int destination=1;destination<3;destination++)
            {
                var route=new GameObject("Journey arc "+destination,typeof(LineRenderer));route.layer=30;route.transform.SetParent(earth,false);
                var line=route.GetComponent<LineRenderer>();line.sharedMaterial=ink;line.useWorldSpace=false;line.widthMultiplier=.0032f;line.positionCount=101;
                line.startColor=new Color(.95f,.71f,.27f);line.endColor=new Color(.43f,.68f,.60f);
                Vector3 a=Geo(places[0]),b=Geo(places[destination]);
                for(int j=0;j<=100;j++){float t=j/100f;line.SetPosition(j,Vector3.Slerp(a,b,t)*(1.008f+.14f*Mathf.Sin(t*Mathf.PI)));}
            }
        }
        void AnimateScenery()
        {
            if(stars!=null&&!ReducedMotion)stars.localRotation=Quaternion.Euler(0,0,Mathf.Sin(Time.unscaledTime*.035f)*.25f);
        }
        void Polygon(Transform parent,Color color,params Vector2[] vertices)
        {
            var rect=Box(parent,"Illustrated terrain",Vector2.zero,Vector2.zero,new Vector2(100,100));
            var shape=rect.gameObject.AddComponent<AtlasPolygon>();shape.Points=vertices;shape.color=color;shape.raycastTarget=false;
        }
        void Tree(Transform p,float x,float y,float size,Color c)
        {
            Polygon(p,new Color(.40f,.30f,.20f),new Vector2(x-1,y-4),new Vector2(x+1,y-4),new Vector2(x+1,y+8),new Vector2(x-1,y+8));
            Polygon(p,c,new Vector2(x-size*.5f,y),new Vector2(x+size*.5f,y),new Vector2(x,y+size*1.5f));
            Polygon(p,c*.86f,new Vector2(x-size*.4f,y+size*.5f),new Vector2(x+size*.4f,y+size*.5f),new Vector2(x,y+size*1.9f));
        }
        void AddLandscapeStamp(Transform parent,int kind)
        {
            var p=Box(parent,"Landscape miniature",new Vector2(0,.5f),new Vector2(2,-45),new Vector2(104,100));p.pivot=Vector2.zero;
            // Each tiny isometric diorama is an illustration of the biome, not sourced terrain.
            Polygon(p,new Color(.12f,.23f,.22f),new Vector2(6,37),new Vector2(52,9),new Vector2(98,37),new Vector2(52,66));
            Polygon(p,new Color(.30f,.41f,.29f),new Vector2(6,45),new Vector2(52,17),new Vector2(98,45),new Vector2(52,73));
            Polygon(p,new Color(.50f,.64f,.37f),new Vector2(6,49),new Vector2(52,21),new Vector2(98,49),new Vector2(52,77));
            if(kind==0)
            {
                Polygon(p,new Color(.73f,.69f,.37f),new Vector2(17,48),new Vector2(40,34),new Vector2(56,44),new Vector2(33,58));
                Polygon(p,new Color(.82f,.77f,.45f),new Vector2(36,60),new Vector2(59,46),new Vector2(73,55),new Vector2(50,69));
                Polygon(p,new Color(.32f,.70f,.75f),new Vector2(46,25),new Vector2(53,21),new Vector2(66,45),new Vector2(65,61),new Vector2(59,64),new Vector2(60,45));
                Tree(p,23,50,12,new Color(.18f,.43f,.34f));Tree(p,40,65,12,new Color(.22f,.49f,.32f));Tree(p,78,48,12,new Color(.20f,.42f,.34f));Tree(p,72,40,10,new Color(.28f,.51f,.35f));
            }
            else if(kind==1)
            {
                Polygon(p,new Color(.19f,.59f,.68f),new Vector2(6,49),new Vector2(52,21),new Vector2(68,31),new Vector2(42,46),new Vector2(35,66));
                Polygon(p,new Color(.75f,.82f,.65f),new Vector2(35,66),new Vector2(42,46),new Vector2(68,31),new Vector2(71,34),new Vector2(47,49),new Vector2(41,70));
                Polygon(p,new Color(.72f,.67f,.42f),new Vector2(53,58),new Vector2(92,49),new Vector2(70,73));
                Tree(p,70,62,10,new Color(.26f,.46f,.32f));Tree(p,83,51,8,new Color(.24f,.43f,.31f));
            }
            else
            {
                Polygon(p,new Color(.43f,.56f,.52f),new Vector2(20,53),new Vector2(53,47),new Vector2(37,87));
                Polygon(p,new Color(.67f,.73f,.63f),new Vector2(37,87),new Vector2(53,47),new Vector2(72,62));
                Polygon(p,new Color(.90f,.93f,.83f),new Vector2(30,73),new Vector2(37,87),new Vector2(49,77),new Vector2(41,79),new Vector2(38,74),new Vector2(35,78));
                Polygon(p,new Color(.36f,.48f,.49f),new Vector2(54,58),new Vector2(91,52),new Vector2(71,82));
                Polygon(p,new Color(.86f,.91f,.83f),new Vector2(65,72),new Vector2(71,82),new Vector2(78,72),new Vector2(72,75));
                Polygon(p,new Color(.34f,.68f,.71f),new Vector2(44,27),new Vector2(50,24),new Vector2(74,45),new Vector2(69,49));
            }
        }
    }
    // Convex decorative pieces; no raycasts or gameplay state.
    public sealed class AtlasPolygon : MaskableGraphic
    {
        public Vector2[] Points;
        protected override void OnPopulateMesh(VertexHelper helper)
        {
            helper.Clear();if(Points==null||Points.Length<3)return;
            for(int i=0;i<Points.Length;i++)helper.AddVert(Points[i],color,Vector2.zero);
            for(int i=1;i<Points.Length-1;i++)helper.AddTriangle(0,i,i+1);
        }
    }
}
