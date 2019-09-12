/* vim:ts=4:sw=4:
 * Created by Chee <dev_chee@outlook.com>
 */

#ifndef BP3_WATER_CG_INCLUDED
#define BP3_WATER_CG_INCLUDED

#include "UnityCG.cginc"

#ifdef BP3_WATER_REALTIME_REFLECTIVE_ON
#define BP3_HAS_WATER_REFLECTIVE
#define BP3_HAS_WATER_REALTIME_REFLECTIVE
#elif defined(BP3_WATER_REFLECTIVE_ON)
#define BP3_HAS_WATER_REFLECTIVE
#endif

half _WaveScale;
half4 _WaveSpeed;
sampler2D _ReflectiveTex;
half4 _ReflectiveTex_ST;
half4 _ReflectiveColor;
sampler2D _BumpMap;
half _BumpScale;
#ifdef BP3_HAS_WATER_REFLECTIVE
sampler2D _ReflectionTex;
half _ReflectionScale;
#endif

struct VertexInput {
    float4 vertex : POSITION;
    half4 color : COLOR;
#if defined(BP3_HAS_WATER_REFLECTIVE) && !defined(BP3_HAS_WATER_REALTIME_REFLECTIVE)
    float4 texcoord : TEXCOORD0;
#endif
};

struct VertexOutput {
    UNITY_POSITION(pos);
    half4 color : COLOR;
    float4 texcoord : TEXCOORD0;
    float3 viewDir : TEXCOORD1;
#ifdef BP3_HAS_WATER_REFLECTIVE
    float4 texcoord1 : TEXCOORD2;
#endif
    UNITY_FOG_COORDS(3)
};

VertexOutput WaterVert(VertexInput v) {
    VertexOutput o;
    UNITY_INITIALIZE_OUTPUT(VertexOutput, o);

    o.pos = UnityObjectToClipPos(v.vertex);
    o.color = v.color;
    float4 wPos = mul(unity_ObjectToWorld, v.vertex);
    float4 scale = float4(_WaveScale, _WaveScale, _WaveScale * 0.4, _WaveScale * 0.45);
    float4 offset = frac(scale * _WaveSpeed * _Time.x);
    o.texcoord.yxzw = wPos.xzxz * scale + offset;
    o.viewDir.xzy = WorldSpaceViewDir(v.vertex); // normalize in fragment
    //o.texcoord.xyzw = wPos.xzxz * scale + offset;
    //o.viewDir.xyz = WorldSpaceViewDir(v.vertex); // normalize in fragment
#ifdef BP3_HAS_WATER_REALTIME_REFLECTIVE
    o.texcoord1 = ComputeScreenPos(o.pos);
#elif defined(BP3_HAS_WATER_REFLECTIVE)
    o.texcoord1 = v.texcoord;
#endif

    UNITY_TRANSFER_FOG(o, o.pos);
    return o;
}

half4 WaterFrag(VertexOutput i) : SV_Target {
    float3 viewDir = normalize(i.viewDir);
    half3 bump1 = UnpackNormal(tex2D(_BumpMap, i.texcoord.xy));
    half3 bump2 = UnpackNormal(tex2D(_BumpMap, i.texcoord.zw));

#ifdef BP3_HAS_WATER_REALTIME_REFLECTIVE
    half3 bump = (bump1 + bump2) * 0.5;
#else
    half3 bump = (bump1 + bump2) * _BumpScale;
#endif
    float term = dot(viewDir, bump);
    float2 uv = float2(term, term) * _ReflectiveTex_ST.xy + _ReflectiveTex_ST.zw;
    half4 water = half4(UNITY_SAMPLE_1CHANNEL(_ReflectiveTex, uv) * _ReflectiveColor.rgb, _ReflectiveColor.a);

    half4 color;
#ifdef BP3_HAS_WATER_REALTIME_REFLECTIVE
    float4 uv1 = i.texcoord1;
    uv1.xy += bump.xy * _BumpScale/*ReflectionDistort*/;
    half4 refl = tex2Dproj(_ReflectionTex, UNITY_PROJ_COORD(uv1));

    color.rgb = lerp(water.rgb, refl.rgb, _ReflectionScale);
    color.a = water.a * refl.a;
#elif defined (BP3_HAS_WATER_REFLECTIVE)
    float2 uv1 = i.texcoord1.xy;
    uv1 += bump.xy * 0.5;
    half3 refl = tex2D(_ReflectionTex, uv1);

    color.rgb = lerp(water.rgb, refl, _ReflectionScale);
    color.a = water.a;
#else
    color = water;
#endif
    color.a *= i.color.a;

    UNITY_APPLY_FOG(i.fogCoord, color);
    return color;
}

#endif /* BP3_WATER_CG_INCLUDED */
