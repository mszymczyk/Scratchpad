#ifndef FONT_CONSTANTS_H
#define FONT_CONSTANTS_H

#include "HlslFrameworkInterop.h"

// follows vector math convention, matrices are column major
// multiplication must be done: vecResult = mat * vec

CBUFFER CbTextRendererConstants REGISTER_B(0)
{
	float4x4 Transform; // world to view/camera
	float4 ViewportSize; // width, height, 1.0f/width, 1.0f/height
	float4 TextureSize; // width, height, 1.0f/width, 1.0f/height
	float4 Color;
};


#endif // FONT_CONSTANTS_H
