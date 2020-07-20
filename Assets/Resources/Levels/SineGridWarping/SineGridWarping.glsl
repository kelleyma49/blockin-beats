// taken from https://www.shadertoy.com/view/4tVyRz
float q(vec2 pos,float angle){
    return pos.x*cos(angle)+pos.y*sin(angle);
}
    
void mainImage( out vec4 fragColor, in vec2 fragCoord )
{
    float pi=atan(0.0,1.0)*2.0;
    // Normalized pixel coordinates (from 0 to 1)
    vec2 uv = fragCoord/iResolution.xy;
	vec2 pos=(uv-vec2(0.5,0.5))*(iResolution.xy/vec2(iResolution.y,iResolution.y))*vec2(100.0f,100.0f);
    float s=iTime/5.0;
    float angle=atan(pos.x,pos.y)+s/2.0;
    pos=vec2(length(pos),length(pos))*vec2(cos(angle),sin(angle));
    float c=cos(q(pos,pi/3.0))+cos(q(pos,0.0))+cos(q(pos,s+pi/3.0))+cos(q(pos,s+0.0))+cos(q(pos,pi/3.0*2.0))+cos(q(pos,s+pi/3.0*2.0));
    // Time varying pixel color
    vec3 col = vec3(0.5,0.5,0.5) + vec3(0.5,0.5,0.5)*cos(vec3(iTime+c,iTime+c,iTime+c)+uv.xyx+vec3(0.0,2.0,4.0));

    // Output to screen
    fragColor = vec4(col*(-c),1.0);
}