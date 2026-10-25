#!/usr/bin/env python3
"""Verify the geometry behind the model's existing site/context partition (Shapely environment)."""
import json,subprocess,sys
from pathlib import Path
from shapely.geometry import shape,box
from shapely.ops import unary_union
root=Path(__file__).resolve().parents[1];pack=root/'worlds/meadowbrook-v1'
features=json.loads((pack/'analysis-units.geojson').read_text())['features']
polygons=[shape(f['geometry']) for f in features]
frame=box(*json.loads((pack/'manifest.json').read_text())['contextBounds']);union=unary_union(polygons)
gap=frame.difference(union).area;outside=union.difference(frame).area;overlap=sum(p.area for p in polygons)-union.area
assert gap<.1 and outside<.1 and abs(overlap)<.1
assert all(p.is_valid and abs(p.area-f['properties']['areaM2'])<1e-6 for p,f in zip(polygons,features))
subprocess.run([sys.executable,str(root/'scripts/prepare-hydrology.py'),'--check'],check=True)
report={'passed':True,'units':len(polygons),'areaM2':frame.area,'gapM2':gap,'outsideM2':outside,'overlapM2':overlap,'toleranceM2':.1,'method':'GEOS geometry union and declared-area checks; normalization matches source partition and world SHA256.'}
out=root/'docs/evidence/phase4/partition-check.json';out.parent.mkdir(parents=True,exist_ok=True);out.write_text(json.dumps(report,indent=2)+'\n');print(json.dumps(report))
