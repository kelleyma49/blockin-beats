Shader "BouncingPrimitive"
{
   Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_AudioTex("AudioTexture", 2D) = "grey" {}
		_NoiseTex("NoiseTexture", 2D) = "grey" {}
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
			sampler2D _NoiseTex;
            int ResolutionX;
			int ResolutionY;
			float4 _MainTex_ST;


			//***********************************************************************************
			// The MIT License
			// Copyright © 2017 Piotr Podziemski 
			// ----------------------------------------
			// A first shader - commented heavily for myself and others at begginer level
			// Thanks for inspiration to: @jonobr1, @iq
			//***********************************************************************************


#define CS(a) float2(cos(a),sin(a)) 

			//-----------------------------------------------------------
			// Get color in 0-1 range based on 0-255 RGB value
			//-----------------------------------------------------------
			float3 rgbNormalized(float r, float g, float b)
			{
				return float3(r / 255.0, g / 255.0, b / 255.0);
			}

			//-----------------------------------------------------------
			// 2d signed distance to the circle
			//-----------------------------------------------------------
			float sd2dCircle(float2 uv, float2 origin, float radius)
			{
				float d = length(origin - uv) - radius;
				return d;
			}

			//-----------------------------------------------------------
			// 2d signed distance to the square
			//-----------------------------------------------------------
			float vmax(float2 v)
			{
				return max(v.x, v.y);
			}
			float sd2dBox(float2 uv, float2 origin, float2 s)
			{
				return vmax(abs(uv - origin) - s);
			}
			
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

			fixed4 frag(UNITY_VPOS_TYPE screenPos : VPOS) : SV_Target
			{
				float2 iResolution = float2(ResolutionX,ResolutionY);
				float iGlobalTime = _Time * 10.0f;

				float time = 3.*iGlobalTime; //global time
				float noiseValue = tex2D(_NoiseTex, screenPos.xy / iResolution.x).x;

				//[1] Coordinates of the fragment we are dealing with right now
				float2 uv = -1.0 + 2.*screenPos.xy / iResolution.xy;    //uv coordinates -1<u<1 ; -1<v<1 of the fragment on the viewport screen
				uv.y *= iResolution.y / iResolution.x;  //correct y coordinate for the right aspect ratio

														//[2] Properties of the objects
				float radius = 0.25;
				float2 centerPoint = float2(0.0, 0.0);

				//[2] Define movement of the center point of the circle
				float amplitudeMod = 0.02 + 0.4*tex2D(_AudioTex, float2(0.3, 0.0)).x;
				float shapeChangeMod = 1.0*tex2D(_AudioTex, float2(0.5, 0.0)).x;
				float2 movement = amplitudeMod * CS(shapeChangeMod + time + noiseValue);

				centerPoint += movement;

				// Background color
				float4 layer1 = float4(rgbNormalized(200.0, 210.0, 220.0), 1.0);
				float4 layer2 = float4(0.0, 0.0, 0.0, 0.0);
				// Circle
				float3 red = rgbNormalized(225.0, 95.0, 60.0);


				float minD = lerp
				(
					sd2dCircle(uv, centerPoint, radius),
					sd2dBox(uv, centerPoint, float2(radius, radius)),
					(cos(shapeChangeMod*20.0 + time*1.564526) + 1.0) / 2.0
				);
				if (minD <0.1)
				{
					float t = clamp(minD*iResolution.x, 0.0, 1.0);
					layer2 = float4(0.5 + 0.5*shapeChangeMod, 0.0 + 0.1*shapeChangeMod, 0.0, 1.0 - t);
				}

				// vigneting from iq
				float2 q = screenPos.xy / iResolution.xy;
				layer1 *= 0.2 + 0.8*pow(16.0*q.x*q.y*(1.0 - q.x)*(1.0 - q.y), 0.1);


				// Blend the two
				return lerp(layer1, layer2, layer2.a);
			}

            ENDCG
        }
    }
}




