#version 430
 
in		vec3 aPosition;

out		vec3 vTexture;

uniform mat4 uMVP;
 
void main()
{
    vTexture = aPosition; 
    gl_Position = (uMVP * vec4(aPosition, 1.0)).xyww;
}