#!/usr/bin/env python3
"""Measure the four Unity payloads actually named by Terra's frozen wrapper.

This measures file bodies, not a cold-cache network request or load timing.
Optional map downloads and wrapper artwork are outside the Unity subtotal.
"""
import argparse
import hashlib
import json
from pathlib import Path
import re


def measure(export, source):
    root = export.resolve(strict=True)
    html = (root / "index.html").read_text()
    match = re.search(r"window\.TERRA_BUILD\s*=\s*\{([^}]+)\}", html)
    if not match:
        raise ValueError("Expected the frozen wrapper's window.TERRA_BUILD object")
    refs = dict(re.findall(r"\b(loader|data|framework|code)\s*:\s*['\"]([^'\"]+)['\"]", match[1]))
    if set(refs) != {"loader", "data", "framework", "code"}:
        raise ValueError("Wrapper must identify all four Unity payloads")
    files = []
    for role in ("loader", "data", "framework", "code"):
        path = (root / refs[role]).resolve(strict=True)
        relative = path.relative_to(root)
        body = path.read_bytes()
        files.append({"role": role, "path": relative.as_posix(), "bytes": len(body),
                      "sha256": hashlib.sha256(body).hexdigest()})
    if len({f["path"] for f in files}) != 4:
        raise ValueError("Unity payload references must be distinct")
    total = sum(f["bytes"] for f in files)
    return {"source": source, "export": str(root), "requiredUnityPayloadBytes": total,
            "budgetBytes": 50_000_000, "bytesOverBudget": max(0, total - 50_000_000),
            "belowBudget": total < 50_000_000, "files": files,
            "note": "File-body bytes for wrapper-referenced Unity payloads only. Excludes wrapper, artwork, map downloads, HTTP headers, cache effects and timing. A subtotal below budget does not prove the full initial-transfer gate."}


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("export", type=Path)
    parser.add_argument("--source", required=True, help="Recorded source commit of this export")
    args = parser.parse_args()
    print(json.dumps(measure(args.export, args.source), indent=2))
