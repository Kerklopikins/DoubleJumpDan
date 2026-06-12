Shader "DJD/Motion Blur"
{
    Properties
    {
        _MainTex ("Screen Texture", 2D) = "white" {}
        _Intensity ("Blur Intensity", Range(0.0, 1.0)) = 0.5
        _SampleCount ("Sample Count", Int) = 8
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        ZTest Always
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;
            float4x4  _PreviousVP;
            float4x4  _CurrentVPInverse;
            float     _Intensity;
            int       _SampleCount;

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

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
                depth = Linear01Depth(depth);

                float4 ndcPos = float4(i.uv * 2.0 - 1.0, depth * 2.0 - 1.0, 1.0);

                float4 worldPos = mul(_CurrentVPInverse, ndcPos);
                worldPos /= worldPos.w;

                float4 prevClipPos = mul(_PreviousVP, worldPos);
                prevClipPos /= prevClipPos.w;

                float2 velocity = (ndcPos.xy - prevClipPos.xy) * 0.5;
                velocity *= _Intensity;

                float2 uv = i.uv;
                fixed4 col = fixed4(0, 0, 0, 0);

                int samples = clamp(_SampleCount, 1, 64);

                for (int s = 0; s < samples; s++)
                {
                    float t = (float)s / (float)(samples - 1) - 0.5;
                    float2 sampleUV = uv + velocity * t;
                    sampleUV = clamp(sampleUV, 0.0, 1.0);
                    col += tex2D(_MainTex, sampleUV);
                }

                col /= (float)samples;
                return col;
            }
            ENDCG
        }
    }

    FallBack Off
}