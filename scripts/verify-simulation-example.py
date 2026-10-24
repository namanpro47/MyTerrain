#!/usr/bin/env python3
"""Check frozen simulation I/O with an independent scalar reference, no Unity scene."""
import json
import math
from pathlib import Path
root = Path(__file__).resolve().parents[1]
fixture = root / 'tests/fixtures/simulation'
inputs = json.loads((fixture / 'input.json').read_text())
expected = json.loads((fixture / 'expected.json').read_text())
assert inputs['schemaVersion'] == expected['schemaVersion'] == 1
assert inputs['revision'] == expected['revision']
rain = inputs['storm']['rainfallMm']
def runoff(p, cn):
    assert math.isfinite(p) and p >= 0 and math.isfinite(cn) and 0 < cn <= 100
    s = 25400 / cn - 254
    return 0 if p <= .2*s else (p - .2*s)**2 / (p + .8*s)
generated = {s['siteId']: runoff(rain, s['curveNumber'])*s['areaM2']/1000 for s in inputs['sites']}
groups = {}
for s in inputs['sites']:
    g = groups.setdefault(s['contributingAreaId'], {'generated': 0, 'capacity': 0})
    g['generated'] += generated[s['siteId']]
    g['capacity'] += s['retentionCapacityM3']
retained = {}
for s in inputs['sites']:
    g = groups[s['contributingAreaId']]
    retained[s['siteId']] = min(g['generated'], g['capacity'])*generated[s['siteId']]/g['generated'] if g['generated'] else 0
for result in expected['sites']:
    id = result['siteId']
    assert abs(result['generatedM3'] - generated[id]) < 1e-5
    assert abs(result['retainedM3'] - retained[id]) < 1e-5
    assert abs(result['dischargedM3'] - (generated[id]-retained[id])) < 1e-5
assert abs(expected['generatedM3'] - sum(generated.values())) < 1e-5
assert abs(expected['retainedM3'] - sum(retained.values())) < 1e-5
assert abs(expected['dischargedM3'] - sum(generated.values()) + sum(retained.values())) < 1e-5
assert retained['c'] == 0, 'Disconnected western site cannot benefit from eastern capacity'
assert runoff(0, 78) == 0 and runoff(10, 61) == 0
assert runoff(50, 61) < runoff(50, 78)
print(json.dumps({'passed': True, 'fixtureVersion': '1.0.0', 'generatedM3': sum(generated.values()), 'retainedM3': sum(retained.values()), 'disconnectedSiteRetainedM3': retained['c'], 'note': 'Scalar interface fixture check, not an integrated simulator'}, indent=2))
