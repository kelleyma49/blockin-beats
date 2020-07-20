void mainImage( out vec4 fragColor, in vec2 fragCoord ) {
	vec2 uv = fragCoord.xy / iResolution.xy;
    vec2 mid = vec2(0.5, 0.5);
    
    vec3 c = vec3(0.0, 0.0, 0.0);
    for(float i = 0.2; i<1.0; i+=0.2) {
        vec4 c1 = texture(iChannel0, vec2(i, 0.5));   
        float f1 = 1.0 / (50.0 * abs(c1.r - distance(uv, mid) + 0.03 * sin(5.0 * uv.y + 5.0 * uv.x + iTime)));

        vec4 c2 = texture(iChannel0, vec2(i, 0.5));   
        float f2 = 1.0 / (50.0 * abs(c2.r - uv.y + 0.03 * cos(5.0 * uv.y + 5.0 * uv.x)));

        c += f1 * vec3(0.3 - i * 0.3, 0.15, i * 0.3) + 
             f2 * vec3(i * 0.3, 0.1, 0.3 - i * 0.3);
    }
    
	fragColor = vec4(c, 1.0);;
}