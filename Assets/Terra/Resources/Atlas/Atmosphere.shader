Shader "MyTerrain/Atlas/Atmosphere" {
SubShader { Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Transparent"} Pass { Blend SrcAlpha One ZWrite Off Cull Front
HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
struct A {float4 positionOS:POSITION;float3 normalOS:NORMAL;};struct V {float4 positionCS:SV_POSITION;float3 normal:TEXCOORD0;float3 world:TEXCOORD1;};
V vert(A a){V o;o.positionCS=TransformObjectToHClip(a.positionOS.xyz);o.world=TransformObjectToWorld(a.positionOS.xyz);o.normal=TransformObjectToWorldNormal(a.normalOS);return o;}
half4 frag(V i):SV_Target{float rim=pow(1-abs(dot(normalize(i.normal),normalize(GetWorldSpaceViewDir(i.world)))),5);return half4(.23,.60,.67,rim*.55);}
ENDHLSL
} } }
