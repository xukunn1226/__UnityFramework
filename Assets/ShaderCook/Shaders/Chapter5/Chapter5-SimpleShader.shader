Shader "Unity Shaders Book/Chapter 5/Simple Shader"
{
	Properties
    {
		_Color ("Color Tint", Color) = (1, 1, 1, 1)
	}

	SubShader
    {
        Tags { "RenderPipeline"="UniversalRenderPipeline" }

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    
            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                half4 color         : COLOR0;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
            CBUFFER_END

            Varyings vert(Attributes v)
            {
                Varyings o;
            	o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
            	o.color = half4(v.normalOS * 0.5 + half3(0.5, 0.5, 0.5), 1);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
            	half3 c = i.color.rgb;
            	c *= _Color.rgb;
                return half4(c, 1.0);
            }
            ENDHLSL
        }
    }
}
