"""Shared, deterministic projection and source normalization for the frozen AOI."""
from pathlib import Path
import json
import numpy as np
import rasterio
from rasterio.warp import reproject, Resampling
from rasterio.transform import from_origin
from pyproj import Transformer
from shapely.geometry import Polygon, box, shape
from shapely.ops import transform
from shapely.geometry.polygon import orient

ROOT = Path(__file__).resolve().parents[1]
CACHE = ROOT / 'worldgen/cache'
PACK = ROOT / 'worlds/meadowbrook-v1'
ORIGIN = [-91.037, 43.7015]
WGS_BOUNDS = [-91.043, 43.697, -91.031, 43.706]
CONTEXT = [-590, -630, 590, 630]
TO_METRIC = Transformer.from_crs(4326, 26915, always_xy=True)
TO_WGS = Transformer.from_crs(26915, 4326, always_xy=True)
OX, OZ = TO_METRIC.transform(*ORIGIN)
FRAME = box(*CONTEXT)
CLASS = {11:'water', 12:'sealed', 21:'grass', 22:'sealed', 23:'sealed',
         24:'sealed', 31:'sealed', 41:'woodland', 42:'woodland',
         43:'woodland', 52:'grass', 71:'grass', 81:'grass', 82:'cultivated',
         90:'woodland', 95:'grass'}
PALETTE = {'water':'#68a8b5','grass':'#aeb782','sealed':'#c1b5a2',
           'woodland':'#497b62','cultivated':'#d1ba73'}
CONTROLS = [('origin',-91.037,43.7015),
            ('western-stream-junction',-91.04136251631216,43.70247288083808),
            ('hwy162-brye-junction',-91.03213094478438,43.702001949420364)]

def control_points():
    result=[]
    for name,lon,lat in CONTROLS:
        x,z=TO_METRIC.transform(lon,lat)
        result.append({'id':name,'longitude':lon,'latitude':lat,'localX':round(x-OX,3),'localZ':round(z-OZ,3)})
    return result

def local(x, y, z=None):
    xx, yy = TO_METRIC.transform(x, y)
    return np.asarray(xx)-OX, np.asarray(yy)-OZ

def ring(poly):
    return [{'x':round(x,3), 'z':round(z,3)}
            for x,z in list(orient(poly, 1).exterior.coords)[:-1]]

def terrain():
    # Pixel centres are the terrain vertices. Never interpret pixel edges as nodes.
    target = from_origin(OX-595, OZ+635, 10, 10)
    height = np.full((127,119), np.nan, np.float32)
    for name in ['x65y484.tif', 'x65y485.tif']:
        with rasterio.open(CACHE/name) as src:
            assert src.crs.to_epsg() == 26915
            projected = np.full_like(height, np.nan)
            reproject(rasterio.band(src,1), projected, src_transform=src.transform,
                      src_crs=src.crs, src_nodata=src.nodata, dst_transform=target,
                      dst_crs='EPSG:26915', dst_nodata=np.nan,
                      resampling=Resampling.bilinear)
            valid = np.isfinite(projected)
            height[valid] = projected[valid]
    if not np.isfinite(height).all():
        raise ValueError('DEM has missing cells inside the complete context grid')
    origin = float(np.floor(height.min()/10)*10)
    return np.round(height[::-1]-origin,3), origin

def covers():
    result=[]
    with rasterio.open(CACHE/'nlcd-2021.tif') as src:
        assert src.crs.to_epsg() == 5070 and src.res == (30,30)
        classes=src.read(1)
        projection=Transformer.from_crs(src.crs,26915,always_xy=True)
        for row in range(src.height):
            for col in range(src.width):
                corners=[src.transform*p for p in [(col,row),(col,row+1),(col+1,row+1),(col+1,row)]]
                poly=Polygon([(x-OX,z-OZ) for x,z in [projection.transform(*p) for p in corners]]).intersection(FRAME)
                if poly.is_empty or poly.area<.05: continue
                code=int(classes[row,col])
                if code not in CLASS: raise ValueError(f'Unsupported/missing NLCD class {code}')
                result.append((f'nlcd-{row:02}-{col:02}',code,poly))
    return result

