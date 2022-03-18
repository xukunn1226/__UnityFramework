// 升级到URP见https://zhuanlan.zhihu.com/p/369525578
Shader "Unity Shaders Book/Chapter 3/Redify"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
    }
	
	SubShader
	{
		Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline" }
		LOD 200

		Pass
		{
			Tags { "LightMode"="UniversalForward" }

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma shader_feature REDIFY_ON

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			struct Attributes
			{
				float4 positionOS   : POSITION;
				float2 uv           : TEXCOORD0;
			};

			struct Varyings
			{
				float2 uv           : TEXCOORD0;
				float4 positionHCS  : SV_POSITION;
			};

			TEXTURE2D(_MainTex);
			SAMPLER(sampler_MainTex);

			CBUFFER_START(UnityPerMaterial)
				float4 _MainTex_ST;
			CBUFFER_END

			Varyings vert(Attributes i)
			{
				Varyings o;
				o.positionHCS = TransformObjectToHClip(i.positionOS.xyz);
				o.uv = TRANSFORM_TEX(i.uv, _MainTex);
				return o;
			}

			half4 frag(Varyings i) : SV_TARGET
			{
				half4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
				#if REDIFY_ON
				c.gb *= 0.5;
				#endif
				return c;
			}
			ENDHLSL
		}
	}
	CustomEditor "CustomShaderGUI"
}