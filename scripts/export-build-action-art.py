"""Export Unity-generated art as glTF 2.0 and make a review sheet. Requires Pillow.

Run BuildActionAssetGenerator.Generate in Unity first. No downloaded models/images.
"""
import base64
import json
import math
from pathlib import Path
import struct

from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[1]
EVIDENCE = ROOT / "docs/evidence/build-action-art"
OUT = ROOT / "artifacts/build-actions"
KIT = ROOT / "Assets/Terra/Art/ExternalKit/BuildActions"


def linear(x):
    return x / 12.92 if x <= 0.04045 else ((x + 0.055) / 1.055) ** 2.4


def export_glb(model):
    binary = bytearray()
    doc = {"asset": {"version": "2.0", "generator": "Terra metric build action kit"},
           "scene": 0, "scenes": [{"nodes": [0]}], "nodes": [{"name": model["id"], "mesh": 0}],
           "meshes": [{"primitives": []}], "materials": [], "bufferViews": [], "accessors": []}

    def accessor(values, size, component, target, bounds=False):
        while len(binary) % 4:
            binary.append(0)
        offset = len(binary)
        fmt = "f" if component == 5126 else "I"
        binary.extend(struct.pack("<" + fmt * len(values), *values))
        view = len(doc["bufferViews"])
        doc["bufferViews"].append({"buffer": 0, "byteOffset": offset, "byteLength": len(binary)-offset, "target": target})
        data = {"bufferView": view, "componentType": component, "count": len(values)//size, "type": "VEC3" if size == 3 else "SCALAR"}
        if bounds:
            data["min"] = [min(values[i::size]) for i in range(size)]
            data["max"] = [max(values[i::size]) for i in range(size)]
        doc["accessors"].append(data)
        return len(doc["accessors"])-1

    for part in model["parts"]:
        vertices = [-v["x"] if axis == "x" else v[axis] for v in part["vertices"] for axis in ("x", "y", "z")]
        normals = [-v["x"] if axis == "x" else v[axis] for v in part["normals"] for axis in ("x", "y", "z")]
        # Unity's left-handed mesh becomes right-handed glTF: reflect X and reverse winding.
        source = part["indices"]
        indices = [source[i+j] for i in range(0, len(source), 3) for j in (0, 2, 1)]
        assert all(math.isfinite(x) for x in vertices + normals)
        assert len(normals) == len(vertices) and max(indices) < len(vertices)//3
        pos = accessor(vertices, 3, 5126, 34962, True)
        norm = accessor(normals, 3, 5126, 34962)
        idx = accessor(indices, 1, 5125, 34963)
        color = part["color"]
        doc["materials"].append({"name": part["name"], "pbrMetallicRoughness": {
            "baseColorFactor": [linear(color[k]) for k in ("r", "g", "b")] + [1], "metallicFactor": 0, "roughnessFactor": .82}})
        doc["meshes"][0]["primitives"].append({"attributes": {"POSITION": pos, "NORMAL": norm}, "indices": idx, "material": len(doc["materials"])-1})
    doc["buffers"] = [{"byteLength": len(binary)}]
    meta = json.dumps(doc, separators=(",", ":")).encode()
    meta += b" " * (-len(meta) % 4)
    binary += b"\0" * (-len(binary) % 4)
    result = struct.pack("<4sII", b"glTF", 2, 12 + 8 + len(meta) + 8 + len(binary))
    result += struct.pack("<I4s", len(meta), b"JSON") + meta + struct.pack("<I4s", len(binary), b"BIN\0") + binary
    path = OUT / "models" / (model["id"] + ".glb")
    path.write_bytes(result)
    # Independently read container headers and buffer bounds after writing.
    raw = path.read_bytes()
    magic, version, length = struct.unpack_from("<4sII", raw)
    assert magic == b"glTF" and version == 2 and length == len(raw)
    json_len, kind = struct.unpack_from("<I4s", raw, 12)
    assert kind == b"JSON"
    read = json.loads(raw[20:20+json_len])
    for view in read["bufferViews"]:
        assert view["byteOffset"] % 4 == 0 and view["byteOffset"] + view["byteLength"] <= read["buffers"][0]["byteLength"]
    return len(result)


def font(size, bold=False):
    for name in (["C:/Windows/Fonts/segoeuib.ttf"] if bold else ["C:/Windows/Fonts/segoeui.ttf"]):
        if Path(name).exists():
            return ImageFont.truetype(name, size)
    return ImageFont.load_default(size=size)


def contact_sheet(entries):
    width, height = 1600, 1470
    sheet = Image.new("RGB", (width, height), "#eff0e8")
    draw = ImageDraw.Draw(sheet)
    draw.text((44, 24), "TERRA / BUILD ACTIONS", font=font(36, True), fill="#263b33")
    draw.text((44, 75), "17 matching models + icons  /  real metre units  /  representative dimensions", font=font(20), fill="#52655a")
    draw.text((44, 105), "Icons are individually framed. The Unity gallery preserves their actual relative scale.", font=font(17), fill="#657167")
    for i, entry in enumerate(entries):
        x, y = 28 + (i % 5) * 314, 155 + (i // 5) * 320
        draw.rounded_rectangle((x, y, x+299, y+302), radius=18, fill="#fafbf5")
        icon = Image.open(KIT / "Icons" / (entry["actionId"] + ".png")).convert("RGBA")
        icon.thumbnail((272, 236))
        sheet.paste(icon, (x+(299-icon.width)//2, y+6), icon)
        title = entry["title"]
        if title == "Conservation / stewardship":
            title = "Conservation / stewardship"
        draw.text((x+16, y+243), title, font=font(16, True), fill="#263b33")
        size = entry["siteSizeMetres"]
        label = f'{size["x"]:g} × {size["y"]:g} m ' + ("sample" if entry["landscapeSample"] else "site")
        draw.text((x+16, y+269), label, font=font(15), fill="#657167")
    draw.text((680, 1230), "DIMENSIONS, NOT PREDICTIONS", font=font(22, True), fill="#263b33")
    draw.text((680, 1272), "No costs, output capacities or site approvals implied.\nArt kit only — playable-map integration is separate.", font=font(18), fill="#657167", spacing=10)
    sheet.save(EVIDENCE / "contact-sheet.png")


def review_page(entries):
    world = json.loads((ROOT / "worlds/meadowbrook-v1/world.json").read_text())
    data = []
    for entry in entries:
        data.append({**entry, "image": "data:image/png;base64," + base64.b64encode((KIT / "Icons" / (entry["actionId"]+".png")).read_bytes()).decode()})
    source = '''<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
<title>Terra · Build action art</title><style>
*{box-sizing:border-box}body{margin:0;background:#eff0e8;color:#263b33;font:16px system-ui}header{padding:30px 4vw 20px}h1{font-size:32px;margin:0 0 8px}p{max-width:850px;line-height:1.5}main{display:grid;grid-template-columns:1fr 410px;gap:24px;padding:0 4vw 40px}.cards{display:grid;grid-template-columns:repeat(auto-fill,minmax(180px,1fr));gap:12px}button{border:2px solid transparent;background:#fafbf5;color:inherit;border-radius:14px;padding:10px;cursor:pointer;font:inherit;text-align:left}button[aria-pressed=true]{border-color:#427761}button:focus-visible{outline:3px solid #e2a954}button img{width:100%;aspect-ratio:1;object-fit:contain}button strong,button small{display:block}button small{color:#617468;margin-top:5px}aside{position:sticky;top:16px;align-self:start;background:#fafbf5;padding:20px;border-radius:16px}canvas{width:100%;border:1px solid #d1dbcd;background:#e4e6d5}a{color:#215c49}label{display:block;margin:16px 0}input{width:100%}.hint{font-size:13px;color:#617468}#dimensions{font-weight:650} @media(max-width:900px){main{display:block}aside{position:static;margin-top:24px}}
</style><header><h1>Build actions, at real scale.</h1><p>17 matching 3D models and transparent icons from the PRD. Select a card to compare its land footprint with Meadowbrook Valley. Dimensions are representative design assumptions.</p></header>
<main><section class="cards" aria-label="Action asset catalog" id="cards"></section><aside><h2 id="title"></h2><p id="dimensions"></p><p id="notes"></p><p><a id="download">Download 3D model (.glb)</a></p><h3>Footprint against the real map</h3><canvas id="map" width="680" height="680" aria-label="Real map with a draggable footprint at one metre per world unit"></canvas><label>Rotate footprint <input id="rotation" type="range" min="0" max="180" value="0" aria-label="Footprint rotation"></label><p class="hint">Click or drag to move the orange footprint. This demonstrates scale only; it does not validate terrain, eligibility, setbacks or connections. Map: the project's real land-cover and site polygons. Icons above are individually framed.</p></aside></main>
<script>const assets=ASSETS, world=WORLD;let selected=assets[0],point={x:0,z:0};const canvas=document.querySelector('#map'),ctx=canvas.getContext('2d');const colors={woodland:'#789273',cultivated:'#c6b984',grass:'#acbd8b',water:'#80aab0',sealed:'#999c8e'};
function path(points){ctx.beginPath();points.forEach((p,i)=>{const x=(p.x+630)/1260*680,y=(630-p.z)/1260*680;i?ctx.lineTo(x,y):ctx.moveTo(x,y)});ctx.closePath()}
function render(){ctx.clearRect(0,0,680,680);for(const patch of world.covers){path(patch.polygon);ctx.fillStyle=colors[patch.coverId]||'#cbd3bb';ctx.fill()}for(const site of world.sites){path(site.polygon);ctx.strokeStyle=site.protectedSite?'#b88c92':'#5c7055';ctx.lineWidth=1;ctx.stroke()}let a=+document.querySelector('#rotation').value*Math.PI/180,w=selected.siteSizeMetres.x/2,d=selected.siteSizeMetres.y/2;let points=[[-w,-d],[w,-d],[w,d],[-w,d]].map(([x,z])=>({x:point.x+x*Math.cos(a)-z*Math.sin(a),z:point.z+x*Math.sin(a)+z*Math.cos(a)}));path(points);ctx.fillStyle='#ec9f3488';ctx.fill();ctx.strokeStyle='#8b4d14';ctx.lineWidth=2;ctx.stroke();ctx.fillStyle='#263b33';ctx.fillRect(24,636,100/1260*680,4);ctx.font='18px system-ui';ctx.fillText('100 m',24,626);}
function select(asset){selected=asset;document.querySelector('#title').textContent=asset.title;document.querySelector('#dimensions').textContent=`${asset.siteSizeMetres.x} × ${asset.siteSizeMetres.y} m ${asset.landscapeSample?'sample':'site'} · ${asset.triangles.toLocaleString()} triangles`;document.querySelector('#notes').textContent=asset.dimensionNotes;const link=document.querySelector('#download');link.href='models/'+asset.actionId+'.glb';link.download=asset.actionId+'.glb';document.querySelectorAll('.cards button').forEach(b=>b.setAttribute('aria-pressed',b.dataset.id===asset.actionId));render()}
for(const asset of assets){const button=document.createElement('button');button.dataset.id=asset.actionId;button.setAttribute('aria-pressed','false');const image=document.createElement('img');image.src=asset.image;image.alt='';const title=document.createElement('strong');title.textContent=asset.title;const dims=document.createElement('small');dims.textContent=`${asset.siteSizeMetres.x} × ${asset.siteSizeMetres.y} m`;button.append(image,title,dims);button.onclick=()=>select(asset);document.querySelector('#cards').append(button)}
function move(e){const r=canvas.getBoundingClientRect();point={x:(e.clientX-r.left)/r.width*1260-630,z:630-(e.clientY-r.top)/r.height*1260};render()}canvas.onpointerdown=e=>{canvas.setPointerCapture(e.pointerId);move(e)};canvas.onpointermove=e=>{if(e.buttons)move(e)};document.querySelector('#rotation').oninput=render;select(selected);</script></html>'''
    # This is a local art review page, not the playable game or a production site.
    source = source.replace("ASSETS", json.dumps(data)).replace("WORLD", json.dumps({"covers": world["covers"], "sites": world["sites"]}))
    (OUT / "index.html").write_text(source, encoding="utf-8")


def main():
    (OUT / "models").mkdir(parents=True, exist_ok=True)
    geometry = json.loads((ROOT / "Builds/BuildActionArt/geometry.json").read_text())
    entries = json.loads((EVIDENCE / "catalog.json").read_text())["entries"]
    assert {m["id"] for m in geometry["models"]} == {e["actionId"] for e in entries} and len(entries) == 17
    sizes = {model["id"]: export_glb(model) for model in geometry["models"]}
    contact_sheet(entries)
    review_page(entries)
    (EVIDENCE / "export-verification.json").write_text(json.dumps({"format": "glTF 2.0 binary", "metres": True, "models": sizes, "structuralChecksPassed": True, "note": "Container/accessor validation; separate Khronos validator and browser rendering are not claimed."}, indent=2))
    print(f"Exported {len(sizes)} GLBs, contact sheet and local map-scale review page ({sum(sizes.values()):,} model bytes).")


if __name__ == "__main__":
    main()
