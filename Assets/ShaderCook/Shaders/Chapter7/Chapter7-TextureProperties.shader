Shader "Unity Shaders Book/Chapter 7/Texture Properties"
{
	Properties
	{
		_MainTex ("Main Tex", 2D) = "white" {}
	}

	SubShader
	{
		Tags { "RenderType" = "Opaque" "RenderPipeline"="UniversalRenderPipeline" }

		Pass
		{ 
			Tags { "LightMode"="UniversalForward" }
		
			HLSLPROGRAM			
			#pragma vertex vert
			#pragma fragment frag
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			struct Attributes
			{
				float4 positionOS 	: POSITION;
				float4 texcoord 	: TEXCOORD0;
			};

			struct Varyings
			{
				float4 positionHCS 	: SV_POSITION;
				float2 uv 			: TEXCOORD0;
			};

			TEXTURE2D(_MainTex);
			SAMPLER(sampler_MainTex);
			
			CBUFFER_START(UnityPerMaterial)
			float4 _MainTex_ST;
			CBUFFER_END
			
			Varyings vert(Attributes v)
			{
			 	Varyings o;
			 	// Transform the vertex from object space to projection space
			 	o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);

			 	o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
			 	
			 	return o;
			}
			
			half4 frag(Varyings i) : SV_Target
			{
				half4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

				return half4(c.rgb, 1.0);
			}
			
			ENDHLSL
		}
	}
}
