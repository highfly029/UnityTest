/**
 * Created by Chee <dev_chee@outlook.com>
 */
Shader "BadPerson3/Skybox" {
    Properties {
        _Tint("Tint Color", Color) = (0.5, 0.5, 0.5, 0.5)
        [Gamma] _Exposure("Exposure", Range(0, 8)) = 1.0
        _Rotation("Rotation", Range(0, 360)) = 0
        [NoScaleOffset] _Tex("Cubemap (HDR)", Cube) = "grey" {}
    }

    SubShader {
        Tags { "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" }
        Cull Off ZWrite Off

        Pass {
            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "BP3CG.cginc"

            half4 _Tint;
            half _Exposure;
            half _Rotation;
            samplerCUBE _Tex;
            half4 _Tex_HDR;

            float3 RotateAroundYInDegrees(float3 vertex, half degrees) {
                half alpha = degrees * BP3_PI_DIV_180;
                half sina, cosa;
                sincos(alpha, sina, cosa);
                float2x2 m = float2x2(cosa, -sina, sina, cosa);
                return float3(mul(m, vertex.xz), vertex.y).xzy;
            }

            struct AppData {
                float4 vertex : POSITION;
            };

            struct VertOutput {
                float4 vertex : SV_POSITION;
                float3 texcoord : TEXCOORD0;
            };

            VertOutput Vert(AppData v) {
                VertOutput o;
                float3 rotated = RotateAroundYInDegrees(v.vertex, _Rotation);
                o.vertex = UnityObjectToClipPos(rotated);
                o.texcoord = v.vertex.xyz;
                return o;
            }

            half4 Frag(VertOutput i) : SV_Target {
                half4 col = texCUBE(_Tex, i.texcoord);
                col.rgb = DecodeHDR(col, _Tex_HDR);
                col.rgb = col.rgb * _Tint.rgb * unity_ColorSpaceDouble.rgb;
                col *= _Exposure;
                return col;
            }
            ENDCG
        }
    }

    Fallback Off
}