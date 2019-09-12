// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "Hidden/BadPerson3/OccluderTransparent" {
    Properties{
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _OccAlpha("Occlusion Alpha", Range(0.1, 1)) = 1
    }

    SubShader{
        Tags { "RenderType" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
        LOD 200

        CGPROGRAM
        #pragma target 3.0
        #pragma surface surf Lambert alpha:fade

        fixed4 _Color;
        sampler2D _MainTex;
        half _OccAlpha;

        struct Input {
            float2 uv_MainTex;
        };

        UNITY_INSTANCING_BUFFER_START(Props)
        // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf(Input IN, inout SurfaceOutput o) {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) *_Color;
            o.Albedo = c.rgb;
            o.Alpha = _OccAlpha;
        }
        ENDCG
    }
}
