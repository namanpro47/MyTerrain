// Server-only ElevenLabs gateway. Never copy .env into the Web export.
const {createHash} = require('node:crypto');
const fs = require('node:fs/promises');
const path = require('node:path');
const os = require('node:os');
const DEFAULT_VOICE = 'XrExE9yKIg1WjnnlVkGX'; // Matilda: warm, clear female narrator.
const MODEL = 'eleven_flash_v2_5';
const MAX_TEXT = 700;
const SPEED = 1.10;
function createHandler({env=process.env, fetcher=fetch, cacheDir=path.join(os.tmpdir(),'myterrain-speech-v1'), now=Date.now}={}) {
  const pending = new Map(), limits = new Map();
  let active=0;
  const voice=env.ELEVENLABS_VOICE_ID || DEFAULT_VOICE;
  const model=env.ELEVENLABS_MODEL || MODEL;
  const revision=createHash('sha256').update(`${voice}:${model}:0.55:0.75:${SPEED}:v2`).digest('hex').slice(0,16);
  const allowed=new Set((env.TERRA_VOICE_ORIGINS || 'https://www.myterrain.io,https://myterrain.io,https://terra-browser.vercel.app').split(',').map(x=>x.trim()));
  // Vercel supplies this deployment's own hostname; do not allow arbitrary *.vercel.app origins.
  if(env.VERCEL_URL)allowed.add('https://'+env.VERCEL_URL);
  function json(res,status,error) {res.statusCode=status;res.setHeader('Content-Type','application/json');res.end(JSON.stringify({error}));}
  async function read(req) {
    if (req.body !== undefined) return typeof req.body==='string' ? JSON.parse(req.body) : req.body;
    let size=0, chunks=[];
    for await (const chunk of req) {size+=chunk.length;if(size>4096)throw new Error('size');chunks.push(chunk);}
    return JSON.parse(Buffer.concat(chunks).toString('utf8'));
  }
  return async function handler(req,res) {
    res.setHeader('Cache-Control','no-store');res.setHeader('X-Content-Type-Options','nosniff');
    const origin=req.headers.origin;
    const local=env.VERCEL!=='1' && /^(localhost|127\.0\.0\.1)(:\d+)?$/.test(req.headers.host || '');
    if(origin && !allowed.has(origin) && !(local && /^http:\/\/(localhost|127\.0\.0\.1):\d+$/.test(origin)))return json(res,403,'Origin not allowed.');
    if(!origin && !local && req.headers['sec-fetch-site']!=='same-origin')return json(res,403,'Open narration from the game.');
    if(origin){res.setHeader('Access-Control-Allow-Origin',origin);res.setHeader('Vary','Origin');res.setHeader('Access-Control-Allow-Headers','Content-Type');res.setHeader('Access-Control-Allow-Methods','GET, POST, OPTIONS');}
    if(req.method==='OPTIONS'){res.statusCode=204;return res.end();}
    if(req.method==='GET') {res.setHeader('Content-Type','application/json');return res.end(JSON.stringify({configured:!!env.ELEVENLABS_API_KEY,voice:env.ELEVENLABS_VOICE_NAME||'Matilda',revision,speed:SPEED,maxCharacters:MAX_TEXT}));}
    if(req.method!=='POST'){res.setHeader('Allow','GET, POST, OPTIONS');return json(res,405,'Method not allowed.');}
    let input;
    try {input=await read(req);}catch{return json(res,400,'Invalid speech request.');}
    if(!input || typeof input.text!=='string' || Object.keys(input).some(k=>k!=='text') || !input.text.trim() || input.text.length>MAX_TEXT || /[<>\x00-\x08]/.test(input.text))return json(res,400,'Speech must contain 1–700 plain-text characters.');
    const text=input.text.trim();
    const key=createHash('sha256').update(revision+'\n'+text).digest('hex');
    const file=path.join(cacheDir,key+'.mp3');
    function audio(bytes) {res.statusCode=200;res.setHeader('Content-Type','audio/mpeg');res.setHeader('X-Narration-Revision',revision);res.end(bytes);}
    try {const bytes=await fs.readFile(file);return audio(bytes);}catch{}
    if(!env.ELEVENLABS_API_KEY)return json(res,503,'Voice narration is not configured. Text remains available.');
    // Per-instance abuse throttle, with the provider key's hard credit cap as the global budget boundary.
    const ip=String(req.headers['x-forwarded-for']||req.socket?.remoteAddress||'unknown').split(',')[0];
    const minute=Math.floor(now()/60000);let limit=limits.get(ip);
    if(!limit || limit.minute!==minute){limit={minute,characters:0,requests:0};limits.set(ip,limit);}
    if(limits.size>1000)for(const [id,l] of limits)if(l.minute!==minute)limits.delete(id);
    if(limit.requests>=30 || limit.characters+text.length>10000){res.setHeader('Retry-After','60');return json(res,429,'Voice limit reached. Please try again later.');}
    limit.requests++;limit.characters+=text.length;
    if(!pending.has(key)) {
      if(active>=3)return json(res,429,'Narrator is busy. Please try again.');
      active++;
      const generate=(async()=>{
        const response=await fetcher(`https://api.elevenlabs.io/v1/text-to-speech/${encodeURIComponent(voice)}?output_format=mp3_44100_128`,{
          method:'POST',headers:{'xi-api-key':env.ELEVENLABS_API_KEY,'Content-Type':'application/json','Accept':'audio/mpeg'},
          body:JSON.stringify({text,model_id:model,voice_settings:{stability:.55,similarity_boost:.75,speed:SPEED}}),signal:AbortSignal.timeout(20000)
        });
        if(!response.ok){const error=new Error('Voice unavailable. Text remains available.');error.status=response.status===429?429:503;throw error;}
        const bytes=Buffer.from(await response.arrayBuffer());
        if(bytes.length<100 || bytes.length>2*1024*1024 || !(response.headers.get('content-type')||'').includes('audio'))throw new Error('Invalid voice response');
        await fs.mkdir(cacheDir,{recursive:true});
        // Bounded process cache on local disk / Vercel's ephemeral filesystem.
        const entries=await fs.readdir(cacheDir);
        if(entries.length>=400)await Promise.all(entries.slice(0,100).filter(x=>/^[a-f0-9]+\.mp3$/.test(x)).map(x=>fs.unlink(path.join(cacheDir,x)).catch(()=>{})));
        await fs.writeFile(file,bytes);return bytes;
      })().finally(()=>{pending.delete(key);active--;});
      pending.set(key,generate);
    }
    try{return audio(await pending.get(key));}catch(error){return json(res,error.status||503,'Voice unavailable. Read the text or replay to retry.');}
  };
}
module.exports=createHandler();module.exports.createHandler=createHandler;
