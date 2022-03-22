Shader "Unity Shaders Book/Chapter 7/Ramp Texture"
{
	Properties
	{
		_Color ("Color Tint", Color) = (1, 1, 1, 1)
		_RampTex ("Ramp Tex", 2D) = "white" {}
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
			
			TEXTURE2D(_RampTex);
			SAMPLER(sampler_RampTex);

			CBUFFER_START(UnityPerMaterial)
			half4 _Color;
			float4 _RampTex_ST;
			half4 _Specular;
			float _Gloss;
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
				
				o.uv = TRANSFORM_TEX(v.texcoord, _RampTex);
				
				return o;
			}
			
			half4 frag(Varyings i) : SV_Target
			{
				half3 worldNormal = normalize(i.normalWS);
				half3 worldLightDir = normalize(_MainLightPosition.xyz);
				
				half3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz;
				
				// Use the texture to sample the diffuse color
				half halfLambert  = 0.5 * dot(worldNormal, worldLightDir) + 0.5;
				half3 diffuseColor = SAMPLE_TEXTURE2D(_RampTex, sampler_RampTex, half2(halfLambert, halfLambert)).rgb * _Color.rgb;
				
				half3 diffuse = _MainLightColor.rgb * diffuseColor;
				
				half3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.positionWS.xyz);
				half3 halfDir = normalize(worldLightDir + viewDir);
				half3 specular = _MainLightColor.rgb * _Specular.rgb * pow(saturate(dot(worldNormal, halfDir)), _Gloss);
				
				return half4(ambient + diffuse + specular, 1.0);
			}
			
			ENDHLSL
		}
	}
}
