Shader "SqEq1"
{
   Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_AudioTex("AudioTexture", 2D) = "grey" {}
		[HideInInspector] ResolutionX("ResolutionX", Int) = 512
		[HideInInspector] ResolutionY("ResolutionY", Int) = 512
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
				return o;
			}

		//https://www.shadertoy.com/view/XdX3z2
#define bars 32.0                 // How many buckets to divide spectrum into
#define barSize 1.0 / bars        // Constant to avoid division in main loop
#define barGap 0.1 * barSize      // 0.1 represents gap on both sides, so a bar is
			// shown to be 80% as wide as the spectrum it samples
#define sampleSize 0.02           // How accurately to sample spectrum, must be a factor of 1.0
			// Setting this too low may crash your browser!

			// Helper for intensityToColour
			float h2rgb(float h) {
				if (h < 0.0) h += 1.0;
				if (h < 0.166666) return 0.1 + 4.8 * h;
				if (h < 0.5) return 0.9;
				if (h < 0.666666) return 0.1 + 4.8 * (0.666666 - h);
				return 0.1;
			}

			// Map [0, 1] to rgb using hues from [240, 0] - ie blue to red
			float3 intensityToColour(float i) {
				// Algorithm rearranged from http://www.w3.org/TR/css3-color/#hsl-color
				// with s = 0.8, l = 0.5
				float h = 0.666666 - (i * 0.666666);

				return float3(h2rgb(h + 0.333333), h2rgb(h), h2rgb(h - 0.333333));
			}

			fixed4 frag(UNITY_VPOS_TYPE screenPos : VPOS) : SV_Target {
				float2 resolution = float2(ResolutionX,ResolutionY);
				
				// Map input coordinate to [0, 1)
				float2 uv = screenPos.xy / resolution.xy;

				// Get the starting x for this bar by rounding down
				float barStart = floor(uv.x * bars) / bars;

				// Discard pixels in the 'gap' between bars
				if (uv.x - barStart < barGap || uv.x > barStart + barSize - barGap) {
					return float4(0.0,0.0,0.0,0.0);
				}
				else
				{
					// Sample spectrum in bar area, keep cumulative total
					float intensity = 0.0;
					for (float s = 0.0; s < barSize; s += barSize * sampleSize) {
						// Shader toy shows loudness at a given frequency at (f, 0) with the same value in all channels
						intensity += tex2D(_AudioTex, float2(barStart + s, 0.0f)).r;
					}
					intensity *= sampleSize; // Divide total by number of samples taken (which is 1 / sampleSize)
					intensity = clamp(intensity, 0.005, 1.0); // Show silent spectrum to be just poking out of the bottom

															  // Only want to keep this pixel if it is lower (screenwise) than this bar is loud
					float i = float(intensity > uv.y); // Casting a comparison to float sets i to 0.0 or 1.0

													   //fragColor = vec4(intensityToColour(uv.x), 1.0);       // Demo of HSL function
													   //fragColor = vec4(i);                                  // Black and white output
					return float4(intensityToColour(intensity) * i, i);  // Final output
				}
				// Note that i2c output is multiplied by i even though i is sent in the alpha channel
				// This is because alpha is not 'pre-multiplied' for fragment shaders, you need to do it yourself
			}

            ENDCG
        }
    }
}
