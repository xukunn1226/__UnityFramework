Shader "Unity Shaders Book/Chapter 8/Blend Operations 0"
{
	Properties
	{
		_Color ("Color Tint", Color) = (1, 1, 1, 1)
		_MainTex ("Main Tex", 2D) = "white" {}
		_AlphaScale ("Alpha Scale", Range(0, 1)) = 1
	}

	SubShader
	{
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "RenderPipeline"="UniversalRenderPipeline"}
		
		Pass
		{
			Tags { "LightMode"="UniversalForward" }
			
			ZWrite Off
			
			Blend SrcAlpha OneMinusSrcAlpha, One Zero
			
			HLSLPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag			
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
						
			TEXTURE2D(_MainTex);
			SAMPLER(sampler_MainTex);

			CBUFFER_START(UnityPerMaterial)
			half4 _Color;
			float4 _MainTex_ST;
			half _AlphaScale;
			CBUFFER_END
			
			struct Attributes
			{
				float4 positionOS	: POSITION;
				float3 normalOS		: NORMAL;
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

			 	o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
			 	
			 	return o;
			}
			
			half4 frag(Varyings i) : SV_Target
			{
				half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
			 	
				return half4(texColor.rgb * _Color.rgb, texColor.a * _AlphaScale);
			}
			
			ENDHLSL
		}
	}
}
