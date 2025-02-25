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
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"}
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 worldPos : TEXCOORD0;
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

            // Simple pseudo-random noise
            float rand(float3 n) { 
                return frac(sin(dot(n, float3(12.9898, 78.233, 37.719)))*43758.5453);
            }

            // Simple smooth noise function
            float noise(float3 p){
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

                float3 vertexPos = input.positionOS.xyz;

                float vertexHeight = length(vertexPos) - _PlanetRadius;

                float inWater = step(vertexHeight, _WaterHeight);

                // Calculate smooth, natural-looking waves
                float wave = noise(vertexPos * _WaveScale + _Time.y * _WaveSpeed) - 0.5;
                wave *= _WaveStrength;

                vertexPos += normalize(vertexPos) * wave * inWater;

                VertexPositionInputs vertexInput = GetVertexPositionInputs(vertexPos);
                output.positionHCS = vertexInput.positionCS;
                output.worldPos = vertexInput.positionWS;

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float height = length(input.worldPos) - _PlanetRadius;
                float t = saturate((height - _MinHeight) / (_MaxHeight - _MinHeight));
                half4 col = SAMPLE_TEXTURE2D(_GradientTex, sampler_GradientTex, float2(t, 0));
                return col;
            }
            ENDHLSL
        }
    }
}
