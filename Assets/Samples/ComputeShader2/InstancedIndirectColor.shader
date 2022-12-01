// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

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
                UNITY_VERTEX_INPUT_INSTANCE_ID
            }; 

            struct MeshProperties {
                float4x4 mat;
                float4 color;
            };


            StructuredBuffer<MeshProperties> _Properties;
#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
			void setup()
            {
				unity_ObjectToWorld = _Properties[unity_InstanceID].mat;
			}
#endif

            /////////// 方法一：使用setup构建object2world矩阵
            v2f vert(appdata_t i)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(i);       // 这行代码将触发setup调用

                o.vertex = UnityObjectToClipPos(i.vertex);
#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED) || defined(UNITY_INSTANCING_ENABLED)
                o.color = _Properties[unity_InstanceID].color;
#endif
                return o;
            }

            //////////// 方法二：直接使用_Properties[instanceID].mat，绕过UnityObjectToClipPos
            //v2f vert(appdata_t i, uint instanceID: SV_InstanceID)
            //{
            //    v2f o;

            //    float4 pos = mul(_Properties[instanceID].mat, i.vertex);
            //    //o.vertex = UnityObjectToClipPos(pos);
            //    o.vertex = mul(UNITY_MATRIX_VP, pos);
            //    o.color = _Properties[instanceID].color;
            //    return o;
            //}
            
            fixed4 frag(v2f i) : SV_Target {
                return i.color;
            }
            
            ENDCG
        }
    }
}