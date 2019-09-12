﻿// Upgrade NOTE: replaced '_Projector' with 'unity_Projector'
// Upgrade NOTE: replaced '_ProjectorClip' with 'unity_ProjectorClip'

Shader "BadPerson3/EffectBlindProjector" {
	Properties{
		_Color("Main Color", Color) = (1,1,1,1)
		_ShadowTex("Cookie", 2D) = "" {}
		_FalloffTex("FallOff", 2D) = "" {}
		_ScaleCenter("_ScaleCenter",Float) = 1
	}

	Subshader{
		Tags{ "Queue" = "Transparent" }
		Pass{
			ZTest LEqual
			ZWrite Off
			ColorMask RGB
			Blend Zero SrcColor
			Offset -1, -1
			Cull Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#include "UnityCG.cginc"

			struct v2f {
				float4 uvShadow : TEXCOORD0;
				float4 uvFalloff : TEXCOORD1;
				UNITY_FOG_COORDS(2)
				float4 pos : SV_POSITION;
			};

			float4x4 unity_Projector;
			float4x4 unity_ProjectorClip;

			fixed4 _Color;
			sampler2D _ShadowTex;
			sampler2D _FalloffTex;
			float _ScaleCenter;
			v2f vert(float4 vertex : POSITION)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(vertex);
				o.uvShadow = mul(unity_Projector, vertex);


				o.uvFalloff = mul(unity_ProjectorClip, vertex);
		
				UNITY_TRANSFER_FOG(o,o.pos);
				return o;
			}

			

			fixed4 frag(v2f i) : SV_Target
			{
				float2 uvShadow = i.uvShadow.xy / i.uvShadow.w;

				uvShadow = uvShadow*_ScaleCenter + (1 - _ScaleCenter)*float2(0.5, 0.5);

				fixed4 texS = tex2D(_ShadowTex, uvShadow);
				texS.rgb *= _Color.rgb;
				texS.a = 1.0 - texS.a;

				fixed4 texF = tex2Dproj(_FalloffTex, UNITY_PROJ_COORD(i.uvFalloff));
				fixed4 res = texS * texF.a;

				//res.a = Luminance(res.rgb);

				UNITY_APPLY_FOG_COLOR(i.fogCoord, res, fixed4(0,0,0,0));
				return res;
			}
			ENDCG
		} 
	}
}
