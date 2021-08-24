// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "ZGame/AnimationInstancing/DiffuseInstancing"
{
	Properties
	{
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 200

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex 		: POSITION;
	    		float4 tangent 		: TANGENT;
    			float3 normal 		: NORMAL;
    			float2 texcoord 	: TEXCOORD0;
    			float4 texcoord1 	: TEXCOORD1;	// 第二纹理坐标
	    		float4 texcoord2 	: TEXCOORD2;	// 第三纹理坐标
    			fixed4 color 		: COLOR;		// 顶点颜色
    			UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex 		: SV_POSITION;
				float2 uv 			: TEXCOORD0;
				float4 tangent 		: TANGENT;
				float3 normal 		: NORMAL;
				UNITY_VERTEX_INPUT_INSTANCE_ID 		// necessary only if you want to access instanced properties in fragment Shader
			};

			#include "AnimationInstancingBase.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _Color;

			v2f vert(appdata v)
			{
				v2f o;

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o); 	// necessary only if you want to access instanced properties in the fragment Shader.

				o.vertex = UnityObjectToClipPos(skinning(v));
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.tangent = v.tangent;
				o.normal = v.normal;

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);			
				fixed4 c = tex2D(_MainTex, i.uv) * _Color;
				return c;
			}
			ENDCG
		}
	}
}