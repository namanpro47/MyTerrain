#!/usr/bin/env python3
"""Validate package D against the repository's actual frozen C# DTO declarations.

Standard library only. This checks data/adapter contracts, not Unity JsonUtility,
UI layout, educational efficacy or runtime mission integration.
"""
import argparse
import json
import math
from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parents[2]
TOKEN = re.compile(r'\{([A-Za-z][A-Za-z0-9]*)\}')
ID = re.compile(r'[a-z][a-z0-9-]*\Z')
FORMATS = {'units', 'signedUnits', 'integer', 'volume'}
GUARDS = {'missionConfigured', 'currentResultAvailable', 'matchedRunoffAvailable',
          'runoffAvailable', 'objectivesMet', 'continuePreservesProposal', 'retryAvailable'}


def require(condition, message):
    if not condition:
        raise ValueError(message)


def read_json(path):
    def unique_keys(pairs):
        result = {}
        for key, value in pairs:
            require(key not in result, f'{path}: duplicate JSON key {key}')
            result[key] = value
        return result
    return json.loads(path.read_text(encoding='utf-8'), object_pairs_hook=unique_keys)


def dto_fields(source, name):
    match = re.search(r'public sealed class ' + name + r'\s*\{([^}]+)\}', source)
    require(match is not None, f'Missing actual DTO: {name}')
    return {field.strip(): kind for kind, fields in
            re.findall(r'public\s+(\w+(?:\[\])?)\s+([^;]+);', match[1])
            for field in fields.split(',')}


def tokens(text):
    found = TOKEN.findall(text)
    require('{' not in TOKEN.sub('', text) and '}' not in TOKEN.sub('', text),
            'Malformed placeholder syntax')
    return set(found)


def validate_entry(entry, sources, placeholders, limits, entry_fields, choice_fields):
    require(isinstance(entry, dict) and set(entry) == set(entry_fields), 'ContentEntry fields differ from actual DTO')
    for field in ('id', 'title', 'body'):
        require(isinstance(entry[field], str) and entry[field].strip() == entry[field] and entry[field], f'Invalid {field}')
    require(ID.fullmatch(entry['id']), 'Invalid stable content ID')
    for field in ('title', 'body'):
        require(len(entry[field]) <= limits[field], f'{entry["id"]}: {field} too long')
        require('<' not in entry[field] and '>' not in entry[field], 'Content must be plain text')
        require(tokens(entry[field]) <= set(placeholders), f'{entry["id"]}: unknown placeholder')
    require(not tokens(entry['title']), 'Titles must not contain placeholders')
    require(isinstance(entry['sourceIds'], list) and entry['sourceIds'], 'Missing sourceIds')
    require(all(isinstance(s, str) and s in sources for s in entry['sourceIds']), 'Unknown source reference')
    require(len(set(entry['sourceIds'])) == len(entry['sourceIds']), 'Duplicate source reference')
    require(isinstance(entry['choices'], list), 'choices must be an array')
    seen = set()
    for choice in entry['choices']:
        require(isinstance(choice, dict) and set(choice) == set(choice_fields), 'ContentChoice fields differ from actual DTO')
        require(type(choice['correct']) is bool, 'correct must be a JSON boolean')
        require(isinstance(choice['id'], str) and ID.fullmatch(choice['id']), 'Invalid stable choice ID')
        require(choice['id'] not in seen, 'Duplicate choice ID')
        seen.add(choice['id'])
        for field, limit in [('text', 'choiceText'), ('explanation', 'choiceExplanation')]:
            value = choice[field]
            require(isinstance(value, str) and value.strip() == value and value, f'Missing choice {field}')
            require(len(value) <= limits[limit], f'Choice {field} too long')
            require(not tokens(value), 'Choices must not contain placeholders')
            require('<' not in value and '>' not in value, 'Choices must be plain text')


