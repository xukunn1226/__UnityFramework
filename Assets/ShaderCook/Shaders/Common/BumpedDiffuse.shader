Shader "Unity Shaders Book/Common/Bumped Diffuse"
{
	Properties
	{
		_Color ("Color Tint", Color) = (1, 1, 1, 1)
		_MainTex ("Main Tex", 2D) = "white" {}
		_BumpMap ("Normal Map", 2D) = "bump" {}
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

			CBUFFER_START(UnityPerMaterial)			
			half4 _Color;
			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _BumpMap;
			float4 _BumpMap_ST;
			CBUFFER_END

			struct Attributes
			{
				float4 positionOS	: POSITION;
				float3 normalOS		: NORMAL;
				float4 tangentOS	: TANGENT;
				float4 texcoord		: TEXCOORD0;
			};

			struct Varyings
			{
				float4 positionHCS	: SV_POSITION;
				float4 uv			: TEXCOORD0;
				float4 TtoW0		: TEXCOORD1;
				float4 TtoW1		: TEXCOORD2;
				float4 TtoW2 		: TEXCOORD3;
			};
			
			Varyings vert(Attributes v)
			{
				Varyings o;
				o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
				
				o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.uv.zw = TRANSFORM_TEX(v.texcoord, _BumpMap);
				
				float3 worldPos = TransformObjectToWorld(v.positionOS);
				half3 worldNormal = TransformObjectToWorldNormal(v.normalOS, true);
				half3 worldTangent = TransformObjectToWorldDir(v.tangentOS.xyz);
				half3 worldBinormal = cross(worldNormal, worldTangent) * v.tangentOS.w;

				// float3x3 worldToTangent = float3x3(worldTangent, worldBinormal, worldNormal);
				
				// construct tangent to world matrix
				o.TtoW0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
				o.TtoW1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
				o.TtoW2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);  
				
				// TRANSFER_SHADOW(o);
				
				return o;
			}
			
			half4 frag(Varyings i) : SV_Target
			{
				float3 worldPos = float3(i.TtoW0.w, i.TtoW1.w, i.TtoW2.w);
				half3 lightDir = normalize(_MainLightPosition.xyz);
				half3 viewDir = normalize(GetWorldSpaceViewDir(worldPos));
				
				half3 bump = UnpackNormal(tex2D(_BumpMap, i.uv.zw));
				// method 1.
				// bump = normalize(half3(dot(i.TtoW0.xyz, bump), dot(i.TtoW1.xyz, bump), dot(i.TtoW2.xyz, bump)));

				// method 2.
				float3x3 TW = float3x3(i.TtoW0.xyz, i.TtoW1.xyz, i.TtoW2.xyz);
				bump = mul(TW, bump);
				
				half3 albedo = tex2D(_MainTex, i.uv.xy).rgb * _Color.rgb;
				
				half3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz * albedo;
			
			 	half3 diffuse = _MainLightColor.rgb * albedo * saturate(dot(bump, lightDir));
				
				// UNITY_LIGHT_ATTENUATION(atten, i, worldPos);
				half3 atten = half3(1, 1, 1);
				
				return half4(ambient + diffuse * atten, 1.0);
			}
			
			ENDHLSL
		}
		
// 		Pass
// 		{ 
// 			Tags { "LightMode"="ForwardAdd" }
			
// 			Blend One One
		
// 			CGPROGRAM
			
// 			#pragma multi_compile_fwdadd
// 			// Use the line below to add shadows for point and spot lights
// //			#pragma multi_compile_fwdadd_fullshadows
			
// 			#pragma vertex vert
// 			#pragma fragment frag
			
// 			#include "Lighting.cginc"
// 			#include "AutoLight.cginc"
			
// 			fixed4 _Color;
// 			sampler2D _MainTex;
// 			float4 _MainTex_ST;
// 			sampler2D _BumpMap;
// 			float4 _BumpMap_ST;
			
// 			struct a2v {
// 				float4 vertex : POSITION;
// 				float3 normal : NORMAL;
// 				float4 tangent : TANGENT;
// 				float4 texcoord : TEXCOORD0;
// 			};
			
// 			struct v2f {
// 				float4 pos : SV_POSITION;
// 				float4 uv : TEXCOORD0;
// 				float4 TtoW0 : TEXCOORD1;  
// 				float4 TtoW1 : TEXCOORD2;  
// 				float4 TtoW2 : TEXCOORD3;
// 				SHADOW_COORDS(4)
// 			};
			
// 			v2f vert(a2v v) {
// 				v2f o;
// 				o.pos = UnityObjectToClipPos(v.vertex);
				
// 				o.uv.xy = v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
// 				o.uv.zw = v.texcoord.xy * _BumpMap_ST.xy + _BumpMap_ST.zw;
				
// 				float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;  
// 				fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);  
// 				fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);  
// 				fixed3 worldBinormal = cross(worldNormal, worldTangent) * v.tangent.w; 
				
// 				o.TtoW0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
// 				o.TtoW1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
// 				o.TtoW2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);  
				
// 				TRANSFER_SHADOW(o);
				
// 				return o;
// 			}
			
// 			fixed4 frag(v2f i) : SV_Target {
// 				float3 worldPos = float3(i.TtoW0.w, i.TtoW1.w, i.TtoW2.w);
// 				fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
// 				fixed3 viewDir = normalize(UnityWorldSpaceViewDir(worldPos));
				
// 				fixed3 bump = UnpackNormal(tex2D(_BumpMap, i.uv.zw));
// 				bump = normalize(half3(dot(i.TtoW0.xyz, bump), dot(i.TtoW1.xyz, bump), dot(i.TtoW2.xyz, bump)));
				
// 				fixed3 albedo = tex2D(_MainTex, i.uv.xy).rgb * _Color.rgb;
				
// 			 	fixed3 diffuse = _LightColor0.rgb * albedo * max(0, dot(bump, lightDir));
				
// 				UNITY_LIGHT_ATTENUATION(atten, i, worldPos);
				
// 				return fixed4(diffuse * atten, 1.0);
// 			}
			
// 			ENDCG
// 		}
	}
}
