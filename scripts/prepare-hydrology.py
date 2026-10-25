#!/usr/bin/env python3
"""Compile the existing disjoint geography partition into a small runtime model configuration."""
import hashlib, json, collections, argparse
from pathlib import Path
root=Path(__file__).resolve().parents[1]
pack=root/'worlds/meadowbrook-v1'
world=json.loads((pack/'world.json').read_text())
analysis=json.loads((pack/'analysis-units.geojson').read_text())['features']
site_by_id={s['id']:s for s in world['sites']}
seen=set(); context=collections.defaultdict(float)
for feature in analysis:
    p=feature['properties']; sid=p['siteId']
    if sid:
        assert sid not in seen and sid in site_by_id
        seen.add(sid); site=site_by_id[sid]
        assert abs(site['analysisAreaM2']-p['areaM2'])<.01 and site['baselineCover']==p['baselineCover']
    else:
        assert len(p['baselineCover'])==1 and p['baselineCover'][0]['fraction']==1
        context[p['baselineCover'][0]['coverId']]+=p['areaM2']
assert seen==set(site_by_id)
manifest=json.loads((pack/'manifest.json').read_text()); b=manifest['contextBounds']; area=(b[2]-b[0])*(b[3]-b[1])
assert abs(sum(context.values())+sum(s['analysisAreaM2'] for s in world['sites'])-area)<.1
# These disjoint connections are educational scenario assumptions, not inferred drainage.
south={'south-04','south-05','south-07','south-08'}
east={'south-03','south-06','east-09'}
config={'schemaVersion':1,'modelVersion':'terra-landscape-runoff-v1','worldSha256':hashlib.sha256((pack/'world.json').read_bytes()).hexdigest(),
 'partitionSha256':hashlib.sha256((pack/'analysis-units.geojson').read_bytes()).hexdigest(),'analysisAreaM2':area,
 'sourceId':'usda-tr55-1986','sourceUrl':'https://www.hec.usace.army.mil/confluence/hmsdocs/hmstrm/cn-tables',
 'assumptionSourceId':'scenario-assumptions-v1','connectionNote':'Hypothetical groups; not measured drainage. Unchanged context is isolated from devices.',
 'parameters':[{'coverId':k,'curveNumber':v} for k,v in [('cultivated',78),('grass',61),('woodland',55),('sealed',98)]],
 'context':[{'id':'context-'+k,'coverId':k,'areaM2':round(v,6)} for k,v in sorted(context.items())],
 'connections':[{'siteId':s['id'],'groupId':'southern-plots' if s['id'] in south else 'eastern-plots' if s['id'] in east else 'isolated-'+s['id']} for s in world['sites']]}
out=json.dumps(config,indent=2,allow_nan=False)+'\n'
parser=argparse.ArgumentParser();parser.add_argument('--check',action='store_true');args=parser.parse_args()
if args.check:assert (pack/'hydrology.json').read_text()==out,'Hydrology configuration is stale; run scripts/prepare-hydrology.py'
else:(pack/'hydrology.json').write_text(out)
print(f'Hydrology partition: {len(seen)} sites + {len(context)} unchanged cover aggregates = {area:.0f} m². '+('Verified.' if args.check else 'Written.'))
