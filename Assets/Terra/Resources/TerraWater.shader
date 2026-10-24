Shader "Terra/Painted Stream"
{
    Properties { _MotionTime("Motion time",Float)=0 _NaturalBanks("Natural bank detail",Float)=0 _RiverMap("Bank distance",2D)="black"{} _LandBounds("Bounds",Vector)=(0,0,1,1) }
    SubShader
    {
        Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent"}
        Pass
        {
            Tags {"LightMode"="UniversalForward"}
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            CBUFFER_START(UnityPerMaterial)
                float _MotionTime;float _NaturalBanks;float4 _LandBounds;
            CBUFFER_END
            TEXTURE2D(_RiverMap);SAMPLER(sampler_RiverMap);
            struct A { float4 positionOS:POSITION; float3 normalOS:NORMAL; };
            struct V { float4 positionCS:SV_POSITION; float3 positionWS:TEXCOORD0; half3 normalWS:TEXCOORD1; };
            V Vert(A input){V o;o.positionWS=TransformObjectToWorld(input.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.positionWS);o.normalWS=TransformObjectToWorldNormal(input.normalOS);return o;}
            float Hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
            float Noise(float2 p){float2 i=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(Hash(i),Hash(i+float2(1,0)),f.x),lerp(Hash(i+float2(0,1)),Hash(i+1),f.x),f.y);}
            half4 Frag(V i):SV_Target
            {
                float2 p=i.positionWS.xz;
                float wash=Noise(p*.065+float2(_MotionTime*.015,0));
                half3 albedo=SRGBToLinear(lerp(half3(.17,.41,.48),half3(.29,.55,.59),wash));
                float ripple=smoothstep(.76,.9,Noise(p*float2(.75,2.4)+float2(_MotionTime*.1,0)));
                float bankDistance=SAMPLE_TEXTURE2D(_RiverMap,sampler_RiverMap,(p-_LandBounds.xy)/_LandBounds.zw).r;
                float edge=smoothstep(.19,.28,bankDistance)*_NaturalBanks;
                albedo=lerp(albedo,SRGBToLinear(half3(.27,.42,.34)),edge*.45);
                float fade=1-saturate(max(length(ddx(p)),length(ddy(p)))*.5);
                Light sun=GetMainLight(TransformWorldToShadowCoord(i.positionWS));
                half light=.65+.35*sun.shadowAttenuation;
                return half4(albedo*light+ripple*fade*.012, .96);
            }
            ENDHLSL
        }
    }
}
