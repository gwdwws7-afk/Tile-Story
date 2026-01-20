Shader "Custom_Shader/Outlight"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _LightColor("Light Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _Size("Size", Int) = 1
        _Step("Step", Int) = 1
    }

        SubShader{

            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGINCLUDE
            #include "UnityCG.cginc"

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv: TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            half4 _MainTex_TexelSize;
            float4 _LightColor;
            int _Size;
            int _Step;

            ENDCG

            Pass {

                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"

                v2f vert(appdata_img IN)
                {
                    v2f OUT;
                    OUT.pos = UnityObjectToClipPos(IN.vertex);
                    OUT.uv = TRANSFORM_TEX(IN.texcoord, _MainTex);
                    return OUT;
                }

                fixed4 frag(v2f IN) : SV_Target
                {
                    fixed4 color = tex2D(_MainTex, IN.uv);
                    if (_Size == 1)
                        return color;


                    fixed4 c = _LightColor;
                    float sum = tex2D(_MainTex, IN.uv).a;
                    for (int i = -_Size; i <= _Size; i+= _Step) {
                        for (int j = -_Size; j <= _Size; j+= _Step) {
                            //if (abs(i) != abs(j))
                            {
                                sum += tex2D(_MainTex, IN.uv + _MainTex_TexelSize.xy * half2(i, j)).a;
                            }
                        }
                    }

                    c.a = sum / (_Size * _Size * 2) * c.a * _Step * _Step;

                    return color.a * color + (1 - color.a) * c;
                    //return step(0.1, color.a) * color + step(0.1, 1 - color.a) * c;

                }

                ENDCG
            }
        }
}