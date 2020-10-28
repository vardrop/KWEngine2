#version 430
 
in		vec3 aPosition;
in		vec2 aTexture;

out		vec2 vTexture;

uniform mat4 uMVP;
uniform vec2 uTextureTransform;
 
void main()
{
	vTexture = vec2(aTexture.x, 1.0 - aTexture.y) * uTextureTransform; 
	gl_Position = (uMVP * vec4(aPosition, 1.0)).xyww; 
}