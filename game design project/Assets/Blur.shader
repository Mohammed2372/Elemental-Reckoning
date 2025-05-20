Shader "Custom/Blur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurAmount ("Blur Amount", Range(0, 10)) = 1
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

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _BlurAmount;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = fixed4(0,0,0,0);
                float blur = _BlurAmount * 0.001;

                // Simple box blur
                col += tex2D(_MainTex, i.uv + float2(-blur, -blur));
                col += tex2D(_MainTex, i.uv + float2(-blur, 0));
                col += tex2D(_MainTex, i.uv + float2(-blur, blur));
                col += tex2D(_MainTex, i.uv + float2(0, -blur));
                col += tex2D(_MainTex, i.uv);
                col += tex2D(_MainTex, i.uv + float2(0, blur));
                col += tex2D(_MainTex, i.uv + float2(blur, -blur));
                col += tex2D(_MainTex, i.uv + float2(blur, 0));
                col += tex2D(_MainTex, i.uv + float2(blur, blur));

                return col / 9;
            }
            ENDCG
        }
    }
} 