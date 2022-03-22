Shader "Unity Shaders Book/Chapter 6/Diffuse Vertex-Level"
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
				float3 color		: COLOR0;
			};
			
			// 逐顶点光照
			// 在vs阶段计算法线、light，并归一化，计算最终color（ambient + diffuse）
			Varyings vert(Attributes v)
			{
				Varyings o;
				// Transform the vertex from object space to projection space
				o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
				
				// Get ambient term
				half3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz;
				
				// Transform the normal from object space to world space				
				half3 worldNormal = TransformObjectToWorldNormal(v.normalOS, true);

				// Get the light direction in world space
				half3 worldLight = normalize(_MainLightPosition.xyz);

				// Compute diffuse term
				half3 diffuse = _MainLightColor.rgb * _Diffuse.rgb * saturate(dot(worldNormal, worldLight));
				
				o.color = ambient + diffuse;
				
				return o;
			}
			
			half4 frag(Varyings i) : SV_Target
			{
				return half4(i.color, 1.0);
			}
			
			ENDHLSL
		}
	}
}
