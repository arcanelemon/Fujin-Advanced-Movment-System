Shader "Fujin/Texture Paint Shader"{

    Properties{
        [HDR] _Color("Color", Color) = (0, 0, 0, 0)
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _Metallic("Metallic", Range(0,1)) = 0.0
        _Glossiness("Smoothness", Range(0,1)) = 0.5
        _Strength("Strength", Range(0,1)) = 1.0
        _Hardness("Hardness", Range(0,1)) = 0.5
        _NormalTex("Normal", 2D) = "white" {}
    }

    SubShader{
        Cull Off ZWrite Off ZTest Off

        Pass{
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            float4 _Color;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _NormalTex;
            float _Glossiness;
            float _Metallic;
            float _Strength;
            float _Hardness;

            float3 _Position;
            float _Radius;
            float _PrepareUV;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
            };

            float mask(float3 position, float3 center, float radius, float hardness) {
                float m = distance(center, position);
                return 1 - smoothstep(radius * hardness, radius, m);
            }

            v2f vert(appdata v) {
                v2f o;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.uv = v.uv;
                float4 uv = float4(0, 0, 0, 1);
                uv.xy = float2(1, _ProjectionParams.x) * (v.uv.xy * float2(2, 2) - float2(1, 1));
                o.vertex = uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target{
                if (_PrepareUV > 0) {
                    return float4(0, 0, 1, 1);
                }

                float4 col = tex2D(_MainTex, i.uv);
                float f = mask(i.worldPos, _Position, _Radius, _Hardness);
                float edge = f * _Strength;
                return lerp(col, _Color, edge);
            }
            ENDCG
        }
}
}