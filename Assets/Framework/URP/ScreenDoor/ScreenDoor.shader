Shader "Hidden/MyURP/ScreenDoor"
{
    Properties 
    {
         _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Main Texture", 2D) = "white" {}
        _Transparency("Alpha", Range(0.0, 1.0)) = 1
    }

    SubShader
    {
        Tags{ "RenderType" = "Opaque" "Queue" = "Geometry" "RenderPipeline"="UniversalRenderPipeline"}

        Pass
        {            
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderVariablesFunctions.hlsl"
            #include "DitherFunctions.hlsl"
            #pragma vertex vert
            #pragma fragment frag

            TEXTURE2D(_MainTex);    SAMPLER(sampler_MainTex);
            
            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float _Transparency;
             float4 _Color;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS       : POSITION;
                float2 texcoord         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS      : SV_POSITION;
                float2 uv               : TEXCOORD0;
                float4 positionSS       : TEXCOORD1;        // screen position
		    };

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.positionSS = ComputeScreenPos(o.positionHCS);

                return o;
            }

            half4 frag(Varyings i) : SV_Target
            { 
                 half4 col = _Color * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                // ditherClip(i.positionSS.xy / i.positionSS.w, col.a, sampler_DitherTex, _Transparency);
                ditherClip(i.positionSS.xy / i.positionSS.w, 0.3);

                return col;
            }

            ENDHLSL
        }

        Pass
        {
            Tags{"LightMode" = "ShadowCaster"}

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderVariablesFunctions.hlsl"
            #include "DitherFunctions.hlsl"
            #pragma vertex vert
            #pragma fragment frag

            TEXTURE2D(_MainTex);    SAMPLER(sampler_MainTex);
            
            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float _Transparency;
            float4 _Color;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS       : POSITION;
                float2 texcoord         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS      : SV_POSITION;
                float2 uv               : TEXCOORD0;
                float4 positionSS       : TEXCOORD1;        // screen position
		    };

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.positionSS = ComputeScreenPos(o.positionHCS);

                return o;
            }

            half4 frag(Varyings i) : SV_Target
            { 
                half4 col = _Color * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                // ditherClip(i.positionSS.xy / i.positionSS.w, col.a, sampler_DitherTex, _Transparency);
                ditherClip(i.positionSS.xy / i.positionSS.w, _Transparency);

                return half4(0,0,0,0);
            }

            ENDHLSL
        }
    }
}
