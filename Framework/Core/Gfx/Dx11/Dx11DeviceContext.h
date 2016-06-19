#pragma once

#include <Dx11Util\Dx11Util.h>
#include "Dx11InputLayoutCache.h"

namespace spad
{

class Dx11DeviceContext
{
public:
	Dx11DeviceContext( const char* debugName )
		: debugName_( debugName )
	{	}

	~Dx11DeviceContext()
	{
		DeInitialize();
	}

	void Initialize( ID3D11Device* device, ID3D11DeviceContext* context );
	void DeInitialize();

	ID3D11DeviceContext* context = nullptr;
	Dx11InputLayoutCache inputLayoutCache;

private:
	std::string debugName_;
};


} // namespace spad
