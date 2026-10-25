#!/usr/bin/env python3
"""Rebuild the two explicitly authorized regional scenarios from cached public DEM tiles.
Elevation is observed; every cover patch, path and management site is authored.
No Wisconsin vertices, cover, hydrology groups or parcels are reused.
Requires numpy and Pillow; curl performs verified HTTPS downloads.
"""
import hashlib, json, math, pathlib, subprocess
import numpy as np
from PIL import Image
ROOT=pathlib.Path(__file__).resolve().parents[1]
CACHE=ROOT/'worldgen/cache/regions'; CACHE.mkdir(parents=True,exist_ok=True)
REGIONS=[('point-reyes-v1',38.066,-122.810,'Point Reyes','Coastal meadow'),('suusamyr-v1',42.18,73.96,'Suusamyr Valley','High pasture')]
Z=14;N=2**Z;R=6378137.0

def write(path,data):
    path.write_text(json.dumps(data,indent=2)+'\n')
def ring(x,z,w,d):return [{'x':x,'z':z},{'x':x+w,'z':z},{'x':x+w,'z':z+d},{'x':x,'z':z+d}]
def fraction(cover):return [{'coverId':cover,'fraction':1}]
def pixels(lat,lon):return (lon+180)/360*N*256,(1-math.asinh(math.tan(math.radians(lat)))/math.pi)/2*N*256

