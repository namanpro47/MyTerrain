import fs from 'node:fs';
import vm from 'node:vm';
import assert from 'node:assert/strict';
import zlib from 'node:zlib';
const library={}, strings=new Map(); let next=1, checks=0;
const context={LibraryManager:{library},mergeInto:Object.assign,Uint8Array,DataView,TextDecoder,TextEncoder,console:{error(){}},btoa,atob,UTF8ToString:p=>strings.get(p),lengthBytesUTF8:s=>Buffer.byteLength(s),_malloc:()=>next++,stringToUTF8:(s,p)=>strings.set(p,s)};
vm.createContext(context);vm.runInContext(fs.readFileSync('Assets/Plugins/WebGL/TerraSaveCodec.jslib','utf8'),context);
for(const [k,v] of Object.entries(library))if(k.startsWith('$')&&!k.endsWith('__deps'))context[k.slice(1)]=v;
const call=(fn,value)=>{const p=next++;strings.set(p,value);const out=library[fn](p);return out?strings.get(out):null;};
const prefix='terra-gzip-v1:';
function check(value,message){assert.ok(value,message);checks++;}
for(const source of ['', '{"year":10,"story":"Meadow 🌾 — café 日本語"}', zlib.gunzipSync(fs.readFileSync('docs/evidence/annual-feedback/restore-fixture/save-roundtrip-original.json.gz')).toString(), JSON.stringify(Array.from({length:4000},(_,i)=>({year:i%10,score:12899.999713897705,story:'Land and community '+i})))]){
 const encoded=call('TerraSaveEncode',source);check(encoded?.startsWith(prefix),'browser encodes');
 check(zlib.gunzipSync(Buffer.from(encoded.slice(prefix.length),'base64')).toString()===source,'native-compatible GZIP');
 check(call('TerraSaveDecode',encoded)===source,'browser exact roundtrip');
 check(call('TerraSaveDecode',prefix+zlib.gzipSync(source).toString('base64'))===source,'native gzip browser decode');
}
for(const invalid of ['x',prefix+'not-base64',prefix+'AAAA',prefix+'A'.repeat(1398108)])check(call('TerraSaveDecode',invalid)===null,'malformed input rejected');
const good=zlib.gzipSync('exact saved history');
for(const offset of [good.length-8,good.length-4]){const corrupt=Buffer.from(good);corrupt[offset]^=1;check(call('TerraSaveDecode',prefix+corrupt.toString('base64'))===null,'CRC/size mismatch rejected');}
const bomb=Buffer.from(good);bomb.writeUInt32LE(33*1024*1024,bomb.length-4);check(call('TerraSaveDecode',prefix+bomb.toString('base64'))===null,'oversized decoded output rejected');
const truncated=good.subarray(0,good.length-3);check(call('TerraSaveDecode',prefix+truncated.toString('base64'))===null,'truncated gzip rejected');
console.log(`Browser save codec: ${checks} checks passed`);
