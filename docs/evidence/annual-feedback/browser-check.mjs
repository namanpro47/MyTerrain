import {createRequire} from 'node:module';
import fs from 'node:fs/promises';
const require=createRequire(import.meta.url);
const {chromium}=require('/Users/naman/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/playwright');
const output=new URL('./chrome-final/',import.meta.url).pathname;await fs.mkdir(output,{recursive:true});
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
try{
 await page.goto(process.env.TERRA_TEST_URL||'http://127.0.0.1:8896/');
 await page.waitForFunction(()=>window.terraPlayer,null,{timeout:180000});await page.waitForTimeout(2000);
 await page.screenshot({path:output+'atlas-1440.png'});
 await key('1');await key('Enter');
 await page.waitForTimeout(1000);
 for(let i=0;i<120&&!logs.some(l=>l.includes('MYTERRAIN_ARRIVAL'));i++)await page.waitForTimeout(500);
 await page.waitForTimeout(2000);
 let a=await capture('arrival-1440');await click(/^Choose your run/);
 for(const years of [1,5,10,3])await click(new RegExp('^'+years+' years?'));
 a=await capture('setup-1440');check(visible(a,'3 years · selected'),'Duration selection lost');
 await page.setViewportSize({width:1024,height:576});await page.waitForTimeout(500);await capture('setup-1024');
 await click('Begin · 3 decision years');await page.setViewportSize({width:1440,height:900});await page.waitForTimeout(500);
 await key('1');a=await capture('selected-before-placement');
 check(visible(a,/upfront/),'Selected tool has no price');check(!visible(a,/Living Margin/),'Removed tool remains visible');
 for(let i=0;i<30;i++){await key('Tab');await key('Enter');a=await audit();if(a.state.includes('queued=1'))break;}
 check(a.state.includes('queued=1'),'First farm not queued');
 await click('End Year');a=await capture('budget-1440');check(visible(a,/Tax receipts to collect/),'Tax line missing');check(!visible(a,/All [0-9]+ proposals/),'Nested proposals remain');
 await page.setViewportSize({width:1024,height:576});await page.waitForTimeout(500);await capture('budget-1024');await page.mouse.move(500,280);await page.mouse.wheel(0,1000);await page.waitForTimeout(300);await capture('budget-1024-scrolled');
 await click('End Year');a=await waitState(a=>a.state.includes('phase=Reviewing'));check(a.state.includes('year=1'),'Wrong first review');
 await story();await click('Read chapter →');await capture('chapter-budget-1024');await click('Next card →');await capture('chapter-people-1024');await click('Next card →');a=await capture('chapter-land-1024');
 check(visible(a,/Environmental.*100/),'Environmental progress missing');
 await page.mouse.move(500,280);await page.mouse.wheel(0,1200);await page.waitForTimeout(300);await capture('chapter-land-1024-scrolled');
 await click('Plan year 2 →');await click('Settings');await click('Text: standard');await key('Escape');await key('l');await story();await click('Read chapter →');await click('Next card →');await capture('chapter-people-1024-larger');await click('Next card →');a=await capture('chapter-land-1024-larger');check(a.largeText,'Larger text did not persist');
 check(a.controls.filter(c=>c.enabled).every(c=>c.rect.x>=-1&&c.rect.y>=-1&&c.rect.x+c.rect.width<=1025&&c.rect.y+c.rect.height<=577),'A reading control is outside compact viewport');
 await page.mouse.move(500,280);await page.mouse.wheel(0,1200);await page.waitForTimeout(300);await capture('chapter-land-1024-larger-scrolled');await click('Return to plan');await page.setViewportSize({width:1440,height:900});await page.waitForTimeout(500);
 await key('2');a=await audit();for(let i=0;i<40&&!visible(a,'Review change · Enter');i++){await key('Tab');a=await audit();}
 check(visible(a,'Review change · Enter'),'Accepted farm cannot be converted');await click('Review change · Enter');a=await capture('conversion-review');check(visible(a,/Removal, site preparation/),'Conversion cost not explained');await click('Queue land-use change');
 a=await audit();check(a.state.includes('queued=1')&&a.state.includes('assets=1'),'Conversion replaced accepted state before End Year');
 await endYear();await click(/^Plan year 3/);await endYear();a=await capture('last-decision-review');check(visible(a,/Review Year 10/),'Short run offers more decisions');
 await click(/Review Year 10/);a=await waitState(a=>a.state.includes('year=10')&&a.state.includes('phase=Completed'));await capture('short-run-year10');
 await story();a=await capture('decade-narrative');check(visible(a,/changed direction/),'Decade omits conversions');
 await key('Escape');await key('Escape');await click('More');await click('New run');await capture('replay-confirmation');await click('Start new run');
 a=await waitState(a=>a.state.includes('year=1')&&a.state.includes('phase=Planning'));await capture('replay-success');check(a.state.includes('assets=0'),'Replay retained old assets');
 await click(/^Choose your run/);await click(/^1 year/);await click('Begin · 1 decision year');
 await click('Homes & Community');
 for(let count=1;count<=2;count++){
  await click('Apartments');
  for(let i=0;i<40;i++){await key('Tab');await key('Enter');a=await audit();if(a.state.includes('queued='+count))break;}
  check(a.state.includes('queued='+count),'Listed apartment cost rejected despite available current cash');
 }
 a=await capture('two-apartments-current-budget');check(visible(a,/\$0 .*available/),'Two $5M apartments do not leave exactly zero available from $10M');
 await endYear();await click(/Review Year 10/);
 a=await waitState(a=>a.state.includes('year=10')&&a.state.includes('phase=Completed'));const saved=a.state;await capture('one-decision-year10');
 await page.reload();await page.waitForFunction(()=>window.terraPlayer,null,{timeout:180000});await page.waitForTimeout(2000);await key('1');await key('Enter');await page.waitForTimeout(10000);
 a=await capture('reload-year10');check(a.state===saved,'Reload changed completed run');
 check(!logs.some(l=>/PlayerPrefsException|NullReferenceException|ArgumentException:/.test(l)),'Runtime exception observed');
 await fs.writeFile(output+'checks.json',JSON.stringify({assertions,source:'e74496bf55ffa77427be69dcb221bb3bd70f3b41',version:browser.version(),errors,saved},null,2));
}catch(e){await page.screenshot({path:output+'failure.png'});await fs.writeFile(output+'failure-logs.json',JSON.stringify(logs,null,2));throw e;}finally{await browser.close();}
