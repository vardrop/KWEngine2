#version 430

in		vec3 aPosition;
in		vec2 aTexture;
in		ivec3 aBoneIndices;
in		vec3 aBoneWeights;

out		vec2 vTexture;

uniform mat4 uMVP;
uniform mat4 uBoneTransforms[36];
uniform int uUseAnimations;

void main()
{
	vec4 totalLocalPos = vec4(0);
	//vec4 totalNormal = vec4(0);
	//vec4 totalTangent = vec4(0);
	//vec4 totalBiTangent = vec4(0);

	if(uUseAnimations > 0)
	{	
		for(int i = 0; i < 3; i++)
		{
		    int index = aBoneIndices[i];
			totalLocalPos += aBoneWeights[i] * uBoneTransforms[index] * vec4(aPosition, 1);
			//totalNormal  += aBoneWeights[i] * uBoneTransforms[index] * vec4(aNormal, 0);
			//totalTangent += aBoneWeights[i] * uBoneTransforms[index] * vec4(aNormalTangent, 0);
			//totalBiTangent  += aBoneWeights[i] * uBoneTransforms[index] * vec4(aNormalBiTangent, 0);
		}
	}
	else
	{

		totalLocalPos = vec4(aPosition, 1.0);
		//totalNormal = vec4(aNormal, 0);
		//totalTangent = vec4(aNormalTangent, 0);
		//totalBiTangent = vec4(aNormalBiTangent, 0);
	}


	vTexture = aTexture;
	gl_Position = uMVP * totalLocalPos;
}