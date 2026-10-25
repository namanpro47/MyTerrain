import importlib.util
import json
from pathlib import Path
import tempfile
import shutil
import unittest

import numpy as np
from pyproj import Transformer

ROOT=Path(__file__).resolve().parents[2]
spec=importlib.util.spec_from_file_location('community',ROOT/'worldgen/community.py')
community=importlib.util.module_from_spec(spec);spec.loader.exec_module(community)


class CommunityImportChecks(unittest.TestCase):
    def test_real_imports_and_projection(self):
        identities=[]
        for identifier in ('coon-valley-community-v1','tomales-community-v1'):
            folder=ROOT/'worlds/communities'/identifier
            p=community.validate(folder); identities.append(p['id'])
            projection=Transformer.from_crs(4326,p['crs'],always_xy=True)
            origin=projection.transform(p['longitude'],p['latitude'])
            np.testing.assert_allclose(origin,p['projectedOrigin'],atol=.001)
            b=p['projectedBounds'];t=p['terrain'];n=t['rows']
            # Independent control points: every source image edge lands on the corresponding mesh edge.
            for u,v in [(0,0),(1,0),(0,1),(1,1),(.5,.5),(.25,.75)]:
                projected=(b[0]+u*(b[2]-b[0]),b[3]-v*(b[3]-b[1]))
                local=(projected[0]-origin[0],projected[1]-origin[1])
                np.testing.assert_allclose(local,((u-.5)*p['extentM'],(.5-v)*p['extentM']),atol=.001)
            self.assertGreater(np.ptp(t['heights']),30)
            self.assertEqual((n-1)*t['cellSizeM'],1200)
            self.assertEqual(p['imageYear'],'2022')
            catalog=json.loads((folder/'imagery-catalog.json').read_text())
            self.assertTrue(all(f['attributes']['resolution_value']==.6 for f in catalog['features']))
            self.assertTrue(all(f['attributes']['acquisition_date'] for f in catalog['features']))
        self.assertEqual(set(identities),{'coon-valley-community-v1','tomales-community-v1'})

    def test_corrupt_image_rejected(self):
        with tempfile.TemporaryDirectory() as temp:
            dest=Path(temp)/'pack';shutil.copytree(ROOT/'worlds/communities/tomales-community-v1',dest)
            with (dest/'imagery.jpg').open('ab') as f:f.write(b'changed')
            with self.assertRaisesRegex(ValueError,'Checksum'):community.validate(dest)

    def test_wrong_orientation_rejected(self):
        with tempfile.TemporaryDirectory() as temp:
            dest=Path(temp)/'pack';shutil.copytree(ROOT/'worlds/communities/tomales-community-v1',dest)
            p=json.loads((dest/'community.json').read_text());p['rasterRows']='north-to-south';community.write(dest/'community.json',p)
            with self.assertRaisesRegex(ValueError,'orientation'):community.validate(dest)

    def test_missing_elevation_rejected(self):
        with tempfile.TemporaryDirectory() as temp:
            dest=Path(temp)/'pack';shutil.copytree(ROOT/'worlds/communities/tomales-community-v1',dest)
            p=json.loads((dest/'community.json').read_text());p['terrain']['heights'].pop();community.write(dest/'community.json',p)
            with self.assertRaisesRegex(ValueError,'elevation'):community.validate(dest)

    def test_refuses_overwrite_and_unsupported_region(self):
        import argparse
        args=argparse.Namespace(id='tomales-community-v1',lat=38.24639,lon=-122.90556,extent=1200,pixels=2048,output=ROOT/'worlds/communities')
        with self.assertRaisesRegex(ValueError,'already exists'):community.build(args)
        args.lat=42.18;args.lon=73.96
        with self.assertRaisesRegex(ValueError,'CONUS'):community.build(args)


if __name__=='__main__':unittest.main()
