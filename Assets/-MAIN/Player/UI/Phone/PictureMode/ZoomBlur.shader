Shader "Unlit/ZoomBlurVR"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurSize ("Blur Amount", Float) = 0.0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ UNITY_SINGLE_PASS_STEREO
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _BlurSize;

            struct appdata
            {
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                UNITY_VERTEX_OUTPUT_STEREO
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = UnityObjectToClipPos(v.vertex);

                // Adjust UVs per-eye for VR
                o.uv = UnityStereoScreenSpaceUVAdjust(v.uv, float4(1, 1, 0, 0));
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float2 offset = _MainTex_TexelSize.xy * _BlurSize;

                fixed4 col = tex2D(_MainTex, uv) * 0.4;
                col += tex2D(_MainTex, uv + offset) * 0.15;
                col += tex2D(_MainTex, uv - offset) * 0.15;
                col += tex2D(_MainTex, uv + offset * float2(1, -1)) * 0.15;
                col += tex2D(_MainTex, uv + offset * float2(-1, 1)) * 0.15;

                return col;
            }
            ENDCG
        }
    }
}