Shader "Hexs Pixel"
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

			float3 hue2rgb(float h)
			{
				h = frac(h) * 6 - 2;
				return clamp(float3(abs(h - 1) - 1, 2 - abs(h), 2 - abs(h - 2)), 0, 1);
			}
     
            fixed4 frag (UNITY_VPOS_TYPE screenPos : VPOS) : SV_Target
			{
				float time = _Time * 10.0f;
				float2 resolution = float2(ResolutionX,ResolutionY);

				const float pi = 3.1415926535;
				float2 p = screenPos.xy - resolution / 2;
				float phi = atan2(p.y, p.x + 1e-5);

				float fin = fmod(floor(phi * 3 / pi + 0.5), 6);
				float phi_fin = fin * pi / 3;

				float2 dir = float2(cos(phi_fin), sin(phi_fin));
				float l = dot(dir, p) - time * resolution.y / 8;
				float seg = floor(l * 40 / resolution.y);

				float th = sin(time) * 0.4 + 0.5;
				float t = sin(seg * 92.198763) * time;

				float3 c  = hue2rgb(sin(seg * 99.374662) * 237.28364);
				c *= step(th, frac(phi / pi / 2 + t));

				return fixed4(c, 1);

				/*

                float time = _Time * 10.0f;
                float2 resolution = float2(ResolutionX,ResolutionY);
                float2 coord = screenPos.xy - resolution * 0.5;

				float phi = atan2(coord.x , coord.y + 1e-6);
				phi = phi / PI * 0.5 + 0.5;
				float seg = floor(phi * 6);

				float theta = (seg + 0.5) / 6 * PI * 2;
				float2 dir1 = float2(cos(theta), sin(theta));
				float2 dir2 = float2(-dir1.y, dir1.x);

				float l = dot(dir1, coord);
				float w = sin(seg * 31.374) * 18 + 20;
				float prog = l / w + time * 2;
				float idx = floor(prog);

				float phase = time * 0.8;
				float th1 = frac(273.84937 * sin(idx * 54.67458 + floor(phase    )));
				float th2 = frac(273.84937 * sin(idx * 54.67458 + floor(phase + 1)));
				float thresh = lerp(th1, th2, smoothstep(0.75, 1, frac(phase)));

				float l2 = dot(dir2, coord);
				float slide = frac(idx * 32.74853) * 200 * time;
				float w2 = frac(idx * 39.721784) * 500;
				float prog2 = (l2 + slide) / w2;

				float c = clamp((frac(prog) - thresh) * w * 0.3, 0, 1);
				c *= clamp((frac(prog2) - 1 + thresh) * w2 * 0.3, 0, 1);

				return fixed4(c, c, c, 1);
				*/
            }
            ENDCG
        }
    }
}


/*
#version 150

uniform float time;
uniform vec2 resolution;
out vec4 fragColor;

vec3 hue2rgb(float h)
{
    h = fract(h) * 6 - 2;
    return clamp(vec3(abs(h - 1) - 1, 2 - abs(h), 2 - abs(h - 2)), 0, 1);
}

void main(void)
{
    const float pi = 3.1415926535;
    vec2 p = gl_FragCoord.xy - resolution / 2;
    float phi = atan(p.y, p.x + 1e-5);

    float fin = mod(floor(phi * 3 / pi + 0.5), 6);
    float phi_fin = fin * pi / 3;

    vec2 dir = vec2(cos(phi_fin), sin(phi_fin));
    float l = dot(dir, p) - time * resolution.y / 8;
    float seg = floor(l * 40 / resolution.y);

    float th = sin(time) * 0.4 + 0.5;
    float t = sin(seg * 92.198763) * time;

    vec3 c  = hue2rgb(sin(seg * 99.374662) * 237.28364);
    c *= step(th, fract(phi / pi / 2 + t));

    fragColor = vec4(c, 1);
}
*/