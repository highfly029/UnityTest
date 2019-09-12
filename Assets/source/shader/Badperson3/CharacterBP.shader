/**
 * Created by Chee <dev_chee@outlook.com>
 */

Shader "BadPerson3/CharacterBP" {
    Properties{
        _Color("Main Color", color) = (1,1,1,1)
        _MainTex("Base (RGB)", 2D) = "white" {}
        [NoScaleOffset] _AlphaTex("Alpha (A)", 2D) = "white" {}
        _Cutoff("Alpha Cutoff", Range(0,1)) = 0.5

        [NoScaleOffset] _MaskTex("Mask Gloss(R) Reflect(G)", 2D) = "white" {}
        [NoScaleOffset][Normal] _NormalTex("Normal Map", 2D) = "bump" {}
        [NoScaleOffset] _CubeTex("Reflect Cubemap", Cube) = "" {}
        _ReflectScale("Reflect Scale", Range(0,1)) = 0.5

        _Indensity("Light incident indensity", Range(0, 1)) = 1

        _SpecColor("Specular Color", Color) = (1,1,1,1)
        _SpecPower("Specular Power", Range(0.1,1)) = 0.8
        _SpecScale("Specular Scale", Range(0,1)) = 1

        [ToggleOff] _Rim("Rim Light", Float) = 0
        _RimColor("Rim Color", Color) = (1, 1, 1, 1)
        [NoScaleOffset] _RimTex("Rim Texture", 2D) = "black" {}
        _RimPower("Rim Power", Range(0, 5)) = 1
        _RimRange("Rim Range", Range(0, 1)) = 0.2

        [ToggleOff] _HitGlow("Hit Glow", Float) = 0
        _HitGlowColor("Glow Color", Color) = (1, 1, 1, 1)
        _HitGlowPower("Glow Power", Range(0.1, 8)) = 4

        [ToggleOff] _Highlights("Specular Highlights", Float) = 1
        [ToggleOff] _Reflections("Glossy Reflections", Float) = 1

        [HideInInspector] _Mode("__mode", Float) = 0
        [HideInInspector] _SrcBlend("__src", Float) = 1
        [HideInInspector] _DstBlend("__dst", Float) = 0
        [HideInInspector] _ZWrite("__zw", Float) = 1
    }

    SubShader {
        Tags { "RenderType" = "Opaque" "PerformanceChecks" = "False" }
        LOD 300

        Pass {
            Name "FORWARD_BASE"
            Tags { "LightMode" = "ForwardBase"}

            Blend[_SrcBlend][_DstBlend]
            ZWrite[_ZWrite]

            CGPROGRAM
            #pragma target 3.0

            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _REFLECT_CUBEMAP
            #pragma shader_feature _RIM_LIGHT
            #pragma shader_feature _SPEC_HIGHLIGHTS_OFF
            #pragma shader_feature _GLOSS_REFLECTIONS_OFF

            #pragma multi_compile_fwdbase
            #pragma multi_compile_instancing
            #pragma multi_compile_fog
            //#pragma multi_compile _ LOD_FADE_CROSSFADE
            #pragma multi_compile _ _HIT_GLOW

            #include "CharacterCommon.cginc"
            #pragma vertex VertForward
            #pragma fragment FragForward
            ENDCG
        }

        Pass {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On ZTest LEqual

            CGPROGRAM
            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            //#pragma skip_variants SHADOWS_SOFT
            #pragma multi_compile_shadowcaster
            #pragma multi_compile_instancing
            //#pragma multi_compile _ LOD_FADE_CROSSFADE

            #include "CharacterCommon.cginc"
            #pragma vertex ShadowCasterVert
            #pragma fragment ShadowCasterFrag
            ENDCG
        }

        Pass {
            Name "META"
            Tags { "LightMode"="Meta" }

            Cull Off

            CGPROGRAM
            #pragma vertex vert_meta
            #pragma fragment frag_meta
            #pragma shader_feature EDITOR_VISUALIZATION
            #include "UnityStandardMeta.cginc"
            ENDCG
        }
    }

    CustomEditor "CharacterBPShaderGUI"
}
