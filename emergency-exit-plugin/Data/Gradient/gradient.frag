#version 130

uniform float width = 1280.0;
uniform float size = 0.0;

void main()
{
	float x = gl_FragCoord.x / width;
	if (x > size)
	{
		gl_FragColor = vec4(0, 0, 0, 0);
	} 
	else
	{
		gl_FragColor = vec4(mix(vec3(1.0, 1.0, 0.0), vec3(1.0, 0.0, 0.0), x), 1.0);
	}
}