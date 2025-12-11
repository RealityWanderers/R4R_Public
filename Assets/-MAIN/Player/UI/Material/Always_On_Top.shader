Shader "R4R/AlwaysOnTopWithTexture_VR"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _Color ("Object Color", Color) = (1,1,1,1)
        [Enum(Off,0,Front,1,Back,2)] _Cull("Render Face", Float) = 2
    }

    SubShader
    {
        Tags
        {
            "Queue"="Overlay"   // Ensures the shader renders on top of other objects
            "RenderType"="Opaque" // Treat the shader as an opaque surface
            "IgnoreProjector"="True"
        }

        // Default ZTest and ZWrite settings
        ZTest LEqual    // Tests if the current fragment is in front of or equal to the current depth.
        ZWrite On       // Writes depth information to the depth buffer so that future fragments are tested properly.
        //Cull Off        // No culling by default, both sides should be rendered (even for inverted normals)
        Cull [_Cull]
        Blend SrcAlpha OneMinusSrcAlpha  // Standard alpha blending for transparency if necessary

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // Structure for vertex data
            struct appdata_t
            {
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0; // Texture coordinates
            };

            struct v2f
            {
                UNITY_VERTEX_OUTPUT_STEREO
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;  // Texture coordinates
            };

            // Declare the color, texture sampler, and cull mode
            uniform fixed4 _Color;
            sampler2D _MainTex;  // The texture
            uniform float4 _MainTex_ST; // Texture scale and offset
            uniform float _CullMode; // Cull mode variable (0 = Front, 1 = Back, 2 = Off)

            // Vertex shader for stereo rendering
            v2f vert(appdata_t v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                // Transform the vertex position to clip space using Unity's internal stereo matrix handling
                o.vertex = UnityObjectToClipPos(v.vertex);  // This automatically handles the stereo view matrix for both eyes
                
                // Pass the texture coordinates and apply texture scaling/offset
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

                // Apply the color multiplier
                o.color = v.color * _Color;

                return o;
            }

            // Fragment shader
            fixed4 frag(v2f i) : SV_Target
            {
                // Sample the texture using the texture coordinates
                fixed4 texColor = tex2D(_MainTex, i.texcoord);

                // Blend the texture color with the object color
                return texColor * i.color;  // Multiply color and texture for blending
            }
            ENDCG
        }
    }

    Fallback "Diffuse"
}
