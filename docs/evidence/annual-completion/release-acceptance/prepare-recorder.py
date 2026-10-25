#!/usr/bin/env python3
"""Make a separate local recording copy of an immutable, already-verified export."""
import argparse
import hashlib
import json
from pathlib import Path
import re
import shutil

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('export', type=Path)
parser.add_argument('output', type=Path)
parser.add_argument('--source', required=True, help='Verified source Git SHA (7–40 hexadecimal characters)')
args = parser.parse_args()
original, output = args.export.resolve(), args.output.resolve()
if not re.fullmatch(r'[0-9a-f]{7,40}', args.source):
    parser.error('Source must be the verified hexadecimal Git SHA.')
if not (original / 'index.html').is_file() or not (original / 'Build').is_dir():
    parser.error('Export must contain index.html and the Unity Build directory.')
if output == original or original in output.parents or output.exists():
    parser.error('Choose a new output directory outside the immutable source export.')
html = (original / 'index.html').read_text()
if '</body>' not in html or 'record-local.js' in html:
    parser.error('Expected an unmodified native player HTML document.')

def fingerprint(root):
    return {p.relative_to(root).as_posix(): {'bytes': p.stat().st_size, 'sha256': hashlib.sha256(p.read_bytes()).hexdigest()}
            for p in sorted(root.rglob('*')) if p.is_file()}

before = fingerprint(original)
shutil.copytree(original, output)
recorder = Path(__file__).with_name('record-local.js').read_text().replace('__TERRA_SOURCE__', args.source[:7])
(output / 'record-local.js').write_text(recorder)
(output / 'index.html').write_text(html.replace('</body>', '<script src="record-local.js"></script></body>'))
after = fingerprint(output)
unchanged = [name for name, info in before.items() if after.get(name) == info]
assert len(unchanged) == len(before) - 1 and 'index.html' not in unchanged
assert fingerprint(original) == before, 'Immutable export changed unexpectedly.'
manifest = {'source': args.source, 'purpose': 'Local silent canvas recording only; not a public deployment',
            'originalExport': str(original), 'originalFiles': before, 'wrapperFiles': after,
            'changedOriginalFiles': ['index.html'], 'addedFiles': ['record-local.js'],
            'unchangedOriginalFiles': unchanged,
            'acceptance': 'Prepared only; capture, download, decoding and content inspection pending'}
(output / 'recording-wrapper-manifest.json').write_text(json.dumps(manifest, indent=2)+'\n')
print(json.dumps({'output': str(output), 'unchangedOriginalFiles': len(unchanged), 'source': args.source}))
