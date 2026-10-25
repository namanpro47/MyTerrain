"""Verify anonymous serving of the exact local export; browser checks remain required."""
import argparse
import gzip
import hashlib
import json
import ssl
import subprocess
import sys
from pathlib import Path
from urllib.request import urlopen

def decode_unity_payload(payload):
    """Keep verifying old gzip exports as well as Brotli release exports.

    Node's built-in decoder avoids adding a Python package just for release QA.
    It runs locally on bytes; it never executes anything from the export.
    """
    if payload.startswith(b"\x1f\x8b"):
        return "gzip", gzip.decompress(payload)
    try:
        decoded = subprocess.run(
            ["node", "-e", "process.stdout.write(require('node:zlib').brotliDecompressSync(require('node:fs').readFileSync(0)))"],
            input=payload, capture_output=True, check=True,
        ).stdout
    except FileNotFoundError as error:
        raise RuntimeError("Brotli export verification requires Node.js on PATH") from error
    except subprocess.CalledProcessError as error:
        raise ValueError("Invalid Brotli Unity payload") from error
    return "br", decoded


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("url")
    parser.add_argument("--build", default="Builds/Web")
    args = parser.parse_args()
    root = Path(args.build)
    context = ssl.create_default_context(cafile="/etc/ssl/cert.pem" if sys.platform == "darwin" else None)
    results = []
    served = [root / "index.html", *sorted((root / "Build").iterdir())]
    served += sorted(path for path in (root / "StreamingAssets").rglob("*") if path.is_file())
    served += sorted(path for path in (root / "atlas").rglob("*") if path.is_file())
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
            item["compression"], decoded = decode_unity_payload(actual)
            item["decodedBytes"] = len(decoded)
            if ".wasm." in relative:
                assert decoded[:8] == b"\0asm\x01\0\0\0", "Invalid WASM header"
        results.append(item)
    print(json.dumps({"url": args.url, "anonymous": True, "files": results}, indent=2))


if __name__ == "__main__":
    main()
