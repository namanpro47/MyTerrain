"""Release verifier must decode both export generations and reject damaged bytes."""
import gzip
import importlib.util
from pathlib import Path
import subprocess
import unittest

ROOT = Path(__file__).resolve().parents[2]
spec = importlib.util.spec_from_file_location('verify_web', ROOT / 'scripts/verify-web.py')
verify = importlib.util.module_from_spec(spec)
spec.loader.exec_module(verify)

class CompressionChecks(unittest.TestCase):
    raw = b'\0asm\x01\0\0\0' + bytes(range(256)) * 100

    @classmethod
    def setUpClass(cls):
        cls.brotli = subprocess.run(
            ['node', '-e', "process.stdout.write(require('node:zlib').brotliCompressSync(require('node:fs').readFileSync(0)))"],
            input=cls.raw, capture_output=True, check=True).stdout

    def test_historical_gzip(self):
        self.assertEqual(verify.decode_unity_payload(gzip.compress(self.raw)), ('gzip', self.raw))

    def test_brotli(self):
        self.assertEqual(verify.decode_unity_payload(self.brotli), ('br', self.raw))

    def test_corrupt_brotli(self):
        with self.assertRaises(ValueError):
            verify.decode_unity_payload(b'not a compressed Unity export')

    def test_truncated_brotli(self):
        with self.assertRaises(ValueError):
            verify.decode_unity_payload(self.brotli[:len(self.brotli)//2])

if __name__ == '__main__':
    unittest.main()
