/**
 * Created by Chee <dev_chee@outlook.com>
 */

Shader "BadPerson3/Object" {
    Properties {
        [NoScaleOffset] _AlphaTex("Alpha (A)", 2D) = "white" {}
        _Cutoff("Cutoff", Range(0,1)) = 0.5
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Base (RGB)", 2D) = "white" {}
        [NoScaleOffset][Normal] _BumpMap("Normal", 2D) = "bump" {}
        _BumpScale("Normal Scale", Float) = 1

        _SpecColor("Specular Color", Color) = (0.5,0.5,0.5,1)
        _SpecShine("Specular Shine", Float) = 0.6
        _SpecGlossness("Specular Glossness", Range(0.03,1)) = 0.078125

        [ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 0
        [ToggleOff] _DoubleSided("Double Sided", Float) = 0
        [ToggleOff] _DoubleSidedLighting("Double Sided Lighting", Float) = 0

        [HideInInspector] _Mode("__mode", Float) = 0
        [HideInInspector] _SrcBlend("__src", Float) = 1
        [HideInInspector] _DstBlend("__dst", Float) = 0
        [HideInInspector] _ZWrite("__zwrite", Float) = 1
        [HideInInspector] _Cull("__cull", Float) = 2
    }

    SubShader {
        Tags { "Queue" = "Geometry" "RenderType" = "Opaque" }
        LOD 200

        Pass {
            Name "FORWARD_BASE"
            Tags { "LightMode" = "ForwardBase"}

            Blend [_SrcBlend] [_DstBlend]
            Cull [_Cull]
            ZTest LEqual
            ZWrite [_ZWrite]

            CGPROGRAM
            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON
            #pragma shader_feature _ _NORMALMAP
            #pragma shader_feature _ _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature _ _DOUBLE_SIDED_LIGHTING

            #pragma multi_compile_instancing
            #pragma multi_compile_fog
            #pragma multi_compile_fwdbase

            #include "BP3Object.cginc"
            #pragma vertex FwdBaseVert
            #pragma fragment FwdBaseFrag
            ENDCG
        }

        Pass {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZTest LEqual
            ZWrite On

            CGPROGRAM
            #pragma skip_variants SHADOWS_SOFT
            #pragma multi_compile_shadowcaster
            #pragma multi_compile_instancing
            //#pragma multi_compile _ LOD_FADE_CROSSFADE

            #include "BP3Object.cginc"
            #pragma vertex ShadowCasterVert
            #pragma fragment ShadowCasterFrag
            ENDCG
        }

        Pass {
            Name "META"
            Tags { "LightMode" = "Meta" }

            Cull Off

            CGPROGRAM
            #pragma vertex vert_meta
            #pragma fragment frag_meta
            #pragma shader_feature EDITOR_VISUALIZATION
            #include "UnityStandardMeta.cginc"
            ENDCG
        }
    }

    CustomEditor "BP3ObjectShaderGUI"
}
