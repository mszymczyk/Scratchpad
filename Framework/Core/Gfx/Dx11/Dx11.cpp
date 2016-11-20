#include "Gfx_pch.h"
#include "Dx11.h"
#include "Dx11DeviceStates.h"
#include <FrameworkSettings/FrameworkSettings_General.h>

#include <Initguid.h>
#include <dxgidebug.h>

typedef HRESULT( WINAPI *MyDXGIGetDebugInterface )( REFIID riid, void **ppDebug );

namespace spad
{
ID3D11Device* gDx11Device;

Dx11::~Dx11()
{
	ShutDown();
}

bool Dx11::StartUp( const Param& param )
{
	backBufferFormat_ = param.backBufferFormat_;
	backBufferWidth_ = param.backBufferWidth_;
	backBufferHeight_ = param.backBufferHeight_;
	debugDevice_ = param.debugDevice;

	DXGI_SWAP_CHAIN_DESC desc;
	ZeroMemory( &desc, sizeof( DXGI_SWAP_CHAIN_DESC ) );

	desc.BufferCount = 2;
	desc.BufferDesc.Format = backBufferFormat_;
	desc.BufferDesc.Width = backBufferWidth_;
	desc.BufferDesc.Height = backBufferHeight_;
	desc.BufferDesc.Scaling = DXGI_MODE_SCALING_UNSPECIFIED;
	desc.BufferDesc.ScanlineOrdering = DXGI_MODE_SCANLINE_ORDER_UNSPECIFIED;
	desc.BufferDesc.RefreshRate.Numerator = 60;
	desc.BufferDesc.RefreshRate.Denominator = 1;
	desc.SampleDesc.Count = 1;
	desc.SampleDesc.Quality = 0;
	desc.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
	desc.Flags = DXGI_SWAP_CHAIN_FLAG_ALLOW_MODE_SWITCH;
	desc.SwapEffect = DXGI_SWAP_EFFECT_DISCARD;
	desc.OutputWindow = param.hWnd;
	desc.Windowed = true;

	u32 flags = 0;
	if ( param.debugDevice )
		flags |= D3D11_CREATE_DEVICE_DEBUG;

	DXCall( D3D11CreateDeviceAndSwapChain( NULL, D3D_DRIVER_TYPE_HARDWARE, NULL, flags, NULL, 0, D3D11_SDK_VERSION, &desc, &swapChain_, &device_, NULL, &immediateContext_ ) );

	gDx11Device = device_;
	featureLevel_ = device_->GetFeatureLevel();

	DXCall( swapChain_->GetBuffer( 0, __uuidof( ID3D11Texture2D ), reinterpret_cast<void**>( &backBufferTexture_ ) ) );
	DXCall( device_->CreateRenderTargetView( backBufferTexture_, NULL, &backBufferRTV_ ) );

	if ( debugDevice_ )
	{
		debug::Dx11SetDebugName2( backBufferTexture_ );
		debug::Dx11SetDebugName2( backBufferRTV_ );
	}

	immediateContextWrapper_ = std::make_unique<Dx11DeviceContext>( "ImmediateContext" );
	immediateContextWrapper_->Initialize( device_, immediateContext_ );

	BlendStates::Initialize( device_ );
	RasterizerStates::Initialize( device_ );
	DepthStencilStates::Initialize( device_ );
	SamplerStates::Initialize( device_ );

	return true;
}

void Dx11::SetBackBufferRT()
{
	// Set default render targets
	immediateContext_->OMSetRenderTargets( 1, &backBufferRTV_, NULL );

	// Setup the viewport
	D3D11_VIEWPORT vp;
	vp.Width = static_cast<float>( backBufferWidth_ );
	vp.Height = static_cast<float>( backBufferHeight_ );
	vp.MinDepth = 0.0f;
	vp.MaxDepth = 1.0f;
	vp.TopLeftX = 0;
	vp.TopLeftY = 0;
	immediateContext_->RSSetViewports( 1, &vp );
}

void Dx11::Present()
{
	DXCall( swapChain_->Present( std::min(static_cast<int>(gFrameworkSettings->mGeneral->vsync), 3), 0 ) );
}

void Dx11::ShutDown()
{
	immediateContextWrapper_.reset();

	if ( immediateContext_ )
	{
		immediateContext_->ClearState();
		immediateContext_->Flush();
		immediateContext_->Release();
		immediateContext_ = nullptr;
	}

	DX_SAFE_RELEASE( backBufferRTV_ );
	DX_SAFE_RELEASE( backBufferTexture_ );

	if ( swapChain_ )
	{
		swapChain_->Release();
		swapChain_ = nullptr;
	}

	SamplerStates::DeInitialize();
	DepthStencilStates::DeInitialize();
	RasterizerStates::DeInitialize();
	BlendStates::DeInitialize();

	if ( device_ )
	{
		gDx11Device = device_;
		device_->Release();
		device_ = nullptr;

		if ( debugDevice_ )
		{
			IDXGIDebug* dxgiDebug = NULL;

			HMODULE moduleHandle = GetModuleHandle( TEXT( "Dxgidebug.dll" ) );
			MyDXGIGetDebugInterface funcPtr = (MyDXGIGetDebugInterface)GetProcAddress( moduleHandle, "DXGIGetDebugInterface" );

			HRESULT hr = funcPtr( __uuidof( IDXGIDebug ), reinterpret_cast<void**>( &dxgiDebug ) );
			if ( SUCCEEDED( hr ) && dxgiDebug )
			{
				dxgiDebug->ReportLiveObjects( DXGI_DEBUG_ALL, DXGI_DEBUG_RLO_ALL );
				dxgiDebug->Release();
			}
		}
	}
}

} // namespace spad
