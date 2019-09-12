/**
 * Created by Chee <dev_chee@outlook.com>
 */

#ifndef BP3_CHARACTER_COMMON_CGINC
#define BP3_CHARACTER_COMMON_CGINC

#include "BP3CG.cginc"
#include "FixShader.cginc"

//#define BP3_ENABLE_FOG
//#define BP3_ENABLE_AMBIENT

half4 _Color;
sampler2D _MainTex;
float4 _MainTex_ST;
sampler2D _AlphaTex;
half _Cutoff;
sampler2D _MaskTex;
sampler2D _NormalTex;
samplerCUBE _CubeTex;
half _ReflectScale;
half _Indensity;
//half4 _SpecColor;
half _SpecPower;
half _SpecScale;
half4 _RimColor;
sampler2D _RimTex;
half _RimPower;
half _RimRange;
half4 _HitGlowColor;
half _HitGlowPower;
sampler2D _XrayTex;

inline half3 NormalizePerVertex(float3 n) { // takes float to avoid overflow
#if (SHADER_TARGET < 30)
    return normalize(n);
#else
    return n; // will normalize per-pixel instead
#endif
}

inline half3 NormalizePerPixel(half3 n) {
#if (SHADER_TARGET < 30)
    return n;
#else
    return normalize(n);
#endif
}

half3 UnpackPackedNormal(half4 packednormal) {
    half3 normal;
#if defined(UNITY_NO_DXT5nm)
    normal.xy = packednormal.xy * 2 - 1;
    normal.z = sqrt(1.0 - saturate(dot(normal.xy, normal.xy)));
    return normal;
#else
    normal.xy = (packednormal.wy * 2 - 1);
    normal.z = sqrt(1.0 - saturate(dot(normal.xy, normal.xy)));
    return normal;
#endif
}

inline half3 PerPixelWorldNormal(half4 tNormalPacked, half4 wTangent[3]) {
#ifdef _NORMALMAP
    half3 tangent = wTangent[0].xyz;
    half3 binormal = wTangent[1].xyz;
    half3 normal = wTangent[2].xyz;

#if UNITY_TANGENT_ORTHONORMALIZE
    normal = NormalizePerPixel(normal);
    // ortho-normalize Tangent
    tangent = normalize(tangent - normal * dot(tangent, normal));
    // recalculate Binormal
    half3 newB = cross(normal, tangent);
    binormal = newB * sign(dot(newB, binormal));
#endif

    half3 tNormal = UnpackPackedNormal(tNormalPacked);
    // @TODO: see if we can squeeze this normalize on SM2.0 as well
    half3 wNormal = NormalizePerPixel(tangent * tNormal.x + binormal * tNormal.y + normal * tNormal.z);
#else
    half3 wNormal = normalize(wTangent[2].xyz);
#endif
    return wNormal;
}

/**********************************************************************
 * Forward Base
 */

struct FwdVIn {
    float4 vertex : POSITION;
    half3 normal : NORMAL;
#ifdef _NORMALMAP
    half4 tangent : TANGENT;
#endif
    float2 texcoord : TEXCOORD0;
    float2 texcoord1 : TEXCOORD1;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct FwdVOut {
    UNITY_POSITION(pos);
    float4 texcoord  : TEXCOORD0;
    half3 wViewVec : TEXCOORD1;
    half4 wTangentAndPos[3] : TEXCOORD2; // [3x3:Tangent | 1x3:Position]
#ifdef BP3_ENABLE_AMBIENT
    half4 ambient : TEXCOORD5; // SH & Point Lights
#endif
    UNITY_SHADOW_COORDS(6)
#ifndef BP3_ENABLE_FOG
        UNITY_FOG_COORDS(7)
#endif
        UNITY_VERTEX_INPUT_INSTANCE_ID
        UNITY_VERTEX_OUTPUT_STEREO
};

FwdVOut VertForward(FwdVIn v) {
    UNITY_SETUP_INSTANCE_ID(v);

    FwdVOut o;
    UNITY_INITIALIZE_OUTPUT(FwdVOut, o);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    o.pos = UnityObjectToClipPos(v.vertex);
    o.texcoord.xy = TRANSFORM_TEX(v.texcoord, _MainTex);

    half4 wPos = mul(unity_ObjectToWorld, v.vertex);
    o.wTangentAndPos[0].w = wPos.x;
    o.wTangentAndPos[1].w = wPos.y;
    o.wTangentAndPos[2].w = wPos.z;

    o.wViewVec = NormalizePerVertex(wPos.xyz - _WorldSpaceCameraPos);
    half3 wNormal = UnityObjectToWorldNormal(v.normal);

#ifdef _NORMALMAP
    half4 wTangent = half4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);
    half sign = wTangent.w * unity_WorldTransformParams.w;
    half3 wBinormal = cross(wNormal, wTangent) * sign;
    o.wTangentAndPos[0].xyz = wTangent;
    o.wTangentAndPos[1].xyz = wBinormal;
    o.wTangentAndPos[2].xyz = wNormal;
#else
    o.wTangentAndPos[0].xyz = 0;
    o.wTangentAndPos[1].xyz = 0;
    o.wTangentAndPos[2].xyz = wNormal;
#endif

