// Browser transport only. The globe, selection UI and travel animation are Unity objects.
const canvas=document.getElementById('unity-canvas');let player;
const loading=document.getElementById('loading'),status=document.getElementById('status');
function fail(error){status.textContent='The game could not open. '+(error.message||error);document.getElementById('retry').hidden=false;loading.style.display='grid';}
window.addEventListener('myterrain-ready',()=>{loading.style.display='none';canvas.focus();});
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
script.onload=()=>createUnityInstance(canvas,{dataUrl:config.data,frameworkUrl:config.framework,codeUrl:config.code,streamingAssetsUrl:'StreamingAssets',companyName:'Terra',productName:'MyTerrain',productVersion:'1.0',autoSyncPersistentDataPath:true,showBanner:(message,type)=>{console[type==='error'?'error':'warn'](message);if(type==='error')fail(new Error(message));}},p=>document.getElementById('progress').value=p).then(instance=>{player=instance;window.terraPlayer=instance;canvas.focus();}).catch(fail);document.body.appendChild(script);
