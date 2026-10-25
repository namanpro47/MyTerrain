import {locations,locationFromId} from './locations.js';
import {loadPlayer,pausePlayer,resumePlayer,hasPlayer} from './player.js';
const $=id=>document.getElementById(id);
const autoEnter=new URLSearchParams(location.search).get('enter')==='1';
let selected=locationFromId(new URLSearchParams(location.search).get('place')),globe=null,busy=false,journeyToken=0,loadedPlace=null;
let reduced=matchMedia('(prefers-reduced-motion: reduce)').matches;
try{const saved=localStorage.getItem('terra.atlas.motion');if(saved)reduced=saved==='reduced';}catch{}
function setMotion(){document.body.classList.toggle('reduced-motion',reduced);$('motion').textContent=`Motion: ${reduced?'reduced':'full'}`;$('motion').setAttribute('aria-pressed',String(reduced));window.terraReducedMotion=reduced;}
setMotion();$('motion').addEventListener('click',()=>{reduced=!reduced;setMotion();try{localStorage.setItem('terra.atlas.motion',reduced?'reduced':'full');}catch{}});
locations.forEach(place=>{
  const button=document.createElement('button');button.className='location-card';button.dataset.place=place.id;button.setAttribute('aria-label',`Select ${place.name}, ${place.region}`);button.setAttribute('aria-pressed','false');
  button.innerHTML=`<div class="landscape-image"><img src="atlas/images/${place.image}" alt="" width="720" height="360"><span class="card-number">${place.number}</span><span class="selected-badge">✓ SELECTED</span></div><div class="card-body"><span class="card-region">${place.region}</span><span class="card-name">${place.name}</span><span class="card-biome">${place.biome}</span><span class="card-arrow" aria-hidden="true">↗</span></div>`;
  button.addEventListener('click',()=>select(place.id));
  button.addEventListener('keydown',e=>{const index=locations.indexOf(place);if(e.key==='ArrowRight'||e.key==='ArrowLeft'){e.preventDefault();const next=locations[(index+(e.key==='ArrowRight'?1:2))%3];select(next.id);document.querySelector(`[data-place="${next.id}"]`).focus();}});
  $('location-cards').append(button);
});
function select(id){if(busy)return;selected=locationFromId(id);$('destination-region').textContent=selected.region;$('destination-name').textContent=selected.name;$('destination-summary').textContent=selected.summary;$('place-evidence').textContent=selected.evidence;
  $('enter').firstChild.textContent=loadedPlace===selected.id?'Return to your landscape ':'Explore this landscape ';
  document.querySelectorAll('.location-card').forEach(card=>card.setAttribute('aria-pressed',String(card.dataset.place===selected.id)));
  document.querySelector('.atlas-caption span:last-child').textContent=`${selected.number} / 03`;globe?.focus(selected);
  const url=new URL(location.href);url.searchParams.set('place',selected.id);url.searchParams.delete('enter');history.replaceState(null,'',url);
}
select(selected.id);
try{const {Globe}=await import('./globe.js');globe=new Globe($('globe'),locations,select,()=>reduced);
  // Opening composition looks across North America; later selections turn directly to their destination.
  if(selected!==locations[0])globe.focus(selected,true);
}catch(error){console.warn('Interactive globe unavailable',error);$('globe-fallback').hidden=false;$('globe').hidden=true;$('pins').hidden=true;}
$('globe').addEventListener('texture-error',()=>{$('globe-fallback').hidden=false;$('globe-fallback').innerHTML='<p>The Earth image could not load.</p><p>You can still select a landscape below.</p>';});
$('zoom-in').addEventListener('click',()=>globe?.zoom(-.3));$('zoom-out').addEventListener('click',()=>globe?.zoom(.3));$('reset-globe').addEventListener('click',()=>globe?.focus(selected));
$('about').addEventListener('click',()=>$('about-dialog').showModal());
function closeAbout(){$('about-dialog').close();$('about').focus({preventScroll:true});}
$('close-about').addEventListener('click',closeAbout);$('about-done').addEventListener('click',closeAbout);
$('about-dialog').addEventListener('click',e=>{if(e.target===$('about-dialog')){const r=e.target.getBoundingClientRect();if(e.clientX<r.left||e.clientX>r.right||e.clientY<r.top||e.clientY>r.bottom)closeAbout();}});
const delay=ms=>new Promise(resolve=>setTimeout(resolve,ms));
async function enter(){if(busy)return;
  // A fresh runtime per world avoids sharing scene objects, input capture or a ledger between packs.
  if(hasPlayer()&&loadedPlace!==selected.id){const url=new URL(location.href);url.searchParams.set('place',selected.id);url.searchParams.set('enter','1');location.assign(url);return;}
  window.terraAtlasVisible=false;busy=true;const token=++journeyToken;document.body.classList.add('departing');$('atlas').inert=true;$('journey').hidden=false;$('retry').hidden=true;
  $('journey-title').textContent=selected.name;$('journey-caption').textContent=selected.invitation;$('progress').textContent='Finding your place in the world…';$('progress-bar').style.width='3%';globe?.depart();document.body.style.overflow='hidden';
  $('game').hidden=false; // Keep the real canvas measurable while it initializes behind the atlas.
  $('game').style.visibility='hidden';window.terraSelectedLocation=selected.id;
  const load=loadPlayer(selected,(value,message)=>{if(token!==journeyToken)return;$('progress-bar').style.width=`${Math.round(value*100)}%`;$('progress').textContent=message;});
  // Handle failures immediately, even while the camera animation is still running.
  const result=load.then(value=>({value}),error=>({error}));
  await delay(reduced?0:2300);if(token!==journeyToken)return;$('journey').classList.add('veiled');$('cancel-journey').focus({preventScroll:true});
  const outcome=await result;if(token!==journeyToken){pausePlayer();return;}
  if(outcome.error){$('progress').textContent=outcome.error.message;$('retry').hidden=false;$('retry').focus({preventScroll:true});return;}
  loadedPlace=selected.id;await delay(reduced?0:500);if(token!==journeyToken)return;
  $('game').style.visibility='visible';$('atlas').hidden=true;$('journey').hidden=true;$('journey').classList.remove('veiled');document.body.classList.remove('departing');document.body.style.overflow='hidden';busy=false;if(globe)globe.active=false;resumePlayer();
}
function returnToAtlas(){window.terraAtlasVisible=true;journeyToken++;busy=false;pausePlayer();$('game').hidden=true;$('game').style.visibility='';$('journey').hidden=true;$('journey').classList.remove('veiled');$('atlas').hidden=false;$('atlas').inert=false;document.body.classList.remove('departing');document.body.style.overflow='';globe?.restore();select(selected.id);$('enter').focus({preventScroll:true});}
$('enter').addEventListener('click',enter);$('return-atlas').addEventListener('click',returnToAtlas);$('cancel-journey').addEventListener('click',returnToAtlas);$('retry').addEventListener('click',()=>{const url=new URL(location.href);url.searchParams.set('enter','1');location.assign(url);});
window.addEventListener('keydown',e=>{if(e.key==='Escape'&&(busy||(e.shiftKey&&$('atlas').hidden))){e.preventDefault();e.stopPropagation();returnToAtlas();}},true);
// Read before select() normalizes the URL: this flag is used only for switching an existing runtime.
if(autoEnter)enter();
window.terraAtlas={get selected(){return selected.id;},get state(){return busy?'entering':$('atlas').hidden?'playing':'selecting';}};
