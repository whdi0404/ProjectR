Shader "Unlit/TileShader"
{
    Properties
    {
        _AtlasTexture("AtlasTexture", 2D) = "white" {}
        _Width("Width", Int) = 16
        _Height("Height", Int) = 16
        _Pos("Pos", Vector) = (0,0,0,0)
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

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                uint instanceID : TEXCOORD1;
            };

            struct MeshProperties {
                int tex;
            };

            struct AtlasInfo {
                int order;
                float2 uvStart;
                float2 uvEnd;
            };

            StructuredBuffer<MeshProperties> _Properties;
            StructuredBuffer<AtlasInfo> _AtlasProperties;

            sampler2D _AtlasTexture;
            float4 _MainTex_ST;
            int _Width;
            int _Height;
            float4 _Pos;

            v2f vert (appdata v, uint instanceID: SV_InstanceID)
            {
                v2f o;

                int x = instanceID % _Width;
                int y = instanceID / _Width;

                float4 pos = float4(v.vertex.x + x, v.vertex.y + y,v.vertex.z,1.0f) + _Pos;
                o.vertex = mul(UNITY_MATRIX_VP, pos);
                o.uv = v.uv;
                o.instanceID = instanceID;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                int tex = _Properties[i.instanceID].tex;
                int x = i.instanceID % _Width;
                int y = i.instanceID / _Width;

                float2 texCoord = lerp(_AtlasProperties[tex].uvStart, _AtlasProperties[tex].uvEnd,i.uv);
                // sample the texture
                fixed4 col = tex2D(_AtlasTexture, texCoord);
                // apply fog
                return col;
            }
            ENDCG
        }
    }
}
