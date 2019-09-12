// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "BadPerson3/EffectInvisible" {
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_RimColor("Rim Color", Color) = (1, 1, 1, 1)
		_FadeWidth("Fade Width", Range(0.01,1)) = 0.1
		_RimWidth("Rim Width", Range(0,1)) = 0.1
		_BumpMap("Normal Map", 2D) = "bump" {}
		_BumpScale("Normal Scale", Float) = 1

	}

	SubShader
	{

		Tags
		{
			"Queue" = "Transparent"
			"RenderType" = "Transparent"
		}
		Pass
		{
			Lighting Off
			ZWrite On
			ZTest LEqual
			Blend  SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			#pragma multi_compile _ _NORMALMAP
			#pragma vertex vert  
			#pragma fragment frag  
			#include "UnityCG.cginc"  
			#include "BP3CG.cginc"  

			#ifdef _NORMALMAP
			#define BP3_HAS_NORMALMAP
			#endif

			struct v2f
			{
				float4 pos : SV_POSITION;
				float4 texcoord : TEXCOORD0;
				BP3_DECLARE_TANGENT_AND_POS(1);
			};
			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _RimColor;
			float _RimWidth;
			float _FadeWidth;
			sampler2D _BumpMap;
			half _BumpScale;
			v2f vert(appdata_base v) 
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.texcoord.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
				BP3_TRANSFER_TANGENT_AND_POS(v, o, wPos, wNormal);
				return o;
			}

			fixed4 frag(v2f i) : COLOR
			{
				BP3_APPLY_TANGENT_AND_POS(i, wPos, wNormal);
				float3 wViewDir = normalize(UnityWorldSpaceViewDir(wPos));
				float dotProduct = 1 - dot(wNormal, wViewDir);
				float fade = min(max(dotProduct - 1 + _RimWidth, 0) / _FadeWidth, 1);

				fixed4 color = tex2D(_MainTex,i.texcoord.xy)*_RimColor*fade;

				return color;
			}
			ENDCG
		}
	}
}