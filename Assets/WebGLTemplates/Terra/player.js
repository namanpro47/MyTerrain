// The lightweight native-scene preview is visible while Unity starts.
// The interactive globe, selection UI and travel animation remain Unity objects.
const canvas=document.getElementById('unity-canvas');let player;
const loading=document.getElementById('loading'),status=document.getElementById('status');
const progress=document.getElementById('progress'),arrival=document.querySelector('.arrival');
let atlasReady=false,startFailed=false,revealing=false;
function fail(error){
 startFailed=true;loading.hidden=false;loading.classList.add('failed');loading.setAttribute('aria-busy','false');
 canvas.setAttribute('aria-hidden','true');canvas.tabIndex=-1;
 status.textContent='The world could not open. Please try again.';
 document.getElementById('retry').hidden=false;console.error(error);
}
function revealWorld(){
 if(!atlasReady||!player||startFailed||revealing)return;
 revealing=true;
 // Awake signals readiness before the first rendered frame. Wait for a paint,
 // not a cosmetic minimum duration, and keep the preview until input is bound.
 requestAnimationFrame(()=>requestAnimationFrame(()=>{
  if(startFailed)return;
  loading.hidden=true;loading.setAttribute('aria-busy','false');
  canvas.removeAttribute('aria-hidden');canvas.tabIndex=0;canvas.focus();
  console.info('MYTERRAIN_WORLD_VISIBLE',Math.round(performance.now()));
 }));
}
window.addEventListener('myterrain-ready',()=>{atlasReady=true;revealWorld();},{once:true});
function downloadProgress(value){
 if(startFailed)return;
 if(value>=1){arrival.dataset.stage='starting';progress.removeAttribute('value');status.textContent='Opening your world…';}
 else{arrival.dataset.stage='downloading';progress.value=value;status.textContent='Loading your world · '+Math.round(value*100)+'%';}
}
function pointer(kind,e){if(!player)return;window.terraInputPending={at:performance.now(),source:'pointer'};const b=canvas.getBoundingClientRect();player.SendMessage('TerraMain','PointerMessage',[kind,(e.clientX-b.left)/b.width,(e.clientY-b.top)/b.height,e.shiftKey||e.button===1?1:0].join(','));}
canvas.addEventListener('pointerdown',e=>{if(e.button!==0&&e.button!==1)return;e.preventDefault();canvas.focus();canvas.setPointerCapture(e.pointerId);pointer(0,e);});
canvas.addEventListener('pointermove',e=>pointer(1,e));canvas.addEventListener('pointerup',e=>pointer(2,e));
function cancel(){player?.SendMessage('TerraMain','LoseFocus','');}
function audioFocus(){player?.SendMessage('TerraMain','AudioVisibility',document.hidden||!document.hasFocus()?'0':'1');}
window.addEventListener('blur',audioFocus);window.addEventListener('focus',audioFocus);document.addEventListener('visibilitychange',audioFocus);
canvas.addEventListener('pointercancel',cancel);canvas.addEventListener('lostpointercapture',cancel);window.addEventListener('blur',cancel);document.addEventListener('visibilitychange',()=>{if(document.hidden)cancel();});
canvas.addEventListener('wheel',e=>{e.preventDefault();player?.SendMessage('TerraMain','Scroll',e.deltaY*(e.deltaMode===1?16:e.deltaMode===2?canvas.clientHeight:1)/100);},{passive:false});
canvas.addEventListener('contextmenu',e=>e.preventDefault());canvas.addEventListener('keydown',e=>{window.terraInputPending={at:performance.now(),source:'keyboard'};if(['Tab',' ','ArrowLeft','ArrowRight','ArrowUp','ArrowDown'].includes(e.key))e.preventDefault();});
const config=window.TERRA_BUILD,script=document.createElement('script');script.src=config.loader;script.onerror=()=>fail(new Error('Check your connection and try again.'));
script.onload=()=>createUnityInstance(canvas,{dataUrl:config.data,frameworkUrl:config.framework,codeUrl:config.code,streamingAssetsUrl:'StreamingAssets',companyName:'Terra',productName:'MyTerrain',productVersion:'1.0',autoSyncPersistentDataPath:true,showBanner:(message,type)=>{console[type==='error'?'error':'warn'](message);if(type==='error')fail(new Error(message));}},downloadProgress).then(instance=>{player=instance;window.terraPlayer=instance;revealWorld();}).catch(fail);document.body.appendChild(script);
