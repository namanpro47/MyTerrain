Shader "Terra/Soft Groundcover"
{
    Properties
    {
        [MainTexture] _BaseMap("Artist foliage",2D)="white"{}
        [MainColor] _BaseColor("Tint",Color)=(1,1,1,1)
        _Cutoff("Cutout",Range(0,1))=.5
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
            CBUFFER_END
            float _TerraDisplayPixelRatio;
            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            struct A {float4 positionOS:POSITION;float3 normalOS:NORMAL;float2 uv:TEXCOORD0;UNITY_VERTEX_INPUT_INSTANCE_ID};
            struct V {float4 positionCS:SV_POSITION;float3 positionWS:TEXCOORD0;half3 normalWS:TEXCOORD1;float2 uv:TEXCOORD2;float plantPixels:TEXCOORD3;};
            V Vert(A input)
            {
                UNITY_SETUP_INSTANCE_ID(input);
                V o;o.positionWS=TransformObjectToWorld(input.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.positionWS);
                // Low overlapping growth reads as a continuous surface, retaining a little leaf orientation.
                o.normalWS=normalize(TransformObjectToWorldNormal(input.normalOS)*.2+half3(0,.8,0));
                o.uv=TRANSFORM_TEX(input.uv,_BaseMap);
                float3 root=TransformObjectToWorld(float3(0,0,0));
                float height=distance(root,TransformObjectToWorld(float3(0,1.3,0)));
                o.plantPixels=height*_ScreenParams.y*abs(UNITY_MATRIX_P._m11)*.5/(max(.001,abs(TransformWorldToHClip(root).w))*max(1,_TerraDisplayPixelRatio));
                return o;
            }
            half4 Frag(V input):SV_Target
            {
                half4 paint=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,input.uv)*_BaseColor;
                half coverage=saturate((paint.a-_Cutoff)/max(fwidth(paint.a),.001)+.5);
                // Retain the painted bank surface when individual blades become subpixel.
                coverage*=smoothstep(1.5,3.0,input.plantPixels);clip(coverage-.001);
                half3 normal=normalize(input.normalWS);
                Light sun=GetMainLight(TransformWorldToShadowCoord(input.positionWS));
                half diffuse=saturate(dot(normal,sun.direction)*.7+.3);
                half3 illumination=SampleSH(normal)+sun.color*diffuse*sun.shadowAttenuation;
                return half4(paint.rgb*illumination,coverage);
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Simple Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Simple Lit/DepthOnly"
    }
}
