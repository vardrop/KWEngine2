#version 430

#define M_PI 3.141592
#define M_PIE 3.141592 * 2

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
uniform int uAlgorithm;
uniform vec4 uAxes[512];

mat4 rotationMatrix(vec3 axis, float angle)
{
    float s = sin(angle);
    float c = cos(angle);
    float oc = 1.0 - c;
    
    return mat4(oc * axis.x * axis.x + c,           oc * axis.x * axis.y - axis.z * s,  oc * axis.z * axis.x + axis.y * s,  0.0,
                oc * axis.x * axis.y + axis.z * s,  oc * axis.y * axis.y + c,           oc * axis.y * axis.z - axis.x * s,  0.0,
                oc * axis.z * axis.x - axis.y * s,  oc * axis.y * axis.z + axis.x * s,  oc * axis.z * axis.z + c,           0.0,
                0.0,                                0.0,                                0.0,                                1.0);
}

vec3 rotateVectorY(vec3 v, float angle)
{
    return vec3(cos(angle) * v.x - sin(angle) * v.z, v.y, sin(angle) * v.x + cos(angle) *v.z);
}

void main()
{
    float instancePercent = gl_InstanceID / (uNumber + 1);
    vec4 axis = uAxes[gl_InstanceID];
    mat4 rotation = rotationMatrix(axis.xyz, instancePercent * (1.95 * M_PI));
    vec3 lookAt = normalize(vec3(rotation[0][0], rotation[1][0], rotation[2][0]));

	mat4 modelMatrix = mat4(1.0);
    float sizeFactor = max(pow(sin(2.0 * uTime + 1.25), 4.0), 0.0);
    modelMatrix[0][0] *= sizeFactor * uSize * axis.w;
    modelMatrix[0][1] *= sizeFactor * uSize * axis.w;
    modelMatrix[0][2] *= sizeFactor * uSize * axis.w;
    modelMatrix[1][0] *= sizeFactor * uSize * axis.w;
    modelMatrix[1][1] *= sizeFactor * uSize * axis.w;
    modelMatrix[1][2] *= sizeFactor * uSize * axis.w;
    modelMatrix[2][0] *= sizeFactor * uSize * axis.w;
    modelMatrix[2][1] *= sizeFactor * uSize * axis.w;
    modelMatrix[2][2] *= sizeFactor * uSize * axis.w;

    if(uAlgorithm == 0)
    {
        modelMatrix[3][0] = uPosition.x + uSpread * uTime * lookAt.x * axis.w;
        modelMatrix[3][1] = uPosition.y + uSpread * uTime * lookAt.y * axis.w;
        modelMatrix[3][2] = uPosition.z + uSpread * uTime * lookAt.z * axis.w;
    }
    else
    {
      
        modelMatrix[3][0] = uPosition.x + uSpread * uTime * lookAt.x * axis.w + lookAt.x * sin(uTime * 1.5 * M_PI);
        modelMatrix[3][1] = uPosition.y + uSpread * uTime * lookAt.y * axis.w + uSpread * 1.5 * uTime;
        modelMatrix[3][2] = uPosition.z + uSpread * uTime * lookAt.z * axis.w + lookAt.z * sin(uTime * 1.5 * M_PI);
    }


	mat4 mvp = uVP * modelMatrix;
	
	vTexture = aTexture * uTextureTransform;
	gl_Position = mvp * vec4(aPosition, 1.0);
}