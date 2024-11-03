#version 130

uniform sampler2D texture;

void main()
{
 	vec4 pixel = texture2D(texture, gl_TexCoord[0].xy) * gl_Color;
	float g = (pixel.r + pixel.g + pixel.b) / 3.0;
	gl_FragColor = vec4(g, g, g, pixel.a);
}