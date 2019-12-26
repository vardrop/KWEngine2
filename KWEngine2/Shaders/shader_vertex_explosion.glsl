#version 430
#define M_PI 3.1415926535897932384626433832795

in		vec3 aPosition;
in		vec2 aTexture;

out		vec2 vTexture;

uniform vec2 uTextureTransform;
uniform mat4 uVP;
uniform float uTime;
uniform float uSpread;
uniform float uNumber;
uniform float uSize;
uniform vec3 uPosition;
uniform vec4 uAxes[512];

mat4 rotationMatrix(vec3 axis, float angle)
{
    axis = normalize(axis);
    float s = sin(angle);
    float c = cos(angle);
    float oc = 1.0 - c;
    
    return mat4(oc * axis.x * axis.x + c,           oc * axis.x * axis.y - axis.z * s,  oc * axis.z * axis.x + axis.y * s,  0.0,
                oc * axis.x * axis.y + axis.z * s,  oc * axis.y * axis.y + c,           oc * axis.y * axis.z - axis.x * s,  0.0,
                oc * axis.z * axis.x - axis.y * s,  oc * axis.y * axis.z + axis.x * s,  oc * axis.z * axis.z + c,           0.0,
                0.0,                                0.0,                                0.0,                                1.0);
}

void main()
{
    float instancePercent = (gl_InstanceID + 1) / uNumber;
    float instancePercentNormalized = instancePercent - 0.5;
    vec4 axis = uAxes[gl_InstanceID % int(uNumber)];
    mat4 rotation = rotationMatrix(axis.xyz, instancePercent * (2 * M_PI));
	mat4 modelMatrix = mat4(1.0);
    modelMatrix[3][0] = uPosition.x; 
    modelMatrix[3][1] = uPosition.y; 
    modelMatrix[3][2] = uPosition.z; 

    
    //float sizeFactor = -2.8 * ((uTime - 0.4) * (uTime - 0.4)) + 1.0;
    float sizeFactor = 5.0 * (  pow(uTime -1.0, 4.0) * -1.12 - 1.28 * pow(uTime - 1.0, 3.0)  );
    modelMatrix[0][0] *= sizeFactor * uSize * axis.w;
    modelMatrix[0][1] *= sizeFactor * uSize * axis.w;
    modelMatrix[0][2] *= sizeFactor * uSize * axis.w;
    modelMatrix[1][0] *= sizeFactor * uSize * axis.w;
    modelMatrix[1][1] *= sizeFactor * uSize * axis.w;
    modelMatrix[1][2] *= sizeFactor * uSize * axis.w;
    modelMatrix[2][0] *= sizeFactor * uSize * axis.w;
    modelMatrix[2][1] *= sizeFactor * uSize * axis.w;
    modelMatrix[2][2] *= sizeFactor * uSize * axis.w;

    modelMatrix = rotation  * modelMatrix;
    vec3 lookAt = normalize(vec3(modelMatrix[0][0], modelMatrix[1][0], modelMatrix[2][0]));
    
    modelMatrix[3][0] = uPosition.x + uSpread * uTime * lookAt.x * axis.w;
    modelMatrix[3][1] = uPosition.y + uSpread * uTime * lookAt.y * axis.w;
    modelMatrix[3][2] = uPosition.z + uSpread * uTime * lookAt.z * axis.w;

	mat4 mvp = uVP * modelMatrix;
	
	vTexture = aTexture * uTextureTransform;
	gl_Position = mvp * vec4(aPosition, 1.0);
}