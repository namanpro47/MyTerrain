using UnityEngine;
using UnityEngine.UI;

namespace Terra.UI
{
    // Original vector marks: no font-symbol dependency, textures or external assets.
    public sealed class HudGlyph : MaskableGraphic
    {
        public string symbol;
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();var r=rectTransform.rect;float scale=Mathf.Min(r.width,r.height)/24f;
            Vector2 P(float x,float y)=>r.center+new Vector2(x-12,12-y)*scale;
            void Line(float x,float y,float u,float v)
            {
                var a=P(x,y);var b=P(u,v);var n=new Vector2(-(b-a).y,(b-a).x).normalized*scale;
                int i=mesh.currentVertCount;mesh.AddVert(a-n,color,Vector2.zero);mesh.AddVert(a+n,color,Vector2.zero);
                mesh.AddVert(b+n,color,Vector2.zero);mesh.AddVert(b-n,color,Vector2.zero);
                mesh.AddTriangle(i,i+1,i+2);mesh.AddTriangle(i,i+2,i+3);
            }
            void Circle(float x,float y,float radius)
            {for(int i=0;i<24;i++){float a=i*Mathf.PI/12,b=(i+1)*Mathf.PI/12;Line(x+Mathf.Cos(a)*radius,y+Mathf.Sin(a)*radius,x+Mathf.Cos(b)*radius,y+Mathf.Sin(b)*radius);}}
            void Box(float x,float y,float w,float h){Line(x,y,x+w,y);Line(x+w,y,x+w,y+h);Line(x+w,y+h,x,y+h);Line(x,y+h,x,y);}
            switch(symbol)
            {
                case "coin":Circle(12,12,9);Line(12,6,12,18);Line(16,8,9,8);Line(9,8,8,12);Line(8,12,16,12);Line(16,12,15,16);Line(15,16,8,16);break;
                case "calendar":Box(3,5,18,16);Line(3,10,21,10);Line(7,2,7,7);Line(17,2,17,7);Line(8,15,11,18);Line(11,18,17,13);break;
                case "people":Circle(9,7,3);Circle(17,9,2);Line(3,21,3,17);Line(3,17,6,13);Line(6,13,12,13);Line(12,13,15,17);Line(15,17,15,21);Line(18,14,21,17);Line(21,17,21,21);break;
                case "work":Box(3,8,18,13);Box(8,3,8,5);Line(3,13,21,13);Line(12,11,12,16);break;
                case "leaf":Line(4,21,16,9);Line(4,15,4,10);Line(4,10,10,4);Line(10,4,21,3);Line(21,3,20,14);Line(20,14,14,20);Line(14,20,8,20);break;
                case "home":Line(2,11,12,3);Line(12,3,22,11);Line(5,9,5,21);Line(5,21,19,21);Line(19,21,19,9);Box(10,14,4,7);break;
                case "energy":Line(14,2,4,14);Line(4,14,11,14);Line(11,14,10,22);Line(10,22,20,10);Line(20,10,13,10);Line(13,10,14,2);break;
                case "water":Line(12,2,5,12);Line(5,12,4,16);Line(4,16,7,21);Line(7,21,17,21);Line(17,21,20,16);Line(20,16,19,12);Line(19,12,12,2);break;
                case "warning":Line(12,3,2,21);Line(2,21,22,21);Line(22,21,12,3);Line(12,9,12,14);Circle(12,18,.5f);break;
                case "undo":Line(3,10,8,5);Line(3,10,8,15);Line(3,10,15,10);Line(15,10,20,14);Line(20,14,20,20);break;
                case "redo":Line(21,10,16,5);Line(21,10,16,15);Line(21,10,9,10);Line(9,10,4,14);Line(4,14,4,20);break;
                case "map":Box(4,4,16,16);Circle(12,12,4);Line(12,1,12,6);Line(12,18,12,23);Line(1,12,6,12);Line(18,12,23,12);break;
                case "guide":Box(4,3,16,18);Line(8,3,8,21);Line(11,8,17,8);Line(11,12,17,12);break;
                case "settings":for(int i=0;i<3;i++){float y=5+i*7;Line(3,y,21,y);Circle(7+i*5,y,2);}break;
                case "more":Circle(5,12,1);Circle(12,12,1);Circle(19,12,1);break;
                default:Circle(12,12,9);Line(7,12,11,16);Line(11,16,18,8);break;
            }
        }
    }
}
