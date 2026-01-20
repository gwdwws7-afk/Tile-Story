Shader "CameraPlay/ShakeEnvelopeStable"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Value ("Speed", Float) = 30
        _Value2 ("X Max Strength", Float) = 0.01
        _Value3 ("Y Max Strength", Float) = 0.01
        _Envelope ("Envelope", Float) = 1
    }
    SubShader
    {
        Pass
        {
            ZTest Always
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _TimeX;
            float _Value;
            float _Value2;
            float _Value3;
            float _Envelope;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float2 texcoord : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                return OUT;
            }

            float4 frag(v2f i) : SV_Target
            {
                float2 uv = i.texcoord.xy;

                // X/Y 振动方向，正弦波稳定震动
                float xOffset = sin(_TimeX * _Value * 10.0) * _Value2 * _Envelope;
                float yOffset = cos(_TimeX * _Value * 12.0) * _Value3 * _Envelope;

                float3 col = tex2D(_MainTex, uv + float2(xOffset, yOffset)).rgb;
                return float4(col, 1.0);
            }
            ENDCG
        }
    }
}
