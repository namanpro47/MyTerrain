// Browser-transport lifecycle checks. The actual Unity player is checked in Chrome.
const {readFileSync}=require('node:fs');
const vm=require('node:vm');
const assert=require('node:assert/strict');
const source=readFileSync('Assets/WebGLTemplates/Terra/player.js','utf8');
function boot(){
 const elements={};
 for(const id of ['unity-canvas','loading','status','progress','retry','.arrival']) elements[id]={
  hidden:id==='retry',dataset:{},attributes:{},classList:{add(){}},
  addEventListener(){},setAttribute(k,v){this.attributes[k]=v;},removeAttribute(k){delete this.attributes[k];},
  focus(){this.focused=true;},getBoundingClientRect(){return {left:0,top:0,width:100,height:100};}
 };
 const events={},frames=[];let script,resolve,reject,advance;
 const context={document:{getElementById:id=>elements[id],querySelector:s=>elements[s],
  createElement:()=>({}),body:{appendChild:s=>script=s},addEventListener(){}},
  window:{TERRA_BUILD:{loader:'loader.js'},addEventListener:(n,f)=>events[n]=f},
  console:{info(){},error(){},warn(){}},performance:{now:()=>1},
  requestAnimationFrame:f=>frames.push(f),
  createUnityInstance:(canvas,config,progress)=>{advance=progress;return new Promise((a,b)=>{resolve=a;reject=b;});}
 };
 vm.runInNewContext(source,context);
 return {elements,events,frames,script,start(){script.onload();},progress:p=>advance(p),
  resolve:async()=>{resolve({SendMessage(){}});await Promise.resolve();},
  reject:async()=>{reject(new Error('test startup rejection'));await Promise.resolve();await Promise.resolve();},
  paint(){while(frames.length)frames.shift()();}};
}
(async()=>{
 for(const order of ['native-first','instance-first']){
  const b=boot();b.start();b.progress(.42);assert.equal(b.elements.progress.value,.42);
  b.progress(1);assert.equal(b.elements['.arrival'].dataset.stage,'starting');
  if(order==='native-first')b.events['myterrain-ready']();else await b.resolve();
  b.paint();assert.equal(b.elements.loading.hidden,false,'one readiness signal must not expose a blank canvas');
  if(order==='native-first')await b.resolve();else b.events['myterrain-ready']();
  assert.equal(b.elements.loading.hidden,false,'keep preview until paint');
  b.paint();assert.equal(b.elements.loading.hidden,true);assert.equal(b.elements['unity-canvas'].focused,true);
 }
 for(const failure of ['download','startup','between-ready-and-paint']){
  const b=boot();
  if(failure==='download')b.script.onerror();
  else{b.start();if(failure==='startup')await b.reject();else{await b.resolve();b.events['myterrain-ready']();b.script.onerror();}}
  b.paint();b.events['myterrain-ready']();b.paint();
  assert.equal(b.elements.loading.hidden,false);assert.equal(b.elements.retry.hidden,false);
  assert.equal(b.elements['unity-canvas'].tabIndex,-1);
 }
 console.log('PASS: both readiness orders, paint boundary, download/startup failures and error during handoff');
})();