def vectors(require=True):
    result=[]
    for filename,kind,source,width in [
        ('water.geojson','water','usgs-nhd',5),
        ('roads.geojson','path','usgs-transport',5),
        ('connecting-roads.geojson','path','usgs-transport',7),
        ('highways.geojson','path','usgs-transport',8)]:
        if not (CACHE/filename).exists():
            if require: raise ValueError(f'Missing required source {filename}')
            continue
        data=json.loads((CACHE/filename).read_text())
        if data.get('error') or data.get('exceededTransferLimit'):
            raise ValueError(f'Incomplete service response: {filename}')
        for feature in data['features']:
            geom=transform(local,shape(feature['geometry'])).intersection(FRAME)
            if geom.is_empty: continue
            props=feature['properties']
            # Ditches/streams/artificial channel paths are mapped lines, not inferred flow.
            if kind=='water' and props.get('ftype') not in [336,460,558]: continue
            pieces=list(geom.geoms) if hasattr(geom,'geoms') else [geom]
            for i,line in enumerate(pieces):
                if line.geom_type!='LineString' or line.length<1: continue
                line=line.simplify(1,preserve_topology=True)
                ident=str(props.get('permanent_identifier',feature.get('id')))
                result.append(({'id':f'{filename.split(".")[0]}-{ident}-{i}',
                    'kind':kind,'sourceId':source,'decorative':False,'widthM':width,
                    'points':[{'x':round(x,3),'z':round(z,3)} for x,z in line.coords]}, props))
    return result

def overview(path, sites=None, require=True):
    import matplotlib
    matplotlib.use('Agg')
    import matplotlib.pyplot as plt
    from matplotlib.collections import PatchCollection
    from matplotlib.patches import Polygon as PlotPolygon
    heights,origin=terrain()
    cs=covers()
    fig,ax=plt.subplots(figsize=(10,11))
    ax.add_collection(PatchCollection([PlotPolygon(np.array(p.exterior.coords)) for _,_,p in cs],
        facecolors=[PALETTE[CLASS[c]] for _,c,_ in cs],edgecolors='none'))
    xx=np.linspace(-590,590,119);zz=np.linspace(-630,630,127)
    lines=ax.contour(xx,zz,heights+origin,levels=12,colors='#34473b',alpha=.45,linewidths=.5)
    ax.clabel(lines,fontsize=7,fmt='%d m')
    for f,_ in vectors(require):
        points=np.array([[p['x'],p['z']] for p in f['points']])
        ax.plot(points[:,0],points[:,1],color='#237da3' if f['kind']=='water' else '#745a45',linewidth=2)
    editable=transform(local,box(*WGS_BOUNDS))
    ax.plot(*editable.exterior.xy,'--',color='#343b35',lw=1)
    for s in sites or []:
        pts=np.array([[p['x'],p['z']] for p in s['polygon']]);closed=np.vstack([pts,pts[0]])
        ax.plot(*closed.T,color='#ac3856' if s['protectedSite'] else '#fff4e3',lw=2)
        ax.text(*pts.mean(axis=0),s['id'],fontsize=8,ha='center',color='#142b25',bbox=dict(facecolor='white',alpha=.8,pad=2,edgecolor='none'))
    for c in control_points():
        x,z=c['localX'],c['localZ'];ax.plot(x,z,'+',color='black')
        ax.annotate(c['id'],xy=(x,z),xytext=(x-110,z+20),fontsize=7,
                    bbox=dict(facecolor='white',alpha=.8,pad=2,edgecolor='none'))
    ax.set(xlim=(-610,610),ylim=(-650,650),aspect='equal',xlabel='Local X / east (m)',ylabel='Local Z / north (m)',
           title='Meadowbrook Valley • real Wisconsin AOI\n3DEP 2020 terrain · NLCD 2021 cover · NHD water · TNM roads')
    ax.grid(alpha=.15);fig.tight_layout();fig.savefig(path,dpi=150);plt.close(fig)

if __name__=='__main__':
    overview(CACHE/'alignment-draft.png',require=False)
    print('Draft:',CACHE/'alignment-draft.png')
