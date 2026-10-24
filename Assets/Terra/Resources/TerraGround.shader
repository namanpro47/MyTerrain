Shader "Terra/Illustrated Ground"
{
    Properties
    {
        [MainColor] _BaseColor("Land colour", Color) = (0.5,0.6,0.3,1)
        _BaseMap("Base",2D)="white"{}
        _Cutoff("Cutoff",Range(0,1))=0.5
        _SurfaceKind("Decorative surface",Float)=0
        _LandCoverMap("Visual cover weights",2D)="white"{}
        _RiverMap("Illustrative bank distance",2D)="white"{}
        _LandBounds("World bounds",Vector)=(-590,-630,1180,1260)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Name "IllustratedGround"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
                half _Cutoff;
                half _SurfaceKind;
                float4 _LandBounds;
            CBUFFER_END
            TEXTURE2D(_LandCoverMap); SAMPLER(sampler_LandCoverMap);
            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_RiverMap); SAMPLER(sampler_RiverMap);
            struct A { float4 positionOS:POSITION; float3 normalOS:NORMAL; };
            struct V { float4 positionCS:SV_POSITION; float3 positionWS:TEXCOORD0; half3 normalWS:TEXCOORD1; };
            V Vert(A input){V o;o.positionWS=TransformObjectToWorld(input.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.positionWS);o.normalWS=TransformObjectToWorldNormal(input.normalOS);return o;}
            float Hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
            float Noise(float2 p){float2 i=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(Hash(i),Hash(i+float2(1,0)),f.x),lerp(Hash(i+float2(0,1)),Hash(i+1),f.x),f.y);}
            half4 Frag(V input):SV_Target
            {
                float2 p=input.positionWS.xz;
                // World-space grain is decorative; source geometry and classification stay intact.
                float broad=Noise(p*.045)*.55+Noise(p*.12)*.3+Noise(p*.38)*.15;
                float fade=1-saturate(max(length(ddx(p)),length(ddy(p)))*.55);
                float grain=(Noise(p*1.6)-.5)*.035*fade;
                half3 albedo=_BaseColor.rgb*(.84+broad*.32+grain);
                // Fine marks fade before becoming pixel noise at the valley camera.
                if (_SurfaceKind > .5 && _SurfaceKind < 1.5) {
                    float paintedValue=dot(SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,p/32).rgb,half3(.2126,.7152,.0722));
                    albedo*=clamp(paintedValue/.18,.94,1.06);
                    float2 cellPos=p*.55;
                    float seed=Hash(floor(cellPos));
                    float2 local=frac(cellPos)-.5;
                    float blade=(1-smoothstep(.035,.11,abs(local.x+local.y*.2)))*(1-smoothstep(.12,.34,abs(local.y)));
                    albedo+=blade*step(.88,seed)*fade*half3(.025,.035,.01);
                    albedo*=1-(1-smoothstep(.15,.29,length(local)))*step(.83,seed)*fade*.10;
                }
                if (_SurfaceKind > 1.5 && _SurfaceKind < 2.5) {
                    float ripples=Noise(p*float2(.55,1.15));
                    albedo*=.91+ripples*.20;
                }
                if (_SurfaceKind > 2.5) {
                    float2 offset=float2(Noise(p*.11),Noise(p*.13+91))-.5;
                    half4 cover=SAMPLE_TEXTURE2D(_LandCoverMap,sampler_LandCoverMap,(p+offset*7-_LandBounds.xy)/_LandBounds.zw);
                    float patches=Noise(p*.025+offset)*.6+Noise(p*.10)*.4;
                    half3 woodland=lerp(half3(.22,.35,.12),half3(.37,.47,.18),patches);
                    half3 meadow=lerp(half3(.40,.57,.19),half3(.61,.68,.30),patches);
                    half3 field=lerp(half3(.61,.53,.28),half3(.76,.65,.36),patches);
                    float row=(.5+.5*sin((p.x+p.y*.11)*2.5));
                    float rowFade=1-saturate(max(length(ddx(p)),length(ddy(p)))*.45);
                    field*=1-.16*pow(row,3)*rowFade;
                    // Coarse mapped sealed pixels are context; the separate mapped route defines its visible surface.
                    half3 verge=lerp(meadow,half3(.62,.58,.43),.35+patches*.15);
                    albedo=woodland*cover.r+field*cover.g+meadow*cover.b+verge*cover.a;
                    float bankDistance=SAMPLE_TEXTURE2D(_RiverMap,sampler_RiverMap,(p-_LandBounds.xy)/_LandBounds.zw).r;
                    // A distinct dark earth face meets a narrow grassy lip; avoid broad pale outlined banks.
                    float bank=1-smoothstep(.29,.35,bankDistance);
                    half3 bankColor=lerp(half3(.24,.25,.17),half3(.40,.36,.24),smoothstep(.16,.30,bankDistance));
                    albedo=lerp(albedo,bankColor,bank);
                    albedo=SRGBToLinear(albedo);
                    half3 painted=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,p/32).rgb;
                    float paintValue=dot(painted,half3(.2126,.7152,.0722));
                    albedo*=lerp(clamp(paintValue/.18,.97,1.03),1,saturate(cover.g*.65+cover.a+bank));
                    albedo*=.995+Noise(p*.9)*.01;
                }
                half3 normal=normalize(input.normalWS);
                Light sun=GetMainLight(TransformWorldToShadowCoord(input.positionWS));
                half diffuse=saturate(dot(normal,sun.direction));
                half3 lighting=SampleSH(normal)+sun.color*diffuse*sun.shadowAttenuation;
                return half4(albedo*lighting,1);
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Simple Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Simple Lit/DepthOnly"
    }
}
