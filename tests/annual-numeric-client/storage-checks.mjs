import fs from 'node:fs';
import vm from 'node:vm';
import assert from 'node:assert/strict';
const source = fs.readFileSync('Assets/Plugins/WebGL/TerraNumericStorage.jslib','utf8');
let checks=0;
function check(value,message) { assert.ok(value,message);checks++; }
const candidate = {schemaVersion:1,sequence:1,saveHash:'a'.repeat(64),town:{runId:'fixture'},narration:[]};
function harness(options={}) {
 const replies=[]; let put=null, closes=0, opens=0;
 const read={}; const open={};
 const transaction={error:null,abort(){this.onabort();},objectStore(){return {
   get(){if(options.getError)throw options.getError;return read;},
   put(value){if(options.putError)throw options.putError;put=value;}
 };}};
 const db={objectStoreNames:{contains:()=>true},close(){closes++;},transaction(...args){check(args[1]==='readonly'||args[2].durability==='strict','Write requests strict durability');return transaction;}};
 const context={LibraryManager:{library:{}}, mergeInto:(a,b)=>Object.assign(a,b), UTF8ToString:x=>x,
  SendMessage:(receiver,method,value)=>{check(receiver==='receiver'&&method==='OnNumericStorageReply','Callback address');replies.push(JSON.parse(value));},
  indexedDB:{open(){opens++;if(options.openError)throw options.openError;return open;}}};
 vm.runInNewContext(source,context);
 const invoke=(operation='write',expectedHash=null,value=candidate)=>context.LibraryManager.library.TerraNumericStorage('receiver',JSON.stringify({
  operationId:'test',key:'myterrain.numeric.v1/test/schema-3/hash/run',operation,expectedHash,value:JSON.stringify(value)}));
 const ready=()=>{open.result=db;open.onsuccess();};
 const found=value=>{read.result=value;read.onsuccess();};
 return {invoke,ready,found,replies,open,transaction,get put(){return put;},get closes(){return closes;},get opens(){return opens;}};
}
let h=harness();h.invoke();h.ready();h.found(undefined);
check(h.put.json===JSON.stringify(candidate)&&h.replies.length===0,'Put success does not acknowledge');
h.transaction.oncomplete();check(h.replies.length===1&&h.replies[0].ok,'Transaction completion acknowledges');
h.transaction.onabort();check(h.replies.length===1,'Duplicate completion ignored');
h=harness();h.invoke();h.ready();h.found(undefined);h.transaction.error={name:'QuotaExceededError'};h.transaction.onabort();
check(!h.replies[0].ok&&h.replies[0].error==='quota-exceeded','Failure after put cannot become successful save');
h=harness();h.invoke('write','b'.repeat(64));h.ready();h.found({hash:'c'.repeat(64),json:'old'});
check(h.put===null&&h.replies[0].error==='write-conflict','Stale writer aborts before put');
h=harness();h.invoke('read');h.ready();h.found({hash:candidate.saveHash,json:JSON.stringify(candidate)});
check(h.replies.length===0,'Read waits for transaction completion');h.transaction.oncomplete();
check(h.replies[0].value===JSON.stringify(candidate),'Read returns complete envelope');
h=harness();h.invoke('read');h.ready();h.found(undefined);h.transaction.oncomplete();check(h.replies[0].value==='','Absent run is distinct from failed read');
h=harness();h.invoke('read');h.ready();h.found({hash:'broken'});check(h.replies[0].error==='corrupt-record','Malformed record rejected');
for(const field of ['getError','putError']) {h=harness({[field]:{name:'QuotaExceededError'}});h.invoke();h.ready();if(field==='putError')h.found(undefined);check(h.replies[0].error==='quota-exceeded','Synchronous request error aborts');}
h=harness({openError:{name:'SecurityError'}});h.invoke();check(h.replies[0].error==='storage-unavailable','Unavailable storage fails explicitly');
h=harness();h.invoke();h.open.onblocked();h.ready();check(h.replies.length===1&&h.replies[0].error==='storage-blocked'&&h.closes===1,'Late blocked open closes without write');
h=harness();h.invoke('write',null,{...candidate,saveHash:'bad'});check(h.opens===0&&h.replies[0].error==='invalid-envelope','Invalid envelope rejected before opening');
console.log(`PASS ${checks} bridge fault assertions; mocked IndexedDB, not browser acceptance.`);
