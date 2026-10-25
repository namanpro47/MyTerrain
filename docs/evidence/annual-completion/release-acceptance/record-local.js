// Local evidence wrapper only. No game messages, save access, or input remapping.
(() => {
  'use strict';
  const canvas = document.getElementById('unity-canvas');
  const panel = document.createElement('div');
  panel.style.cssText = 'position:fixed;right:12px;top:12px;z-index:10000;background:#14231ee8;color:#fff;padding:8px;border-radius:8px;font:12px system-ui;max-width:260px;display:grid';
  const status = document.createElement('span');
  status.textContent = 'Local recording · source __TERRA_SOURCE__ · silent canvas';
  const start = document.createElement('button'); start.textContent = 'Start recording';
  const stop = document.createElement('button'); stop.textContent = 'Stop and save'; stop.hidden = true;
  for (const button of [start, stop]) button.style.cssText = 'margin-top:6px;padding:5px 10px;font:12px system-ui';
  panel.append(status, start, stop); document.body.append(panel);
  let recorder, stream, timer, began;
  start.onclick = () => {
    if (recorder && recorder.state !== 'inactive') return;
    const mime = ['video/webm;codecs=vp9','video/webm;codecs=vp8','video/webm'].find(x => MediaRecorder.isTypeSupported(x));
    if (!mime) { status.textContent = 'Recording unavailable in this browser'; return; }
    const chunks = [];
    try {
      stream = canvas.captureStream(30);
      recorder = new MediaRecorder(stream,{mimeType:mime,videoBitsPerSecond:6000000});
      recorder.ondataavailable = event => { if(event.data.size) chunks.push(event.data); };
      recorder.onerror = event => { status.textContent = 'Recording failed: '+event.error.message; };
      recorder.onstop = () => {
        clearTimeout(timer); stream.getTracks().forEach(track => track.stop());
        const seconds = (performance.now()-began)/1000;
        const link = document.createElement('a');
        link.href = URL.createObjectURL(new Blob(chunks,{type:mime}));
        link.download = 'terra-recorded-2026-10-25-__TERRA_SOURCE__-local-canvas.webm';
        link.textContent = 'Download recorded gameplay'; link.style.cssText = 'display:block;color:#bcf4cb;margin-top:6px';
        panel.append(link); stop.hidden = true;
        status.textContent = 'Recorded '+seconds.toFixed(1)+'s · __TERRA_SOURCE__ · no audio';
        console.info('TERRA_LOCAL_RECORDING_COMPLETE',JSON.stringify({seconds,source:'__TERRA_SOURCE__',mime,chunks:chunks.length,bytes:chunks.reduce((n,b)=>n+b.size,0),audio:false}));
      };
      began = performance.now(); recorder.start(1000); start.hidden = true; stop.hidden = false;
      status.textContent = 'Recording actual canvas · __TERRA_SOURCE__ · no audio';
      timer = setTimeout(() => { if(recorder.state==='recording') recorder.stop(); },240000);
      canvas.focus(); console.info('TERRA_LOCAL_RECORDING_STARTED');
    } catch(error) { stream?.getTracks().forEach(track=>track.stop()); status.textContent = 'Recording failed: '+error.message; }
  };
  stop.onclick = () => { if(recorder?.state==='recording') recorder.stop(); canvas.focus(); };
})();
