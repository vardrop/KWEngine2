#version 430

in		vec3 vPosition;
in		vec3 vNormal;
in		vec2 vTexture;
in		vec2 vTextureNoRepetitions;
in		mat3 vTBN;
in		vec4 vShadowCoord;
in		vec4 vShadowCoord2;

uniform sampler2D uTextureDiffuse;
uniform int uUseTextureDiffuse;

uniform sampler2D uTextureNormal;
uniform int uUseTextureNormal;

uniform sampler2D uTextureSpecular;
uniform int uUseTextureSpecular;

uniform sampler2D uTextureDiffuseR;
uniform sampler2D uTextureDiffuseG;
uniform sampler2D uTextureDiffuseB;
uniform sampler2D uTextureDiffuseBlend;
uniform int uTextureUseBlend;

uniform sampler2DShadow uTextureShadowMap;
uniform sampler2DShadow uTextureShadowMap2;

uniform int uShadowLightPosition;

uniform vec4 uGlow;
uniform vec3 uTintColor;
uniform float uSunAmbient;
uniform int uSunAffection;
uniform vec3 uSunPosition;
uniform vec3 uSunDirection; // to sun!
uniform vec4 uSunIntensity;
uniform int uLightAffection;

uniform vec3 uCameraPos;
uniform vec3 uCameraDirection;

uniform float uSpecularArea;
uniform float uSpecularPower;

uniform float uBiasCoefficient;
uniform float uBiasCoefficient2;

uniform vec4 uLightsPositions[10];
uniform vec4 uLightsTargets[10];
uniform vec4 uLightsColors[10];
uniform int uLightCount;

out vec4 color;
out vec4 bloom;

float calculateDarkening(float cosTheta, vec4 shadowCoord)
{
	float bias = uBiasCoefficient * sqrt ( 1.0f - cosTheta * cosTheta   ) / cosTheta;
	bias = clamp(bias, 0.0 ,0.01);
	shadowCoord.z -= bias;
	float darkening = 0.0;
	//darkening += textureProjOffset(uTextureShadowMap, shadowCoord, ivec2(-1,-1));
	//darkening += textureProjOffset(uTextureShadowMap, shadowCoord, ivec2(-1,1));
	darkening += textureProjOffset(uTextureShadowMap, shadowCoord, ivec2(0,0));
	//darkening += textureProjOffset(uTextureShadowMap, shadowCoord, ivec2(1,1));
	//darkening += textureProjOffset(uTextureShadowMap, shadowCoord, ivec2(1,-1));
	//darkening /= 5.0;
	return darkening;
}

float calculateDarkening2(float cosTheta, vec4 shadowCoord)
{
	float bias = uBiasCoefficient2 * sqrt ( 1.0f - cosTheta * cosTheta   ) / cosTheta;
	bias = clamp(bias, 0.0 ,0.01);
	shadowCoord.z -= bias;
	float darkening = 0.0;
	//darkening += textureProjOffset(uTextureShadowMap2, shadowCoord, ivec2(-1,-1));
	//darkening += textureProjOffset(uTextureShadowMap2, shadowCoord, ivec2(-1,1));
	darkening += textureProjOffset(uTextureShadowMap2, shadowCoord, ivec2(0,0));
	//darkening += textureProjOffset(uTextureShadowMap2, shadowCoord, ivec2(1,1));
	//darkening += textureProjOffset(uTextureShadowMap2, shadowCoord, ivec2(1,-1));
	//darkening /= 5.0;
	return darkening;
}

