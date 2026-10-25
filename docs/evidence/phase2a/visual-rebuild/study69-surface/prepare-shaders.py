"""Prepare isolated comparison shaders outside Assets. Import only with Editor reservation."""
from pathlib import Path

output = Path('Builds/ArtResearch/Study69/Shaders')
output.mkdir(parents=True, exist_ok=True)
shared = '''// Same artist paint and world coordinates for ground and vegetation roots.
// Julio Sillet / JulioVII source and attribution remain in the existing asset register.
half3 StudyMeadowColor(float2 worldXZ)
{
    float2 uv=worldXZ/max(1,_MeadowScale);
    half3 paint=SAMPLE_TEXTURE2D(_MeadowPaint,sampler_MeadowPaint,uv).rgb;
    half3 broad=SAMPLE_TEXTURE2D_LOD(_MeadowPaint,sampler_MeadowPaint,uv,6).rgb;
    // Remove the source image's broad cloud-shaped exposure shifts while retaining
    // painted blade structure. Ground and foliage use the identical calculation.
    half3 strokes=clamp(paint/max(broad,.025),.55,1.55);
    return half3(.23,.38,.12)*lerp(half3(1,1,1),strokes,.65);
}
'''
(output/'MeadowSurface.hlsl').write_text(shared)
terrain = Path('Assets/Terra/Resources/TerraGround.shader').read_text()
terrain = terrain.replace('Shader "Terra/Illustrated Ground"', 'Shader "Study69/Shared Meadow Ground"', 1)
# Keep the far-distance texture shortcut, but use one initialized return value.
# The original early return produces a Metal uninitialized-function warning.
terrain = terrain.replace('                [branch] if(distant>.999)return average;',
    '                half4 filtered=average;\n                [branch] if(distant<=.999)\n                {', 1)
terrain = terrain.replace('                return lerp(paint,average,distant);',
    '                filtered=lerp(paint,average,distant);\n                }\n                return filtered;', 1)
start = terrain.index('                    half3 living=lerp(grassPaint,edgePaint,edgeWeight);')
end = terrain.index('                    living*=lerp(1,.70,cover.r);', start)
terrain = terrain[:start] + '''                    half3 living=StudyMeadowColor(p);
                    living=lerp(living,edgePaint*half3(.30,.48,.25),edgeWeight*.35);
''' + terrain[end:]
terrain = terrain.replace('            half3 CultivatedSoil(half3 paint)', '            #include "MeadowSurface.hlsl"\n            half3 CultivatedSoil(half3 paint)', 1)
(output/'Ground.shader').write_text(terrain)

foliage = Path('Assets/Terra/Resources/TerraGroundcover.shader').read_text()
foliage = foliage.replace('Shader "Terra/Soft Groundcover"', 'Shader "Study69/Shared Meadow Foliage"', 1)
foliage = foliage.replace('        _Cutoff("Cutout",Range(0,1))=.5', '''        _MeadowPaint("Shared artist meadow paint",2D)="white"{}
        _MeadowScale("Shared paint metres per tile",Float)=40
        _Cutoff("Cutout",Range(0,1))=.5''', 1)
foliage = foliage.replace('                float4 _PlantBounds;', '                float4 _PlantBounds;\n                float _MeadowScale;', 1)
foliage = foliage.replace('            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);', '''            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_MeadowPaint); SAMPLER(sampler_MeadowPaint);''', 1)
foliage = foliage.replace('            half4 Frag(V input):SV_Target', '            #include "MeadowSurface.hlsl"\n            half4 Frag(V input):SV_Target', 1)
foliage = foliage.replace('                float height=distance(root,TransformObjectToWorld(float3(0,1.3,0)));', '''                // Broad low tufts retain their footprint before individual blades resolve.
                float height=max(distance(root,TransformObjectToWorld(float3(0,_PlantBounds.y-_PlantBounds.x,0))),
                    distance(root,TransformObjectToWorld(float3(max(.01,_PlantBounds.z),0,0))));''', 1)
start = foliage.index('                half value=dot(paint.rgb,half3(.2126,.7152,.0722));')
end = foliage.index('                return half4(blade*illumination*_BaseColor.rgb,coverage);', start)
foliage = foliage[:start] + '''                half3 blade=StudyMeadowColor(input.positionWS.xz);
                // Roots share the terrain albedo; modest tip lift reveals overlapping leaves.
                blade*=lerp(1,1.10,smoothstep(.25,1,input.bladeHeight));
''' + foliage[end:]
(output/'Foliage.shader').write_text(foliage)
print('Prepared Ground.shader, Foliage.shader and shared MeadowSurface.hlsl outside Assets')
