/* Spoken presentation only. No keys, game rules, or save-state writes. */
(function(root){
  'use strict';
  // Speak the message, not the dashboard. Unity supplies plain text with line
  // breaks represented as sentence separators; filter before splitting/caching.
  function coreText(value){
    const plain=String(value||'').replace(/<[^>]*>/g,'').replace(/\s+/g,' ').trim();
    if(/^What will you leave behind\?/i.test(plain))
      return 'What will you leave behind? Shape this landscape over ten years. Balance homes and livelihoods with land and water. Choose a project, find a place, then End Year. Edit freely until then. Accepted projects take time and need upkeep. Real terrain; fictional community and outcomes.';
    return plain.split(/(?<=[.!?])\s+/).map(s=>s.replace(/^[.\s]+|[.\s]+$/g,'').trim()).filter(s=>{
      if(!s)return false;
      // Resource/score cards, budget arithmetic, option captions and before/now labels.
      if(/^(?:public funds|homes now\s*\/\s*needed|jobs now\s*\/\s*target|before|now|selected)$/i.test(s))return false;
      if(/^[\d\s.,+−–\-/:%]+$/.test(s))return false;
      if(/^\d+\s+years?(?:\s*[.·]\s*selected)?$/i.test(s))return false;
      if(/[$€£]|\d\s*%|\d\s*\/\s*\d/.test(s))return false;
      if(/^(?:economic|social|environmental|change|score|homes|jobs|cash|balance|income|cost|upkeep|budget|funds|unpaid bills|future obligations)\b/i.test(s)&&/\d/.test(s))return false;
      if((s.match(/\d+(?:[.,]\d+)*/g)||[]).length>=3)return false;
      return true;
    }).map(s=>/[!?]$/.test(s)?s:s+'.').join(' ');
  }
  function split(text,max=600){
    const words=text.replace(/<[^>]*>/g,'').replace(/\s+/g,' ').trim().split(' '),result=[];let part='';
    for(let word of words){
      if(part.length+word.length+1>max){if(part)result.push(part);part='';}
      while(word.length>max){result.push(word.slice(0,max));word=word.slice(max);}
      part+=(part?' ':'')+word;
    }
    if(part)result.push(part);return result;
  }
  if(typeof module!=='undefined')module.exports={split,coreText};
  if(!root.document)return;
  let ctx,gain,source,abort,timer,version=0,text='',raw='',parts=[],index=0,volume=0,enabled=true,gesture=false;
  let status=0,revision='',buffer=null,started=false,configPromise;
  const endpoint=/^(localhost|127\.0\.0\.1)$/.test(location.hostname)?'http://127.0.0.1:8788/api/narration':'/api/narration';
  const memory=new Map();let cachePromise;
  function available(){return gesture&&enabled&&volume>0&&!document.hidden&&document.hasFocus();}
  function context(){if(!ctx){ctx=new AudioContext();gain=ctx.createGain();gain.connect(ctx.destination);}return ctx;}
  function stop(){clearTimeout(timer);version++;if(abort)abort.abort();abort=null;if(source){source.onended=null;try{source.stop();}catch{}source.disconnect();source=null;}buffer=null;started=false;status=0;}
  async function digest(value){return Array.from(new Uint8Array(await crypto.subtle.digest('SHA-256',new TextEncoder().encode(value))),x=>x.toString(16).padStart(2,'0')).join('');}
  async function cache(){try{return await (cachePromise||=caches.open('myterrain-narration-v1'));}catch{return null;}}
  async function config(){
    if(!configPromise)configPromise=fetch(endpoint,{signal:AbortSignal.timeout(5000)}).then(r=>{if(!r.ok)throw Error();return r.json();}).then(c=>{revision=c.revision;try{localStorage.setItem('myterrain.voice.config',JSON.stringify(c));}catch{}return c;}).catch(e=>{configPromise=null;try{const saved=JSON.parse(localStorage.getItem('myterrain.voice.config'));if(saved&&/^[a-f0-9]{16}$/.test(saved.revision)){revision=saved.revision;return saved;}}catch{}throw e;});
    return configPromise;
  }
  async function audioBytes(part,signal){
    await config();const key=new URL('/__voice_cache__/'+await digest(revision+'\n'+part),location.origin).href;
    if(memory.has(key))return memory.get(key).slice(0);
    const store=await cache();let saved;try{saved=await store?.match(key);}catch{}
    if(saved)return saved.arrayBuffer();
    const response=await fetch(endpoint,{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({text:part}),signal});
    if(!response.ok)throw Error('Voice unavailable');
    const bytes=await response.arrayBuffer();
    memory.set(key,bytes);if(memory.size>30)memory.delete(memory.keys().next().value);
    try{if(store){await store.put(key,new Response(bytes,{headers:{'Content-Type':'audio/mpeg'}}));const keys=await store.keys();if(keys.length>250)await Promise.all(keys.slice(0,keys.length-250).map(k=>store.delete(k)));}}catch{}
    return bytes.slice(0);
  }
  function playReady(token){
    if(token!==version||!available()||!buffer||source)return;
    const c=context();if(c.state!=='running'){status=3;return;}
    source=c.createBufferSource();source.buffer=buffer;source.connect(gain);source.onended=()=>{
      if(token!==version)return;source.disconnect();source=null;buffer=null;index++;started=false;pump();
    };source.start();status=2;
  }
  async function pump(){
    if(!text||!available())return;
    if(source){status=2;return;}if(buffer){playReady(version);return;}
    if(started)return;if(index>=parts.length){status=5;return;}
    const token=version;started=true;status=1;abort=new AbortController();const controller=abort;
    const timeout=setTimeout(()=>controller.abort(),25000);
    try{const bytes=await audioBytes(parts[index],controller.signal);if(token!==version)return;const decoded=await context().decodeAudioData(bytes);if(token!==version)return;buffer=decoded;playReady(token);}
    catch{if(token===version){status=4;started=true;}}
    finally{clearTimeout(timeout);if(abort===controller)abort=null;}
  }
  function sync(){
    if(ctx){gain.gain.value=available()?volume:0;if(available()){if(ctx.state!=='running')ctx.resume().then(pump).catch(()=>{status=3;});}else{if(ctx.state==='running')ctx.suspend().catch(()=>{});if(text&&status!==4&&status!==5)status=3;}}
    if(available())pump();
  }
  function unlock(){gesture=true;context().resume().then(sync).catch(()=>{status=3;});}
  document.addEventListener('pointerdown',unlock,{capture:true});document.addEventListener('keydown',unlock,{capture:true});
  document.addEventListener('visibilitychange',sync);window.addEventListener('blur',sync);window.addEventListener('focus',sync);
  window.addEventListener('pagehide',()=>{stop();text='';raw='';});
  root.terraNarration={
    set(value,on,level){
      enabled=!!on;volume=Math.max(0,Math.min(1,level));
      if(value!==raw){
        raw=value;const next=coreText(value);
        if(next!==text){stop();text=next;parts=split(next);index=0;timer=setTimeout(()=>{timer=null;sync();},350);}
      }
      if(!timer)sync();
    },
    replay(){stop();index=0;sync();},
    status(){return text&&!available()&&status!==4?3:status;},
    audit(){return {status:this.status(),segments:parts.length,segment:index,enabled,volume,gesture,playing:!!source,revision};}
  };
})(typeof window!=='undefined'?window:globalThis);
