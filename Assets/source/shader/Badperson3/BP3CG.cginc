/* Created by Chee <dev_chee@outlook.com>
 */

#ifndef BP3_CG_INCLUDED
#define BP3_CG_INCLUDED

#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "AutoLight.cginc"

#define BP3_PI_DIV_180 0.0174532925199444

#ifdef BP3_HAS_NORMALMAP
#   define BP3_HAS_TANGENT_AND_POS
#endif

//#if defined(LIGHTMAP_ON) && defined(DIRLIGHTMAP_COMBINED)
//#   define BP3_HAS_TANGENT_AND_POS
//#endif

#if (UNITY_SHOULD_SAMPLE_SH) && defined(BP3_HAS_CHARACTER)
#   ifdef UNITY_SHOULD_SAMPLE_SH
#       undef UNITY_SHOULD_SAMPLE_SH
#   endif
#   define UNITY_SHOULD_SAMPLE_SH 0
#endif

//#define _BP3_TEXCOORD(i) TEXCOORD##i
//#define _BP3_IDX_ADD(x,y) (x+y)
//#define BP3_TEXCOORD(i,x) _BP3_TEXCOORD(_BP3_IDX_ADD(i,x))

inline float3 BP3RotateAroundYInDegrees(float3 vertex, float degrees) {
    float alpha = degrees * BP3_PI_DIV_180;
    float sina, cosa;
    sincos(alpha, sina, cosa);
    float2x2 m = float2x2(cosa, -sina, sina, cosa);
    return float3(mul(m, vertex.xz), vertex.y).xzy;
}

/**********************************************************************
 * Tangent Space and World Position transforms
 */
#ifdef BP3_HAS_TANGENT_AND_POS
sampler2D _BumpMap;
half _BumpScale;

// [3x3:Tangent | 1x3:Position]
#   define BP3_DECLARE_TANGENT_AND_POS(idx) float4 wTangentAndPos[3] : TEXCOORD##idx

#   define BP3_TRANSFER_TANGENT_AND_POS(v, o, wPos, wNormal) \
        float3 wPos = mul(unity_ObjectToWorld, v.vertex); \
        half3 wNormal = UnityObjectToWorldNormal(v.normal); \
        half3 wTangent = UnityObjectToWorldDir(v.tangent.xyz); \
        half3 wBinormal = cross(wNormal, wTangent) * v.tangent.w * unity_WorldTransformParams.w; \
        o.wTangentAndPos[0] = float4(wTangent.x, wBinormal.x, wNormal.x, wPos.x); \
        o.wTangentAndPos[1] = float4(wTangent.y, wBinormal.y, wNormal.y, wPos.y); \
        o.wTangentAndPos[2] = float4(wTangent.z, wBinormal.z, wNormal.z, wPos.z)

#   define BP3_APPLY_TANGENT_AND_POS(i, wPos, wNormal) \
        float3 wPos = float3(i.wTangentAndPos[0].w, i.wTangentAndPos[1].w, i.wTangentAndPos[2].w); \
        half3 wNormal = UnpackScaleNormal(tex2D(_BumpMap, i.texcoord.xy), _BumpScale); \
        wNormal = normalize(half3(dot(i.wTangentAndPos[0].xyz, wNormal), dot(i.wTangentAndPos[1].xyz, wNormal), dot(i.wTangentAndPos[2].xyz, wNormal)))

#else
// [1x3:Position | 1x3:Normal]
#   define BP3_DECLARE_TANGENT_AND_POS(idx) float3 wPos : TEXCOORD##idx; half3 wNormal : NORMAL

#   define BP3_TRANSFER_TANGENT_AND_POS(v, o, wPos, wNormal) \
        float3 wPos = mul(unity_ObjectToWorld, v.vertex); o.wPos = wPos; \
        half3 wNormal = UnityObjectToWorldNormal(v.normal); o.wNormal = wNormal

#   define BP3_APPLY_TANGENT_AND_POS(i, wPos, wNormal) \
        float3 wPos = i.wPos; \
        half3 wNormal = normalize(i.wNormal)
#endif

/**********************************************************************
 * LightMap & SH/Ambient & VertexLight
 */

#if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
#   define BP3_DECLARE_LIGHTMAP(idx) float4 lightMapCoord : TEXCOORD##idx;
#else
#   define BP3_DECLARE_LIGHTMAP(idx)
#endif

#if UNITY_SHOULD_SAMPLE_SH
#   define BP3_DECLARE_SH(idx) half3 sh : COLOR##idx;
#else
#   define BP3_DECLARE_SH(idx)
#endif

inline float4 BP3TransLightMapCoord(float2 lm, float2 dlm) {
    float4 uv = 0;
#ifdef LIGHTMAP_ON
    uv.xy = lm * unity_LightmapST.xy + unity_LightmapST.zw;
#endif
#ifdef DYNAMICLIGHTMAP_ON
    uv.zw = dlm * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
#endif
    return uv;
}

