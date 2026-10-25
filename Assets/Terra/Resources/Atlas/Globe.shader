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
half4 frag(V i):SV_Target
{
    float3 n=normalize(i.normal),v=normalize(GetWorldSpaceViewDir(i.world));
    float3 source=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv).rgb;
    float sea=smoothstep(.005,.065,source.b-source.r);
    float height=dot(source,float3(.35,.50,.15));
    float vegetation=saturate((source.g-source.r)*7+.40);
    float3 land=lerp(float3(.72,.66,.39),float3(.33,.61,.37),vegetation);
    land=lerp(land,float3(.90,.94,.81),smoothstep(.46,.78,height));
    land*=.80+height*.45;
    float3 ocean=lerp(float3(.055,.31,.43),float3(.19,.56,.60),saturate(height*5));
    float3 surface=lerp(land,ocean,sea);
    float light=saturate(dot(n,normalize(float3(-.65,.75,-1))));
    float rim=pow(1-saturate(dot(n,v)),3);
    float3 color=surface*(.42+light*.83)+float3(.15,.47,.52)*rim*.70;
    return half4(color,1);
}
ENDHLSL
} } }