def build(pack,lat,lon,name,field):
    out=ROOT/'worlds'/pack;out.mkdir(parents=True,exist_ok=True)
    degrees=180/math.pi/R; cos=math.cos(math.radians(lat))
    def wgs(x,z):return lat+z*degrees,lon+x*degrees/cos
    tiles={};sources=[]
    for x in (-600,600):
        for z in (-600,600):
            px,py=pixels(*wgs(x,z));tiles[int(px)//256,int(py)//256]=None
    xx=[key[0] for key in tiles]; yy=[key[1] for key in tiles]
    for tx in range(min(xx),max(xx)+1):
        for ty in range(min(yy),max(yy)+1):
            path=CACHE/f'{Z}-{tx}-{ty}.png';url=f'https://s3.amazonaws.com/elevation-tiles-prod/terrarium/{Z}/{tx}/{ty}.png'
            if not path.exists():subprocess.run(['curl','-fLsS','--retry','2',url,'-o',str(path)],check=True)
            values=np.asarray(Image.open(path).convert('RGB')).astype(float)
            tiles[tx,ty]=values[:,:,0]*256+values[:,:,1]+values[:,:,2]/256-32768
            sources.append({'url':url,'sha256':hashlib.sha256(path.read_bytes()).hexdigest()})
    def sample(x,z):
        px,py=pixels(*wgs(x,z));ix,iy=int(px),int(py);u,v=px-ix,py-iy
        def val(a,b):
            key=a//256,b//256
            if key not in tiles:return tiles[ix//256,iy//256][min(255,b%256),min(255,a%256)]
            return tiles[key][b%256,a%256]
        return (val(ix,iy)*(1-u)+val(ix+1,iy)*u)*(1-v)+(val(ix,iy+1)*(1-u)+val(ix+1,iy+1)*u)*v
    absolute=np.array([[sample(x,z) for x in range(-600,601,10)] for z in range(-600,601,10)])
    origin=math.floor(float(absolute.min())/10)*10
    heights=np.round(absolute-origin,3)
    terrain={'columns':121,'rows':121,'cellSizeM':10,'xMin':-600,'zMin':-600,'heights':heights.flatten().tolist()}
    # An authored landscape mosaic, not a land-cover classification. Nonoverlapping squares
    # cover the entire domain; protected cells remain out of the opportunity search.
    covers=[];coverarea={'grass':0,'cultivated':0,'woodland':0}
    for iz,z in enumerate(range(-600,600,100)):
        for ix,x in enumerate(range(-600,600,100)):
            cover='grass'
            if pack=='point-reyes-v1' and ((ix<2 and iz>6) or (ix>9 and iz>8)):cover='woodland'
            if pack=='point-reyes-v1' and iz<5 and 3<=ix<=7:cover='cultivated'
            covers.append({'id':f'cover-{ix}-{iz}','coverId':cover,'sourceId':'scenario-assumptions-v1','polygon':ring(x,z,100,100)})
            coverarea[cover]+=10000
    # Choose separated, reasonably flat opportunities from real terrain. Never flatten the DEM.
    candidates=[]
    for z in range(-440,281,40):
        for x in range(-440,281,40):
            v=heights[(z+600)//10:(z+600+160)//10+1,(x+600)//10:(x+600+160)//10+1]
            candidates.append((float(v.max()-v.min()),x,z))
    chosen=[]
    for relief,x,z in sorted(candidates):
        if any(x<ox+190 and x+190>ox and z<oz+190 and z+190>oz for _,ox,oz in chosen):continue
        chosen.append((relief,x,z))
        if len(chosen)==10:break
    assert len(chosen)>=8
    sites=[]
    for i,(relief,x,z) in enumerate(chosen):
        # site rectangles split cover analytically by exact intersections with the mosaic.
        areas={key:0 for key in coverarea}
        for c in covers:
            a=c['polygon'][0];area=max(0,min(x+160,a['x']+100)-max(x,a['x']))*max(0,min(z+160,a['z']+100)-max(z,a['z']))
            areas[c['coverId']]+=area
        if areas['woodland']>0: # Keep offered sites visually open; update authored cover coherently.
            for c in covers:
                a=c['polygon'][0]
                if x<a['x']+100 and x+160>a['x'] and z<a['z']+100 and z+160>a['z'] and c['coverId']=='woodland':
                    c['coverId']='grass';coverarea['woodland']-=10000;coverarea['grass']+=10000
            areas['grass']+=areas['woodland'];areas['woodland']=0
        swale=i in (0,3)
        offers=[{'treatmentId':tid,'cost':cost,'production':production,'retentionCapacityM3':capacity} for tid,cost,production,capacity in [('cultivate-standard',24,12,0),('cultivate-margin',30,9,0),('restore-grass',18,0,0)]]+([{'treatmentId':'water-swale','cost':22,'production':0,'retentionCapacityM3':8}] if swale else [])
        sites.append({'id':f'region-{i+1:02}','name':f'{field} {i+1}','sourceId':'scenario-assumptions-v1','contributingAreaId':f'site-only-region-{i+1:02}','modelParameterId':'cn-b-cover-assumed-v1','polygon':ring(x,z,160,160),'analysisAreaM2':25600,'baselineProduction':2,'protectedSite':False,'protectionReason':'','baselineCover':[{'coverId':c,'fraction':a/25600} for c,a in areas.items() if a],'allowedTreatments':[o['treatmentId'] for o in offers],'offers':offers})
    # Recalculate every site's cover after all authored-patch adjustments, then subtract it once.
    for site in sites:
        x,z=site['polygon'][0]['x'],site['polygon'][0]['z'];areas={key:0 for key in coverarea}
        for c in covers:
            a=c['polygon'][0];areas[c['coverId']]+=max(0,min(x+160,a['x']+100)-max(x,a['x']))*max(0,min(z+160,a['z']+100)-max(z,a['z']))
        site['baselineCover']=[{'coverId':c,'fraction':a/25600} for c,a in areas.items() if a]
        for c,a in areas.items():coverarea[c]-=a
    world={'terrain':terrain,'covers':covers,'features':[],'sites':sites};write(out/'world.json',world)
    scenario=json.loads((ROOT/'worlds/meadowbrook-v1/scenario.json').read_text());scenario.update(packId=pack,id=pack+'-annual',introId=pack+'-intro',learningId='regional-land-use',assumptionsId='scenario-assumptions-v1');write(out/'scenario.json',scenario)
    lat0,lon0=wgs(-600,-600);lat1,lon1=wgs(600,600)
    manifest={'schemaVersion':1,'packId':pack,'fixtureVersion':'1.0.0','geographyKind':'real-geography','selectedLocationId':pack,'sourceCrs':'WGS84 / local equirectangular metre approximation at origin latitude','elevationDatum':'USGS source elevation via Mapzen; vertical datum varies by source. No engineering datum claim.','boundsWgs84':[lon0,lat0,lon1,lat1],'originWgs84':[lon,lat],'originElevationM':origin,'editableBounds':[-500,-500,500,500],'contextBounds':[-600,-600,600,600],'worldDataPath':'world.json','scenarioPath':'scenario.json','rasterRows':'south-to-north','winding':'counterclockwise-xz',
      'layers':[{'id':'terrain','path':'world.json','availability':'observed'},{'id':'cover','path':'world.json','availability':'authored'},{'id':'sites','path':'world.json','availability':'authored'},{'id':'water','path':'','availability':'unavailable'},{'id':'paths','path':'','availability':'unavailable'}],
      'sources':[{'id':'mapzen-terrain','title':'Mapzen / AWS Terrain Tiles; USGS 3DEP and global SRTM/GMTED2010','url':'https://registry.opendata.aws/terrain-tiles/','date':'Retrieved 2026-10-25; source acquisition dates vary, not determined per tile','license':'USGS public domain; Mapzen attribution retained','role':'Terrarium z14 elevation sampled to a 10 m display grid. Effective resolution may be coarser; no added observed detail.'},{'id':'scenario-assumptions-v1','title':'Terra authored regional scenario','url':'','date':'2026-10-25','license':'project-original','role':'Every cover patch, management site, baseline production, consent, service connection, budget, rule and runoff connection is hypothetical; not observed land use.'}]};write(out/'manifest.json',manifest)
    hydro=json.loads((ROOT/'worlds/meadowbrook-v1/hydrology.json').read_text());hydro.update(worldSha256=hashlib.sha256((out/'world.json').read_bytes()).hexdigest(),partitionSha256=hashlib.sha256(json.dumps(coverarea,sort_keys=True).encode()).hexdigest(),analysisAreaM2=1440000,connectionNote='Each regional site is isolated. No mapped drainage or external inflow. Cover and soil are authored assumptions.',context=[{'id':'context-'+c,'coverId':c,'areaM2':a} for c,a in coverarea.items() if a>0],connections=[{'siteId':s['id'],'groupId':s['contributingAreaId']} for s in sites]);write(out/'hydrology.json',hydro)
    write(out/'provenance.json',{'name':name,'command':'python3 worldgen/build-regions.py','elevationTiles':sources,'elevationRangeM':[float(absolute.min()),float(absolute.max())],'siteReliefM':[round(r,3) for r,_,_ in chosen],'evidence':'Real elevation only. All cover/sites/gameplay parameters authored. No observed water/road/parcel data, no local policy or suitability claim. USD 2026 values are shared game rules, including in Kyrgyzstan.','attribution':'Mapzen / AWS Terrain Tiles. United States 3DEP and global GMTED2010 and SRTM terrain data courtesy of the U.S. Geological Survey.','attributionUrl':'https://github.com/tilezen/joerd/blob/master/docs/attribution.md'})
    write(out/'checksums.json',{p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in sorted(out.glob('*.json')) if p.name!='checksums.json'})
    print(pack,'elevation range',round(float(absolute.min()),1),round(float(absolute.max()),1),'sites',len(sites),'reliefs',[round(c[0],1) for c in chosen],flush=True)
for region in REGIONS:build(*region)
