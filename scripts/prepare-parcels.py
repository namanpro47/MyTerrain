"""Prepare versioned open-land parcels; source geography is never rewritten.

All intersection tests use polygon area, not just a parcel centre sample.
Python standard library + Pillow (diagnostic plan views only). No GIS service.
"""
import argparse
import hashlib
import json
import math
from collections import Counter
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
PACKS = ('meadowbrook-v1', 'point-reyes-v1', 'suusamyr-v1')
STORIES = {
    'meadowbrook-v1': ('Farms beside the creek', 'In this fictional Meadowbrook community, four households share a farm store, a small village square and a recreation ground. Productive fields sit between woodland and creek corridors. Add homes and livelihoods while keeping those corridors intact.'),
    'point-reyes-v1': ('Room for a rural community', 'In this fictional Point Reyes scenario, four households share a farm store, a small village square and a recreation ground. Plan the open agricultural land around the existing community; woodland stays outside the building area.'),
    'suusamyr-v1': ('A foothold in the valley', 'In this fictional Suusamyr scenario, four households share a farm store, a small village square and a recreation ground. Open valley land offers room to grow, while terrain limits which projects fit. Choose how farms, homes and shared services develop.')
}


def area(poly):
    return abs(sum(a[0]*b[1]-b[0]*a[1] for a, b in zip(poly, poly[1:]+poly[:1])))/2


def points(poly):
    return [(p['x'], p['z']) for p in poly]


def clip(poly, box):
    for axis, edge, sign in ((0, box[0], 1), (0, box[2], -1), (1, box[1], 1), (1, box[3], -1)):
        result = []
        for a, b in zip(poly, poly[1:]+poly[:1]):
            ina, inb = sign*(a[axis]-edge) >= 0, sign*(b[axis]-edge) >= 0
            if ina:
                result.append(a)
            if ina != inb:
                t = (edge-a[axis])/(b[axis]-a[axis])
                result.append((a[0]+t*(b[0]-a[0]), a[1]+t*(b[1]-a[1])))
        poly = result
        if not poly:
            break
    return poly


def bounds(poly):
    return min(p[0] for p in poly), min(p[1] for p in poly), max(p[0] for p in poly), max(p[1] for p in poly)


def intersects(a, b):
    return a[0] < b[2] and a[2] > b[0] and a[1] < b[3] and a[3] > b[1]


def line_crosses(a, b, box):
    lo, hi = 0., 1.
    for axis in (0, 1):
        d = b[axis]-a[axis]
        if abs(d) < 1e-9:
            if not box[axis] <= a[axis] <= box[axis+2]:
                return False
        else:
            t, u = (box[axis]-a[axis])/d, (box[axis+2]-a[axis])/d
            lo, hi = max(lo, min(t, u)), min(hi, max(t, u))
            if lo > hi:
                return False
    return True


def height(t, x, z):
    fx, fz = (x-t['xMin'])/t['cellSizeM'], (z-t['zMin'])/t['cellSizeM']
    ix, iz = min(int(fx), t['columns']-2), min(int(fz), t['rows']-2)
    u, v, n = fx-ix, fz-iz, iz*t['columns']+ix
    a, b = t['heights'][n:n+2]
    c, d = t['heights'][n+t['columns']:n+t['columns']+2]
    return a+(b-a)*u+(c-a)*v if u+v <= 1 else d+(c-d)*(1-u)+(b-d)*(1-v)


