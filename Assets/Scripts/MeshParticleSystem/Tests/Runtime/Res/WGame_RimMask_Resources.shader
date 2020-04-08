
Shader "WGame/RimMask_Resources" {
Properties {
	//_TintColor("Color", Color) = (1.0,1.0,1.0,1.0)
	_MainTex ("Diffuse", 2D) = "white" {}	
	//_ColorMask("Mask", 2D) = "black" {}
	//_NoiseValue("Noise Value", Range(0,1)) = 0.0
	//[NoScaleOffset]_NoiseTex ("Noise Texture ", 2D) = "White" {}
	[NoScaleOffset]_RimMask ("Rim Mask Texture ", 2D) = "White" {}
	_RimColor ("Rim Color", Color) = (1.0, 1.0, 1.0, 1.0)
	_RimPower ("Rim Power", Range(0.01,10.0 )) = 5.0
}
SubShader{
	Tags { "RenderType" = "Opaque" }
	LOD 100
	Cull Back

	Pass{
		Tags{ "LightMode" = "ForwardBase" }
		Blend off
		CGPROGRAM
		
		#pragma vertex vert
		#pragma fragment frag
		#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
		#pragma skip_variants SHADOWS_SCREEN LIGHTPROBE_SH
		#pragma multi_compile_instancing
		
#include "UnityCG.cginc"
#include "UnityLightingCommon.cginc"
#include "AutoLight.cginc"
#include "assets/scripts/meshparticlesystem/tests/runtime/res/curvedrendercommon.cginc"

		struct appdata
		{
			float4 vertex : POSITION;
			float3 normal : NORMAL;
			float2 uv : TEXCOORD0;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		struct v2f
		{
			float4 pos : SV_POSITION;
			float4 diffuse : COLOR0;
			float2 uv : TEXCOORD0;
			half4 ambient : TEXCOORD1;
			float4 NdotV: COLOR1;
			SHADOW_COORDS(2)
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		sampler2D _MainTex;
		//sampler2D _ColorMask;
		//sampler2D _NoiseTex;
		sampler2D _RimMask;
		float4 _RimColor;
		float _RimPower;
		
		UNITY_INSTANCING_BUFFER_START(Props)
           UNITY_DEFINE_INSTANCED_PROP(float4, _MainTex_ST)
        UNITY_INSTANCING_BUFFER_END(Props)

		v2f vert(appdata v)
		{
			v2f o;
			
			UNITY_SETUP_INSTANCE_ID(v);

			o.pos = CalculateCurvedViewPos(v.vertex);
			
			o.uv = v.uv * UNITY_ACCESS_INSTANCED_PROP(Props, _MainTex_ST).xy + UNITY_ACCESS_INSTANCED_PROP(Props, _MainTex_ST).zw;

			half3 wNormal = UnityObjectToWorldNormal(v.normal);
			half nl = max(0, dot(wNormal, _WorldSpaceLightPos0.xyz));
			o.diffuse = nl * _LightColor0;

			o.ambient = half4(ShadeSH9(half4(wNormal, 1)), 0.0f);

			float3 V = WorldSpaceViewDir(v.vertex);
			V = mul(unity_WorldToObject, V);
			o.NdotV.x = saturate(dot(v.normal, normalize(V)));

			TRANSFER_SHADOW(o);

			return o;
		}

		fixed4 frag(v2f i) : SV_Target
		{
			fixed4 col = tex2D(_MainTex, i.uv);
			fixed rimMask = tex2D(_RimMask, i.uv).r;

			//fixed2 uvN = i.uv * 2.0;
			//fixed n = tex2D(_NoiseTex, uvN).r;
			//fixed mask = tex2D(_ColorMask, i.uv).r;
			//fixed3 mcolor = (dot(col.rgb, float3(0.3, 0.59, 0.11)) * 2) ;
			//col.rgb = lerp(col.rgb, mcolor.rgb, mask) * lerp(1, n, noiseVal);
			

			float4 Emissive =  (_RimColor * pow((1 - i.NdotV.x), _RimPower) * rimMask) * 20 ;
		
			fixed shadow = SHADOW_ATTENUATION(i);

			fixed4 diffuse = i.diffuse * shadow + i.ambient;
			col *= diffuse + Emissive;
			col.a = 1.0f;

			return col;
		}

		ENDCG
	}

	UsePass "VertexLit/SHADOWCASTER"
}
}
