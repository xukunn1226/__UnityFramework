Shader "Unity Shaders Book/Chapter 11/Scrolling Background"
{
	Properties
	{
		_MainTex ("Base Layer (RGB)", 2D) = "white" {}
		_DetailTex ("2nd Layer (RGB)", 2D) = "white" {}
		_ScrollX ("Base layer Scroll Speed", Float) = 1.0
		_Scroll2X ("2nd layer Scroll Speed", Float) = 1.0
		_Multiplier ("Layer Multiplier", Float) = 1
	}
	
	SubShader
	{
		Tags { "RenderType"="Opaque" "Queue"="Geometry" "RenderPipeline"="UniversalRenderPipeline"}
		
		Pass
		{ 
			Tags { "LightMode"="UniversalForward" }
			
			HLSLPROGRAM			
			#pragma vertex vert
			#pragma fragment frag			
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			
			CBUFFER_START(UnityPerMaterial)
			sampler2D _MainTex;
			sampler2D _DetailTex;
			float4 _MainTex_ST;
			float4 _DetailTex_ST;
			float _ScrollX;
			float _Scroll2X;
			float _Multiplier;
			CBUFFER_END
			
			struct Attributes
			{
				float4 positionOS	: POSITION;
				float2 texcoord		: TEXCOORD0;
			};

			struct Varyings
			{
				float4 positionHCS	: SV_POSITION;
				float4 uv			: TEXCOORD0;
			};			
			
			Varyings vert (Attributes v)
			{
				Varyings o;
				o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
				
				o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex) + frac(float2(_ScrollX, 0.0) * _Time.y);
				o.uv.zw = TRANSFORM_TEX(v.texcoord, _DetailTex) + frac(float2(_Scroll2X, 0.0) * _Time.y);
				
				return o;
			}
			
			half4 frag (Varyings i) : SV_Target
			{
				half4 firstLayer = tex2D(_MainTex, i.uv.xy);
				half4 secondLayer = tex2D(_DetailTex, i.uv.zw);
				
				half4 c = lerp(firstLayer, secondLayer, secondLayer.a);
				c.rgb *= _Multiplier;
				
				return c;
			}
			
			ENDHLSL
		}
	}
}
