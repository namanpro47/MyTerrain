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
            struct A { float4 positionOS:POSITION; float3 normalOS:NORMAL; float2 uv:TEXCOORD0; };
            struct V { float4 positionCS:SV_POSITION; float3 positionWS:TEXCOORD0; half3 normalWS:TEXCOORD1; float2 uv:TEXCOORD2; };
            V Vert(A input){V o;o.positionWS=TransformObjectToWorld(input.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.positionWS);o.normalWS=TransformObjectToWorldNormal(input.normalOS);o.uv=input.uv;return o;}
            float Hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
            float Noise(float2 p){float2 i=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(Hash(i),Hash(i+float2(1,0)),f.x),lerp(Hash(i+float2(0,1)),Hash(i+1),f.x),f.y);}
            half4 Frag(V i):SV_Target
            {
                float2 p=i.positionWS.xz;
                // These are illustrative surface currents and shallows, not modelled depth or flow.
                float wash=Noise(p*.24+float2(_MotionTime*.015,0));
                float bankDistance=SAMPLE_TEXTURE2D(_RiverMap,sampler_RiverMap,(p-_LandBounds.xy)/_LandBounds.zw).r;
                float edge=lerp(abs(i.uv.x-.5)*2,smoothstep(.10,.285,bankDistance),_NaturalBanks);
                float shallow=smoothstep(.28,.95,edge+wash*.18);
                half3 deep=lerp(half3(.12,.39,.40),half3(.25,.49,.47),wash);
                half3 shore=lerp(half3(.34,.49,.37),half3(.50,.61,.46),Noise(p*.6));
                half3 albedo=SRGBToLinear(lerp(deep,shore,shallow*.82));
                float footprint=max(length(ddx(p)),length(ddy(p)));
                float fade=1-saturate(footprint*.65);
                float wave=i.uv.y*1.3-_MotionTime*.50+Noise(p*.28)*6;
                float waveWidth=max(fwidth(wave),.06);
                float crest=1-smoothstep(.11,.11+waveWidth,abs(sin(wave)));
                float broken=smoothstep(.50,.77,Noise(float2(i.uv.x*10,i.uv.y*.34)));
                float ripple=crest*broken*fade*(1-shallow*.65);
                float rim=smoothstep(.91,.98,edge)*(1-smoothstep(.98,1.03,edge));
                rim*=smoothstep(.35,.75,Noise(p*.8))*fade;
                Light sun=GetMainLight(TransformWorldToShadowCoord(i.positionWS));
                half light=.72+.28*sun.shadowAttenuation;
                return half4(albedo*light+(ripple*.065+rim*.045)*half3(.72,.87,.82), .98);
            }
            ENDHLSL
        }
    }
}
