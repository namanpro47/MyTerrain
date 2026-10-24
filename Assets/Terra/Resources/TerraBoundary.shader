Shader "Terra/Legible Boundary"
{
 Properties { [MainColor] _BaseColor("Colour", Color)=(1,1,1,1) }
 SubShader
 {
  Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent+101" }
  Pass
  {
   Cull Off
   ZWrite Off
   ZTest Always
   Offset -1, -1
   HLSLPROGRAM
   #pragma vertex Vert
   #pragma fragment Frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   CBUFFER_START(UnityPerMaterial)
   half4 _BaseColor;
   CBUFFER_END
   struct A { float4 positionOS:POSITION; float2 uv:TEXCOORD0; };
   struct V { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; };
   V Vert(A i){V o;o.positionCS=TransformObjectToHClip(i.positionOS.xyz);o.uv=i.uv;return o;}
   half4 Frag(V i):SV_Target{float edge=min(i.uv.y,1-i.uv.y);float inner=smoothstep(.20,.30,edge);return half4(lerp(half3(.10,.19,.15),_BaseColor.rgb,inner),1);}
   ENDHLSL
  }
 }
}
