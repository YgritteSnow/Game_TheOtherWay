Shader "* Custom/blur"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_StepX("StepX", Float) = 0.1
		_StepY("StepY", Float) = 0.1
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

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
			float _StepX;
			float _StepY;

			static const int core_width = 5;
			static const int core_height = 5;

			fixed4 frag(v2f i) : SV_Target
			{
				float4 col = float4(0,0,0,0);
				for (int idx_x = 0; idx_x < core_width; ++idx_x)
				{
					for (int idx_y = 0; idx_y < core_height; ++idx_y)
					{
						col += tex2D(_MainTex, i.uv + float2(_StepX*idx_x, _StepY*idx_y));
					}
				}
				col = col / core_width / core_height;
				return col;
			}
			ENDCG
		}
	}
}