    UNITY_TRANSFER_SHADOW(o, v.texcoord1);

#ifdef BP3_ENABLE_AMBIENT
    o.ambient.rgb = ShadeSHPerVertex(wNormal, o.ambient);
#endif

#ifdef BP3_ENABLE_FOG
    UNITY_TRANSFER_FOG(o, o.pos);
#endif

    return o;
}
            
half4 FragForward(FwdVOut i) : SV_Target {
    UNITY_APPLY_DITHER_CROSSFADE(i.pos.xy);

    half alpha = _Color.a;

#if defined(_ALPHATEST_ON) || defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON)
    alpha *= tex2D(_AlphaTex, i.texcoord.xy).r;
#ifdef _ALPHATEST_ON
    clip(alpha - _Cutoff);
#endif
#endif

    half3 diffuse = _Color.rgb * tex2D(_MainTex, i.texcoord.xy).rgb;
    half2 mask = tex2D(_MaskTex, i.texcoord.xy).rg;
    half3 wPos = half3(i.wTangentAndPos[0].w, i.wTangentAndPos[1].w, i.wTangentAndPos[2].w);
    half3 wViewDir = NormalizePerPixel(i.wViewVec);
    half4 tNormalPacked = tex2D(_NormalTex, i.texcoord.xy);
    half3 wNormal = PerPixelWorldNormal(tNormalPacked, i.wTangentAndPos);

    // @FIXME: shader relies on pre-multiply alpha-blend (_SrcBlend = One, _DstBlend = OneMinusSrcAlpha)
    diffuse = PreMultiplyAlpha(diffuse, alpha, 0.9/*oneMinusReflectivity*/, /*out*/ alpha);

    UNITY_SETUP_INSTANCE_ID(i);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

    // Global Illumination
    UNITY_LIGHT_ATTENUATION(atten, i, wPos);

    UnityGI gi;
    UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
    gi.indirect.diffuse = 0;
    gi.indirect.specular = 0;
    gi.light.color = _LightColor0.rgb * atten;
    gi.light.dir = _WorldSpaceLightPos0.xyz;

#ifdef BP3_ENABLE_AMBIENT
    i.ambient = ShadeSHPerPixel(wNormal, i.ambient, wPos);
#endif

    wViewDir = -wViewDir;
    half4 c;

    // Local Illumination - Bilnn-Phong BRDF
    //half d = max(0, dot(wNormal, gi.light.dir));
    half3 h = normalize(gi.light.dir + wViewDir);
    half nh = max(0, dot(wNormal, h));
    float s = pow(nh, _SpecPower * 64.0) * _SpecScale;
    //c.rgb = diffuse * gi.light.color * d + gi.light.color * _SpecColor.rgb * s;
    c.rgb = gi.light.color * _Indensity * diffuse + gi.light.color * _Indensity * _SpecColor.rgb * s * mask.r;

#ifdef _RIM_LIGHT
    float2 uv = float2(dot(wViewDir, wNormal), 0.5);
    uv.x *= _RimRange;
    half rim = saturate(pow(tex2D(_RimTex, uv), _RimPower));
    c.rgb += _RimColor * rim;
#endif

#ifdef _REFLECT_CUBEMAP
    half3 worldRefl = reflect(wViewDir, wNormal);
    c.rgb += texCUBE(_CubeTex, worldRefl).rgb * _ReflectScale * mask.g;
#endif

#ifdef BP3_ENABLE_AMBIENT
    c.rgb += diffuse.rgb * gi.indirect.diffuse;
#endif

#ifdef _HIT_GLOW
    half hit = 1 - saturate(pow(dot(wViewDir, wNormal), _HitGlowPower));
    c.rgb += _HitGlowColor * hit;
#endif

#ifdef BP3_ENABLE_FOG
    UNITY_APPLY_FOG(i.fogCoord, c.rgb);
#endif 

#if defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON)
    c.a = alpha;
#else
    UNITY_OPAQUE_ALPHA(c.a);
#endif

    return c;
}

/**********************************************************************
 * X-Ray Effect
 */

struct XRayVIn {
    float4 vertex : POSITION;
    half3 normal : NORMAL;
};

struct XRayVOut {
    float4 pos : POSITION;
    float2 uv : TEXCOORD0;
};

XRayVOut VertXRay(XRayVIn v) {
    XRayVOut o;

    o.pos = UnityObjectToClipPos(v.vertex);
#ifdef UNITY_ASSUME_UNIFORM_SCALING
    half3 wNormal = normalize(mul((float3x3)unity_ObjectToWorld, v.normal));
#else
    half3 wNormal = normalize(mul(v.normal, (float3x3)unity_WorldToObject));
#endif
    half3 wPos = mul((float3x3)unity_ObjectToWorld, v.vertex.xyz);
    half3 wViewDir = normalize(wPos - _WorldSpaceCameraPos);
    half3 r = reflect(wViewDir, wNormal);
    o.uv = float2(r.x * 0.5 + 0.5, r.y * 0.5 + 0.5);

    return o;
}

half4 FragXRay(XRayVOut i) : SV_Target {
    half4 color = tex2D(_XrayTex, i.uv);
    return color;
}


#endif /* BP3_CHARACTER_COMMON_CGINC */