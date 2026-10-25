import * as THREE from './vendor/three.module.min.js';
const rad = Math.PI / 180;
const clamp = THREE.MathUtils.clamp;
export function coordinates(lat, lon) {
  return new THREE.Vector3(Math.cos(lat * rad) * Math.cos(lon * rad), Math.sin(lat * rad), -Math.cos(lat * rad) * Math.sin(lon * rad));
}
export class Globe {
  constructor(canvas, places, select, reducedMotion) {
    this.canvas = canvas; this.places = places; this.reducedMotion = reducedMotion;
    this.renderer = new THREE.WebGLRenderer({canvas, antialias:true, alpha:true, powerPreference:'low-power'});
    this.renderer.setPixelRatio(Math.min(devicePixelRatio, 1.75));
    this.renderer.setClearColor(0, 0);
    this.scene = new THREE.Scene(); this.camera = new THREE.PerspectiveCamera(37, 1, .01, 100);
    this.camera.position.z = 3.25;
    this.root = new THREE.Group(); this.scene.add(this.root);
    this.pitch = 25 * rad; this.yaw = 5 * rad; this.targetPitch = this.pitch; this.targetYaw = this.yaw;
    this.distance = this.targetDistance = 3.25; this.velocity = {x:0,y:0};
    this.selected = places[0]; this.drag = null; this.lastFrame = performance.now(); this.active = true;
    this.material = new THREE.ShaderMaterial({
      uniforms: {map:{value:null},ready:{value:0}},
      vertexShader:`varying vec2 vUv; varying vec3 vNormal; varying vec3 vPosition;
        void main(){vUv=uv; vec4 p=modelViewMatrix*vec4(position,1.);vNormal=normalize(normalMatrix*normal);vPosition=p.xyz;gl_Position=projectionMatrix*p;}`,
      fragmentShader:`uniform sampler2D map;uniform float ready;varying vec2 vUv;varying vec3 vNormal;varying vec3 vPosition;
        void main(){vec3 t=texture2D(map,vUv).rgb;float lum=dot(t,vec3(.299,.587,.114));
        float ocean=smoothstep(.015,.085,t.b-t.r)* (1.-smoothstep(.15,.38,t.r));
        vec3 land=mix(vec3(lum),t,.57)*vec3(1.08,1.14,.89);
        land=mix(land,vec3(.48,.59,.34),.14);land=mix(land,vec3(.87,.91,.82),smoothstep(.68,.95,lum));
        vec3 sea=mix(vec3(.115,.29,.29),vec3(.33,.53,.49),clamp(lum*3.2,0.,1.));
        vec3 color=mix(land,sea,ocean);color=mix(vec3(.35,.48,.38),color,ready);
        vec3 n=normalize(vNormal);float diffuse=max(dot(n,normalize(vec3(-.65,.85,1.4))),0.);
        float edge=pow(1.-max(dot(n,normalize(-vPosition)),0.),3.);
        color*=.60+.54*diffuse;color=mix(color,vec3(.69,.82,.72),edge*.55);
        gl_FragColor=vec4(color,1.);}`
    });
    this.earth = new THREE.Mesh(new THREE.SphereGeometry(1, 128, 80), this.material); this.root.add(this.earth);
    const texture = new THREE.TextureLoader().load(new URL('./images/earth.jpg',import.meta.url).href, t => {
      t.anisotropy = Math.min(8,this.renderer.capabilities.getMaxAnisotropy());
      this.material.uniforms.map.value=t; this.material.uniforms.ready.value=1;
      this.canvas.dispatchEvent(new CustomEvent('texture-ready'));
    },undefined,()=>this.canvas.dispatchEvent(new CustomEvent('texture-error')));
    this.texture=texture;
    const halo=new THREE.Mesh(new THREE.SphereGeometry(1.025,96,64),new THREE.ShaderMaterial({transparent:true,depthWrite:false,side:THREE.BackSide,
      vertexShader:`varying vec3 n;varying vec3 p;void main(){vec4 v=modelViewMatrix*vec4(position,1.);n=normalize(normalMatrix*normal);p=v.xyz;gl_Position=projectionMatrix*v;}`,
      fragmentShader:`varying vec3 n;varying vec3 p;void main(){float a=pow(clamp(1.-abs(dot(normalize(n),normalize(-p))),0.,1.),5.);gl_FragColor=vec4(.72,.85,.76,a*.25);}`}));this.root.add(halo);
    // Fine cartographic graticule: restrained and geographically registered.
    const points=[];
    for(let lat=-60;lat<=60;lat+=30)for(let lon=-180;lon<180;lon+=2){points.push(coordinates(lat,lon).multiplyScalar(1.0015),coordinates(lat,lon+2).multiplyScalar(1.0015));}
    for(let lon=-180;lon<180;lon+=30)for(let lat=-88;lat<88;lat+=2){points.push(coordinates(lat,lon).multiplyScalar(1.0015),coordinates(lat+2,lon).multiplyScalar(1.0015));}
    this.root.add(new THREE.LineSegments(new THREE.BufferGeometry().setFromPoints(points),new THREE.LineBasicMaterial({color:0xe5edc6,transparent:true,opacity:.095,depthWrite:false})));
    this.pins=places.map(place=>{
      const button=document.createElement('button');button.className='map-pin';button.setAttribute('aria-label',`Select ${place.name}, ${place.region}`);button.setAttribute('aria-pressed',String(place===this.selected));
      const label=document.createElement('span');label.textContent=place.short;button.append(label);button.addEventListener('click',()=>select(place.id));
      document.getElementById('pins').append(button);
      return {place,button,point:coordinates(...place.coords).multiplyScalar(1.012)};
    });
    // Resize in the animation frame, outside ResizeObserver delivery.
    // Hiding/restoring the atlas must not recursively resize either WebGL canvas.
    this.resizeObserver=new ResizeObserver(()=>{this.needsResize=true;});this.resizeObserver.observe(canvas);this.resize();this.bind();this.frame=this.frame.bind(this);this.raf=requestAnimationFrame(this.frame);
  }
  resize(){const rect=this.canvas.getBoundingClientRect();if(!rect.width||!rect.height)return;if(this.width===rect.width&&this.height===rect.height)return;this.width=rect.width;this.height=rect.height;this.renderer.setSize(rect.width,rect.height,false);this.camera.aspect=rect.width/rect.height;this.camera.updateProjectionMatrix();}
  focus(place,instant=false){this.selected=place;this.targetPitch=(place.coords[0]-7)*rad;
    const yaw=(-place.coords[1]-90)*rad;this.targetYaw=this.yaw+Math.atan2(Math.sin(yaw-this.yaw),Math.cos(yaw-this.yaw));
    this.targetDistance=3.25;this.velocity.x=this.velocity.y=0;
    if(instant||this.reducedMotion()){this.pitch=this.targetPitch;this.yaw=this.targetYaw;this.distance=this.targetDistance;}
    this.pins.forEach(pin=>pin.button.setAttribute('aria-pressed',String(pin.place===place)));
  }
  zoom(delta){if(this.departing)return;this.targetDistance=clamp(this.targetDistance+delta,2.45,4.5);}
  bind(){const c=this.canvas;
    c.addEventListener('pointerdown',e=>{if(e.button!==0||this.departing)return;c.focus({preventScroll:true});c.setPointerCapture(e.pointerId);this.drag={id:e.pointerId,x:e.clientX,y:e.clientY};this.velocity.x=this.velocity.y=0;this.targetYaw=this.yaw;this.targetPitch=this.pitch;c.classList.add('dragging');});
    c.addEventListener('pointermove',e=>{if(!this.drag||this.drag.id!==e.pointerId)return;const dx=e.clientX-this.drag.x,dy=e.clientY-this.drag.y;
      this.velocity.x=dx*.004;this.velocity.y=dy*.004;this.targetYaw+=this.velocity.x;this.targetPitch=clamp(this.targetPitch+this.velocity.y,-1.4,1.4);this.yaw=this.targetYaw;this.pitch=this.targetPitch;this.drag.x=e.clientX;this.drag.y=e.clientY;});
    const release=()=>{this.drag=null;c.classList.remove('dragging');if(this.reducedMotion())this.velocity.x=this.velocity.y=0;};
    c.addEventListener('pointerup',release);c.addEventListener('pointercancel',release);c.addEventListener('lostpointercapture',release);window.addEventListener('blur',release);
    c.addEventListener('wheel',e=>{e.preventDefault();this.zoom(e.deltaY*(e.deltaMode===1?.015:e.deltaMode===2?.6:.0015));},{passive:false});
    c.addEventListener('keydown',e=>{if(this.departing)return;const step=.15;
      if(['ArrowLeft','ArrowRight','ArrowUp','ArrowDown','+','=','-','Home'].includes(e.key))e.preventDefault();
      if(e.key==='ArrowLeft')this.targetYaw-=step;if(e.key==='ArrowRight')this.targetYaw+=step;if(e.key==='ArrowUp')this.targetPitch=clamp(this.targetPitch-step,-1.4,1.4);if(e.key==='ArrowDown')this.targetPitch=clamp(this.targetPitch+step,-1.4,1.4);
      if(e.key==='+'||e.key==='=')this.zoom(-.3);if(e.key==='-')this.zoom(.3);if(e.key==='Home')this.focus(this.selected);
    });
    c.addEventListener('webglcontextlost',e=>{e.preventDefault();this.active=false;document.getElementById('globe-fallback').hidden=false;document.getElementById('pins').hidden=true;});
    c.addEventListener('webglcontextrestored',()=>{this.active=true;document.getElementById('globe-fallback').hidden=true;document.getElementById('pins').hidden=false;});
  }
  depart(){this.focus(this.selected);
    const stage=this.canvas.parentElement,r=stage.getBoundingClientRect();this.stage=stage;
    stage.style.cssText=`position:fixed;left:${r.left}px;top:${r.top}px;width:${r.width}px;height:${r.height}px;margin:0;z-index:8;`;
    this.flight=stage.animate([{left:r.left+'px',top:r.top+'px',width:r.width+'px',height:r.height+'px'},{left:'0px',top:'0px',width:innerWidth+'px',height:innerHeight+'px'}],{duration:this.reducedMotion()?0:2400,easing:'cubic-bezier(.4,0,.2,1)',fill:'forwards'});
    this.targetPitch=this.selected.coords[0]*rad;this.departing={start:performance.now(),from:this.distance};this.velocity.x=this.velocity.y=0;}
  restore(){this.flight?.cancel();if(this.stage)this.stage.style.cssText='';this.departing=null;this.active=true;this.focus(this.selected,true);this.canvas.style.transform='';}
  frame(now){this.raf=requestAnimationFrame(this.frame);const dt=Math.min((now-this.lastFrame)/1000,.05);this.lastFrame=now;if(!this.active||document.hidden)return;
    if(this.needsResize){this.needsResize=false;this.resize();}
    if(!this.drag&&!this.departing){this.targetYaw+=this.velocity.x;this.targetPitch=clamp(this.targetPitch+this.velocity.y,-1.4,1.4);this.velocity.x*=Math.exp(-dt*8);this.velocity.y*=Math.exp(-dt*8);}
    const smooth=this.reducedMotion()?1:1-Math.exp(-dt*5.5);this.yaw+=(this.targetYaw-this.yaw)*smooth;this.pitch+=(this.targetPitch-this.pitch)*smooth;
    if(this.departing){const t=clamp((now-this.departing.start-650)/2050,0,1);const eased=t*t*t;this.targetDistance=THREE.MathUtils.lerp(this.departing.from,1.08,eased);}
    this.distance+=(this.targetDistance-this.distance)*smooth;this.camera.position.z=this.distance;
    this.root.rotation.set(this.pitch,this.yaw,0,'XYZ');this.root.updateMatrixWorld(true);this.camera.updateMatrixWorld(true);
    this.pins.forEach(({point,button})=>{const p=point.clone().applyMatrix4(this.root.matrixWorld);const visible=p.dot(this.camera.position.clone().sub(p))>0;
      const screen=p.clone().project(this.camera);const x=(screen.x+1)*this.width/2,y=(-screen.y+1)*this.height/2;
      const inside=x>10&&x<this.width-10&&y>20&&y<this.height-40;button.classList.toggle('align-left',x>this.width-125);
      button.hidden=!visible||!inside;button.style.transform=`translate(${x-15}px,${y-15}px)`;
    });
    this.renderer.render(this.scene,this.camera);
  }
}
