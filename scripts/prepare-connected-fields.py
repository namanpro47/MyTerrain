#!/usr/bin/env python3
"""Compile a same-location, shared-edge planning layout from the frozen source cover.
No GIS downloads or source-layer changes. Rectangle clipping uses the supplied rings.
"""
import argparse, copy, hashlib, json, math
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
WORLD=ROOT/'worlds/meadowbrook-v1/world.json'
OUTPUT=ROOT/'Assets/Terra/Resources/ConnectedFields.json'
BOUNDS=[(-95,-212.5,15,-105),(15,-212.5,145,-105),(145,-212.5,255,-105),
        (-95,-332.5,15,-212.5),(15,-332.5,145,-212.5),(145,-300,250,-212.5),
        (-65,-430,15,-332.5),(15,-430,145,-332.5)]
def polygon_area(poly):
    return abs(sum(a[0]*b[1]-b[0]*a[1] for a,b in zip(poly,poly[1:]+poly[:1])))/2

def clipped_area(poly,box):
    for k,edge,sign in [(0,box[0],1),(0,box[2],-1),(1,box[1],1),(1,box[3],-1)]:
        out=[]
        for a,b in zip(poly,poly[1:]+poly[:1]):
            ai=sign*(a[k]-edge)>=0;bi=sign*(b[k]-edge)>=0
            if ai:out.append(a)
            if ai!=bi:
                f=(edge-a[k])/(b[k]-a[k]);out.append((a[0]+f*(b[0]-a[0]),a[1]+f*(b[1]-a[1])))
        poly=out
        if not poly:break
    return polygon_area(poly)

def point_segment(p,a,b):
    dx,dz=b[0]-a[0],b[1]-a[1];length=dx*dx+dz*dz
    f=max(0,min(1,((p[0]-a[0])*dx+(p[1]-a[1])*dz)/length)) if length else 0
    return math.hypot(p[0]-a[0]-f*dx,p[1]-a[1]-f*dz)

def segment_distance(box,a,b):
    lo,hi=0.,1.;dx,dz=b[0]-a[0],b[1]-a[1]
    for p,q in [(-dx,a[0]-box[0]),(dx,box[2]-a[0]),(-dz,a[1]-box[1]),(dz,box[3]-a[1])]:
        if p==0:
            if q<0:break
        elif p<0:lo=max(lo,q/p)
        else:hi=min(hi,q/p)
    else:
        if lo<=hi:return 0
    corners=[(box[0],box[1]),(box[2],box[1]),(box[2],box[3]),(box[0],box[3])]
    return min([point_segment(p,a,b) for p in corners]+[point_segment(p,c,d) for p in [a,b] for c,d in zip(corners,corners[1:]+corners[:1])])

def build():
    world=json.loads(WORLD.read_text());sites=[];checks=[]
    for original,b in zip(world['sites'][:8],BOUNDS):
        area=(b[2]-b[0])*(b[3]-b[1]);cover={}
        for c in world['covers']:
            n=clipped_area([(p['x'],p['z']) for p in c['polygon']],b)
            cover[c['coverId']]=cover.get(c['coverId'],0)+n
        assert abs(sum(cover.values())-area)<.001
        assert cover.get('woodland',0)<.001 and cover.get('sealed',0)<.001
        distances={kind:min(segment_distance(b,(a['x'],a['z']),(z['x'],z['z'])) for f in world['features'] if f['kind']==kind for a,z in zip(f['points'],f['points'][1:])) for kind in ['water','path']}
        assert distances['water']>=20 and distances['path']>=15,(original['id'],distances)
        s=copy.deepcopy(original)
        s.update(sourceId='scenario-connected-fields-v2',polygon=[{'x':x,'z':z} for x,z in [(b[0],b[1]),(b[2],b[1]),(b[2],b[3]),(b[0],b[3])]],analysisAreaM2=area,
                 baselineCover=[{'coverId':k,'fraction':round(v/area,8)} for k,v in sorted(cover.items()) if v>.001])
        sites.append(s);checks.append({'siteId':s['id'],'areaM2':area,'sourceCoverAreaM2':cover,'minimumMappedCenterlineDistanceM':distances})
    edges=[]
    for i,a in enumerate(BOUNDS):
        for j,b in enumerate(BOUNDS[:i]):
            assert max(0,min(a[2],b[2])-max(a[0],b[0]))*max(0,min(a[3],b[3])-max(a[1],b[1]))==0
            shared=max(0,min(a[3],b[3])-max(a[1],b[1])) if a[0]==b[2] or a[2]==b[0] else max(0,min(a[2],b[2])-max(a[0],b[0])) if a[1]==b[3] or a[3]==b[1] else 0
            if shared:edges.append([sites[j]['id'],sites[i]['id'],shared])
    assert len(edges)==10
    return {'version':'connected-fields-v2','sourceWorldSha256':hashlib.sha256(WORLD.read_bytes()).hexdigest(),'sites':sites}, {'sites':checks,'sharedEdgesM':edges,'overlapM2':0,'note':'Eight contiguous hypothetical management parcels. Mapped water/woodland determine the stepped outer edge. Isolated east/west opportunities and protected zones are unchanged.'}
if __name__=='__main__':
    parser=argparse.ArgumentParser();parser.add_argument('--check',action='store_true');args=parser.parse_args()
    data,evidence=build();text=json.dumps(data,indent=2)+'\n'
    if args.check:assert OUTPUT.read_text()==text,'Connected layout stale'
    else:OUTPUT.write_text(text)
    print(json.dumps(evidence,indent=2))
