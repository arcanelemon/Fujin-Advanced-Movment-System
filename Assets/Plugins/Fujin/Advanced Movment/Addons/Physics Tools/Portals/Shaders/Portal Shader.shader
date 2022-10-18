Shader "Fujin/Portal Shader"
{
	Properties
	{
		[HDR] _Color("Color", Color) = (1, 1, 1, 1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		[HDR] _Tint("Tint", Color) = (1, 1, 1, 1)
		_RenderTex("Render Texture", 2D) = "white" {}
		[HDR] _Emission("Emission", Color) = (0, 0, 0, 1)
		_Noise("Noise", Range(0,1)) = 0.5
		_DissolveEdge("Dissolve Edge", Range(0,1)) = 0.0
		_DissolveAmount("Disolve Amount", Range(0,1)) = 0.0
		_DissolveProgress("Disolve Progress", Range(0,1)) = 0.0
	}
		SubShader
	{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		Lighting Off
		Cull Back
		ZWrite On
		ZTest Less

		Fog{ Mode Off }

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				//float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float4 screenPos : TEXCOORD1;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.screenPos = ComputeScreenPos(o.vertex);
				return o;
			}

			float4 _Color;
			sampler2D _MainTex;
			float4 _Tint;
			sampler2D _RenderTex;

			fixed4 frag(v2f i) : SV_Target
			{
				i.screenPos /= i.screenPos.w;
				fixed4 col = _Tint * tex2D(_RenderTex, float2(i.screenPos.x, i.screenPos.y));

				return col;
			}

			ENDCG
		}
	}
}