def render_body(entry_id, entries, bindings, values, guards):
    """Reference adapter oracle; values are raw numbers from the stated DTO instance.

    False availability, missing or invalid numeric inputs select a whole-entry
    fallback. Zero is valid if the availability guard explicitly passes.
    """
    entry = entries[entry_id]
    names = tokens(entry['body'])
    capability = bindings['capabilityGuards'].get(entry_id)
    unavailable = capability is not None and guards.get(capability) is not True
    for name in names:
        spec = bindings['placeholders'][name]
        value = values.get(name)
        valid = type(value) in (int, float) and math.isfinite(value)
        if valid and spec['format'] == 'integer':
            valid = float(value).is_integer()
        if valid and spec['format'] != 'signedUnits':
            valid = value >= 0
        unavailable |= guards.get(spec['guard']) is not True or not valid
    if unavailable:
        fallback = bindings['fallbacks'].get(entry_id)
        return entries[fallback]['body'] if fallback else None  # Hide guarded action/copy.
    def substitute(match):
        name = match[1]
        value = values[name]
        format_ = bindings['placeholders'][name]['format']
        if format_ == 'integer':
            return f'{value:,.0f}'
        if format_ == 'signedUnits':
            return f'{value:+,.1f}' if value else '0.0'
        return f'{value:,.1f}'
    body = TOKEN.sub(substitute, entry['body'])
    require(len(body) <= bindings['characterLimits']['renderedBody'], 'Rendered body exceeds copy budget')
    return body


