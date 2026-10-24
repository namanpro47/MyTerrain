#!/usr/bin/env python3
"""Independent source-alignment, coverage, safety and reproducibility evidence."""
import hashlib, json, math, subprocess, sys, tempfile
from pathlib import Path
import numpy as np
import rasterio
from shapely.geometry import shape, Polygon, LineString, box
from shapely.ops import unary_union, transform
from prepare import ROOT,CACHE,PACK,OX,OZ,ORIGIN,TO_METRIC,TO_WGS,FRAME,local,CONTEXT,WGS_BOUNDS

def read(name):return json.loads((PACK/name).read_text())
def polygon(points):return Polygon([(p['x'],p['z']) for p in points])
def require(value,message):
    if not value:raise AssertionError(message)
    checks.append(message)

checks=[];metrics={}
m=read('manifest.json');w=read('world.json');provenance=read('provenance.json')
require(m['schemaVersion']==1 and m['geographyKind']=='real-geography','Frozen real-geography v1 contract')
require(m['boundsWgs84']==WGS_BOUNDS,'Selected editable WGS84 bounds preserved')
require(abs(OX-658170.12464317)<.001 and abs(OZ-4840592.3595111435)<.001,'Golden origin EPSG:26915 control point within 1 mm numerical tolerance')
controls=[{'id':'origin','longitude':ORIGIN[0],'latitude':ORIGIN[1]},
          {'id':'western-stream-junction','longitude':-91.04136251631216,'latitude':43.70247288083808},
          {'id':'hwy162-brye-junction','longitude':-91.03213094478438,'latitude':43.702001949420364}]
for c in controls:
    x,z=TO_METRIC.transform(c['longitude'],c['latitude']);lon,lat=TO_WGS.transform(x,z)
    xx,zz=TO_METRIC.transform(lon,lat)
    c.update(localX=round(x-OX,3),localZ=round(z-OZ,3),roundTripM=math.hypot(xx-x,zz-z))
require(max(c['roundTripM'] for c in controls)<.001,'Reference point round trips within 1 mm numerical tolerance; not a source-accuracy claim')
metrics['controlPoints']=controls
cover=[polygon(c['polygon']) for c in w['covers']]
require(all(p.is_valid and p.exterior.is_ccw and p.area>0 and p.equals(p.convex_hull) for p in cover),'Every render cover is valid, convex, CCW and nonzero')
union=unary_union(cover)
metrics['coverGapM2']=FRAME.difference(union).area
metrics['coverOverlapM2']=sum(p.area for p in cover)-union.area
require(metrics['coverGapM2']<2 and metrics['coverOverlapM2']<2,'Rounded rendered cover conserves context area within 2 m²')
units=read('analysis-units.geojson')['features'];unit_shapes=[shape(f['geometry']) for f in units]
unit_union=unary_union(unit_shapes)
metrics['analysisGapM2']=FRAME.difference(unit_union).area
metrics['analysisOverlapM2']=sum(p.area for p in unit_shapes)-unit_union.area
require(metrics['analysisGapM2']<.1 and abs(metrics['analysisOverlapM2'])<.1,'Analysis units partition the entire context without double counting (0.1 m² tolerance)')
require(all(abs(f['properties']['areaM2']-shape(f['geometry']).area)<1e-6 for f in units),'Analysis geometry areas match their declared metric areas')
exact=transform(local,box(*WGS_BOUNDS))
require(all(exact.contains(polygon(s['polygon'])) for s in w['sites']),'All sites lie within exact projected WGS84 AOI, not just its bounding rectangle')
clearances=read('site-clearances.json')
require(all(s['protected'] or (s['minimumCenterlineDistanceM']['water']>=20 and s['minimumCenterlineDistanceM']['path']>=15 and s['woodlandOverlapM2']<=1 and s['wetlandOverlapM2']<=1) for s in clearances),'Editable sites preserve mapped woodland/wetland and scenario road/stream buffers')
# Compare source line geometries with their clipped/simplified runtime representation.
line_errors={}
for filename,kind in [('water.geojson','water'),('roads.geojson','path'),('connecting-roads.geojson','path'),('highways.geojson','path')]:
    raw=json.loads((CACHE/filename).read_text());original=[]
    for f in raw['features']:
        if kind=='water' and f['properties'].get('ftype') not in [336,460,558]:continue
        geom=transform(local,shape(f['geometry'])).intersection(FRAME)
        if not geom.is_empty:original.append(geom)
    exported=[LineString([(p['x'],p['z']) for p in f['points']]) for f in w['features'] if f['id'].startswith(filename.split('.')[0]+'-')]
    if not original:
        require(not exported,f'{filename}: no invented geometry for an empty source');continue
    error=unary_union(original).hausdorff_distance(unary_union(exported));line_errors[filename]=error
    require(error<1.01,f'{filename}: exported lines within 1.01 m of projected source')
