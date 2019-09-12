/* vim:ts=4:sw=4:
 * Created by Chee <dev_chee@outlook.com>
 */
Shader "BadPerson3/Water" {
    Properties {
        _WaveScale("Wave Scale", Range(0.02,0.15)) = 0.063
        _WaveSpeed("Wave Speed(map1 x,y; map2 x,y)", Vector) = (19,9,-16,-7)
        [NoScaleOffset] _ReflectiveTex("Reflective (A)", 2D) = "" {}
        _ReflectiveColor("Reflective", Color) = (1,1,1,0.8)
        [Normal][NoScaleOffset] _BumpMap("Normal Map", 2D) = "bump" {}
        _BumpScale("Normal Scale", Range(0.3, 2.0)) = 0.5
        _ReflectionTex("Reflection Tex", 2D) = "white" {}
        _ReflectionScale("Reflection Scale", Range(0,1)) = 0.44

        [HideInInspector] _WaterMode("__Water Mode", Float) = 0.0
    }

    SubShader {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "PerformanceChecks" = "False" }
        LOD 300

        Pass {
            Name "WATER"
            Tags { "LightMode" = "Always" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite On

            CGPROGRAM
            #pragma vertex WaterVert
            #pragma fragment WaterFrag
            #pragma shader_feature _ BP3_WATER_REFLECTIVE_ON BP3_WATER_REFRACTIVE_ON BP3_WATER_REALTIME_REFLECTIVE_ON
            #pragma multi_compile_fog
            #include "BP3Water.cginc"
            ENDCG
        }
    }

    CustomEditor "BP3WaterShaderGUI"
}