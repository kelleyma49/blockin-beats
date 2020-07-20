Shader "Bars Pixel"
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
			};

			sampler2D _MainTex;
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
				return o;
			}

            const float PI = 3.141592;
     
            fixed4 frag (UNITY_VPOS_TYPE screenPos : VPOS) : SV_Target
			{
                float time = _Time * 10.0f;
                float2 resolution = float2(ResolutionX,ResolutionY);
				float2 coord = screenPos.xy;
				float2 size = resolution.xx / float2(60, 15);

				float y = coord.y / size.y;
				float scr = 3 + 3 * frac(floor(y) * 12433.34);
				float x = coord.x / size.x + scr * time;
 
				float t = time * 1.1;
				float t01 = frac(t);
				float phase = floor(x) * 2353.48272 + floor(y) * 2745.32782 + floor(t);

				float h = lerp(
					frac(sin(phase    ) * 1423.84),
					frac(sin(phase + 1) * 1423.84),
					smoothstep(0.8, 1, t01) * 1.3 - smoothstep(0.5, 0.8, t01) * 0.3
				);

				float c1 = (0.4 - abs(0.4 - frac(x))) / 0.8 * size.x;
				float c2 = (h - frac(y)) * size.y;
				float c = clamp(c1, 0, 1) * clamp(c2, 0, 1);

				return fixed4(c, c, c, 1);
            }
            ENDCG
        }
    }
}
