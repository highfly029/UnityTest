/**********************************************************************
 * Created by Chee <dev_chee@outlook.com>
 */

Shader "BadPerson3/Particle/Distortion" {
    Properties {
        [HideInInspector] _MainTex("Base", 2D) = "white" {}
        _BumpTex("Normal", 2D) = "bump" {}
        _Strength("Distort Strength", Range(0,1)) = 0.01
    }

    CGINCLUDE
    sampler2D _BumpTex;
    float4 _BumpTex_ST;
    sampler2D _GrabBackBufferTex;
    fixed _Strength;

    struct appdata_t {
        float4 vertex   : POSITION;
        float2 texcoord : TEXCOORD0;
        fixed4 color    : COLOR;
    };

    struct v2f {
        float4 pos          : SV_POSITION;
        float4 screenPos    : TEXCOORD0;
        float2 texcoord     : TEXCOORD1;
        fixed4 color        : COLOR;
    };
    ENDCG

    SubShader {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" "PreviewType" = "Plane" }
        LOD 200

        GrabPass {
            Name "GrabPass"
            "_GrabBackBufferTex"
        }

        Pass {
            Tags { "LightMode" = "Always" }
            Name "Distortion"

            Blend Off
            Cull Off ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityStandardUtils.cginc"

            v2f vert(appdata_t v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeGrabScreenPos(o.pos);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _BumpTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                half4 _packed = tex2D(_BumpTex, i.texcoord);
                half2 offset = UnpackNormal(_packed).xy * _Strength*i.color.a;
                i.screenPos.xy -= offset;
                fixed4 color = fixed4(tex2Dproj(_GrabBackBufferTex, UNITY_PROJ_COORD(i.screenPos)).rgb, i.color.r);
                return color;
            }

            ENDCG
        }
    }
}
