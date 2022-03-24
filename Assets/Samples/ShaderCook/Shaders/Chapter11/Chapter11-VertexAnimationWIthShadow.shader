Shader "Unity Shaders Book/Chapter 11/Vertex Animation With Shadow"
{
	Properties
	{
		_MainTex ("Main Tex", 2D) = "white" {}
		_Color ("Color Tint", Color) = (1, 1, 1, 1)
		_Magnitude ("Distortion Magnitude", Float) = 1
 		_Frequency ("Distortion Frequency", Float) = 1
 		_InvWaveLength ("Distortion Inverse Wave Length", Float) = 10
 		_Speed ("Speed", Float) = 0.5
	}
	
	SubShader
	{
		// Need to disable batching because of the vertex animation
		Tags {"DisableBatching"="True" "RenderPipeline"="UniversalRenderPipeline"}
		
		Pass {
			Tags { "LightMode"="UniversalForward" }
			
			Cull Off
			
			HLSLPROGRAM  
			#pragma vertex vert 
			#pragma fragment frag
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			
			CBUFFER_START(UnityPerMaterial)
			sampler2D _MainTex;
			float4 _MainTex_ST;
			half4 _Color;
			float _Magnitude;
			float _Frequency;
			float _InvWaveLength;
			float _Speed;
			CBUFFER_END
			
			struct Attributes
			{
				float4 positionOS	: POSITION;
				float4 texcoord		: TEXCOORD0;
			};

			struct Varyings
			{
				float4 positionHCS	: SV_POSITION;
				float2 uv			: TEXCOORD0;
			};	
						
			Varyings vert(Attributes v)
			{
				Varyings o;
				
				float4 offset;
				offset.yzw = float3(0.0, 0.0, 0.0);
				offset.x = sin(_Frequency * _Time.y + v.positionOS.x * _InvWaveLength + v.positionOS.y * _InvWaveLength + v.positionOS.z * _InvWaveLength) * _Magnitude;
				o.positionHCS = TransformObjectToHClip(v.positionOS + offset);
				
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.uv +=  float2(0.0, _Time.y * _Speed);
				
				return o;
			}
			
			half4 frag(Varyings i) : SV_Target
			{
				half4 c = tex2D(_MainTex, i.uv);
				c.rgb *= _Color.rgb;
				
				return c;
			} 
			
			ENDHLSL
		}
		
		// Pass to render object as a shadow caster
		Pass {
			Tags { "LightMode" = "ShadowCaster" }
			
			HLSLPROGRAM			
			#pragma vertex vert
			#pragma fragment frag
			
			#pragma multi_compile_shadowcaster
			
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			
			CBUFFER_START(UnityPerMaterial)
			float _Magnitude;
			float _Frequency;
			float _InvWaveLength;
			float _Speed;
			CBUFFER_END
			
			struct Attributes
			{
				float4 positionOS	: POSITION;
			};

			struct Varyings
			{
				float4 positionHCS	: SV_POSITION;
				// V2F_SHADOW_CASTER;
			};

			// struct v2f { 
			//     V2F_SHADOW_CASTER;
			// };
			
			Varyings vert(Attributes v)
			{
				Varyings o;
				
				float4 offset;
				offset.yzw = float3(0.0, 0.0, 0.0);
				offset.x = sin(_Frequency * _Time.y + v.positionOS.x * _InvWaveLength + v.positionOS.y * _InvWaveLength + v.positionOS.z * _InvWaveLength) * _Magnitude;
				v.positionOS = v.positionOS + offset;

				o.positionHCS = TransformObjectToHClip(v.positionOS + offset);

				// TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
				
				return o;
			}
			
			half4 frag(Varyings i) : SV_Target
			{
			    // SHADOW_CASTER_FRAGMENT(i)
				return half4(1, 0, 0, 1);
			}
			ENDHLSL
		}
	}
}
