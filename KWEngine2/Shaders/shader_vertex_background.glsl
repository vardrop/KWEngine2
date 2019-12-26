#version 430
 
in		vec3 aPosition;
in		vec2 aTexture;

out		vec2 vTexture;

uniform mat4 uMVP;
uniform vec2 uTextureTransform;
 
void main()
{
	vTexture = aTexture * uTextureTransform; 
	gl_Position = (uMVP * vec4(aPosition, 1.0)).xyww; 
}