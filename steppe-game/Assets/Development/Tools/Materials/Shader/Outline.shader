Shader "UI/OutlineSmoothClamped"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth ("Outline Width", Range(0, 20)) = 1
        
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
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

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            fixed4 _OutlineColor;
            float _OutlineWidth;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;

            static const int SAMPLE_COUNT = 16;
            static const float2 SAMPLE_OFFSETS[SAMPLE_COUNT] = 
            {
                float2(1.0, 0.0),
                float2(0.9239, 0.3827),
                float2(0.7071, 0.7071),
                float2(0.3827, 0.9239),
                float2(0.0, 1.0),
                float2(-0.3827, 0.9239),
                float2(-0.7071, 0.7071),
                float2(-0.9239, 0.3827),
                float2(-1.0, 0.0),
                float2(-0.9239, -0.3827),
                float2(-0.7071, -0.7071),
                float2(-0.3827, -0.9239),
                float2(0.0, -1.0),
                float2(0.3827, -0.9239),
                float2(0.7071, -0.7071),
                float2(0.9239, -0.3827)
            };

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                
                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;
                
                return o;
            }

            // Сэмплирование с проверкой границ
            half SampleAlphaSafe(float2 uv)
            {
                // Если UV за пределами [0,1] — возвращаем 0
                if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1)
                    return 0;
                return tex2D(_MainTex, uv).a;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 texelSize = _MainTex_TexelSize.xy * _OutlineWidth;
                
                // Проверяем, находимся ли мы внутри текстуры
                bool insideTexture = (i.uv.x >= 0 && i.uv.x <= 1 && i.uv.y >= 0 && i.uv.y <= 1);
                
                half4 mainColor = insideTexture 
                    ? (tex2D(_MainTex, i.uv) + _TextureSampleAdd) * i.color 
                    : half4(0, 0, 0, 0);
                
                half outlineAlpha = 0;
                
                for (int s = 0; s < SAMPLE_COUNT; s++)
                {
                    float2 sampleUV = i.uv + SAMPLE_OFFSETS[s] * texelSize;
                    outlineAlpha = max(outlineAlpha, SampleAlphaSafe(sampleUV));
                }
                
                fixed4 outlineColorFinal = _OutlineColor;
                outlineColorFinal.a *= outlineAlpha;
                
                fixed4 result = lerp(outlineColorFinal, mainColor, mainColor.a);
                result.a = max(mainColor.a, outlineColorFinal.a);
                
                #ifdef UNITY_UI_CLIP_RECT
                result.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
                #endif

                return result;
            }
            ENDCG
        }
    }
}