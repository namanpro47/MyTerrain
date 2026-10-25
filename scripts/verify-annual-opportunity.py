#!/usr/bin/env python3
"""Verify the additional hypothetical wind envelope against the canonical same-AOI data."""
import json, math
from pathlib import Path
world=json.loads(Path('worlds/meadowbrook-v1/world.json').read_text())
box=(375.,-495.,525.,-345.)
def area(poly):
    return abs(sum(a[0]*b[1]-b[0]*a[1] for a,b in zip(poly,poly[1:]+poly[:1])))/2
def clip(poly):
    for k,edge,sign in [(0,box[0],1),(0,box[2],-1),(1,box[1],1),(1,box[3],-1)]:
        out=[]
        for a,b in zip(poly,poly[1:]+poly[:1]):
            ai=sign*(a[k]-edge)>=0;bi=sign*(b[k]-edge)>=0
            if ai:out.append(a)
            if ai!=bi:
                f=(edge-a[k])/(b[k]-a[k]);out.append((a[0]+f*(b[0]-a[0]),a[1]+f*(b[1]-a[1])))
        poly=out
        if not poly:break
    return area(poly) if poly else 0
for site in world['sites']:
    assert clip([(p['x'],p['z']) for p in site['polygon']])<1e-6,site['id']
def segment_hits(a,b,pad):
    # Liang–Barsky against the clearance rectangle, inflated by mapped feature half-width + 5 m.
    t0,t1=0.,1.;dx=b['x']-a['x'];dz=b['z']-a['z']
    for p,q in [(-dx,a['x']-box[0]+pad),(dx,box[2]+pad-a['x']),(-dz,a['z']-box[1]+pad),(dz,box[3]+pad-a['z'])]:
        if p==0:
            if q<0:return False
        elif p<0:t0=max(t0,q/p)
        else:t1=min(t1,q/p)
        if t0>t1:return False
    return True
for feature in world['features']:
    for a,b in zip(feature['points'],feature['points'][1:]):
        assert not segment_hits(a,b,feature.get('widthM',0)/2+5),feature['id']
cover={}
for row in world['covers']:
    cover[row['coverId']]=cover.get(row['coverId'],0)+clip([(p['x'],p['z']) for p in row['polygon']])
assert abs(sum(cover.values())-22500)<.001
assert abs(cover['grass']-222.1696208)<.001
assert cover.get('woodland',0)==0 and cover.get('sealed',0)==0
report={'scope':'Same AOI. Authored wind-only opportunity; not surveyed ownership, consent or engineering suitability.','bounds':box,'areaM2':22500,'sourceCoverAreaM2':cover,'overlapExistingSites':False,'mappedFeatureBufferClear':True,'sourceResolutionM':30,'contextAreaRemovedM2':22500}
Path('docs/evidence/full-game').mkdir(parents=True,exist_ok=True)
Path('docs/evidence/full-game/wind-opportunity.json').write_text(json.dumps(report,indent=2)+'\n')
print('Annual wind opportunity: geometry, source cover and disjoint partition passed')
