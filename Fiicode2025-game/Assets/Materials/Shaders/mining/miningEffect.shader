Shader "Custom/MiningEffect"
{
    Properties
    {
        _Color1("Color 1", Color) = (1, 0, 0, 1)
        _Color2("Color 2", Color) = (1, 1, 1, 1)
        _Speed("Rotation Speed", Range(0, 10)) = 1.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Cull Off
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

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
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
            };

            float4 _Color1;
            float4 _Color2;
            float  _Speed;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Calculăm unghiul de rotație în funcție de timp
                float angle = _Time.y * _Speed;
                
                // Centrul rotației (0.5, 0.5) – de regulă la mijlocul UV-urilor
                float2 center = float2(0.5, 0.5);

                // Translatăm UV-urile astfel încât "centrul" să fie la (0,0)
                float2 uv = i.uv - center;

                // Matricea de rotație 2D
                float cosA = cos(angle);
                float sinA = sin(angle);

                // Aplicăm rotația
                float2 rotatedUV;
                rotatedUV.x = uv.x * cosA - uv.y * sinA;
                rotatedUV.y = uv.x * sinA + uv.y * cosA;

                // Reașezăm UV-urile înapoi
                rotatedUV += center;

                // Folosim componenta Y (verticală) pentru a amesteca cele două culori
                // Poți schimba după preferințe (de ex. rotatedUV.x, un radial etc.)
                float t = saturate(rotatedUV.y);

                // Interpolăm între cele două culori
                return lerp(_Color1, _Color2, t);
            }
            ENDCG
        }
    }
}
