#version 430

in		vec2 vTexture;

uniform sampler2D uTextureScene;
uniform sampler2D uTextureBloom;

out		vec4 color;

void main()
{
	vec3 result = texture(uTextureScene, vTexture).rgb;
	result += texture(uTextureBloom, vTexture).rgb;

	color.x = result.x;
	color.y = result.y;
	color.z = result.z;
	color.w = 1.0;
}