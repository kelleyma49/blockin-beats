Shader "Circles Pixel"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		[HideInInspector] ResolutionX ("ResolutionX", Int) = 512
		[HideInInspector] ResolutionY ("ResolutionY", Int) = 512
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
				float2 iResolution = float2(2048.0,2048.0);
				float interval = iResolution.x * 0.04;
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

			float3 circle(float2 coord, float bias)
			{
				float iGlobalTime = _Time * 10.0f;
				float repeat = sin(iGlobalTime * 0.4) * 10.0 + 30.0;

				float2 iResolution = float2(2048.0,2048.0);
				float interval = iResolution.x / repeat;
				float2 center = iResolution.xy * 0.5;
			
				float dist1 = distance(coord, center);
				float num = max(floor(dist1 / interval + 0.5) + bias, 1.0);
				float radius = num * interval;

				float phase1 = iGlobalTime * 3.0 + radius * 0.04;
				float phase2 = phase1 * 1.3426;
				float2 offs = float2(sin(phase1), cos(phase2)) * interval * 0.5;
				float dist2 = distance(coord, center + offs);

				float width = interval * 0.33;
				float c = clamp(width * 0.5 - abs(radius - dist2), 0.0, 1.0);

			#if MONOCHROME
				return float3(1, 1, 1) * c;
			#else
				float c_r = 0.7 + 0.2 * sin(phase1 * 0.12);
				float c_g = 0.5 + 0.2 * sin(phase1 * 0.34);
				float c_b = 0.3 + 0.2 * sin(phase1 * 0.176);
				return float3(c_r, c_g, c_b) * c;
			#endif
			}
			
			fixed4 frag (v2f i,UNITY_VPOS_TYPE screenPos : VPOS) : SV_Target
			{
				float2 p = screenPos.xy;
				float3 c1 = circle(p, -1.0);
				float3 c2 = circle(p,  0.0);
				float3 c3 = circle(p,  1.0);
				return fixed4(max(max(c1, c2), c3), 1);
			}
			ENDCG
		}
	}
}
