#!/usr/bin/env python3
"""Fetch only the frozen Meadowbrook Valley source window; cached inputs are reusable."""
from pathlib import Path
import hashlib, json, math, requests
from pyproj import Transformer
ROOT=Path(__file__).resolve().parents[1]; CACHE=ROOT/'worldgen/cache';CACHE.mkdir(parents=True,exist_ok=True)
BBOX=[-91.044,43.696,-91.030,43.707]
lock_path=ROOT/'worldgen/source-lock.json'
locked={v['file']:v for v in json.loads(lock_path.read_text())} if lock_path.exists() else {}
def fetch(name,url,params=None):
    path=CACHE/name
    if not path.exists():
        response=requests.get(url,params=params,timeout=180);response.raise_for_status()
        if name.endswith('.geojson'):
            data=response.json()
            if data.get('error') or data.get('exceededTransferLimit') or data.get('type')!='FeatureCollection':
                raise ValueError(f'Incomplete vector response: {name}')
        content=response.content
        if name in locked and hashlib.sha256(content).hexdigest()!=locked[name]['sha256']:
            raise ValueError(f'Upstream data changed: {name}; review a deliberate source-version update')
        temporary=path.with_suffix(path.suffix+'.part');temporary.write_bytes(content);temporary.replace(path)
    if name in locked and hashlib.sha256(path.read_bytes()).hexdigest()!=locked[name]['sha256']:
        raise ValueError(f'Cached input checksum mismatch: {name}')
    return {'file':name,'url':requests.Request('GET',url,params=params).prepare().url}
sources=[]
sources.append(fetch('elevation-metadata.json','https://www.sciencebase.gov/catalog/item/65bb34cdd34e18c6baf2e0f6',{'format':'json'}))
for tile in ['x65y484','x65y485']:
    sources.append(fetch(tile+'.tif',f'https://prd-tnm.s3.amazonaws.com/StagedProducts/Elevation/1m/Projects/WI_8County_2020_A20/TIFF/USGS_1M_15_{tile}_WI_8County_2020_A20.tif'))
# Request native NLCD integer classes on exactly aligned 30 m Albers pixel edges.
t=Transformer.from_crs(4326,5070,always_xy=True)
points=[t.transform(x,y) for x in [BBOX[0],BBOX[2]] for y in [BBOX[1],BBOX[3]]]
x0=-2493045+math.floor((min(p[0] for p in points)-120+2493045)/30)*30
x1=-2493045+math.ceil((max(p[0] for p in points)+120+2493045)/30)*30
y0=177285+math.floor((min(p[1] for p in points)-120-177285)/30)*30
y1=177285+math.ceil((max(p[1] for p in points)+120-177285)/30)*30
sources.append(fetch('nlcd-2021.tif','https://www.mrlc.gov/geoserver/mrlc_download/wcs',{'service':'WCS','version':'1.0.0','request':'GetCoverage','coverage':'NLCD_2021_Land_Cover_L48','crs':'EPSG:5070','response_crs':'EPSG:5070','bbox':f'{x0},{y0},{x1},{y1}','resx':30,'resy':30,'format':'GeoTIFF','interpolation':'nearest neighbor'}))
query={'where':'1=1','geometry':','.join(map(str,BBOX)),'geometryType':'esriGeometryEnvelope','inSR':'4326','outSR':'4326','outFields':'*','returnGeometry':'true','f':'geojson'}
for name,url in [('water.geojson','https://hydro.nationalmap.gov/arcgis/rest/services/nhd/MapServer/6/query'),('roads.geojson','https://carto.nationalmap.gov/arcgis/rest/services/transportation/MapServer/32/query'),('connecting-roads.geojson','https://carto.nationalmap.gov/arcgis/rest/services/transportation/MapServer/31/query'),('highways.geojson','https://carto.nationalmap.gov/arcgis/rest/services/transportation/MapServer/30/query')]:sources.append(fetch(name,url,query))
for s in sources:print(s['file'],(CACHE/s['file']).stat().st_size)
(CACHE/'requests.json').write_text(json.dumps(sources,indent=2)+'\n')
