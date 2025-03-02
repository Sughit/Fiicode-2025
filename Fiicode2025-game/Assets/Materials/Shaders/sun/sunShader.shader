Shader "Custom/SunShader"
{
    Properties
    {
        _Color("Sun Color", Color) = (1, 1, 0, 1)
        _GlowIntensity("Glow Intensity", Range(0, 5)) = 1.0
        _PulsationSpeed("Pulsation Speed", Range(0, 5)) = 1.0
        _NoiseTex("Noise Texture (optional)", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv     : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            fixed4 _Color;
            float _GlowIntensity;
            float _PulsationSpeed;
            sampler2D _NoiseTex;
            float4 _NoiseTex_ST;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Intensitate uniformă pe întreaga suprafață
                float intensity = 1.0;
                
                // Efect de pulsare pentru dinamism
                float pulsate = sin(_Time.y * _PulsationSpeed * 6.28318) * 0.5 + 0.5;
                intensity *= lerp(0.8, 1.2, pulsate);

                // Adăugăm o variație subtilă folosind zgomotul (nu modifică culoarea, ci doar intensitatea)
                float2 noiseUV = i.uv * 3.0 + _Time.y * 0.05;
                float noise = tex2D(_NoiseTex, noiseUV).r;
                intensity *= lerp(0.9, 1.1, noise);

                // Aplicăm intensitatea uniformă, astfel încât întreaga suprafață să rămână de aceeași culoare
                fixed4 col = _Color * intensity * _GlowIntensity;
                col.a = 1.0;
                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
