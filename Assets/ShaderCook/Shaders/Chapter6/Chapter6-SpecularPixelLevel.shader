Shader "Unity Shaders Book/Chapter 6/Specular Pixel-Level"
{
	Properties
	{
		_Diffuse ("Diffuse", Color) = (1, 1, 1, 1)
		_Specular ("Specular", Color) = (1, 1, 1, 1)
		_Gloss ("Gloss", Range(8.0, 256)) = 20
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
			half4 _Specular;
			float _Gloss;
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
				float3 positionWS	: TEXCOORD1;
			};
			
			Varyings vert(Attributes v)
			{
				Varyings o;
				// Transform the vertex from object space to projection space
				o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
				
				// Transform the normal from object space to world space
				o.normalWS = TransformObjectToWorldNormal(v.normalOS, true);

				// Transform the vertex from object spacet to world space
				o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
				
				return o;
			}
			
			half4 frag(Varyings i) : SV_Target
			{
				// Get ambient term
				half3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz;
				
				half3 worldNormal = normalize(i.normalWS);
				half3 worldLightDir = normalize(_MainLightPosition.xyz);		// 指向光源的方向，而不是光源射向物体的方向
				
				// Compute diffuse term
				half3 diffuse = _MainLightColor.rgb * _Diffuse.rgb * saturate(dot(worldNormal, worldLightDir));
				
				// Get the reflect direction in world space
				half3 reflectDir = normalize(reflect(-worldLightDir, worldNormal));
				// Get the view direction in world space
				half3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.positionWS.xyz);
				// Compute specular term
				half3 specular = _MainLightColor.rgb * _Specular.rgb * pow(saturate(dot(reflectDir, viewDir)), _Gloss);
				
				return half4(ambient + diffuse + specular, 1.0);
			}
			
			ENDHLSL
		}
	}
}
