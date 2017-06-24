Shader "* Custom/mapping_pivot"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Transparent("Transparent", Range(0, 1)) = 1
	}
	SubShader
	{
		// No culling or depth
		Lighting Off
		Cull Off ZWrite Off ZTest Always
		Blend SrcAlpha OneMinusSrcAlpha
		Tags{
			"Queue" = "Transparent"
			"RenderType" = "Transparent"
		}

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
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _Transparent;

			// pivot mapping
			float2 mapping(float2 iuv)
			{
				float2 uv = (iuv - 0.5) * 2;
				float theta = atan2(uv.r, uv.g) / 3.14159 / 2 + 0.5;
				float radiu = distance(uv, float2(0,0));
				return float2(theta, radiu);
			}

			fixed4 frag (v2f i) : SV_Target
			{
				clip(0.5 - distance(i.uv - 0.5, float2(0,0)));
				float2 uv = mapping(i.uv);
				float2 new_uv = TRANSFORM_TEX(uv, _MainTex);
				half4 col = tex2D(_MainTex, new_uv);
				col.a = _Transparent;
				return col;
			}
			ENDCG
		}
	}
}
