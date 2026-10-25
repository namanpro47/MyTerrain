using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
namespace Terra.Gameplay
{
    public sealed partial class WorldAtlas
    {
        Sprite rounded,circle;
        void BuildGlobe()
        {
            geometry=new GameObject("Orbital scene");geometry.transform.SetParent(transform,false);geometry.transform.localPosition=new Vector3(0,10000,0);
            var cameraObject=new GameObject("Atlas camera",typeof(Camera));cameraObject.transform.SetParent(geometry.transform,false);globeCamera=cameraObject.GetComponent<Camera>();globeCamera.cullingMask=1<<30;globeCamera.clearFlags=CameraClearFlags.SolidColor;globeCamera.backgroundColor=Navy;globeCamera.fieldOfView=36;globeCamera.nearClipPlane=.01f;globeCamera.farClipPlane=30;globeCamera.depth=20;
            earth=new GameObject("Earth · NASA Blue Marble").transform;earth.SetParent(geometry.transform,false);
            var mesh=new Mesh{name="Equirectangular Earth"};int rows=96,cols=192;var vertices=new Vector3[(rows+1)*(cols+1)];var uv=new Vector2[vertices.Length];var indices=new int[rows*cols*6];
            for(int y=0;y<=rows;y++)for(int x=0;x<=cols;x++){int i=y*(cols+1)+x;uv[i]=new Vector2((float)x/cols,(float)y/rows);vertices[i]=Geo(new Vector2(-90+180f*y/rows,-180+360f*x/cols));}
            int n=0;for(int y=0;y<rows;y++)for(int x=0;x<cols;x++){int a=y*(cols+1)+x,b=a+cols+1;indices[n++]=a;indices[n++]=b;indices[n++]=a+1;indices[n++]=a+1;indices[n++]=b;indices[n++]=b+1;}
            mesh.vertices=vertices;mesh.uv=uv;mesh.triangles=indices;mesh.RecalculateNormals();mesh.RecalculateBounds();owned.Add(mesh);
            var material=new Material(Resources.Load<Shader>("Atlas/Globe")){name="Living Earth"};material.SetTexture("_MainTex",Resources.Load<Texture2D>("Atlas/Earth"));owned.Add(material);
            var surface=new GameObject("Earth surface",typeof(MeshFilter),typeof(MeshRenderer));surface.layer=30;surface.transform.SetParent(earth,false);surface.GetComponent<MeshFilter>().sharedMesh=mesh;surface.GetComponent<MeshRenderer>().sharedMaterial=material;
            var atmosphere=new Material(Resources.Load<Shader>("Atlas/Atmosphere"));owned.Add(atmosphere);var halo=new GameObject("Atmospheric rim",typeof(MeshFilter),typeof(MeshRenderer));halo.layer=30;halo.transform.SetParent(earth,false);halo.transform.localScale=Vector3.one*1.035f;halo.GetComponent<MeshFilter>().sharedMesh=mesh;halo.GetComponent<MeshRenderer>().sharedMaterial=atmosphere;
        }
        RectTransform Box(Transform parent,string title,Vector2 anchor,Vector2 position,Vector2 size)
        {var o=new GameObject(title,typeof(RectTransform));var r=o.GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=r.pivot=anchor;r.anchoredPosition=position;r.sizeDelta=size;return r;}
        TextMeshProUGUI Text(Transform parent,string text,Vector2 anchor,Vector2 position,Vector2 size,float fontSize,Color color,TextAlignmentOptions align=TextAlignmentOptions.Left)
        {var r=Box(parent,text,anchor,position,size);var label=r.gameObject.AddComponent<TextMeshProUGUI>();label.font=TMP_Settings.defaultFontAsset;label.text=text;label.fontSize=fontSize;label.color=color;label.alignment=align;label.raycastTarget=false;label.textWrappingMode=TextWrappingModes.Normal;return label;}
        UnityEngine.UI.Button Button(Transform parent,string title,Vector2 anchor,Vector2 position,Vector2 size,System.Action action,bool primary=false)
        {var r=Box(parent,title,anchor,position,size);var image=r.gameObject.AddComponent<UnityEngine.UI.Image>();image.sprite=rounded;image.type=UnityEngine.UI.Image.Type.Sliced;image.color=primary?Mint:new Color(.085f,.16f,.19f);var b=r.gameObject.AddComponent<UnityEngine.UI.Button>();b.targetGraphic=image;b.onClick.AddListener(()=>action());var c=b.colors;c.highlightedColor=new Color(1.15f,1.15f,1.15f);c.pressedColor=new Color(.7f,.8f,.75f);b.colors=c;var t=Text(r,title,new Vector2(.5f,.5f),Vector2.zero,size-new Vector2(16,4),18,primary?Navy:Cream,TextAlignmentOptions.Center);return b;}
        Sprite Shape(bool disk)
        {int size=64;var texture=new Texture2D(size,size,TextureFormat.RGBA32,false);texture.filterMode=FilterMode.Bilinear;for(int y=0;y<size;y++)for(int x=0;x<size;x++){float dx=Mathf.Max(Mathf.Abs(x-31.5f)-(disk?0:23),0),dy=Mathf.Max(Mathf.Abs(y-31.5f)-(disk?0:23),0);float a=Mathf.Clamp01((disk?31:8)-Mathf.Sqrt(dx*dx+dy*dy));texture.SetPixel(x,y,new Color(1,1,1,a));}texture.Apply();var s=Sprite.Create(texture,new Rect(0,0,size,size),Vector2.one*.5f,100,0,SpriteMeshType.FullRect,new Vector4(10,10,10,10));owned.Add(texture);owned.Add(s);return s;}
        void BuildUI()
        {
            rounded=Shape(false);circle=Shape(true);
            var go=new GameObject("MyTerrain · Atlas interface",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));go.transform.SetParent(transform,false);canvas=go.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=100;uiRoot=(RectTransform)go.transform;
            var scaler=go.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1440,900);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
            if(EventSystem.current==null){var events=new GameObject("Atlas event system",typeof(EventSystem));events.transform.SetParent(transform,false);
#if !UNITY_WEBGL || UNITY_EDITOR
                events.AddComponent<InputSystemUIInputModule>();
#endif
                EventSystem.current.sendNavigationEvents=false;
            }
            var panel=Box(uiRoot,"Selection",new Vector2(.5f,.5f),Vector2.zero,Vector2.zero);panel.anchorMin=Vector2.zero;panel.anchorMax=Vector2.one;selection=panel.gameObject.AddComponent<CanvasGroup>();
            Text(panel,"MyTerrain",new Vector2(0,1),new Vector2(42,-30),new Vector2(260,45),30,Cream);
            Text(panel,"SELECT YOUR TERRAIN",new Vector2(0,1),new Vector2(44,-80),new Vector2(400,24),11,Muted);
            Text(panel,"CHOOSE A LANDSCAPE",new Vector2(.5f,1),new Vector2(0,-42),new Vector2(420,28),15,Mint,TextAlignmentOptions.Center);
            motion=Button(panel,"Motion: full",new Vector2(1,1),new Vector2(-42,-34),new Vector2(150,40),()=>SetMotion(!ReducedMotion));
            motion.GetComponentInChildren<TextMeshProUGUI>().text=ReducedMotion?"Motion: reduced":"Motion: full";
            region=Text(panel,"",new Vector2(0,.5f),new Vector2(44,66),new Vector2(330,25),14,Mint);
            nameLabel=Text(panel,"",new Vector2(0,.5f),new Vector2(42,18),new Vector2(305,90),32,Cream);
            description=Text(panel,"",new Vector2(0,.5f),new Vector2(44,-52),new Vector2(265,70),17,Muted);
            coordinates=Text(panel,"",new Vector2(0,.5f),new Vector2(44,-135),new Vector2(310,25),14,Muted);
            Text(panel,"N",new Vector2(.5f,1),new Vector2(0,-108),new Vector2(40,25),13,Mint,TextAlignmentOptions.Center);
            for(int i=0;i<3;i++)
            {
                int index=i;var pin=Button(panel,"",new Vector2(.5f,.5f),Vector2.zero,new Vector2(42,42),()=>Select(index));pin.image.sprite=circle;pin.image.type=UnityEngine.UI.Image.Type.Simple;pin.image.color=new Color(Mint.r,Mint.g,Mint.b,.24f);pins[i]=(RectTransform)pin.transform;
                var dot=Box(pin.transform,"Pin center",new Vector2(.5f,.5f),Vector2.zero,new Vector2(12,12));var im=dot.gameObject.AddComponent<UnityEngine.UI.Image>();im.sprite=circle;im.color=Mint;im.raycastTarget=false;
                Text(pin.transform,Terra.Core.LocationProfile.At(i).Name,new Vector2(i==1?1:0,.5f),new Vector2(i==1?-52:52,0),new Vector2(210,32),15,Cream,i==1?TextAlignmentOptions.Right:TextAlignmentOptions.Left);
                choices[i]=Button(panel,"",new Vector2(.5f,0),new Vector2(-412+i*278,105),new Vector2(264,77),()=>Select(index));
                Text(choices[i].transform,$"0{i+1}  /  "+new[]{"RIVER & RIDGE","PACIFIC COAST","ALPINE VALLEY"}[i],new Vector2(0,1),new Vector2(18,-12),new Vector2(230,20),12,Mint);
                Text(choices[i].transform,Terra.Core.LocationProfile.At(i).Name,new Vector2(0,1),new Vector2(18,-37),new Vector2(230,30),21,Cream);
            }
            Button(panel,"Enter landscape  →",new Vector2(.5f,0),new Vector2(450,105),new Vector2(260,77),StartJourney,true);
            Text(panel,"Drag to rotate    ·    Scroll to zoom    ·    1–3 choose    ·    Enter to descend    ·    M motion",new Vector2(.5f,0),new Vector2(0,58),new Vector2(1000,25),16,Muted,TextAlignmentOptions.Center);
            Button(panel,"−",new Vector2(1,.5f),new Vector2(-44,-32),new Vector2(44,44),()=>Zoom(1));Button(panel,"+",new Vector2(1,.5f),new Vector2(-44,22),new Vector2(44,44),()=>Zoom(-1));Button(panel,"Focus",new Vector2(1,.5f),new Vector2(-44,-86),new Vector2(70,36),()=>Select(Selected));
            Text(panel,"NASA BLUE MARBLE  /  REAL PLACES · ILLUSTRATIVE FUTURES",new Vector2(.5f,0),new Vector2(0,16),new Vector2(1000,20),10,Muted,TextAlignmentOptions.Center);
            Button(panel,"Field notes  I",new Vector2(1,1),new Vector2(-42,-86),new Vector2(150,36),()=>notes.SetActive(true));
            var notesRect=Box(uiRoot,"Atlas field notes",Vector2.one*.5f,Vector2.zero,new Vector2(700,580));notes=notesRect.gameObject;
            var notesBackground=notes.AddComponent<UnityEngine.UI.Image>();notesBackground.sprite=rounded;notesBackground.type=UnityEngine.UI.Image.Type.Sliced;notesBackground.color=new Color(.09f,.16f,.19f);
            Text(notesRect,"MyTerrain  /  FIELD NOTES",new Vector2(0,1),new Vector2(36,-28),new Vector2(620,30),14,Mint);
            Text(notesRect,"Real places. Possible futures.",new Vector2(0,1),new Vector2(36,-80),new Vector2(620,50),30,Cream);
            Text(notesRect,"Shape ten years of a rural landscape. Plan projects, balance a public budget and see the consequences. Each place keeps its own saved run.\n\nThe terrain is sourced. Projects, ownership, cover in the new regions, USD budgets and future outcomes are educational scenarios, not local forecasts or permission to build.\n\nEarth: NASA Blue Marble Next Generation, July 2004. Reto Stöckli / NASA Earth Observatory. Colour adapted for this globe; not live imagery. Landscape sources and asset credits are in the in-game field guide. Stream normal: MatrixRex, Uber Stylized Water (MIT). Providers do not endorse MyTerrain.",new Vector2(0,1),new Vector2(36,-145),new Vector2(628,340),18,Muted);
            Button(notesRect,"Back to globe",new Vector2(.5f,0),new Vector2(0,28),new Vector2(240,48),()=>notes.SetActive(false),true);notes.SetActive(false);
            var shade=Box(uiRoot,"Arrival veil",Vector2.one*.5f,Vector2.zero,Vector2.zero);shade.anchorMin=Vector2.zero;shade.anchorMax=Vector2.one;veil=shade.gameObject.AddComponent<UnityEngine.UI.Image>();veil.color=Color.clear;veil.raycastTarget=false;
            journeyTitle=Text(uiRoot,"",Vector2.one*.5f,new Vector2(0,36),new Vector2(1100,80),48,Cream,TextAlignmentOptions.Center);
            journeyStatus=Text(uiRoot,"",Vector2.one*.5f,new Vector2(0,-43),new Vector2(900,80),18,Muted,TextAlignmentOptions.Center);
            back=Button(uiRoot,"← Back to globe",new Vector2(.5f,0),new Vector2(0,70),new Vector2(220,48),Show);
            retry=Button(uiRoot,"Try again · Enter",new Vector2(.5f,.5f),new Vector2(0,-135),new Vector2(220,48),Retry,true);
        }
    }
}
