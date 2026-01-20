Shader "Unlit/RadialGradient_Alpha"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _AlphaFrom("Alpha From", Range(0, 1)) = 0
        _AlphaTo("Alpha To", Range(0, 1)) = 1
        _Radius("Radius", Range(0, 1000)) = 500
        _Position("Position", Vector) = (0, 0, 0, 0)
        _XAxisMultiple("X Axis Multiple",float)=1
        _YAxisMultiple("Y Axis Multiple",float)=1
        _DisThreshold("Distance Threshold",float)=0
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
                float4 _MainTex_ST;
                float _AlphaFrom;
                float _AlphaTo;
                float _Radius;
                float4 _Position;
                float _XAxisMultiple;
                float _YAxisMultiple;
                float _DisThreshold;

                v2f vert(appdata v)
                {
                    v2f o;
                    o.worldPosition = v.vertex;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {        
                    fixed2 pixelPoint = fixed2(i.worldPosition.x, i.worldPosition.y);
                    float xDist = pow(pixelPoint.x - _Position.x, 2);
                    float yDist = pow(pixelPoint.y - _Position.y, 2);
                    float dist = saturate(sqrt(xDist*_XAxisMultiple + yDist*_YAxisMultiple) / _Radius)-_DisThreshold;

                    fixed4 colorTex = tex2D(_MainTex, i.uv);
                    float t=i.uv.y>0.5?(i.uv.y-0.5)*2:(0.5-i.uv.y)*2;
                    fixed4 col=fixed4(colorTex.rgb,lerp(_AlphaFrom,_AlphaTo,t*dist));

                    return col;
                }
                ENDCG
            }
        }
}
