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
	float3 pos : POSITION;
	float3 nrm : NORMAL;
};

struct PS_IN
{
	float4 hpos : SV_POSITION;
	float3 worldPos : TEXCOORD0;
	float3 worldNormal : TEXCOORD1;
};

cbuffer cbFrameParams : register( b0 )
{
	float4x4 worldViewProj;
	float4x4 world;
	float4x4 worldIT;
	//float4 color;
};

PS_IN VS( VS_IN input )
{
	PS_IN output = (PS_IN)0;

	output.hpos = mul( worldViewProj, float4( input.pos.xyz, 1 ) );
	output.worldPos = mul( world, float4( input.pos.xyz, 1 ) ).xyz;
	output.worldNormal = mul( (float3x3)worldIT, input.nrm.xyz );
	
	return output;
}

float4 PS_Color( PS_IN input ) : SV_Target
{
	//return color;
	//return float4( 0, 0, 1, 1 );
	//return float4( input.worldNormal, 1 );

	float3 lightPosition = float3( 4, 4, 4 );
	float3 worldPosition = input.worldPos;
	float3 worldNormal = input.worldNormal;
	float3 viewDir = normalize( -lightPosition );

	float3 lightDir = lightPosition - worldPosition; //3D position in space of the surface
	float distance = length( lightDir );
	lightDir = lightDir / distance; // = normalize( lightDir );

	//Intensity of the diffuse light. Saturate to keep within the 0-1 range.
	float NdotL = dot( worldNormal, lightDir );
	float intensity = saturate( NdotL );

	// Calculate the diffuse light factoring in light color, power and the attenuation
	float3 diffuse = float3( 0, 1, 0 ) * intensity;

	////Calculate the half vector between the light vector and the view vector.
	////This is typically slower than calculating the actual reflection vector
	//// due to the normalize function's reciprocal square root
	//float3 H = normalize( lightDir + viewDir );

	////Intensity of the specular light
	//float NdotH = dot( worldNormal, H );
	//float specularHardness = 64;
	//intensity = pow( saturate( NdotH ), specularHardness );
	//float3 specular = intensity;

	//return float4( specular + diffuse, 1 );
	return float4( diffuse, 1 );
}

technique10 Render
{
	pass Color
	{
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetPixelShader( CompileShader( ps_4_0, PS_Color() ) );
	}
}