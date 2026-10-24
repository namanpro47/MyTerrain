using UnityEngine;
using UnityEngine.UI;

namespace Terra.UI
{
    // Original vector miniatures: common perspective, light and land edge. No reference-game art.
    public sealed class TreatmentIllustration : MaskableGraphic
    {
        public int Kind;
        protected override void Awake(){base.Awake();raycastTarget=false;}
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Color soil=new Color(.43f,.32f,.20f),edge=new Color(.28f,.24f,.16f),grass=new Color(.61f,.72f,.38f),leaf=new Color(.29f,.50f,.29f),light=new Color(.84f,.81f,.45f);
            Vector2 P(float x,float y)=>new Vector2(rectTransform.rect.xMin+x*rectTransform.rect.width,rectTransform.rect.yMin+y*rectTransform.rect.height);
            void Poly(Color color,params Vector2[] points){int start=vh.currentVertCount;foreach(var p in points)vh.AddVert(P(p.x,p.y),color,Vector2.zero);for(int i=1;i<points.Length-1;i++)vh.AddTriangle(start,start+i,start+i+1);}
            Vector2 Land(float u,float v)=>new Vector2(.08f+.43f*u+.40f*v,.35f-.18f*u+.24f*v);
            void Plot(float u,float v,float w,float h,Color c)=>Poly(c,Land(u,v),Land(u+w,v),Land(u+w,v+h),Land(u,v+h));
            Poly(edge,new Vector2(.08f,.35f),new Vector2(.51f,.17f),new Vector2(.51f,.08f),new Vector2(.08f,.26f));
            Poly(soil,new Vector2(.51f,.17f),new Vector2(.91f,.41f),new Vector2(.91f,.32f),new Vector2(.51f,.08f));
            Plot(0,0,1,1,Kind==0?soil:grass);
            if(Kind==0){for(int row=0;row<4;row++){Plot(.08f+row*.23f,.05f,.09f,.90f,leaf);for(int j=0;j<5;j++){var p=Land(.12f+row*.23f,.10f+j*.18f);Poly(light,p,p+new Vector2(-.025f,.08f),p+new Vector2(.015f,.06f),p+new Vector2(.025f,.10f),p+new Vector2(.03f,.01f));}}}
            else if(Kind==1){for(int i=0;i<16;i++){var p=Land(.08f+(i%4)*.25f,.08f+(i/4)*.26f);Poly(leaf,p,p+new Vector2(-.028f,.10f),p+new Vector2(.004f,.055f),p+new Vector2(.035f,.12f),p+new Vector2(.025f,.006f));if(i%3==0)Poly(i%2==0?light:new Color(.93f,.76f,.67f),p+new Vector2(.014f,.09f),p+new Vector2(.028f,.105f),p+new Vector2(.043f,.09f),p+new Vector2(.028f,.075f));}}
            else{Plot(0,.39f,1,.25f,new Color(.24f,.40f,.29f));Plot(0,.45f,1,.10f,new Color(.36f,.65f,.64f));for(int i=0;i<7;i++){var p=Land(.1f+i*.13f,.78f);Poly(leaf,p,p+new Vector2(-.02f,.15f),p+new Vector2(.008f,.09f),p+new Vector2(.03f,.17f),p+new Vector2(.03f,.02f));}}
        }
    }
}
