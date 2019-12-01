#version 430

in		vec3 aPosition;

uniform mat4 uMVP;

void main()
{
	gl_Position = uMVP * aPosition;
}