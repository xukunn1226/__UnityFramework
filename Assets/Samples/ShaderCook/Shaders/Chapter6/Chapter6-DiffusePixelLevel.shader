Shader "Unity Shaders Book/Chapter 6/Diffuse Pixel-Level"
{
	Properties
	{
		_Diffuse ("Diffuse", Color) = (1, 1, 1, 1)
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

			CBUFFER_START(UnityPerMaterial)		
			half4 _Diffuse;
			CBUFFER_END

			struct Attributes
			{
				float4 positionOS   : POSITION;
				float3 normalOS     : NORMAL;
			};

			struct Varyings
			{
				float4 positionHCS  : SV_POSITION;
				float3 normalWS		: TEXCOORD0;
			};
			
			// 逐像素光照
			// 相比逐顶点光照，计算法线并没有归一化，插值后在ps阶段归一化
			Varyings vert(Attributes v)
			{
				Varyings o;
				// Transform the vertex from object space to projection space
				o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);

				// Transform the normal from object space to world space
				o.normalWS = TransformObjectToWorldNormal(v.normalOS, false);

				return o;
			}
			
			half4 frag(Varyings i) : SV_Target
			{
				// Get ambient term
				half3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz;
				
				// Get the normal in world space
				half3 worldNormal = normalize(i.normalWS);
				// Get the light direction in world space
				half3 worldLightDir = normalize(_MainLightPosition.xyz);
				
				// Compute diffuse term
				half3 diffuse = _MainLightColor.rgb * _Diffuse.rgb * saturate(dot(worldNormal, worldLightDir));
				
				half3 color = ambient + diffuse;
				
				return half4(color, 1.0);
			}
			
			ENDHLSL
		}
	}
}
