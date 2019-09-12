/**********************************************************************
 * Created by Chee <dev_chee@outlook.com>
 *
 * Heat distortion effect for Particles System that with GrabPass feature.
 */

Shader "BadPerson3/Particle/GrabPassHeatDistortion"
{
	Properties {
		[HideInInspector] _MainTex ("Alpha (A)", 2D) = "white" {}
		_NoiseTex ("Noise (R(x), G(y), B(mask))", 2D) = "white" {}
		_Strength ("Distort Strength", Range(0,1)) = 0.01
		_TimeFactor("Distort Time Factor", Range(0,1)) = 0.5 
	}

	SubShader {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "PreviewType"="Plane" "RenderType"="Transparent" }
		LOD 200
	
		GrabPass { // Grab backbuffer
			Name "GrabPass"

			"_GrabBackBufferTex"
		}

		Pass { // Make distortion
			Name "Distortion"

			//Blend SrcAlpha Zero
			Blend SrcAlpha OneMinusSrcAlpha
			//AlphaTest Greater 0.01
			Cull Off ZWrite Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			//#pragma shader_feature MASK
			//#pragma shader_feature MIRROR_EDGE
			//#pragma shader_feature DEBUGUV
			//#pragma shader_feature DEBUGDISTANCEFADE
			#pragma fragmentoption ARB_precision_hint_fastest

            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"

			/*******************************************************************
			 * Variables declaration
			 */

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _NoiseTex;
			sampler2D _GrabBackBufferTex;

			float _Strength;
			float _TimeFactor;

            struct appdata_t {
                float4 vertex	: POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f {
                float4 position		: SV_POSITION;
                float4 screen_pos	: TEXCOORD0;
                float2 uv			: TEXCOORD1;
            };

			/*******************************************************************
			 * Vertex shader
			 */
			v2f vert(appdata_t v) {
				v2f o;
                UNITY_INITIALIZE_OUTPUT(v2f, o);

                o.position = UnityObjectToClipPos(v.vertex);
                o.screen_pos = ComputeGrabScreenPos(o.position);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				return o;
			}

			/*******************************************************************
			 * Fragment shader
			 */
			fixed4 frag (v2f i) : SV_Target {
                half2 offset = tex2D(_NoiseTex, (i.uv - _SinTime.zz) * _TimeFactor).rg; // get R,G channel
                i.screen_pos.xy -= offset * _Strength;

                half2 mask = tex2D(_NoiseTex, i.uv).b; // distort mask

                half4 color = tex2Dproj(_GrabBackBufferTex, UNITY_PROJ_COORD(i.screen_pos));
                color.a *= mask;

                half4 tint = tex2D(_MainTex, i.uv);
                color *= tint;
                return color;
			}

			ENDCG
		}
	}
}
