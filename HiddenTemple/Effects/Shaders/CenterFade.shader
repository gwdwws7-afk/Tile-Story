Shader "Custom/CenterFade" 
{
    Properties 
    {
        _MainTex ("Texture", 2D) = "white" {}
        _AlphaFrom("Alpha From", Range(0, 1)) = 0
        _AlphaTo("Alpha To", Range(0, 1)) = 1
        _FadeDistance ("Fade Distance", Range(0, 10)) = 0.5 // 控制渐隐的距离
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

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
 
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
 
            struct appdata 
            {
                float4 vertex : POSITION;
                float4 color    : COLOR;
                float2 uv : TEXCOORD0;
            };
 
            struct v2f 
            {
                float2 uv : TEXCOORD0;
                fixed4 color    : COLOR;
                float4 vertex : SV_POSITION;
                float4 worldPosition : TEXCOORD1;
            };
 
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _ClipRect;
            float _AlphaFrom;
            float _AlphaTo;
            float _FadeDistance;
 
            v2f vert (appdata v) 
            {
                v2f o;
                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color=v.color;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
 
            fixed4 frag (v2f i) : SV_Target 
            {
                fixed4 colorTex = tex2D(_MainTex, i.uv)*i.color;

                #ifdef UNITY_UI_CLIP_RECT
                    colorTex.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                    clip (colorTex.a - 0.001);
                #endif

                float2 center = float2(0.5, 0.5); // 屏幕中心点
                float2 uv = i.uv - center;
                float distanceSquared = dot(uv, uv);
                float fadeFactor = 1.0 - saturate(_FadeDistance * distanceSquared);
                
                return fixed4(colorTex.rgb,colorTex.a*lerp(_AlphaFrom,_AlphaTo,fadeFactor));
            }
            ENDCG
        }
    }
}
