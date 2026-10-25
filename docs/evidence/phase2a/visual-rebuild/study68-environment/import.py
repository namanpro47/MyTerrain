"""Run only with this worktree's editor + unity-heavy reservation, Editor closed."""
from pathlib import Path
import json
import shutil

source = Path('Builds/ArtResearch/Study68/Extracted/Assets/Assets/Stylized Asia RG')
target = Path('Assets/Terra/Art/RGPolyStudy')
remap = json.loads(Path('docs/evidence/phase2a/visual-rebuild/study68-environment/guid-remap.json').read_text())
if target.exists():
    raise SystemExit('Study directory already exists; inspect rather than overwrite')
shutil.copytree(source, target)
changed = []
for path in target.rglob('*'):
    if not path.is_file():
        continue
    contents = path.read_bytes()
    if contents.startswith(b'%YAML') or path.suffix in ('.meta', '.shadergraph', '.shadersubgraph'):
        revised = contents.replace(remap['originalSceneGuid'].encode(), remap['studySceneGuid'].encode())
        if revised != contents:
            path.write_bytes(revised)
            changed.append(str(path))
print(json.dumps({'importedInto': str(target), 'guidRemappedFiles': changed}))
