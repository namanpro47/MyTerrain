Shader "MyTerrain/Atlas/Sky" {
SubShader { Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"} Pass { Cull Off
HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
struct A {float4 positionOS:POSITION;};struct V {float4 positionCS:SV_POSITION;float2 position:TEXCOORD0;};
V vert(A a){V o;o.positionCS=TransformObjectToHClip(a.positionOS.xyz);o.position=a.positionOS.xy;return o;}
half4 frag(V i):SV_Target{float glow=exp(-dot(i.position,i.position)*.035);return half4(lerp(float3(.012,.032,.055),float3(.045,.14,.17),glow),1);}
ENDHLSL
} } }
