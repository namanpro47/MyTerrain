const {test}=require('node:test');const assert=require('node:assert/strict');const vm=require('node:vm');const fs=require('node:fs');const {webcrypto}=require('node:crypto');
function harness(){
 const events={},timers=[],sources=[],requests=[];let fail=false;
 class AudioContext{constructor(){this.state='suspended';this.destination={};}createGain(){return {gain:{value:0},connect(){}};}resume(){this.state='running';return Promise.resolve();}suspend(){this.state='suspended';return Promise.resolve();}async decodeAudioData(bytes){return {bytes};}createBufferSource(){const s={connect(){},disconnect(){},start(){s.played=true;},stop(){s.stopped=true;}};sources.push(s);return s;}}
 const doc={hidden:false,hasFocus:()=>true,addEventListener:(n,f)=>events[n]=f};
 const context={document:doc,location:{hostname:'localhost',origin:'http://localhost:8999'},AudioContext,crypto:webcrypto,TextEncoder,URL,Response,AbortController,AbortSignal,
   setTimeout:(f,ms)=>{const timer={f,ms,cancelled:false};timers.push(timer);return timer;},clearTimeout:t=>{if(t)t.cancelled=true;},
   caches:{open:async()=>{throw Error('private mode');}},addEventListener:(n,f)=>events[n]=f,
   fetch:async(url,opts)=>{if(!opts||!opts.method)return new Response(JSON.stringify({revision:'test'}));requests.push(JSON.parse(opts.body));if(fail)return new Response('',{status:503});return new Response(new Uint8Array(128));}};
 context.window=context;context.globalThis=context;vm.runInNewContext(fs.readFileSync('Assets/WebGLTemplates/Terra/narration.js','utf8'),context);
 return {player:context.terraNarration,events,doc,sources,requests,setFail:v=>fail=v,flush(){for(const t of timers.splice(0))if(!t.cancelled&&t.ms===350)t.f();}};
}
async function settle(){for(let i=0;i<15;i++)await new Promise(r=>setImmediate(r));}
test('gesture gate, mute pause, page replacement cancels old audio, close stops, replay reuses cache',async()=>{
 const h=harness();h.player.set('Welcome to the valley.',true,.8);h.flush();await settle();assert.equal(h.requests.length,0);
 h.events.pointerdown();await settle();assert.equal(h.requests.length,1);assert.equal(h.player.status(),2);
 h.player.set('Welcome to the valley.',false,.8);await settle();assert.equal(h.player.status(),3);
 h.player.set('Welcome to the valley.',true,.8);await settle();assert.equal(h.player.status(),2);assert.equal(h.requests.length,1);
 h.player.replay();await settle();assert.equal(h.requests.length,1);
 h.player.set('Next year.',true,.8);h.flush();await settle();assert.equal(h.requests.length,2);assert.ok(h.sources[1].stopped);
 h.player.set('',true,.8);await settle();assert.ok(h.sources.at(-1).stopped);assert.equal(h.player.status(),0);
});
test('provider failure is visible and only explicit replay retries; no repeated frame spending',async()=>{
 const h=harness();h.setFail(true);h.events.pointerdown();h.player.set('Read this page.',true,.8);h.flush();await settle();assert.equal(h.player.status(),4);
 for(let i=0;i<10;i++)h.player.set('Read this page.',true,.8);await settle();assert.equal(h.requests.length,1);
 h.setFail(false);h.player.replay();await settle();assert.equal(h.player.status(),2);assert.equal(h.requests.length,2);
});
test('hidden tab pauses and does not request next segment until visible',async()=>{
 const h=harness();h.events.pointerdown();h.player.set('Sentence. '.repeat(100),true,.8);h.flush();await settle();assert.equal(h.requests.length,1);
 h.doc.hidden=true;h.events.visibilitychange();h.sources[0].onended();await settle();assert.equal(h.requests.length,1);
 h.doc.hidden=false;h.events.visibilitychange();await settle();assert.equal(h.requests.length,2);
});
