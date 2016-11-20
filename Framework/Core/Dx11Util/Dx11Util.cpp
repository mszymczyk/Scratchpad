#include "Dx11Util_pch.h"
#include "DDSTextureLoader.h"
#include "WICTextureLoader.h"

using namespace DirectX;

namespace spad
{

namespace debug
{

void Dx11SetDebugName3( ID3D11DeviceChild* obj, const char* format, ... )
{
	va_list	args;

	va_start( args, format );
	char debugName[512];
	int debugNameLen = vsnprintf( debugName, 512, format, args );
	if (debugNameLen >= 512)
	{
		debugNameLen = 511;
		debugName[511] = 0;
	}
	va_end( args );

	obj->SetPrivateData( WKPDID_D3DDebugObjectName, (UINT)debugNameLen, debugName );
}

} // namespace debug


ID3D11ShaderResourceViewPtr spad::LoadTexture( ID3D11Device* device, const char* filePath )
{
	ID3D11DeviceContextPtr context;
	device->GetImmediateContext( &context );

	ID3D11ResourcePtr resource;
	ID3D11ShaderResourceViewPtr srv;

	const std::string extension = GetFileExtension( filePath );
	const std::wstring filePathW = stringToWstring( filePath );
	if ( extension == "DDS" || extension == "dds" )
	{
		DXCall( CreateDDSTextureFromFile( device, filePathW.c_str(), &resource, &srv, 0 ) );
		return srv;
	}
	else
	{
		DXCall( CreateWICTextureFromFile( device, context, filePathW.c_str(), &resource, &srv, 0 ) );
		return srv;
	}
}

ID3D11ShaderResourceView* LoadTexturePtr( ID3D11Device* device, const char* filePath )
{
	ID3D11ShaderResourceViewPtr p = LoadTexture( device, filePath );
	// p Refcount==1
	ID3D11ShaderResourceView* r = p;
	r->AddRef();
	// p Refcount==2
	return r;
	// p destroyed, Refcount==1
}

u32 Dx11HashInputElementDescriptions( const D3D11_INPUT_ELEMENT_DESC* descriptions, u32 nDescriptions )
{
	memstream ms;
	ms.write( reinterpret_cast<const u8*>( descriptions ), nDescriptions * sizeof( D3D11_INPUT_ELEMENT_DESC ) );
	for ( u32 ie = 0; ie < nDescriptions; ++ie )
	{
		const D3D11_INPUT_ELEMENT_DESC& e = descriptions[ie];
		ms.write( e.SemanticName );
	}

	return MurmurHash3( ms.data(), (int)ms.size() );
}

} // namespace spad
