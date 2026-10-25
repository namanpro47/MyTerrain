#!/usr/bin/env python3
"""Validate shipped atlas references and regional source identities without a browser."""
import hashlib,json,pathlib,re
root=pathlib.Path(__file__).resolve().parents[1];atlas=root/'Assets/WebGLTemplates/Terra/atlas';checks=0

def check(value,why):
    global checks
    assert value,why;checks+=1
locations=['meadowbrook-v1','point-reyes-v1','suusamyr-v1'];keys=[];heights=[]
for id in locations:
    base=root/'worlds'/id;m=json.loads((base/'manifest.json').read_text());w=json.loads((base/'world.json').read_text());h=json.loads((base/'hydrology.json').read_text())
    check(m['packId']==id,'pack identity');check(hashlib.sha256((base/'world.json').read_bytes()).hexdigest()==h['worldSha256'],'hydrology world hash')
    check(abs(sum(c['areaM2'] for c in h['context'])+sum(s['analysisAreaM2'] for s in w['sites'])-h['analysisAreaM2'])<.2,'area conservation')
    heights.append(hashlib.sha256(json.dumps(w['terrain']['heights']).encode()).hexdigest())
    if id!='meadowbrook-v1':
        for name,digest in json.loads((base/'checksums.json').read_text()).items():check(hashlib.sha256((base/name).read_bytes()).hexdigest()==digest,'checksum '+name)
        check(next(l for l in m['layers'] if l['id']=='cover')['availability']=='authored','honest cover label')
        check(w['features']==[],'no invented mapped roads or water')
        check(len(json.loads((base/'provenance.json').read_text())['elevationTiles'])>0,'tile source receipts')
check(len(set(heights))==3,'three distinct terrains')
template=root/'Assets/WebGLTemplates/Terra'
check(not (template/'atlas').exists(),'no HTML atlas runtime')
check((template/'index.html').read_text().count('<canvas')==1,'one Unity canvas')
check('<title>MyTerrain</title>' in (template/'index.html').read_text(),'player brand')
check((root/'Assets/Terra/Resources/Atlas/Earth.jpg').stat().st_size>1000000,'native Earth texture')
for name in ['WorldAtlas.cs','WorldAtlas.Visuals.cs','TerraMain.Atlas.cs']:
    check((root/'Assets/Terra/Scripts/Gameplay'/name).exists(),'native atlas component '+name)
print(f'Native atlas package: {checks} checks passed.')
