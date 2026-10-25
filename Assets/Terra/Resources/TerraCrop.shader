Shader "Terra/Matte Grain"
{
    Properties
    {
        [MainColor] _BaseColor("Grain colour",Color)=(.74,.65,.38,1)
        _BaseMap("Base",2D)="white"{}
        _Cutoff("Cutoff",Range(0,1))=.5
    }
    SubShader
    {
        Tags {"RenderType"="Opaque" "RenderPipeline"="UniversalPipeline"}
        Pass
        {
            Tags {"LightMode"="UniversalForward"}
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
                half _Cutoff;
            CBUFFER_END
            struct A {float4 positionOS:POSITION;float3 normalOS:NORMAL;UNITY_VERTEX_INPUT_INSTANCE_ID};
            struct V {float4 positionCS:SV_POSITION;float3 positionWS:TEXCOORD0;half3 normalWS:TEXCOORD1;half height:TEXCOORD2;};
            V Vert(A i)
            {
                UNITY_SETUP_INSTANCE_ID(i);
                V o;o.positionWS=TransformObjectToWorld(i.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.positionWS);
                o.normalWS=TransformObjectToWorldNormal(i.normalOS);o.height=i.positionOS.y;return o;
            }
            half4 Frag(V i):SV_Target
            {
                Light sun=GetMainLight(TransformWorldToShadowCoord(i.positionWS));
                half3 normal=normalize(i.normalWS);
                // Broad wrapped light keeps tiny grain facets from becoming a black/gold pixel grid.
                half wrap=.7+.3*saturate(dot(normal,sun.direction));
                half3 light=SampleSH(half3(0,1,0))+sun.color*wrap*lerp(.55,1,sun.shadowAttenuation);
                half stalk=lerp(.72,1,smoothstep(.3,1.8,i.height));
                return half4(_BaseColor.rgb*light*stalk,1);
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Simple Lit/DepthOnly"
    }
}
