// Install before the local game's code using Playwright context.addInitScript.
// Observe the actual Web Audio destination, without bypassing gesture policy or adding audible output.
(() => {
  const originalConnect = AudioNode.prototype.connect;
  const contexts = [];
  AudioNode.prototype.connect = function(destination, ...rest) {
    if (destination instanceof AudioDestinationNode) {
      let capture = contexts.find(c => c.context === destination.context);
      if (!capture) {
        const analyser = destination.context.createAnalyser();
        analyser.fftSize = 2048;
        originalConnect.call(analyser, destination);
        capture = { context: destination.context, analyser, samples: new Float32Array(2048) };
        contexts.push(capture);
      }
      return originalConnect.call(this, capture.analyser, ...rest);
    }
    return originalConnect.call(this, destination, ...rest);
  };
  window.audioProbe = {
    sample() {
      return contexts.map(({context, analyser, samples}) => {
        analyser.getFloatTimeDomainData(samples);
        let peak = 0, sum = 0;
        for (const x of samples) { peak = Math.max(peak, Math.abs(x)); sum += x*x; }
        return {state: context.state, rms: Math.sqrt(sum/samples.length), peak, time: context.currentTime};
      });
    },
    async record(ms = 15000) {
      if (!contexts.length) throw Error('No game audio graph');
      const {context, analyser} = contexts[0];
      const destination = context.createMediaStreamDestination();
      originalConnect.call(analyser, destination);
      const recorder = new MediaRecorder(destination.stream);
      const chunks=[];
      recorder.ondataavailable=e=>chunks.push(e.data);
      const done = new Promise(resolve=>recorder.onstop=resolve);
      recorder.start(); await new Promise(r=>setTimeout(r,ms)); recorder.stop(); await done;
      analyser.disconnect(destination);
      const blob=new Blob(chunks,{type:recorder.mimeType});
      const data=new Uint8Array(await blob.arrayBuffer());
      let binary=''; for (let i=0;i<data.length;i+=8192) binary+=String.fromCharCode(...data.subarray(i,i+8192));
      return {mime:recorder.mimeType,base64:btoa(binary)};
    }
  };
})();
