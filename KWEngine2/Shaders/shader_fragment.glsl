#version 430

in		vec3 vPosition;
in		vec3 vNormal;
in		vec2 vTexture;
in		vec2 vTexture2;
in		mat3 vTBN;
in		vec4 vShadowCoord;

uniform sampler2D uTextureDiffuse;
uniform int uUseTextureDiffuse;

uniform sampler2D uTextureNormal;
uniform int uUseTextureNormal;

uniform sampler2D uTextureSpecular;
uniform int uUseTextureSpecular;
uniform int uRoughness;

uniform sampler2D uTextureEmissive;
uniform int uUseTextureEmissive;

uniform sampler2D uTextureLightmap;
uniform int uUseTextureLightmap;

uniform sampler2DShadow uTextureShadowMap;

uniform float uOpacity;
uniform vec3 uBaseColor;
uniform vec4 uGlow;
uniform vec4 uOutline;
uniform vec3 uTintColor;
uniform vec4 uEmissiveColor;
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
	darkening += textureProjOffset(uTextureShadowMap, shadowCoord, ivec2(-1,-1));
	darkening += textureProjOffset(uTextureShadowMap, shadowCoord, ivec2(-1,1));
	darkening += textureProjOffset(uTextureShadowMap, shadowCoord, ivec2(0,0));
	darkening += textureProjOffset(uTextureShadowMap, shadowCoord, ivec2(1,1));
	darkening += textureProjOffset(uTextureShadowMap, shadowCoord, ivec2(1,-1));
	darkening /= 5.0;
	return darkening;
}

void main()
{
	// Texture mapping:
	vec3 texColor = uBaseColor;
	vec4 texColor4 = vec4(1.0, 1.0, 1.0, 1.0);
	if(uUseTextureDiffuse > 0)
	{
		texColor4 = texture(uTextureDiffuse, vTexture);
		if(texColor4.w <= 0.5)
		{
			discard;
		}
		texColor = texColor4.xyz;
		if(uUseTextureLightmap > 0)
		{
			texColor *= texture(uTextureLightmap, vTexture2).xyz;
		}
	}

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
	float dotNormalLight = max(dot(theNormal, uSunDirection), 0.0);								
	float dotNormalLightShadow = max(dot(theNormal, fragmentToSun), 0.0);
	float darkeningAbsolute = max(calculateDarkening(dotNormalLightShadow, vShadowCoord), 0.0);
	float darkening = max(darkeningAbsolute, uSunAmbient);
	
	
	vec3 ambient = vec3(0.0);
	vec3 totalSpecColor = vec3(0);
	float specularFactor = 1.0;
	if(uUseTextureSpecular > 0)
	{
		if(uRoughness > 0)
			specularFactor = 1.0 - texture(uTextureSpecular, vTexture).r;
		else
			specularFactor = texture(uTextureSpecular, vTexture).r;
	}
	
	vec3 reflectionVector = reflect(-uSunDirection, vNormal);
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

	vec3 emissive = vec3(0.0);
	if(uUseTextureEmissive > 0)
	{
		emissive = texture(uTextureEmissive, vTexture).xyz;
	}
	else
	{
		emissive = uEmissiveColor.xyz * uEmissiveColor.w;
	}
	ambient += emissive;

	vec3 colorComponentTotal = vec3(0.0);
	if(uLightAffection > 0)
	{
		for(int i = 0; i < uLightCount; i++)
		{
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
			float dotProductNormalLight = dotProduct * distanceFactor * differenceLightDirectionAndFragmentDirection;

			//calculate specular highlights:
			reflectionVector = reflect(-lightVector, theNormal);
			float specular = max(specularFactor * uSpecularPower * pow(max(0.0, dot(surfaceToCamera, reflectionVector)), uSpecularArea), 0.0);
			totalSpecColor += uLightsColors[i].xyz * specular * pow(dotLightNormalInverted, 2.0);

			colorComponentTotal += lightColor * dotProductNormalLight * uLightsColors[i].w * pow(differenceLightDirectionAndFragmentDirection, 5.0);
		}
	}

	colorComponentTotal += totalSpecColor;

	vec3 finalColor =(colorComponentTotal + ambient) * uBaseColor.xyz * uTintColor.xyz * texColor;
	
	float dotOutline = max(1.0 - 4.0 * pow(abs(dot(uCameraDirection, vNormal)), 2.0), 0.0) * uOutline.w;

    color.x = finalColor.x + uOutline.x * dotOutline * 0.6;
	color.y = finalColor.y + uOutline.y * dotOutline * 0.6;
	color.z = finalColor.z + uOutline.z * dotOutline * 0.6;
	color.w = uOpacity;

	vec3 addedBloom = vec3(max(finalColor.x - 1.0, 0.0), max(finalColor.y - 1.0, 0.0), max(finalColor.z - 1.0, 0.0)) + (uEmissiveColor.xyz * 0.5);
	bloom.x = addedBloom.x + uGlow.x * uGlow.w + uOutline.x * dotOutline;
	bloom.y = addedBloom.y + uGlow.y * uGlow.w + uOutline.y * dotOutline;
	bloom.z = addedBloom.z + uGlow.z * uGlow.w + uOutline.z * dotOutline;
	bloom.w = 1.0;


}