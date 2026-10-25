"""Check the emitted overlays against source geography, including full joined cells."""
import json
import runpy
import unittest
from pathlib import Path

ROOT=Path(__file__).resolve().parents[2]
geo=runpy.run_path(str(ROOT/'scripts/prepare-parcels.py'))


class ParcelGeometry(unittest.TestCase):
    def test_source_exclusions_and_disjoint_partition(self):
        for pack in geo['PACKS']:
            with self.subTest(pack=pack):
                world=json.loads((ROOT/'worlds'/pack/'world.json').read_text())
                overlay=json.loads((ROOT/'Assets/Terra/Resources/Parcels'/f'{pack}.json').read_text(encoding='utf-8'))
                slots=[s for s in overlay['sites'] if s['sourceId']=='scenario-open-grid-v1']
                self.assertGreater(len(slots),30)
                for i,s in enumerate(slots):
                    rect=geo['bounds'](geo['points'](s['polygon']))
                    for other in overlay['sites']:
                        if other['id']==s['id']:
                            continue
                        self.assertLessEqual(geo['area'](geo['clip'](geo['points'](other['polygon']),rect)),.001,
                                             f"{pack}/{s['id']} overlaps {other['id']}")
                    for cover in world['covers']:
                        if cover['coverId'] in ('woodland','water','sealed'):
                            self.assertLessEqual(geo['area'](geo['clip'](geo['points'](cover['polygon']),rect)),.001,
                                                 f"{pack}/{s['id']} encroaches on {cover['coverId']}")
                    for f in world['features']:
                        m=f.get('widthM',0)/2+8
                        expanded=(rect[0]-m,rect[1]-m,rect[2]+m,rect[3]+m)
                        route=geo['points'](f['points'])
                        self.assertFalse(any(geo['line_crosses'](a,b,expanded) for a,b in zip(route,route[1:])),
                                         f"{pack}/{s['id']} encroaches on a mapped corridor")
                self.assertEqual(len(overlay['initialBuildings']),7)
                self.assertEqual(sum(x['actionId']=='low-density-housing' for x in overlay['initialBuildings']),4)


if __name__=='__main__':
    unittest.main()
