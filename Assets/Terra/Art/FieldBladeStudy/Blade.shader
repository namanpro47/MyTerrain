// Study adaptation of GDQuest wind_grass.shader, MIT, copyright (c) 2020 GDQuest.
// Original license/source retained in GDQuest/. URP world wind, normalized height UV,
// and optional received shadow are adaptations; no character interaction is used.
Shader "Terra/Studies/Field Blade" {
Properties {
_BaseColor("Status tint",Color)=(1,1,1,1)
_RootColor("Root",Color)=(.054902,.556863,.439216,1)
_MidColor("Middle",Color)=(.321569,.890196,.341176,1)
_TipColor("Tip",Color)=(.4991,.92,.3588,1)
_WindNoise("Wind noise",2D)="gray"{}
_WindStrength("Horizontal wind meters",Float)=1.2
_WindVertical("Vertical wind meters",Float)=.18
_WindSpeed("Wind speed",Float)=.2
_WindTile("World wind tile",Float)=20
_ShadowStrength("Received shadow",Range(0,1))=0
_StudyTime("Fixed study time; negative runs",Float)=-1
}
SubShader {Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"}
Pass {Tags {"LightMode"="UniversalForward"} Cull Off ZWrite On
HLSLPROGRAM
#pragma vertex Vert
#pragma fragment Frag
#pragma multi_compile_instancing
#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
#pragma multi_compile_fragment _ _SHADOWS_SOFT
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
TEXTURE2D(_WindNoise);SAMPLER(sampler_WindNoise);
CBUFFER_START(UnityPerMaterial)
float4 _BaseColor,_RootColor,_MidColor,_TipColor,_WindNoise_ST;
float _WindStrength,_WindVertical,_WindSpeed,_WindTile,_ShadowStrength,_StudyTime;
CBUFFER_END
struct A{float4 p:POSITION;float2 uv:TEXCOORD0;UNITY_VERTEX_INPUT_INSTANCE_ID};
struct V{float4 p:SV_POSITION;float3 w:TEXCOORD0;float height:TEXCOORD1;};
V Vert(A i){UNITY_SETUP_INSTANCE_ID(i);V o;o.w=TransformObjectToWorld(i.p.xyz);o.height=saturate(i.uv.y);float time=_StudyTime<0?_Time.y:_StudyTime;float2 direction=normalize(float2(1,.5));float2 worldUV=o.w.xz/max(.1,_WindTile)+direction*time*_WindSpeed;float noise=SAMPLE_TEXTURE2D_LOD(_WindNoise,sampler_WindNoise,worldUV,0).r-.5;float scale=length(TransformObjectToWorldDir(float3(0,1,0),false));o.w+=float3(direction.x*noise*_WindStrength,(1-noise)*_WindVertical,direction.y*noise*_WindStrength)*o.height*scale;o.p=TransformWorldToHClip(o.w);return o;}
half4 Frag(V i):SV_Target{half3 color=i.height<.486339?lerp(_RootColor.rgb,_MidColor.rgb,saturate(i.height/.486339)):lerp(_MidColor.rgb,_TipColor.rgb,saturate((i.height-.486339)/(.966102-.486339)));Light sun=GetMainLight(TransformWorldToShadowCoord(i.w));return half4(color*_BaseColor.rgb*lerp(1,sun.shadowAttenuation,_ShadowStrength),1);}
ENDHLSL
}}
}
