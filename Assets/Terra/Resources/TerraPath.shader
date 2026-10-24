Shader "Terra/Soft Rural Path"
{
    SubShader
    {
        Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Transparent-10" "RenderType"="Transparent"}
        Pass
        {
            Tags {"LightMode"="UniversalForward"}
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite On
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            struct A {float4 positionOS:POSITION;float3 normalOS:NORMAL;float2 uv:TEXCOORD0;};
            struct V {float4 positionCS:SV_POSITION;float3 positionWS:TEXCOORD0;half3 normalWS:TEXCOORD1;float2 uv:TEXCOORD2;};
            V Vert(A i){V o;o.positionWS=TransformObjectToWorld(i.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.positionWS);o.normalWS=TransformObjectToWorldNormal(i.normalOS);o.uv=i.uv;return o;}
            float Hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
            float Noise(float2 p){float2 a=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(Hash(a),Hash(a+float2(1,0)),f.x),lerp(Hash(a+float2(0,1)),Hash(a+1),f.x),f.y);}
            half4 Frag(V i):SV_Target
            {
                float2 p=i.positionWS.xz;float grain=Noise(p*3);float mottling=Noise(p*.5);
                float edge=abs(i.uv.x)+(Noise(p*1.1)-.5)*.08;
                float alpha=1-smoothstep(.73,1,edge);clip(alpha-.02);
                half3 color=SRGBToLinear(lerp(half3(.53,.51,.42),half3(.59,.57,.46),mottling))*(.94+grain*.12);
                // Two quiet worn lanes; visual surface detail, not mapped lane markings.
                color*=1-.075*exp(-pow((abs(i.uv.x)-.36)*9,2));
                Light sun=GetMainLight(TransformWorldToShadowCoord(i.positionWS));half3 normal=normalize(i.normalWS);
                return half4(color*(SampleSH(normal)+sun.color*saturate(dot(normal,sun.direction))*sun.shadowAttenuation),alpha);
            }
            ENDHLSL
        }
    }
}
