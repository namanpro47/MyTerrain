#!/usr/bin/env python3
"""One AOI, frozen v1 contract. Run with the pinned worldgen environment."""
import hashlib, json, argparse
import numpy as np
from shapely.geometry import box, Polygon, LineString, mapping
from shapely.ops import transform, unary_union
from prepare import (ROOT,CACHE,PACK,ORIGIN,OX,OZ,WGS_BOUNDS,CONTEXT,CLASS,
                     FRAME,local,ring,terrain,covers,vectors,overview,control_points)

def write(path,data):
    path.parent.mkdir(parents=True,exist_ok=True)
    path.write_text(json.dumps(data,indent=2,allow_nan=False)+'\n')

def fractions(poly, cover_polys):
    areas={key:0 for key in sorted(set(CLASS.values()))}
    for _,code,p in cover_polys:
        if p.intersects(poly): areas[CLASS[code]]+=p.intersection(poly).area
    total=sum(areas.values())
    if abs(total-poly.area)>1: raise ValueError(f'Cover gap/overlap: {total-poly.area:.3f} m2')
    return [{'coverId':k,'fraction':round(v/total,7)} for k,v in areas.items() if v>1e-6]

def build(output=PACK):
    height,origin=terrain();cover_polys=covers();mapped=vectors()
    exact_editable=transform(local,box(*WGS_BOUNDS))
    templates=json.loads((ROOT/'worldgen/sites.json').read_text())
    scenario=json.loads((ROOT/'worlds/fixture-v1/scenario.json').read_text())
    scenario.update(id='meadowbrook-balance-v1',packId='meadowbrook-v1',introId='meadowbrook-intro')
    sites=[];safety=[]
    wetland=unary_union([p for _,code,p in cover_polys if code in [90,95]])
    woodland=unary_union([p for _,code,p in cover_polys if CLASS[code]=='woodland'])
    for index,item in enumerate(templates):
        poly=box(*item['bounds']); protected=item.get('protected',False)
        if not exact_editable.contains(poly): raise ValueError(f'{item["id"]} outside actual WGS84 AOI')
        if any(poly.intersection(Polygon([(v['x'],v['z']) for v in s['polygon']])).area>.01 for s in sites):
            raise ValueError('Management sites overlap')
        distances={}
        for kind,clearance in [('water',20),('path',15)]:
            lines=[LineString([(v['x'],v['z']) for v in f['points']]) for f,_ in mapped if f['kind']==kind]
            distances[kind]=min(poly.distance(line) for line in lines)
            if not protected and distances[kind]<clearance: raise ValueError(f'{item["id"]}: {kind} clearance {distances[kind]} < {clearance} m')
        if not protected and (poly.intersection(woodland).area>1 or poly.intersection(wetland).area>1):
            raise ValueError(f'{item["id"]} intersects mapped woodland/wetland')
        allowed=[] if protected else ['cultivate-standard','cultivate-margin','restore-grass']
        if item.get('swale'):allowed.append('water-swale')
        offers=[{'treatmentId':t,'cost':{'cultivate-standard':24,'cultivate-margin':30,'restore-grass':18,'water-swale':28}[t],
                 'production':12 if t=='cultivate-standard' else 9 if t=='cultivate-margin' else 0,
                 'retentionCapacityM3':8 if t=='water-swale' else 0} for t in allowed]
        sites.append({'id':item['id'],'name':item['name'],'sourceId':'scenario-assumptions-v1',
            'contributingAreaId':'site-only-'+item['id'],'modelParameterId':'cn-b-cover-assumed-v1',
            'polygon':ring(poly),'analysisAreaM2':poly.area,'baselineProduction':1 if index<6 else 0,
            'protectedSite':protected,'protectionReason':'Scenario no-edit zone; not a legal designation.' if protected else '',
            'baselineCover':fractions(poly,cover_polys),'allowedTreatments':allowed,'offers':offers})
        safety.append({'siteId':item['id'],'protected':protected,'minimumCenterlineDistanceM':distances,
                       'woodlandOverlapM2':poly.intersection(woodland).area,'wetlandOverlapM2':poly.intersection(wetland).area})
    manifest={'schemaVersion':1,'packId':'meadowbrook-v1','fixtureVersion':'1.0.0','geographyKind':'real-geography',
        'selectedLocationId':'coon-valley-brye-v1','sourceCrs':'EPSG:26915','elevationDatum':'NAVD88 metres (3DEP source metadata)',
        'boundsWgs84':WGS_BOUNDS,'originWgs84':ORIGIN,'originElevationM':origin,
        'editableBounds':[round(v,3) for v in exact_editable.bounds],'contextBounds':CONTEXT,
        'worldDataPath':'world.json','scenarioPath':'scenario.json','rasterRows':'south-to-north','winding':'counterclockwise-xz',
        'layers':[{'id':k,'path':'world.json','availability':v} for k,v in [('terrain','observed'),('cover','observed'),('water','mapped'),('paths','mapped'),('sites','hypothetical')]]+
                 [{'id':'buildings','path':'','availability':'not-ingested'},{'id':'soil','path':'','availability':'assumed'}],
        'sources':[
          {'id':'usgs-3dep-wi-2020','title':'USGS 3DEP WI_8County_2020_A20, x65y484 / x65y485','url':'https://www.sciencebase.gov/catalog/item/65bb34cdd34e18c6baf2e0f6','date':'Acquired 2020-04-11–2020-05-07; published 2024-01-24','license':'Public domain; credit USGS','role':'1 m bare-earth elevation, bilinear resampling to 10 m vertices'},
          {'id':'usgs-nlcd-2021','title':'USGS/MRLC NLCD 2021 Land Cover L48 (20230630)','url':'https://www.mrlc.gov/geoserver/mrlc_download/wcs','date':'2021 classification; release 2023-06-30','license':'Public domain; credit USGS/MRLC','role':'Native 30 m categorical cover; grouped display classes'},
          {'id':'usgs-nhd','title':'USGS NHD high-resolution flowlines','url':'https://hydro.nationalmap.gov/arcgis/rest/services/nhd/MapServer/6','date':'Retrieved 2026-10-24; individual feature dates in provenance','license':'Public domain; credit USGS','role':'Mapped stream/ditch centreline; rendered width is illustrative'},
          {'id':'usgs-transport','title':'USGS National Transportation, road layers 30–32','url':'https://carto.nationalmap.gov/arcgis/rest/services/transportation/MapServer','date':'Retrieved 2026-10-24; feature metadata retained; not a survey date','license':'Public domain; credit USGS / US Census Bureau','role':'Mapped road centreline; rendered width is illustrative'},
          {'id':'scenario-assumptions-v1','title':'Terra hypothetical management scenario','url':'','date':'2026-10-24','license':'project-original','role':'Sites, no-edit rules, costs, production, soil/storm and capacity assumptions'}]}
    world={'terrain':{'columns':119,'rows':127,'cellSizeM':10,'xMin':-590,'zMin':-630,'heights':[round(float(v),3) for v in height.ravel()]},
           'covers':[{'id':ident,'coverId':CLASS[code],'sourceId':'usgs-nlcd-2021','polygon':ring(p)} for ident,code,p in cover_polys],
           'features':[f for f,_ in mapped],'sites':sites}
    write(output/'manifest.json',manifest);write(output/'world.json',world)
    # Primary owns mission tuning after integration. Regeneration must never overwrite it.
    write(output/'scenario.seed.json',scenario)
    if not (output/'scenario.json').exists():write(output/'scenario.json',scenario)
    write(output/'source-features.json',[{'id':f['id'],'sourceId':f['sourceId'],'properties':p} for f,p in mapped])
    write(output/'site-clearances.json',safety)
    # Disjoint analysis units include every unchanged cover cell, minus curated site footprints.
    # GeometryCollection/MultiPolygon and interior rings are preserved in this offline GeoJSON,
    # while the frozen runtime v1 render layer stays convex and hole-free.
    site_union=unary_union([box(*s['bounds']) for s in templates]);units=[]
    for site in sites:
        geom=Polygon([(p['x'],p['z']) for p in site['polygon']])
        units.append({'type':'Feature','geometry':mapping(geom),'properties':{
            'id':site['id'],'siteId':site['id'],'areaM2':geom.area,'baselineCover':site['baselineCover'],
            'contributingAreaId':site['contributingAreaId'],'modelParameterId':site['modelParameterId'],
            'assumption':'Soil B; connection limited to this site only. Not a delineated catchment.'}})
    for ident,code,poly in cover_polys:
        remainder=poly.difference(site_union)
        if remainder.area<1e-6: continue
        units.append({'type':'Feature','geometry':mapping(remainder),'properties':{
            'id':'context-'+ident,'siteId':None,'areaM2':remainder.area,
            'baselineCover':[{'coverId':CLASS[code],'fraction':1}],
            'contributingAreaId':'isolated-'+ident,'modelParameterId':'cn-b-cover-assumed-v1',
            'assumption':'Unchanged context; no connection to a hypothetical retention site.'}})
    write(output/'analysis-units.geojson',{'type':'FeatureCollection','features':units})
    source_files=['x65y484.tif','x65y485.tif','nlcd-2021.tif','water.geojson','roads.geojson','connecting-roads.geojson','highways.geojson','elevation-metadata.json']
    inputs=[{'file':name,'bytes':(CACHE/name).stat().st_size,'sha256':hashlib.sha256((CACHE/name).read_bytes()).hexdigest()} for name in source_files]
    write(output/'provenance.json',{'displayName':'Meadowbrook Valley','realPlace':'Coon Valley / Brye Lane / Coon Creek, Wisconsin',
        'retrieved':'2026-10-24','originProjectedM':[OX,OZ],'exactEditablePolygon':ring(exact_editable),
        'contextWgs84Envelope':[-91.044,43.696,-91.030,43.707],
        'contextNote':'10 m local axis grid encloses the frozen WGS84 context; maximum extra corner margin 40.886 m. Editable sites remain strictly inside the exact frozen WGS84 polygon. Vector queries select features intersecting the frozen WGS84 context; wholly external features in the small extra corners are not inventoried.',
        'verticalExaggeration':1,'absoluteElevationRangeM':[float(height.min()+origin),float(height.max()+origin)],
        'processing':['DEM bilinear 1 m to 10 m vertices; no datum conversion or invented infill',
            'NLCD native 30 m cells projected corner-by-corner, clipped to context; original class retained by ID in cover-classes.json',
            'Vector lines projected, clipped and simplified at 1 m tolerance; MultiLineStrings split; no network inference',
            'Runtime covers are convex clipped source cells; auxiliary analysis geometry preserves holes and multipart shapes'],
        'classMapping':CLASS,'sources':manifest['sources'],'inputs':inputs,'requests':json.loads((CACHE/'requests.json').read_text()),
        'assumptions':['Meadowbrook Valley is a fictional game name for this real AOI.',
            'Rendered tree locations, individual vegetation, crop rows, road/water widths and animation are decorative, not surveyed.',
            'NLCD developed-open-space, shrub and pasture/hay map to grass; developed classes/barren map to sealed; woody wetland maps to woodland. This display grouping is not an imperviousness or ecological condition estimate.',
            'Hypothetical sites avoid mapped woodland/wetland and maintain 20 m water / 15 m road centreline clearance; scenario buffers are not legal setbacks.',
            'No-edit zones are educational assumptions. No ownership, legal conversion rights, farm condition or agricultural suitability is inferred.',
            'Soil B, 50 mm storm, illustrative production, demand, costs, site-only connection groups and retention capacity are uncalibrated game assumptions. Runoff remains disabled.',
            'No surveyed parcels, exact tree inventory, structures or local soil survey ingested; NLCD cannot resolve narrow field margins.',
            'Context crop is not a full watershed; no external inflow, flood depth, downstream flood protection or yield forecasts.']})
    write(output/'cover-classes.json',{ident:code for ident,code,_ in cover_polys})
    write(output/'control-points.json',control_points())
    overview(output/'alignment.png',sites)
    hashes={p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in sorted(output.iterdir()) if p.suffix in ['.json','.geojson']}
    write(output/'checksums.json', {k:v for k,v in hashes.items() if k!='checksums.json'})
    print(f'Built {len(cover_polys)} cover cells, {len(mapped)} mapped line parts, {len(sites)} sites, {len(units)} analysis units. Elevation origin {origin} m.')

if __name__=='__main__':
    parser=argparse.ArgumentParser();parser.add_argument('--output',type=__import__('pathlib').Path,default=PACK)
    build(parser.parse_args().output)
