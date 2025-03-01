Shader "Custom/ScanningEffectObject"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {} 
        // The dynamic base height provided by ScanningManager.
        _ScanBaseHeight ("Scan Base Height", Float) = 1.0
        // Thickness (in world units) of the scanning band.
        _ScanBandWidth ("Scan Band Width", Float) = 0.2
        // The matte scan color.
        _ScanColor ("Scan Color", Color) = (0,1,1,1)
        // Controls how strongly the band appears.
        _ScanIntensity ("Scan Intensity", Range(0,1)) = 1.0
    }
    
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100
        
        // Enable standard alpha blending and disable ZWrite for proper transparency.
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _ScanBaseHeight;
            float _ScanBandWidth;
            float4 _ScanColor;
            float _ScanIntensity;
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f 
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
            };
            
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                // We're ignoring the underlying texture here since we want full transparency outside the band.
                // Compute the absolute difference between the fragment's world Y and the scan base.
                float diff = abs(i.worldPos.y - _ScanBaseHeight);
                // Create a smooth mask: fragments near _ScanBaseHeight (within _ScanBandWidth) get a high mask value.
                float band = 1.0 - smoothstep(0.0, _ScanBandWidth, diff);
                
                // Output the scan color with an alpha equal to band * intensity.
                fixed4 col = _ScanColor;
                col.a = band * _ScanIntensity;
                return col;
            }
            ENDCG
        }
    }
}
