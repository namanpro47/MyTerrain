Shader "Terra/Artist Leaves"
{
    Properties
    {
        [MainTexture] _BaseMap("Artist leaves",2D)="white"{}
        [MainColor] _BaseColor("Tint",Color)=(1,1,1,1)
        _Cutoff("Cutout",Range(0,1))=.4
        _CrownCenter("Crown center",Vector)=(0,4,0,0)
        _CrownExtent("Crown extent",Vector)=(3,3,3,0)
    }
    SubShader
    {
        Tags {"RenderType"="TransparentCutout" "Queue"="AlphaTest" "RenderPipeline"="UniversalPipeline"}
        Cull Off
        Pass
        {
            Tags {"LightMode"="UniversalForward"}
            AlphaToMask On
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half _Cutoff;
                float4 _CrownCenter;
                float4 _CrownExtent;
            CBUFFER_END
            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            struct A {float4 positionOS:POSITION;float3 normalOS:NORMAL;float2 uv:TEXCOORD0;UNITY_VERTEX_INPUT_INSTANCE_ID};
            struct V {float4 positionCS:SV_POSITION;float3 positionWS:TEXCOORD0;half3 normalWS:TEXCOORD1;float2 uv:TEXCOORD2;half variation:TEXCOORD3;};
            V Vert(A input)
            {
                UNITY_SETUP_INSTANCE_ID(input);
                V o;o.positionWS=TransformObjectToWorld(input.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.positionWS);
                // Shade the crown volume rather than exposing every crossing leaf plane.
                float3 radial=(input.positionOS.xyz-_CrownCenter.xyz)/max(_CrownExtent.xyz,float3(.01,.01,.01));
                radial.y=radial.y*.85+.1;
                o.normalWS=TransformObjectToWorldNormal(normalize(radial));
                o.uv=TRANSFORM_TEX(input.uv,_BaseMap);
                float3 root=TransformObjectToWorld(float3(0,0,0));
                o.variation=frac(sin(dot(root.xz,float2(.071,.037)))*143.71);
                return o;
            }
            half4 Frag(V input):SV_Target
            {
                half4 paint=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,input.uv)*_BaseColor;
                half coverage=saturate((paint.a-_Cutoff)/max(fwidth(paint.a),.001)+.5);clip(coverage-.001);
                half3 normal=normalize(input.normalWS);
                Light sun=GetMainLight(TransformWorldToShadowCoord(input.positionWS));
                half diffuse=saturate(dot(normal,sun.direction)*.65+.35);
                half3 ambient=SampleSH(half3(0,1,0));
                half3 illumination=ambient*.55+sun.color*(.15+diffuse*.85)*lerp(.45,1,sun.shadowAttenuation);
                half3 tint=lerp(half3(.88,1,.94),half3(1.08,1,.77),input.variation);
                return half4(paint.rgb*illumination*tint,coverage);
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Simple Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Simple Lit/DepthOnly"
    }
}
