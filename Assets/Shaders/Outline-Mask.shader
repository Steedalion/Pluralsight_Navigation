// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/Outline-Mask"
{
	Properties
	{
	}
	SubShader
	{
		Tags
	{
		"RenderType" = "Transparent"
		"Queue" = "Transparent+1"
	}
		LOD 100

		// render model

		Pass
	{
		Stencil
	{
		Ref 128
		Comp always
		Pass replace
	}

		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off

		CGPROGRAM

#pragma vertex vert
#pragma fragment frag

		float4 _Color;

	struct appdata
	{
		float4 vertex : POSITION;
		float3 normal : NORMAL;
	};

	struct v2f
	{
		float4 pos : SV_POSITION;
	};

	v2f vert(appdata v)
	{
		v2f o;

		float4 vert = v.vertex;

		o.pos = UnityObjectToClipPos(vert);

		return o;
	}

	half4 frag(v2f i) : COLOR
	{
		return 0;
	}

		ENDCG
	}
	}

}
