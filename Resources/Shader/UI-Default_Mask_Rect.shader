Shader "UI/Default_Mask_Rect"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		
		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255
 
		_ColorMask ("Color Mask", Float) = 15
 
 
		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
 
 
	//-------------------add----------------------
      _Center("Center", vector) = (0, 0, 0, 0)
      _SliderX ("_SliderX", Range (0,1000)) = 1000 // sliders
      _SliderY ("_SliderY", Range (0,1000)) = 1000
	  _Edge("_Edge", Range(0,200)) = 12 // sliders
	  _Radius("_Radius", Range(0,1000)) = 16 // sliders
    //-------------------add----------------------
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
			Name "Default"
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
 
			#include "UnityCG.cginc"
			#include "UnityUI.cginc"
 
			#pragma multi_compile __ UNITY_UI_ALPHACLIP
			
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
			
			fixed4 _Color;
			fixed4 _TextureSampleAdd;
			float4 _ClipRect;
			//-------------------add----------------------
            float _SliderX;
            float _SliderY;
			float _Edge;
			float _Radius;
            float2 _Center;
            //-------------------add----------------------
			v2f vert(appdata_t IN)
			{
				v2f OUT;
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
				OUT.worldPosition = IN.vertex;
				OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
 
				OUT.texcoord = IN.texcoord;
				
				OUT.color = IN.color * _Color;
				return OUT;
			}
 
			sampler2D _MainTex;
 
			fixed4 frag(v2f IN) : SV_Target
			{
				half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
				
				color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
				
				#ifdef UNITY_UI_ALPHACLIP
				clip (color.a - 0.001);
				#endif
				//-------------------add----------------------
				float deltaX = IN.worldPosition.x - _Center.x;
				float deltaY = IN.worldPosition.y - _Center.y;
				bool clear = false;
				float c = 1;
				float xDot = deltaX < 0 ? -1 : 1;
				float yDot = deltaY < 0 ? -1 : 1;
				if (deltaX < (_SliderX + _Edge) && deltaX > -(_SliderX + _Edge) && deltaY < (_SliderY + _Edge) && deltaY > -(_SliderY + _Edge))
				{
					if (deltaX < _SliderX && deltaX > -_SliderX && deltaY < _SliderY && deltaY > -_SliderY) {
						//中心正方形
						c = 0;
					}
					else {
						//边缘处理
						float disX = (deltaX * xDot) - _SliderX;
						if (disX > 0 && disX < _Edge) {
							float temp = disX / _Edge;
							c = temp;
						}
						float disY = (deltaY * yDot) - _SliderY;
						if (disY > 0 && disY < _Edge) {
							float temp = disY / _Edge;
							c = temp;
						}
					}

					//获得圆角裁剪的中心点
					float2 roundCenter = float2(xDot * _SliderX - xDot * _Radius + _Center.x, yDot * _SliderY - yDot * _Radius + _Center.y);
					//确定象限
					if ((IN.worldPosition.x - roundCenter.x)*xDot > 0 && (IN.worldPosition.y - roundCenter.y)*yDot > 0) {
						float dis = distance(IN.worldPosition.xy, roundCenter);
						//象限状态离边缘距离
						if (dis > _Radius) {
							dis = (dis - _Radius);
							if(dis > 0 && dis < _Edge)
								c = dis / _Edge;
							else {
								c = 1;
							}
						}
					}
				}
				color.a *= c;
				color.rgb *= color.a;
               	//-------------------add----------------------
				return color;
			
			}

            bool pointInRect(float2 A,float2 B,float2 C,float2 M)
            {
                
            }
		ENDCG
		}
	}
}