#!/usr/bin/env python3
"""Stage the actual WebGL template around a recorded local player for UI iteration.
This does not rebuild Unity; use TerraBuild.BuildWeb for the integrated release.
"""
import argparse, pathlib, re, shutil
p=argparse.ArgumentParser();p.add_argument('--player',required=True);p.add_argument('--output',default='Builds/AtlasPreview');a=p.parse_args()
root=pathlib.Path(__file__).resolve().parents[1];player=pathlib.Path(a.player).resolve();out=(root/a.output).resolve();out.mkdir(parents=True,exist_ok=True)
source=root/'Assets/WebGLTemplates/Terra';html=(source/'index.html').read_text();existing=(player/'index.html').read_text()
for token,suffix in [('LOADER_FILENAME','loader.js'),('DATA_FILENAME','data.unityweb'),('FRAMEWORK_FILENAME','framework.js.unityweb'),('CODE_FILENAME','wasm.unityweb')]:
    files=list((player/'Build').glob('*.'+suffix));assert len(files)==1,(token,files)
    html=html.replace('{{{ '+token+' }}}',files[0].name)
(out/'index.html').write_text(html)
for name,target in [('atlas',source/'atlas'),('Build',player/'Build'),('StreamingAssets',player/'StreamingAssets')]:
    dest=out/name
    if dest.is_symlink():dest.unlink()
    if not dest.exists():dest.symlink_to(target,target_is_directory=True)
print(out)
