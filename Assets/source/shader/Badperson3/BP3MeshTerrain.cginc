/*
 * Created by Chee <dev_chee@outlook.com>
 */

#ifndef BP3_MESH_TERRAIN_CG_INCLUDED
#define BP3_MESH_TERRAIN_CG_INCLUDED

#ifdef _SPLATMAP0
#define BP3_HAS_SPLATMAP0
#endif

#ifdef _SPLATMAP1
#define BP3_HAS_SPLATMAP1
#endif

#ifdef _SPLATMAP2
#define BP3_HAS_SPLATMAP2
#endif

#ifdef _SPLATMAP3
#define BP3_HAS_SPLATMAP3
#endif

#ifdef _NORMALMAP0
#define BP3_HAS_NORMALMAP0
#endif

#ifdef _NORMALMAP1
#define BP3_HAS_NORMALMAP1
#endif

#ifdef _NORMALMAP2
#define BP3_HAS_NORMALMAP2
#endif

#ifdef _NORMALMAP3
#define BP3_HAS_NORMALMAP3
#endif

#if defined(BP3_HAS_NORMALMAP0) || defined(BP3_HAS_NORMALMAP1) || defined(BP3_HAS_NORMALMAP2) || defined(BP3_HAS_NORMALMAP3)
#define BP3_HAS_NORMALMAP
#endif


#ifdef _SPECULAR
#define BP3_HAS_SPECULAR
#endif

#include "BP3CG.cginc"

sampler2D _Control;
#ifdef BP3_HAS_SPLATMAP0
sampler2D _Splat0;
float4 _Splat0_ST;
#endif
#ifdef BP3_HAS_SPLATMAP1
sampler2D _Splat1;
float4 _Splat1_ST;
#endif
#ifdef BP3_HAS_SPLATMAP2
sampler2D _Splat2;
float4 _Splat2_ST;
#endif
#ifdef BP3_HAS_SPLATMAP3
sampler2D _Splat3;
float4 _Splat3_ST;
#endif

#ifdef BP3_HAS_NORMALMAP0
sampler2D _BumpSplat0;
half _BumpScale0;
#endif
#ifdef BP3_HAS_NORMALMAP1
sampler2D _BumpSplat1;
half _BumpScale1;
#endif
#ifdef BP3_HAS_NORMALMAP2
sampler2D _BumpSplat2;
half _BumpScale2;
#endif
#ifdef BP3_HAS_NORMALMAP3
sampler2D _BumpSplat3;
half _BumpScale3;
#endif

#ifdef BP3_HAS_SPECULAR
half _SpecPower;
half _ShininessL0;
half _ShininessL1;
half _ShininessL2;
half _ShininessL3;
#endif

