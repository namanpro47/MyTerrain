#!/usr/bin/env python3
"""Reproduce C3 mastered WAVs from the existing Kenney sources and pinned wind1.wav.
Usage: python3 tests/audio-kit/build_kit.py --wind-source /path/to/wind1.wav
Dependencies: numpy and soundfile; no network calls, Unity, or runtime changes.
"""
import argparse
import hashlib
import json
from pathlib import Path
import struct
import uuid
import wave

import numpy as np
import soundfile as sf

ROOT = Path(__file__).resolve().parents[2]
KIT = ROOT / 'Assets/Terra/Audio/ExternalKit'
SOURCE = ROOT / 'Assets/ThirdParty/Kenney/InterfaceSounds/Audio'
RATE = 22050
NAMESPACE = uuid.UUID('4e37d69e-d7b9-4329-841c-07b025ce6699')

def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

def mono_resample(path):
    data, rate = sf.read(path, dtype='float64', always_2d=True)
    x = data.mean(axis=1)
    # Frequency-domain low-pass removes ultrasonic/high-frequency hiss before decimation.
    # A smooth rolloff, not a brick-wall cut; all existing sources are exactly 44.1 kHz.
    assert rate == 44100, (path, rate)
    spectrum = np.fft.rfft(x)
    freq = np.fft.rfftfreq(len(x), 1 / rate)
    spectrum *= 1 / np.sqrt(1 + (freq / 4500) ** 12)
    return np.fft.irfft(spectrum, n=len(x))[::2]

def write_wav(path, x, loop_start=None):
    pcm = np.rint(np.clip(x, -1, 32767 / 32768) * 32768).astype('<i2')
    with wave.open(str(path), 'wb') as w:
        w.setnchannels(1); w.setsampwidth(2); w.setframerate(RATE)
        w.writeframes(pcm.tobytes())
    if loop_start is not None:
        # RIFF smpl sampler chunk: one forward, infinite, sample-accurate loop.
        # Unity recommends 1024 silent prefix frames + loop start after the guard for Web AAC.
        header = struct.pack('<9I', 0, 0, round(1e9 / RATE), 60, 0, 0, 0, 1, 0)
        loop = struct.pack('<6I', 0, 0, loop_start, len(pcm) - 1, 0, 0)
        raw = path.read_bytes() + b'smpl' + struct.pack('<I', len(header + loop)) + header + loop
        path.write_bytes(raw[:4] + struct.pack('<I', len(raw) - 8) + raw[8:])
    return len(pcm)

def guid_for(path):
    return uuid.uuid5(NAMESPACE, path.relative_to(ROOT).as_posix()).hex

