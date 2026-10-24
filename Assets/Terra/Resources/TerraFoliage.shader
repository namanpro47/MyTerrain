Shader "Terra/Painted Canopy"
{
    Properties
    {
        [MainTexture] _BaseMap("Leaf silhouette",2D)="white"{}
        [MainColor] _BaseColor("Canopy color",Color)=(.5,.7,.3,1)
        _Cutoff("Leaf cutoff",Range(0,1))=.35
        _CrownCenter("Crown center",Vector)=(0,4.5,0,0)
        _Cull("Cull",Float)=0
        _Painted("Painted pigment",Float)=0
    }
    SubShader
    {
        Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="TransparentCutout" "Queue"="AlphaTest"}
        Cull Off
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
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half _Cutoff;
                half _Painted;
                float4 _CrownCenter;
            CBUFFER_END
            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            struct A {float4 positionOS:POSITION;float3 normalOS:NORMAL;float2 uv:TEXCOORD0;UNITY_VERTEX_INPUT_INSTANCE_ID};
            struct V {float4 positionCS:SV_POSITION;float3 positionWS:TEXCOORD0;half3 volumeNormal:TEXCOORD1;float2 uv:TEXCOORD2;float3 positionOS:TEXCOORD3;};
            V Vert(A input)
            {
                UNITY_SETUP_INSTANCE_ID(input);
                V o;o.positionWS=TransformObjectToWorld(input.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.positionWS);
                // Leaf cards describe the silhouette; a rounded canopy normal supplies volume rather than lighting every card flat.
                float3 crown=input.positionOS.xyz-_CrownCenter.xyz;
                crown.y*=.65;
                o.volumeNormal=TransformObjectToWorldNormal(normalize(crown+float3(0,.15,0)));
                o.uv=TRANSFORM_TEX(input.uv,_BaseMap);o.positionOS=input.positionOS.xyz;return o;
            }
            half4 Frag(V i):SV_Target
            {
                half4 leaf=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv);clip(leaf.a-_Cutoff);
                Light sun=GetMainLight(TransformWorldToShadowCoord(i.positionWS));
                float diffuse=saturate(dot(normalize(i.volumeNormal),sun.direction)*.65+.35);
                float pigment=.94+.06*sin(i.positionOS.x*16+i.positionOS.z*11)*sin(i.positionOS.y*9);
                half3 shade=lerp(half3(.52,.68,.56),half3(1.12,1.08,.88),diffuse);
                half3 light=SampleSH(half3(0,1,0))*.35+sun.color*(.48+diffuse*.52)*lerp(.64,1,sun.shadowAttenuation);
                float detail=lerp(1,clamp(.5+dot(leaf.rgb,half3(.2126,.7152,.0722))*2.5,.55,1.5),_Painted);
                return half4(_BaseColor.rgb*shade*light*pigment*detail,1);
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Simple Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Simple Lit/DepthOnly"
    }
}
