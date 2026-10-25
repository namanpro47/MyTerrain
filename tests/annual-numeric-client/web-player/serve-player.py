from http.server import SimpleHTTPRequestHandler, HTTPServer
from pathlib import Path
import json,hashlib
ROOT=Path(__file__).resolve().parents[3]
PROJECT=ROOT/'Builds/NumericWebCheckProject'
class Handler(SimpleHTTPRequestHandler):
 extensions_map={**SimpleHTTPRequestHandler.extensions_map,'.wasm':'application/wasm'}
 def __init__(self,*args,**kwargs):super().__init__(*args,directory=str(PROJECT/'Build'),**kwargs)
 def end_headers(self):
  self.send_header('Cache-Control','no-store');super().end_headers()
 def do_POST(self):
  if self.path!='/evidence':self.send_error(404);return
  length=int(self.headers.get('Content-Length','0'))
  if not 0<length<65536:self.send_error(413);return
  value=json.loads(self.rfile.read(length));name='webgl-reload.json' if value.get('reloaded') else 'webgl-first-run.json'
  value['sourceManifestSha256']=hashlib.sha256((PROJECT/'source-manifest.json').read_bytes()).hexdigest()
  (ROOT/'docs/evidence/annual-numeric-client'/name).write_text(json.dumps(value,indent=2)+'\n')
  self.send_response(204);self.end_headers()
if __name__=='__main__':HTTPServer(('127.0.0.1',8878),Handler).serve_forever()
