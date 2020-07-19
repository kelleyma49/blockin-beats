////////////////////////////////////////////////////////////////////////////////
//
// Created by Matthew Arcus, 2018
// Wythoff construction for dual snub quadrille tessellation
//
// Fold down to square fundamental region. Draw lines to "Wythoff point".
// Keep track of folding for consistent coloring.
//
// Now with extra colors:
// 'c' changes color scheme to one per tile type
// 'r' shows fundamental region
//
////////////////////////////////////////////////////////////////////////////////

const float PI = 3.141592654;
#define TWOPI 2.0*PI

vec2 perp(vec2 r) {
  return vec2(-r.y,r.x);
}

int imod(int n, int m) {
  int k = n - n/m*m;
  if (k < 0) return k+m;
  else return k;
}

const int CHAR_C = 67;
const int CHAR_R = 82;
const int CHAR_S = 83;
const int CHAR_Z = 90;

bool keypress(int key) {
#if __VERSION__ < 300
    return false;
#else
    return texelFetch(iChannel0, ivec2(key,2),0).x != 0.0;
#endif
}

vec3 getcol0(ivec2 s) {
  int i = 2*imod(s.x,2)+imod(s.y,2);
  if (i == 0) return vec3(1,0,0);
  if (i == 1) return vec3(0,1,0);
  if (i == 2) return vec3(0,0,1);
  if (i == 3) return vec3(1,1,0);
  if (i == 4) return vec3(1,0,1);
  if (i == 5) return vec3(0,1,1);
  if (i == 6) return vec3(1,1,1);
  return vec3(1,1,1);
}

vec3 getcol1(ivec2 s) {
  // http://www.iquilezles.org/www/articles/palettes/palettes.htm
  float t = 0.1*iTime + 0.1*0.618*float(s.x+s.y);
  vec3 a = vec3(0.4,0.4,0.4);
  vec3 b = vec3(0.6,0.6,0.6);
  vec3 c = vec3(1,1,1);
  vec3 d = vec3(0,0.33,0.67);
  vec3 col = a + b*cos(TWOPI*(c*t+d));
  return col;
}

vec3 getcol(ivec2 s) {
  if (keypress(CHAR_C)) {
    return 0.4+0.6*getcol0(s);
  } else {
    return getcol1(s);
  }
}

// segment function by FabriceNeyret2
float segment(vec2 p, vec2 a, vec2 b) {
  vec2 pa = p - a;
  vec2 ba = b - a;
  float h = clamp(dot(pa, ba) / dot(ba, ba), 0.0, 1.0);
  float d = length(pa - ba * h);
  return d;
}

ivec2 nextcell(ivec2 s, int q) {
  q = imod(q,4);
  if (q == 0) s.x++;
  else if (q == 1) s.y--;
  else if (q == 2) s.x--;
  else if (q == 3) s.y++;
  return s;
}

void mainImage( out vec4 fragColor, in vec2 fragCoord ) {
  float scale = 3.3;
  if (keypress(CHAR_Z)) scale *= 2.0;
  float lwidth = 0.025;
  // Half the width of the AA line edge
  float awidth = 1.5*scale/iResolution.y;
  vec2 q,p = (2.0*fragCoord.xy-iResolution.xy)/iResolution.y;
/*  if (iMouse.x > 5.0) {
    q = (iMouse.xy-25.0)/(iResolution.xy-50.0);
    q = clamp(q,0.0,1.0);
  } else {*/
    // Just bouncing around
    q = mod(0.3*iTime*vec2(1,1.618),2.0);
    q = min(q,2.0-q);
  //}
  p *= scale;
  ivec2 s = ivec2(floor(p));
  p = mod(p,2.0)-1.0; // Fold down to ±1 square
  int parity = int((p.y < 0.0) != (p.x < 0.0)); // Reflection?
  int quad = 2*int(p.x < 0.0) + parity; // Quadrant
  p = abs(p);
  if (parity != 0) p.xy = p.yx;
  // Lines from triangle vertices to Wythoff point
  float d = 1e8;
  d = min(d,segment(p,vec2(0,0),q));
  d = min(d,segment(p,vec2(1,0),q));
  d = min(d,segment(p,vec2(1,1),q));
  d = min(d,segment(p,vec2(-q.y,q.x),vec2(q.y,-q.x)));
  d = min(d,segment(p,vec2(-q.y,q.x),vec2(q.y,2.0-q.x)));
  d = min(d,segment(p,vec2(2.0-q.y,q.x),vec2(q.y,2.0-q.x)));
  // Color - what side of the lines are we?
  float a = dot(p-q,perp(vec2(0,0)-q));
  float b = dot(p-q,perp(vec2(1,0)-q));
  float c = dot(p-q,perp(vec2(1,1)-q));
  bool unit = s == ivec2(0,0);
  if (b > 0.0) {
    if (c < 0.0) s = nextcell(s,quad);
  } else {
    if (a > 0.0) s = nextcell(s,quad+1);
  }
  vec3 col = getcol(s);
  col = mix(col,vec3(1,1,1),0.1);
  col *= 0.75;
  col = mix(vec3(0,0,0),col,smoothstep(lwidth-awidth,lwidth+awidth,d));
  if (keypress(CHAR_R)) {
    vec2 p1 = min(p,1.0-p);
    float d1 = min(p1.x,p1.y);
    col = mix(vec3(0.5,0.5,0.5),col,smoothstep(0.5*lwidth-awidth,0.5*lwidth+awidth,d1));
  }
  if (keypress(CHAR_S)) {
    vec2 q0 = q.yx;
    vec2 q1 = vec2(2.0-q.x,q.y);
    vec2 q2 = vec2(q.x,-q.y);
    vec2 q3 = vec2(q.x,2.0-q.y);
    vec2 q4 = vec2(-q.x,q.y);
    vec2 q5 = vec2(-q.y,2.0-q.x);
    d = min(segment(p,q0,q1),segment(p,q0,q2));
    d = min(d,segment(p,q0,q3));
    d = min(d,segment(p,q0,q4));
    d = min(d,segment(p,q0,q5));
    col = mix(vec3(0.5,0.5,0.5),col,smoothstep(0.5*lwidth-awidth,0.5*lwidth+awidth,d));
  }

  fragColor = vec4(sqrt(col),1.0);
}
