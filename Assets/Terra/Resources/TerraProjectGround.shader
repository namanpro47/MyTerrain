Shader "Terra/Project Ground"
{
    Properties
    {
        _BaseColor("Base", Color)=(1,1,1,1)
        _BaseMap("Base",2D)="white"{}
        _Cutoff("Cutoff",Range(0,1))=.5
        _GroundKind("Grass 0 / gravel 1",Float)=0
        _MeadowPaint("Artist meadow",2D)="white"{}
        _MeadowRelief("Artist meadow normal",2D)="bump"{}
        _EarthPaint("Artist earth",2D)="white"{}
        _EarthRelief("Artist earth normal",2D)="bump"{}
    }
    SubShader
    {
        Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry"}
        Pass
        {
            Tags {"LightMode"="UniversalForward"}
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor; float4 _BaseMap_ST; half _Cutoff; half _GroundKind;
            CBUFFER_END
            TEXTURE2D(_MeadowPaint); SAMPLER(sampler_MeadowPaint);
            TEXTURE2D(_MeadowRelief); SAMPLER(sampler_MeadowRelief);
            TEXTURE2D(_EarthPaint); SAMPLER(sampler_EarthPaint);
            TEXTURE2D(_EarthRelief); SAMPLER(sampler_EarthRelief);
            struct A {float4 positionOS:POSITION; float3 normalOS:NORMAL;};
            struct V {float4 positionCS:SV_POSITION; float3 positionWS:TEXCOORD0; half3 normalWS:TEXCOORD1;};
            V Vert(A i){V o;o.positionWS=TransformObjectToWorld(i.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.positionWS);o.normalWS=TransformObjectToWorldNormal(i.normalOS);return o;}
            float Hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
            // Same translated triangular blend as the landscape: retain the licensed
            // brush detail without laying a repeated square stamp across a plaza.
            half4 Paint(TEXTURE2D_PARAM(tex,smp),float2 uv,bool averageDetail)
            {
                float2 dx=ddx(uv),dy=ddy(uv);
                float distant=smoothstep(.035,.09,max(length(dx),length(dy)));
                half4 average=SAMPLE_TEXTURE2D_LOD(tex,smp,uv,10);
                if(distant>.999)return average;
                float2 cell=floor(uv),f=frac(uv),a=cell,b=cell+float2(1,0),c=cell+float2(0,1);
                float3 weights=float3(1-f.x-f.y,f.x,f.y);
                if(f.x+f.y>1){a=cell+1;weights=float3(f.x+f.y-1,1-f.y,1-f.x);}
                weights=pow(max(weights,0),4);weights/=max(dot(weights,1),.0001);
                float2 oa=float2(Hash(a),Hash(a+37))*17,ob=float2(Hash(b),Hash(b+37))*17,oc=float2(Hash(c),Hash(c+37))*17;
                half4 paint=average;
                if(averageDetail)
                    paint=SAMPLE_TEXTURE2D_LOD(tex,smp,uv+oa,6)*weights.x+SAMPLE_TEXTURE2D_LOD(tex,smp,uv+ob,6)*weights.y+SAMPLE_TEXTURE2D_LOD(tex,smp,uv+oc,6)*weights.z;
                else
                    paint=SAMPLE_TEXTURE2D_GRAD(tex,smp,uv+oa,dx,dy)*weights.x+SAMPLE_TEXTURE2D_GRAD(tex,smp,uv+ob,dx,dy)*weights.y+SAMPLE_TEXTURE2D_GRAD(tex,smp,uv+oc,dx,dy)*weights.z;
                return lerp(paint,average,distant);
            }
            half4 Frag(V i):SV_Target
            {
                half3 normal=normalize(i.normalWS);
                // World-metre projection also handles vertical foundation sides;
                // no source mesh UVs, bounds or placement geometry are modified.
                bool top=abs(normal.y)>.6;
                bool sideX=abs(normal.x)>abs(normal.z);
                float2 p=top?i.positionWS.xz:sideX?i.positionWS.zy:i.positionWS.xy;
                float fade=1-saturate(max(length(ddx(p)),length(ddy(p)))*.55);
                half3 albedo,relief;
                if(_GroundKind>.5)
                {
                    half3 earth=Paint(TEXTURE2D_ARGS(_EarthPaint,sampler_EarthPaint),p/9,false).rgb;
                    albedo=lerp(half3(.36,.32,.22),earth*half3(.82,.84,.79),.25);
                    relief=UnpackNormal(Paint(TEXTURE2D_ARGS(_EarthRelief,sampler_EarthRelief),p/9,false));
                }
                else
                {
                    half3 grass=Paint(TEXTURE2D_ARGS(_MeadowPaint,sampler_MeadowPaint),p/40,false).rgb;
                    half3 average=Paint(TEXTURE2D_ARGS(_MeadowPaint,sampler_MeadowPaint),p/40,true).rgb;
                    half3 strokes=clamp(grass/max(average,.025),.55,1.55);
                    half luma=dot(grass,half3(.2126,.7152,.0722));
                    half3 living=lerp(luma.xxx,grass,.65)*half3(.43,.64,.56);
                    albedo=lerp(half3(.23,.38,.12),living,.15)*lerp(half3(1,1,1),strokes,.55);
                    relief=UnpackNormal(Paint(TEXTURE2D_ARGS(_MeadowRelief,sampler_MeadowRelief),p/40,false));
                }
                half3 tangent=normalize(cross(abs(normal.y)>.95?half3(0,0,1):half3(0,1,0),normal));
                half3 bitangent=cross(normal,tangent);
                relief=normalize(lerp(half3(0,0,1),relief,.35*fade));
                normal=normalize(tangent*relief.x+bitangent*relief.y+normal*relief.z);
                Light sun=GetMainLight(TransformWorldToShadowCoord(i.positionWS));
                return half4(albedo*_BaseColor.rgb*(SampleSH(normal)+sun.color*saturate(dot(normal,sun.direction))*sun.shadowAttenuation),1);
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Simple Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Simple Lit/DepthOnly"
    }
}
