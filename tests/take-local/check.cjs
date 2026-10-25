const assert=require('node:assert/strict');const vm=require('node:vm');const fs=require('node:fs');
const code=fs.readFileSync('Assets/WebGLTemplates/Terra/take-local/app.js','utf8');
function boot(saved,denied=false){const elements={};const values={};const context={console,Date,URL,Blob,setTimeout,navigator:{},location:{href:'https://example.test/take-local/'},localStorage:{getItem:()=>saved,setItem:(k,v)=>{if(denied)throw Error();values[k]=v},removeItem:()=>{}},document:{getElementById:id=>elements[id]??={innerHTML:'',textContent:'',addEventListener(){},querySelector(){return {focus(){}}}},createElement:()=>({click(){}})},window:{scrollTo(){},open(){}}};vm.createContext(context);vm.runInContext(code,context);return {run:s=>vm.runInContext(s,context),elements,values};}
let c=boot('{broken');assert.match(c.elements.app.innerHTML,/Take it local/);
c.run("state.when='<img src=x onerror=alert(1)>';go(3)");assert(!c.elements.app.innerHTML.includes('<img'));assert.match(c.elements.app.innerHTML,/&lt;img/);
c.run("state.step=2;state.level=2;state.draft='My own edited draft';render()");assert.match(c.elements.app.innerHTML,/My own edited draft/);
c.run("state.reason=2;render()");assert.match(c.elements.app.innerHTML,/My own edited draft/);
for(let i=0;i<4;i++){c.run(`state.level=${i};state.step=2;render()`);assert(!c.elements.app.innerHTML.includes('undefined'));c.run('go(3)');assert(!c.elements.app.innerHTML.includes('undefined'));}
c.run("state.date='2026-11-02T10:30';download=(n,s,t)=>{globalThis.file={n,s,t}};calendar()");assert.match(c.run('file.s'),/BEGIN:VCALENDAR\r\n/);assert.match(c.run('file.s'),/DTSTART:20261102T/);assert.match(c.run('file.n'),/\.ics$/);
c.run("state.showCity=false;state.showNext=true;state.draft='PRIVATE';state.when='PRIVATE';state.date='PRIVATE'");assert(!c.run('shareText()').includes('PRIVATE'));assert(!c.run('shareText()').includes('Point Reyes Station'));
c=boot(JSON.stringify({step:999,level:99,habit:99,checkInAt:1}));assert.match(c.elements.app.innerHTML,/Time for your check-in/);assert.equal(c.run('state.level'),3);assert.equal(c.run('state.habit'),5);
c=boot(null,true);assert.equal(c.run('save()'),false);assert.match(c.elements.notice.textContent,/storage is unavailable/);
console.log('PASS: recovery, escaping, draft preservation, all branches, calendar, share privacy, due check-in, denied storage');
