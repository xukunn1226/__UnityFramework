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
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
            }; 

            struct MeshProperties
            {
                float4x4 mat;
            };

#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            StructuredBuffer<MeshProperties> _Properties;
#endif

#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
			void setup()
            {
				unity_ObjectToWorld = _Properties[unity_InstanceID];
			}
#endif

            v2f vert(appdata_t i, uint instanceID: SV_InstanceID)
            {
                v2f o;
//#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
//                float4 pos = mul(_Properties[instanceID].mat, i.vertex);
//#else
//                float4 pos = 0;
//#endif
                o.vertex = UnityObjectToClipPos(i.vertex);
                o.color = i.color;

                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                return i.color;
            }
            
            ENDCG
        }
    }
}
