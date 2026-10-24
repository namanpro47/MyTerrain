"""Verify anonymous serving of the exact local export; browser checks remain required."""
import argparse
import gzip
import hashlib
import json
import ssl
import sys
from pathlib import Path
from urllib.request import urlopen

parser = argparse.ArgumentParser()
parser.add_argument("url")
parser.add_argument("--build", default="Builds/Web")
args = parser.parse_args()
root = Path(args.build)
context = ssl.create_default_context(cafile="/etc/ssl/cert.pem" if sys.platform == "darwin" else None)
results = []
served = [root / "index.html", *sorted((root / "Build").iterdir())]
served += sorted(path for path in (root / "StreamingAssets").rglob("*") if path.is_file())
for path in served:
    relative = path.relative_to(root).as_posix()
    expected = path.read_bytes()
    with urlopen(args.url.rstrip("/") + "/" + relative, timeout=60, context=context) as response:
        actual = response.read()
        assert response.status == 200, (relative, response.status)
        assert actual == expected, f"Response differs from export: {relative}"
        item = {"file": relative, "status": response.status, "bytes": len(actual),
                "sha256": hashlib.sha256(actual).hexdigest(),
                "contentType": response.headers.get("Content-Type"),
                "contentEncoding": response.headers.get("Content-Encoding"),
                "cacheControl": response.headers.get("Cache-Control")}
    if relative.endswith(".unityweb"):
        decoded = gzip.decompress(actual)
        item["decodedBytes"] = len(decoded)
        if ".wasm." in relative:
            assert decoded[:8] == b"\0asm\x01\0\0\0", "Invalid WASM header"
    results.append(item)
print(json.dumps({"url": args.url, "anonymous": True, "files": results}, indent=2))
