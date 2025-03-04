Shader "Custom/PlanetHeightURPShader_Grass"
{
    Properties
    {
        // Proprietăți originale:
        _MainTex ("Textura Principală", 2D) = "white" {}
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

        // Proprietăți noi pentru efectul de iarbă:
        _GrassTex ("Textura de Iarbă", 2D) = "white" {}
        _GradientMin ("Interval Gradient - Min", Color) = (0,0.5,0,1)
        _GradientMax ("Interval Gradient - Max", Color) = (0,1,0,1)
        _WindSpeed ("Viteza Vântului", Float) = 1.0
        _WindStrength ("Intensitatea Vântului", Float) = 0.05
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            Name "FORWARD"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // Structurile folosite – păstrează orice câmpuri erau deja definite în shaderul original.
            struct Attributes
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float3 worldPos   : TEXCOORD1;
            };

            // Blocul de constant: se includ proprietățile originale și noile proprietăți pentru iarbă.
            CBUFFER_START(UnityPerMaterial)
                TEXTURE2D(_MainTex);
                SAMPLER(sampler_MainTex);

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
                float4 _FoamColor;

                // Proprietăți pentru iarbă
                TEXTURE2D(_GrassTex);
                SAMPLER(sampler_GrassTex);
                float4 _GradientMin;
                float4 _GradientMax;
                float _WindSpeed;
                float _WindStrength;
            CBUFFER_END

            // Funcția vertex – păstrată identică cu cea originală.
            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.vertex);
                OUT.uv = IN.uv;
                OUT.worldPos = TransformObjectToWorld(IN.vertex).xyz;
                return OUT;
            }

            // Funcția fragment extinsă:
            half4 frag (Varyings IN) : SV_Target
            {
                // --- Calculul original al culorii ---
                // Se preia culoarea de bază și culoarea gradientului
                half4 baseColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                half4 gradColor = SAMPLE_TEXTURE2D(_GradientTex, sampler_GradientTex, IN.uv);
                
                // Se calculează factorul de înălțime. Exemplu:
                // Calculăm diferența dintre distanța de la centru și raza planetei,
                // apoi remapăm rezultatul folosind _MinHeight și _MaxHeight.
                float height = length(IN.worldPos) - _PlanetRadius;
                float heightFactor = saturate((height - _MinHeight) / (_MaxHeight - _MinHeight));
                
                // Se face blending între textura de bază și gradient, în funcție de heightFactor.
                half4 originalColor = lerp(baseColor, gradColor, heightFactor);

                // --- (Aici ar urma eventuale calcule pentru apă și spumă, folosind _WaterHeight, _WaveStrength, _WaveScale, _WaveSpeed, _FoamThreshold, _FoamStrength și _FoamColor) ---

                // --- Adăugarea efectului de iarbă ---
                // Se calculează o mască pe baza canalului verde din originalColor.
                // Textura de iarbă se va aplica doar pe zonele unde valoarea canalului verde se încadrează între _GradientMin.g și _GradientMax.g.
                float mask = smoothstep(_GradientMin.g, _GradientMax.g, originalColor.g) -
                             smoothstep(_GradientMax.g, _GradientMax.g + 0.1, originalColor.g);
                mask = saturate(mask);

                // Efectul de vânt: se modifică coordonatele UV ale texturii de iarbă.
                float wind = sin(_Time.y * _WindSpeed + IN.worldPos.x * 5.0) * _WindStrength;
                float2 grassUV = IN.uv + float2(wind, wind);
                half4 grassColor = SAMPLE_TEXTURE2D(_GrassTex, sampler_GrassTex, grassUV);

                // Se combină rezultatul original cu textura de iarbă folosind masca calculată.
                half4 finalColor = lerp(originalColor, grassColor, mask);

                return finalColor;
            }
            ENDHLSL
        }
    }
    FallBack "Universal Forward"
}
