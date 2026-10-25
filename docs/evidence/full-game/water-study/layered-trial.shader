Shader "Terra/Study Layered Stream"
{
    Properties { _BedMap("Artist bed stones",2D)="white"{} _MotionTime("Motion time",Float)=0 _NaturalBanks("Natural bank detail",Float)=0 _RiverMap("Bank distance",2D)="black"{} _LandBounds("Bounds",Vector)=(0,0,1,1) }
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
            TEXTURE2D(_BedMap);SAMPLER(sampler_BedMap);
            struct A { float4 positionOS:POSITION; float3 normalOS:NORMAL; float2 uv:TEXCOORD0; };
            struct V { float4 positionCS:SV_POSITION; float3 positionWS:TEXCOORD0; half3 normalWS:TEXCOORD1; float2 uv:TEXCOORD2; };
            V Vert(A input){V o;o.positionWS=TransformObjectToWorld(input.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.positionWS);o.normalWS=TransformObjectToWorldNormal(input.normalOS);o.uv=input.uv;return o;}
            float Hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
            float Noise(float2 p){float2 i=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(Hash(i),Hash(i+float2(1,0)),f.x),lerp(Hash(i+float2(0,1)),Hash(i+1),f.x),f.y);}
            half4 Frag(V i):SV_Target
            {
                float2 p=i.positionWS.xz;
                // Illustrative optical layers only: no surveyed bed, depth or physical flow claim.
                float distance=SAMPLE_TEXTURE2D(_RiverMap,sampler_RiverMap,(p-_LandBounds.xy)/_LandBounds.zw).r;
                float edge=lerp(abs(i.uv.x-.5)*2,smoothstep(.08,.285,distance),_NaturalBanks);
                float shallows=smoothstep(.28,.95,edge);
                float footprint=max(length(ddx(p)),length(ddy(p)));
                float detail=1-saturate(footprint*.7);
                float t=_MotionTime;
                float broad=Noise(p*.16+float2(t*.012,0));
                float waveA=i.uv.y*1.7-t*.32+i.uv.x*3;
                float waveB=i.uv.y*.73+t*.17-i.uv.x*4;
                float2 distortion=float2(sin(waveA),cos(waveB))*.025*detail;
                half3 stone=SAMPLE_TEXTURE2D(_BedMap,sampler_BedMap,p/2.7+distortion).rgb;
                float stoneLuma=dot(stone,half3(.2126,.7152,.0722));
                half3 deep=SRGBToLinear(lerp(half3(.12,.38,.40),half3(.20,.46,.47),broad));
                half3 shallowsColor=SRGBToLinear(half3(.30,.48,.42));
                half3 water=lerp(deep,shallowsColor,shallows*.70);
                // Color absorption stays over the whole bed; no dry paving-like border.
                water*=lerp(.87,1.20,saturate(stoneLuma*2.3))*(.96+.04*broad);
                water=lerp(deep,water,.25+.65*shallows);
                float3 surfaceNormal=normalize(float3((sin(waveA)*.10+sin(waveB)*.055)*detail,1,cos(waveA*.8)*.08*detail));
                float3 view=GetWorldSpaceNormalizeViewDir(i.positionWS);
                float fresnel=.035+.30*pow(1-saturate(dot(view,surfaceNormal)),4);
                half3 reflectedSky=SRGBToLinear(half3(.56,.69,.66));
                water=lerp(water,reflectedSky,fresnel);
                Light sun=GetMainLight(TransformWorldToShadowCoord(i.positionWS));
                float3 halfway=SafeNormalize(view+sun.direction);
                float glint=pow(saturate(dot(surfaceNormal,halfway)),80)*.22*detail;
                float stream=i.uv.y*1.1-t*.28+Noise(p*.24)*4;
                float width=max(fwidth(stream),.06);
                float crest=1-smoothstep(.04,.04+width,abs(sin(stream)));
                float broken=smoothstep(.62,.83,Noise(float2(i.uv.x*8,i.uv.y*.28)));
                float ripple=crest*broken*detail*(1-shallows*.65)*.04;
                float rim=smoothstep(.94,.985,edge)*(1-smoothstep(.985,1.02,edge));
                rim*=smoothstep(.34,.72,Noise(p*.7))*detail*.02;
                float light=.72+.28*sun.shadowAttenuation;
                return half4(water*light+(glint+ripple+rim)*sun.color*sun.shadowAttenuation,.98);
            }
            ENDHLSL
        }
    }
}
