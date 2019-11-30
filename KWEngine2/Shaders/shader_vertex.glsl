#version 430

in		vec3 aPosition;
in		vec3 aNormal;
in		vec2 aTexture;
in		vec2 aTexture2;
in		vec4 aNormalTangent;
in		vec3 aNormalBiTangent;
in		ivec4 aJoints;
in		vec4 aWeights;

out		vec3 vNormal;
out		vec2 vTexture;
out		vec2 vTexture2;
out		vec3 vPosition;
out		vec4 vShadowCoord;
out		mat3 vTBN;

uniform int uHasBones;
uniform mat4 uModelMatrix;
uniform mat4 uNormalMatrix;
uniform mat4 uMVP;
uniform mat4 uMVPShadowMap;
uniform mat4[36] uBoneTransforms;

uniform vec2 uTextureTransform;

void main()
{
	vec4 tmpNormal = vec4(0);
	vec4 tmpPosition = vec4(0);
	vec4 tmpTangent = vec4(0);
	vec4 tmpBiTangent = vec4(0);

	if(uHasBones > 0)
	{
		for(int i = 0; i < 4; i++)
		{
			tmpPosition += aWeights[i] * uBoneTransforms[aJoints[i]] * vec4(aPosition, 1.0);
			tmpNormal  += aWeights[i] * uBoneTransforms[aJoints[i]] * vec4(aNormal, 0.0);
			tmpTangent += aWeights[i] * uBoneTransforms[aJoints[i]] * vec4(aNormalTangent.xyz, 0.0);
			tmpBiTangent  += aWeights[i] * uBoneTransforms[aJoints[i]] * vec4(aNormalBiTangent, 0.0);
		}
	}
	else
	{
		tmpPosition = vec4(aPosition, 1.0);
		tmpNormal = vec4(aNormal, 0.0);
		tmpTangent = vec4(aNormalTangent.xyz, 0.0);
		tmpBiTangent = vec4(aNormalBiTangent.xyz, 0.0);
	}

	vNormal = normalize(vec3(uNormalMatrix * tmpNormal));
	vec3 vTangent = normalize(vec3(uNormalMatrix * tmpTangent));
	vec3 vBiTangent = normalize(vec3(uNormalMatrix * tmpBiTangent));

	vTexture = aTexture * uTextureTransform;
	vTexture2 = aTexture2;
	vPosition = (uModelMatrix * tmpPosition).xyz;
	vShadowCoord = uMVPShadowMap * tmpPosition;
	vTBN = mat3(vTangent.xyz, vBiTangent.xyz, vNormal.xyz);

	gl_Position = uMVP * tmpPosition;
}