#!/usr/bin/env python3
"""Verify the integrated B/D/C3 deliveries without controlling a Unity Editor."""
import argparse
import hashlib
import json
from pathlib import Path
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[1]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--phase3-core', type=Path, help='Read-only compatibility check against another checkout\'s WorldStateStore.cs')
    parser.add_argument('--output', type=Path, help='Write combined evidence JSON')
    args = parser.parse_args()
    commands = [
        ['dotnet', 'run', '--project', 'tests/simulation/SimulationChecks.csproj', '--', str(ROOT)],
        ['dotnet', 'build', 'tests/simulation/UnityApiCompatibility.csproj', '--nologo'],
        [sys.executable, 'scripts/verify-simulation-example.py'],
        [sys.executable, 'tests/education/validate_content.py'],
        [sys.executable, '-m', 'unittest', 'discover', '-s', 'tests/education', '-v'],
        [sys.executable, 'tests/audio-kit/verify_kit.py'],
    ]
    external_before = None
    if args.phase3_core:
        args.phase3_core = args.phase3_core.resolve(strict=True)
        external_before = hashlib.sha256(args.phase3_core.read_bytes()).hexdigest()
        commands.append([sys.executable, 'tests/education/validate_content.py', '--core-source', str(args.phase3_core)])
    results = []
    for command in commands:
        run = subprocess.run(command, cwd=ROOT, text=True, stdout=subprocess.PIPE, stderr=subprocess.STDOUT)
        results.append({'command': command, 'exitCode': run.returncode, 'output': run.stdout})
        print(('PASS' if run.returncode == 0 else 'FAIL') + ': ' + ' '.join(command), flush=True)
        if run.returncode:
            print(run.stdout, flush=True)
    external_after = hashlib.sha256(args.phase3_core.read_bytes()).hexdigest() if args.phase3_core else None
    report = {
        'passed': all(r['exitCode'] == 0 for r in results),
        'scope': 'Combined standalone package verification; Unity import is recorded separately',
        'commands': results,
        'alternateCore': {'path': str(args.phase3_core), 'sha256Before': external_before,
                          'sha256After': external_after, 'unchangedDuringRead': external_before == external_after} if args.phase3_core else None,
    }
    # A concurrent owner may legitimately change its file while these read-only checks run.
    # Report that as inconclusive compatibility evidence rather than editing or reverting it.
    if args.phase3_core and external_before != external_after:
        report['passed'] = False
        report['compatibilityNote'] = 'Alternate Core changed concurrently; rerun its read-only compatibility check.'
    if args.output:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        args.output.write_text(json.dumps(report, indent=2) + '\n')
    return 0 if report['passed'] else 1


if __name__ == '__main__':
    raise SystemExit(main())
