#!/usr/bin/env python3
"""Package a verified v1 pack; default is the single real landscape."""
import argparse
import hashlib
import json
from pathlib import Path
import shutil
import subprocess
parser=argparse.ArgumentParser()
parser.add_argument('--pack', choices=['meadowbrook-v1','fixture-v1','point-reyes-v1','suusamyr-v1','all'], default='meadowbrook-v1')
args=parser.parse_args()
root=Path(__file__).resolve().parents[1]
if args.pack=='all':
    for pack in ['meadowbrook-v1','point-reyes-v1','suusamyr-v1']:
        subprocess.run(['python3',__file__,'--pack',pack],check=True)
    raise SystemExit(0)
source=root/'worlds'/args.pack
target=root/'Assets/StreamingAssets/WorldPacks'/args.pack
names=['manifest.json','world.json','scenario.json']
if args.pack=='fixture-v1':
    expected={name:digest for digest,name in [row.split() for row in (source/'checksums.sha256').read_text().splitlines()]}
else:
    expected=json.loads((source/'checksums.json').read_text())
for name in names:
    assert hashlib.sha256((source/name).read_bytes()).hexdigest()==expected[name],f'Pack checksum mismatch: {name}'
if args.pack=='meadowbrook-v1':
    subprocess.run(['python3',str(root/'scripts/prepare-hydrology.py'),'--check'],check=True)
    names.append('hydrology.json')
elif args.pack!='fixture-v1':
    names.append('hydrology.json')
    for name in names:
        assert hashlib.sha256((source/name).read_bytes()).hexdigest()==expected[name],f'Pack checksum mismatch: {name}'
target.mkdir(parents=True,exist_ok=True)
for name in names:shutil.copyfile(source/name,target/name)
print(f'Packaged {args.pack} / v1 ({len(names)} files, checksums verified).')
