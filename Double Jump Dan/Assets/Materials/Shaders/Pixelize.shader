Shader "DJD/Pixelize"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        [Range(0, 1)]
        _PixelAmount ("Pixel Amount", Range(0, 1)) = 0

        _MinPixelSize ("Min Pixel Size", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque"}
        ZWrite Off
        Cull Off
        ZTest Always

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
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

            float _PixelAmount;
            float _MinPixelSize;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float blockSize = lerp(1, _MinPixelSize * 2, _PixelAmount);
                float2 screenPixel = i.uv * _MainTex_TexelSize.zw;
                float2 snapped = floor(screenPixel / blockSize) * blockSize;
                float2 snappedUV = snapped * _MainTex_TexelSize.xy;

                return tex2D(_MainTex, snappedUV);
            }
            ENDCG
        }
    }
}
