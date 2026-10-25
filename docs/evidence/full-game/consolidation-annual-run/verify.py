"""Compare actual F10 Chrome receipts with the committed strategy fixture."""
import json
import re
from pathlib import Path

here = Path(__file__).resolve().parent
root = here.parents[3]
fixture_path = root / 'docs/evidence/hud-connected/community-and-services.json'
fixture = json.loads(fixture_path.read_text())
logs = json.loads((here / 'final-state-logs.json').read_text())
pattern = re.compile(r'year=(\d+); phase=(\w+); revision=(\d+); cash=(\d+); queued=(\d+); assets=(\d+)')
rows = []
for entry in logs:
    match = pattern.search(entry['message'])
    assert match, entry
    y, phase, rev, cash, queued, assets = match.groups()
    rows.append(dict(timestamp=entry['timestamp'], year=int(y), phase=phase,
                     revision=int(rev), cashCents=int(cash), queued=int(queued), assets=int(assets)))
checks = []
def check(name, condition):
    checks.append({'name': name, 'pass': bool(condition)})
    assert condition, name

accepted = rows[:10]
check('Ten sequential manually accepted years', [r['year'] for r in accepted] == list(range(1, 11)))
for row, expected in zip(accepted, fixture['history']):
    check(f"Year {row['year']} cash", row['cashCents'] == expected['ledger']['closingCashCents'])
    check(f"Year {row['year']} asset count", row['assets'] == sum(a['startedYear'] <= row['year'] for a in fixture['assets']))
    check(f"Year {row['year']} phase and empty queue", row['queued'] == 0 and row['phase'] == ('Completed' if row['year'] == 10 else 'Reviewing'))
final = accepted[-1]
check('Completed revision matches strategy', final['revision'] == fixture['revision'])
for label, row in zip(['Opening comparison', 'Restored future', 'Browser reload'], rows[10:13]):
    check(label + ' preserves diagnostic state', all(row[k] == final[k] for k in ['year', 'phase', 'revision', 'cashCents', 'queued', 'assets']))
check('All three persistence observations exist', len(rows[10:13]) == 3)
check('Confirmed replay resets seven-project run', rows[-1]['year'] == 1 and rows[-1]['phase'] == 'Planning' and rows[-1]['revision'] == 0 and rows[-1]['cashCents'] == 1000000000 and rows[-1]['assets'] == 0 and rows[-1]['queued'] == 0)
check('No captured error-level browser logs', json.loads((here / 'pre-reload-errors.json').read_text()) == [] and json.loads((here / 'final-errors.json').read_text()) == [])
result = {'source': '2f8543dd2734172942f92c1e3395b695174395e9', 'fixture': str(fixture_path.relative_to(root)), 'browser': 'Chrome 155.0.8059.85', 'viewport': [1280, 720], 'mode': 'actual controls, ten manual End Year submissions, labelled offline chapters', 'checks': checks, 'observations': rows,
          'visualEndingObservation': {'cashCents': 524000000, 'futureCommitmentsCents': 79700000, 'arrearsCents': 0, 'displayedScores': [86.2, 99.0, 52.1], 'evidence': 'year10-ending.jpg'},
          'limits': ['Local frozen provisional candidate; not final public release', 'Diagnostic logs expose summary state, not a complete saved-state JSON comparison', 'No fast-forward, timing benchmark, novice session or timed rehearsal claimed', 'Visual acceptance remains open']}
(here / 'verification.json').write_text(json.dumps(result, indent=2) + '\n')
print(f'{len(checks)} receipt comparisons passed')
