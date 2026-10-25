#!/usr/bin/env python3
"""Import a bounded US community as georeferenced imagery + elevation, not inferred land use.

Run with the project's worldgen Python environment. Downloads are cached by request hash.
The output is a separate CommunityPack; it cannot overwrite a playable annual WorldPack.
"""
import argparse
import hashlib
import json
import math
from pathlib import Path
import re
import shutil
import tempfile
from datetime import datetime, timezone

import numpy as np
from PIL import Image
from pyproj import Transformer
import rasterio
import requests

ROOT = Path(__file__).resolve().parents[1]
NAIP = 'https://imagery.nationalmap.gov/arcgis/rest/services/USGSNAIPImagery/ImageServer'
DEM = 'https://elevation.nationalmap.gov/arcgis/rest/services/3DEPElevation/ImageServer'
LICENSE = 'https://www.usgs.gov/centers/eros/science/usgs-eros-archive-aerial-photography-national-agriculture-imagery-program-naip'


def write(path, data):
    path.write_text(json.dumps(data, indent=2, allow_nan=False) + '\n')


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def fetch(url, params, cache, suffix):
    request = requests.Request('GET', url, params=params).prepare().url
    key = hashlib.sha256(request.encode()).hexdigest()
    path = cache / (key + suffix)
    if not path.exists():
        response = requests.get(request, timeout=120, headers={'User-Agent': 'TerraCommunityImport/1.0'})
        response.raise_for_status()
        if 'json' in response.headers.get('Content-Type', ''):
            data = response.json()
            if 'error' in data:
                raise ValueError(f'Source rejected request: {data["error"]}')
        temporary = path.with_suffix('.part')
        temporary.write_bytes(response.content)
        temporary.replace(path)
    return path, request


def validate(folder):
    p = json.loads((folder / 'community.json').read_text())
    if p['schemaVersion'] != 1 or p['status'] != 'terrain-study':
        raise ValueError('Not a supported community terrain study')
    if not re.fullmatch(r'[a-z0-9]+(?:-[a-z0-9]+)*', p['id']):
        raise ValueError('Unsafe community ID')
    t = p['terrain']; h = np.array(t['heights'])
    if not all(math.isfinite(p[k]) for k in ('extentM','latitude','longitude','originElevationM','imageMetersPerPixel')) or not all(math.isfinite(t[k]) for k in ('cellSizeM','xMin','zMin')):
        raise ValueError('Non-finite geography metadata')
    if t['columns'] != t['rows'] or not 2 <= t['rows'] <= 257:
        raise ValueError('Invalid grid dimensions')
    if h.size != t['rows'] * t['columns'] or not np.isfinite(h).all():
        raise ValueError('Incomplete or non-finite elevation')
    if t['cellSizeM'] <= 0 or abs((t['rows'] - 1) * t['cellSizeM'] - p['extentM']) > .001:
        raise ValueError('Terrain/image extent mismatch')
    if p['rasterRows'] != 'south-to-north' or p['imageRows'] != 'north-to-south':
        raise ValueError('Unsupported orientation')
    image = Image.open(folder / 'imagery.jpg')
    if image.size != (p['imageWidth'], p['imageHeight']) or image.width != image.height:
        raise ValueError('Image dimensions mismatch')
    if np.asarray(image).std() < 3:
        raise ValueError('Blank imagery')
    hashes = json.loads((folder / 'checksums.json').read_text())
    if set(hashes) != {'community.json','imagery.jpg','imagery-catalog.json','imagery-service.json','provenance.json'}:
        raise ValueError('Incomplete checksum manifest')
    for name, expected in hashes.items():
        if Path(name).name != name or digest(folder / name) != expected:
            raise ValueError('Checksum mismatch: ' + name)
    return p


