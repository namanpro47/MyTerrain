import {createRequire} from 'node:module';
import fs from 'node:fs/promises';
const require=createRequire(import.meta.url);
const {chromium}=require('/Users/naman/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/playwright');
const output=new URL('.',import.meta.url).pathname;
const browser=await chromium.launch({executablePath:'/Applications/Google Chrome.app/Contents/MacOS/Google Chrome',headless:true,args:['--use-angle=metal','--disable-background-timer-throttling','--disable-renderer-backgrounding']});
const context=await browser.newContext({viewport:{width:1440,height:900},deviceScaleFactor:1});
const page=await context.newPage();const logs=[],errors=[];let assertions=0;
page.on('console',m=>{logs.push(m.text());if(m.type()==='error')errors.push(m.text());});page.on('pageerror',e=>errors.push(String(e)));
await page.route('http://127.0.0.1:8787/**',route=>route.abort());
function check(value,message){if(!value)throw Error(message);assertions++;}
async function key(k){await page.locator('#unity-canvas').focus();await page.keyboard.press(k,{delay:200});await page.waitForTimeout(350);}
async function audit(){const start=logs.length;await key('F10');for(let i=0;i<30;i++){const line=logs.slice(start).find(l=>l.includes('TERRA_UI_AUDIT: '));if(line){const parsed=JSON.parse(line.slice(line.indexOf('TERRA_UI_AUDIT: ')+16));const state=logs.slice(start).find(l=>l.includes('TERRA_ANNUAL_STATE: '));return {...parsed,state};}await page.waitForTimeout(100);}throw Error('Missing UI audit');}
async function click(text){const a=await audit();const label=a.labels.find(l=>l.text===text);check(label,'Missing control '+text);await page.mouse.click(label.rect.x+label.rect.width/2,label.rect.y+label.rect.height/2);await page.waitForTimeout(300);}
async function capture(name){const a=await audit();console.log(name,a.state);await fs.writeFile(output+name+'.json',JSON.stringify(a,null,2));await page.screenshot({path:output+name+'.png'});return a;}
try{
 await page.goto(process.env.TERRA_TEST_URL||'http://127.0.0.1:49857/');
 await page.waitForFunction(()=>window.terraPlayer,null,{timeout:120000});await page.waitForTimeout(2000);
 await key('1');await key('Enter');
 for(let i=0;i<120&&!logs.some(l=>l.includes('MYTERRAIN_ARRIVAL'));i++)await page.waitForTimeout(500);
 await page.waitForTimeout(3000);
 let a=await audit();if(a.labels.some(l=>l.text==='Explore the landscape'))await click('Explore the landscape');
 await key('Escape');await key('1');
 for(let i=0;i<14;i++){await key('Tab');await key('Enter');a=await audit();if(a.state?.includes('queued=3'))break;}
 check(a.state.includes('queued=3'),'Failed to queue three projects');
 check(a.labels.some(l=>l.text==='End Year'),'Landscape button has count or wrong caption');
 const planning=a.state;
 await key('e');a=await capture('budget-1440');
 check(a.labels.some(l=>l.text.includes('Tax receipts to collect')),'Missing tax row');
 check(!a.labels.some(l=>/All \d+ proposals/.test(l.text)),'Nested proposal screen remains');
 check(a.state===planning,'Budget review changes state');
 await page.setViewportSize({width:1024,height:576});await page.waitForTimeout(400);await capture('budget-1024');
 await page.mouse.move(512,288);await page.mouse.wheel(0,900);await page.waitForTimeout(300);await capture('budget-1024-scrolled');
 await page.setViewportSize({width:1440,height:900});await page.waitForTimeout(400);
 await click('End Year');
 for(let i=0;i<20;i++){a=await audit();if(a.labels.some(l=>l.text==='Use offline chapter'))break;await page.waitForTimeout(500);}
 await click('Use offline chapter');await page.waitForTimeout(500);
 await click('Year story');await capture('chapter-intro-1440');await click('Read chapter →');await capture('chapter-budget-1440');await click('Next card →');await capture('chapter-people-1440');await click('Next card →');
 a=await capture('chapter-land-1440');const accepted=a.state;
 check(a.labels.some(l=>l.text==='Environmental  / 100'),'Missing environmental bars');
 check(!a.labels.some(l=>l.text.includes('Output multiplier')||l.text.includes('vs last year')),'Raw score text remains');
 for(const [width,height] of [[1280,720],[1024,576]]){
  await page.setViewportSize({width,height});await page.waitForTimeout(500);a=await capture('chapter-land-'+width);
  check(a.state===accepted,'Resize changes accepted year');
  const before=a.labels.find(l=>l.text.includes('The land carries')).rect.y;
  await page.mouse.move(width/2,height/2);await page.mouse.wheel(0,800);await page.waitForTimeout(300);
  a=await capture('chapter-land-'+width+'-scrolled');
  check(a.labels.find(l=>l.text.includes('The land carries')).rect.y<=before,'Wheel does not scroll reading surface');
 }
 await click('Plan year 2 →');a=await audit();check(a.state.includes('year=2')&&a.state.includes('phase=Planning'),'Continue failed');
 await click('Settings');await click('Text: standard');await key('Escape');await key('l');await click('Year story');await click('Read chapter →');await click('Next card →');a=await capture('chapter-people-1024-larger');
 check(a.largeText,'Larger text not enabled');
 check(a.labels.some(l=>l.text==='Economic  / 100')&&a.labels.some(l=>l.text==='Social  / 100'),'People bars missing');
 await click('Next card →');await capture('chapter-land-1024-larger');
 await page.mouse.wheel(0,900);await page.waitForTimeout(300);await capture('chapter-land-1024-larger-scrolled');
 await key('Escape');await key('Escape');await key('g');a=await capture('guide-after-chapter');
 check(!a.labels.some(l=>l.text==='Environmental  / 100'),'Bars leaked into guide');await key('Escape');
 const saved=(await audit()).state;
 await page.reload();await page.waitForFunction(()=>window.terraPlayer,null,{timeout:120000});await page.waitForTimeout(2000);await key('1');await key('Enter');await page.waitForTimeout(12000);
 a=await capture('reloaded');check(a.state===saved,'Reload changed accepted state');
 await fs.writeFile(output+'browser-checks.json',JSON.stringify({assertions,version:browser.version(),errors,accepted,saved},null,2));
}catch(e){await page.screenshot({path:output+'failure.png'});await fs.writeFile(output+'failure-logs.json',JSON.stringify(logs,null,2));throw e;}finally{await browser.close();}
