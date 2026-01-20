Shader "Unlit/RadialGradient"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _ColorFrom("Color From", Color) = (1, 0, 1, 1)
        _ColorTo("Color To", Color) = (1, 1, 0, 1)
        _RadiusMultiplier("Radius", Range(0, 5)) = 1
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
                float _RadiusMultiplier;
                float4 _GradientPivot;

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
                    float xDist = pow(pixelPoint.x - _GradientPivot.x, 2);
                    float yDist = pow(pixelPoint.y - _GradientPivot.y, 2);
                    float dist = sqrt(xDist + yDist) / _RadiusMultiplier;
                    dist = clamp(dist, 0, 1);
                    fixed4 col = lerp(_ColorFrom, _ColorTo, dist);
                    return col;
                }
                ENDCG
            }
        }
}