def build(args):
    if not re.fullmatch(r'[a-z0-9]+(?:-[a-z0-9]+)*', args.id):
        raise ValueError('ID must be lowercase letters, numbers and hyphens')
    if not (-125 <= args.lon <= -66 and 24 <= args.lat <= 50):
        raise ValueError('This adapter supports CONUS only. Research another licensed provider for this location.')
    if not 400 <= args.extent <= 2000 or args.extent % 10 or not 512 <= args.pixels <= 4000:
        raise ValueError('Extent must be 400–2000 m in 10 m increments; image 512–4000 pixels')
    output = args.output.resolve() / args.id
    if output.exists():
        raise ValueError('Output already exists; use a new versioned ID, never overwrite accepted geography')
    args.output.mkdir(parents=True, exist_ok=True)
    cache = ROOT / 'worldgen/cache/community'; cache.mkdir(parents=True, exist_ok=True)
    epsg = 32600 + int((args.lon + 180) // 6) + 1
    project = Transformer.from_crs(4326, epsg, always_xy=True)
    inverse = Transformer.from_crs(epsg, 4326, always_xy=True)
    cx, cy = project.transform(args.lon, args.lat)
    half = args.extent / 2
    bbox = [cx-half, cy-half, cx+half, cy+half]
    bounds = inverse.transform_bounds(*bbox)
    common = {'bbox': ','.join(map(str, bbox)), 'bboxSR': epsg, 'imageSR': epsg}
    service, service_url = fetch(NAIP, {'f': 'json'}, cache, '.json')
    catalog, catalog_url = fetch(NAIP + '/query', {
        'f': 'json', 'where': 'Category=1', 'geometry': common['bbox'],
        'geometryType': 'esriGeometryEnvelope', 'inSR': epsg, 'spatialRel': 'esriSpatialRelIntersects',
        'outFields': '*', 'returnGeometry': 'true', 'outSR': epsg}, cache, '.json')
    records = json.loads(catalog.read_text())
    if records.get('exceededTransferLimit') or not records.get('features'):
        raise ValueError('Missing or incomplete NAIP source catalog; do not fabricate source dates')
    # Lock the actual mosaic IDs so imagery cannot drift independently of its recorded source catalog.
    ids = [f['attributes']['OBJECTID'] for f in records['features']]
    mosaic = json.dumps({'mosaicMethod': 'esriMosaicLockRaster', 'lockRasterIds': ids, 'mosaicOperation': 'MT_FIRST'})
    meta, image_request = fetch(NAIP + '/exportImage', {
        **common, 'size': f'{args.pixels},{args.pixels}', 'format': 'jpg',
        'bandIds': '0,1,2', 'mosaicRule': mosaic, 'noData': '0', 'f': 'json',
        'adjustAspectRatio': 'false', 'interpolation': 'RSP_BilinearInterpolation'}, cache, '.json')
    export = json.loads(meta.read_text())
    for key, value in zip(('xmin','ymin','xmax','ymax'), bbox):
        if abs(export['extent'][key] - value) > .01:
            raise ValueError('Imagery service changed the requested extent')
    # Image hrefs are ephemeral; reuse bytes by stable export request, not an expiring URL.
    image_path = cache / (hashlib.sha256(image_request.encode()).hexdigest() + '.jpg')
    if not image_path.exists():
        response = requests.get(export['href'], timeout=120); response.raise_for_status()
        image_path.write_bytes(response.content)
    n = int(args.extent / 10) + 1
    # Pixel centers, not raster edges, coincide with mesh vertices. North is flipped exactly once.
    dem_bbox = [bbox[0]-5, bbox[1]-5, bbox[2]+5, bbox[3]+5]
    dem, dem_request = fetch(DEM + '/exportImage', {
        'bbox': ','.join(map(str, dem_bbox)), 'bboxSR': epsg, 'imageSR': epsg,
        'size': f'{n},{n}', 'format': 'tiff', 'pixelType': 'F32',
        'renderingRule': json.dumps({'rasterFunction': 'None'}), 'f': 'image',
        'interpolation': 'RSP_BilinearInterpolation', 'adjustAspectRatio': 'false'}, cache, '.tif')
    with rasterio.open(dem) as raster:
        heights = raster.read(1, masked=True)
        if raster.crs.to_epsg() != epsg or heights.shape != (n,n) or np.ma.getmaskarray(heights).any():
            raise ValueError('Elevation CRS/coverage mismatch')
        if max(abs(a-b) for a,b in zip(raster.xy(0,0), (bbox[0],bbox[3]))) > .02:
            raise ValueError('Elevation pixel centers are misaligned')
        heights = np.flipud(np.asarray(heights, dtype=float))
    if not np.isfinite(heights).all() or heights.min() < -500 or heights.max() > 9000:
        raise ValueError('Invalid elevation values')
    origin = math.floor(float(heights.min()))
    dates = sorted(set(str(f['attributes'].get('Year', 'unknown')) for f in records['features']))
    with tempfile.TemporaryDirectory(dir=args.output, prefix='.import-') as temp:
        folder = Path(temp)
        shutil.copyfile(image_path, folder / 'imagery.jpg')
        shutil.copyfile(catalog, folder / 'imagery-catalog.json')
        shutil.copyfile(service, folder / 'imagery-service.json')
        write(folder / 'community.json', {
            'schemaVersion': 1, 'id': args.id, 'name': args.name, 'region': args.region,
            'status': 'terrain-study', 'latitude': args.lat, 'longitude': args.lon,
            'extentM': args.extent, 'crs': f'EPSG:{epsg}', 'projectedOrigin': [cx,cy],
            'projectedBounds': bbox, 'boundsWgs84': list(bounds), 'originElevationM': origin,
            'rasterRows': 'south-to-north', 'imageRows': 'north-to-south',
            'imageWidth': args.pixels, 'imageHeight': args.pixels,
            'imageMetersPerPixel': args.extent / args.pixels,
            'imageYear': ' / '.join(dates), 'imageryKind': 'NAIP aerial orthophotography',
            'attribution': 'USDA NAIP / USGS The National Map · public domain',
            'terrain': {'columns': n, 'rows': n, 'cellSizeM': 10, 'xMin': -half, 'zMin': -half,
                        'heights': np.round(heights-origin,3).flatten().tolist()},
            'limitations': 'Photo draped on real elevation, not reconstructed buildings or trees. 10 m display grid; effective DEM detail varies. Photo and elevation dates differ. No inferred ownership, land use, hazards or local annual simulation.',
        })
        write(folder / 'provenance.json', {
            'retrievedUtc': datetime.now(timezone.utc).isoformat(), 'locationEvidence': args.location_source,
            'licenseUrl': LICENSE, 'serviceUrl': service_url, 'catalogUrl': catalog_url,
            'imageryRequest': image_request, 'elevationRequest': dem_request,
            'sourceHashes': {'imagery': digest(image_path), 'elevation': digest(dem), 'catalog': digest(catalog)},
            'processing': 'NAIP RGB mosaic locked to source IDs, reprojected by USGS to local UTM. 3DEP unrendered float elevation sampled at 10 m; north-to-south rows flipped. No height exaggeration. Image UVs share the exact projected extent.',
            'elevationDate': 'Mixed USGS 3DEP mosaic; acquisition date not resolved. Retrieval is not acquisition.',
            'imageExport': {k: export[k] for k in ('width','height','extent')},
        })
        write(folder / 'checksums.json', {p.name: digest(p) for p in sorted(folder.iterdir())})
        validate(folder)
        shutil.copytree(folder, output)
    print(f'Imported {args.name}: {args.extent} m, {args.extent/args.pixels:.3f} m/image pixel, elevation {heights.min():.1f}–{heights.max():.1f} m; {output}')


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--validate', type=Path)
    parser.add_argument('--id'); parser.add_argument('--name'); parser.add_argument('--region')
    parser.add_argument('--lat', type=float); parser.add_argument('--lon', type=float)
    parser.add_argument('--location-source')
    parser.add_argument('--extent', type=int, default=1200)
    parser.add_argument('--pixels', type=int, default=2048)
    parser.add_argument('--output', type=Path, default=ROOT/'worlds/communities')
    args = parser.parse_args()
    if args.validate:
        print('Validated ' + validate(args.validate)['id']); return
    if any(getattr(args, key) is None for key in ('id','name','region','lat','lon','location_source')):
        parser.error('Import requires --id, --name, --region, --lat, --lon and --location-source')
    build(args)


if __name__ == '__main__':
    main()
