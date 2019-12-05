#version 430

in vec2 vTexture;
in vec3 colors;

uniform sampler2D uTextureDiffuse;
uniform int uUseTextureDiffuse;

out vec4 color;


void main()
{
	if(uUseTextureDiffuse > 0)
		color = texture(uTextureDiffuse, vTexture);
	else
	    color = vec4(colors, 1.0);
		//color = vec4(1.0, 1.0, 1.0, 1.0);
}