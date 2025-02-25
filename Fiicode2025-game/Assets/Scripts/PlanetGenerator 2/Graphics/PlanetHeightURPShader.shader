Shader "Custom/PlanetHeightURPShader"
{
    Properties
    {
        _GradientTex ("Gradient Texture", 2D) = "white" {}
        _PlanetRadius ("Planet Radius", Float) = 1
        _MinHeight ("Minimum Height", Float) = -0.2
        _MaxHeight ("Maximum Height", Float) = 1
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

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
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
