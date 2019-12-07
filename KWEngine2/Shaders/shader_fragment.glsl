#version 430

in vec2 vTexture;

uniform sampler2D uTextureDiffuse;
uniform int uUseTextureDiffuse;
uniform vec3 uBaseColor;

out vec4 color;


void main()
{
	if(uUseTextureDiffuse > 0)
		color = texture(uTextureDiffuse, vTexture);
	else
	    color = vec4(uBaseColor, 1.0);
}