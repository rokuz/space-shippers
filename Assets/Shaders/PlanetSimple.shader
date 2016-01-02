Shader "Space/Planet"
{
    Properties
    {
        _MainTex("Albedo", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
       
        CGPROGRAM
        #pragma surface surf Planet fullforwardshadows
        #pragma target 3.0
 
        struct Input
        {
            half2 uv_MainTex;
        };
       
        struct SurfaceOutputPlanet
        {
            fixed3 Albedo;
            fixed3 Normal;
            fixed3 Emission;
            fixed Alpha;
        };
       
        sampler2D _MainTex;
       
        inline fixed3 CalculateLighting(SurfaceOutputPlanet s, half3 n, UnityLight light)
        {
            return light.color * s.Albedo * saturate(dot(n, light.dir)) + s.Albedo * 0.03;
        }
       
        inline fixed4 LightingPlanet(SurfaceOutputPlanet s, half3 viewDir, UnityGI gi)
        {
            return fixed4(CalculateLighting(s, normalize(s.Normal), gi.light), 1.0);
        }

        inline void LightingPlanet_GI(SurfaceOutputPlanet s, UnityGIInput data, inout UnityGI gi)
        {
            gi = UnityGlobalIllumination(data, 1.0, 1.0, normalize(s.Normal));
        }
       
        void surf(Input IN, inout SurfaceOutputPlanet o)
        {
            o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
