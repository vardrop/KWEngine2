#version 430

in vec2 vTexture;

uniform sampler2D uTextureDiffuse;

out vec4 color;


void main()
{
	color = texture(uTextureDiffuse, vTexture);
}