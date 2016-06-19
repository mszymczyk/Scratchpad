#ifdef FX_HEADER
#ifdef FX_PASSES
passes :
{
	Fill = {
		VertexProgram = "fillVp";
		FragmentProgram = "fillFp";
	}
};
#endif // FX_PASSES
#endif // FX_HEADER

//cbuffer cbMaterialParams : register( b2 )
//{
//	float4x4 proj;
//	float4 bgCol;
//};

//Texture2D fontTex;
//SamplerState fontTexSamp;
//
//Texture2D iconTex;
//SamplerState iconTexSamp;

///////////////////////////////////////////////////////////////////////////////
// vertex program
///////////////////////////////////////////////////////////////////////////////
struct vs_output
{
    float4 hpos			: SV_POSITION; // vertex position 
    float2 texCoord0	: TEXCOORD0;   // vertex texture coords 
	//float4 color		: COLOR0;
};

vs_output fillVp(
					//  float4 position : POSITION
					//, float2 texCoord0 : TEXCOORD0
					//, float4 color : COLOR0
					 uint vertexID : SV_VertexID
				)
{
	vs_output OUT;
	//OUT.hpos = mul( proj, float4(position, 0, 1) );
	//OUT.texCoord0 = texCoord0;
	////OUT.color = color;
	if ( vertexID == 0 )
	{
		OUT.hpos = float4( -1, -1, 0, 1 );
		//OUT.hpos = float4( 0.0f, 0.5f, 0.5f, 1 );
		OUT.texCoord0 = float2( 0, 1 );
	}
	else if ( vertexID == 1 )
	{
		OUT.hpos = float4( -1, 3, 0, 1 );
		//OUT.hpos = float4( 1, -1, 0.5f, 1 );
		//OUT.hpos = float4( 0.5f, -0.5f, 0.5f, 1 );
		OUT.texCoord0 = float2( 2, 1 );
	}
	else if ( vertexID == 2 )
	{
		OUT.hpos = float4( 3, -1, 0, 1 );
		//OUT.hpos = float4( -0.5f, -0.5f, 0.5f, 1 );
		OUT.texCoord0 = float2( 0, -1 );
	}

	//OUT.hpos = position;
	//OUT.texCoord0 = float2( 0, 0 );

	return OUT;
}

///////////////////////////////////////////////////////////////////////////////
// fragment program
///////////////////////////////////////////////////////////////////////////////

float4 fillFp( in vs_output IN ) : SV_Target
{
	//float t = fontTex.SampleLevel( fontTexSamp, float2(IN.texCoord0.x, IN.texCoord0.y), 0 ).x;
	//float4 res = float4( t * IN.color );
	//return res;
	return float4( 1, 1, 1, 1 );
}
