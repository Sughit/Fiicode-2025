Shader "Custom/ScanningEffectObject"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ScanBaseHeight ("Scan Base Height", Float) = 1.0
        _ScanBandWidth ("Scan Band Width", Float) = 0.2
        _ScanColor ("Scan Color", Color) = (0,1,1,1)
        _ScanIntensity ("Scan Intensity", Range(0,1)) = 1.0
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        
        Pass 
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
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
                fixed4 col = tex2D(_MainTex, i.uv);
                
                float diff = abs(i.worldPos.y - _ScanBaseHeight);
                float band = 1.0 - smoothstep(0.0, _ScanBandWidth, diff);
                
                col = lerp(col, _ScanColor, band * _ScanIntensity);
                return col;
            }
            ENDCG
        }
    }
}