void main()
{
	// Normal mapping:
	vec3 theNormal = vec3(0);
	if(uUseTextureNormal > 0)
    {
            theNormal = normalize(texture(uTextureNormal, vTexture).xyz * 2.0 - 1.0);
            theNormal = normalize(vTBN * theNormal);
    }
    else
    {
            theNormal = vNormal;
    }

	vec3 surfaceToCamera = normalize(uCameraPos - vPosition);
	vec3 fragmentToSun = normalize(uSunPosition - vPosition);

	// Shadow mapping:
	float darkening = 1.0;
	float darkeningAbsolute = 1.0;
	float dotNormalLight = 1.0;
	if(uSunAffection > 0)
	{
		dotNormalLight = max(dot(theNormal, uSunDirection), 0.0);								
		float dotNormalLightShadow = max(dot(vNormal, fragmentToSun), 0.0);
		darkeningAbsolute = max(calculateDarkening(dotNormalLightShadow, vShadowCoord), 0.0);
		darkening = max(darkeningAbsolute, uSunAmbient);
	}
	
	// Shadow mapping 2:
	float darkening2 = 1.0;
	if(uShadowLightPosition >= 0)
	{
		float dotNormalLightShadow2 = max(dot(vNormal, normalize(uLightsPositions[uShadowLightPosition].xyz - vPosition)), 0.0);
		float darkeningAbsolute2 = max(calculateDarkening2(dotNormalLightShadow2, vShadowCoord2), 0.0);
		darkening2 = max(darkeningAbsolute2, uSunAmbient);
	}
	
	vec3 ambient = vec3(0.0);
	vec3 totalSpecColor = vec3(0);
	float specularFactor = 1.0;
	if(uUseTextureSpecular > 0)
	{
		specularFactor = texture(uTextureSpecular, vTexture).r;
	}
	
	vec3 reflectionVector = reflect(-uSunDirection, theNormal);
	if(uSunAffection > 0)
	{
		//check if camera may see highlights:
		float dotSunNormalInverted = max(0.0, dot(-theNormal, -uSunDirection));

		//Specular highlights from sun:
		float specular = max(0.0, specularFactor * uSpecularPower * pow(max(0.0, dot(surfaceToCamera, reflectionVector)), uSpecularArea));
		totalSpecColor += uSunIntensity.xyz * specular * uSunIntensity.w * darkeningAbsolute * dotSunNormalInverted;

		ambient = uSunIntensity.xyz * (uSunIntensity.w * min(max(dotNormalLight, uSunAmbient), darkening));
	}
	else
	{
		ambient = vec3(uSunAmbient);
	}

	vec3 colorComponentTotal = vec3(0.0);
	if(uLightAffection > 0)
	{
		for(int i = 0; i < uLightCount; i++)
		{
			float isShadowLight = i == uShadowLightPosition ? darkening2 : 1;
			vec3 lightPos = uLightsPositions[i].xyz;
			vec3 lightColor = uLightsColors[i].xyz;
			vec3 lightDirection = normalize(uLightsTargets[i].xyz - lightPos); // to light target

			vec3 lightVector = lightPos - vPosition;
			float distance = dot(lightVector, lightVector);
			lightVector = normalize(lightVector);

			//check if camera may see highlights:
			float dotLightNormalInverted; // = 1.0;

			float distanceFactor = (uLightsPositions[i].w * 10 / distance);

			// directional light falloff:
			float differenceLightDirectionAndFragmentDirection = 1.0;
			if(uLightsTargets[i].w > 0.0){ // directional
				differenceLightDirectionAndFragmentDirection = max(dot(lightDirection, -lightVector), 0.0);
				dotLightNormalInverted = max(dot(lightDirection, -theNormal), 0.0) * clamp(8.0 * distanceFactor, 0.0, 1.0);
			}
			else
			{
				dotLightNormalInverted = 1.0 * clamp(8.0 * distanceFactor, 0.0, 1.0);
			}

			// Normal light affection:
			float dotProduct = max(dot(theNormal, lightVector), 0.0);
			float dotProductNormalLight = dotProduct * distanceFactor * differenceLightDirectionAndFragmentDirection * isShadowLight;

			//calculate specular highlights:
			reflectionVector = reflect(-lightVector, theNormal);
			float specular = max(specularFactor * uSpecularPower * pow(max(0.0, dot(surfaceToCamera, reflectionVector)), uSpecularArea), 0.0);
			totalSpecColor += uLightsColors[i].xyz * specular * pow(dotLightNormalInverted, 2.0);

			colorComponentTotal += lightColor * dotProductNormalLight * uLightsColors[i].w * pow(differenceLightDirectionAndFragmentDirection, 5.0);
		}
	}

	colorComponentTotal += totalSpecColor;

	vec4 texColorStandard = vec4(1.0, 1.0, 1.0, 1.0);
	if(uUseTextureDiffuse > 0)
	{
		texColorStandard = texture(uTextureDiffuse, vTexture);
	}
	else
	{
		texColorStandard = vec4(uTintColor, 1.0);
	}

	vec4 texColor = vec4(0.0);
	if(uTextureUseBlend > 0)
	{	
		vec3 blendmapFactor = vec3(texture(uTextureDiffuseBlend, vTextureNoRepetitions));
		vec4 texR = texture(uTextureDiffuseR, vTexture);
		vec4 texG = texture(uTextureDiffuseG, vTexture);
		vec4 texB = texture(uTextureDiffuseB, vTexture);
		texColor = texColorStandard * (1.0 - (blendmapFactor.r + blendmapFactor.g + blendmapFactor.b)) +
						texR * blendmapFactor.r + 
						texG * blendmapFactor.g + 
						texB * blendmapFactor.b;
	}
	else
	{
		texColor = texColorStandard;
	}

	vec3 finalColor = (colorComponentTotal + ambient) * (texColor.xyz * uTintColor);
	
    color.x = finalColor.x;
	color.y = finalColor.y;
	color.z = finalColor.z;
	color.w = 1.0;

	vec3 addedBloom = vec3(max(finalColor.x - 1.0, 0.0), max(finalColor.y - 1.0, 0.0), max(finalColor.z - 1.0, 0.0));
	bloom.x = addedBloom.x + uGlow.x * uGlow.w;
	bloom.y = addedBloom.y + uGlow.y * uGlow.w;
	bloom.z = addedBloom.z + uGlow.z * uGlow.w;
	bloom.w = 1.0;
}