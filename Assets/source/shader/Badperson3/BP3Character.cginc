/**
 * Created by Chee <dev_chee@outlook.com>
 */

#ifndef BP3_CHARACTER_CG_INCLUDED
#define BP3_CHARACTER_CG_INCLUDED

#define BP3_HAS_CHARACTER 1 // Required

#ifdef _BP3_CHARACTER_HAIR
#define BP3_HAS_CHARACTER_HAIR
#endif

#ifdef _NORMALMAP
#define BP3_HAS_NORMALMAP
#endif

#ifdef _SPECULAR
#define BP3_HAS_SPECULAR
#endif

#ifdef _CUBEMAP
#define BP3_HAS_CUBEMAP
#endif

#include "BP3CG.cginc"
#include "FixShader.cginc"

#ifdef BP3_HAS_CHARACTER_HAIR
#endif
sampler2D _AlphaTex;
fixed4 _Color;
sampler2D _MainTex;
float4 _MainTex_ST;
half _GlossScale;
sampler2D _MaskTex;
#ifdef BP3_HAS_CUBEMAP
half _ReflecRadio;
samplerCUBE _CubeMap;
float _CubeMapRotation;
#endif
float _SpecPower;
fixed4 _RimColor;
sampler2D _RimMap;
half _RimRange;
float _RimPower;
fixed4 _HitGlowColor;
float _HitGlowPower;

float4 _MaskRect;

struct FwdBaseAppInput {
    float4 vertex : POSITION;
    float3 normal : NORMAL;
#ifdef BP3_HAS_NORMALMAP
    float4 tangent : TANGENT;
#endif
    float2 texcoord : TEXCOORD0; // MainTex
    float2 texcoord1 : TEXCOORD1; // Shadows
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct FwdBaseFragInput {
    UNITY_POSITION(pos);
    float4 texcoord  : TEXCOORD0;
	//float3 vpos : NORMAL0; 

    BP3_DECLARE_TANGENT_AND_POS(1);
    UNITY_SHADOW_COORDS(4)
    UNITY_FOG_COORDS(5)
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

FwdBaseFragInput FwdBaseVert(FwdBaseAppInput v) {
    FwdBaseFragInput o;
    UNITY_INITIALIZE_OUTPUT(FwdBaseFragInput, o);
//	o.vpos =v.vertex.xyz;//--
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, o);

    o.pos = UnityObjectToClipPos(v.vertex);
    o.texcoord.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
    BP3_TRANSFER_TANGENT_AND_POS(v, o, wPos, wNormal);
    UNITY_TRANSFER_SHADOW(o, v.texcoord1.xy);
    UNITY_TRANSFER_FOG(o, o.pos);
    return o;
}

half4 FwdBaseFrag(FwdBaseFragInput i) : SV_Target {
    UNITY_APPLY_DITHER_CROSSFADE(i.pos.xy);
    UNITY_SETUP_INSTANCE_ID(i);
    BP3_APPLY_TANGENT_AND_POS(i, wPos, wNormal);


    fixed3 col = 0;
#if defined(BP3_HAS_CHARACTER_HAIR) ||defined(_ALPHABLEND_ON) 
    fixed alpha = tex2D(_AlphaTex, i.texcoord.xy).r * _Color.a;
#else
	fixed alpha =1;
#endif

	

#if defined(BP3_HAS_CUBEMAP) || defined(BP3_HAS_SPECULAR) || defined(_RIM_LIGHT) || defined(_HIT_GLOW)
    half3 wViewDir = normalize(UnityWorldSpaceViewDir(wPos));
#endif

#if defined(BP3_HAS_CUBEMAP) || defined(BP3_HAS_SPECULAR)
    fixed2 mask = tex2D(_MaskTex, i.texcoord.xy).rg;
#endif

    fixed3 albedo = tex2D(_MainTex, i.texcoord.xy) * _Color.rgb;

    UNITY_LIGHT_ATTENUATION(atten, i, wPos);
    half3 wLightColor = _LightColor0.rgb * atten;
	half3 wLightDir = normalize(UnityWorldSpaceLightDir(wPos));

    col += UNITY_LIGHTMODEL_AMBIENT * albedo;

    //col.rgb += wLightColor * albedo * (dot(wNormal, wLightDir) * 0.5 + 0.5);
    col += wLightColor * albedo * ((dot(wNormal, wLightDir) + 1) * 0.5);
    //col.rgb += wLightColor * albedo;

#ifdef BP3_HAS_SPECULAR
    float nh = max(0, dot(wNormal, normalize(wLightDir + wViewDir)));
    float spec = pow(nh, _SpecPower * 128) * mask.r * _GlossScale;
    col += wLightColor * _SpecColor.rgb * spec;
#endif

#ifdef BP3_HAS_CUBEMAP
    //half3 wReflDir = reflect(-wViewDir, wNormal);
    half3 wReflDir = reflect(wViewDir, wNormal);
    wReflDir = BP3RotateAroundYInDegrees(wReflDir, _CubeMapRotation);
    col += texCUBE(_CubeMap, wReflDir) * mask.g * _ReflecRadio;
    //col = col * (1 - mask.g) + lerp(col, texCUBE(_CubeMap, wReflDir), _ReflecRadio) * mask.g;
#endif

#if defined(_RIM_LIGHT) || defined(_HIT_GLOW)
    half vn = dot(wViewDir, wNormal);
#endif

#ifdef _RIM_LIGHT
    float2 uv = float2(vn, 0.5);
    uv.x *= _RimRange;
    //float rim = pow(tex2D(_RimMap, uv), _RimPower);
    half rim = saturate(pow(tex2D(_RimMap, uv), _RimPower));
    col += _RimColor * rim;
#endif

#ifdef _HIT_GLOW
    half hit = 1 - saturate(pow(vn, _HitGlowPower));
    col += _HitGlowColor * hit;
#endif

    fixed4 c = fixed4(col, alpha);
    UNITY_APPLY_FOG(i.fogCoord, c);

#ifdef _HAS_MASK // узуж
	if ((wPos.x < _MaskRect.x) || (wPos.y < _MaskRect.y)|| (wPos.x > _MaskRect.z) || (wPos.y > _MaskRect.w))
	{
		 c.a = 0;
	}
#endif
    return c;
}

#if BP3_HAS_XRAY
struct XRayAppInput {
    float4 vertex : POSITION;
    half3 normal : NORMAL;
};

struct XRayFragInput {
    float4 pos : POSITION;
    float2 uv : TEXCOORD0;
};

XRayFragInput VertXRay(XRayAppInput v) {
    XRayFragInput o;
    UNITY_INITIALIZE_OUTPUT(XRayFragInput, o);

    o.pos = UnityObjectToClipPos(v.vertex);
    half3 wNormal = UnityObjectToWorldNormal(v.normal);
    half3 wPos = mul((float3x3)unity_ObjectToWorld, v.vertex.xyz);
    half3 wViewDir = normalize(UnityWorldSpaceViewDir(wPos));
    half3 r = reflect(wViewDir, wNormal);
    o.uv = float2(r.x * 0.5 + 0.5, r.y * 0.5 + 0.5);
    return o;
}

half4 FragXRay(XRayFragInput i) : SV_Target {
    half4 color = tex2D(_XrayTex, i.uv);
    return color;
}
#endif

#endif /* BP3_CHARACTER_CG_INCLUDED */
