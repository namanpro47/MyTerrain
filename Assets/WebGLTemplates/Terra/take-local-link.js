// Browser companion only: no run data or location is transmitted to the guide.
(() => {
 const link=document.createElement('a');
 link.href='take-local/';link.target='_blank';link.rel='noopener';link.textContent='Take it local ↗';
 link.title='Explore real-world action resources for Point Reyes (opens a new tab)';
 link.id='take-local-link';
 const style=document.createElement('style');
 style.textContent='#take-local-link{position:fixed;right:20px;bottom:80px;z-index:5;padding:10px 16px;border:1px solid #334c56;border-radius:10px;background:#16252ef5;color:#faf5db;font:600 14px system-ui;text-decoration:none;box-shadow:0 2px 12px #0003}#take-local-link:hover{background:#213643}#take-local-link:focus-visible{outline:3px solid #ffca61;outline-offset:3px}@media(max-width:680px){#take-local-link{right:10px;bottom:76px;font-size:12px;padding:8px 12px}}';
 document.head.append(style);document.body.append(link);
 const key='myterrain.take-local.v1';
 function refresh(){try{const saved=JSON.parse(localStorage.getItem(key)||'null');link.textContent=saved?.checkInAt&&Date.now()>=saved.checkInAt?'Local action check-in ↗':'Take it local ↗';}catch{}}
 refresh();window.addEventListener('focus',refresh);
})();