def validate_package(root=ROOT, core_source=None):
    folder = root / 'content/education'
    bindings = read_json(folder / 'bindings.json')
    require(bindings['packageVersion'] == 1 and bindings['contract'] == 'ContentEntry v1', 'Unsupported content package version')
    manifest = read_json(root / 'worlds/meadowbrook-v1/manifest.json')
    scenario = read_json(root / 'worlds/meadowbrook-v1/scenario.json')
    source = (root / 'Assets/Terra/Scripts/Contracts/WorldContracts.cs').read_text()
    entry_fields, choice_fields = dto_fields(source, 'ContentEntry'), dto_fields(source, 'ContentChoice')
    require(entry_fields == {'id':'string','title':'string','body':'string','choices':'ContentChoice[]','sourceIds':'string[]'}, 'ContentEntry DTO changed; review adapter')
    require(choice_fields == {'id':'string','text':'string','explanation':'string','correct':'bool'}, 'ContentChoice DTO changed; review adapter')
    require(bindings['packId'] == manifest['packId'] == scenario['packId'], 'Wrong pack')
    require(bindings['selectedLocationId'] == manifest['selectedLocationId'], 'Wrong location')
    sources = {s['id'] for s in manifest['sources']}
    limits = bindings['characterLimits']
    for name, spec in bindings['placeholders'].items():
        require(re.fullmatch(r'[A-Za-z][A-Za-z0-9]*', name), 'Invalid placeholder name')
        require(spec['format'] in FORMATS and spec['guard'] in GUARDS, 'Invalid placeholder format/guard')
        cls, *fields = spec['path'].split('.')
        for field in fields:
            fields_by_name = dto_fields(source, cls)
            require(field in fields_by_name, f'Unknown DTO placeholder path: {spec["path"]}')
            cls = fields_by_name[field]
        require(cls in {'int', 'float'}, 'Placeholder must reference a numeric DTO field')
    for token, instance in [('baselineDischargedM3', 'baseline'), ('dischargedM3', 'current')]:
        expected = {'path': 'WorldResult.dischargedM3', 'format': 'volume',
                    'guard': 'matchedRunoffAvailable', 'instance': instance}
        require(bindings['placeholders'].get(token) == expected,
                f'Runoff placeholder {token} must preserve matched availability and {instance} provenance')
    files = bindings['entries'] + bindings['legacyEntries']
    require(len(files) == len(set(files)), 'Duplicate entry path')
    require(set(files) == {str(p.relative_to(folder)) for p in folder.rglob('*.json') if p.name != 'bindings.json'}, 'Entry files/index differ')
    entries, choice_ids = {}, set()
    for file in files:
        require((folder/file).resolve().is_relative_to(folder.resolve()), 'Entry path escapes package')
        entry = read_json(folder/file)
        validate_entry(entry, sources, bindings['placeholders'], limits, entry_fields, choice_fields)
        require(entry['id'] not in entries, 'Duplicate content ID')
        require(Path(file).stem == entry['id'], 'Filename/content ID mismatch')
        entries[entry['id']] = entry
        for choice in entry['choices']:
            require(choice['id'] not in choice_ids, 'Choice ID reused across entries')
            choice_ids.add(choice['id'])
    require(scenario['introId'] in entries and scenario['learningId'] in entries, 'Scenario content reference missing')
    require(all('tool-' + t['id'] in entries for t in scenario['treatments']), 'Treatment intent reference missing')
    require(len(bindings['hintIds']) in range(3, 6), 'Expected 3–5 progressive hints')
    for group in ('hintIds','disclosureIds','questionIds','preferenceIds'):
        require(len(bindings[group]) == len(set(bindings[group])), f'Duplicate {group}')
        require(all(id in entries for id in bindings[group]), f'Unknown {group} reference')
    require(bindings['questionIds'] == ['entry-prediction-v1', scenario['learningId']], 'Entry/transfer question coverage changed')
    for id in bindings['questionIds']:
        choices = entries[id]['choices']
        require(len(choices) >= 3 and sum(c['correct'] for c in choices) == 1, 'Question needs one reasoned best answer and distractors')
        require(len({c['explanation'] for c in choices}) == len(choices), 'Question feedback must explain each alternative')
    for id in bindings['preferenceIds']:
        require(entries[id]['choices'] and not any(c['correct'] for c in entries[id]['choices']), 'Preferences must not be scored')
    for id, fallback in bindings['fallbacks'].items():
        require(id in entries and fallback in entries and id != fallback, 'Unknown/self fallback')
        require(not tokens(entries[fallback]['body']) and not entries[fallback]['choices'], 'Fallback must be self-contained')
    for id, guard in bindings['capabilityGuards'].items():
        require(id in entries and guard in GUARDS, 'Unknown capability entry/guard')
    for id, entry in entries.items():
        if tokens(entry['body']):
            require(id in bindings['fallbacks'], 'Placeholder entry lacks whole-entry fallback')
    require(all(id in entries for id in bindings['reasonBindings'].values()), 'Unknown reason content')
    require(bindings['unknownReasonId'] in entries, 'Missing unknown-reason fallback')
    core_path = Path(core_source) if core_source is not None else root/'Assets/Terra/Scripts/Core/WorldStateStore.cs'
    core = core_path.read_text()
    literal_reasons = set(re.findall(r'Invalid\("([^"]+)"\)', core))
    world = read_json(root/'worlds/meadowbrook-v1/world.json')
    literal_reasons |= {s['protectionReason'] for s in world['sites'] if s['protectedSite']}
    require(literal_reasons <= set(bindings['reasonBindings']), 'Runtime reasons changed; review reason adapter')
    preferences = {c['id'] for id in bindings['preferenceIds'] for c in entries[id]['choices']}
    require(set(bindings['choiceTargets']) == preferences, 'Missing Take It Home card')
    require(all(id in entries for id in bindings['choiceTargets'].values()), 'Unknown Take It Home target')
    require(entries['entry-prediction-v1']['body'] != entries['runoff-transfer-v1']['body'], 'Transfer must be a different situation')
    return entries, bindings


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--root', type=Path, default=ROOT)
    parser.add_argument('--core-source', type=Path, help='Read-only alternate WorldStateStore.cs for integration reason compatibility; defaults to --root Core')
    args = parser.parse_args()
    try:
        entries, bindings = validate_package(args.root.resolve(), args.core_source)
        print(json.dumps({'status':'pass','entries':len(entries), 'authoredEntries':len(bindings['entries']),
                          'questions':len(bindings['questionIds']), 'hints':len(bindings['hintIds']),
                          'placeholders':len(bindings['placeholders']), 'dtoSource':'Assets/Terra/Scripts/Contracts/WorldContracts.cs'}, indent=2))
    except (ValueError, KeyError, TypeError) as error:
        print(f'FAIL: {error}', file=sys.stderr)
        return 1
    return 0


if __name__ == '__main__':
    sys.exit(main())
