#version 130

uniform sampler2D texture;
uniform vec2 resolution = vec2(1280.0, 800.0);

uniform float warp = 0.5;
uniform float zoom = -0.05;
uniform float fade = 0.1;

void main()
{
	vec2 uv = gl_FragCoord.xy / resolution;
    vec2 dc = abs(0.5 - uv);
    vec2 dc2 = dc * dc;

	uv.x -= 0.5; uv.x *= 1.0 + (dc2.y * (0.3 * warp)); uv.x += 0.5;
    uv.y -= 0.5; uv.y *= 1.0 + (dc2.x * (0.4 * warp)); uv.y += 0.5;
    
    uv = (uv * (1 - zoom)) + zoom / 2;
    
	if (uv.y > 1.0 || uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0)
    {
        gl_FragColor = vec4(0.0, 0.0, 0.0, 1.0);
    }
    else
    {
        float p = 1 - (fade * length(abs(0.5 - gl_FragCoord.xy / resolution)));
        float ps = sqrt(p);
        gl_FragColor = vec4(ps * texture2D(texture, uv).rgb * (1.0 - length(1.2 * dc2 * dc)), 1.0);
    }
}