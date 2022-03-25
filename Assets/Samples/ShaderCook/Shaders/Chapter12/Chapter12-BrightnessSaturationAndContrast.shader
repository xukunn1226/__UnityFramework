Shader "Unity Shaders Book/Chapter 12/Brightness Saturation And Contrast"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Brightness ("Brightness", Float) = 1
		_Saturation("Saturation", Float) = 1
		_Contrast("Contrast", Float) = 1
	}
	
	SubShader
	{
		Pass
		{  
			ZTest Always Cull Off ZWrite Off
			
			HLSLPROGRAM  
			#pragma vertex vert  
			#pragma fragment frag			  
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			
			CBUFFER_START(UnityPerMaterial)
			sampler2D _MainTex;  
			half _Brightness;
			half _Saturation;
			half _Contrast;
			CBUFFER_END
			  
			struct Attributes
			{
				float4 positionOS	: POSITION;
				float2 texcoord		: TEXCOORD0;
			};

			struct Varyings
			{
				float4 positionHCS	: SV_POSITION;
				float2 uv			: TEXCOORD0;
			};			
			
			Varyings vert(Attributes v)
			{
				Varyings o;
				
				o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
				
				o.uv = v.texcoord;
						 
				return o;
			}
		
			half4 frag(Varyings i) : SV_Target
			{
				half4 renderTex = tex2D(_MainTex, i.uv);  
				  
				// Apply brightness
				half3 finalColor = renderTex.rgb * _Brightness;
				
				// Apply saturation
				half luminance = 0.2125 * renderTex.r + 0.7154 * renderTex.g + 0.0721 * renderTex.b;
				half3 luminanceColor = half3(luminance, luminance, luminance);
				finalColor = lerp(luminanceColor, finalColor, _Saturation);
				
				// Apply contrast
				half3 avgColor = half3(0.5, 0.5, 0.5);
				finalColor = lerp(avgColor, finalColor, _Contrast);
				
				return half4(finalColor, renderTex.a);  
			}  
			  
			ENDHLSL
		}  
	}
}
