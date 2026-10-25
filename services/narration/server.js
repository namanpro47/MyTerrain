// node --env-file=.env services/narration/server.js
const http=require('node:http');
const handler=require('./api/narration.js');
const port=Number(process.env.TERRA_VOICE_PORT||8788);
http.createServer((req,res)=>{
  if(req.url.split('?')[0]!=='/api/narration'){res.statusCode=404;return res.end();}
  handler(req,res).catch(()=>{if(!res.headersSent)res.statusCode=500;res.end();});
}).listen(port,'127.0.0.1',()=>console.log(`Terra voice service ready on http://127.0.0.1:${port} (key ${process.env.ELEVENLABS_API_KEY?'configured':'missing'})`));
