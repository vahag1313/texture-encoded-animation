Shader "ArmNomads Games/Texture Baked Animation"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _AnimTex ("Animation Texture", 2D) = "grey" {}
        _FrameRate ("Animation Frame Rate", float) = 60
        _AnimBounds ("Bounds", Vector) = (0,0,0,0)
        _VertexCount ("Vertex Count", float) = 0
        _AnimFrameCount ("Animation Frame Count", float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            #pragma target 3.5

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                uint vid : SV_VertexID;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _AnimTex;
            float4 _AnimTex_TexelSize;
            float _AnimFrameCount;
            float _VertexCount;
            float _FrameRate;
            float3 _AnimBounds; 

            v2f vert (appdata v)
            {
                v2f o;
                
                float frame = _AnimTex_TexelSize.x * int(_Time.y * _FrameRate % _AnimFrameCount);
                float vertex =  _AnimTex_TexelSize.y * (v.vid + 0.5);
                float3 pos = tex2Dlod(_AnimTex, float4(frame, vertex,0,0)).xyz;
                pos = (pos * 2.0 - 1.0) * _AnimBounds; 
                o.vertex = UnityObjectToClipPos(pos);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
