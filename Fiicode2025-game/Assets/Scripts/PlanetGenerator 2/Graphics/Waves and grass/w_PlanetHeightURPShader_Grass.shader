Shader "Custom/w_PlanetHeightURPShader_Grass"
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

        _GrassWindStrength ("Grass Wind Strength", Float) = 0.02
        _GrassWindSpeed ("Grass Wind Speed", Float) = 1.5
        _GrassThreshold ("Grass Color Threshold", Float) = 0.3
        _GrassHeight ("Grass Height", Float) = 0.1
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"}
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
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

            float _GrassWindStrength;
            float _GrassWindSpeed;
            float _GrassThreshold;
            float _GrassHeight;

            // Noise function for randomness
            float rand(float3 seed) {
                return frac(sin(dot(seed, float3(12.9898, 78.233, 37.719))) * 43758.5453);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float3 worldPos = TransformObjectToWorld(input.positionOS.xyz);
                float3 normal = normalize(worldPos);

                // Compute UVs for texture sampling
                float height = length(worldPos) - _PlanetRadius;
                float t = saturate((height - _MinHeight) / (_MaxHeight - _MinHeight));
                float2 uv = float2(t, 0);

                // Sample the texture in the vertex shader using LOD
                half4 sampledColor = SAMPLE_TEXTURE2D_LOD(_GradientTex, sampler_GradientTex, uv, 0);

                // Check if this is a grass area (green dominant)
                float isGrass = step(_GrassThreshold, sampledColor.g - (sampledColor.r + sampledColor.b) * 0.5);

                // Apply wind effect only to grass
                float windEffect = sin(_Time.y * _GrassWindSpeed + worldPos.x * 0.5) * _GrassWindStrength;
                
                // Make grass blades stand up
                worldPos.y += _GrassHeight * isGrass;
                worldPos.xz += windEffect * isGrass;

                output.positionHCS = TransformWorldToHClip(worldPos);
                output.uv = input.uv;
                output.worldPos = worldPos;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float height = length(input.worldPos) - _PlanetRadius;
                float t = saturate((height - _MinHeight) / (_MaxHeight - _MinHeight));
                half4 col = SAMPLE_TEXTURE2D(_GradientTex, sampler_GradientTex, float2(t, 0));

                return col; // Keep the terrain color the same
            }
            ENDHLSL
        }
    }
}
