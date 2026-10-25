Shader "Terra/Artist Grain Field"
{
    Properties
    {
        [MainColor] _BaseColor("Grain colour",Color)=(.72,.62,.34,1)
        [MainTexture] _BaseMap("Artist wheat",2D)="white"{}
        _Cutoff("Silhouette",Range(0,1))=.35
    }
    SubShader
    {
        Tags {"RenderType"="TransparentCutout" "Queue"="AlphaTest" "RenderPipeline"="UniversalPipeline"}
        Pass
        {
            Tags {"LightMode"="UniversalForward"}
            Cull Off
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
                half4 _BaseColor;float4 _BaseMap_ST;half _Cutoff;
            CBUFFER_END
            float _TerraDisplayPixelRatio;
            TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
            struct A {float4 positionOS:POSITION;float2 uv:TEXCOORD0;UNITY_VERTEX_INPUT_INSTANCE_ID};
            struct V {float4 positionCS:SV_POSITION;float3 positionWS:TEXCOORD0;float2 uv:TEXCOORD1;float plantPixels:TEXCOORD2;};
            V Vert(A i)
            {
                UNITY_SETUP_INSTANCE_ID(i);
                V o;o.positionWS=TransformObjectToWorld(i.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.positionWS);o.uv=i.uv;
                float3 root=TransformObjectToWorld(float3(0,0,0));
                float height=distance(root,TransformObjectToWorld(float3(0,2.3,0)));
                o.plantPixels=height*_ScreenParams.y*abs(UNITY_MATRIX_P._m11)*.5/(max(.001,abs(TransformWorldToHClip(root).w))*max(1,_TerraDisplayPixelRatio));
                return o;
            }
            half4 Frag(V i):SV_Target
            {
                half4 painted=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv);
                // Feed coverage to MSAA, retaining clipping on targets without multisampling.
                half coverage=saturate((painted.a-_Cutoff)/max(fwidth(painted.a),.001)+.5);
                coverage*=smoothstep(2.4,4.2,i.plantPixels);
                clip(coverage-.01);
                Light sun=GetMainLight(TransformWorldToShadowCoord(i.positionWS));
                half3 light=SampleSH(half3(0,1,0))+sun.color*.85*lerp(.5,1,sun.shadowAttenuation);
                // The artist's grayscale paint gives the stalk depth without stretching its grain silhouette.
                float fieldU=i.positionWS.x-12*sin(i.positionWS.z*.012)-5*sin(i.positionWS.z*.029);
                float patch=smoothstep(.18,.82,.5+.5*sin(fieldU*.1636246));
                half3 grain=lerp(_BaseColor.rgb*half3(.80,.92,.64),_BaseColor.rgb*half3(1.04,1.01,.94),patch);
                half height=lerp(.62,1,smoothstep(.22,.88,i.uv.y));
                return half4(grain*light*height*lerp(.8,1,painted.r),coverage);
            }
            ENDHLSL
        }
    }
}
