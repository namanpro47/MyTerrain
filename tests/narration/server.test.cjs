const {test}=require('node:test');const assert=require('node:assert/strict');
const http=require('node:http');const fs=require('node:fs/promises');const os=require('node:os');const path=require('node:path');
const {createHandler}=require('../../services/narration/api/narration.js');
const {split}=require('../../Assets/WebGLTemplates/Terra/narration.js');
async function fixture(t,overrides={}){
  let calls=[];const cacheDir=await fs.mkdtemp(path.join(os.tmpdir(),'terra-voice-test-'));
  const fetcher=async(url,options)=>{calls.push({url,options});return new Response(Buffer.alloc(128,1),{headers:{'content-type':'audio/mpeg'}});};
  const handler=createHandler({env:{ELEVENLABS_API_KEY:'TEST_ONLY',VERCEL:'1'},cacheDir,fetcher,...overrides});
  const server=http.createServer(handler);await new Promise(r=>server.listen(0,'127.0.0.1',r));
  t.after(async()=>{await new Promise(r=>server.close(r));await fs.rm(cacheDir,{recursive:true,force:true});});
  return {calls,url:'http://127.0.0.1:'+server.address().port+'/api/narration',post(text,headers={},extra={}){return fetch(this.url,{method:'POST',headers:{Origin:'https://www.myterrain.io','Content-Type':'application/json',...headers},body:JSON.stringify({text,...extra})});}};
}
test('same dialogue shares one generation and cached replay; key stays in upstream header',async t=>{
 const f=await fixture(t);const responses=await Promise.all([f.post('Welcome to the valley.'),f.post('Welcome to the valley.')]);
 for(const r of responses){assert.equal(r.status,200);assert.equal((await r.arrayBuffer()).byteLength,128);}
 assert.equal(f.calls.length,1);assert.equal((await f.post('Welcome to the valley.')).status,200);assert.equal(f.calls.length,1);
 assert.equal(f.calls[0].options.headers['xi-api-key'],'TEST_ONLY');assert.ok(!f.calls[0].options.body.includes('TEST_ONLY'));
 const config=await fetch(f.url,{headers:{Origin:'https://www.myterrain.io'}}).then(r=>r.json());assert.ok(config.configured);assert.ok(!JSON.stringify(config).includes('TEST_ONLY'));
});
test('rejects origins, missing browser origin, arbitrary provider controls, empty/oversized/markup input before quota',async t=>{
 const f=await fixture(t);
 assert.equal((await f.post('Hello',{Origin:'https://evil.example'})).status,403);
 assert.equal((await f.post('Hello',{Origin:''})).status,403);
 for(const text of ['',null,42,'a'.repeat(701),'<script>oops</script>'])assert.equal((await f.post(text)).status,400);
 assert.equal((await f.post('Hello',{}, {voice:'attacker'})).status,400);assert.equal(f.calls.length,0);
});
test('missing configuration and provider errors never echo secrets',async t=>{
 const missing=await fixture(t,{env:{VERCEL:'1'}});assert.equal((await missing.post('Hello')).status,503);
 const f=await fixture(t,{fetcher:async()=>new Response('TEST_ONLY provider body',{status:401})});const r=await f.post('Hello');assert.equal(r.status,503);assert.ok(!(await r.text()).includes('TEST_ONLY'));
});
test('throttle limits new spend; cached replay remains available',async t=>{
 const f=await fixture(t);await f.post('Cached first.');
 for(let i=0;i<29;i++)assert.equal((await f.post('Unique '+i)).status,200);
 assert.equal((await f.post('Too many.')).status,429);assert.equal((await f.post('Cached first.')).status,200);assert.equal(f.calls.length,30);
});
test('long dialogue is completely segmented without markup or dropped words',()=>{
 const text=Array.from({length:500},(_,i)=>'word'+i).join(' ');const parts=split('<b>'+text+'</b>');assert.equal(parts.join(' '),text);assert.ok(parts.every(p=>p.length<=600));assert.deepEqual(split(''),[]);assert.equal(split('a'.repeat(1400)).join(''),'a'.repeat(1400));
});
