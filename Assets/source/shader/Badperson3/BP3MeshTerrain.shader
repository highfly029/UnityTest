/*
 * Created by Chee <dev_chee@outlook.com>
 */

Shader "BadPerson3/MeshTerrain" {
    Properties {
        [ToggleOff] _Specular("Specular", Float) = 0
        _SpecColor("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
        _SpecPower("Specular Power", Float) = 0.6
        _Splat0("Layer 1", 2D) = "white" {}
        _Splat1("Layer 2", 2D) = "white" {}
        _Splat2("Layer 3", 2D) = "white" {}
        _Splat3("Layer 4", 2D) = "white" {}
        _ShininessL0("Layer 0 Shininess", Range(0.0, 1.0)) = 1.0
        _ShininessL1("Layer 1 Shininess", Range(0.0, 1.0)) = 1.0
        _ShininessL2("Layer 2 Shininess", Range(0.0, 1.0)) = 1.0
        _ShininessL3("Layer 3 Shininess", Range(0.0, 1.0)) = 1.0
        [NoScaleOffset][Normal] _BumpSplat0("Layer 0 Normal", 2D) = "bump" {}
        [NoScaleOffset][Normal] _BumpSplat1("Layer 1 Normal", 2D) = "bump" {}
        [NoScaleOffset][Normal] _BumpSplat2("Layer 2 Normal", 2D) = "bump" {}
        [NoScaleOffset][Normal] _BumpSplat3("Layer 3 Normal", 2D) = "bump" {}
        _BumpScale0("Layer 0 Normal Scale", Float) = 1
        _BumpScale1("Layer 1 Normal Scale", Float) = 1
        _BumpScale2("Layer 2 Normal Scale", Float) = 1
        _BumpScale3("Layer 3 Normal Scale", Float) = 1

        [NoScaleOffset]_Control("Control (RGBA)", 2D) = "white" {}
        [HideInInspector] _MainTex("Never Used", 2D) = "white" {}
    }

    SubShader {
        Tags { "SplatCount" = "4" "RenderType" = "Opaque" }
        LOD 200

        Pass {
            Name "FORWARD_BASE"
            Tags { "LightMode" = "ForwardBase"}

            Cull Back
            ZTest LEqual
            ZWrite On

            CGPROGRAM
            #pragma shader_feature _ _SPLATMAP0
            #pragma shader_feature _ _SPLATMAP1
            #pragma shader_feature _ _SPLATMAP2
            #pragma shader_feature _ _SPLATMAP3
            #pragma shader_feature _ _NORMALMAP0
            #pragma shader_feature _ _NORMALMAP1
            #pragma shader_feature _ _NORMALMAP2
            #pragma shader_feature _ _NORMALMAP3
            #pragma shader_feature _ _SPECULAR
            //#pragma multi_compile_instancing
            #pragma multi_compile_fog
            #pragma multi_compile_fwdbase

            #include "BP3MeshTerrain.cginc"
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

            #include "BP3MeshTerrain.cginc"
            #pragma vertex ShadowCasterVert
            #pragma fragment ShadowCasterFrag
            ENDCG
        }

        Pass {
            Name "META"
            Tags { "LightMode" = "Meta" }

            Cull Off

            CGPROGRAM
            #pragma shader_feature EDITOR_VISUALIZATION
            #include "UnityStandardMeta.cginc"
            #pragma vertex vert_meta
            #pragma fragment frag_meta
            ENDCG
        }
    }

    CustomEditor "BP3MeshTerrainShaderGUI"
}
