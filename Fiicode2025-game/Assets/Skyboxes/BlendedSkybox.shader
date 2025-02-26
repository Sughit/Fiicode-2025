Shader "Custom/BlendedSkybox"
{
    Properties
    {
        _Cubemap1 ("Day Skybox", CUBE) = "" {}
        _Cubemap2 ("Night Skybox", CUBE) = "" {}
        _Blend ("Blend Factor", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags { "Queue" = "Background" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            samplerCUBE _Cubemap1;
            samplerCUBE _Cubemap2;
            float _Blend;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float3 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float3 texcoord : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 color1 = texCUBE(_Cubemap1, i.texcoord);
                fixed4 color2 = texCUBE(_Cubemap2, i.texcoord);
                
                // Blend based on the _Blend factor
                return lerp(color2, color1, _Blend);
            }
            ENDCG
        }
    }
}
