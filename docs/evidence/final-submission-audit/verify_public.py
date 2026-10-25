"""Independently compare a frozen static export with its public aliases."""
import argparse
import concurrent.futures
import hashlib
import json
from pathlib import Path
import ssl
import subprocess
import sys
from urllib.parse import quote
from urllib.request import urlopen

parser = argparse.ArgumentParser()
parser.add_argument("--build", type=Path, required=True)
parser.add_argument("--output", type=Path, required=True)
parser.add_argument("urls", nargs="+")
args = parser.parse_args()
root = args.build.resolve()
paths = [p for p in sorted(root.rglob("*")) if p.is_file()
         and not any(part.startswith(".") for part in p.relative_to(root).parts)
         and p.relative_to(root).parts[0] != "api"
         and p.name not in ("vercel.json", "package.json", "package-lock.json")]
context = ssl.create_default_context(cafile="/etc/ssl/cert.pem" if sys.platform == "darwin" else None)

def verify(url):
    rows = []
    for path in paths:
        relative = path.relative_to(root).as_posix()
        expected = path.read_bytes()
        with urlopen(url.rstrip("/") + "/" + quote(relative), timeout=60,
                     context=context) as response:
            actual = response.read()
            if response.status != 200 or actual != expected:
                raise ValueError(f"Public content differs: {url}/{relative}")
        row = {"path": relative, "bytes": len(actual),
               "sha256": hashlib.sha256(actual).hexdigest()}
        if relative.endswith(".unityweb"):
            result = subprocess.run(["node", "-e",
                "const b=require('node:fs').readFileSync(0),z=require('node:zlib');"
                "const gzip=b[0]===31&&b[1]===139;"
                "const d=gzip?z.gunzipSync(b):z.brotliDecompressSync(b);"
                "process.stdout.write(JSON.stringify({compression:gzip?'gzip':'br',"
                "decodedBytes:d.length,header:d.subarray(0,8).toString('hex')}));"],
                input=actual, capture_output=True, check=True)
            decoded = json.loads(result.stdout)
            if ".wasm." in relative and decoded["header"] != "0061736d01000000":
                raise ValueError(f"Invalid decoded WebAssembly header: {relative}")
            row.update(decoded)
        rows.append(row)
    return {"url": url, "anonymous": True, "files": rows,
            "totalBytes": sum(row["bytes"] for row in rows)}

with concurrent.futures.ThreadPoolExecutor(max_workers=2) as pool:
    results = list(pool.map(verify, args.urls))
args.output.write_text(json.dumps({"export": str(root), "results": results}, indent=2) + "\n")
print(json.dumps([{"url": row["url"], "files": len(row["files"]),
                   "bytes": row["totalBytes"]} for row in results]))
