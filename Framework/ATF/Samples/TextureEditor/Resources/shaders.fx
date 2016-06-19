// -----------------------------------------------------------------------------
// Original code from SlimDX project.
// Greetings to SlimDX Group. Original code published with the following license:
// -----------------------------------------------------------------------------
/*
* Copyright (c) 2007-2011 SlimDX Group
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/

struct VS_IN
{
	float4 pos : POSITION;
	float4 uv0 : TEXCOORD0;
};

struct PS_IN
{
	float4 pos : SV_POSITION;
	float4 uv0 : TEXCOORD0;
};

cbuffer cbFrameParams : register( b0 )
{
	int xOffset;
	int yOffset;
	int mipLevel;
	int sliceIndex;
	float gamma;
	float gammaExp;
	int flipYExp;
	int redVisible;
	int greenVisible;
	int blueVisible;
	int alphaVisible;
};

Texture2D tex2D : register(t0);
Texture2DArray tex2DArray : register(t0);
//TextureCubeArray texCubeArray : register(t0);
Texture2D tex2DExp : register( t1 );
Texture2DArray tex2DArrayExp : register( t1 );

SamplerState ssPoint : register(s0);

float4 applyVisibilityMask(float4 x)
{
	float4 r = x;
	if (!redVisible)
		r.r = 0;
	if (!greenVisible)
		r.g = 0;
	if (!blueVisible)
		r.b = 0;
	if (!alphaVisible)
		r.a = 1;

	return r;
}

PS_IN VS( VS_IN input )
{
	PS_IN output = (PS_IN)0;
	
	output.pos = input.pos;
	output.uv0 = input.uv0;
	
	return output;
}

float4 PS_Clear( PS_IN input ) : SV_Target
{
	uint2 pixelCoord = ( uint2 )( input.pos.xy );
	uint2 xy = pixelCoord.xy / 8;
	uint xm = xy.x % 2;
	uint ym = xy.y % 2;
	if ( ( xm == 0 && ym == 0 ) || ( xm == 1 && ym == 1 ) )
		return float4( 0, 0.5, 0, 1 );
	else
		return float4( 0.25, 0.25, 0.25, 1 );
}

float4 PS_Clear2( PS_IN input ) : SV_Target
{
	uint2 pixelCoord = ( uint2 )( input.pos.xy );
	if ( pixelCoord.x % 2 == 0 )
		return float4( 0, 0, 0, 1 );
	else
		return float4( 1, 1, 1, 1 );
}

float4 PS_Tex2D_Sample( PS_IN input ) : SV_Target
{
	float4 t = tex2D.SampleLevel( ssPoint, input.uv0.xy, mipLevel );
	t.xyz = pow( t.xyz, gamma );
	t = applyVisibilityMask(t);
	return float4( t );
}

float4 PS_Tex2D_Sample_Exp( PS_IN input ) : SV_Target
{
	float2 uvExp = flipYExp ? float2( input.uv0.x, 1 - input.uv0.y ) : input.uv0.xy;
	float4 tExp = tex2DExp.SampleLevel( ssPoint, uvExp, mipLevel );
	tExp.xyz = pow( tExp.xyz, gammaExp );

	return float4( tExp );
}

float4 PS_Tex2D_Sample_Diff( PS_IN input ) : SV_Target
{
	float4 t = tex2D.SampleLevel( ssPoint, input.uv0.xy, mipLevel );
	t.xyz = pow( t.xyz, gamma );

	float2 uvExp = flipYExp ? float2( input.uv0.x, 1 - input.uv0.y ) : input.uv0.xy;
	float4 tExp = tex2DExp.SampleLevel( ssPoint, uvExp, mipLevel );
	tExp.xyz = pow( tExp.xyz, gammaExp );

	float4 diff = t - tExp;

	return float4( abs( diff.xyz ) * 1, 1 );
}

float4 PS_Tex2DArray_Sample( PS_IN input ) : SV_Target
{
	//input.uv0.x *= 0.25;
	float4 t = tex2DArray.SampleLevel( ssPoint, float3( input.uv0.xy, sliceIndex ), mipLevel );
	t.xyz = pow( t.xyz, gamma );
	return float4( t );
}

float4 PS_Tex2DArray_Sample_Diff( PS_IN input ) : SV_Target
{
	//input.uv0.x *= 0.25;
	float4 t = tex2DArray.SampleLevel( ssPoint, float3( input.uv0.xy, sliceIndex ), mipLevel );
	t.xyz = pow( t.xyz, gamma );
	
	float4 tExp = tex2DArrayExp.SampleLevel( ssPoint, float3( input.uv0.xy, sliceIndex ), mipLevel );
	tExp.xyz = pow( tExp.xyz, gamma );

	return float4( t );
}

float4 PS_TexCube_Sample( PS_IN input ) : SV_Target
{
	//float4 t = texCubeArray.SampleLevel( ssPoint, float4( input.uv0.xyz, sliceIndex ), mipLevel );
	float4 t = tex2DArray.SampleLevel( ssPoint, float3( input.uv0.xy, input.uv0.z + 6 * sliceIndex ), mipLevel );
	t.xyz = pow( t.xyz, gamma );
	return float4( t );
}

float4 PS_Tex2D_Load( PS_IN input ) : SV_Target
{
	int2 pixelCoord = ( int2 )( input.pos.xy );
	float3 t = tex2D.Load( int3( pixelCoord + int2(xOffset, yOffset), mipLevel ) ).xyz;
	return float4( t, 1 );
}

float4 PS_Present( PS_IN input ) : SV_Target
{
	float4 t = tex2D.SampleLevel( ssPoint, input.uv0.xy, 0 );
	t = saturate( t );
	//t = pow( t, 1 / 2.2 );
	//t *= 64;
	//t = floor( t );
	//t /= 64;
	//t = pow( t, 2.2 );
	//t = pow( t, 1 / 2.2 );
	return float4( t.xyz, 1 );
	//return float4( 0.5, 0.5, 0.5, 1 );
}

float4 PS_Gradient( PS_IN input ) : SV_Target
{
	//input.uv0.xyz *= 0.25;
	float3 t = input.uv0.xyz;
	//t = pow( t, 2.2 );
	return float4( t, 1 );
}

technique10 Render
{
	pass Clear
	{
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetPixelShader( CompileShader( ps_4_0, PS_Clear() ) );
	}
	
	pass Clear2
	{
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetPixelShader( CompileShader( ps_4_0, PS_Clear2() ) );
	}

	pass Tex2D_Sample
	{
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetPixelShader( CompileShader( ps_4_0, PS_Tex2D_Sample() ) );
	}

	pass Tex2D_Sample_Exp
	{
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetPixelShader( CompileShader( ps_4_0, PS_Tex2D_Sample_Exp() ) );
	}

	pass Tex2D_SampleDiff
	{
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetPixelShader( CompileShader( ps_4_0, PS_Tex2D_Sample_Diff() ) );
	}

	pass Tex2DArray_Sample
	{
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetPixelShader( CompileShader( ps_4_0, PS_Tex2DArray_Sample() ) );
	}

	pass Tex2DArray_Sample_Diff
	{
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetPixelShader( CompileShader( ps_4_0, PS_Tex2DArray_Sample_Diff() ) );
	}

	pass TexCube_Sample
	{
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetPixelShader( CompileShader( ps_4_0, PS_TexCube_Sample() ) );
	}

	pass Tex2D_Load
	{
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetPixelShader( CompileShader( ps_4_0, PS_Tex2D_Load() ) );
	}

	pass Present
	{
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetPixelShader( CompileShader( ps_4_0, PS_Present() ) );
	}

	pass Gradient
	{
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetPixelShader( CompileShader( ps_4_0, PS_Gradient() ) );
	}
}