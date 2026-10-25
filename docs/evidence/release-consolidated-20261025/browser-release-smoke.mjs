import {createRequire} from 'node:module';
import fs from 'node:fs/promises';
const require=createRequire(import.meta.url);
const {chromium}=require('/Users/naman/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/playwright');
const output=new URL('./chrome-release/',import.meta.url).pathname;await fs.mkdir(output,{recursive:true});
const visual=process.env.TERRA_CAPTURE!=='0';
const browser=await chromium.launch({executablePath:'/Applications/Google Chrome.app/Contents/MacOS/Google Chrome',headless:true,args:['--use-angle=metal','--disable-background-timer-throttling','--disable-renderer-backgrounding']});
const context=await browser.newContext({viewport:{width:1440,height:900},deviceScaleFactor:1});
const page=await context.newPage();const logs=[],errors=[];let assertions=0;
page.on('console',m=>{logs.push(m.text());if(m.type()==='error')errors.push(m.text());});page.on('pageerror',e=>errors.push(String(e)));
await page.route('**/*',route=>{const u=new URL(route.request().url());return /\/(chapter|session)$/.test(u.pathname)?route.abort():route.continue();});
function check(value,message){if(!value)throw Error(message);assertions++;}
async function key(k){await page.locator('#unity-canvas').focus();await page.keyboard.press(k,{delay:200});await page.waitForTimeout(350);}
async function audit(){
 const start=logs.length;await key('F10');
 for(let i=0;i<50;i++){
  const fresh=logs.slice(start),state=fresh.find(l=>l.includes('TERRA_ANNUAL_STATE: '));
  const line=fresh.find(l=>l.includes('TERRA_UI_AUDIT: '));let parsed;
  if(line)try{parsed=JSON.parse(line.slice(line.indexOf('TERRA_UI_AUDIT: ')+16));}catch{}
  if(!parsed){
   const parts=fresh.map(l=>l.match(/TERRA_UI_PART (\d+)\/(\d+): ([\s\S]*)/)).filter(Boolean);
   if(parts.length&&parts.length===Number(parts[0][2]))
    parsed=JSON.parse(parts.sort((a,b)=>Number(a[1])-Number(b[1])).map(p=>p[3].replace(/\n$/,'')).join(''));
  }
  if(parsed&&state)return {...parsed,state};
  await page.waitForTimeout(100);
 }
 throw Error('Missing UI audit');
}
async function click(text){const a=await audit();const label=a.labels.find(l=>text instanceof RegExp?text.test(l.text):l.text===text);check(label,'Missing control '+text);await page.mouse.click(label.rect.x+label.rect.width/2,label.rect.y+label.rect.height/2);await page.waitForTimeout(300);}
async function capture(name){const a=await audit();console.log(name,a.state);await fs.writeFile(output+name+'.json',JSON.stringify(a,null,2));if(visual)await page.screenshot({path:output+name+'.png'});return a;}
async function waitState(test){for(let i=0;i<100;i++){const a=await audit();if(test(a))return a;await page.waitForTimeout(300);}throw Error('Expected game state did not arrive');}
async function endYear(){await click('End Year');await click('End Year');return waitState(a=>a.state.includes('phase=Reviewing')||a.state.includes('phase=Completed'));}
async function story(){await click(/^(Year story|Local summary|AI explanation)/);}
function visible(a,text){return a.labels.some(l=>typeof text==='string'?l.text===text:text.test(l.text));}
async function readyChapter(){
 for(let i=0;i<30;i++){
  const a=await audit();if(visible(a,'Read chapter →'))return;
  if(visible(a,'Use local summary'))await click('Use local summary');
  else if(visible(a,/^(Year story|Local summary|AI explanation)/))await story();
  await page.waitForTimeout(300);
 }
 throw Error('Annual chapter did not appear');
}
try{
 await page.goto(process.env.TERRA_TEST_URL||'http://127.0.0.1:8896/');
 await page.waitForFunction(()=>window.terraPlayer,null,{timeout:180000});await page.waitForTimeout(2000);
 await key('1');await key('Enter');
 for(let i=0;i<120&&!logs.some(l=>l.includes('MYTERRAIN_ARRIVAL'));i++)await page.waitForTimeout(500);
 await page.waitForTimeout(1500);await click(/^Choose your run/);
 await click(/^10 years/);await click('Begin · 10 decision years');
 await click('Homes & Community');await click('Apartments');
 let a=await audit();
 for(let i=0;i<50;i++){await key('Tab');await key('Enter');a=await audit();if(a.state.includes('queued=1'))break;}
 check(a.state.includes('queued=1'),'Project did not queue');await capture('placed');
 await endYear();a=await capture('year-accepted');const saved=a.state;
 check(saved.includes('phase=Reviewing'),'Year not accepted');
 await page.reload();await page.waitForFunction(()=>window.terraPlayer,null,{timeout:180000});await page.waitForTimeout(1500);await key('1');await key('Enter');
 a=await waitState(a=>a.state.includes('phase=Reviewing'));await capture('reload');
 check(a.state===saved,'Reload changed accepted state');
 check(!logs.some(l=>/PlayerPrefsException|NullReferenceException|ArgumentException:|RuntimeError|unreachable/.test(l))&&!errors.some(l=>/RuntimeError|unreachable/.test(l)),'Runtime save failure observed');
 await fs.writeFile(output+'checks.json',JSON.stringify({assertions,source:'a3eea78',version:browser.version(),errors,saved},null,2));
}catch(e){await page.screenshot({path:output+'failure.png'});await fs.writeFile(output+'failure-logs.json',JSON.stringify(logs,null,2));throw e;}finally{await browser.close();}
