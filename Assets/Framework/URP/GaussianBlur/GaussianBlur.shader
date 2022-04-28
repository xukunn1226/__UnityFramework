Shader "Hidden/MyURP/GaussianBlur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
		HLSLINCLUDE
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

		#pragma enable_d3d11_debug_symbols
		
		TEXTURE2D(_MainTex);
		SAMPLER(sampler_MainTex);

		half4 _MainTex_TexelSize;
		float _BlurSize;
		
		struct Attributes
		{
			float4 positionOS 	: POSITION;
			float2 texcoord		: TEXCOORD;
		};

		struct Varyings
        {
			float4 positionWS 	: SV_POSITION;
			half2 uv[5]			: TEXCOORD0;
		};
        
		Varyings vertBlurVertical(Attributes v)
        {
			Varyings o;
			o.positionWS = TransformObjectToHClip(v.positionOS.xyz);
			
			half2 uv = v.texcoord;
			
			o.uv[0] = uv;
			o.uv[1] = uv + float2(0.0, _MainTex_TexelSize.y * 1.0) * _BlurSize;
			o.uv[2] = uv - float2(0.0, _MainTex_TexelSize.y * 1.0) * _BlurSize;
			o.uv[3] = uv + float2(0.0, _MainTex_TexelSize.y * 2.0) * _BlurSize;
			o.uv[4] = uv - float2(0.0, _MainTex_TexelSize.y * 2.0) * _BlurSize;
					 
			return o;
		}
		
		Varyings vertBlurHorizontal(Attributes v)
        {
			Varyings o;
			o.positionWS = TransformObjectToHClip(v.positionOS.xyz);
			
			half2 uv = v.texcoord;
			
			o.uv[0] = uv;
			o.uv[1] = uv + float2(_MainTex_TexelSize.x * 1.0, 0.0) * _BlurSize;
			o.uv[2] = uv - float2(_MainTex_TexelSize.x * 1.0, 0.0) * _BlurSize;
			o.uv[3] = uv + float2(_MainTex_TexelSize.x * 2.0, 0.0) * _BlurSize;
			o.uv[4] = uv - float2(_MainTex_TexelSize.x * 2.0, 0.0) * _BlurSize;
					 
			return o;
		}
		
		half4 fragBlur(Varyings i) : SV_Target
        {
			float weight[3] = {0.4026, 0.2442, 0.0545};
			
			half3 sum = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv[0]).rgb * weight[0];
			
			for (int it = 1; it < 3; it++)
			{
				sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv[it*2-1]).rgb * weight[it];
				sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv[it*2]).rgb * weight[it];
			}
			
			return half4(sum, 1.0);
		}
        
		ENDHLSL
		
		ZTest Always Cull Off ZWrite Off
		
		Pass
        {			
			HLSLPROGRAM
			  
			#pragma vertex vertBlurVertical
			#pragma fragment fragBlur
			  
			ENDHLSL
		}
		
		Pass
        {			
			HLSLPROGRAM
			
			#pragma vertex vertBlurHorizontal
			#pragma fragment fragBlur
			
			ENDHLSL
		}
	}
}
