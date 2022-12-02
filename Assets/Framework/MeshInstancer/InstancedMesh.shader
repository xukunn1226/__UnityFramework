Shader "Unlit/InstancedMesh"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma instancing_options procedural:setup
            
            #include "UnityCG.cginc"
            
            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                //float2 uv       : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                //float2 uv       : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            }; 

#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            struct MeshProperties
            {
                float4x4 mat;
            };
            StructuredBuffer<MeshProperties> _Properties;
#endif

#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
			void setup()
            {
				unity_ObjectToWorld = _Properties[unity_InstanceID].mat;
			}
#endif

            v2f vert(appdata_t i)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(i);

//#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
//                float4 pos = mul(_Properties[instanceID].mat, i.vertex);
//#else
//                float4 pos = 0;
//#endif
                o.vertex = UnityObjectToClipPos(i.vertex);
                o.color = i.color;
                //o.uv = i.uv;

                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                //return fixed4(i.uv.xy, 0, 1);
                return i.color;
            }
            
            ENDCG
        }
    }
}
