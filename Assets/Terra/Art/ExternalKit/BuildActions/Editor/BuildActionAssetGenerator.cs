using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terra.Art;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

// Original metric geometry. Icons are rendered from the saved prefabs, not a second illustration.
public static class BuildActionAssetGenerator
{
    const string Root = "Assets/Terra/Art/ExternalKit/BuildActions";
    static readonly Dictionary<string, Material> Materials = new Dictionary<string, Material>();
    static Transform model;
    static int checks;
    sealed class Spec
    {
        public string id, title, section, notes;
        public float width, depth, clearance;
        public bool landscape;
        public Action build;
        public Spec(string id, string title, float width, float depth, string section, string notes, Action build, bool landscape = false, float clearance = 0)
        { this.id=id; this.title=title; this.width=width; this.depth=depth; this.section=section; this.notes=notes; this.build=build; this.landscape=landscape; this.clearance=clearance; }
    }

    static Spec[] Specs() => new[] {
        new Spec("cultivation", "Traditional farmland", 40, 40, "A", "40 x 40 m illustrative field sample; clip/repeat to actual site. Crop rows 0.75 m apart.", () => Field(0), true),
        new Spec("cover-crop", "Cover crop", 40, 40, "B", "40 x 40 m seasonal cover sample; no permanent loss of cropped area implied.", () => Field(1), true),
        new Spec("conservation-rotation", "Conservation rotation", 40, 40, "B", "40 x 40 m illustrative crop strips; depicts a practice, not simultaneous annual yields.", () => Field(2), true),
        new Spec("living-margin", "Living margin", 40, 40, "B", "40 x 40 m sample with a 10 m reserved vegetated strip (25 percent); clip to site.", () => Field(3), true),
        new Spec("ground-cover-restoration", "Ground-cover restoration", 40, 40, "C", "40 x 40 m native meadow sample, scattered shrubs; does not prescribe woodland everywhere.", () => Meadow(false), true),
        new Spec("vegetated-swale", "Vegetated swale", 32, 10, "D", "30 m long, 2 m flat bottom, 0.6 m illustrative depth, 4H:1V vegetated sides; separate terrain excavation needed.", Swale, true),
        new Spec("low-density-housing", "Low-density housing", 26, 32, "Wider catalog", "One 12 x 9 m house, 3 m eaves, 5.8 m ridge; 26 x 32 m lot includes access and yard.", HouseLot),
        new Spec("high-density-housing", "High-density housing", 44, 38, "Wider catalog", "32 x 18 m apartment block, four 3 m storeys, 13.2 m parapet; 44 x 38 m site.", Apartments),
        new Spec("public-park", "Public park", 48, 40, "Wider catalog", "48 x 40 m park; 8 x 6 m shelter, 2 m paths, benches and planted areas.", Park),
        new Spec("agricultural-storage", "Agricultural storage", 48, 42, "Wider catalog", "30 x 18 m agricultural shed, 6 m eaves, 8.5 m ridge; two 5 m diameter grain bins. Not a distribution warehouse.", Storage),
        new Spec("data-center", "Data center", 80, 64, "Wider catalog", "54 x 32 m single-storey data hall, 7 m roof; cooling and utility yard. No computing/electrical capacity inferred.", DataCenter),
        new Spec("village-center", "Village center", 64, 52, "Wider catalog", "Three two-storey shops with 3.2 m storeys, public square and access; representative village cluster.", Village),
        new Spec("non-renewable-power", "Gas power facility", 72, 60, "Wider catalog", "Authored small gas-engine facility: 32 x 20 x 9 m hall, 22 m exhaust stacks; no capacity or emissions claim.", GasPower),
        new Spec("solar", "Solar array", 64, 48, "Wider catalog", "60 fixed-tilt tables, each 4.4 x 4.2 m, tilted 25 degrees; 7 m row spacing. No MW rating assigned.", Solar),
        new Spec("wind", "Wind turbine", 150, 150, "Wider catalog", "100 m hub, 130 m rotor, 165 m maximum tip; 16 x 16 m pad. 65 m rotor envelope is not a legal setback or wind-farm spacing rule.", Wind, false, 65),
        new Spec("stewardship-agreement", "Conservation / stewardship", 48, 40, "Wider catalog", "48 x 40 m landscape sample with restoration and a modest interpretation sign; no invented cultural motifs or assumed public access.", () => Meadow(true), true),
        new Spec("wastewater-upgrade", "Wastewater treatment", 64, 52, "Wider catalog", "Two 12 m diameter treatment tanks, 20 x 10 m process building and two 8 x 16 m basins; no flow/capacity inferred.", Wastewater)
    };