#if UNITY_SHOULD_SAMPLE_SH
inline half3 BP3SHVertex(float3 wPos, half3 wNormal) {
    half3 sh = 0;
#ifdef VERTEXLIGHT_ON
    sh += Shade4PointLights(unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0, unity_LightColor[0].rgb,
            unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb, unity_4LightAtten0, wPos, wNormal);
#endif
    return ShadeSHPerVertex(wNormal, sh);
}
#endif

inline UnityGI BP3GIBase(float3 wPos, half3 wNormal, half atten, half3 sh, float4 lightMapCoord) {
    UnityLight li;
    UNITY_INITIALIZE_OUTPUT(UnityLight, li);
    li.dir = normalize(UnityWorldSpaceLightDir(wPos));
    li.color = _LightColor0.rgb;

    UnityGIInput giInput;
    UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
    giInput.light = li;
    giInput.worldPos = wPos;
    giInput.atten = atten;
#if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
    giInput.lightmapUV = lightMapCoord;
    //#else
    //giInput.lightmapUV = 0.0;
#endif
#if UNITY_SHOULD_SAMPLE_SH
    giInput.ambient = sh;
    //#else
    //giInput.ambient = 0.0;
#endif
    giInput.probeHDR[0] = unity_SpecCube0_HDR;
    giInput.probeHDR[1] = unity_SpecCube1_HDR;
#if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
    giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
#endif
#ifdef UNITY_SPECCUBE_BOX_PROJECTION
    giInput.boxMax[0] = unity_SpecCube0_BoxMax;
    giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
    giInput.boxMax[1] = unity_SpecCube1_BoxMax;
    giInput.boxMin[1] = unity_SpecCube1_BoxMin;
    giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
#endif

    return UnityGlobalIllumination(giInput, 1.0, wNormal);
}
UnityGI gi;
#define _BP3_GI_BASE(i, wPos, wNormal, sh, lightMapCoord, gi) UNITY_LIGHT_ATTENUATION(atten, i, wPos); gi = BP3GIBase(wPos, wNormal, atten, sh, lightMapCoord);

#ifdef LIGHTMAP_ON
// [xy: LightMap | zw: Dynamic LightMap]
#   ifdef DYNAMICLIGHTMAP_ON
#       define BP3_TRANSFER_INDIRECT(v, o, wPos, wNormal) o.lightMapCoord = BP3TransLightMapCoord(v.texcoord1.xy, v.texcoord2.xy);
#   else
#       define BP3_TRANSFER_INDIRECT(v, o, wPos, wNormal) o.lightMapCoord = BP3TransLightMapCoord(v.texcoord1.xy, float2(0,0));
#   endif
#   define BP3_APPLY_INDIRECT(i, wPos, wNormal, gi) _BP3_GI_BASE(i, wPos, wNormal, 0, i.lightMapCoord, gi)
#elif UNITY_SHOULD_SAMPLE_SH
// [SH & VertexLight]
#   ifdef DYNAMICLIGHTMAP_ON
#       define BP3_TRANSFER_INDIRECT(v, o, wPos, wNormal) \
            o.sh = BP3SHVertex(wPos, wNormal); \
            o.lightMapCoord = BP3TransLightMapCoord(float2(0,0), v.texcoord2.xy);
#       define BP3_APPLY_INDIRECT(i, wPos, wNormal, gi) _BP3_GI_BASE(i, wPos, wNormal, i.sh, i.lightMapCoord, gi)
#   else
#       define BP3_TRANSFER_INDIRECT(v, o, wPos, wNormal) o.sh = BP3SHVertex(wPos, wNormal);
#       define BP3_APPLY_INDIRECT(i, wPos, wNormal, gi) _BP3_GI_BASE(i, wPos, wNormal, i.sh, float4(0,0,0,0), gi)
#   endif
#else
#   define BP3_TRANSFER_INDIRECT(v, o, wPos, wNormal)
#   define BP3_APPLY_INDIRECT(i, wPos, wNormal, gi)
#endif

/**********************************************************************
 * ReamTime Lighting
 */
#define BP3_LIGHT_AMBIENT(ambient, diffuse) fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz * diffuse;

#ifdef BP3_DOUBLE_SIDED_LIGHTING
#  define BP3_LIGHT_DIFFUSE(diffCol, light, wNormal, diffuse) \
    fixed diff = abs(dot(wNormal, light.dir)); \
    fixed3 diffCol = diffuse * light.color * diff;
#else
#  define BP3_LIGHT_DIFFUSE(diffCol, light, wNormal, diffuse) \
    fixed diff = max(0, dot(wNormal, light.dir)); \
    fixed3 diffCol = diffuse * light.color * diff;
