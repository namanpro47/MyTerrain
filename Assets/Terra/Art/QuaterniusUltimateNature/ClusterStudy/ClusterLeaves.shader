Shader "Terra/Study44 Cluster Leaves"
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
        HLSLINCLUDE
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
        ENDHLSL
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
            struct A {float4 positionOS:POSITION;float3 normalOS:NORMAL;float2 uv:TEXCOORD0;UNITY_VERTEX_INPUT_INSTANCE_ID};
            struct V {float4 positionCS:SV_POSITION;float3 positionWS:TEXCOORD0;half3 normalWS:TEXCOORD1;float2 uv:TEXCOORD2;half variation:TEXCOORD3;};
            V Vert(A input)
            {
                UNITY_SETUP_INSTANCE_ID(input);
                V o;o.positionWS=TransformObjectToWorld(input.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.positionWS);
                // Shade the crown volume rather than exposing every crossing leaf plane.
                float3 radial=(input.positionOS.xyz-_CrownCenter.xyz)/max(_CrownExtent.xyz,float3(.0001,.0001,.0001));
                radial.z=radial.z*.85+.1;
                o.normalWS=TransformObjectToWorldNormal(input.normalOS);
                o.uv=TRANSFORM_TEX(input.uv,_BaseMap);
                float3 root=TransformObjectToWorld(float3(0,0,0));
                o.variation=frac(sin(dot(root.xz,float2(.071,.037)))*143.71);
                return o;
            }
            half4 Frag(V input):SV_Target
            {
                half4 paint=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,input.uv)*_BaseColor;
                half coverage=saturate((paint.a-_Cutoff)/max(fwidth(paint.a),.001)+.5);clip(coverage-.001);
                paint.rgb=lerp(paint.rgb,dot(paint.rgb,half3(.2126,.7152,.0722)).xxx,.28);
                half3 normal=normalize(input.normalWS);
                Light sun=GetMainLight(TransformWorldToShadowCoord(input.positionWS));
                half diffuse=saturate(dot(normal,sun.direction)*.85+.15);
                half3 ambient=SampleSH(half3(0,1,0));
                half3 illumination=ambient*.35+sun.color*(.08+diffuse*.92)*lerp(.25,1,sun.shadowAttenuation);
                half3 tint=lerp(half3(.88,1,.94),half3(1.08,1,.77),input.variation);
                return half4(paint.rgb*illumination*tint,coverage);
            }
            ENDHLSL
        }
        // Own alpha-tested passes share the exact material layout with the visible crown.
        // A borrowed opaque Simple Lit pass casts leaf-card rectangles and mismatches its buffer.
        Pass
        {
            Name "ShadowCaster"
            Tags {"LightMode"="ShadowCaster"}
            ZWrite On ZTest LEqual ColorMask 0
            HLSLPROGRAM
            #pragma vertex ShadowVert
            #pragma fragment CutoutFrag
            #pragma multi_compile_instancing
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            float3 _LightDirection;
            float3 _LightPosition;
            struct SA {float4 positionOS:POSITION;float3 normalOS:NORMAL;float2 uv:TEXCOORD0;UNITY_VERTEX_INPUT_INSTANCE_ID};
            struct SV {float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;};
            SV ShadowVert(SA input)
            {
                UNITY_SETUP_INSTANCE_ID(input);
                SV o;float3 pos=TransformObjectToWorld(input.positionOS.xyz);
                float3 n=TransformObjectToWorldNormal(input.normalOS);
                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                float3 lightDirection=normalize(_LightPosition-pos);
                #else
                float3 lightDirection=_LightDirection;
                #endif
                o.positionCS=ApplyShadowClamping(TransformWorldToHClip(ApplyShadowBias(pos,n,lightDirection)));
                o.uv=TRANSFORM_TEX(input.uv,_BaseMap);return o;
            }
            half4 CutoutFrag(SV input):SV_Target
            {clip(SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,input.uv).a*_BaseColor.a-_Cutoff);return 0;}
            ENDHLSL
        }
        Pass
        {
            Name "DepthOnly"
            Tags {"LightMode"="DepthOnly"}
            ZWrite On ColorMask R
            HLSLPROGRAM
            #pragma vertex DepthVert
            #pragma fragment DepthFrag
            #pragma multi_compile_instancing
            struct DA {float4 positionOS:POSITION;float2 uv:TEXCOORD0;UNITY_VERTEX_INPUT_INSTANCE_ID};
            struct DV {float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;};
            DV DepthVert(DA input)
            {UNITY_SETUP_INSTANCE_ID(input);DV o;o.positionCS=TransformObjectToHClip(input.positionOS.xyz);o.uv=TRANSFORM_TEX(input.uv,_BaseMap);return o;}
            half4 DepthFrag(DV input):SV_Target
            {clip(SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,input.uv).a*_BaseColor.a-_Cutoff);return input.positionCS.z;}
            ENDHLSL
        }
    }
}