def flatness(t, box, w=26, d=32):
    best = float('inf')
    for w, d in ((w, d), (d, w)):
        for u in (.5, .25, .75):
            for v in (.5, .25, .75):
                x, z = box[0]+w/2+(box[2]-box[0]-w)*u, box[1]+d/2+(box[3]-box[1]-d)*v
                # Grid plus footprint edges on the piecewise-linear source terrain.
                xs = [x-w/2, x+w/2]+list(frange(t['xMin'], t['cellSizeM'], x-w/2, x+w/2))
                zs = [z-d/2, z+d/2]+list(frange(t['zMin'], t['cellSizeM'], z-d/2, z+d/2))
                hs = [height(t, px, pz) for px in xs for pz in zs]
                for diagonal in frange(t['xMin']+t['zMin'],t['cellSizeM'],x-w/2+z-d/2-.00001,x+w/2+z+d/2):
                    for px in (x-w/2,x+w/2):
                        pz=diagonal-px
                        if z-d/2 <= pz <= z+d/2:
                            hs.append(height(t,px,pz))
                    for pz in (z-d/2,z+d/2):
                        px=diagonal-pz
                        if x-w/2 <= px <= x+w/2:
                            hs.append(height(t,px,pz))
                best = min(best, max(hs)-min(hs))
    return best


def frange(origin, step, low, high):
    n = origin+math.ceil((low-origin)/step)*step
    while n < high:
        if n > low:
            yield n
        n += step


def candidates(world, size):
    t = world['terrain']
    xmin, zmin = t['xMin'], t['zMin']
    xmax, zmax = xmin+(t['columns']-1)*t['cellSizeM'], zmin+(t['rows']-1)*t['cellSizeM']
    covers = [(c['coverId'], points(c['polygon']), bounds(points(c['polygon']))) for c in world['covers']]
    protected = [points(s['polygon']) for s in world['sites'] if s['protectedSite']]
    accepted, reasons = [], Counter()
    for row, z in enumerate(range(math.ceil(zmin/size)*size, int(zmax-size)+1, size)):
        for col, x in enumerate(range(math.ceil(xmin/size)*size, int(xmax-size)+1, size)):
            box = (x, z, x+size, z+size)
            fractions = Counter()
            for kind, poly, bb in covers:
                if intersects(box, bb):
                    fractions[kind] += area(clip(poly, box))
            if fractions['woodland'] > .001 or fractions['water'] > .001 or fractions['sealed'] > .001:
                reasons['woodland/water/sealed'] += 1
                continue
            if abs(sum(fractions.values())-size*size) > .1:
                reasons['unmapped'] += 1
                continue
            if any(area(clip(poly, box)) > .001 for poly in protected):
                reasons['protected'] += 1
                continue
            blocked = False
            for f in world['features']:
                # Keep full cells off mapped water, roads and paths, including a safety margin.
                margin = f.get('widthM', 0)/2+8
                expanded = (x-margin, z-margin, x+size+margin, z+size+margin)
                route = points(f['points'])
                if any(line_crosses(a, b, expanded) for a, b in zip(route, route[1:])):
                    blocked = True
                    break
            if blocked:
                reasons['mapped corridor'] += 1
                continue
            if flatness(t, box) > 3:
                reasons['steep'] += 1
                continue
            accepted.append(dict(box=box, row=row, col=col, cover=fractions))
    return accepted, reasons


def draw(pack, world, cells, output, size):
    from PIL import Image, ImageDraw
    t = world['terrain']
    image = Image.new('RGB', (920, 970), '#ececdd')
    pen = ImageDraw.Draw(image)
    def xy(p):
        return (35+(p[0]-t['xMin'])*.7, 900-(p[1]-t['zMin'])*.7)
    colors = {'woodland':'#3e684a', 'grass':'#a5b87b', 'cultivated':'#d3bb77', 'water':'#68a5b2', 'sealed':'#8b8683'}
    for c in world['covers']:
        pen.polygon([xy(p) for p in points(c['polygon'])], fill=colors.get(c['coverId'], '#a0a0a0'))
    for f in world['features']:
        pen.line([xy(p) for p in points(f['points'])], fill='#31718b' if f['kind']=='water' else '#6b6354', width=4)
    for s in world['sites']:
        if s['protectedSite']:
            p = [xy(p) for p in points(s['polygon'])]
            pen.line(p+p[:1], fill='#9f4055', width=3)
    for c in cells:
        l,b,r,u = c['box']
        box = [xy((l,u)),xy((r,b))]
        pen.rectangle(box, outline='#fff9d5', width=2)
        label=c.get('label','')
        text = ('H'+label[-1] if label.startswith('Existing home') else
                {'Existing village square':'V','Existing farm store':'F','Existing recreation ground':'P',
                 'large-wind':'W','large-works-1':'L1','large-works-2':'L2'}.get(label, f"{c['col']+1},{c['row']+1}"))
        pen.text(xy((l+3,u-4)), text, fill='#182e2b')
    pen.text((35, 15), f'{pack} | {size} m grid | {len(cells)} suitable cells', fill='#182e2b')
    pen.text((35, 925), 'Diagnostic plan: open-land slots only. Woodland, water, mapped corridors and protected areas excluded.', fill='#182e2b')
    pen.text((35, 945), 'H1-H4: inherited homes | F: farm store | V: village square | P: recreation | W/L: joined large-project cells', fill='#182e2b')
    output.parent.mkdir(parents=True, exist_ok=True)
    image.save(output)


