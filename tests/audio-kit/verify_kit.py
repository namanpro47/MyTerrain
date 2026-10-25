#!/usr/bin/env python3
"""Offline integrity, licensing, PCM headroom and loop checks. Python standard library only."""
import argparse
import array
import hashlib
import json
import math
from pathlib import Path
import re
import struct
import sys
import wave

ROOT = Path(__file__).resolve().parents[2]
KIT = ROOT / 'Assets/Terra/Audio/ExternalKit'

def rms(x): return math.sqrt(sum(v*v for v in x) / len(x))
def db(x): return round(20*math.log10(max(x, 1e-12)), 3)
def sha(path): return hashlib.sha256(path.read_bytes()).hexdigest()

def smpl(path):
    data = path.read_bytes(); pos = 12
    while pos + 8 <= len(data):
        kind = data[pos:pos+4]; size = struct.unpack_from('<I', data, pos+4)[0]
        if kind == b'smpl':
            values = struct.unpack_from('<15I', data, pos+8)
            return {'count': values[7], 'type': values[10], 'start': values[11], 'end': values[12], 'play_count': values[14]}
        pos += 8 + size + size % 2
    return None

def verify():
    manifest = json.loads((KIT/'manifest.json').read_text())
    sources = {s['id']: s for s in manifest['sources']}
    checks = 0
    def check(condition, message):
        nonlocal checks
        if not condition: raise AssertionError(message)
        checks += 1
    check(set(c['id'] for c in manifest['clips']) == {'place','undo','cancel','blocked','ambient-wind'}, 'unexpected event inventory')
    for src in sources.values():
        check(src['license'] == 'CC0-1.0', 'license is not approved CC0')
        check((KIT/src['local_license']).is_file(), 'missing source license')
        check(src['page_url'].startswith('https://') and src['download_url'].startswith('https://'), 'missing source URLs')
    clips = []
    for item in manifest['clips']:
        path = KIT/item['file']
        check(path.is_file() and sha(path) == item['sha256'], 'missing or changed audio: '+item['id'])
        check(item['source_family'] in sources, 'unresolved provenance')
        check(bool(re.fullmatch('[0-9a-f]{64}', item['source_sha256'])), 'missing pinned source hash')
        if item['source_family'] == 'kenney-interface-1.0':
            check(sha(ROOT/item['source_file']) == item['source_sha256'], 'Kenney source drift')
        with wave.open(str(path), 'rb') as w:
            check(w.getnchannels()==1 and w.getsampwidth()==2 and w.getframerate()==22050, 'unexpected PCM format')
            check(w.getnframes()==item['frames'], 'unexpected duration')
            samples = array.array('h', w.readframes(w.getnframes()))
            if sys.byteorder != 'little': samples.byteswap()
        x = [v/32768 for v in samples]
        peak = max(abs(v) for v in x)
        check(peak < .205, 'lost at least 13.7 dB sample headroom')
        check(rms(x) > 1e-5, 'silent clip')
        record = {'id':item['id'], 'bytes':path.stat().st_size, 'frames':len(x), 'seconds':round(len(x)/22050,6), 'rate_hz':22050, 'channels':1, 'sample_peak_dbfs':db(peak), 'sample_headroom_db':-db(peak), 'rms_dbfs':db(rms(x)), 'dc_offset':sum(x)/len(x), 'float32_decoded_bytes_estimate':len(x)*4}
        loop = smpl(path)
        if item['loop']:
            check(loop is not None and loop['count']==1 and loop['type']==0 and loop['play_count']==0, 'missing forward infinite smpl loop')
            check(loop['start']==1024 and loop['end']==len(x)-1, 'incorrect loop range')
            check(all(v==0 for v in x[:1024]), 'AAC prefix guard is not silent')
            cycle=x[loop['start']:loop['end']+1]
            seam=abs(cycle[-1]-cycle[0])
            step_rms=rms([b-a for a,b in zip(cycle,cycle[1:])])
            check(seam < .01 and seam < 2*step_rms, 'loop boundary exceeds ordinary adjacent sample variation')
            check(len(cycle)/22050 > 50, 'ambient loop is too short')
            head,tail=rms(cycle[:22050]),rms(cycle[-22050:])
            check(abs(db(head)-db(tail))<6, 'large first/last-second level mismatch')
            check(abs(db(rms(cycle)) - item['target_rms_dbfs'])<.1, 'ambient RMS target drift')
            record.update({'loop':loop,'loop_seconds':round(len(cycle)/22050,6),'seam_step_dbfs':db(seam),'seam_to_adjacent_step_rms_ratio':round(seam/step_rms,3),'loop_rms_dbfs':db(rms(cycle)),'first_last_second_rms_difference_db':round(abs(db(head)-db(tail)),3)})
        else:
            check(loop is None, 'one-shot unexpectedly has loop markers')
            check(x[0]==0 and x[-1]==0, 'one-shot boundaries not faded')
            check(len(x)/22050 < .3, 'cue longer than intention')
            check(abs(db(peak)-item['target_peak_dbfs'])<.02, 'cue peak target drift')
        clips.append(record)
    # Independent Unity identity check: collide neither with the kit nor any existing project asset.
    guids={}
    for meta in (ROOT/'Assets').rglob('*.meta'):
        match=re.search(r'^guid: ([0-9a-f]{32})$',meta.read_text(errors='replace'),re.M)
        if match: guids.setdefault(match[1],[]).append(meta)
    for path in [KIT.parent,KIT]+[p for p in KIT.rglob('*') if p.suffix!='.meta']:
        meta=path.with_name(path.name+'.meta')
        check(meta.is_file(), 'missing stable Unity identity: '+str(path))
        match=re.search(r'^guid: ([0-9a-f]{32})$',meta.read_text(),re.M)
        check(match is not None and len(guids[match[1]])==1,'colliding Unity GUID')
    total=sum(c['bytes'] for c in clips)
    check(total < 3_000_000, 'audio source budget exceeded')
    return {'status':'pass','checks':checks,'source_wav_bytes':total,'source_size_is_not_web_download_size':True,'float32_decoded_bytes_estimate':sum(c['float32_decoded_bytes_estimate'] for c in clips),'actual_unity_memory_or_build_measurement':False,'subjective_listening_review':False,'unity_import_tested':False,'unity_web_playback_tested':False,'clips':clips}

if __name__=='__main__':
    parser=argparse.ArgumentParser(description=__doc__); parser.add_argument('--output',type=Path); args=parser.parse_args()
    result=verify(); output=json.dumps(result,indent=2)+'\n'
    if args.output: args.output.write_text(output)
    print(output)
