Shader "Terra/Proposal Boundary"
{
 Properties { [MainColor] _BaseColor("Colour", Color)=(.7,1,.9,1) _DashLength("Dash length in metres", Float)=12 }
 SubShader
 {
  Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="TransparentCutout" "Queue"="Transparent+102" }
  Pass
  {
   Cull Off
   ZWrite Off
   ZTest Always
   HLSLPROGRAM
   #pragma vertex Vert
   #pragma fragment Frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   CBUFFER_START(UnityPerMaterial)
   half4 _BaseColor;
   float _DashLength;
   CBUFFER_END
   struct A { float4 positionOS:POSITION; float2 uv:TEXCOORD0; };
   struct V { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; };
   V Vert(A i){V o;o.positionCS=TransformObjectToHClip(i.positionOS.xyz);o.uv=i.uv;return o;}
   half4 Frag(V i):SV_Target
   {
    clip(.68-frac(i.uv.x/max(.2,_DashLength)));
    float edge=min(i.uv.y,1-i.uv.y);
    return half4(lerp(half3(.08,.16,.15),_BaseColor.rgb,smoothstep(.12,.24,edge)),1);
   }
   ENDHLSL
  }
 }
}
