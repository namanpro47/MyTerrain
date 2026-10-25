"""Stage exact frozen model/data sources; run from repository root."""
from pathlib import Path
import hashlib
import json
import subprocess
import sys

source = "e74496bf55ffa77427be69dcb221bb3bd70f3b41"
evidence = Path("docs/evidence/annual-completion/dense-v5")
stress = "--stress" in sys.argv
output = Path("Builds/DenseV5Stress" if stress else "Builds/DenseV5Acceptance")
output.mkdir(parents=True, exist_ok=True)
paths = subprocess.check_output(["git", "ls-tree", "-r", "--name-only", source]).decode().splitlines()
selected = [p for p in paths if (
    p.startswith(("Assets/Terra/Scripts/Core/", "Assets/Terra/Scripts/Contracts/", "Assets/Terra/Scripts/Simulation/")) and p.endswith(".cs")) or
    (p.startswith(("worlds/meadowbrook-v1/", "worlds/point-reyes-v1/", "worlds/suusamyr-v1/", "Assets/Terra/Resources/Parcels/")) and p.endswith(".json")) or
    p == "Assets/Terra/Art/ExternalKit/BuildActions/BuildActionCatalog.asset"]
rows = []
for path in selected:
    body = subprocess.check_output(["git", "show", source + ":" + path])
    target = output / path
    target.parent.mkdir(parents=True, exist_ok=True)
    target.write_bytes(body)
    rows.append({"path": path, "bytes": len(body), "sha256": hashlib.sha256(body).hexdigest()})
program = (evidence / ("StressProgram.cs.txt" if stress else "Program.cs.txt")).read_bytes()
(output / "Program.cs").write_bytes(program)
(output / "DenseV5Acceptance.csproj").write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings></PropertyGroup></Project>\n')
(evidence / ("stress-source.json" if stress else "source.json")).write_text(json.dumps({"source": source, "programSha256": hashlib.sha256(program).hexdigest(), "files": rows}, indent=2) + "\n")
print(f"Staged {len(rows)} pinned files in {output}")
