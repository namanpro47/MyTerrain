Shader "Terra/Painted Tree Study"
{
    Properties {_BaseMap("Original tree atlas",2D)="white"{} _Cutoff("Silhouette",Range(0,1))=.32}
    SubShader
    {
        Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="TransparentCutout" "Queue"="AlphaTest"}
        Cull Off
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;half _Cutoff;
        CBUFFER_END
        TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
        float3 _PaintedTreeRight;
        struct A {float4 positionOS:POSITION;float2 uv:TEXCOORD0;UNITY_VERTEX_INPUT_INSTANCE_ID};
        struct V {float4 positionCS:SV_POSITION;float3 positionWS:TEXCOORD0;float2 uv:TEXCOORD1;};
        float3 TreePosition(float3 p)
        {
            float4x4 m=GetObjectToWorldMatrix();float3 center=m._m03_m13_m23;
            float sx=length(m._m00_m10_m20),sy=length(m._m01_m11_m21);
            return center+_PaintedTreeRight*p.x*sx+float3(0,p.y*sy,0);
        }
        V Vert(A i){UNITY_SETUP_INSTANCE_ID(i);V o;o.positionWS=TreePosition(i.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.positionWS);o.uv=i.uv;return o;}
        ENDHLSL
        Pass
        {
            Tags {"LightMode"="UniversalForward"}
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            half4 Frag(V i):SV_Target
            {
                half4 color=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv);clip(color.a-_Cutoff);
                Light sun=GetMainLight(TransformWorldToShadowCoord(i.positionWS));
                return half4(color.rgb*lerp(.68,1,sun.shadowAttenuation),1);
            }
            ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"
            Tags {"LightMode"="ShadowCaster"}
            ZWrite On ColorMask 0
            HLSLPROGRAM
            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag
            #pragma multi_compile_instancing
            float3 _LightDirection;
            V ShadowVert(A i){UNITY_SETUP_INSTANCE_ID(i);V o;o.positionWS=TreePosition(i.positionOS.xyz);o.positionCS=ApplyShadowClamping(TransformWorldToHClip(ApplyShadowBias(o.positionWS,float3(0,1,0),_LightDirection)));o.uv=i.uv;return o;}
            half4 ShadowFrag(V i):SV_Target{clip(SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv).a-_Cutoff);return 0;}
            ENDHLSL
        }
    }
}
