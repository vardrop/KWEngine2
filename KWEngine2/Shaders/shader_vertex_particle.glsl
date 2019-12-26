#version 430
 
in		vec3 aPosition;
in		vec2 aTexture;

//out		vec4 vPosition;
out		vec2 vTexture;
//out		vec4 vDepthPosition;

uniform mat4 uMVP;
uniform int uAnimationState;
uniform int uAnimationStates;
//uniform mat4 uDepthMVP;

 
void main()
{
	//vPosition = uMVP * vec4(aPosition.xyz, 1.0); 
	//vDepthPosition = uDepthMVP * vec4(aPosition.xyz, 1.0);

	float offsetX = float(uAnimationState % uAnimationStates) / uAnimationStates;
	float offsetY = float(int(uAnimationState / uAnimationStates) % uAnimationStates) / uAnimationStates;
	float texX = (aTexture.x / uAnimationStates) + offsetX;
	float texY = (aTexture.y / uAnimationStates) + offsetY;
	vTexture = vec2(texX, texY); 

	//gl_Position = vPosition;
	gl_Position = uMVP * vec4(aPosition.xyz, 1.0); 
}