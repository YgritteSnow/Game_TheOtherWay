Shader "* Custom/mapping_quad"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_TexRotate("TexRotate", Range(0,3.14)) = 0
		_LayoutRotate("LayoutRotate", Range(0,3.14)) = 0
		_Transparent("Transparent", Range(0, 1)) = 1
	}
		SubShader
	{
		Tags{
			"Queue" = "Transparent"
			"RenderType" = "Transparent"
		}
		// No culling or depth
		Lighting Off
		Cull Off ZWrite Off ZTest Always
		Blend SrcAlpha OneMinusSrcAlpha

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

	v2f vert(appdata v)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = v.uv;
		return o;
	}

	sampler2D _MainTex;
	float4 _MainTex_ST;
	float _TexRotate;
	float _LayoutRotate;
	float _Transparent;

	// quad mapping
	float2 mapping(float2 iuv)
	{
		float2 new_iuv = (iuv - 0.5) * 0.707;
		float2 cs = float2(cos(_TexRotate), -sin(_TexRotate));
		float2 sc = float2(sin(_TexRotate), cos(_TexRotate));
		new_iuv = float2(dot(cs, new_iuv), dot(sc, new_iuv)) + 0.5;

		float2 uv = abs((new_iuv - 0.5) * 2);
		float2 new_uv = (uv - 0.5) * 0.707;
		cs = float2(cos(_LayoutRotate), -sin(_LayoutRotate));
		sc = float2(sin(_LayoutRotate), cos(_LayoutRotate));
		return float2(dot(cs, new_uv), dot(sc, new_uv)) + 0.5;
	}

	fixed4 frag(v2f i) : SV_Target
	{
		float2 uv = mapping(i.uv);
		float2 new_uv = TRANSFORM_TEX(uv, _MainTex);
		half4 col = tex2D(_MainTex, new_uv);
		col.a *= _Transparent;
		return col;
	}
		ENDCG
	}
	}
}
