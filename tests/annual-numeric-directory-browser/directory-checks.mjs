import fs from 'node:fs';
import vm from 'node:vm';
import { fileURLToPath } from 'node:url';
import path from 'node:path';
const root=path.resolve(path.dirname(fileURLToPath(import.meta.url)),'../..');
import assert from 'node:assert/strict';
import { IDBFactory } from 'fake-indexeddb';
const directorySource=fs.readFileSync(path.join(root,'Assets/Plugins/WebGL/TerraNumericDirectory.jslib'),'utf8');
const runSource=fs.readFileSync(path.join(root,'Assets/Plugins/WebGL/TerraNumericStorage.jslib'),'utf8');
let checks=0, serial=0;
function check(value,message){assert.ok(value,message);checks++;}
const H=n=>n.toString(16).padStart(64,'0');
const escape=s=>encodeURIComponent(s).replace(/[!'()*]/g,c=>'%'+c.charCodeAt(0).toString(16).toUpperCase());
const world="place !'()*", scope='myterrain.numeric-directory.v1/'+escape(world)+'/schema-3/annual-numeric-v4';
const runKey=(run,definition=H(1))=>'myterrain.numeric.v1/'+escape(world)+'/schema-3/'+definition+'/'+escape(run);
const definitionKey=hash=>'myterrain.numeric-definition.v1/'+escape(world)+'/schema-3/'+hash;
const definition=JSON.stringify({schemaVersion:3,worldPackId:world,actions:[{vegetatedFraction:0.024625792811839322}]});
const entry=run=>({runId:run,definitionHash:H(1)});
const save=(run,hash=H(10))=>JSON.stringify({schemaVersion:1,sequence:1,saveHash:hash,town:{runId:run,definitionHash:H(1),history:[]},narration:[]});
function next(previous,run,create=true){return {schemaVersion:1,townSchemaVersion:3,worldPackId:world,modelFamily:'annual-numeric-v4',sequence:(previous?.sequence||0)+1,directoryHash:H(++serial+100),activeRunId:run,operationId:'intent-'+serial,operationHash:H(serial+200),runs:create?[...(previous?.runs||[]),entry(run)]:previous.runs};}
function harness(){
 const idb=new IDBFactory(), fault={}, pending=new Map();let committed=false;
 const indexedDB={open(...args){const req=idb.open(...args);req.addEventListener('success',()=>{
  const db=req.result, original=db.transaction.bind(db);
  db.transaction=(...args)=>{const tx=original(...args);committed=false;tx.addEventListener('complete',()=>{committed=true;});
   const originalStore=tx.objectStore.bind(tx);tx.objectStore=name=>{
    const store=originalStore(name);
    for(const method of ['add','put']){const originalWrite=store[method].bind(store);store[method]=(...args)=>{
     if(fault.quota===name){delete fault.quota;throw new DOMException('quota','QuotaExceededError');}
     const request=originalWrite(...args);
     if(fault.abort===name){delete fault.abort;tx.abort();}
     return request;
    };}return store;
   };return tx;
  };
 });return req;}};
 const context={LibraryManager:{library:{}},mergeInto:(a,b)=>Object.assign(a,b),UTF8ToString:x=>x,indexedDB,
  SendMessage:(_receiver,method,json)=>{const value=JSON.parse(json);if(value.ok)check(committed,'Only transaction completion acknowledges success');
   if(fault.drop){delete fault.drop;return;}const target=pending.get(value.operationId);if(target){pending.delete(value.operationId);target(value);}}
 };
 vm.runInNewContext(directorySource,context);vm.runInNewContext(runSource,context);
 function call(data,run=false,timeout=2000){return new Promise((resolve,reject)=>{
  const operationId='bridge-'+(++serial);const timer=setTimeout(()=>{pending.delete(operationId);if(timeout<2000)resolve(null);else reject(new Error('No bridge reply'));},timeout);
  pending.set(operationId,r=>{clearTimeout(timer);resolve(r);});
  const fn=run?context.LibraryManager.library.TerraNumericStorage:context.LibraryManager.library.TerraNumericDirectory;
  fn('receiver',JSON.stringify({operationId,key:scope,worldPackId:world,...data}));
 });}
 async function direct(store,key,value,version=2){const req=idb.open('MyTerrainNumericSavesV1',version);const db=await new Promise((resolve,reject)=>{req.onupgradeneeded=()=>{for(const s of version===1?['runs']:['runs','definitions','directories'])if(!req.result.objectStoreNames.contains(s))req.result.createObjectStore(s);};req.onsuccess=()=>resolve(req.result);req.onerror=()=>reject(req.error);});
  const tx=db.transaction(store,value===undefined?'readonly':'readwrite');const r=value===undefined?tx.objectStore(store).get(key):value===null?tx.objectStore(store).delete(key):tx.objectStore(store).put(value,key);
  const result=await new Promise((resolve,reject)=>{tx.oncomplete=()=>resolve(r.result);tx.onabort=()=>reject(tx.error);});db.close();return result;
 }
 const read=()=>call({operation:'read'});
 const create=(d,old=null,definitionJson=definition,saveJson=save(d.activeRunId))=>call({operation:'create',expectedHash:old?.directoryHash,directoryJson:JSON.stringify(d),definitionJson,saveJson});
 const select=(d,old,expectedSaveHash=H(10))=>call({operation:'select',expectedHash:old.directoryHash,directoryJson:JSON.stringify(d),expectedSaveHash});
 return {idb,fault,call,direct,read,create,select};
}
let h=harness();check((await h.read()).directoryJson==='','Absent directory returns absence');
let a=next(null,'run !one'), b;
check((await h.create(a)).ok,'Atomic first creation');let read=await h.read();
check(read.definitionJson===definition&&read.saveJson===save(a.activeRunId),'Exact JSON text is preserved including regression double');
check(JSON.parse(read.directoryJson).activeRunId===a.activeRunId,'Active pointer resolves to created run');
b=next(a,'second');check((await h.create(b,a)).ok,'Replay preserves first run');
read=await h.call({operation:'read-run',runId:a.activeRunId});check(read.saveJson===save(a.activeRunId),'Inactive run reads its original save');
let selected=next(b,a.activeRunId,false);check((await h.select(selected,b)).ok,'Select retained run');
check((await h.direct('runs',runKey('second'))).json===save('second'),'Selection does not rewrite another run');
check((await h.select(next(selected,'second',false),selected,H(999))).error==='run-write-conflict','Selection checks current target save hash');
check((await h.read()).directoryJson===JSON.stringify(selected),'Failed selection preserves active pointer');
let c=next(selected,'third'), d=next(selected,'fourth');const race=await Promise.all([h.create(c,selected),h.create(d,selected)]);
check(race.filter(r=>r.ok).length===1&&race.some(r=>r.error==='write-conflict'),'Concurrent creates have exactly one winner');
let current=JSON.parse((await h.read()).directoryJson);
const selectRace=await Promise.all([h.select(next(current,'second',false),current),h.select(next(current,a.activeRunId,false),current)]);
check(selectRace.filter(r=>r.ok).length===1,'Concurrent selections have exactly one winner');
current=JSON.parse((await h.read()).directoryJson);
check((await h.create(next(current,'second'),current)).error==='invalid-request','Duplicate run identity rejected');
check((await h.call({operation:'read-run',runId:'absent'})).error==='missing-run','Unknown retained run is an error');
for(const stage of ['definitions','runs','directories']){
 h=harness();a=next(null,'abort-'+stage);h.fault.abort=stage;check(!(await h.create(a)).ok,'Abort after '+stage+' cannot acknowledge');
 check((await h.read()).directoryJson==='','Abort leaves no active pointer');
 check(await h.direct('runs',runKey(a.activeRunId))===undefined,'Abort rolls back inserted run');
 check(await h.direct('definitions',definitionKey(H(1)))===undefined,'Abort rolls back inserted definition');
}
h=harness();a=next(null,'quota');h.fault.quota='runs';check((await h.create(a)).error==='quota-exceeded','Synchronous quota error is explicit');check((await h.read()).directoryJson==='','Quota abort preserves absent directory');
h=harness();a=next(null,'lost-ack');h.fault.drop=true;check(await h.call({operation:'create',directoryJson:JSON.stringify(a),definitionJson:definition,saveJson:save(a.activeRunId)},false,50)===null,'Lost acknowledgement remains unknown');
check(JSON.parse((await h.read()).directoryJson).activeRunId==='lost-ack','Recovery reads the committed lost-ack selection');
for(const kind of ['definition','run','directory']){
 h=harness();a=next(null,'corrupt');await h.create(a);
 const store=kind==='definition'?'definitions':kind==='run'?'runs':'directories';const key=kind==='definition'?definitionKey(H(1)):kind==='run'?runKey(a.activeRunId):scope;
 await h.direct(store,key,{hash:H(1),json:'{}'});check(!(await h.read()).ok,'Corrupt '+kind+' cannot become absence');
}
h=harness();const oldRun=runKey('v1');await h.direct('runs',oldRun,{hash:H(10),json:save('v1')},1);
check((await h.read()).ok,'V1 database upgrades through directory bridge');check((await h.direct('runs',oldRun)).json===save('v1'),'Upgrade preserves v1 run record');
let runReply=await h.call({operation:'read',key:oldRun},true);check(runReply.ok&&runReply.value===save('v1'),'Existing per-run bridge opens upgraded v2 database');
h=harness();runReply=await h.call({operation:'write',key:oldRun,value:save('v1')},true);check(runReply.ok,'Per-run bridge can create v2 first');check((await h.read()).ok,'Directory stores exist after per-run-first upgrade');
h=harness();a=next(null,'immutable');await h.create(a);b=next(a,'replay');check((await h.create(b,a,definition.replace('0.024625792811839322','0.02462579281183932'))).error==='definition-conflict','Immutable definition cannot be replaced under same hash');
check(JSON.parse((await h.read()).directoryJson).activeRunId==='immutable','Definition conflict preserves pointer');
for(const kind of ['definition','run']){
 h=harness();a=next(null,'missing');await h.create(a);
 await h.direct(kind==='definition'?'definitions':'runs',kind==='definition'?definitionKey(H(1)):runKey(a.activeRunId),null);
 check(!(await h.read()).ok,'Missing '+kind+' is an error, never absence');
}
h=harness();a=next(null,'orphan');await h.direct('runs',runKey(a.activeRunId),{hash:H(10),json:save(a.activeRunId)});
check((await h.create(a)).error==='run-already-exists','Create cannot overwrite an orphaned pre-existing run');
check((await h.read()).directoryJson==='','Rejected overwrite does not create a directory');
h=harness();a=next(null,'keep');await h.create(a);b=next(a,'second');await h.create(b,a);
let bad=next(b,'second',false);bad.runs=bad.runs.slice(1);check((await h.select(bad,b)).error==='invalid-transition','Selection cannot remove prior entries');
bad=next(b,'second',false);bad.runs=bad.runs.map(r=>({...r,definitionHash:H(999)}));check((await h.select(bad,b)).error==='invalid-transition','Selection cannot replace retained rules');
bad=next(b,'third');bad.sequence++;check((await h.create(bad,b)).error==='invalid-transition','Directory sequence cannot skip');
bad=next(b,'third');bad.worldPackId='other';check((await h.create(bad,b)).error==='invalid-request','Directory cannot cross bound world');
check((await h.read()).directoryJson===JSON.stringify(b),'Rejected transitions preserve the previous exact directory');
console.log(`PASS ${checks} IndexedDB model assertions; fake-indexeddb6.2.6, not actual-browser acceptance.`);
