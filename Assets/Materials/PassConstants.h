#ifndef PASS_CONSTANTS_H
#define PASS_CONSTANTS_H

#include "HlslFrameworkInterop.h"

// follows vector math convention, matrices are column major
// multiplication must be done: vecResult = mat * vec

//cbuffer CbPassConstants : register( b0 )
CBUFFER CbPassConstants REGISTER_B(0)
{
	float4x4 View; // world to view/camera
	float4x4 Projection; // view/camera to clip space
	float4x4 ViewProjection; // proj * view, world to clip space
};

//cbuffer CbObjectConstants : register( b1 )
CBUFFER CbObjectConstants REGISTER_B(1)
{
	float4x4 World; // object to world
	float4x4 WorldIT; // object to world, transpose(inverse(World)), for transforming normals
};


#endif // PASS_CONSTANTS_H
