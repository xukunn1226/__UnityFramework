Shader "Unity Shaders Book/Chapter 7/Mask Texture"
{
	Properties
	{
		_Color ("Color Tint", Color) = (1, 1, 1, 1)
		_MainTex ("Main Tex", 2D) = "white" {}
		_BumpMap ("Normal Map", 2D) = "bump" {}
		_BumpScale("Bump Scale", Float) = 1.0
		_SpecularMask ("Specular Mask", 2D) = "white" {}
		_SpecularScale ("Specular Scale", Float) = 1.0
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
			
			TEXTURE2D(_MainTex);
			SAMPLER(sampler_MainTex);
			TEXTURE2D(_BumpMap);
			SAMPLER(sampler_BumpMap);
			TEXTURE2D(_SpecularMask);
			SAMPLER(sampler_SpecularMask);

			CBUFFER_START(UnityPerMaterial)
			half4 _Color;
			float4 _MainTex_ST;
			float _BumpScale;
			float _SpecularScale;
			half4 _Specular;
			float _Gloss;
			CBUFFER_END
			
			struct Attributes
			{
				float4 positionOS	: POSITION;
				float3 normalOS		: NORMAL;
				float4 tangentOS	: TANGENT;
				float2 texcoord		: TEXCOORD0;
			};

			struct Varyings
			{
				float4 positionHCS	: SV_POSITION;
				float2 uv			: TEXCOORD0;
				float3 lightDir		: TEXCOORD1;
				float3 viewDir		: TEXCOORD2;
			};
			
			Varyings vert(Attributes v) {
				Varyings o;
				o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
				
				o.uv.xy = v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				
				half3 worldNormal = TransformObjectToWorldNormal(v.normalOS, true);
				half3 worldTangent = TransformObjectToWorldDir(v.tangentOS.xyz);
				half3 worldBinormal = cross(worldNormal, worldTangent) * v.tangentOS.w;
				float3x3 worldToTangent = float3x3(worldTangent, worldBinormal, worldNormal);

				o.lightDir = mul(worldToTangent, _MainLightPosition.xyz);
				o.viewDir = mul(worldToTangent, GetWorldSpaceViewDir(TransformObjectToWorld(v.positionOS.xyz)));

				return o;
			}
			
			half4 frag(Varyings i) : SV_Target
			{
			 	half3 tangentLightDir = normalize(i.lightDir);
				half3 tangentViewDir = normalize(i.viewDir);

				half3 tangentNormal = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, i.uv));
				tangentNormal.xy *= _BumpScale;
				tangentNormal.z = sqrt(1.0 - saturate(dot(tangentNormal.xy, tangentNormal.xy)));

				half3 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv).rgb * _Color.rgb;
				
				half3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz * albedo;
				
				half3 diffuse = _MainLightColor.rgb * albedo * saturate(dot(tangentNormal, tangentLightDir));
				
			 	half3 halfDir = normalize(tangentLightDir + tangentViewDir);
			 	// Get the mask value
			 	half specularMask = SAMPLE_TEXTURE2D(_SpecularMask, sampler_SpecularMask, i.uv).r * _SpecularScale;
			 	// Compute specular term with the specular mask
			 	half3 specular = _MainLightColor.rgb * _Specular.rgb * pow(saturate(dot(tangentNormal, halfDir)), _Gloss) * specularMask;
			
				return half4(ambient + diffuse + specular, 1.0);
			}
			
			ENDHLSL
		}
	}
}
