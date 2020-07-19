Shader "DistortSquare"
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

			// #defines for including GLSL shader:
			#define iGlobalTime		_Time.y
			#define iTime			_Time.y
			#define iResolution		_ScreenParams
			#define vec2			float2
			#define vec3			float3
			#define vec4			float4
			#define mat2			float2x2
			#define mat3			float3x3
			#define mat4			float4x4
			#define Texture2D		tex2D 
			#define texture			tex2D
			#define atan(x,y)		atan2(y,x)
			#define mix				lerp
			#define fract			frac
			#define iChannel0		_AudioTex

			float mod(float x, float y)
			{
				return x - y * floor(x/y);
			}

			#include "DistortSquare.glsl"

			fixed4 frag(UNITY_VPOS_TYPE screenPos : VPOS) : SV_Target 
			{
				float4 fragColor;
				mainImage(fragColor,screenPos);
				
				return fixed4(fragColor.x,fragColor.y,fragColor.z,fragColor.w);
			}
           ENDCG
        }
    }
}