def identity_meta(path):
    # Identity only: let Unity generate platform-specific importer settings on first import.
    # This does not pretend that any importer has been configured or SaveAndReimport tested.
    meta = path.with_name(path.name + '.meta')
    if not meta.exists():
        meta.write_text('fileFormatVersion: 2\nguid: ' + guid_for(path) + ('\nfolderAsset: yes\n' if path.is_dir() else '\n'))

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--wind-source', required=True, type=Path)
    args = parser.parse_args()
    # Pinned source identity prevents accidentally mastering a different recording/license.
    expected = 'a2856f794fbf77e05f8241dc4adeade7f62fa21c3c9dc49f7725b6c6218060c3'
    if sha(args.wind_source) != expected:
        raise SystemExit('wind1.wav does not match the documented source SHA-256')
    clips = []
    for name, original, peak in [('place', 'drop_001', -14), ('undo', 'back_001', -18), ('cancel', 'close_001', -20), ('blocked', 'error_001', -21)]:
        path = SOURCE / (original + '.ogg')
        x = mono_resample(path)
        # Remove DC, suppress boundary discontinuities, preserve the authored cue character.
        x -= x.mean()
        attack, release = min(round(.002 * RATE), len(x)//4), min(round(.012 * RATE), len(x)//4)
        x[:attack] *= np.linspace(0, 1, attack)
        x[-release:] *= np.linspace(1, 0, release)
        x *= 10 ** (peak / 20) / np.max(np.abs(x))
        out = KIT / 'Clips' / (name + '.wav')
        frames = write_wav(out, x)
        clips.append({'id': name, 'file': 'Clips/' + out.name, 'sha256': sha(out), 'source_family': 'kenney-interface-1.0', 'source_file': str(path.relative_to(ROOT)), 'source_sha256': sha(path), 'loop': False, 'frames': frames, 'target_peak_dbfs': peak, 'processing': 'Stereo mean where needed; smooth 4.5 kHz low-pass; 44.1 to 22.05 kHz decimation; DC removal; 2 ms attack/12 ms release; peak attenuation; PCM16 export.'})
    x = mono_resample(args.wind_source)
    x -= x.mean()
    # Two-second constant-power seam crossfade creates a continuous cycle without a silence dip.
    overlap = 2 * RATE
    tail = x[-overlap:].copy()
    xloop = x[overlap:].copy()
    theta = np.linspace(0, np.pi/2, overlap)
    xloop[-overlap:] = tail * np.cos(theta) + x[:overlap] * np.sin(theta)
    xloop *= min(10 ** (-36 / 20) / np.sqrt(np.mean(xloop*xloop)), 10 ** (-18 / 20) / np.max(np.abs(xloop)))
    guard = 1024
    guarded = np.concatenate([np.zeros(guard), xloop])
    out = KIT / 'Clips/ambient-wind.wav'
    frames = write_wav(out, guarded, loop_start=guard)
    clips.append({'id': 'ambient-wind', 'file': 'Clips/' + out.name, 'sha256': sha(out), 'source_family': 'luke-rustltd-wind1', 'source_file': 'wind1.wav', 'source_sha256': sha(args.wind_source), 'loop': True, 'frames': frames, 'loop_start_frame': guard, 'loop_end_frame_inclusive': frames-1, 'loop_seconds': len(xloop)/RATE, 'target_rms_dbfs': -36, 'peak_ceiling_dbfs': -18, 'processing': 'Smooth 4.5 kHz low-pass; 44.1 to 22.05 kHz decimation; DC removal; two-second constant-power tail/head crossfade; RMS gain/peak ceiling; 1024 silent prefix frames + RIFF smpl loop; PCM16 export.'})
    manifest = {'schema_version': 1, 'package': 'C3 audio-only', 'status': 'prepared assets; Unity import, listening review and in-game mix pending', 'date': '2026-10-24', 'sample_rate_hz': RATE, 'channels': 1, 'encoding': 'PCM16 WAV', 'reference_patterns': ['TN-07', 'TN-08'], 'location': 'Meadowbrook Valley', 'geographic_claim': 'Decorative sound design only. Wind source is synthesized, not a local field recording or evidence of local species/weather.', 'sources': [{'id': 'kenney-interface-1.0', 'creator': 'Kenney', 'title': 'Interface Sounds 1.0', 'page_url': 'https://kenney.nl/assets/interface-sounds', 'download_url': 'https://kenney.nl/media/pages/assets/interface-sounds/fa43c1dd4d-1677589452/kenney_interface-sounds.zip', 'archive_sha256': 'f2193d072726d6758a5f7871b2dcc54dcce0d5c35c6f0a62f92549b327c81232', 'license': 'CC0-1.0', 'license_url': 'https://creativecommons.org/publicdomain/zero/1.0/', 'local_license': 'Licenses/Kenney-Interface-Sounds.txt', 'acquisition': 'Existing project subset, source archive identity from docs/toolkit-sources.json. Pack page rechecked 2026-10-24.'}, {'id': 'luke-rustltd-wind1', 'creator': 'Luke.RUSTLTD', 'title': 'wind1', 'page_url': 'https://opengameart.org/content/wind1', 'download_url': 'https://opengameart.org/sites/default/files/wind1.wav', 'download_sha256': sha(args.wind_source), 'license': 'CC0-1.0', 'license_url': 'https://creativecommons.org/publicdomain/zero/1.0/', 'local_license': 'Licenses/CC0-1.0.txt', 'acquisition': 'Downloaded and source page/license inspected 2026-10-24. Source describes wind synthesized in PureData; author credits Andy Farnell tutorial.'}], 'clips': clips}
    (KIT / 'manifest.json').write_text(json.dumps(manifest, indent=2) + '\n')
    for path in [KIT.parent, KIT] + sorted(KIT.rglob('*')):
        if path.suffix != '.meta': identity_meta(path)
    print(json.dumps({'clips': len(clips), 'wav_bytes': sum((KIT/c['file']).stat().st_size for c in clips), 'manifest': str(KIT/'manifest.json')}))

if __name__ == '__main__': main()
