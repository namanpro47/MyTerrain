#!/usr/bin/env python3
"""Reproducible CC0 sound bank. Run with numpy, scipy, soundfile, imageio-ffmpeg.
Downloads cache under Builds/audio-sources; only the mastered, compact bank is shipped.
All textures are decorative: park recordings are from Adelaide, not the playable AOIs.
"""
import hashlib, json, math, pathlib, shutil, subprocess, uuid, zipfile
import numpy as np
import soundfile as sf
from scipy.signal import butter, sosfilt, resample_poly
import imageio_ffmpeg
ROOT = pathlib.Path(__file__).resolve().parents[1]
CACHE = ROOT / 'Builds/audio-sources'
OUT = ROOT / 'Assets/Terra/Resources/Audio'
RATE = 32000
SOURCES = [
 dict(id='impact',creator='Kenney',title='Impact Sounds 1.0',page='https://kenney.nl/assets/impact-sounds',url='https://kenney.nl/media/pages/assets/impact-sounds/87b4ddecda-1677589768/kenney_impact-sounds.zip',file='impact.zip',license='CC0-1.0'),
 dict(id='piano',creator='Indieteur',title='Mystical Piano',page='https://opengameart.org/content/mystical-piano',url='https://opengameart.org/sites/default/files/Mystical%20Piano.Wav',file='piano.wav',license='CC0-1.0'),
 dict(id='vaporware',creator='The Cynic Project / cynicmusic.com / pixelsphere.org',title='Vaporware',page='https://opengameart.org/content/calm-piano-1-vaporware',url='https://opengameart.org/sites/default/files/003_Vaporware_2.mp3',file='vaporware.mp3',license='CC0-1.0'),
 dict(id='river',creator='Thimras',title='Park ambiences: river',page='https://opengameart.org/content/park-ambiences',url='https://opengameart.org/sites/default/files/park_ambience_river.wav',file='river.wav',license='CC0-1.0'),
 dict(id='birds',creator='Thimras',title='Park ambiences: birds',page='https://opengameart.org/content/park-ambiences',url='https://opengameart.org/sites/default/files/park_ambience_birds.wav',file='birds.wav',license='CC0-1.0')]
records=[]

def meta(path, audio=False, long=False):
    target=path.with_name(path.name+'.meta')
    guid=uuid.uuid5(uuid.NAMESPACE_URL,'terra-audio-v1/'+str(path.relative_to(ROOT))).hex
    if target.exists(): return
    text='fileFormatVersion: 2\nguid: '+guid+'\n'
    if path.is_dir(): text+='folderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n  userData:\n  assetBundleName:\n  assetBundleVariant:\n'
    elif audio:
        text+='AudioImporter:\n  externalObjects: {}\n  serializedVersion: 8\n  defaultSettings:\n    serializedVersion: 2\n    loadType: '+('1' if long else '0')+'\n    sampleRateSetting: 1\n    sampleRateOverride: 32000\n    compressionFormat: 1\n    quality: '+('0.65' if long else '0.8')+'\n    conversionMode: 0\n    preloadAudioData: '+('0' if long else '1')+'\n  platformSettingOverrides: {}\n  forceToMono: 0\n  normalize: 0\n  loadInBackground: 1\n  ambisonic: 0\n  3D: 0\n  userData:\n  assetBundleName:\n  assetBundleVariant:\n'
    else:text+='DefaultImporter:\n  externalObjects: {}\n  userData:\n  assetBundleName:\n  assetBundleVariant:\n'
    target.write_text(text)

