Shader "MasqueradeMystery/SpriteOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] _OutlineEnabled ("Outline Enabled", Float) = 0
        _OutlineColor ("Outline Color", Color) = (1,0,0,1)
        _OutlineWidth ("Outline Width", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            float _OutlineEnabled;
            fixed4 _OutlineColor;
            float _OutlineWidth;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.texcoord);
                col *= i.color;

                // Check if outline is enabled and current pixel is transparent
                if (_OutlineEnabled > 0.5 && col.a < 0.1)
                {
                    // Sample neighboring pixels
                    float2 texelSize = _MainTex_TexelSize.xy * _OutlineWidth;

                    float alphaUp = tex2D(_MainTex, i.texcoord + float2(0, texelSize.y)).a;
                    float alphaDown = tex2D(_MainTex, i.texcoord + float2(0, -texelSize.y)).a;
                    float alphaLeft = tex2D(_MainTex, i.texcoord + float2(-texelSize.x, 0)).a;
                    float alphaRight = tex2D(_MainTex, i.texcoord + float2(texelSize.x, 0)).a;

                    // Also check diagonals for smoother outline
                    float alphaUpLeft = tex2D(_MainTex, i.texcoord + float2(-texelSize.x, texelSize.y)).a;
                    float alphaUpRight = tex2D(_MainTex, i.texcoord + float2(texelSize.x, texelSize.y)).a;
                    float alphaDownLeft = tex2D(_MainTex, i.texcoord + float2(-texelSize.x, -texelSize.y)).a;
                    float alphaDownRight = tex2D(_MainTex, i.texcoord + float2(texelSize.x, -texelSize.y)).a;

                    float maxAlpha = max(max(max(alphaUp, alphaDown), max(alphaLeft, alphaRight)),
                                        max(max(alphaUpLeft, alphaUpRight), max(alphaDownLeft, alphaDownRight)));

                    // If any neighbor is opaque, draw outline
                    if (maxAlpha > 0.5)
                    {
                        col = _OutlineColor;
                        col.rgb *= col.a; // Premultiply alpha for correct blending
                    }
                }
                else
                {
                    // Premultiply alpha for correct blending
                    col.rgb *= col.a;
                }

                return col;
            }
            ENDCG
        }
    }
}
