#pragma once

#include <Util/Exceptions.h>

namespace spad
{
	namespace FxLib
	{
		struct FxFileHlslCompileOptions
		{
			FxFileHlslCompileOptions()
				: dxCompilerFlags( 0 )
				, generatePreprocessedOutput( false )
			{	}

			u32 dxCompilerFlags;
			bool generatePreprocessedOutput;
		};

		struct HlslProgramData
		{
			~HlslProgramData()
			{
				if ( shaderBlob_ )
					shaderBlob_->Release();
				if ( preprocessedShaderBlob_ )
					preprocessedShaderBlob_->Release();
				if ( reflection_ )
					reflection_->Release();

				DX_SAFE_RELEASE( vs_ );
				DX_SAFE_RELEASE( ps_ );
				DX_SAFE_RELEASE( gs_ );
				DX_SAFE_RELEASE( cs_ );
				DX_SAFE_RELEASE( vsInputSignature_ );
			}

			ID3D10Blob* shaderBlob_ = nullptr;
			ID3D10Blob* preprocessedShaderBlob_ = nullptr;
			ID3D11ShaderReflection* reflection_ = nullptr;

			ID3D11VertexShader* vs_ = nullptr;
			ID3D11PixelShader* ps_ = nullptr;
			ID3D11GeometryShader* gs_ = nullptr;
			ID3D11ComputeShader* cs_ = nullptr;
			ID3D10Blob* vsInputSignature_ = nullptr;
			uint32_t vsInputSignatureHash_ = 0;
		};

		class HlslException : public Exception
		{
		public:
			HlslException( std::string&& exceptionMessage, const char* hlslMessage, HRESULT hres, const char* file, int line )
				: Exception( std::move( exceptionMessage ), file, line )
				, hlslMessage_( hlslMessage )
				, hres_( hres )
			{	}

			const std::string& GetHlslErrorMessage() const throw( )
			{
				return hlslMessage_;
			}

		protected:
			std::string hlslMessage_; // The error message
			HRESULT hres_;
		};

#define THROW_HLSL_EXCEPTION(msg, hlslMsg, hres) throw HlslException( msg, hlslMsg, hres, __FILE__, __LINE__ )

	} // namespace FxLib
} // namespace spad
