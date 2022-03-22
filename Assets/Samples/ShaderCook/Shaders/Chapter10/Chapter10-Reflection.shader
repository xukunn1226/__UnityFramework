﻿Shader "Unity Shaders Book/Chapter 10/Reflection"
{
	Properties
	{
		_Color ("Color Tint", Color) = (1, 1, 1, 1)
		_ReflectColor ("Reflection Color", Color) = (1, 1, 1, 1)
		_ReflectAmount ("Reflect Amount", Range(0, 1)) = 1
		_Cubemap ("Reflection Cubemap", Cube) = "_Skybox" {}
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
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RealtimeLights.hlsl"
			
			CBUFFER_START(UnityPerMaterial)
			half4 _Color;
			half4 _ReflectColor;
			half _ReflectAmount;
			samplerCUBE _Cubemap;
			CBUFFER_END
			
			struct Attributes
			{
				float4 positionOS	: POSITION;
				float3 normalOS		: NORMAL;
			};

			struct Varyings
			{
				float4 positionHCS	: SV_POSITION;
				float3 positionWS	: TEXCOORD0;
				half3 normalWS		: TEXCOORD1;
				half3 viewDirWS		: TEXCOORD2;
				half3 ReflWS		: TEXCOORD3;
				float4 shadowCoord	: TEXCOORD4;
			};

			Varyings vert(Attributes v)
			{
				Varyings o;
				
				o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
				
				o.normalWS = TransformObjectToWorldNormal(v.normalOS, true);
				
				o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
				
				o.viewDirWS = normalize(_WorldSpaceCameraPos.xyz - o.positionWS.xyz);
				
				o.ReflWS = reflect(-o.viewDirWS, o.normalWS);
				
				o.shadowCoord = TransformWorldToShadowCoord(o.positionWS);
				
				return o;
			}
			
			half4 frag(Varyings i) : SV_Target
			{
				half3 worldNormal = normalize(i.normalWS);
				half3 worldLightDir = normalize(GetMainLight().direction);
				half3 worldViewDir = normalize(i.viewDirWS);		
				
				half3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz;
				
				half3 diffuse = _MainLightColor.rgb * _Color.rgb * saturate(dot(worldNormal, worldLightDir));
				
				// Use the reflect dir in world space to access the cubemap
				half3 reflection = texCUBE(_Cubemap, i.ReflWS).rgb * _ReflectColor.rgb;
				
				// UNITY_LIGHT_ATTENUATION(atten, i, i.positionWS);
				half3 atten = half3(1.0, 1.0, 1.0);
				
				// Mix the diffuse color with the reflected color
				half3 color = ambient + lerp(diffuse, reflection, _ReflectAmount) * atten;
				
				return half4(color, 1.0);
			}
			
			ENDHLSL
		}
	}
}