def prepare(pack, world, cells, size):
    available = {(c['col'], c['row']): c for c in cells}
    plots = []
    def joined(n, label, w, d):
        choices = []
        for (col, row), c in available.items():
            keys = [(col+i, row+j) for i in range(n) for j in range(n)]
            if not all(k in available for k in keys):
                continue
            x, z = c['box'][:2]
            box = (x,z,x+n*size,z+n*size)
            relief = flatness(world['terrain'], box, w, d)
            if relief <= 2.95:
                choices.append((relief, col, row, keys, box))
        if not choices:
            raise ValueError(f'{pack}: no suitable joined plot for {label}')
        _, col, row, keys, box = min(choices)
        cover = Counter()
        for key in keys:
            cover.update(available.pop(key)['cover'])
        result = dict(box=box, col=col, row=row, cover=cover, label=label)
        plots.append(result)
        return result
    # Clearance envelopes remain full size. Larger projects use joined grid cells.
    joined(3, 'large-wind', 16, 16)
    joined(2, 'large-works-1', 80, 64)
    joined(2, 'large-works-2', 72, 60)
    village = joined(2, 'existing-square', 64, 52)
    plots.extend(available.values())
    for c in plots:
        c['id'] = f"grid-{c['col']:02}-{c['row']:02}"
    initial = [(village, 'village-center', 'Existing village square')]
    occupied = {village['id']}
    vx, vz = village['box'][0], village['box'][1]
    for action, name, w, d in ([('low-density-housing', f'Existing home {i+1}', 26, 32) for i in range(4)] +
                               [('agricultural-storage', 'Existing farm store', 48, 42), ('public-park', 'Existing recreation ground',48,40)]):
        choices = [c for c in plots if c['id'] not in occupied and not c.get('label')
                   and flatness(world['terrain'], c['box'], w, d) <= 2.95]
        if not choices:
            raise ValueError(f'{pack}: no initial slot for {name}')
        c = min(choices, key=lambda c: (c['box'][0]-vx)**2+(c['box'][1]-vz)**2)
        c['label'] = name
        occupied.add(c['id'])
        initial.append((c, action, name))
    village['label'] = 'Existing village square'
    # Repartition the exact previous analysis domain; retain its cover totals in context.
    hydro = json.loads((ROOT/'worlds'/pack/'hydrology.json').read_text())
    totals = Counter()
    for c in hydro['context']:
        totals[c['coverId']] += c['areaM2']
    for s in world['sites']:
        for c in s['baselineCover']:
            totals[c['coverId']] += s['analysisAreaM2']*c['fraction']
    sites, connections = [], []
    def add(s):
        sites.append(s)
        connections.append(dict(siteId=s['id'],groupId=s['contributingAreaId']))
        for cover in s['baselineCover']:
            totals[cover['coverId']] -= s['analysisAreaM2']*cover['fraction']
    for s in world['sites']:
        if s['protectedSite']:
            add(s)
    starters = {c['id']:(action,name) for c,action,name in initial}
    for c in plots:
        l,b,r,t = c['box']
        a = (r-l)*(t-b)
        cultivated = c['cover'].get('cultivated',0)/a
        offers = [dict(treatmentId='cultivate-standard',cost=26,production=3*a/10000,retentionCapacityM3=0),
                  dict(treatmentId='cultivate-margin',cost=26,production=2.25*a/10000,retentionCapacityM3=0),
                  dict(treatmentId='restore-grass',cost=14,production=0,retentionCapacityM3=0),
                  dict(treatmentId='water-swale',cost=30,production=0,retentionCapacityM3=100*a/10000)]
        s = dict(id=c['id'],name=starters[c['id']][1] if c['id'] in starters else f"Open plot {c['col']+1} · {c['row']+1}",
                 sourceId='scenario-open-grid-v1',contributingAreaId='grid-isolated-'+c['id'],modelParameterId='cn-b-cover-assumed-v1',
                 polygon=[dict(x=l,z=b),dict(x=r,z=b),dict(x=r,z=t),dict(x=l,z=t)],analysisAreaM2=a,
                 baselineProduction=cultivated*a/10000,protectedSite=c['id'] in starters,
                 protectionReason='Existing community asset. Redevelopment is outside this game.' if c['id'] in starters else '',
                 baselineCover=[dict(coverId=k,fraction=v/a) for k,v in sorted(c['cover'].items()) if v>.0001],
                 allowedTreatments=[] if c['id'] in starters else [o['treatmentId'] for o in offers],
                 offers=[] if c['id'] in starters else offers)
        add(s)
    if any(v < -.1 for v in totals.values()):
        raise ValueError(f'{pack}: negative context {totals}')
    context = [dict(id='grid-context-'+k,coverId=k,areaM2=max(0,v)) for k,v in sorted(totals.items()) if v>.001]
    # Initial structures are part of the baseline, not free player-built revenue/jobs.
    dimensions = {'low-density-housing':(26,32,.35),'agricultural-storage':(48,42,.1),'village-center':(64,52,.25),'public-park':(48,40,.85)}
    for s in sites:
        if s['id'] not in starters:
            continue
        w,d,green = dimensions[starters[s['id']][0]]
        share = w*d/s['analysisAreaM2']
        mix = Counter({c['coverId']:c['fraction']*(1-share) for c in s['baselineCover']})
        mix['grass'] += share*green
        mix['sealed'] += share*(1-green)
        s['baselineCover'] = [dict(coverId=k,fraction=v) for k,v in sorted(mix.items()) if v>0]
        s['baselineProduction'] *= 1-share
    result = dict(schemaVersion=1, packId=pack, version='4.0.0', gridSizeM=size,
                  worldSha256=hashlib.sha256((ROOT/'worlds'/pack/'world.json').read_bytes()).hexdigest(),
                  title=STORIES[pack][0], story=STORIES[pack][1], sites=sites, context=context, connections=connections,
                  initialBuildings=[dict(siteId=c['id'],actionId=a,title=n) for c,a,n in initial])
    target = ROOT/'Assets/Terra/Resources/Parcels'/f'{pack}.json'
    target.parent.mkdir(parents=True,exist_ok=True)
    target.write_text(json.dumps(result,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    draw(pack,world,plots,ROOT/'docs/evidence/map-parcels'/f'{pack}-layout.png',size)
    print('  plots:',len(plots),'existing:',len(initial),'available:',len(plots)-len(initial))


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--size', type=int, default=60)
    parser.add_argument('--inspect', action='store_true')
    args = parser.parse_args()
    if not args.inspect and args.size != 60:
        parser.error('Version 4 uses a 60 m base grid. Other sizes are diagnostic --inspect runs only.')
    for pack in PACKS:
        world = json.loads((ROOT/'worlds'/pack/'world.json').read_text())
        cells, reasons = candidates(world, args.size)
        print(pack, len(cells), dict(reasons))
        if args.inspect:
            draw(pack, world, cells, ROOT/'docs/evidence/map-parcels'/f'{pack}-{args.size}.png', args.size)
        else:
            prepare(pack,world,cells,args.size)


if __name__ == '__main__':
    main()
