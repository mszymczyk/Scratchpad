#pragma once

#include "../Dx11Util/Dx11Util.h"

namespace spad
{
	namespace FxLib
	{
		struct FxRuntimePass
		{
			~FxRuntimePass()
			{
				DX_SAFE_RELEASE( vs_ );
				DX_SAFE_RELEASE( ps_ );
				DX_SAFE_RELEASE( gs_ );
				DX_SAFE_RELEASE( cs_ );
				DX_SAFE_RELEASE( vsInputSignature_ );
			}

			ID3D11VertexShader* vs_ = nullptr;
			ID3D11PixelShader* ps_ = nullptr;
			ID3D11GeometryShader* gs_ = nullptr;
			ID3D11ComputeShader* cs_ = nullptr;
			ID3D10Blob* vsInputSignature_ = nullptr;
			uint32_t vsInputSignatureHash_ = 0;

			void setVS( ID3D11DeviceContext* deviceContext ) const
			{
				FR_ASSERT( vs_ );
				deviceContext->VSSetShader( vs_, nullptr, 0 );
			}

			void setPS( ID3D11DeviceContext* deviceContext ) const
			{
				FR_ASSERT( ps_ );
				deviceContext->PSSetShader( ps_, nullptr, 0 );
			}
		};

		class FxRuntime
		{
		public:
			const FxRuntimePass& getPass( const char* pass ) const;

		protected:

		private:
			typedef std::map<std::string, FxRuntimePass> PassMap;
			PassMap passes_;
			std::string filename_;

			friend class FxFile;
		};

	} // namespace FxLib

	typedef std::unique_ptr<FxLib::FxRuntime> FxRuntimePtr;

} // namespace spad
