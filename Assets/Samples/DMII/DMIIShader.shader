﻿Shader "DMIIShader" {
	Properties {
		_MainTex("Texture", 2D) = "white" {}
		_FadeInT("Fade in time", Float) = 10
	}
	SubShader {
		Tags {
			"Queue" = "Transparent"
		}
		Cull Off
		Lighting Off
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"
			#pragma instancing_options procedural:setup

			struct vertex {
				float4 loc	: POSITION;
				float2 uv	: TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			struct fragment {
				float4 loc	: SV_POSITION;
				float2 uv	: TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			
			CBUFFER_START(MyData)
			    float4 posDirBuffer[7];
			    float timeBuffer[7];
			CBUFFER_END
			
	#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
			void setup() {
				float2 position = posDirBuffer[unity_InstanceID].xy;
				float2 direction = posDirBuffer[unity_InstanceID].zw;
				direction *= smoothstep(0, 10, timeBuffer[unity_InstanceID]);

				unity_ObjectToWorld = float4x4(
					direction.x, -direction.y, 0, position.x,
					direction.y, direction.x, 0, position.y,
					0, 0, 1, 0,
					0, 0, 0, 1
					);
			}
	#endif

			sampler2D _MainTex;
			float _FadeInT;

			fragment vert(vertex v) {
				fragment f;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, f);
				f.loc = UnityObjectToClipPos(v.loc);
				f.uv = v.uv;
				//f.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return f;
			}

			float4 frag(fragment f) : SV_Target{
				UNITY_SETUP_INSTANCE_ID(f);
				float4 c = tex2D(_MainTex, f.uv);
	#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED) || defined(UNITY_INSTANCING_ENABLED)
				c.a *= smoothstep(0.0, _FadeInT, timeBuffer[unity_InstanceID]);
	#endif
				return c;
			}
			ENDCG
		}
	}
}
