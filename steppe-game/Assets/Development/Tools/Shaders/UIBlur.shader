Shader "UI/Blur"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _BlurSize ("Blur Size", Range(0, 10)) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "Default"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float4 _MainTex_TexelSize;
            float _BlurSize;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                OUT.texcoord = v.texcoord;

                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord;
                float2 pixelSize = _MainTex_TexelSize.xy * _BlurSize * 0.5;
                half4 color = half4(0, 0, 0, 0);
                float totalWeight = 0.0;
                
                // Увеличенное количество сэмплов для плавного размытия
                // Используем два кольца сэмплирования
                
                // Центральная точка
                color += tex2D(_MainTex, uv) * 4.0;
                totalWeight += 4.0;
                
                // Внутреннее кольцо (8 точек)
                for (int i = 0; i < 8; i++)
                {
                    float angle = i * 3.14159 * 2.0 / 8.0;
                    float2 offset = float2(cos(angle), sin(angle)) * pixelSize;
                    color += tex2D(_MainTex, uv + offset) * 2.0;
                    totalWeight += 2.0;
                }
                
                // Среднее кольцо (8 точек)
                for (int j = 0; j < 8; j++)
                {
                    float angle = (j + 0.5) * 3.14159 * 2.0 / 8.0;
                    float2 offset = float2(cos(angle), sin(angle)) * pixelSize * 2.0;
                    color += tex2D(_MainTex, uv + offset) * 1.0;
                    totalWeight += 1.0;
                }
                
                // Внешнее кольцо (16 точек) - только если размытие сильное
                if (_BlurSize > 2.0)
                {
                    for (int k = 0; k < 16; k++)
                    {
                        float angle = k * 3.14159 * 2.0 / 16.0;
                        float2 offset = float2(cos(angle), sin(angle)) * pixelSize * 3.0;
                        color += tex2D(_MainTex, uv + offset) * 0.5;
                        totalWeight += 0.5;
                    }
                }
                
                color /= totalWeight;
                color *= IN.color;
                return color;
            }
            ENDCG
        }
    }
}