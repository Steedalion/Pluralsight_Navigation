﻿// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/Outline-Render"
{
	Properties
	{
		_Outline("Outline Length", Range(0.0, 1.0)) = 0.2
		_OutlineColor("Outline Color", Color) = (0.2, 0.2, 0.2, 1.0)
	}

		SubShader
	{
		Tags
	{
		"RenderType" = "Transparent"
		"Queue" = "Transparent+2"
	}
		LOD 100

		// render outline

		Pass
	{
		Stencil
	{
		Ref 128
		Comp NotEqual
	}

		Cull Off
		ZWrite Off

		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off

		CGPROGRAM

#pragma vertex vert
#pragma fragment frag

		float _Outline;
	float4 _OutlineColor;

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
		vert.xyz += v.normal * _Outline;

		o.pos = UnityObjectToClipPos(vert);

		return o;
	}

	half4 frag(v2f i) : COLOR
	{
		return _OutlineColor;
	}

		ENDCG
	}

	}
}
