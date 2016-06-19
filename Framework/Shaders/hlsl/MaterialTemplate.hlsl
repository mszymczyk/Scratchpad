#ifdef FX_HEADER

#ifdef FX_PASSES
passes :
{
	Pass0 = {
		VertexProgram = "MainVp";
		FragmentProgram = "MainFp";
	}
};
#endif // FX_PASSES
#endif // FX_HEADER

#include "PassConstants.h"

struct MaterialParameters
{
	float2 texCoord0;
};

UNIFORMS_DECLARATION

RESOURCE_DECLARATION

///////////////////////////////////////////////////////////////////////////////
// vertex program
///////////////////////////////////////////////////////////////////////////////
struct vs_output
{
    float4 hpos			: SV_POSITION; // vertex position in clip space
	float3 normalWS		: NORMAL;      // vertex normal in world space
	float2 texCoord0	: TEXCOORD0;   // vertex texture coords 
};

vs_output MainVp(
					  float3 position : POSITION
					, float3 normal : NORMAL
					, float2 texCoord0 : TEXCOORD0
				)
{
	vs_output OUT;

	float4 positionWorld = mul( World, float4( position, 1 ) );

	OUT.hpos = mul( ViewProjection, positionWorld );
	OUT.normalWS = mul( ( float3x3 )WorldIT, normal );
	OUT.texCoord0 = texCoord0;

	return OUT;
}

///////////////////////////////////////////////////////////////////////////////
// fragment program
///////////////////////////////////////////////////////////////////////////////

void MainFp( in vs_output IN,
		out float4 color0 : SV_Target
	)
{
	MaterialParameters mp = (MaterialParameters)0;

	float3 baseColor = float3( 1, 1, 1 );

SHADER_EVALUATION

	color0 = float4( baseColor, 1 );
}
