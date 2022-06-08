Shader "Custom/UnlitColor"
{
    SubShader
    {
        Pass
        {
            Tags { "LightMode" = "CustomLightModeTag"}

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

        float4x4 unity_MatrixVP;
            float4x4 unity_ObjectToWorld;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.vertex = mul(unity_MatrixVP, worldPos);
                return o;
            }

            float4 frag(v2f i) : SV_TARGET
            {
                return float4(0.5,1,0.5,1);
            }
            ENDHLSL
        }
    }
}