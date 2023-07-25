Shader "Hidden/newSobel"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

            float4 blur(sampler2D tex, float2 uv, float4 size){
                float4 c = tex2D(tex, uv + float2(-size.x, size.y)) + tex2D(tex, uv + float2(0, size.y)) + tex2D(tex, uv + float2(size.x, size.y)) +
                            tex2D(tex, uv + float2(-size.x, 0)) + tex2D(tex, uv + float2(0, 0)) + tex2D(tex, uv + float2(size.x, 0)) +
                            tex2D(tex, uv + float2(-size.x, -size.y)) + tex2D(tex, uv + float2(0, -size.y)) + tex2D(tex, uv + float2(size.x, -size.y));
                            
                return c / 9;
            }

            float4 greyscale(sampler2D tex, float2 uv){
                fixed4 col = tex2D(tex, uv);
                float intensity = col.x * 0.299 + col.y * 0.587 + col.z * 0.114;
                return fixed4(intensity, intensity, intensity, col.w);
            }

            float4 SobelDepth(sampler2D tex, float2 uv, float3 offset){
            {
                float4 centerPixel = tex2D(tex, uv);
                float4 leftPixel = tex2D(tex, uv - offset.xz);
                float4 rightPixel = tex2D(tex, uv + offset.xz);
                float4 upPixel = tex2D(tex, uv + offset.xy);
                float4 downPixel = tex2D(tex, uv - offset.xy);

                return abs(leftPixel - centerPixel)*2  +
                       abs(rightPixel - centerPixel)*2 +
                       abs(upPixel - centerPixel)*2    +
                       abs(downPixel - centerPixel)*2;
            }
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float3 sobelNormalVec = SobelDepth(_MainTex, i.uv, _MainTex_TexelSize).xyz;
                float sobelNormal = sobelNormalVec.x + sobelNormalVec.y + sobelNormalVec.z;
                sobelNormal = pow(sobelNormal * 1, 1);
                return fixed4(sobelNormal, sobelNormal, sobelNormal, col.w);
            }
            ENDCG
        }
    }
}
