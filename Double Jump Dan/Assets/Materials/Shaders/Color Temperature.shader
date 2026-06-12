Shader "DJD/Color Temperature"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Temperature ("Color Temperature", Range(-1, 1)) = 0
        _Exposure ("Exposure", Range(0, 3)) = 1
        _TintColor ("Tint Color", Color) = (.5, .5, .5, 1)
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // Texture and parameters
            sampler2D _MainTex;
            float _Temperature;
            float _Exposure;
            float4 _TintColor;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            // Vertex shader
            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            // Fragment shader
            half4 frag(v2f i) : SV_Target
            {
                // Sample texture
                half4 texColor = tex2D(_MainTex, i.uv);

                // Apply color temperature adjustment
                half3 tempColor = texColor.rgb;

                // Adjust based on temperature value
                if (_Temperature > 0) 
                {
                    // Warmer tones (increased red, decreased blue)
                    tempColor.r += _Temperature * 0.5;
                    tempColor.b -= _Temperature * 0.5;
                }
                else
                {
                    // Cooler tones (increased blue, decreased red)
                    tempColor.r += _Temperature * 0.5;
                    tempColor.b -= _Temperature * 0.5;
                }

                // Apply exposure
                texColor.rgb *= pow(2.0, _Exposure);

                // Apply tint color
                texColor.rgb *= _TintColor.rgb;

                return texColor;
            }
            ENDCG//////////////////////////////////asdsadasd
        }
    }
    Fallback "Diffuse"
}