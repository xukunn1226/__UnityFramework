Shader "Unity Shaders Book/Chapter 8/Alpha Blend With Both Side"
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
			Tags { "LightMode"="SRPDefaultUnlit" }
			
			// First pass renders only back faces 
			Cull Front
			
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha
			
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
				float3 normalWS		: TEXCOORD0;
				float3 positionWS	: TEXCOORD1;
				float2 uv			: TEXCOORD2;
			};

			Varyings vert(Attributes v)
			{
				Varyings o;
				o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
				
				o.normalWS = TransformObjectToWorldNormal(v.normalOS, true);

				o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
				
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				
				return o;
			}

			half4 frag(Varyings i) : SV_Target
			{
				half3 worldNormal = normalize(i.normalWS);
				half3 worldLightDir = normalize(_MainLightPosition.xyz);
				
				half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
				
				half3 albedo = texColor.rgb * _Color.rgb;
				
				half3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz * albedo;
				
				half3 diffuse = _MainLightColor.rgb * albedo * saturate(dot(worldNormal, worldLightDir));
				
				return half4(ambient + diffuse, texColor.a * _AlphaScale);
			}			
			
			ENDHLSL
		}
		
		Pass
		{
			Tags { "LightMode"="UniversalForward" }
			
			// Second pass renders only front faces 
			Cull Back
			
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha
			
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
				float3 normalWS		: TEXCOORD0;
				float3 positionWS	: TEXCOORD1;
				float2 uv			: TEXCOORD2;
			};

			Varyings vert(Attributes v)
			{
				Varyings o;
				o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
				
				o.normalWS = TransformObjectToWorldNormal(v.normalOS, true);

				o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
				
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				
				return o;
			}

			half4 frag(Varyings i) : SV_Target
			{
				half3 worldNormal = normalize(i.normalWS);
				half3 worldLightDir = normalize(_MainLightPosition.xyz);
				
				half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
				
				half3 albedo = texColor.rgb * _Color.rgb;
				
				half3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz * albedo;
				
				half3 diffuse = _MainLightColor.rgb * albedo * saturate(dot(worldNormal, worldLightDir));
				
				return half4(ambient + diffuse, texColor.a * _AlphaScale);
			}			
			
			ENDHLSL
		}
	}
}
