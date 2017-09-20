// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Space/PlanetGround" 
{
	Properties 
	{
		_MainTex("Albedo", 2D) = "white" {}
		_Blueish("Blueish", float) = 1.5
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }

		CGPROGRAM
		#pragma surface surf Lambert noforwardadd

		sampler2D _MainTex;
		float _Blueish;

		struct Input
		{
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			c.b *= _Blueish;
			o.Albedo = c.rgb;
			o.Alpha = 1.0;
		}
		ENDCG  
	}
}