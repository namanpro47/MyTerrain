// Browser-bridge checks: no network calls; denied clipboard leaves a selectable fallback.
const fs=require('fs'),vm=require('vm'),assert=require('assert');
const code=fs.readFileSync('Assets/Plugins/LocalFollowup.jslib','utf8');
let library={},elements=[],downloads=0,revoked=0;
const document={body:{appendChild(e){elements.push(e)}},createElement(tag){return {tag,style:{},focus(){},select(){this.selected=true},setAttribute(k,v){this[k]=v},click(){downloads++},remove(){elements=elements.filter(x=>x!==this)}}},execCommand(){return false}};
const context={LibraryManager:{library},mergeInto:Object.assign,UTF8ToString:x=>x,Blob,document,navigator:{clipboard:{writeText:()=>Promise.reject(new Error('denied'))}},URL:{createObjectURL:()=> 'blob:preview',revokeObjectURL:()=>revoked++},setTimeout:fn=>fn()};
vm.runInNewContext(code,context);
(async()=>{
 library.LocalFollowupDownload('plan.ics','text/calendar','BEGIN:VCALENDAR');
 assert.equal(downloads,1);assert.equal(revoked,1);assert.equal(elements.length,0);
 library.LocalFollowupCopy('A user-selected plan');await new Promise(resolve=>setImmediate(resolve));
 assert.equal(elements.length,1);assert.equal(elements[0].value,'A user-selected plan');assert(elements[0].selected);
 elements[0].onkeydown({key:'Escape'});assert.equal(elements.length,0);
 console.log('7 browser bridge assertions passed.');
})().catch(e=>{console.error(e);process.exitCode=1});
