using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Terra.UI
{
    // Non-interactive screen label. Follows the footprint, never the cursor, and never owns a command.
    public sealed class HudPlacementAnchor : MonoBehaviour
    {
        public Canvas canvas;
        public Camera view;
        public Vector3[] points;
        public RectTransform[] obstacles;
        public Func<bool> show;
        readonly Vector3[] corners=new Vector3[4];
        void LateUpdate()
        {
            var group=GetComponent<CanvasGroup>();group.alpha=show==null||show()?1:0;
            if(view==null||canvas==null||points==null||points.Length==0)return;
            var rect=(RectTransform)transform;float scale=canvas.scaleFactor;
            Vector2 size=rect.sizeDelta*scale;float pad=14*scale;
            float left=float.MaxValue,right=float.MinValue,top=float.MinValue,bottom=float.MaxValue;bool inFront=false;
            foreach(var point in points)
            {var p=view.WorldToScreenPoint(point);if(p.z<=0)continue;inFront=true;left=Mathf.Min(left,p.x);right=Mathf.Max(right,p.x);top=Mathf.Max(top,p.y);bottom=Mathf.Min(bottom,p.y);}
            var screen=canvas.pixelRect;
            Vector2 fallback=new Vector2(screen.center.x-size.x/2,screen.yMax-118*scale-size.y);
            var candidates=inFront?new[]{new Vector2((left+right-size.x)/2,top+pad),new Vector2(left-pad-size.x,(top+bottom-size.y)/2),new Vector2(right+pad,(top+bottom-size.y)/2),new Vector2((left+right-size.x)/2,bottom-pad-size.y),fallback}:new[]{fallback};
            Vector2 chosen=fallback;
            foreach(var candidate in candidates)
            {
                var area=new Rect(candidate,size);
                if(area.xMin<screen.xMin+pad||area.xMax>screen.xMax-pad||area.yMin<screen.yMin+pad||area.yMax>screen.yMax-pad)continue;
                bool overlap=false;
                foreach(var obstacle in obstacles)
                {
                    if(obstacle==null||!obstacle.gameObject.activeInHierarchy)continue;
                    obstacle.GetWorldCorners(corners);
                    var bounds=Rect.MinMaxRect(corners[0].x-pad,corners[0].y-pad,corners[2].x+pad,corners[2].y+pad);
                    if(area.Overlaps(bounds)){overlap=true;break;}
                }
                var cursor=Mouse.current?.position.ReadValue();
                if(cursor.HasValue&&area.Contains(cursor.Value))overlap=true;
                if(!overlap){chosen=candidate;break;}
            }
            rect.anchorMin=rect.anchorMax=rect.pivot=Vector2.zero;
            rect.anchoredPosition=(chosen-screen.position)/scale;
        }
    }
}
