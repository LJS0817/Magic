Shader "Magic/Dissolve"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        [Header(Dissolve)]
        _NoiseScale ("Noise Scale", Float) = 15.0
        _DissolveAmount ("Dissolve Amount", Range(0, 1)) = 0
        
        [Header(Edge)]
        [HDR] _EdgeColor ("Edge Color", Color) = (1, 1, 0, 1)
        _EdgeWidth ("Edge Width", Range(0, 0.2)) = 0.05
        
        // Required for Sprite Renderer / UI / Line Renderer
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
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
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

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
            float4 _ClipRect;
            float4 _MainTex_ST;

            float _NoiseScale;
            float _DissolveAmount;
            
            float4 _EdgeColor;
            float _EdgeWidth;

            // 프로시저럴 노이즈 생성 함수들
            inline float random(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453123);
            }

            float valueNoise(float2 uv)
            {
                float2 i = floor(uv);
                float2 f = frac(uv);
                float a = random(i);
                float b = random(i + float2(1.0, 0.0));
                float c = random(i + float2(0.0, 1.0));
                float d = random(i + float2(1.0, 1.0));
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
            }

            float fbm(float2 uv)
            {
                float v = 0.0;
                float a = 0.5;
                float2 shift = float2(100.0, 100.0);
                for (int i = 0; i < 3; ++i) { // 3옥타브 노이즈 (구름처럼 부드럽게)
                    v += a * valueNoise(uv);
                    uv = uv * 2.0 + shift;
                    a *= 0.5;
                }
                return v;
            }

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // 베이스 컬러
                half4 color = tex2D(_MainTex, IN.texcoord) * IN.color;
                
                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                // 디졸브 로직
                // 쉐이더 내부에서 프로시저럴 노이즈 생성 (텍스처 불필요)
                float noiseVal = fbm(IN.texcoord * _NoiseScale);
                
                // 3옥타브 fBm의 최대값(0.875)을 기준으로 0~1로 정규화
                noiseVal = noiseVal / 0.875;
                
                // _DissolveAmount가 0일 때 가장자리 빛이 나타나지 않고, 
                // 1일 때 완전히 사라지도록 임계값(threshold) 재계산
                float threshold = lerp(-_EdgeWidth - 0.01, 1.01, _DissolveAmount);
                float clipVal = noiseVal - threshold;
                
                clip(clipVal); 

                // 경계선(Edge) 효과 추가
                // clipVal이 0에 가까울수록(사라지기 직전) 빛나게 함
                if (clipVal < _EdgeWidth)
                {
                    float edgeFactor = 1.0 - (clipVal / _EdgeWidth);
                    color.rgb += _EdgeColor.rgb * edgeFactor * _EdgeColor.a;
                }

                return color;
            }
            ENDCG
        }
    }
}
