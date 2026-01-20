Shader "UI/SimpleDiagonalSplit"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        _ColorA ("Color A", Color) = (1,0,0,1)
        _ColorB ("Color B", Color) = (0,0,1,1)
        _SplitAngle ("Split Angle", Range(0, 360)) = 45
    }
    
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }
        
        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            
            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };
            
            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 uv       : TEXCOORD0;
            };
            
            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _ColorA;
            fixed4 _ColorB;
            float _SplitAngle;
            
            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                o.color = v.color * _Color;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // 将角度转换为弧度
                float angleRad = radians(_SplitAngle);
                float slope = tan(angleRad);
                
                // 计算斜线方程: y = slope * (x - 0.5) + 0.5
                float splitLine = slope * (i.uv.x - 0.5) + 0.5;
                
                // 判断当前像素在斜线上方还是下方
                fixed4 color = (i.uv.y > splitLine) ? _ColorA : _ColorB;
                
                // 应用顶点颜色和透明度
                return color * i.color;
            }
            ENDCG
        }
    }
}