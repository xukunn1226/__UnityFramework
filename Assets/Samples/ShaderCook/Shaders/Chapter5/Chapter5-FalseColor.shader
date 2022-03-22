Shader "Unity Shaders Book/Chapter 5/False Color"
{
	SubShader
	{
		Tags { "RenderPipeline" = "UniversalRenderPipeline" }

		Pass
		{
			Tags { "LightMode" = "UniversalForward" }

			HLSLPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			
			struct Attributes
			{
				float4 positionOS   : POSITION;
				float3 normalOS		: NORMAL;
				float4 tangentOS	: TANGENT;
				float2 texcoord     : TEXCOORD0;
				float2 texcoord1	: TEXCOORD1;
				half4 color			: COLOR0;
			};

			struct Varyings
			{
				float4 positionHCS  : SV_POSITION;
				half4 color			: COLOR0;
			};

			Varyings vert(Attributes v)
			{
				Varyings o;
				o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
				
				// Visualize normal
				o.color = half4(v.normalOS * 0.5 + half3(0.5, 0.5, 0.5), 1.0);
				
				// Visualize tangent
				o.color = half4(v.tangentOS.xyz * 0.5 + half3(0.5, 0.5, 0.5), 1.0);
				
				// Visualize binormal
				half3 binormal = cross(v.normalOS, v.tangentOS.xyz) * v.tangentOS.w;
				o.color = half4(binormal * 0.5 + half3(0.5, 0.5, 0.5), 1.0);
				
				// Visualize the first set texcoord
				o.color = half4(v.texcoord.xy, 0.0, 1.0);
				
				// Visualize the second set texcoord
				o.color = half4(v.texcoord1.xy, 0.0, 1.0);
				
				// Visualize fractional part of the first set texcoord
				o.color = half4(frac(v.texcoord), 0, 1);
				if (any(saturate(v.texcoord) - v.texcoord)) {
					o.color.b = 0.5;
				}
				o.color.a = 1.0;
				
				// Visualize fractional part of the second set texcoord
				o.color = half4(frac(v.texcoord1), 0, 1);
				if (any(saturate(v.texcoord1) - v.texcoord1)) {
					o.color.b = 0.5;
				}
				o.color.a = 1.0;
				
				// Visualize vertex color
				//o.color = v.color;
				
				return o;
			}
			
			half4 frag(Varyings i) : SV_Target
			{
				return i.color;
			}
			
			ENDHLSL
		}
	}
}
