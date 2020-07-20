// from https://www.shadertoy.com/view/lt3BW8

#define PI 3.1415926
#define TWOPI PI*2.0
vec2 rotate(vec2 v, float a) {
	float s = sin(a);
	float c = cos(a);
	mat2 m = mat2(c, -s, s, c);
	return mul(m,v);
}

float sdBox( in vec2 p, in vec2 b )
{
    vec2 d = abs(p)-b;
    return length(max(d,vec2(0,0))) + min(max(d.x,d.y),0.0);
}

float sdCircle( vec2 p, float r )
{
  return length(p) - r;
}

void mainImage( out vec4 fragColor, in vec2 fragCoord )
{
    vec2 uv = 2.0*(fragCoord-0.5*iResolution.xy)/iResolution.y;
    vec2 norm = 2.0*(fragCoord.xy-0.5*iResolution.xy)/iResolution.xy;
    
	
    vec2 boxcd=rotate(uv, iTime/12.0);
    vec2 displace;
    displace.x = sin(iTime*6.0+(boxcd.x*TWOPI*2.0));
    displace.y = cos(iTime*8.0+(boxcd.y*TWOPI*2.0));
    
    //float box=sdBox(boxcd, vec2((cos(iTime)*0.5+0.5)*0.2));
    float box=sdBox(boxcd, vec2(0.3,0.3));
    box+=(displace.x*displace.y)*cos(iTime)*1.2;
    
    float circle=clamp(0.0, 1.0, sdCircle(uv, 0.));
    
    float thickness = 0.1;
    float wireframe = smoothstep(thickness, thickness+0.15, abs(box));
    
    vec3 bg1 = vec3(0.364,0.496,0.500);
    vec3 bg2 = vec3(0.171,0.233,0.235);
    vec3 background = mix(bg1, bg2, pow(norm.x, 2.0)+pow(norm.y, 2.0));
    vec3 fg1 = vec3(0.98, 0.93, 0.9);
    vec3 fg2 = vec3(0.569,0.740,0.726);
    vec3 foreground = mix(fg1, fg2, pow(norm.x, 2.0)+pow(circle, 2.0));
    
    vec3 col = mix(foreground, background, wireframe);

    fragColor = vec4(col,1.0);
    
}