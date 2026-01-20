// jave.lin 2022/06/01 : 带方向的 2D 溶解

Shader "Test/2DVerticleDissolve_ETC1"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _AlphaTex("Sprite Alpha Texture", 2D) = "white" {}
        _DissolveTex("Disslove Tex", 2D) = "white" {}
        _DissolveScale("Dissolove Scale", Float) = 40
        _DissolveNoiseSpeed("Dissove Noise Speed", Float) = 0
        _DissolvePos("Dissolove Pos", Float) = 0.5
        _DissolveWidth("Dissolve Witdh", Float) = 0.2
        _DissolveEdgeRampTex("Dissolve Edge Ramp Tex", 2D) = "white" {}
        _DissolveEdgeRampTintedControlTex("Dissolve Edge Ramp Tinted Control Tex", 2D) = "white" {}
        [Toggle] _DISSOLVE_NOISE_PROCEDURE("Dissolve Noise Procedure", int) = 0
        [Toggle(_DISSOLVE_EDGE_TINTED_OFF)] _DISSOLVE_EDGE_TINTED_OFF("Dissolve Edge Ramp Tinted Off", int) = 0
        _DissolveEdgeBrightness("Dissolve Edge Brightness", Range(0, 5)) = 1
    }
        SubShader
        {
            Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
            Pass
            {
                Cull Off
                ZWrite Off
                Blend SrcAlpha OneMinusSrcAlpha
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma shader_feature _ _DISSOLVE_NOISE_PROCEDURE_ON
                #pragma shader_feature _ _DISSOLVE_EDGE_TINTED_OFF
                #include "UnityCG.cginc"
                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };
                struct v2f
                {
                    float4 vertex : SV_POSITION;
                    float4 uv : TEXCOORD0;
                };
                sampler2D _MainTex;
                sampler2D _AlphaTex;
                float4 _MainTex_ST;
                float _DissolveScale;
                float _DissolveNoiseSpeed;
                fixed _DissolvePos;
                fixed _DissolveWidth;
                sampler2D _DissolveEdgeRampTex;
                sampler2D _DissolveEdgeRampTintedControlTex;
                sampler2D _DissolveTex;
                float4 _DissolveTex_ST;
                float _DissolveEdgeBrightness;

                // simple noise start ========================
                inline float unity_noise_randomValue(float2 uv)
                {
                    return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
                }

                inline float unity_noise_interpolate(float a, float b, float t)
                {
                    return (1.0 - t) * a + (t * b);
                }

                inline float unity_valueNoise(float2 uv)
                {
                    float2 i = floor(uv);
                    float2 tinted = frac(uv);
                    tinted = tinted * tinted * (3.0 - 2.0 * tinted);

                    uv = abs(frac(uv) - 0.5);
                    float2 c0 = i + float2(0.0, 0.0);
                    float2 c1 = i + float2(1.0, 0.0);
                    float2 c2 = i + float2(0.0, 1.0);
                    float2 c3 = i + float2(1.0, 1.0);
                    float r0 = unity_noise_randomValue(c0);
                    float r1 = unity_noise_randomValue(c1);
                    float r2 = unity_noise_randomValue(c2);
                    float r3 = unity_noise_randomValue(c3);

                    float bottomOfGrid = unity_noise_interpolate(r0, r1, tinted.x);
                    float topOfGrid = unity_noise_interpolate(r2, r3, tinted.x);
                    float t = unity_noise_interpolate(bottomOfGrid, topOfGrid, tinted.y);
                    return t;
                }

                void Unity_SimpleNoise_float(float2 UV, float Scale, out float Out)
                {
                    float t = 0.0;

                    float freq = pow(2.0, float(0));
                    float amp = pow(0.5, float(3 - 0));
                    t += unity_valueNoise(float2(UV.x * Scale / freq, UV.y * Scale / freq)) * amp;

                    freq = pow(2.0, float(1));
                    amp = pow(0.5, float(3 - 1));
                    t += unity_valueNoise(float2(UV.x * Scale / freq, UV.y * Scale / freq)) * amp;

                    freq = pow(2.0, float(2));
                    amp = pow(0.5, float(3 - 2));
                    t += unity_valueNoise(float2(UV.x * Scale / freq, UV.y * Scale / freq)) * amp;

                    Out = t;
                }
                // simple noise end ========================

                // remap start ========================
                void Unity_Remap_float4(float4 In, float2 InMinMax, float2 OutMinMax, out float4 Out)
                {
                    Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
                }
                void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
                {
                    Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
                }
                // remap end ========================

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
                    o.uv.zw = TRANSFORM_TEX(v.uv, _DissolveTex);
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    // jave.lin : simple noise
                        float2 noise_uv = i.uv.zw;// +_Time.xz * _DissolveNoiseSpeed;根据时间的偏移

                    #ifdef _DISSOLVE_NOISE_PROCEDURE_ON
                    // // jave.lin : method1 - 程序化 noise
                    fixed noise;
                    Unity_SimpleNoise_float(noise_uv, _DissolveScale, noise);
                #else
                    // // jave.lin : method2 - 预计算采样 noise
                    fixed noise = tex2D(_DissolveTex, noise_uv * _DissolveScale).r;
                #endif

                    // jave.lin : uv.y 方向溶解
                    float dissolvePos = i.uv.y + noise * _DissolveWidth - _DissolvePos;
                    clip(-dissolvePos);//取反进行剔除
                    // jave.lin : 夹到 0~1
                    Unity_Remap_float(dissolvePos, float2(0, _DissolveWidth), float2(0, 1), dissolvePos);
                    // jave.lin : 着色部分
                    float tinted = step(0.001, dissolvePos) * step(dissolvePos, 0.999);
                    // jave.lin : 混合着色
                    fixed4 col = tex2D(_MainTex, i.uv.xy);
                    fixed AlphaTexAlpha = tex2D(_AlphaTex, i.uv).r;
                    col = fixed4(col.rgb, col.a * AlphaTexAlpha);

                    fixed2 rampUV = fixed2(1 - dissolvePos, 0.5);
                    fixed3 fireCol = tex2D(_DissolveEdgeRampTex, rampUV).rgb;
                    #ifdef _DISSOLVE_EDGE_TINTED_OFF
                        fireCol = fireCol * _DissolveEdgeBrightness;
                    #else
                        fixed mix_col_intensity = tex2D(_DissolveEdgeRampTintedControlTex, rampUV).r;
                        fireCol = lerp(col.rgb, fireCol * _DissolveEdgeBrightness, mix_col_intensity);
                    #endif
                    col.rgb = lerp(col.rgb, fireCol, tinted);
                    return col;
                }
                ENDCG
            }
        }
}

