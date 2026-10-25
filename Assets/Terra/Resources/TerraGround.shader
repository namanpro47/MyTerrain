Shader "Terra/Illustrated Ground"
{
    Properties
    {
        [MainColor] _BaseColor("Land colour", Color) = (0.5,0.6,0.3,1)
        _BaseMap("Base",2D)="white"{}
        _BankMap("Artist earth surface",2D)="white"{}
        _GrassNormal("Artist grass relief",2D)="bump"{}
        _EarthNormal("Artist earth relief",2D)="bump"{}
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
            TEXTURE2D(_BankMap); SAMPLER(sampler_BankMap);
            TEXTURE2D(_GrassNormal); SAMPLER(sampler_GrassNormal);
            TEXTURE2D(_EarthNormal); SAMPLER(sampler_EarthNormal);
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
                half3 surfaceNormal=half3(0,0,1);
                // Fine marks fade before becoming pixel noise at the valley camera.
                if (_SurfaceKind > .5 && _SurfaceKind < 1.5) {
                    float paintedValue=dot(SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,p/24).rgb,half3(.2126,.7152,.0722));
                    albedo*=clamp(paintedValue/.18,.78,1.22);
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
                    // The blurred source mask supplies organic contours, not broad colour crossfades.
                    // Sharpen competing classes into readable material masses without altering source cover queries.
                    cover=pow(max(cover,.0001),6);
                    cover/=dot(cover,half4(1,1,1,1));
                    float patches=Noise(p*.025+offset)*.6+Noise(p*.10)*.4;
                    half3 woodland=lerp(half3(.22,.35,.12),half3(.37,.47,.18),patches);
                    half3 meadow=lerp(half3(.40,.57,.19),half3(.61,.68,.30),patches);
                    half3 field=lerp(half3(.61,.53,.28),half3(.76,.65,.36),patches);
                    float row=(.5+.5*sin(p.x*2.617994));
                    float footprint=max(length(ddx(p)),length(ddy(p)));
                    float rowFade=1-saturate(footprint*.45);
                    float farRows=(.5+.5*sin(p.x*.654499));
                    float farField=smoothstep(.6,1.8,footprint);
                    // Wider grouped rows carry the field at overview scale as subpixel stalks fade away.
                    field*=1-.06*pow(row,3)*rowFade+farField*(.018-.035*farRows);
                    // Coarse mapped sealed pixels are context; the separate mapped route defines its visible surface.
                    half3 verge=lerp(meadow,half3(.62,.58,.43),.35+patches*.15);
                    albedo=woodland*cover.r+field*cover.g+meadow*cover.b+verge*cover.a;
                    float bankDistance=SAMPLE_TEXTURE2D(_RiverMap,sampler_RiverMap,(p-_LandBounds.xy)/_LandBounds.zw).r;
                    // A distinct dark earth face meets a narrow grassy lip; avoid broad pale outlined banks.
                    float bank=1-smoothstep(.30,.38,bankDistance+Noise(p*.65)*.018);
                    half3 bankColor=lerp(half3(.27,.30,.22),half3(.52,.46,.30),smoothstep(.22,.35,bankDistance));
                    float bankGrowth=Noise(p*.13)*.6+Noise(p*.43)*.4;
                    float livingBank=smoothstep(.29,.37,bankDistance)*(1-smoothstep(.55+bankGrowth*.12,.72+bankGrowth*.12,bankDistance));
                    half3 bankGrass=lerp(half3(.29,.48,.22),half3(.46,.61,.30),bankGrowth);
                    albedo=lerp(albedo,bankGrass,livingBank*.85);
                    albedo=lerp(albedo,bankColor,bank);
                    albedo=SRGBToLinear(albedo);
                    half3 painted=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,p/48).rgb;
                    float paintValue=dot(painted,half3(.2126,.7152,.0722));
                    albedo*=lerp(clamp(paintValue/.18,.65,1.35),1,saturate(cover.g*.75+cover.a*.25+bank)*(1-livingBank));
                    float earth=dot(SAMPLE_TEXTURE2D(_BankMap,sampler_BankMap,p/lerp(30,9,bank)).rgb,half3(.2126,.7152,.0722));
                    float earthWeight=saturate(bank+cover.g*.55*(1-livingBank));
                    albedo*=lerp(1,clamp(earth/.16,.65,1.4),earthWeight);
                    half3 grassRelief=UnpackNormal(SAMPLE_TEXTURE2D(_GrassNormal,sampler_GrassNormal,p/48));
                    half3 earthRelief=UnpackNormal(SAMPLE_TEXTURE2D(_EarthNormal,sampler_EarthNormal,p/lerp(30,9,bank)));
                    surfaceNormal=normalize(lerp(half3(0,0,1),lerp(grassRelief,earthRelief,earthWeight),.55*fade));
                }
                half3 normal=normalize(input.normalWS);
                half3 tangent=normalize(half3(normal.y,-normal.x,0));
                half3 bitangent=cross(tangent,normal);
                normal=normalize(tangent*surfaceNormal.x+bitangent*surfaceNormal.y+normal*surfaceNormal.z);
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
