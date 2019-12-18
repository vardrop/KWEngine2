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
uniform sampler2D uTextureEmissive;
uniform int uUseTextureEmissive;
uniform sampler2D uTextureLightmap;
uniform int uUseTextureLightmap;
uniform sampler2DShadow uTextureShadowMap;

uniform vec3 uBaseColor;
uniform vec4 uGlow;
uniform vec3 uTintColor;
uniform vec4 uEmissiveColor;
uniform float uSunAmbient;
uniform vec3 uSunPosition;
uniform vec3 uSunDirection;
uniform vec4 uSunIntensity;
uniform vec3 uCameraPos;

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
	vec3 texColor = vec3(1.0, 1.0, 1.0);
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
            theNormal = texture(uTextureNormal, vTexture).xyz * 2.0 - 1.0;
            theNormal = vTBN * theNormal;
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
	float darkeningAbsolute = calculateDarkening(dotNormalLightShadow, vShadowCoord);
	float darkening = max(darkeningAbsolute, uSunAmbient);


	// Lighting loop:
	float specularFactor = 1.0;
	if(uUseTextureSpecular > 0)
	{
		specularFactor = texture(uTextureSpecular, vTexture).r;
	}
	vec3 totalSpecColor = vec3(0);

	//Specular highlights from sun:
	vec3 reflectionVector = reflect(-uSunDirection, theNormal);
    float specular = specularFactor * uSpecularPower * pow(max(0.0, dot(surfaceToCamera, reflectionVector)), uSpecularArea);
    vec3 specColorSun = uSunIntensity.xyz * specular * uSunIntensity.w * darkeningAbsolute;
	totalSpecColor += specColorSun;

	vec3 ambient = vec3(1.0);
	if(uUseTextureLightmap == 0)
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

	vec3 finalColor = (colorComponentTotal + ambient) * uBaseColor.xyz * uTintColor.xyz * texColor;
	//vec3 finalColor = uBaseColor.xyz * uTintColor.xyz * texColor;
	
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