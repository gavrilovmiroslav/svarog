#version 130

uniform sampler2D texture;
uniform vec2 resolution = vec2(1280.0, 800.0);

uniform float scan = 1;
uniform float darken = 1;

void main()
{
	vec2 uv = gl_FragCoord.xy / resolution;
    vec2 dc = abs(0.5 - uv);
    float ld = length(dc);
    
 	vec4 pixel = texture2D(texture, gl_TexCoord[0].xy) * gl_Color;
	
	float drk = mix(1, sqrt(1.0 - ld), darken);
	float apply = max(0.25, abs(sin(gl_FragCoord.y) * scan * sqrt(ld)));
	gl_FragColor = vec4(drk * mix(texture2D(texture, uv).rgb, vec3(0.0), apply), 1.0);
}