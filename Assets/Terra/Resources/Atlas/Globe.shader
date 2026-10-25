Shader "MyTerrain/Atlas/Globe" {
Properties { _MainTex("Earth",2D)="white"{} }
SubShader { Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"} Pass { Cull Back
HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
struct A {float4 positionOS:POSITION;float3 normalOS:NORMAL;float2 uv:TEXCOORD0;};
struct V {float4 positionCS:SV_POSITION;float3 normal:TEXCOORD0;float2 uv:TEXCOORD1;float3 world:TEXCOORD2;};
TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
V vert(A a){V o;o.positionCS=TransformObjectToHClip(a.positionOS.xyz);o.world=TransformObjectToWorld(a.positionOS.xyz);o.normal=TransformObjectToWorldNormal(a.normalOS);o.uv=a.uv;return o;}
half4 frag(V i):SV_Target{float3 n=normalize(i.normal),v=normalize(GetWorldSpaceViewDir(i.world));float3 tex=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv).rgb;float sea=saturate((tex.b-tex.r)*7);tex=lerp(tex*float3(.88,1.05,.85),float3(.035,.19,.24)+tex*.3,sea);float light=saturate(dot(n,normalize(float3(-.6,.65,-1))));float rim=pow(1-saturate(dot(n,v)),3);float3 color=tex*(.30+light*.95)+float3(.12,.35,.39)*rim*.65;float2 grid=abs(frac(i.uv*float2(24,12)+.5)-.5);float gridLine=1-smoothstep(.001,.006,min(grid.x,grid.y));color+=float3(.18,.32,.30)*gridLine*.15;return half4(color,1);}
ENDHLSL
} } }