#endif

#ifdef BP3_DOUBLE_SIDED_LIGHTING
#  define BP3_LIGHT_SPECULAR(specCol, light, wNormal, wViewDir, specColor, specular, gloss) \
    float nh = abs(dot(wNormal, normalize(light.dir + wViewDir))); \
    float spec = pow(nh, specular * 128.0) * gloss; \
    fixed3 specCol = specColor * light.color * spec;
#else
#  define BP3_LIGHT_SPECULAR(specCol, light, wNormal, wViewDir, specColor, specular, gloss) \
    float nh = max(0, dot(wNormal, normalize(light.dir + wViewDir))); \
    float spec = pow(nh, specular * 128.0) * gloss; \
    fixed3 specCol = specColor * light.color * spec;
#endif

#ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT
#define BP3_COLOR_MERGE(color, diffuse, indirectDiffuse) color += diffuse * indirectDiffuse;
#else
#define BP3_COLOR_MERGE(color, diffuse, indirectDiffuse)
#endif

/**********************************************************************
 * Shadows Caster
 */

struct ShadowCasterAppInput {
    float4 vertex : POSITION;
    half3 normal : NORMAL;
    float4 texcoord : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct ShadowCasterFragInput {
    V2F_SHADOW_CASTER;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

ShadowCasterFragInput ShadowCasterVert(ShadowCasterAppInput v) {
    ShadowCasterFragInput o;
    UNITY_INITIALIZE_OUTPUT(ShadowCasterFragInput, o);
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    TRANSFER_SHADOW_CASTER_NORMALOFFSET(o);
    return o;
}

half4 ShadowCasterFrag(ShadowCasterFragInput i) : SV_Target {
    SHADOW_CASTER_FRAGMENT(i);
}

/**********************************************************************
 * PBR BRDF

//tex: Light Vector: $l$, View Vector: $v$
//tex: Halfway Vector: $h=\frac{l+v}{\|l+v\|}$
//tex: Radient Flux: $\Phi$,  Radiance: $L=\frac{\mathrm{d^2}\Phi}{\mathrm{d}A\mathrm{d}\omega\cos{\theta}}$

//tex: Irradiance: $E_i(\omega_{i})$, Radiance: $L_o(\omega_{o})$
//tex: BRDF: $f_{r}(\omega_{i},\omega_{o})=\frac{\mathrm{d}L_o(\omega_{o})}{\mathrm{d}E_i(\omega_{i})}=\frac{\mathrm{d}L_o(\omega_o)}{L_i(\omega_i)\cos\theta_i\mathrm{d}\omega_i}$
//tex: Reflectance Equation:
//$$L_o(p,w_o)=\int\limits_\Omega\!f_{r}(p,\omega_{i},\omega_{o})L_{i}(p,\omega_{i})n\cdot{\omega_{i}}\mathrm{d}\omega{_i}$$
//tex: Cook-Torrance BRDF:
//$$f_r=k_df_{lambert}+k_sf_{cook-torrance}$$
//$$f_{lambert}=\frac{c}{\pi}$$
//$$f_{cook-torrance}=\frac{DFG}{4(\omega_o\cdot{n})(\omega_i\cdot{n})}$$
//tex:
//$$NDF_{GGXTR}(n,h,\alpha)=\frac{\alpha^2}{\pi((n\cdot{h})^2(\alpha^2-1)+1)^2}$$
//tex:
//$$G_{SchlickGGX}(n,v,k)=\frac{n\cdot{v}}{(n\cdot{v})(1-k)+k}$$
//$$k_{direct}=\frac{(\alpha+1)^2}{8}$$
//$$k_{IBL}=\frac{\alpha^2}{2}$$
//$$G(n,v,l,k)=G_{sub}(n,v,k)G_{sub}(n,l,k)$$
//$$F_{Schlick}(n,v,F_{0}) = F_{0} + (1 - F_{0})(1 - (n\cdot{v}))^5$$
//tex: Cook-Torrance Reflectance Equation
//$$L_o(p,w_o)=$$
//$$\int_\limits{\Omega}(k_d\frac{c}{\pi}+k_s\frac{DFG}{4(\omega_o\cdot{n})(\omega_i\cdot{n})})L_i(p,w_i)n\cdot{\omega_i}\mathrm{d}\omega_i$$

//tex: Desney Diffuse Model:
//$$f_d=\frac{K_d}{\pi}(1+(F_{D90}-1)(1-\cos\theta_l)^5)(1+(F_{D90}-1)(1-\cos\theta_v)^5$$
 */

inline half FresnelSchlick(float3 f0, float3 n, float3 v) {
    half p = 1 - max(0, dot(n, v));
    half p5 = p * p * p * p * p;
    return f0 + (1 - f0) * p5;
}
#endif /* BP3_CG_INCLUDED */
