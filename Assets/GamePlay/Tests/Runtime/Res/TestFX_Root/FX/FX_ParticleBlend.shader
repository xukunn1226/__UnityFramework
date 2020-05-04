// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "WGame Particle/Alpha Blended" {
	Properties{
		_TintColor("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_ColorPow("Color Power",Range(0,5)) = 1
		_MainTex("Particle Texture", 2D) = "white" {}
		_ScrollSpeed("Scroll Speed",Range(-10,10)) = 0
		// _InvFade("Soft Particles Factor", Range(0.01,3.0)) = 1.0
		[Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest",Float) = 8
	}

	Category{
		Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "PreviewType" = "Plane" }
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask RGB
		Cull Off Lighting Off ZWrite Off
		ZTest[_ZTest]

		SubShader {
			LOD 100
			Pass {

				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 3.0
				#pragma multi_compile_instancing
				#include "UnityCG.cginc"
				//#include "Assets/Res/FX/CurvedRenderCommon.cginc"
				#include "assets/gameplay/tests/runtime/res/testfx_root/fx/curvedrendercommon.cginc"

				sampler2D _MainTex;
				float4 _MainTex_ST;
				fixed _ScrollSpeed;

				UNITY_INSTANCING_BUFFER_START(MyProperties)				
					UNITY_DEFINE_INSTANCED_PROP(float4, _TintColor)
					UNITY_DEFINE_INSTANCED_PROP(float, _ColorPow)
				UNITY_INSTANCING_BUFFER_END(MyProperties)

				struct appdata_t {
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float2 texcoord : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct v2f {
					float4 vertex : SV_POSITION;
					fixed4 color : COLOR;
					float2 texcoord : TEXCOORD0;
				};

				v2f vert(appdata_t v)
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);						
					fixed4 tintColor = UNITY_ACCESS_INSTANCED_PROP(MyProperties, _TintColor);
					float colPow = UNITY_ACCESS_INSTANCED_PROP(MyProperties, _ColorPow);
					o.vertex = CalculateCurvedViewPos(v.vertex);
					o.color = v.color * tintColor * 2.0;
					o.color.rgb = o.color.rgb * colPow;
					o.texcoord = TRANSFORM_TEX(v.texcoord.xy,_MainTex) + frac(float2(0,_ScrollSpeed) * _Time.y);
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					fixed4 col = i.color * tex2D(_MainTex, i.texcoord);
					col.rgb = saturate(col.rgb);
					return col;
				}
				ENDCG
			}
		}
		//FallBack "Mobile/Particles/Alpha Blended"
	}
}
