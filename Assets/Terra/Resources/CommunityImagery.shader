Shader "Terra/CommunityImagery"
{
    Properties { _BaseMap("Source orthophoto", 2D) = "white" {} }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes { float4 positionOS:POSITION; float2 uv:TEXCOORD0; };
            struct Varyings { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; };
            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            Varyings vert(Attributes input) { Varyings output; output.positionCS=TransformObjectToHClip(input.positionOS.xyz); output.uv=input.uv; return output; }
            half4 frag(Varyings input):SV_Target { return half4(SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,input.uv).rgb,1); }
            ENDHLSL
        }
    }
}
