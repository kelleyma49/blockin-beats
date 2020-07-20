Shader "VertexShader1Mesh"
{
   Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_AudioTex("AudioTexture", 2D) = "grey" {}
		VertexCount("Vertex Count", Int) = 0
	}

    SubShader 
    {
        Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			ZWrite On
			ZTest Always
			Cull Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			
			#include "UnityCG.cginc"

			// num verts: 16384
			// triangles
			// https://www.vertexshaderart.com/art/ke5bF6hENE8zphaSp
#define W 128
#define H 64
#define PI 3.1415926535

			float3 hsv2rgb(float3 c) {
				c = float3(c.x, clamp(c.yz, 0.0, 1.0));
				float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
				float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
				return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
				//return float3(1, 0, 0);
			}


			struct v2f
			{
				float4 pos : SV_POSITION;
				float4 color : COLOR0;
			};

			sampler2D _MainTex;
			sampler2D _AudioTex;
            int ResolutionX;
			int ResolutionY;
			int VertexCount;
			float4 _MainTex_ST;

			v2f vert (
				float4 vertex : POSITION, // vertex position input
                float2 uv : TEXCOORD0 // texture coordinate input
			)
			{
				v2f o;
				
				float time = _Time * 10.0f;
				float vertexId = uv.x;
				float2 resolution = float2(ResolutionX, ResolutionY);
				
				float fv = floor(vertexId / float(W));
				float fu = vertexId - fv*float(W);
				fu /= float(W);
				fv /= float(H);
				float u = fu*2.*PI;
				float v = fv*2.*PI;
				u += time;

				float sin_u = sin(u), cos_u = cos(u);
				float sin_v = sin(v), cos_v = cos(v);
				float f = tex2Dlod(_AudioTex, float4(abs(fu - .5) + .1, fv*.1, 0, 0)).x + .05;
				//float f = 0.05;
				float3 p = float3(cos_u*(cos_v*f + 1.), sin_u*(cos_v*f + 1.), sin_v*f);
				float sin_t = sin(time), cos_t = cos(time);
				p = mul(float3x3(cos_t, 0, sin_t, 0, 1, 0, -sin_t, 0, cos_t),p);
				sin_t = sin(time*.7), cos_t = cos(time*.7);
				p = mul(float3x3(cos_t, sin_t, 0, -sin_t, cos_t, 0, 0, 0, 1),p);
				p.x *= resolution.y / resolution.x;
				p.z += 3.;
				p.xy *= 3. / p.z;

				// output:
				o.pos = float4(p.x, p.y, 1., p.z);
				o.color = float4(hsv2rgb(float3(fu*3., 1., 1.)), 1);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				return i.color;
			}

            ENDCG
        }
    }
}