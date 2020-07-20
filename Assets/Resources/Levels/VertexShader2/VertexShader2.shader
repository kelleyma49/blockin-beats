Shader "VertexShader2"
{
   Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_AudioTex("AudioTexture", 2D) = "grey" {}
	}

    SubShader 
    {
        Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			ZWrite Off
			ZTest Always

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			
			#include "UnityCG.cginc"


			struct v2f
			{
				float4 pos : SV_POSITION;
				fixed3 color : COLOR0;
			};

			sampler2D _MainTex;
			sampler2D _AudioTex;
            int ResolutionX;
			int ResolutionY;
			float4 _MainTex_ST;

			v2f vert (
				float4 vertex : POSITION, // vertex position input
                float2 uv : TEXCOORD0 // texture coordinate input
			)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(vertex);
				o.color = fixed4(1, 0, 0, 1);
							
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				//float2 uv = float2(screenPos.x/ResolutionX,screenPos.y / ResolutionY);
				float2 uv = float2(i.pos.x/ResolutionX,i.pos.y / ResolutionY);
				return tex2D(_MainTex, uv);
			}

            ENDCG
        }
    }
}