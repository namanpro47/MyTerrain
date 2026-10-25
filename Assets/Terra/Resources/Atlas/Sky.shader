Shader "MyTerrain/Atlas/Sky" {
SubShader { Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"} Pass { Cull Off
HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
struct A {float4 positionOS:POSITION;};struct V {float4 positionCS:SV_POSITION;float2 position:TEXCOORD0;};
V vert(A a){V o;o.positionCS=TransformObjectToHClip(a.positionOS.xyz);o.position=a.positionOS.xy;return o;}
half4 frag(V i):SV_Target{float glow=exp(-dot(i.position,i.position)*.035);return half4(lerp(float3(.001,.003,.006),float3(.006,.02,.028),glow),1);}
ENDHLSL
} } }
