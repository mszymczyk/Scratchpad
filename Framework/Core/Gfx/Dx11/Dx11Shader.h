#pragma once

#include <Tools/FxCompiler/FxCompilerLib/FxTypes.h>

namespace spad
{
class RenderState;


class HlslShaderPass
{
public:
	~HlslShaderPass();

	ID3D11VertexShader* vs_ = nullptr;
	ID3D11PixelShader* ps_ = nullptr;
	ID3D11GeometryShader* gs_ = nullptr;
	ID3D11ComputeShader* cs_ = nullptr;
	ID3D10Blob* vsInputSignature_ = nullptr;
	u32 vsInputSignatureHash_ = 0;

	void setVS( ID3D11DeviceContext* context ) const { context->VSSetShader( vs_, nullptr, 0 ); }
	void setPS( ID3D11DeviceContext* context ) const { context->PSSetShader( ps_, nullptr, 0 ); }
};


class HlslShaderPassContainer
{
public:
	~HlslShaderPassContainer();

	const HlslShaderPass* getPass( const u32* indices, size_t nIndices ) const;
	const HlslShaderPass* getPass0() const;

private:
	std::string name_;
	std::vector<u32> dimSizes_;
	std::vector<u32> subtreeSize_;
	std::vector<HlslShaderPass> combinations_;
	const RenderState* renderState_ = nullptr;

	friend class HlslShader;
};


typedef std::unique_ptr<HlslShader> HlslShaderPtr;



// visual studio compiler crash when this struct is declared within _ParseHlsl
struct HlslUniqueProgram
{
	ID3D11VertexShaderPtr vs;
	ID3D11PixelShaderPtr ps;
	ID3D11GeometryShaderPtr gs;
	ID3D11ComputeShaderPtr cs;
	ID3DBlobPtr vsInputSignature;

	ID3D11ShaderReflectionPtr vsRefl;
	ID3D11ShaderReflectionPtr psRefl;
	ID3D11ShaderReflectionPtr gsRefl;
	ID3D11ShaderReflectionPtr csRefl;

	//VertexAttributeInputMask vsInput;
	u32 vsInputSignatureHash = 0;

	fxlib::ShaderStage::Type stage = fxlib::ShaderStage::count;
};



class HlslShader
{
public:
	const HlslShaderPass* getPass( const char* name ) const;
	const HlslShaderPass* getPass( const char* name, const u32* indices, size_t nIndices ) const;
	const HlslShaderPass* getPass( const char* name, std::initializer_list<u32> indices ) const
	{
		return getPass( name, indices.begin(), indices.size() );
	}

private:
	int _ParseHlsl( const u8* buf, size_t bufSize, const char* filename );

private:
	std::vector<HlslShaderPassContainer> passes_;

	friend HlslShaderPtr LoadCompiledFxFile( const char* filename );
};

HlslShaderPtr LoadCompiledFxFile( const char* filename );

} // namespace spad
