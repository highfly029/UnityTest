/**
 * Created by Chee <dev_chee@outlook.com>
 */

#ifndef BP3_OBJECT_CG_INCLUDED
#define BP3_OBJECT_CG_INCLUDED

#ifdef _NORMALMAP
#define BP3_HAS_NORMALMAP
#endif

#ifndef _SPECULARHIGHLIGHTS_OFF
#define BP3_HAS_SPECULAR
#endif

#ifdef _DOUBLE_SIDED_LIGHTING
#define BP3_DOUBLE_SIDED_LIGHTING
#endif

#include "BP3CG.cginc"
#include "FixShader.cginc"

#if defined(_ALPHATEST_ON) || defined(_ALPHABLEND_ON)
sampler2D _AlphaTex;
#endif
#ifdef _ALPHATEST_ON
fixed _Cutoff;
#endif
fixed4 _Color;
sampler2D _MainTex;
float4 _MainTex_ST;
#ifdef BP3_HAS_SPECULAR
//fixed3 _SpecColor;
float _SpecShine;
fixed _SpecGlossness;
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
    float2 texcoord : TEXCOORD0; // _MainTex
    BP3_DECLARE_TANGENT_AND_POS(2);
    BP3_DECLARE_LIGHTMAP(5)
    UNITY_SHADOW_COORDS(6)
    UNITY_FOG_COORDS(7)
    BP3_DECLARE_SH(0)
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

FwdBaseFragInput FwdBaseVert(FwdBaseAppInput v) {
    FwdBaseFragInput o;
    UNITY_INITIALIZE_OUTPUT(FwdBaseFragInput, o);
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(o, v);

    o.pos = UnityObjectToClipPos(v.vertex);
    o.texcoord.xy = TRANSFORM_TEX(v.texcoord.xy, _MainTex);
    BP3_TRANSFER_TANGENT_AND_POS(v, o, wPos, wNormal);
    BP3_TRANSFER_INDIRECT(v, o, wPos, wNormal);
    UNITY_TRANSFER_SHADOW(o, v.texcoord1.xy);
    UNITY_TRANSFER_FOG(o, o.pos);
    return o;
}

fixed4 FwdBaseFrag(FwdBaseFragInput i) : SV_Target {
    UNITY_APPLY_DITHER_CROSSFADE(i.pos.xy);
    UNITY_SETUP_INSTANCE_ID(i);

#if defined(_ALPHATEST_ON) || defined(_ALPHABLEND_ON)
    fixed alpha = tex2D(_AlphaTex, i.texcoord.xy).r * _Color.a;
#ifdef _ALPHATEST_ON
    clip(alpha - _Cutoff);
#endif
#else
	fixed alpha = 1;
#endif

	fixed3 albedo = tex2D(_MainTex, i.texcoord.xy) * _Color.rgb;
	fixed3 col = 0;

    BP3_APPLY_TANGENT_AND_POS(i, wPos, wNormal);
    BP3_APPLY_INDIRECT(i, wPos, wNormal, gi);

//#ifndef LIGHTMAP_ON
    BP3_LIGHT_DIFFUSE(diffCol, gi.light, wNormal, albedo);
    col += diffCol;
//#endif
#ifdef BP3_HAS_SPECULAR
    half3 wViewDir = normalize(UnityWorldSpaceViewDir(wPos));
    BP3_LIGHT_SPECULAR(specCol, gi.light, wNormal, wViewDir, _SpecColor, _SpecShine, _SpecGlossness);
    col += specCol;
#endif

    BP3_COLOR_MERGE(col, albedo, gi.indirect.diffuse);

    fixed4 c = fixed4(col, alpha);
    UNITY_APPLY_FOG(i.fogCoord, c);
    return c;
}

#endif /* BP3_OBJECT_CG_INCLUDED */
