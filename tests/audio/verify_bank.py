"""Validate encoded deliverables and coverage; never writes game state."""
import hashlib,json,pathlib,re,unittest
import numpy as np
import soundfile as sf
ROOT=pathlib.Path(__file__).resolve().parents[2]
BANK=ROOT/'Assets/Terra/Resources/Audio'
class SoundBankChecks(unittest.TestCase):
 @classmethod
 def setUpClass(cls):
  cls.manifest=json.loads((BANK/'manifest.json').read_text());cls.clips={c['id']:c for c in cls.manifest['clips']}
 def test_every_playable_action_has_three_distinct_recordings(self):
  code=(ROOT/'Assets/Terra/Scripts/Core/FullAnnualGameplay.cs').read_text()
  ids=re.findall(r'"([a-z-]+)"',re.search(r'Supported = \{([^}]+)',code).group(1))
  mapping=dict(re.findall(r'case "([a-z-]+)": return "([a-z-]+)";',(ROOT/'Assets/Terra/Scripts/Audio/TerraAudio.cs').read_text()))
  self.assertEqual(17,len(ids))
  self.assertEqual(17,len({mapping[i] for i in ids}))
  for action in ids:
   variants=[self.clips['place-'+mapping[action]+'-'+str(i)] for i in range(3)]
   self.assertEqual(3,len({c['sha256'] for c in variants}),action)
 def test_bank_integrity_and_headroom(self):
  for c in self.clips.values():
   path=BANK/c['file'];x,sr=sf.read(path,always_2d=True)
   self.assertEqual(hashlib.sha256(path.read_bytes()).hexdigest(),c['sha256'],c['id'])
   self.assertEqual(32000,sr);self.assertTrue(np.isfinite(x).all())
   self.assertLess(float(np.max(np.abs(x))),.4,c['id'])
   self.assertGreater(float(np.sqrt(np.mean(x*x))),.001,c['id'])
   self.assertLess(float(np.max(np.abs(x.mean(axis=0)))),.002,c['id'])
   if c['loop']:self.assertLess(float(np.max(np.abs(x[0]-x[-1]))),.025,c['id'])
 def test_every_requested_cue_exists(self):
  for path in (ROOT/'Assets/Terra/Scripts').rglob('*.cs'):
   if 'Editor' in path.parts:continue
   for cue in re.findall(r'Request\("([a-z-]+)"\s*[,)]',path.read_text()):self.assertIn(cue,self.clips,str(path))
 def test_provenance_and_size(self):
  self.assertTrue(all(s['license']=='CC0-1.0' and len(s['sha256'])==64 for s in self.manifest['sources']))
  self.assertLess(sum(c['bytes'] for c in self.clips.values()),10*1024*1024)
  self.assertGreater(self.clips['music-mystical']['seconds'],90)
  self.assertGreater(self.clips['music-vaporware']['seconds'],120)
if __name__=='__main__':unittest.main(verbosity=2)
