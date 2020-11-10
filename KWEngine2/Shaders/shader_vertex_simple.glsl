#version 430

in		vec3 aPosition;

uniform mat4 uMVP;

void main()
{
	gl_PointSize = 8.0;
	gl_Position = uMVP * vec4(aPosition, 1.0);
}