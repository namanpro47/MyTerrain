#!/usr/bin/env python3
"""Package the one technical fixture. This is not a geographic data generator."""
import hashlib
from pathlib import Path
import shutil
root = Path(__file__).resolve().parents[1]
source = root / 'worlds/fixture-v1'
target = root / 'Assets/StreamingAssets/WorldPacks/fixture-v1'
expected = {}
for row in (source/'checksums.sha256').read_text().splitlines():
    digest, name = row.split()
    expected[name] = digest
assert set(expected) == {'manifest.json', 'world.json', 'scenario.json'}
for name, digest in expected.items():
    assert hashlib.sha256((source/name).read_bytes()).hexdigest() == digest, f'Fixture checksum mismatch: {name}'
target.mkdir(parents=True, exist_ok=True)
for name in expected:
    shutil.copyfile(source/name, target/name)
print('Packaged fixture-v1 / 1.0.0 (3 files, checksums verified).')