    [MenuItem("Terra/Build Action Art/Generate and verify")]
    public static void Generate()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        var originalSetup = EditorSceneManager.GetSceneManagerSetup();
        try { GenerateInternal(); }
        finally { if (!Application.isBatchMode) EditorSceneManager.RestoreSceneManagerSetup(originalSetup); }
    }

    static void GenerateInternal()
    {
        foreach (string folder in new[]{"Meshes","Materials","Prefabs","Icons","Preview"}) Directory.CreateDirectory(Root+"/"+folder);
        Directory.CreateDirectory("docs/evidence/build-action-art");
        AssetDatabase.Refresh();
        checks=0; Materials.Clear(); Palette();
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var specs=Specs(); var entries=new List<BuildActionCatalog.Entry>();
        foreach(var spec in specs)
        {
            var go=new GameObject(spec.title); model=go.transform;
            spec.build();
            Combine(go,spec.id);
            var bounds=BoundsOf(go);
            Verify(bounds.min.y>=-.011f,spec.id+": geometry below ground");
            Verify(bounds.min.x>=-spec.width/2-.02f && bounds.max.x<=spec.width/2+.02f && bounds.min.z>=-spec.depth/2-.02f && bounds.max.z<=spec.depth/2+.02f,spec.id+": site does not contain mesh");
            int triangles=go.GetComponentsInChildren<MeshFilter>().Sum(f=>f.sharedMesh.triangles.Length/3);
            Verify(triangles>0 && triangles<40000,spec.id+": triangle budget");
            Verify(go.transform.localScale==Vector3.one && go.transform.position==Vector3.zero,spec.id+": unit root");
            var prefab=PrefabUtility.SaveAsPrefabAsset(go,Root+"/Prefabs/"+spec.id+".prefab");
            entries.Add(new BuildActionCatalog.Entry{actionId=spec.id,title=spec.title,prdSection=spec.section,dimensionNotes=spec.notes,prefab=prefab,siteSizeMetres=new Vector2(spec.width,spec.depth),meshBoundsMetres=bounds,clearanceRadiusMetres=spec.clearance,landscapeSample=spec.landscape,triangles=triangles});
            Object.DestroyImmediate(go);
        }
        AssetDatabase.SaveAssets();
        ExportGeometry(entries);
        RenderIcons(entries);
        var catalog=AssetDatabase.LoadAssetAtPath<BuildActionCatalog>(Root+"/BuildActionCatalog.asset");
        if(catalog==null){catalog=ScriptableObject.CreateInstance<BuildActionCatalog>();AssetDatabase.CreateAsset(catalog,Root+"/BuildActionCatalog.asset");}
        catalog.entries=entries.ToArray();EditorUtility.SetDirty(catalog);AssetDatabase.SaveAssets();
        BuildGallery(entries);
        Verify(entries.Count==17 && entries.Select(e=>e.actionId).Distinct().Count()==17,"PRD catalog completeness");
        foreach(var entry in entries) Verify(entry.icon!=null && entry.prefab!=null,"serialized asset references: "+entry.actionId);
        File.WriteAllText("docs/evidence/build-action-art/catalog.json",JsonUtility.ToJson(new ExportCatalog{entries=entries.Select(e=>new ExportEntry{actionId=e.actionId,title=e.title,siteSizeMetres=e.siteSizeMetres,meshBoundsMetres=e.meshBoundsMetres,clearanceRadiusMetres=e.clearanceRadiusMetres,landscapeSample=e.landscapeSample,triangles=e.triangles,dimensionNotes=e.dimensionNotes,prefab=AssetDatabase.GetAssetPath(e.prefab),icon=AssetDatabase.GetAssetPath(e.icon)}).ToArray()},true));
        File.WriteAllText("docs/evidence/build-action-art/verification.json", "{\"checksPassed\":"+checks+",\"assetCount\":17,\"unity\":\""+Application.unityVersion+"\",\"units\":\"metres\",\"icons\":\"512x512 RGBA rendered from prefabs\",\"mainSceneChanged\":false,\"browserVerified\":false}");
        AssetDatabase.SaveAssets(); Debug.Log("BUILD_ACTION_ART_PASS "+checks+" checks; 17 prefabs and icons");
    }

    static void Verify(bool condition,string message){if(!condition)throw new InvalidOperationException(message);checks++;}

    [MenuItem("Terra/Build Action Art/Verify saved catalog")]
    public static void VerifySavedCatalog()
    {
        checks=0;
        var catalog=AssetDatabase.LoadAssetAtPath<BuildActionCatalog>(Root+"/BuildActionCatalog.asset");
        Verify(catalog!=null && catalog.entries.Length==17,"saved catalog completeness");
        var scenario=JsonUtility.FromJson<Terra.Contracts.Scenario>(File.ReadAllText("worlds/meadowbrook-v1/scenario.json"));
        foreach(var treatment in scenario.treatments) Verify(catalog.Find(treatment.id)?.prefab!=null,"existing treatment alias: "+treatment.id);
        Verify(catalog.Find("unknown-action")==null,"unknown action has no misleading fallback");
        var wind=catalog.Find("wind");
        Verify(Mathf.Abs(wind.meshBoundsMetres.max.y-165)<.01f && wind.clearanceRadiusMetres==65,"wind physical dimensions");
        foreach(var entry in catalog.entries)
        {
            Verify(entry.prefab!=null && entry.icon!=null,"saved references: "+entry.actionId);
            Verify(entry.prefab.transform.localScale==Vector3.one,"saved metre scale: "+entry.actionId);
        }
        File.WriteAllText("docs/evidence/build-action-art/catalog-lookup-verification.json","{\"checksPassed\":"+checks+",\"existingScenarioAliases\":4,\"windTipHeightMetres\":165}");
        Debug.Log("BUILD_ACTION_CATALOG_PASS "+checks+" checks");
    }
    static Material Make(string name,string hex)
    {
        string path=Root+"/Materials/"+name+".mat";
        var m=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
        ColorUtility.TryParseHtmlString("#"+hex,out var color);m.name=name;m.color=color;m.SetFloat("_Smoothness",.18f);m.enableInstancing=true;EditorUtility.SetDirty(m);Materials[name]=m;return m;
    }
    static void Palette()
    {
        Make("plaster","E8DCC2");Make("roof","52696B");Make("clay","B56E4D");Make("trim","F6EDD9");Make("glass","487A87");
        Make("wood","805D3E");Make("soil","806645");Make("grass","8FA668");Make("leaf","4C7553");Make("leafLight","6D9057");
        Make("crop","C9AD60");Make("path","C0B69A");Make("asphalt","737C79");Make("steel","A2B0AE");Make("water","4D959B");
        Make("solar","294D70");Make("flower","C8A5AD");Make("dark","354B4C");
    }
    static GameObject Part(PrimitiveType type,string material,Vector3 p,Vector3 s,Quaternion? rotation=null)
    {
        var go=GameObject.CreatePrimitive(type);go.name=material;go.transform.SetParent(model,false);go.transform.localPosition=p;go.transform.localScale=s;
        if(rotation.HasValue)go.transform.localRotation=rotation.Value;
        Object.DestroyImmediate(go.GetComponent<Collider>());go.GetComponent<MeshRenderer>().sharedMaterial=Materials[material];return go;
    }
    static void Box(string mat,float x,float y,float z,float w,float h,float d) => Part(PrimitiveType.Cube,mat,new Vector3(x,y,z),new Vector3(w,h,d));
    static void Cylinder(string mat,float x,float y,float z,float radius,float height) => Part(PrimitiveType.Cylinder,mat,new Vector3(x,y,z),new Vector3(radius*2,height/2,radius*2));
    static void Orb(string mat,float x,float y,float z,float w,float h,float d) => Part(PrimitiveType.Sphere,mat,new Vector3(x,y,z),new Vector3(w,h,d));
    static void Pad(float w,float d,string mat="grass") => Box(mat,0,.06f,0,w,.12f,d);
    static void Beam(string mat,Vector3 a,Vector3 b,float width,float depth)
    { Part(PrimitiveType.Cube,mat,(a+b)/2,new Vector3(width,(b-a).magnitude,depth),Quaternion.FromToRotation(Vector3.up,b-a)); }
    static void MeshPart(string mat,Vector3[] vertices,int[] triangles)
    {
        var mesh=new Mesh{name="authored geometry"};mesh.vertices=vertices;mesh.triangles=triangles;mesh.RecalculateNormals();mesh.RecalculateBounds();
        var go=new GameObject(mat,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(model,false);go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<MeshRenderer>().sharedMaterial=Materials[mat];
    }
    static void Gable(float x,float z,float w,float d,float eave,float rise,string wall="plaster",string roof="roof")
    {
        Box(wall,x,eave/2+.12f,z,w,eave,d);
        float a=x-w/2,b=x+w/2,c=z-d/2,e=z+d/2,y=eave+.12f;
        MeshPart(wall,new[]{new Vector3(a,y,c),new Vector3(b,y,c),new Vector3(x,y+rise,c),new Vector3(a,y,e),new Vector3(b,y,e),new Vector3(x,y+rise,e)},new[]{0,2,1,3,4,5});
        float slope=Mathf.Atan2(rise,w/2)*Mathf.Rad2Deg, length=Mathf.Sqrt(w*w/4+rise*rise)+.55f;
        Part(PrimitiveType.Cube,roof,new Vector3(x-w/4,y+rise/2+.08f,z),new Vector3(length,.24f,d+.8f),Quaternion.Euler(0,0,slope));
        Part(PrimitiveType.Cube,roof,new Vector3(x+w/4,y+rise/2+.08f,z),new Vector3(length,.24f,d+.8f),Quaternion.Euler(0,0,-slope));
        Box("trim",x,y-.1f,e+.07f,w,.22f,.18f);Box("trim",x,y-.1f,c-.07f,w,.22f,.18f);
    }
    static void Window(float x,float y,float z,float w=1.15f,float h=1.35f)
    {Box("trim",x,y,z,w+.2f,h+.2f,.18f);Box("glass",x,y,z+.11f,w,h,.07f);Box("trim",x,y,z+.16f,.07f,h,.05f);}
    static void Door(float x,float z,float height=2.1f,float width=1)
    {Box("trim",x,height/2+.12f,z,width+.22f,height+.1f,.2f);Box("wood",x,height/2+.12f,z+.12f,width,height,.08f);}
    static void Tree(float x,float z,float height=7)
    {
        Cylinder("wood",x,height*.27f+.12f,z,.17f,height*.54f);
        Orb("leaf",x,height*.67f,z,height*.6f,height*.66f,height*.57f);
        Orb("leafLight",x-height*.14f,height*.80f,z+.15f,height*.41f,height*.4f,height*.42f);
    }
    static void Shrub(float x,float z,float size=1.4f) => Orb("leafLight",x,.12f+size*.4f,z,size,size*.8f,size*.85f);
    static void Bench(float x,float z)
    {Box("wood",x,.65f,z,1.8f,.13f,.55f);Box("wood",x,1.05f,z-.25f,1.8f,.65f,.12f);Box("dark",x-.65f,.35f,z,.12f,.6f,.4f);Box("dark",x+.65f,.35f,z,.12f,.6f,.4f);}
    static void Field(int kind)
    {
        Pad(40,40,"soil");
        if(kind==1){Box("leafLight",0,.17f,0,39.8f,.1f,39.8f);for(int x=-18;x<=18;x+=3)for(int z=-18;z<=18;z+=4)Box("leaf",x,.38f,z,1.5f,.4f,2.2f);return;}
        float maxX=kind==3?9:19;
        for(float x=-19;x<maxX;x+=.75f){string mat=kind==2 && x>0?"leafLight":"crop";Box(mat,x,.55f,0,.28f,.85f,38);}
        if(kind==3){Box("grass",15,.16f,0,10,.1f,40);for(int z=-18;z<=18;z+=3){Shrub(13,z);Box("flower",17,.6f,z,.8f,.6f,.8f);}}
        if(kind==2)Box("soil",0,.22f,0,1.3f,.2f,40);
    }
    static void Meadow(bool stewardship)
    {
        Pad(stewardship?48:40,40);
        for(int i=0;i<36;i++){float x=-17+(i*7%35),z=-17+(i*13%35);Shrub(x,z,.8f+(i%3)*.4f);if(i%3==0)Box("flower",x+.6f,.65f,z,.45f,.5f,.45f);}
        if(stewardship){Tree(-19,-12,8);Tree(18,-10,6);Box("wood",0,1.1f,16,.12f,2,.12f);Box("plaster",0,1.8f,16,2,.9f,.15f);Box("leaf",0,1.8f,16.09f,1.5f,.12f,.04f);}
    }
    static void Swale()
    {
        // Cross section: bottom 2 m, depth .6 m, side run 2.4 m (4:1). Above-ground module requires terrain inset.
        float[] z={-5,-3.4f,-1,1,3.4f,5};float[] y={.72f,.72f,.12f,.12f,.72f,.72f};
        for(int i=0;i<5;i++) MeshPart(i==2?"leafLight":"grass",new[]{new Vector3(-16,y[i],z[i]),new Vector3(16,y[i],z[i]),new Vector3(16,y[i+1],z[i+1]),new Vector3(-16,y[i+1],z[i+1])},new[]{0,2,1,0,3,2});
        for(int x=-13;x<=13;x+=4){Box("path",x,.22f,0,.28f,.2f,2.2f);for(int side=-1;side<=1;side+=2)Box("leaf",x,.9f,side*4.1f,.3f,.8f,.3f);}
    }
    static void HouseLot()
    {
        Pad(26,32);Box("path",6,.15f,8,4,.1f,16);Gable(-2,-3,12,9,3,2.8f);
        Window(-6,1.9f,1.6f);Window(2,1.9f,1.6f);Door(-2,1.65f);
        Box("path",-2,.22f,3,3,.22f,2);Box("roof",-2,2.75f,2.7f,3.5f,.18f,2.5f);
        for(int side=-1;side<=1;side+=2)Box("trim",-2+side*1.4f,1.45f,3.6f,.13f,2.6f,.13f);
        Box("clay",1,4.8f,-4,.8f,2.7f,.8f);Tree(-9,10,6);Shrub(-8,2);Shrub(4,2);
    }
    static void Apartments()
    {
        Pad(44,38);Box("path",0,.15f,0,38,.12f,25);Box("plaster",0,6.12f,-3,32,12,18);
        for(int floor=0;floor<4;floor++){Box("clay",0,.9f+floor*3,6.1f,32,.4f,.16f);for(int col=0;col<8;col++)Window(-13.4f+col*3.8f,1.9f+floor*3,6.15f,1.65f,1.5f);}
        Box("roof",0,12.25f,-3,32.4f,.3f,18.4f);
        Box("trim",0,12.75f,6,32,.9f,.3f);Box("trim",0,12.75f,-12,32,.9f,.3f);
        Box("trim",-15.85f,12.75f,-3,.3f,.9f,18);Box("trim",15.85f,12.75f,-3,.3f,.9f,18);
        Door(0,6.4f,2.4f,2);Box("roof",0,2.9f,7.4f,4,.2f,2.6f);
        for(int i=-1;i<=1;i++){Box("steel",i*8,12.8f,-3,3,1,2);Tree(i*14,14,5);}
    }
    static void Park()
    {
        Pad(48,40);Box("path",0,.15f,0,2,.12f,40);Box("path",0,.15f,3,48,.12f,2);
        Gable(-10,-8,8,6,2.8f,1.5f,"wood");Box("glass",-10,1.5f,-4.9f,6.5f,1.7f,.1f);
        foreach(var p in new[]{new Vector2(-18,12),new Vector2(16,12),new Vector2(17,-12),new Vector2(-20,-11)})Tree(p.x,p.y,7);
        Bench(-5,6);Bench(6,6);Bench(7,-5);Cylinder("path",11,.2f,-8,4,.2f);Cylinder("grass",11,.35f,-8,3.5f,.2f);
        for(int i=0;i<6;i++)Shrub(-19+i*3,16);
    }
    static void Storage()
    {
        Pad(48,42);Box("path",0,.15f,1,45,.12f,37);Gable(-4,-6,30,18,6,2.5f,"clay");
        for(int i=0;i<3;i++){Box("trim",-14+i*10,2.6f,3.12f,7,5.1f,.12f);Box("steel",-14+i*10,2.6f,3.23f,6.6f,4.8f,.1f);for(int j=0;j<5;j++)Box("dark",-14+i*10,.7f+j*.8f,3.3f,6.6f,.05f,.06f);}
        for(int i=0;i<2;i++){Cylinder("steel",17,3.7f,-10+i*7,2.5f,7);Cylinder("roof",17,7.25f,-10+i*7,2.65f,.2f);}
        for(int i=0;i<6;i++)Box("trim",-18+i*5.6f,3.1f,-15.15f,.13f,6,.13f);
    }
    static void FlatHall(float x,float z,float w,float d,float h)
    {Box("plaster",x,h/2+.12f,z,w,h,d);Box("roof",x,h+.2f,z,w+.35f,.3f,d+.35f);for(float a=x-w/2+2;a<x+w/2;a+=5)Box("steel",a,h*.5f,z+d/2+.08f,.15f,h,.14f);}
    static void DataCenter()
    {
        Pad(80,64,"grass");Box("asphalt",0,.15f,0,74,.12f,58);FlatHall(-5,-5,54,32,7);
        for(int row=0;row<2;row++)for(int i=0;i<6;i++){Box("steel",-26+i*8,8.05f,-13+row*14,4.5f,1.4f,4);Cylinder("dark",-26+i*8,8.8f,-13+row*14,1.25f,.12f);}
        for(int i=0;i<4;i++){Box("steel",30,2.3f,-20+i*11,5,4,7);Box("roof",30,4.5f,-20+i*11,5.2f,.2f,7.2f);}
        Box("glass",-20,2.5f,11.12f,17,2,.15f);Door(-6,11.2f);Box("path",-8,.2f,22,43,.15f,3);
    }
    static void Village()
    {
        Pad(64,52);Box("path",0,.17f,3,50,.17f,38);
        for(int i=0;i<3;i++){float x=-20+i*20;Gable(x,-11,16,12,6.4f,2,"plaster",i==1?"clay":"roof");for(int j=0;j<3;j++)Window(x-5+j*5,4.9f,-4.85f,2,1.5f);Box("glass",x,1.7f,-4.84f,13,2.7f,.14f);Door(x,-4.65f);Box(i==1?"leaf":"clay",x,3.2f,-3.6f,15,.3f,2.5f);}
        Cylinder("leafLight",0,.42f,10,4,.5f);Tree(0,10,6);Bench(-7,11);Bench(7,11);
        Tree(-25,16,7);Tree(25,16,7);Box("asphalt",0,.15f,23,64,.12f,5);
    }
    static void GasPower()
    {
        Pad(72,60);Box("path",0,.15f,0,66,.12f,54);FlatHall(-9,-5,32,20,9);
        for(int i=0;i<3;i++){Cylinder("steel",-19+i*10,11.12f,-12,1.05f,22);Cylinder("dark",-19+i*10,22.2f,-12,1.2f,.3f);Box("steel",-19+i*10,2,11,7,3.6f,7);}
        for(int i=0;i<3;i++){Box("steel",23,2.5f,-17+i*13,8,4.5f,8);for(int j=0;j<4;j++)Box("dark",20+j*2,5,-17+i*13,.3f,.4f,6);}
        Door(-9,5.15f,3,2);Box("glass",-18,3,5.15f,8,2,.1f);
    }
    static void Solar()
    {
        Pad(64,48);Box("path",0,.15f,0,3,.12f,48);
        for(int r=0;r<6;r++)for(int c=0;c<10;c++)
        {
            float x=-27+c*6,z=-18+r*7;
            Box("steel",x,1.05f,z,.15f,1.8f,.15f);
            Part(PrimitiveType.Cube,"steel",new Vector3(x,1.9f,z),new Vector3(4.4f,.14f,4.2f),Quaternion.Euler(25,0,0));
            for(int j=0;j<2;j++)for(int k=0;k<2;k++)
            {var offset=Quaternion.Euler(25,0,0)*new Vector3((j-.5f)*2.16f,.10f,(k-.5f)*2.06f);Part(PrimitiveType.Cube,"solar",new Vector3(x,1.9f,z)+offset,new Vector3(2.08f,.06f,1.98f),Quaternion.Euler(25,0,0));}
        }
        Box("steel",-4,.95f,22,2,1.6f,1.5f);
    }
    static void Wind()
    {
        Box("path",0,.12f,0,16,.24f,16);Box("path",0,.10f,40,5,.2f,70);
        // Tapered 100 m tower; ring mesh avoids scaling a short decorative turbine to fit a lot.
        var verts=new List<Vector3>();var tris=new List<int>();int n=20;
        for(int level=0;level<2;level++)for(int i=0;i<n;i++){float a=i*Mathf.PI*2/n,r=level==0?2.4f:1.35f;verts.Add(new Vector3(Mathf.Cos(a)*r,level==0?.24f:100,Mathf.Sin(a)*r));}
        for(int i=0;i<n;i++){int j=(i+1)%n;tris.AddRange(new[]{i,i+n,j,j,j+n,i+n});}
        MeshPart("trim",verts.ToArray(),tris.ToArray());Box("plaster",0,100,-1.8f,4.5f,4.2f,9);Orb("trim",0,100,3.4f,4,4,4);
        for(int b=0;b<3;b++)
        {
            var q=Quaternion.Euler(0,0,b*120);var center=new Vector3(0,100,4.8f);
            var v=new[]{new Vector3(-.8f,2,0),new Vector3(-2.5f,12,0),new Vector3(-1.3f,43,0),new Vector3(.1f,65,0),new Vector3(1.2f,42,0),new Vector3(3.2f,12,0),new Vector3(.8f,2,0)};
            var all=new Vector3[14];for(int i=0;i<7;i++){all[i]=center+q*(v[i]+Vector3.back*.22f);all[i+7]=center+q*(v[i]+Vector3.forward*.22f);}
            var ti=new List<int>();for(int i=1;i<6;i++){ti.AddRange(new[]{0,i+1,i,7,7+i,8+i});}for(int i=0;i<7;i++){int j=(i+1)%7;ti.AddRange(new[]{i,j,i+7,j,j+7,i+7});}MeshPart("trim",all,ti.ToArray());
        }
        Door(0,2.45f,2.3f,.95f);
    }
    static void Wastewater()
    {
        Pad(64,52);Box("path",0,.15f,0,59,.12f,47);FlatHall(-15,-14,20,10,4.5f);Door(-15,-8.85f);
        for(int i=0;i<2;i++){float x=-18+i*18;Cylinder("steel",x,1.5f,7,6,2.7f);Cylinder("water",x,2.88f,7,5.5f,.08f);Box("trim",x,3.1f,7,12,.2f,.8f);Cylinder("roof",x,3.5f,7,.8f,1);}
        for(int i=0;i<2;i++){float z=-13+i*23;Box("steel",21,1.2f,z,8,2,16);Box("water",21,2.25f,z,7,.12f,15);for(int j=0;j<3;j++)Box("trim",21,2.6f,z-5+j*5,8,.16f,.4f);}
        Beam("steel",new Vector3(-15,1,-8),new Vector3(-15,1,0),.4f,.4f);
    }

    static Bounds BoundsOf(GameObject go)
    {var rs=go.GetComponentsInChildren<Renderer>();var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);return b;}
    static void Combine(GameObject go,string id)
    {
        var filters=go.GetComponentsInChildren<MeshFilter>();
        foreach(var material in Materials.Values)
        {
            var matching=filters.Where(f=>f.GetComponent<Renderer>().sharedMaterial==material).ToArray();if(matching.Length==0)continue;
            var mesh=new Mesh{name=id+"-"+material.name,indexFormat=IndexFormat.UInt32};
            mesh.CombineMeshes(matching.Select(f=>new CombineInstance{mesh=f.sharedMesh,transform=f.transform.localToWorldMatrix}).ToArray());
            string path=Root+"/Meshes/"+id+"-"+material.name+".asset";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(existing==null)AssetDatabase.CreateAsset(mesh,path);else{EditorUtility.CopySerialized(mesh,existing);Object.DestroyImmediate(mesh);mesh=existing;}
            var part=new GameObject(material.name,typeof(MeshFilter),typeof(MeshRenderer));part.transform.SetParent(go.transform,false);part.GetComponent<MeshFilter>().sharedMesh=mesh;part.GetComponent<MeshRenderer>().sharedMaterial=material;
            Verify(mesh.vertices.All(v=>float.IsFinite(v.x)&&float.IsFinite(v.y)&&float.IsFinite(v.z)),"finite mesh: "+id);
        }
        foreach(var filter in filters)Object.DestroyImmediate(filter.gameObject);
    }

    static void RenderIcons(List<BuildActionCatalog.Entry> entries)
    {
        // A temporary built-in studio avoids depending on the main scene or URP camera stack.
        // Production prefabs keep URP materials. Copies below are used only during icon rendering.
        var pipeline=GraphicsSettings.defaultRenderPipeline;var quality=QualitySettings.renderPipeline;
        var previousActive=RenderTexture.active;
        var shadowDistance=QualitySettings.shadowDistance;
        var studio=new GameObject("Icon studio");var studioMaterials=new List<Material>();
        try
        {
            GraphicsSettings.defaultRenderPipeline=null;QualitySettings.renderPipeline=null;
            RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.7f,.73f,.77f);
            RenderSettings.fog=false;
            QualitySettings.shadowDistance=600;
            var key=new GameObject("Key",typeof(Light));key.transform.SetParent(studio.transform);key.transform.rotation=Quaternion.Euler(45,-35,0);var light=key.GetComponent<Light>();light.type=LightType.Directional;light.intensity=1.1f;light.shadows=LightShadows.Soft;light.shadowStrength=.35f;light.shadowBias=.03f;
            var camGo=new GameObject("Icon camera",typeof(Camera));camGo.transform.SetParent(studio.transform);var camera=camGo.GetComponent<Camera>();camera.orthographic=true;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;camera.allowHDR=false;camera.allowMSAA=true;
            var rt=new RenderTexture(512,512,24,RenderTextureFormat.ARGB32){antiAliasing=4};rt.Create();camera.targetTexture=rt;
            foreach(var entry in entries)
            {
                var go=(GameObject)PrefabUtility.InstantiatePrefab(entry.prefab);go.transform.SetParent(studio.transform);
                foreach(var renderer in go.GetComponentsInChildren<Renderer>())
                {var original=renderer.sharedMaterial;var m=new Material(Shader.Find("Standard")){color=original.color};m.SetFloat("_Glossiness",.12f);renderer.sharedMaterial=m;studioMaterials.Add(m);}
                Frame(camera,BoundsOf(go));camera.Render();RenderTexture.active=rt;
                var tex=new Texture2D(512,512,TextureFormat.RGBA32,false);tex.ReadPixels(new Rect(0,0,512,512),0,0);tex.Apply();
                var pixels=tex.GetPixels32();int occupied=pixels.Count(p=>p.a>20);
                Verify(occupied>2500 && occupied<512*512*.9f,"icon alpha/content: "+entry.actionId);
                Verify(!Enumerable.Range(0,512).Any(i=>pixels[i].a>20||pixels[511*512+i].a>20||pixels[i*512].a>20||pixels[i*512+511].a>20),"icon framing: "+entry.actionId);
                string path=Root+"/Icons/"+entry.actionId+".png";File.WriteAllBytes(path,tex.EncodeToPNG());Object.DestroyImmediate(tex);Object.DestroyImmediate(go);
                AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=512;importer.SaveAndReimport();entry.icon=AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }
            camera.targetTexture=null;RenderTexture.active=previousActive;rt.Release();Object.DestroyImmediate(rt);
        }
        finally
        {
            RenderTexture.active=previousActive;Object.DestroyImmediate(studio);foreach(var m in studioMaterials)Object.DestroyImmediate(m);
            GraphicsSettings.defaultRenderPipeline=pipeline;QualitySettings.renderPipeline=quality;
            QualitySettings.shadowDistance=shadowDistance;
        }
    }
    static void Frame(Camera camera,Bounds bounds)
    {
        camera.transform.rotation=Quaternion.Euler(30,220,0);
        var rotation=Quaternion.Inverse(camera.transform.rotation);float horizontal=0,vertical=0;
        for(int i=0;i<8;i++){var p=Vector3.Scale(bounds.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1));var v=rotation*p;horizontal=Mathf.Max(horizontal,Mathf.Abs(v.x));vertical=Mathf.Max(vertical,Mathf.Abs(v.y));}
        camera.orthographicSize=Mathf.Max(horizontal,vertical)*1.12f;
        float distance=bounds.size.magnitude*2+20;camera.transform.position=bounds.center-camera.transform.forward*distance;camera.nearClipPlane=.1f;camera.farClipPlane=distance*3;
    }
    static void BuildGallery(List<BuildActionCatalog.Entry> entries)
    {
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.65f,.68f,.72f);
        var sun=new GameObject("Sun",typeof(Light));sun.transform.rotation=Quaternion.Euler(48,-35,0);sun.GetComponent<Light>().type=LightType.Directional;sun.GetComponent<Light>().intensity=1.1f;
        for(int i=0;i<entries.Count;i++)
        {
            var go=(GameObject)PrefabUtility.InstantiatePrefab(entries[i].prefab);go.transform.position=new Vector3((i%5)*180,0,(i/5)*180);
            var label=new GameObject(entries[i].title+" dimensions",typeof(TextMesh));label.transform.position=go.transform.position+new Vector3(-60,.2f,-85);label.transform.rotation=Quaternion.Euler(90,0,0);var text=label.GetComponent<TextMesh>();text.text=entries[i].title+"\n"+entries[i].siteSizeMetres.x+" x "+entries[i].siteSizeMetres.y+" m site";text.characterSize=2;text.fontSize=32;text.color=new Color(.18f,.27f,.22f);
        }
        var camera=new GameObject("Preview camera",typeof(Camera));camera.tag="MainCamera";camera.transform.position=new Vector3(470,800,-600);camera.transform.LookAt(new Vector3(350,0,250));camera.GetComponent<Camera>().farClipPlane=3000;camera.GetComponent<Camera>().backgroundColor=new Color(.87f,.89f,.82f);camera.GetComponent<Camera>().clearFlags=CameraClearFlags.SolidColor;
        EditorSceneManager.SaveScene(scene,Root+"/Preview/BuildActionGallery.unity");
    }

    [Serializable] sealed class ExportCatalog { public ExportEntry[] entries; }
    [Serializable] sealed class ExportEntry
    {
        public string actionId,title,dimensionNotes,prefab,icon;
        public Vector2 siteSizeMetres;
        public Bounds meshBoundsMetres;
        public float clearanceRadiusMetres;
        public bool landscapeSample;
        public int triangles;
    }
    [Serializable] sealed class GeometryExport { public ExportModel[] models; }
    [Serializable] sealed class ExportModel { public string id; public ExportPart[] parts; }
    [Serializable] sealed class ExportPart { public string name; public Color color; public Vector3[] vertices,normals; public int[] indices; }
    static void ExportGeometry(List<BuildActionCatalog.Entry> entries)
    {
        var result=new GeometryExport{models=entries.Select(e=>new ExportModel{id=e.actionId,parts=e.prefab.GetComponentsInChildren<MeshFilter>().Select(f=>new ExportPart{name=f.name,color=f.GetComponent<Renderer>().sharedMaterial.color,vertices=f.sharedMesh.vertices,normals=f.sharedMesh.normals,indices=f.sharedMesh.triangles}).ToArray()}).ToArray()};
        Directory.CreateDirectory("Builds/BuildActionArt");
        File.WriteAllText("Builds/BuildActionArt/geometry.json",JsonUtility.ToJson(result));
    }
}
