Shader "Custom/w_PlanetHeightURPShader"
{
    Properties
    {
        _GradientTex ("Gradient Texture", 2D) = "white" {}
        _PlanetRadius ("Planet Radius", Float) = 1
        _MinHeight ("Minimum Height", Float) = -0.2
        _MaxHeight ("Maximum Height", Float) = 1

        _WaterHeight ("Water Height", Float) = 0.0
        _WaveStrength ("Wave Strength", Float) = 0.05
        _WaveScale ("Wave Scale", Float) = 2.0
        _WaveSpeed ("Wave Speed", Float) = 1.0

        _FoamThreshold ("Foam Threshold", Float) = 0.05
        _FoamStrength ("Foam Strength", Float) = 0.5
        _FoamColor ("Foam Color", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            // Vom transmite înălțimea calculată în object space (fără valuri) către fragment shader.
            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 normal : TEXCOORD1;
                float height : TEXCOORD2;
            };

            TEXTURE2D(_GradientTex);
            SAMPLER(sampler_GradientTex);

            float _PlanetRadius;
            float _MinHeight;
            float _MaxHeight;
            float _WaterHeight;
            float _WaveStrength;
            float _WaveScale;
            float _WaveSpeed;
            float _FoamThreshold;
            float _FoamStrength;
            float4 _FoamColor; // Noul câmp pentru culoarea spumei

            // Funcție de zgomot pseudo-aleator
            float rand(float3 n) 
            { 
                return frac(sin(dot(n, float3(12.9898,78.233,37.719))) * 43758.5453);
            }

            // Funcție de zgomot lin și neted
            float noise(float3 p)
            {
                float3 ip = floor(p);
                float3 fp = frac(p);
                fp = fp * fp * (3.0 - 2.0 * fp);
                float n = lerp(
                    lerp(
                        lerp(rand(ip), rand(ip + float3(1,0,0)), fp.x),
                        lerp(rand(ip + float3(0,1,0)), rand(ip + float3(1,1,0)), fp.x),
                        fp.y),
                    lerp(
                        lerp(rand(ip + float3(0,0,1)), rand(ip + float3(1,0,1)), fp.x),
                        lerp(rand(ip + float3(0,1,1)), rand(ip + float3(1,1,1)), fp.x),
                        fp.y),
                    fp.z);
                return n;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;

                // Poziția originală în object space (fără aplicarea efectului de wave pentru deplasare)
                float3 originalPos = input.positionOS.xyz;
                // Calculăm înălțimea în object space: diferența dintre distanța de la centru și raza planetei.
                float vertexHeight = length(originalPos) - _PlanetRadius;

                // Calculul efectului de wave pentru a anima spuma, fără să deplaseze geometriile
                // (valoarea wave se folosește mai târziu în fragment shader).
                float wave = noise(originalPos * _WaveScale + _Time.y * _WaveSpeed) - 0.5;

                VertexPositionInputs vertexInput = GetVertexPositionInputs(originalPos);
                output.positionHCS = vertexInput.positionCS;
                output.worldPos = vertexInput.positionWS;
                output.normal = normalize(vertexInput.positionWS);
                output.height = vertexHeight;

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Calculăm factorul pentru gradient pe baza înălțimii (constant în object space)
                float height = input.height;
                float t = saturate((height - _MinHeight) / (_MaxHeight - _MinHeight));
                half4 baseColor = SAMPLE_TEXTURE2D(_GradientTex, sampler_GradientTex, float2(t, 0));

                // Obținem lumina principală din URP
                Light mainLight = GetMainLight();
                float3 lightDir = normalize(mainLight.direction);
                float3 normal = normalize(input.normal);

                // Iluminare difuză simplă (Lambert)
                float diff = saturate(dot(normal, lightDir));
                half3 litColor = baseColor.rgb * (diff * mainLight.color.rgb + 0.1);
                float ambientStrength = 0.15;
                litColor += baseColor.rgb * ambientStrength;

                // Aplicăm efectul de spumă: dacă înălțimea este în jurul nivelului apei, adăugăm spumă animată.
                float foamMask = smoothstep(_FoamThreshold, 0.0, abs(height - _WaterHeight));
                float foamNoise = noise(input.worldPos * _WaveScale + _Time.y * _WaveSpeed);

                // Folosim culoarea setată în proprietatea _FoamColor în loc de (1.0, 1.0, 1.0).
                // Intensitatea este controlată de foamMask și _FoamStrength.
                litColor = lerp(litColor, _FoamColor.rgb, foamMask * _FoamStrength * foamNoise);

                return half4(litColor, baseColor.a);
            }
            ENDHLSL
        }
    }
}
