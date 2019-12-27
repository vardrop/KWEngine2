#version 430
 
in		vec3 aPosition;
in		vec2 aTexture;

out		vec2 vTexture;

uniform mat4 uMVP;
 
void main()
{
	vTexture.x = aTexture.x; 
	vTexture.y = 1.0 - aTexture.y;
	gl_Position = (uMVP * vec4(aPosition, 1.0)).xyww; 
}