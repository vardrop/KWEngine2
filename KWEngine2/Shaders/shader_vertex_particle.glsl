#version 430
 
in		vec3 aPosition;
in		vec2 aTexture;

out		vec2 vTexture;

uniform mat4 uMVP;
uniform int uAnimationState;
uniform int uAnimationStates;
 
void main()
{
	float offsetX = float(uAnimationState % uAnimationStates) / uAnimationStates;
	float offsetY = float(int(uAnimationState / uAnimationStates) % uAnimationStates) / uAnimationStates;
	float texX = (aTexture.x / uAnimationStates) + offsetX;
	float texY = (aTexture.y / uAnimationStates) + offsetY;
	vTexture = vec2(texX, texY); 

	gl_Position = uMVP * vec4(aPosition.xyz, 1.0); 
}