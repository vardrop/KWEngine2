#version 430

in		vec3 aPosition;
in		vec2 aTexture;
in		ivec3 aBoneIds;
in		vec3 aBoneWeights;

out		vec2 vTexture;
out		vec3 colors;

uniform mat4 uMVP;
uniform mat4 uBoneTransforms[36];
uniform int uUseAnimations;

void main()
{
	vec4 totalLocalPos = vec4(0.0);
	//vec4 totalNormal = vec4(0.0);
	//vec4 totalTangent = vec4(0.0);
	//vec4 totalBiTangent = vec4(0.0);

	colors = vec3(float(aBoneIds.x) / 3.0, float(aBoneIds.y) / 3.0, float(aBoneIds.z) / 3.0);

	if(uUseAnimations > 0)
	{	
		for(int i = 0; i < 3; i++)
		{
		    int index = aBoneIds[i];
			totalLocalPos += aBoneWeights[i] * uBoneTransforms[index] * vec4(aPosition, 1.0);
			//totalNormal  += aBoneWeights[i] * uBoneTransforms[index] * vec4(aNormal, 0.0);
			//totalTangent += aBoneWeights[i] * uBoneTransforms[index] * vec4(aNormalTangent, 0.0);
			//totalBiTangent  += aBoneWeights[i] * uBoneTransforms[index] * vec4(aNormalBiTangent, 0.0);
		}
	}
	else
	{

		totalLocalPos = vec4(aPosition, 1.0);
		//totalNormal = vec4(aNormal, 0.0);
		//totalTangent = vec4(aNormalTangent, 0.0);
		//totalBiTangent = vec4(aNormalBiTangent, 0.0);
	}


	vTexture = aTexture;
	gl_Position = uMVP * totalLocalPos;
}