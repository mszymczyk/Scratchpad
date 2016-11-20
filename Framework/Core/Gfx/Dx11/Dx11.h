#pragma once

#include <Dx11Util/Dx11Util.h>
#include <Gfx\Dx11\Dx11DeviceContext.h>

namespace spad
{

	class Dx11
	{
	public:
		~Dx11();

		struct Param
		{
			HWND hWnd = nullptr;
			DXGI_FORMAT backBufferFormat_ = DXGI_FORMAT_R8G8B8A8_UNORM_SRGB;
			u32 backBufferWidth_ = 1280;
			u32 backBufferHeight_ = 720;
			bool debugDevice = false;
		};

		bool StartUp( const Param& param );
	
		void SetBackBufferRT();
		void Present();

		ID3D11Device* getDevice() const
		{
			return device_;
		}

		ID3D11DeviceContext* getImmediateContext() const
		{
			return immediateContext_;
		}

		Dx11DeviceContext& getImmediateContextWrapper() const
		{
			return *immediateContextWrapper_;
		}

		ID3D11RenderTargetView* getBackBufferRTV() const
		{
			return backBufferRTV_;
		}
		u32 getBackBufferWidth() const { return backBufferWidth_; }
		u32 getBackBufferHeight() const { return backBufferHeight_; }

		//Dx11InputLayoutCache& getImmediateContextInputLayoutCache()
		//{
		//	return *inputLayoutCache_;
		//}

	private:
		void ShutDown();

	protected:
		ID3D11Device* device_ = nullptr;
		ID3D11DeviceContext* immediateContext_ = nullptr;
		IDXGISwapChain* swapChain_ = nullptr;
		ID3D11Texture2D* backBufferTexture_ = nullptr;
		ID3D11RenderTargetView* backBufferRTV_ = nullptr;
		D3D_FEATURE_LEVEL featureLevel_ = D3D_FEATURE_LEVEL_9_1;

		DXGI_FORMAT backBufferFormat_ = DXGI_FORMAT_R8G8B8A8_UNORM_SRGB;
		u32 backBufferWidth_ = 1280;
		u32 backBufferHeight_ = 720;
		bool debugDevice_ = false;

		std::unique_ptr<Dx11DeviceContext> immediateContextWrapper_;
	};

	extern ID3D11Device* gDx11Device;

} // namespace spad