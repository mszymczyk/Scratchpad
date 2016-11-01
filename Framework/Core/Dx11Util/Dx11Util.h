#pragma once

#include "..\Util\Def.h"
#include "..\Util\Exceptions.h"

// includes
//
// COM Support
//#include <comip.h>
#include <comdef.h>

#include <dxgi.h>
#include <d3d9.h>
#include <d3d11.h>
#include <D3Dcompiler.h>

// Static Lib Imports
#pragma comment(lib, "dxguid.lib")
//#pragma comment(lib, "D3D9.lib")
#pragma comment(lib, "D3D10.lib")
#pragma comment(lib, "D3D11.lib")
#pragma comment(lib, "DXGI.lib")
#pragma comment(lib, "d3dcompiler.lib")


#include "DxPointers.h"

namespace spad
{

#define DX_SAFE_RELEASE(x) if(x) { x->Release(); x = nullptr; }

// Exception thrown when a DirectX Function fails
//
class DxException : public Exception
{
public:
    DxException( std::string&& exceptionMessage, HRESULT hres, const char* file, int line )
        : Exception( std::move( exceptionMessage ), file, line )
        , hres_( hres )
    {	}

protected:
    HRESULT hres_;
};

#define THROW_DX_EXCEPTION(msg, hres) throw DxException( msg, hres, __FILE__, __LINE__ )


#define DXCall(x)                                                           \
    __pragma(warning(push))                                                 \
    __pragma(warning(disable:4127))                                         \
    do                                                                      \
    {                                                                       \
        HRESULT hres = x;                                                   \
        SPAD_ASSERT2(SUCCEEDED(hres), #x "failed");                           \
    }                                                                       \
    while(0)                                                                \
    __pragma(warning(pop))


template<class T>
inline void Dx11SetDebugName( T* obj, const char* debugName )
{
    obj->SetPrivateData( WKPDID_D3DDebugObjectName, (UINT)strlen( debugName ), debugName );
}

template<> inline void Dx11SetDebugName<ID3D11DeviceChild>( ID3D11DeviceChild* obj, const char* debugName )
{
    obj->SetPrivateData( WKPDID_D3DDebugObjectName, (UINT)strlen( debugName ), debugName );
}

#define Dx11SetDebugName2( obj ) Dx11SetDebugName( obj, #obj )

void Dx11SetDebugName3( ID3D11DeviceChild* obj, const char* format, ... );

// Texture loading
ID3D11ShaderResourceViewPtr LoadTexture( ID3D11Device* device, const char* filePath );
ID3D11ShaderResourceView* LoadTexturePtr( ID3D11Device* device, const char* filePath );

u32 Dx11HashInputElementDescriptions( const D3D11_INPUT_ELEMENT_DESC* descriptions, u32 nDescriptions );

} // namespace spad
