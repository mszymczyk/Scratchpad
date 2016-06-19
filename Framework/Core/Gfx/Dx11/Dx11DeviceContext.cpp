#include "Gfx_pch.h"
#include "Dx11DeviceContext.h"

namespace spad
{
	void Dx11DeviceContext::Initialize( ID3D11Device* device, ID3D11DeviceContext* _context )
	{
		context = _context;
		inputLayoutCache.setDxDevice( device );
	}

	void Dx11DeviceContext::DeInitialize()
	{

	}

} // namespace spad
