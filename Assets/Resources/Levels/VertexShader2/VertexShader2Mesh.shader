Shader "VertexShader2Mesh"
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

			// num verts: 30000
			// triangles
			// https://www.vertexshaderart.com/art/w39M6FR7PCQctz5bN

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


			// from: http://lolengine.net/blog/2013/07/27/rgb-to-hsv-in-glsl
			float3 hsv2rgb(float3 c) {
				c = float3(c.x, clamp(c.yz, 0.0, 1.0));
				float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
				float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
				return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
			}


			float3 rgb2hsv(float3 c) {
				float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
				float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
				float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));

				float d = q.x - min(q.w, q.y);
				float e = 1.0e-10;
				return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
			}

#define PI radians(180.)

			float4x4 rotZ(float angleInRadians) {
				float s = sin(angleInRadians);
				float c = cos(angleInRadians);

				return float4x4(
					c, -s, 0, 0,
					s, c, 0, 0,
					0, 0, 1, 0,
					0, 0, 0, 1);
			}

			float4x4 trans(float3 trans) {
				return float4x4(
					1, 0, 0, 0,
					0, 1, 0, 0,
					0, 0, 1, 0,
					trans, 1);
			}

			float4x4 ident() {
				return float4x4(
					1, 0, 0, 0,
					0, 1, 0, 0,
					0, 0, 1, 0,
					0, 0, 0, 1);
			}

			float4x4 scale(float3 s) {
				return float4x4(
					s[0], 0, 0, 0,
					0, s[1], 0, 0,
					0, 0, s[2], 0,
					0, 0, 0, 1);
			}

			float4x4 uniformScale(float s) {
				return float4x4(
					s, 0, 0, 0,
					0, s, 0, 0,
					0, 0, s, 0,
					0, 0, 0, 1);
			}

			float mod(float x, float y)
			{
				return x - y * floor(x / y);
			}

			// hash function from https://www.shadertoy.com/view/4djSRW
			float hash(float p) {
				float2 p2 = frac(float2(p * 5.3983, p * 5.4427));
				p2 += dot(p2.yx, p2.xy + float2(21.5351, 14.3137));
				return frac(p2.x * p2.y * 95.4337);
			}

			float2 getCirclePoint(float id, float numCircleSegments) {
				float ux = floor(id / 6.) + mod(id, 2.);
				float vy = mod(floor(id / 2.) + floor(id / 3.), 2.);

				float angle = ux / numCircleSegments * PI * 2.;
				float c = cos(angle);
				float s = sin(angle);

				float radius = lerp(0.5, 1.0, vy);
				float x = c * radius;
				float y = s * radius;

				return float2(x, y);
			}

			v2f vert(
				float4 vertex : POSITION, // vertex position input
				float2 uv : TEXCOORD0 // texture coordinate input
			)
			{
				float time = _Time * 10.0f;
				float vertexId = uv.x;
				float2 resolution = float2(ResolutionX, ResolutionY);

				float numCircleSegments = 4.0;
				float2 circleXY = getCirclePoint(vertexId, numCircleSegments);
				float numPointsPerCircle = numCircleSegments * 6.;

				float circleId = floor(vertexId / numPointsPerCircle);
				float numCircles = floor(VertexCount / numPointsPerCircle);

				float sliceId = floor(vertexId / 6.);
				float oddSlice = mod(sliceId, 2.);

				float down = sqrt(numCircles);
				float across = floor(numCircles / down);
				float x = mod(circleId, across);
				float y = floor(circleId / across);

				float u = x / (across - 1.);
				float v = y / (across - 1.);

				float su = abs(u - 0.5) * 2.;
				float sv = abs(v - 0.5) * 2.;

				float au = abs(atan2(su, sv)) / PI;
				float av = length(float2(su, sv));

				const int spots = 20;
				float snd = 0.;
				float totalSnd = 0.;
				float3 color = float3(0,0,0);
				for (int sp = 0; sp < spots; ++sp) {
					float spf = float(sp + 11);
					float spx = hash(spf * 7.123);
					float spy = hash(spf * 0.317);
					float sps = hash(spf * 3.411);

					float sds = distance(float2(u, v), float2(spx, spy));
					float invSds = pow(clamp(1. - sds, 0., 1.), 3.);
					totalSnd += invSds;
					snd += tex2Dlod(_AudioTex, float4(lerp(0.001, 0.151, sps), sds * .9, 0, 0)).a *
						lerp(0.95, 1.7, sps) * invSds;

					color = lerp(color, hsv2rgb(float3(sps, 1., 1.)), pow(invSds, 2.));
				}
				snd /= totalSnd;

				float xoff = 0.;//sin(time + y * 0.2) * 0.1;
				float yoff = 0.;//sin(time + x * 0.3) * 0.2;

				float ux = u * 2. - 1. + xoff;
				float vy = v * 2. - 1. + yoff;

				float sc = pow(snd, 5.0) * 2. + oddSlice * 0.;
				float aspect = resolution.x / resolution.y;

				float4 pos = float4(circleXY, 0, 1);
				float4x4 mat = ident();
				//mat *= scale(float3(1, aspect, 1));
				mat = mul(scale(float3(1, aspect, 1)),mat);
				//mat *= rotZ(time * 0.);
				//mat *= trans(float3(ux, vy, 0));
				mat = mul(trans(float3(ux, vy, 0)),mat);
				//mat *= uniformScale(0.2 * sc * 20. / across);
				mat = mul(uniformScale(0.2 * sc * 20. / across),mat);
				//mat *= rotZ(snd * 10. * sign(ux));

				v2f o;
				//o.pos = mul(mat,pos);
				o.pos = mul(pos,mat);

				float soff = 1.;//sin(time + x * y * .02) * 5.;  

				float pump = step(0.7, snd);

				float3 hsv = rgb2hsv(color);
				hsv.x = lerp(0., 0.2, hsv.x) + time * 0.1 + pump * .33;
				hsv.z = lerp(0.5, 1., pump);
				o.color = float4(hsv2rgb(hsv), 1);;
				o.color.rgb *= o.color.a;
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