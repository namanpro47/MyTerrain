"""Local explicit UI harness; never serves a production build or writes game saves."""
from http.server import BaseHTTPRequestHandler, HTTPServer
from pathlib import Path
import hashlib,json
ROOT=Path(__file__).resolve().parents[2]
BRIDGE=ROOT/'Assets/Plugins/WebGL/TerraNumericStorage.jslib'
class Handler(BaseHTTPRequestHandler):
 def do_GET(self):
  files={'/':(ROOT/'tests/annual-numeric-client/browser.html','text/html; charset=utf-8'),'/bridge.js':(BRIDGE,'text/javascript; charset=utf-8')}
  if self.path not in files:self.send_error(404);return
  path,content_type=files[self.path];body=path.read_bytes();self.send_response(200);self.send_header('Content-Type',content_type);self.send_header('Content-Length',str(len(body)));self.send_header('Cache-Control','no-store');self.end_headers();self.wfile.write(body)
 def do_POST(self):
  if self.path!='/evidence':self.send_error(404);return
  size=int(self.headers.get('Content-Length','0'))
  if not 0<size<65536:self.send_error(413);return
  data=json.loads(self.rfile.read(size));kind=data.get('kind')
  if kind not in ('real-indexeddb','after-reload','failure'):self.send_error(400);return
  data['bridgeSha256']=hashlib.sha256(BRIDGE.read_bytes()).hexdigest()
  folder=ROOT/'docs/evidence/annual-numeric-client';folder.mkdir(exist_ok=True)
  (folder/(kind+'.json')).write_text(json.dumps(data,indent=2)+'\n')
  self.send_response(204);self.end_headers()
if __name__=='__main__':HTTPServer(('127.0.0.1',8879),Handler).serve_forever()
