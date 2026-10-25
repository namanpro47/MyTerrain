#!/usr/bin/env python3
"""Stage checksum-verified offline CommunityPacks for the Unity browser."""
import hashlib
import json
from pathlib import Path
import shutil

ROOT = Path(__file__).resolve().parents[1]


def stage(root=ROOT):
    source = root/'worlds/communities'
    target = root/'Assets/StreamingAssets/Communities'
    target.mkdir(parents=True, exist_ok=True)
    entries = []
    for folder in sorted(source.iterdir()):
        if not folder.is_dir() or folder.name.startswith('.'):
            continue
        data = json.loads((folder/'community.json').read_text())
        if data['id'] != folder.name or data['schemaVersion'] != 1:
            raise ValueError('Community identity/schema mismatch: ' + folder.name)
        hashes = json.loads((folder/'checksums.json').read_text())
        for name in ('community.json', 'imagery.jpg', 'provenance.json', 'imagery-catalog.json', 'imagery-service.json'):
            if hashlib.sha256((folder/name).read_bytes()).hexdigest() != hashes[name]:
                raise ValueError('Community checksum mismatch: ' + name)
        output = target/folder.name; output.mkdir(exist_ok=True)
        for name in ('community.json','imagery.jpg'):
            shutil.copyfile(folder/name, output/name)
        entries.append({key: data[key] for key in ('id','name','region','imageYear')})
    (target/'catalog.json').write_text(json.dumps({'schemaVersion':1,'places':entries},indent=2)+'\n')
    print(f'Staged {len(entries)} verified community terrain studies')


if __name__ == '__main__':
    stage()
