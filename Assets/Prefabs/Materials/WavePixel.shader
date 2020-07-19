Shader "Wave Pixel"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
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
			#pragma target 3.0
			
			#include "UnityCG.cginc"

			struct v2f
			{
				float2 uv : TEXCOORD0;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			v2f vert (
				float4 vertex : POSITION, // vertex position input
                float2 uv : TEXCOORD0, // texture coordinate input
				out float4 outpos : SV_POSITION // clip space position output
			)
			{
				v2f o;
				outpos = UnityObjectToClipPos(vertex);
				return o;
			}

			float wave(float2 coord)
			{
				//float interval = iResolution.x * 0.04;
				float interval = 2048.0f * 0.04;
				float2 p = coord / interval;

				float py2t = 0.112 * sin(_Time * 10.0 * 0.378);
				float phase1 = dot(p, float2(0.00, 1.00)) + _Time * 10.0 *  1.338;
				float phase2 = dot(p, float2(0.09, py2t)) + _Time * 10.0 *  0.566;
				float phase3 = dot(p, float2(0.08, 0.11)) + _Time * 10.0 *  0.666;

				float pt = phase1 + sin(phase2) * 3.0;
				pt = abs(frac(pt) - 0.5) * interval * 0.5;

				float lw = 2.3 + sin(phase3) * 1.9;
				return saturate(lw - pt);
			}
			
			fixed4 frag (v2f i,UNITY_VPOS_TYPE screenPos : VPOS) : SV_Target
			{
				float c = wave(screenPos.xy);
				return fixed4(c,c,c,1.0);
				// sample the texture
				//fixed4 col = tex2D(_MainTex, i.uv);
				// apply fog
				//UNITY_APPLY_FOG(i.fogCoord, col);
				//return col;
			}
			ENDCG
		}
	}
}
