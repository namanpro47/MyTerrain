// Unity is loaded once, on demand. Pack selection happens before its first Start().
let player=null, loadPromise=null, landscapeIsReady=false;
const canvas=document.getElementById('unity-canvas');
function pointer(kind,e){if(!player)return;window.terraInputPending={at:performance.now(),source:'pointer'};const b=canvas.getBoundingClientRect();player.SendMessage('TerraMain','PointerMessage',[kind,(e.clientX-b.left)/b.width,(e.clientY-b.top)/b.height,e.shiftKey||e.button===1?1:0].join(','));}
canvas.addEventListener('pointerdown',e=>{if(e.button!==0&&e.button!==1)return;e.preventDefault();canvas.focus();canvas.setPointerCapture(e.pointerId);pointer(0,e);});
canvas.addEventListener('pointermove',e=>pointer(1,e));canvas.addEventListener('pointerup',e=>pointer(2,e));
function cancel(){player?.SendMessage('TerraMain','LoseFocus','');}
canvas.addEventListener('pointercancel',cancel);canvas.addEventListener('lostpointercapture',cancel);window.addEventListener('blur',cancel);document.addEventListener('visibilitychange',()=>{if(document.hidden)cancel();});
canvas.addEventListener('wheel',e=>{e.preventDefault();player?.SendMessage('TerraMain','Scroll',e.deltaY*(e.deltaMode===1?16:e.deltaMode===2?canvas.clientHeight:1)/100);},{passive:false});
canvas.addEventListener('contextmenu',e=>{e.preventDefault();player?.SendMessage('TerraMain','Cancel');});
canvas.addEventListener('keydown',e=>{window.terraInputPending={at:performance.now(),source:'keyboard'};if(['Tab',' ','ArrowLeft','ArrowRight','ArrowUp','ArrowDown'].includes(e.key))e.preventDefault();});
export async function loadPlayer(place,onProgress){
  window.terraSelectedLocation=place.id;
  if(player){player.SendMessage('TerraMain','SetAtlasVisible',0);return player;}
  if(loadPromise)return loadPromise;
  loadPromise=new Promise((resolve,reject)=>{
    const config=window.TERRA_BUILD;
    let timeout;let instanceReady=false,landscapeReady=false,settled=false;
    const complete=()=>{if(instanceReady&&landscapeReady&&!settled){settled=true;clearTimeout(timeout);window.removeEventListener('terra-landscape-ready',onReady);window.removeEventListener('terra-landscape-error',onError);resolve(player);}};
    const fail=error=>{if(settled)return;settled=true;clearTimeout(timeout);window.removeEventListener('terra-landscape-ready',onReady);window.removeEventListener('terra-landscape-error',onError);reject(error instanceof Error?error:new Error(String(error)));};
    const onReady=()=>{landscapeReady=true;landscapeIsReady=true;if(window.terraAtlasVisible)pausePlayer();onProgress(1,'Your landscape is ready');complete();};
    const onError=e=>fail(new Error(e.detail||'The landscape could not load.'));
    window.addEventListener('terra-landscape-ready',onReady);window.addEventListener('terra-landscape-error',onError);
    timeout=setTimeout(()=>fail(new Error('Loading is taking longer than expected. Please try again.')),120000);
    const script=document.createElement('script');script.src=config.loader;
    script.onerror=()=>fail(new Error('The game could not download. Check your connection and try again.'));
    script.onload=()=>{
      createUnityInstance(canvas,{dataUrl:config.data,frameworkUrl:config.framework,codeUrl:config.code,streamingAssetsUrl:'StreamingAssets',companyName:'Terra',productName:'Terra',productVersion:'1.0',autoSyncPersistentDataPath:true,
        showBanner:(message,type)=>{console[type==='error'?'error':'warn'](message);if(type==='error')fail(new Error(message));}},
        p=>onProgress(p*.9,p<.85?'Preparing your landscape…':'Shaping the terrain…'))
      .then(instance=>{player=instance;window.terraPlayer=instance;instanceReady=true;if(window.terraAtlasVisible&&landscapeReady)pausePlayer();complete();}).catch(fail);
    };
    document.body.appendChild(script);
  });
  return loadPromise;
}
export function pausePlayer(){if(player&&landscapeIsReady)player.SendMessage('TerraMain','SetAtlasVisible',1);}
export function resumePlayer(){if(player&&landscapeIsReady)player.SendMessage('TerraMain','SetAtlasVisible',0);canvas.focus({preventScroll:true});}
export function hasPlayer(){return Boolean(loadPromise);}

// Opt-in evidence capture of actual canvas frames, never a staged sequence.
if(new URLSearchParams(location.search).has('capture'))canvas.addEventListener('keydown',e=>{
 if(e.key!=='F8'||window.terraRecording)return;e.preventDefault();
 const stream=canvas.captureStream(30),chunks=[];
 const recorder=new MediaRecorder(stream,{mimeType:'video/webm;codecs=vp9',videoBitsPerSecond:6000000});window.terraRecording=true;
 recorder.ondataavailable=e=>{if(e.data.size)chunks.push(e.data)};
 recorder.onstop=()=>{stream.getTracks().forEach(t=>t.stop());const link=document.createElement('a');link.href=URL.createObjectURL(new Blob(chunks,{type:'video/webm'}));link.download='terra-phase2a-interaction.webm';link.textContent='Download the 60-second interaction recording';link.style='position:fixed;top:0;left:0;padding:12px;background:#fff;color:#123;z-index:100';document.body.appendChild(link);window.terraRecording=false;console.info('TERRA_RECORDING_COMPLETE')};
 recorder.start(1000);console.info('TERRA_RECORDING_STARTED');setTimeout(()=>recorder.stop(),60000);
});