metrics['vectorHausdorffM']=line_errors
# Sample high-resolution source elevations at independent 5 m-offset probe positions.
# Compare against the triangle interpolation used for the runtime terrain mesh.
t=w['terrain'];heights=np.array(t['heights']).reshape(t['rows'],t['columns'])
xs,zs=np.meshgrid(np.arange(-575,590,30),np.arange(-615,630,30))
points=list(zip(xs.ravel()+OX,zs.ravel()+OZ));truth=np.full(len(points),np.nan)
for filename in ['x65y484.tif','x65y485.tif']:
    with rasterio.open(CACHE/filename) as source:
        vals=np.array([v[0] for v in source.sample(points,masked=False)])
        valid=vals!=source.nodata;truth[valid]=vals[valid]
fx=(xs.ravel()+590)/10;fz=(zs.ravel()+630)/10
ix=fx.astype(int);iz=fz.astype(int);u=fx-ix;v=fz-iz
a=heights[iz,ix];b=heights[iz,ix+1];c=heights[iz+1,ix];d=heights[iz+1,ix+1]
pred=np.where(u+v<=1,a+(b-a)*u+(c-a)*v,d+(c-d)*(1-u)+(b-d)*(1-v))+m['originElevationM']
errors=np.abs(pred-truth)
metrics['terrainProbes']={'count':len(points),'meanAbsM':float(errors.mean()),'p95AbsM':float(np.percentile(errors,95)),'maxAbsM':float(errors.max()),'method':'5 m offset probes; source nearest 1 m sample vs runtime 10 m triangle interpolation'}
require(np.isfinite(truth).all(),'Every independent terrain probe has source coverage')
require(errors.mean()<1 and np.percentile(errors,95)<2 and errors.max()<10,'Terrain simplification errors below 1 m mean, 2 m p95 and 10 m maximum')
lock=json.loads((ROOT/'worldgen/source-lock.json').read_text())
for item in lock:
    require(hashlib.sha256((CACHE/item['file']).read_bytes()).hexdigest()==item['sha256'],'Pinned source hash: '+item['file'])
# Regeneration happens in a clean output directory; compare all normalized JSON artifacts.
with tempfile.TemporaryDirectory(prefix='terra-pack-verify-') as temporary:
    target=Path(temporary)
    subprocess.run([sys.executable,str(ROOT/'worldgen/build.py'),'--output',str(target)],check=True,capture_output=True)
    names=[p.name for p in PACK.iterdir() if p.suffix in ['.json','.geojson'] and p.name not in ['checksums.json','scenario.json']]
    for name in sorted(names):
        require((PACK/name).read_bytes()==(target/name).read_bytes(),'Deterministic normalized export: '+name)
    # A mission owner may intentionally edit scenario.json; generator must preserve it.
    sentinel=b'{"primaryEngineerOwnsMission":true}\n';(target/'scenario.json').write_bytes(sentinel)
    subprocess.run([sys.executable,str(ROOT/'worldgen/build.py'),'--output',str(target)],check=True,capture_output=True)
    require((target/'scenario.json').read_bytes()==sentinel,'Regeneration preserves the primary-owned scenario.json')
report={'checks':checks,'count':len(checks),'metrics':metrics,'scope':'Offline geography evidence; browser rendering/performance are separate gates.'}
out=ROOT/'docs/evidence/phase2';out.mkdir(parents=True,exist_ok=True)
(out/'geography-checks.json').write_text(json.dumps(report,indent=2)+'\n')
print(json.dumps({'count':len(checks),'metrics':metrics},indent=2))
