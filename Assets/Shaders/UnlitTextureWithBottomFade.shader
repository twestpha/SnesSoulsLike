Shader "Custom/UnlitTextureWithBottomFade"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _FadeColor ("Fade Color", Color) = (1,1,1,1)
        _FadeHeight ("Fade Height", Range(0,20)) = 0.25
        _FadeOffset ("Fade Offset", Range(0,20)) = 0.25
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _FadeColor;
            float _FadeHeight;
            float _FadeOffset;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float worldY : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldY = mul(unity_ObjectToWorld, v.vertex).y;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Sample texture
                fixed4 col = tex2D(_MainTex, i.uv);

                // Compute fade factor based on worldY position
                float fadeFactor = saturate((i.worldY / _FadeHeight) - _FadeOffset);

                // Lerp from FadeColor to Texture color
                col.rgb = lerp(_FadeColor.rgb, col.rgb, fadeFactor);

                return col;
            }
            ENDCG
        }
    }
}