def read(path):
    try:
        if pathlib.Path(path).suffix=='.mp3': raise ValueError('Use ffmpeg for MP3')
        x,sr=sf.read(path,always_2d=True)
    except Exception:
        data=subprocess.check_output([imageio_ffmpeg.get_ffmpeg_exe(),'-v','error','-i',str(path),'-f','f32le','-ac','2','-ar',str(RATE),'-'])
        return np.frombuffer(data,dtype='<f4').reshape(-1,2).astype(float)
    if sr!=RATE:
        g=math.gcd(sr,RATE);x=resample_poly(x,RATE//g,sr//g,axis=0)
    return x

def filt(x,hz=7000):
    n=min(len(x),4096)
    return sosfilt(butter(2,hz,fs=RATE,output='sos'),np.concatenate([x[-n:],x]),axis=0)[n:]
def envelope(x,attack=.006,release=.06):
    x=x.copy();a=min(len(x)//2,int(RATE*attack));r=min(len(x)//2,int(RATE*release))
    if a:x[:a]*=np.linspace(0,1,a)[:,None]
    if r:x[-r:]*=np.linspace(1,0,r)[:,None]
    return x

def seam(x,seconds=2):
    n=int(RATE*seconds); w=np.linspace(0,1,n)[:,None]
    # Circular linear blend: starts at original n, ends at original n after blended head.
    return np.concatenate([x[n:-n],x[-n:]*(1-w)+x[:n]*w])

def write(name,x,sources,loop=False,music=False):
    x=x-x.mean(axis=0);x=filt(x,11000 if music else 6500)
    if not loop:x=envelope(x,.03 if music else .004,2 if music else .08)
    target=-25 if music else -29 if loop else -24
    rms=max(1e-8,float(np.sqrt(np.mean(x*x))));x*=10**(target/20)/rms
    if not music and not loop:
        x=np.tanh(x*5)/5
        x*=10**(target/20)/max(1e-8,float(np.sqrt(np.mean(x*x))))
    x-=x.mean(axis=0)
    if not loop:
        x=envelope(x,.006,.08 if not music else 2)
        window=np.sin(np.linspace(0,np.pi,len(x)))[:,None]**2
        x-=window*x.mean(axis=0)/window.mean()
    peak=max(1e-8,float(np.max(np.abs(x))));x*=min(1,10**(-9/20)/peak)
    path=OUT/(name+('.ogg' if loop or music else '.wav'))
    if path.suffix=='.ogg':
        subprocess.run([imageio_ffmpeg.get_ffmpeg_exe(),'-v','error','-f','f32le','-ar',str(RATE),'-ac',str(x.shape[1]),'-i','-','-c:a','libvorbis','-q:a','4','-y',str(path)],input=x.astype('<f4').tobytes(),check=True)
    else: sf.write(path,x,RATE,format='WAV',subtype='PCM_16')
    meta(path,True,loop or music)
    # Verify the deliverable after encoding, not just the authoring buffer.
    decoded=read(path)
    records.append(dict(id=name,file=path.name,seconds=round(len(x)/RATE,4),channels=x.shape[1],sources=sources,loop=loop,music=music,peak_dbfs=round(20*np.log10(np.max(np.abs(decoded))+1e-12),2),rms_dbfs=round(20*np.log10(np.sqrt(np.mean(decoded*decoded))+1e-12),2),boundary_step=float(np.max(np.abs(decoded[0]-decoded[-1]))),sha256=hashlib.sha256(path.read_bytes()).hexdigest(),bytes=path.stat().st_size))

def mono(x):return x.mean(axis=1,keepdims=True)
def layer(parts,duration=None):
    end=max(int(at*RATE)+len(x) for at,x,gain in parts)
    y=np.zeros((max(end,int((duration or 0)*RATE)),1))
    for at,x,gain in parts:
        start=int(at*RATE);y[start:start+len(x)]+=mono(x)*gain
    return y

def tone(notes,duration=1.4,step=.16):
    y=np.zeros((int(duration*RATE),1))
    for i,midi in enumerate(notes):
        start=int(i*step*RATE);n=len(y)-start;t=np.arange(n)/RATE;f=440*2**((midi-69)/12)
        v=(np.sin(2*np.pi*f*t)+.23*np.sin(2*np.pi*f*2*t)+.06*np.sin(2*np.pi*f*3*t))*np.exp(-t*5)*(1-np.exp(-t*110))
        y[start:,0]+=v/(1+i*.15)
    return y

CACHE.mkdir(parents=True,exist_ok=True);OUT.mkdir(parents=True,exist_ok=True)
for source in SOURCES:
    path=CACHE/source['file']
    if not path.exists():subprocess.run(['curl','-fLsS','--retry','2',source['url'],'-o',str(path)],check=True)
    source['sha256']=hashlib.sha256(path.read_bytes()).hexdigest()
with zipfile.ZipFile(CACHE/'impact.zip') as archive:archive.extractall(CACHE/'impact')
impact=CACHE/'impact/Audio'
legacy=ROOT/'Assets/Terra/Audio/ExternalKit/Clips'

def hit(name,v):
    x=read(impact/(name+'_'+str(v).zfill(3)+'.ogg'))
    return x*.6/max(1e-8,float(np.max(np.abs(x))))
# Layered material gestures, three real alternate takes per recipe; no stereotype for stewardship.
recipes={
 'farm':[('footstep_grass',0,1),('impactSoft_medium',.13,.6)],
 'rotation':[('footstep_grass',0,.8),('footstep_grass',.21,.6),('impactWood_light',.33,.25)],
 'margin':[('footstep_grass',0,.8),('impactSoft_medium',.2,.3)],
 'cover':[('footstep_grass',0,.6),('footstep_grass',.12,.5),('footstep_grass',.26,.3)],
 'meadow':[('footstep_grass',0,1),('impactSoft_heavy',.14,.25)],
 'swale':[('impactMining',0,.5),('footstep_grass',.12,.4)],
 'park':[('impactWood_light',0,.8),('footstep_grass',.16,.8)],
 'home':[('impactWood_medium',0,.8),('impactPlank_medium',.17,.6)],
 'housing':[('impactMining',0,.6),('impactWood_heavy',.16,.7),('impactMetal_light',.31,.2)],
 'warehouse':[('impactPlank_medium',0,.6),('impactMetal_medium',.17,.65)],
 'data':[('impactMetal_light',0,.6),('impactPlate_light',.16,.35)],
 'village':[('impactWood_light',0,.6),('impactWood_medium',.13,.6),('impactGlass_light',.3,.15)],
 'power':[('impactMetal_heavy',0,.5),('impactTin_medium',.22,.3)],
 'solar':[('impactPlate_light',0,.5),('impactGlass_light',.17,.3)],
 'wind':[('impactMetal_medium',0,.5),('impactWood_light',.17,.3)],
 'stewardship':[('footstep_grass',0,.6),('impactWood_light',.22,.25)],
 'water':[('impactMetal_light',0,.5),('impactMining',.13,.2)]}
river=mono(read(CACHE/'river.wav'))
wind=read(legacy/'ambient-wind.wav')[1024:]
for family,parts in recipes.items():
 for variant in range(3):
    layers=[(at,hit(name,variant),gain) for name,at,gain in parts]
    if family in ('water','swale'):layers.append((.12,envelope(river[int(9*RATE):int(9.7*RATE)],.09,.25),.8))
    if family=='wind':layers.append((.08,envelope(wind[:int(.9*RATE)],.1,.3),1.5))
    if family in ('solar','data'):layers.append((.18,tone([76,83],.65,.09),.02))
    write('place-'+family+'-'+str(variant),layer(layers),['impact']+(['river'] if family in ('water','swale') else ['original-tones'] if family in ('solar','data') else ['legacy-wind'] if family=='wind' else []))
for name,source in [('ui-click','place'),('ui-focus','cancel'),('cancel','cancel'),('undo','undo'),('blocked','blocked')]:
    x=read(legacy/(source+'.wav'))
    if name=='ui-focus': x=x[:int(RATE*.065)]
    write(name,x,['legacy-kenney'])
for name,notes,duration,step in [
 ('ui-confirm',[72],.45,.1),('ui-open',[64,71],.55,.07),('ui-close',[71,64],.5,.06),
 ('tool-select',[67,74],.5,.07),('atlas-select',[62,69,74],.9,.1),
 ('arrival',[50,57,62,66,69],2.5,.18),('travel',[50,57,64],2,.24),
 ('redo',[62,69],.6,.08),('move',[62,67,62],.6,.09),('remove',[64,57],.6,.09),('clear',[64,59,52],.8,.1),
 ('camera',[62],.45,.1),('year-submit',[50,57,62],1.1,.13),('year-report',[62,66,69,74],1.9,.2),
 ('year-attention',[57,60,64],1.7,.22),('year-continue',[57,62,69],1.2,.14),
 ('new-run',[50,57,62,69],1.8,.19),('decade-complete',[50,57,62,66,69,74],3.5,.3)]:
    write(name,tone(notes,duration,step),['original-tones'])
write('ambient-wind',seam(wind[:int(RATE*36)]),['legacy-wind'],loop=True)
write('ambient-river',seam(river[int(RATE*7):int(RATE*39)]),['river'],loop=True)
write('ambient-birds',seam(mono(read(CACHE/'birds.wav'))[int(RATE*22):int(RATE*56)]),['birds'],loop=True)
write('music-mystical',read(CACHE/'piano.wav')[:int(95*RATE)],['piano'],music=True)
write('music-vaporware',read(CACHE/'vaporware.mp3'),['vaporware'],music=True)
(OUT/'manifest.json').write_text(json.dumps(dict(schema=1,license='CC0-1.0',sources=SOURCES,legacy_manifest='Assets/Terra/Audio/ExternalKit/manifest.json',original_tones='Original mathematically synthesized mallet gestures in scripts/prepare-audio.py; CC0-1.0.',processing='32 kHz; DC removal; low pass; RMS/peak mastering; microfades; crossfaded nature loops; music full-length except creator-specified 95s Mystical Piano boundary. Runtime six-second musical transitions.',geography='Decorative illustrative sound only. Thimras recordings were made in Adelaide, Australia, not game locations. No observed species/weather claims.',clips=records),indent=2)+'\n')
(OUT/'CREDITS.txt').write_text('MYTERRAIN SOUND BANK — CC0 1.0\n\n'+ '\n\n'.join(s['title']+' — '+s['creator']+'\n'+s['page']+'\n'+s['license'] for s in SOURCES)+'\n\nKenney Interface Sounds; Luke.RUSTLTD wind1: see Assets/Terra/Audio/ExternalKit/manifest.json.\nMallet cues composed and synthesized for MyTerrain, CC0. Nature recordings are illustrative, not local sound surveys.\n')
shutil.copy(ROOT/'Assets/Terra/Audio/ExternalKit/Licenses/CC0-1.0.txt',OUT/'LICENSE.txt')
shutil.copy(CACHE/'impact/License.txt',OUT/'KENNEY-LICENSE.txt')
for path in OUT.iterdir():
 if path.suffix!='.meta':meta(path)
meta(OUT)
print(json.dumps(dict(clips=len(records),bytes=sum(r['bytes'] for r in records),max_peak=max(r['peak_dbfs'] for r in records)),indent=2))
