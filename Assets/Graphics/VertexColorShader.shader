// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/VertexColorShader"
{
    Properties
    {
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct VertexIn
            {
				float4 vertex : POSITION;
                float4 color : COLOR;
            };

            struct VertexOut
            {
				float4 position : POSITION;
                float4 color : COLOR;
            };

            VertexOut vert (VertexIn vertexIn)
            {
				VertexOut vertexOut;
				vertexOut.position = UnityObjectToClipPos(vertexIn.vertex);
				vertexOut.color = vertexIn.color;
                return vertexOut;
            }

            float4 frag (VertexOut vertexOut) : SV_Target
            {
                return float4(vertexOut.color.xyz, 1.0);
            }
            ENDCG
        }
    }
}
