using TMPro;
using UnityEngine;

namespace Terra.Gameplay
{
    public sealed partial class WorldAtlas
    {
        GameObject communityPanel;CommunityTerrain community;
        UnityEngine.UI.RawImage communityImage;
        TextMeshProUGUI communityStatus,communitySource;
        RectTransform communityList;
        bool communityWasOpen;
        CommunityCatalog communityCatalog;CommunityPlace currentCommunity;
        int communityPage;
        RectTransform communityOptions;
        UnityEngine.UI.Button previousCommunity,nextCommunity;
        bool communityCatalogLoading;

        // Separate partial keeps the existing globe, journey and annual save path owned by the atlas.
        public void InstallCommunityBrowser(CommunityTerrain companion)
        {
            if(communityPanel!=null)return;community=companion;
            communityOptions=uiRoot.Find("Atlas options") as RectTransform;
            if(communityOptions!=null)
            {
                communityOptions.sizeDelta=new Vector2(communityOptions.sizeDelta.x,280);
                Button(communityOptions,"Your community",new Vector2(.5f,1),new Vector2(0,-207),new Vector2(208,48),OpenCommunityBrowser);
            }
            else Button(notes.transform,"Your community",new Vector2(1,1),new Vector2(-24,-22),new Vector2(180,40),OpenCommunityBrowser);
            var panel=Box(notes.transform,"Community browser",Vector2.one*.5f,Vector2.zero,new Vector2(1240,760));communityPanel=panel.gameObject;
            var background=communityPanel.AddComponent<UnityEngine.UI.Image>();background.sprite=rounded;background.type=UnityEngine.UI.Image.Type.Sliced;background.color=Navy;
            Text(panel,"YOUR PLACE, YOUR POSSIBILITIES",new Vector2(0,1),new Vector2(32,-24),new Vector2(700,25),13,Mint);
            Text(panel,"Bring your community into MyTerrain",new Vector2(0,1),new Vector2(30,-60),new Vector2(1040,52),30,Cream);
            Button(panel,"Close  ×",new Vector2(1,1),new Vector2(-28,-26),new Vector2(120,42),CloseCommunityBrowser);
            Text(panel,"1  Find your place",new Vector2(0,1),new Vector2(32,-142),new Vector2(310,32),23,Cream);
            var input=Button(panel,"Town, region or coordinates",new Vector2(0,1),new Vector2(32,-190),new Vector2(304,50),()=>{});
            input.interactable=false;
            Text(panel,"Automatic search is coming later. For now, Codex can prepare a place with the community import skill.",new Vector2(0,1),new Vector2(32,-254),new Vector2(304,94),17,Muted);
            Text(panel,"2  Review the real place",new Vector2(0,1),new Vector2(32,-368),new Vector2(310,32),23,Cream);
            Text(panel,"Try these prepared communities",new Vector2(0,1),new Vector2(32,-410),new Vector2(310,28),16,Mint);
            communityList=Box(panel,"Prepared communities",new Vector2(0,1),new Vector2(32,-452),new Vector2(304,142));
            previousCommunity=Button(panel,"Previous",new Vector2(0,1),new Vector2(32,-560),new Vector2(146,36),()=>CommunityPage(-1));
            nextCommunity=Button(panel,"Next",new Vector2(0,1),new Vector2(190,-560),new Vector2(146,36),()=>CommunityPage(1));
            previousCommunity.interactable=nextCommunity.interactable=false;
            Text(panel,"3  Shape its future",new Vector2(0,1),new Vector2(32,-618),new Vector2(310,32),23,Cream);
            Text(panel,"Terrain exploration is ready. Editable objects and annual scenarios still need authoring.",new Vector2(0,1),new Vector2(32,-661),new Vector2(304,72),16,Muted);
            var viewport=Box(panel,"Source-aligned preview",new Vector2(1,1),new Vector2(-28,-142),new Vector2(840,485));
            var raw=Box(viewport,"Community image",Vector2.one*.5f,Vector2.zero,new Vector2(640,480));communityImage=raw.gameObject.AddComponent<UnityEngine.UI.RawImage>();communityImage.raycastTarget=false;
            var fit=raw.gameObject.AddComponent<UnityEngine.UI.AspectRatioFitter>();fit.aspectMode=UnityEngine.UI.AspectRatioFitter.AspectMode.FitInParent;fit.aspectRatio=4f/3;
            communityStatus=Text(panel,"Choose a prepared community to explore its terrain.",new Vector2(1,1),new Vector2(-32,-635),new Vector2(830,54),18,Cream,TextAlignmentOptions.Center);
            var controls=Box(panel,"Terrain controls",new Vector2(1,1),new Vector2(-40,-104),new Vector2(824,40));
            Button(controls,"Terrain",new Vector2(0,.5f),Vector2.zero,new Vector2(110,36),()=>community.ShowTerrain());
            Button(controls,"Source photo",new Vector2(0,.5f),new Vector2(118,0),new Vector2(148,36),()=>community.ShowSource());
            Button(controls,"Overhead",new Vector2(0,.5f),new Vector2(274,0),new Vector2(120,36),()=>community.Overhead());
            Button(controls,"Turn left",new Vector2(0,.5f),new Vector2(402,0),new Vector2(100,36),()=>community.Rotate(-30));
            Button(controls,"Turn right",new Vector2(0,.5f),new Vector2(510,0),new Vector2(100,36),()=>community.Rotate(30));
            Button(controls,"−",new Vector2(0,.5f),new Vector2(618,0),new Vector2(48,36),()=>community.Zoom(.15f));
            Button(controls,"+",new Vector2(0,.5f),new Vector2(674,0),new Vector2(48,36),()=>community.Zoom(-.15f));
            communitySource=Text(panel,"Photo surfaces preserve the source layout. Buildings and trees are not reconstructed in 3D.",new Vector2(1,0),new Vector2(-32,28),new Vector2(830,42),15,Muted,TextAlignmentOptions.Center);
            communityPanel.SetActive(false);
            LoadCommunityCatalog();
        }
        void LoadCommunityCatalog()
        {
            if(communityCatalogLoading)return;communityCatalogLoading=true;
            StartCoroutine(community.ReadCatalog(catalog=>
            {
                communityCatalogLoading=false;communityCatalog=catalog;CommunityPage(0);
                if(communityWasOpen&&catalog.places.Length>0)LoadCommunity(catalog.places[0]);
            },message=>{communityCatalogLoading=false;communityStatus.text=message;}));
        }
        void CommunityPage(int direction)
        {
            if(communityCatalog==null)return;
            if(communityCatalog.places.Length==0){communityStatus.text="No prepared communities are included in this build.";return;}
            int pages=(communityCatalog.places.Length+1)/2;communityPage=(communityPage+direction+pages)%pages;
            previousCommunity.interactable=nextCommunity.interactable=pages>1;
            foreach(Transform child in communityList){child.gameObject.SetActive(false);Destroy(child.gameObject);}
            for(int i=0;i<2;i++)
            {
                int index=communityPage*2+i;if(index>=communityCatalog.places.Length)break;
                var place=communityCatalog.places[index];
                Button(communityList,place.name+"  →",new Vector2(0,1),new Vector2(0,-i*48),new Vector2(304,42),()=>LoadCommunity(place));
            }
        }
        public void OpenCommunityBrowser()
        {
            if(Traveling||communityPanel==null)return;
            if(communityOptions!=null)communityOptions.gameObject.SetActive(false);
            notes.SetActive(true);communityPanel.SetActive(true);communityWasOpen=true;
            if(currentCommunity!=null)LoadCommunity(currentCommunity);
            else if(communityCatalog!=null&&communityCatalog.places.Length>0)LoadCommunity(communityCatalog.places[0]);
            else if(communityCatalog==null)LoadCommunityCatalog();
            EventSystemSelect(communityPanel.GetComponentInChildren<UnityEngine.UI.Button>());
        }
        static void EventSystemSelect(UnityEngine.UI.Button button)
        {if(UnityEngine.EventSystems.EventSystem.current!=null&&button!=null)UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(button.gameObject);}
        public void CloseCommunityBrowser()
        {
            community.Cancel();community.Clear();communityPanel.SetActive(false);communityWasOpen=false;
            // Keep the atlas input gate through this frame so Enter on Close cannot also start a journey.
            StartCoroutine(CloseCommunityNotes());
            communityStatus.text="Choose a prepared community to explore its terrain.";
        }
        System.Collections.IEnumerator CloseCommunityNotes()
        {
            yield return null;if(communityWasOpen)yield break;notes.SetActive(false);
            var gear=selection.transform.Find("Atlas settings");
            if(gear!=null)EventSystemSelect(gear.GetComponent<UnityEngine.UI.Button>());
            else foreach(var button in selection.GetComponentsInChildren<UnityEngine.UI.Button>())
                if(button.name.Contains("Field notes")){EventSystemSelect(button);break;}
        }
        public void LoadCommunity(CommunityPlace place)
        {
            currentCommunity=place;
            communitySource.text=place.region+" · aerial imagery "+place.imageYear+"\nUSDA NAIP / USGS · Photo surface; buildings and trees are not 3D objects.";
            community.Load(place,communityImage,communityStatus);
        }
        public void TickCommunityBrowser()
        {
            if(communityPanel==null)return;
            // The atlas already owns Escape / I / return. Match that visibility and release GPU memory.
            if(communityWasOpen&&(!notes.activeSelf||!Visible||Traveling))CloseCommunityBrowser();
            if(!communityWasOpen)return;
            var keyboard=UnityEngine.InputSystem.Keyboard.current;if(keyboard==null)return;
            var buttons=communityPanel.GetComponentsInChildren<UnityEngine.UI.Button>();
            var events=UnityEngine.EventSystems.EventSystem.current;
            if(events==null)return;
            if(keyboard.tabKey.wasPressedThisFrame)
            {
                int current=System.Array.FindIndex(buttons,b=>b.gameObject==events.currentSelectedGameObject);
                int direction=keyboard.shiftKey.isPressed?-1:1;
                for(int n=0;n<buttons.Length;n++)
                {current=(current+direction+buttons.Length)%buttons.Length;if(buttons[current].IsInteractable()){EventSystemSelect(buttons[current]);break;}}
            }
            if(keyboard.enterKey.wasPressedThisFrame&&events.currentSelectedGameObject!=null)
            {var button=events.currentSelectedGameObject.GetComponent<UnityEngine.UI.Button>();if(button!=null&&button.transform.IsChildOf(communityPanel.transform)&&button.IsInteractable())button.onClick.Invoke();}
        }
    }
}
