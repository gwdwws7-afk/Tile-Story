Shader "Unlit/RadialGradientSquare"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _ColorFrom("Color From", Color) = (1, 0, 1, 1)
        _ColorTo("Color To", Color) = (1, 1, 0, 1)
        _GradientHeight("Height", Range(0, 1)) = 0.3
        _GradientPivot("Position", Vector) = (0, 0, 0, 0)
    }
        SubShader
        {
            Tags { "Queue" = "Transparent" }

            Blend SrcAlpha OneMinusSrcAlpha

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
                float4 _ColorFrom;
                float4 _ColorTo;
                float4 _GradientPivot;
                float _GradientHeight;

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv;
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    fixed2 pixelPoint = fixed2(i.uv.x, i.uv.y);
                    if (pixelPoint.y > _GradientPivot.y + _GradientHeight/2)
                    {
                        return _ColorFrom;
                    }
                    else if (pixelPoint.y < _GradientPivot.y - _GradientHeight / 2)
                    {
                        return _ColorTo;
                    }
                    else {
                        float yDist = (pixelPoint.y - _GradientPivot.y + _GradientHeight / 2) / _GradientHeight;

                        fixed4 col = lerp(_ColorTo, _ColorFrom, yDist);
                        return col;
                    }
                    
                }
                ENDCG
            }
        }
}