struct FwdBaseAppInput {
    float4 vertex : POSITION;
    float3 normal : NORMAL;
#ifdef BP3_HAS_NORMALMAP
    float4 tangent : TANGENT;
#endif
    float2 texcoord : TEXCOORD0; // MainTex
    float2 texcoord1 : TEXCOORD1; // LightMap & Shadow
#ifdef DYNAMICLIGHTMAP_ON
    float2 texcoord2 : TEXCOORD2; // Dynamic LightMap
#endif
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct FwdBaseFragInput {
    UNITY_POSITION(pos);
    float2 texcoord : TEXCOORD0; // _Control
    float4 texcoord1 : TEXCOORD1; // _Splat0, _Splat1
    float4 texcoord2 : TEXCOORD2; // _Splat2, _Splat3
    BP3_DECLARE_TANGENT_AND_POS(3);
    BP3_DECLARE_LIGHTMAP(6)
    UNITY_SHADOW_COORDS(7)
    UNITY_FOG_COORDS(8)
    BP3_DECLARE_SH(0)
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

FwdBaseFragInput FwdBaseVert(FwdBaseAppInput v) {
    FwdBaseFragInput o;
    UNITY_INITIALIZE_OUTPUT(FwdBaseFragInput, o);
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(o, v);

    o.pos = UnityObjectToClipPos(v.vertex);
    o.texcoord = v.texcoord;
#ifdef BP3_HAS_SPLATMAP0
    o.texcoord1.xy = TRANSFORM_TEX(v.texcoord, _Splat0);
#endif
#ifdef BP3_HAS_SPLATMAP1
    o.texcoord1.zw = TRANSFORM_TEX(v.texcoord, _Splat1);
#endif
#ifdef BP3_HAS_SPLATMAP2
    o.texcoord2.xy = TRANSFORM_TEX(v.texcoord, _Splat2);
#endif
#ifdef BP3_HAS_SPLATMAP3
    o.texcoord2.zw = TRANSFORM_TEX(v.texcoord, _Splat3);
#endif

    BP3_TRANSFER_TANGENT_AND_POS(v, o, wPos, wNormal);
    BP3_TRANSFER_INDIRECT(v, o, wPos, wNormal);
    UNITY_TRANSFER_SHADOW(o, v.texcoord1.xy);
    UNITY_TRANSFER_FOG(o, o.pos);
    return o;
}

fixed4 FwdBaseFrag(FwdBaseFragInput i) : SV_Target {
    UNITY_APPLY_DITHER_CROSSFADE(i.pos.xy);
    UNITY_SETUP_INSTANCE_ID(i);

    fixed4 splatCtrl = tex2D(_Control, i.texcoord.xy);

#ifdef BP3_HAS_NORMALMAP
    //half3 vNormal = half3(i.wTangentAndPos[0].z, i.wTangentAndPos[1].z, i.wTangentAndPos[2].z);
    //half3 wNormal = half3(0, 0, 0);
    half3 wNormal = half3(i.wTangentAndPos[0].z, i.wTangentAndPos[1].z, i.wTangentAndPos[2].z);

//#ifdef BP3_HAS_NORMALMAP0
//    wNormal += UnpackScaleNormal(tex2D(_BumpSplat0, i.texcoord1.xy), _BumpScale0) * splatCtrl.r;
//    normalize(half3(n1.xy + n2.xy, n1.z*n2.z))
//#else
//    wNormal += vNormal;
//#endif
//
//#ifdef BP3_HAS_NORMALMAP1
//    wNormal += UnpackScaleNormal(tex2D(_BumpSplat1, i.texcoord1.zw), _BumpScale1) * splatCtrl.g;
//#else
//    wNormal += vNormal;
//#endif
//
//#ifdef BP3_HAS_NORMALMAP2
//    wNormal += UnpackScaleNormal(tex2D(_BumpSplat2, i.texcoord2.xy), _BumpScale2) * splatCtrl.b;
//#else
//    wNormal += vNormal;
//#endif
//
//#ifdef BP3_HAS_NORMALMAP3
//    wNormal += UnpackScaleNormal(tex2D(_BumpSplat3, i.texcoord2.zw), _BumpScale3) * splatCtrl.a;
//#else
//    wNormal += vNormal;
//#endif

    float3 wPos = float3(i.wTangentAndPos[0].w, i.wTangentAndPos[1].w, i.wTangentAndPos[2].w);
    //wNormal = UnpackScaleNormal(tex2D(_BumpSplat1, splatCoord1), _BumpScale1) * splatCtrl.g;
    wNormal = normalize(half3(dot(i.wTangentAndPos[0].xyz, wNormal), dot(i.wTangentAndPos[1].xyz, wNormal), dot(i.wTangentAndPos[2].xyz, wNormal)));
#else
    float3 wPos = i.wPos;
    half3 wNormal = normalize(i.wNormal);
#endif

    BP3_APPLY_INDIRECT(i, wPos, wNormal, gi);

    fixed3 albedo = 0;
#ifdef BP3_HAS_SPLATMAP0
    albedo += tex2D(_Splat0, i.texcoord1.xy) * splatCtrl.r;
#endif
#ifdef BP3_HAS_SPLATMAP1
    albedo += tex2D(_Splat1, i.texcoord1.zw) * splatCtrl.g;
#endif
#ifdef BP3_HAS_SPLATMAP2
    albedo += tex2D(_Splat2, i.texcoord2.xy) * splatCtrl.b;
#endif
#ifdef BP3_HAS_SPLATMAP3
    albedo += tex2D(_Splat3, i.texcoord2.zw) * splatCtrl.a;
#endif

    fixed3 col = 0;
//#ifndef LIGHTMAP_ON
    //fixed3 diffCol = diffuse * light.color * max(0, dot(wNormal, light.dir)) * 2;
    BP3_LIGHT_DIFFUSE(diffCol, gi.light, wNormal, albedo);
    col += diffCol;
//#endif
#ifdef BP3_HAS_SPECULAR
    half3 wViewDir = normalize(UnityWorldSpaceViewDir(wPos));
    half3 shiness = (_ShininessL0 * splatCtrl.r + _ShininessL1 * splatCtrl.g + _ShininessL2 * splatCtrl.b + _ShininessL3 * splatCtrl.a);
    BP3_LIGHT_SPECULAR(specCol, gi.light, wNormal, wViewDir, _SpecColor, _SpecPower, shiness);
    col += specCol;
#endif

    BP3_COLOR_MERGE(col, albedo, gi.indirect.diffuse);

    fixed4 c = fixed4(col, 1);
    UNITY_APPLY_FOG(i.fogCoord, c);
    return c;
}

#endif /* BP3_MESH_TERRAIN_CG_INCLUDED */
