/**
 * Created by Chee <dev_chee@outlook.com>
 */

Shader "BadPerson3/Character" {
    Properties {
        [NoScaleOffset] _AlphaTex("Alpha (A)", 2D) = "white" {}
        _Color("Color", color) = (1,1,1,1)
        _MainTex("Base (RGB)", 2D) = "white" {}
        _GlossScale("Gloss Scale", Float) = 1
        [NoScaleOffset] _MaskTex("Gloss(R) Reflective(G)", 2D) = "white" {}
        _BumpScale("Normal Scale", Float) = 1
        [NoScaleOffset][Normal] _BumpMap("Normal", 2D) = "bump" {}
        _ReflecRadio("Reflective Radio", Range(0,1)) = 0.4
        [NoScaleOffset] _CubeMap("Reflective Cubemap", Cube) = "" {}
        _CubeMapRotation("CubeMap Rotation", Range(0, 360)) = 0
        [ToggleOff] _Specular("Specular", Float) = 0
        _SpecColor("Specular Color", Color) = (1,1,1,1)
        _SpecPower("Specular Power", Range(0.001,1)) = 0.17
        [ToggleOff] _Rim("Rim Lighting", Float) = 0
        _RimColor("Rim Color", Color) = (1, 1, 1, 1)
        [NoScaleOffset] _RimMap("Rim Map", 2D) = "black" {}
        _RimRange("Rim Range", Range(0, 1)) = 0.2
        _RimPower("Rim Power", Float) = 1
        [ToggleOff] _HitGlow("Hit Glow", Float) = 0
        _HitGlowColor("Glow Color", Color) = (1, 1, 1, 1)
        _HitGlowPower("Glow Power", Float) = 4

        [HideInInspector] _Mode("__mode", Float) = 0
        [HideInInspector] _SrcBlend("__src", Float) = 1
        [HideInInspector] _DstBlend("__dst", Float) = 0
        [HideInInspector] _CullMode("__cull", Float) = 2
		[HideInInspector] _ZWrite("__zw", Float) = 1

		[ToggleOff] _HasMask("Has Mask", Float) = 0
		_MaskRect("_mask",Vector) = (-10,-10,10,10)
    }

    SubShader {
        Tags { "Queue" = "Geometry-100" "RenderType" = "Opaque" }
        LOD 300

        Pass {
            Name "FORWARD_BASE"
            Tags { "LightMode" = "ForwardBase"}

            Blend [_SrcBlend] [_DstBlend]
            Cull [_CullMode]
            ZTest LEqual
            ZWrite [_ZWrite]

            CGPROGRAM
            #pragma shader_feature _ _BP3_CHARACTER_HAIR
            #pragma shader_feature _ _NORMALMAP
            #pragma shader_feature _ _SPECULAR
            #pragma shader_feature _ _CUBEMAP
            #pragma shader_feature _ _RIM_LIGHT

            #pragma multi_compile_fwdbase
            #pragma multi_compile_instancing
            //#pragma multi_compile_fog
            //#pragma multi_compile _ LOD_FADE_CROSSFADE
            #pragma multi_compile _ _HIT_GLOW
			#pragma multi_compile _ _HAS_MASK
			#pragma multi_compile _ _ALPHABLEND_ON

            #include "BP3Character.cginc"
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

            #include "BP3Character.cginc"
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

    SubShader {
        Tags { "Queue" = "Geometry-100" "RenderType" = "Opaque" }
        LOD 200

        Pass {
            Name "FORWARD_BASE"
            Tags { "LightMode" = "ForwardBase"}

            Blend [_SrcBlend] [_DstBlend]
            Cull [_CullMode]
            ZTest LEqual
            ZWrite On

            CGPROGRAM
            #pragma shader_feature _ _BP3_CHARACTER_HAIR
            //#pragma shader_feature _ _NORMALMAP
            #pragma shader_feature _ _SPECULAR
            #pragma shader_feature _ _CUBEMAP
            //#pragma shader_feature _ _RIM_LIGHT
            #pragma multi_compile_fwdbase
            #pragma multi_compile_instancing
            //#pragma multi_compile_fog
            //#pragma multi_compile _ LOD_FADE_CROSSFADE
            #pragma multi_compile _ _HIT_GLOW

            #include "BP3Character.cginc"
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

            #include "BP3Character.cginc"
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

    SubShader {
        Tags { "Queue" = "Geometry-100" "RenderType" = "Opaque" }
        LOD 100

        Pass {
            Name "FORWARD_BASE"
            Tags { "LightMode" = "ForwardBase"}

            Blend [_SrcBlend] [_DstBlend]
            Cull [_CullMode]
            ZTest LEqual
            ZWrite On

            CGPROGRAM
            #pragma shader_feature _ _BP3_CHARACTER_HAIR
            //#pragma shader_feature _ _NORMALMAP
            //#pragma shader_feature _ _SPECULAR
            #pragma shader_feature _ _CUBEMAP
            //#pragma shader_feature _ _RIM_LIGHT
            #pragma multi_compile_fwdbase
            #pragma multi_compile_instancing
            //#pragma multi_compile_fog
            //#pragma multi_compile _ LOD_FADE_CROSSFADE
            #pragma multi_compile _ _HIT_GLOW

            #include "BP3Character.cginc"
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

            #include "BP3Character.cginc"
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

    CustomEditor "BP3CharacterShaderGUI"
}
