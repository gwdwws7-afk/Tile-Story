Shader "Unlit/RadialGradient_WorldPos"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _ColorFrom("Color From", Color) = (1, 0, 1, 1)
        _ColorTo("Color To", Color) = (1, 1, 0, 1)
        _RadiusMultiplier("Radius", Range(0, 1000)) = 500
        _GradientPivot("Position", Vector) = (0, 0, 0, 0)
        _HighlightPivot("HighlightPos",Vector) = (0,0,0,0)
        _HighlightRadius("HighlightRadius",Range(0,1000))=500
    }
        SubShader
        {
            Tags { "Queue" = "Transparent" }

            Cull Off
            Lighting Off
            ZWrite Off
            ZTest[unity_GUIZTestMode]
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
                    float4 worldPosition : TEXCOORD1;
                };

                sampler2D _MainTex;
                float4 _ColorFrom;
                float4 _ColorTo;
                float _RadiusMultiplier;
                float4 _GradientPivot;
                float4 _HighlightPivot;
                float _HighlightRadius;

                v2f vert(appdata v)
                {
                    v2f o;
                    o.worldPosition = v.vertex;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv;
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    fixed2 pixelPoint = fixed2(i.worldPosition.x, i.worldPosition.y);
                    float xDist = pow(pixelPoint.x - _GradientPivot.x, 2);
                    float yDist = pow(pixelPoint.y - _GradientPivot.y, 2);
                    float dist = sqrt(xDist + yDist) / _RadiusMultiplier;
                    dist = clamp(dist, 0, 1);
                    float4 col = lerp(_ColorFrom, _ColorTo, dist);

                    float xPos = pow(pixelPoint.x - _HighlightPivot.x, 2);
                    float yPos = pow(pixelPoint.y - _HighlightPivot.y, 2);
                    float pos = sqrt(xPos + yPos) / _HighlightRadius + 0.1;
                    pos = clamp(pos, 0, 1);
                    col = lerp(_ColorFrom, col, pos);
                    
                    return col;
                }
                ENDCG
            }
        }
}
