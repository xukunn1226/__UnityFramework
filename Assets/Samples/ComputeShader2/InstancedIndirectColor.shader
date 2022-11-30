Shader "Custom/InstancedIndirectColor" {
    SubShader {
        Tags { "RenderType" = "Opaque" }
        Cull Off

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma instancing_options procedural:setup
            
            #include "UnityCG.cginc"
            
            struct appdata_t {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
            }; 

            struct MeshProperties {
                float4x4 mat;
                float4 color;
            };

#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            StructuredBuffer<MeshProperties> _Properties;

			void setup()
            {
				unity_ObjectToWorld = _Properties[unity_InstanceID].mat;
			}
#endif

            v2f vert(appdata_t i, uint instanceID: SV_InstanceID)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(i);

#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
                //float4 pos = mul(_Properties[instanceID].mat, i.vertex);
                o.vertex = UnityObjectToClipPos(i.vertex);

                o.color = _Properties[instanceID].color;
#else
                o.vertex = UnityObjectToClipPos(i.vertex);
                o.color = fixed4(0, 1, 0, 1);
#endif
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target {
                return i.color;
            }
            
            ENDCG
        }
    }
}