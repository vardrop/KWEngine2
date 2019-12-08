#version 430

in		vec3 aPosition;
in		vec2 aTexture;
in		vec2 aTexture2;
in		vec3 aNormal;
in		vec3 aTangent;
in		vec3 aBiTangent;
in		ivec3 aBoneIds;
in		vec3 aBoneWeights;

out		vec3 vNormal;
out		vec2 vTexture;
out		vec2 vTexture2;

uniform mat4 uMVP;
uniform mat4 uBoneTransforms[36];
uniform int uUseAnimations;

void main()
{
	mat4 BoneTransform = mat4(0.0);
	if(uUseAnimations > 0)
	{	
		BoneTransform += uBoneTransforms[aBoneIds[0]] * aBoneWeights[0];
		BoneTransform += uBoneTransforms[aBoneIds[1]] * aBoneWeights[1];
		BoneTransform += uBoneTransforms[aBoneIds[2]] * aBoneWeights[2];
	}
	else
	{
		BoneTransform = mat4(1.0);
	}
	//BoneTransform = mat4(1.0);
	vec4 totalLocalPos = BoneTransform * vec4(aPosition, 1.0);
	vNormal = (BoneTransform * vec4(aNormal, 0.0)).xyz;

	vTexture = aTexture;
	gl_Position = uMVP * totalLocalPos;
}