Shader "Terra/Soft Groundcover"
{
    Properties
    {
        [MainTexture] _BaseMap("Artist foliage",2D)="white"{}
        [MainColor] _BaseColor("Tint",Color)=(1,1,1,1)
        _Cutoff("Cutout",Range(0,1))=.5
        _PlantBounds("Mesh vertical bounds",Vector)=(0,1,0,0)
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
                float4 _PlantBounds;
            CBUFFER_END
            float _TerraDisplayPixelRatio;
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
            struct V {float4 positionCS:SV_POSITION;float3 positionWS:TEXCOORD0;half3 normalWS:TEXCOORD1;float2 uv:TEXCOORD2;float plantPixels:TEXCOORD3;half bladeHeight:TEXCOORD4;};
            V Vert(A input)
            {
                UNITY_SETUP_INSTANCE_ID(input);
                V o;o.positionWS=TransformObjectToWorld(input.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.positionWS);
                // Low overlapping growth reads as a continuous surface, retaining a little leaf orientation.
                o.normalWS=normalize(TransformObjectToWorldNormal(input.normalOS)*.2+half3(0,.8,0));
                o.uv=TRANSFORM_TEX(input.uv,_BaseMap);
                o.bladeHeight=saturate((input.positionOS.y-_PlantBounds.x)/max(.001,_PlantBounds.y-_PlantBounds.x));
                float3 root=TransformObjectToWorld(float3(0,0,0));
                float height=distance(root,TransformObjectToWorld(float3(0,1.3,0)));
                o.plantPixels=height*_ScreenParams.y*abs(UNITY_MATRIX_P._m11)*.5/(max(.001,abs(TransformWorldToHClip(root).w))*max(1,_TerraDisplayPixelRatio));
                return o;
            }
            half4 Frag(V input):SV_Target
            {
                // This source is a narrow color palette with unused white atlas space.
                // Mip averaging crosses palette bands; sample its original colors explicitly.
                half4 paint=SAMPLE_TEXTURE2D_LOD(_BaseMap,sampler_BaseMap,input.uv,0)*_BaseColor;
                half coverage=saturate((paint.a-_Cutoff)/max(fwidth(paint.a),.001)+.5);
                // Retain the painted bank surface when individual blades become subpixel.
                coverage*=smoothstep(1.5,3.0,input.plantPixels);clip(coverage-.001);
                half3 normal=normalize(input.normalWS);
                Light sun=GetMainLight(TransformWorldToShadowCoord(input.positionWS));
                half diffuse=saturate(dot(normal,sun.direction));
                half3 illumination=SampleSH(normal)+sun.color*diffuse*sun.shadowAttenuation;
                // Keep the source silhouette and subtle palette variation, with roots in
                // the terrain's value range. Height shading gives overlapping fans depth.
                half value=dot(paint.rgb,half3(.2126,.7152,.0722));
                half3 blade=lerp(half3(.15,.26,.07),half3(.29,.40,.13),smoothstep(0,1,input.bladeHeight));
                blade*=lerp(.9,1.1,saturate(value));
                blade*=lerp(.72,1,smoothstep(0,.65,input.bladeHeight));
                // Keep the authored blade gradient, but honor each stand's color and
                // annual lifecycle tint. Tint must affect hue, not only luminance.
                return half4(blade*illumination*_BaseColor.rgb,coverage);
            }
            ENDHLSL
        }
        // Own alpha-tested passes share the exact material layout with the visible grass.
        // A borrowed Simple Lit pass uses a different material buffer layout.
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
