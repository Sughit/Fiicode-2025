Shader "Custom/b_PlanetHeightURPShader"
{
    Properties
    {
        _GradientTex ("Gradient Texture", 2D) = "white" {}
        _PlanetRadius ("Planet Radius", Float) = 1
        _MinHeight ("Minimum Height", Float) = -0.2
        _MaxHeight ("Maximum Height", Float) = 1
        _WaterHeight ("Water Height", Float) = 0.0

        _BubbleStrength ("Bubble Strength", Float) = 0.05
        _BubbleScale ("Bubble Scale", Float) = 5.0
        _BubbleSpeed ("Bubble Speed", Float) = 1.0
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
            float _BubbleStrength;
            float _BubbleScale;
            float _BubbleSpeed;

            // Pseudo-random noise function
            float rand(float3 n) {
                return frac(sin(dot(n, float3(12.9898,78.233, 37.719))) * 43758.5453);
            }

            // Bubble-like circular displacement
            float bubbleNoise(float3 pos, float scale, float speed)
            {
                pos *= scale;
                float3 cell = floor(pos);
                float3 fracPos = frac(pos);

                float bubbles = 0.0;

                [unroll]
                for(int x=-1; x<=1; x++)
                for(int y=-1; y<=1; y++)
                for(int z=-1; z<=1; z++)
                {
                    float3 neighbor = cell + float3(x, y, z);
                    float3 bubblePos = rand(neighbor) * 0.8 + 0.1;
                    float bubbleAnim = frac(_Time.y * speed + rand(neighbor));
                    float3 diff = fracPos - bubblePos;
                    float dist = length(diff);

                    bubbles += smoothstep(0.4, 0.0, dist) * sin(bubbleAnim * 3.1415);
                }

                return bubbles;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                float3 vertexPos = input.positionOS.xyz;

                float vertexHeight = length(vertexPos) - _PlanetRadius;

                // Only apply bubble effect below water level
                float underwater = step(vertexHeight, _WaterHeight);

                float displacement = bubbleNoise(normalize(vertexPos), _BubbleScale, _BubbleSpeed);
                displacement *= _BubbleStrength * underwater;

                vertexPos += normalize(vertexPos) * displacement;

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
