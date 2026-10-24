Shader "Terra/Legible Boundary"
{
 Properties { [MainColor] _BaseColor("Colour", Color)=(1,1,1,1) }
 SubShader
 {
  Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }
  Pass
  {
   Cull Off
   HLSLPROGRAM
   #pragma vertex Vert
   #pragma fragment Frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   CBUFFER_START(UnityPerMaterial)
   half4 _BaseColor;
   CBUFFER_END
   struct A { float4 positionOS:POSITION; };
   struct V { float4 positionCS:SV_POSITION; };
   V Vert(A i){V o;o.positionCS=TransformObjectToHClip(i.positionOS.xyz);return o;}
   half4 Frag(V i):SV_Target{return _BaseColor;}
   ENDHLSL
  }
 }
}
