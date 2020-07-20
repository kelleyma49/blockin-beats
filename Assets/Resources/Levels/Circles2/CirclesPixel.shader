Shader "Circles Pixel"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		//_Samples("Samples", Float[64])
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
			float Samples[64];

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			
			#include "UnityCG.cginc"


			struct v2f
			{
				float2 uv : TEXCOORD0;
			};

			sampler2D _MainTex;
			int ResolutionX;
			int ResolutionY;
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

			float2 rotate(float2 p, float theta)
			{
				float2 sncs = float2(sin(theta), cos(theta));
				return float2(p.x * sncs.y - p.y * sncs.x, dot(p, sncs));
			}

			float swirl(float2 coord, float t)
			{
				float2 resolution = float2(ResolutionX,ResolutionY);

				float l = length(coord) / resolution.x;
				float phi = atan2(coord.y, coord.x + 1e-6);
				return sin(l * 10.0 + phi - t * 4.0) * 0.5 + 0.5;
			}

			float halftone(float2 coord, float angle, float t, float amp)
			{
				float time = _Time * 10.0;
				float2 resolution = float2(ResolutionX,ResolutionY);

				coord -= resolution * 0.5;
				float size = resolution.x / (60.0 + sin(time * 0.5) * 50.0);
				float2 uv = rotate(coord / size, angle / 180.0 * 3.14); 
				float2 ip = floor(uv); // column, row
				float2 odd = float2(0.5 * fmod(ip.y, 2.0), 0.0); // odd line offset
				float2 cp = floor(uv - odd) + odd; // dot center
				float d = length(uv - cp - 0.5) * size; // distance
				float r = swirl(cp * size, t) * size * 0.5 * amp + 5.0f * Samples[64*coord.x/ResolutionY];  // dot radius
				return 1.0 - clamp(d  - r, 0.0, 1.0);
			}
		
			fixed4 frag (v2f i,UNITY_VPOS_TYPE screenPos : VPOS) : SV_Target
			{
				float time = _Time * 10.0;
				
				float3 c1 = 1.0 - float3(1.0, 0.0, 0.0) * halftone(screenPos.xy,   0.0, time * 1.00, 0.7);
				float3 c2 = 1.0 - float3(0.0, 1.0, 0.0) * halftone(screenPos.xy,  30.0, time * 1.33, 0.7);
				float3 c3 = 1.0 - float3(0.0, 0.0, 1.0) * halftone(screenPos.xy, -30.0, time * 1.66, 0.7);
				float3 c4 = 1.0 - float3(1.0, 1.0, 1.0) * halftone(screenPos.xy,  60.0, time * 2.13, 0.4);
				return fixed4(c1 * c2 * c3 * c4,1);
			}
			ENDCG
		}
	}
}
