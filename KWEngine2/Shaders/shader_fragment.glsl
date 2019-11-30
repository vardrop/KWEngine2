#version 430

in		vec2 vTexture;
in		vec2 vTexture2;
in		vec3 vPosition;
in		vec3 vNormal;
in		vec4 vShadowCoord;
in		mat3 vTBN;

uniform int uTextureUse;
uniform int uTextureUseNormalMap;
uniform int uTextureUseSpecularMap;
uniform int uTextureUseLightMap;
uniform sampler2D uTexture;
uniform sampler2D uNormalMap;
uniform sampler2D uSpecularMap;
uniform sampler2D uLightMap;
uniform float uSpecularPower;
uniform float uSpecularArea;
uniform sampler2DShadow uTextureShadowMap;
uniform vec4 uBaseColor;
uniform vec4 uEmissiveColor;
uniform vec4 uTintColor;
uniform vec3 uSunPosition;
uniform vec3 uSunDirection;
uniform vec4 uSunIntensity;
uniform float uSunAmbient;
uniform vec4 uGlow;
uniform vec3 uCameraPos;

uniform vec4 uLightsPositions[3];
uniform vec4 uLightsTargets[3];
uniform vec4 uLightsColors[3];
uniform int uLightCount;

layout(location = 0) out vec4 color;
layout(location = 1) out vec4 bloom;

float calculateDarkening(float cosAlpha)
{
	float darkening = 0.0;
	darkening += textureProjOffset(uTextureShadowMap, vShadowCoord, ivec2(-1,-1));
	darkening += textureProjOffset(uTextureShadowMap, vShadowCoord, ivec2(-1,1));
	darkening += textureProjOffset(uTextureShadowMap, vShadowCoord, ivec2(0,0));
	darkening += textureProjOffset(uTextureShadowMap, vShadowCoord, ivec2(1,1));
	darkening += textureProjOffset(uTextureShadowMap, vShadowCoord, ivec2(1,-1));
	darkening /= 5.0;
	return darkening;
}

float calculateDarkening2(float cosTheta, vec4 shadowCoord)
{
	float bias = 0.005 * sqrt ( 1.0f - cosTheta * cosTheta   ) / cosTheta;
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

float calculateDarkening3(float cosTheta, vec4 shadowCoord)
{
	float bias = 0.005 * sqrt ( 1.0f - cosTheta * cosTheta   ) / cosTheta;
	bias = clamp(bias, 0.0 ,0.01);
	shadowCoord.z = shadowCoord.z - bias;
	return textureProjOffset(uTextureShadowMap, shadowCoord, ivec2(0,0));
}

void main()
{
	vec3 texColor = vec3(1.0, 1.0, 1.0);
	vec4 texColor4 = vec4(1.0, 1.0, 1.0, 1.0);
	if(uTextureUse > 0)
	{
		texColor4 = texture(uTexture, vTexture);
		if(texColor4.w <= 0.5)
		{
			discard;
		}
		texColor = texColor4.xyz;
		if(uTextureUseLightMap > 0)
		{
			texColor *= texture(uLightMap, vTexture2).xyz;
		}
	}

	vec4 shadowCoord = vShadowCoord;
	
	vec3 theNormal = vec3(0);
	if(uTextureUseNormalMap > 0)
    {
            theNormal = texture(uNormalMap, vTexture).xyz * 2.0 - 1.0;
            theNormal = normalize(vTBN * theNormal);
    }
    else
    {
            theNormal = vNormal;
    }

	float specularFactor = 1.0;
	if(uTextureUseSpecularMap > 0)
	{
		specularFactor = texture(uSpecularMap, vTexture).r;
	}
	
	vec3 surfaceToCamera = normalize(uCameraPos - vPosition);
	vec3 fragmentToSun = normalize(uSunPosition - vPosition);

	float dotNormalLight = max(dot(theNormal, uSunDirection), 0.0);//step(0.01, dot(theNormal, uSunDirection));
	float dotNormalLightShadow = max(dot(theNormal, fragmentToSun), 0.0);
	float darkeningAbsolute = calculateDarkening2(dotNormalLightShadow, shadowCoord);
	float darkening = max(darkeningAbsolute, uSunAmbient);

	vec3 totalSpecColor = vec3(0);

	//Specular highlights from sun:
	vec3 reflectionVector = reflect(-uSunDirection, theNormal);
    float specular = specularFactor * uSpecularPower * pow(max(0.0, dot(surfaceToCamera, reflectionVector)), uSpecularArea);
    vec3 specColorSun = uSunIntensity.xyz * specular * uSunIntensity.w * darkeningAbsolute;
	totalSpecColor += specColorSun;

	vec3 ambient = vec3(1.0);
	if(uTextureUseLightMap == 0)
	{
		ambient = uSunIntensity.xyz * (uSunIntensity.w * min(max(dotNormalLight, uSunAmbient), darkening));
	}
	vec3 emissive = uEmissiveColor.xyz * uEmissiveColor.w;
	ambient += emissive;
	
	vec3 colorComponentTotal = vec3(0);
	for(int i = 0; i < uLightCount; i++)
	{
		vec3 lightPos = vec3(uLightsPositions[i]);
        vec3 lightColor = uLightsColors[i].xyz * uLightsColors[i].w;
        vec3 lightDirection = normalize(uLightsTargets[i].xyz - lightPos);
		

		vec3 lightVector = lightPos - vPosition;
		float distance = dot(lightVector, lightVector);
        lightVector = normalize(lightVector);

		float differenceLightDirectionAndFragmentDirection = 1.0;
		if(uLightsTargets[i].w > 0.0){ // directional
			differenceLightDirectionAndFragmentDirection = max(dot(lightDirection, -lightVector), 0.0);
		}

		//calculate spec:
		reflectionVector = reflect(-lightVector, theNormal);
		specular = specularFactor * uSpecularPower * pow(max(0.0, dot(surfaceToCamera, reflectionVector)), uSpecularArea) * differenceLightDirectionAndFragmentDirection;
		totalSpecColor += uLightsColors[i].xyz * specular; // * uLightsPositions[i].w;

        float dotProductNormalLight = max(dot(vNormal, lightVector), 0.0) * (uLightsPositions[i].w / distance);

		lightColor = lightColor * dotProductNormalLight * uLightsColors[i].w * pow(differenceLightDirectionAndFragmentDirection, 5.0); // .w includes distance multiplier factor
		colorComponentTotal += lightColor;
	}

	colorComponentTotal += totalSpecColor;

	vec3 finalColor = (colorComponentTotal + ambient) * uBaseColor.xyz * (uTintColor.xyz * uTintColor.w) * texColor;

    color.x = finalColor.x;
	color.y = finalColor.y;
	color.z = finalColor.z;
	color.w = 1.0;
	vec3 addedBloom = vec3(max(finalColor.x - 1.0, 0.0), max(finalColor.y - 1.0, 0.0), max(finalColor.z - 1.0, 0.0)) + (uEmissiveColor.xyz * 0.5);
	bloom.x = addedBloom.x + uGlow.x * uGlow.w;
	bloom.y = addedBloom.y + uGlow.y * uGlow.w;
	bloom.z = addedBloom.z + uGlow.z * uGlow.w;
	bloom.w = 1.0;
}