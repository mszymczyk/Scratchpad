#include "Gfx_pch.h"
#include "Dx11Shader.h"
#include "Dx11.h"
//#include <picoCore/renderer/rhi.h>
#include <Tools/FxCompiler/FxCompilerLib/FxState.h>
#include <util/Hash/HashUtil.h>

#if defined(_MSC_VER) && defined(_DEBUG)
#define new _DEBUG_NEW
#endif


namespace spad
{

HlslShaderPass::~HlslShaderPass()
{
	DX_SAFE_RELEASE( vs_ );
	DX_SAFE_RELEASE( ps_ );
	DX_SAFE_RELEASE( gs_ );
	DX_SAFE_RELEASE( cs_ );
	DX_SAFE_RELEASE( vsInputSignature_ );
}

HlslShaderPassContainer::~HlslShaderPassContainer()
{
}

const HlslShaderPass* HlslShaderPassContainer::getPass( const u32* indices, size_t nIndices ) const
{
	SPAD_ASSERT( nIndices == dimSizes_.size() );
	u32 offset = 0;

	for ( size_t i = 0; i < nIndices; ++i )
	{
		SPAD_ASSERT( indices[i] < dimSizes_[i] );
		offset += indices[i] * subtreeSize_[i];
	}

	return &combinations_[offset];
}


const HlslShaderPass* HlslShaderPassContainer::getPass0() const
{
	SPAD_ASSERT( dimSizes_.size() == 1 );
	SPAD_ASSERT( dimSizes_[0] == 1 );

	return &combinations_[0];
}


const HlslShaderPass* HlslShader::getPass( const char* name ) const
{
	for ( const auto& p : passes_ )
		if ( p.name_ == name )
			return p.getPass0();

	logError( "Pass '%s' not found!", name );
	SPAD_ASSERT2( false, "Pass not found" );

	return nullptr;
}


const HlslShaderPass* HlslShader::getPass( const char* name, const u32* indices, size_t nIndices ) const
{
	for ( const auto& p : passes_ )
		if ( p.name_ == name )
			return p.getPass( indices, nIndices );

	logError( "Pass '%s' not found!", name );
	SPAD_ASSERT2( false, "Pass not found" );

	return nullptr;
}


class FileReader
{
public:
	FileReader( const u8* fileBuffer, const size_t fileBufferSize )
		: buf_( fileBuffer )
		, bufSize_( fileBufferSize )
		, readOffset_( 0 )
	{
		fileBuffer = nullptr;
	}

	~FileReader()
	{	}

	FileReader& operator=( const FileReader& other ) = delete;

	void readString( std::string& dst )
	{
		size_t distToEnd = bufSize_ - readOffset_;
		SPAD_ASSERT( distToEnd >= sizeof( u32 ) );
		u32 strLen;
		memcpy( &strLen, buf_ + readOffset_, sizeof( u32 ) );
		SPAD_ASSERT( strLen >= 1 ); // '\0' always stored in file
		readOffset_ += sizeof( u32 );
		distToEnd = bufSize_ - readOffset_;
		SPAD_ASSERT( distToEnd >= strLen );
		dst.assign( reinterpret_cast<const char*>( buf_ ) + readOffset_, strLen - 1 );
		readOffset_ += strLen;
	}

	void skipString()
	{
		size_t distToEnd = bufSize_ - readOffset_;
		SPAD_ASSERT( distToEnd >= sizeof( u32 ) );
		u32 strLen;
		memcpy( &strLen, buf_ + readOffset_, sizeof( u32 ) );
		SPAD_ASSERT( strLen >= 1 ); // '\0' always stored in file
		readOffset_ += sizeof( u32 );
		distToEnd = bufSize_ - readOffset_;
		SPAD_ASSERT( distToEnd >= strLen );
		readOffset_ += strLen;
	}

	u32 readU32()
	{
		size_t distToEnd = bufSize_ - readOffset_;
		SPAD_ASSERT( distToEnd >= sizeof( u32 ) );
		u32 ret;
		memcpy( &ret, buf_ + readOffset_, sizeof( u32 ) );
		readOffset_ += sizeof( u32 );
		return ret;
	}

	void readU32Array( u32* dst, u32 nWords )
	{
		if ( nWords )
		{
			size_t distToEnd = bufSize_ - readOffset_;
			size_t totalSize = sizeof( u32 ) * nWords;
			SPAD_ASSERT( distToEnd >= totalSize );
			memcpy( dst, buf_ + readOffset_, totalSize );
			readOffset_ += totalSize;
		}
	}

	void readChunk( void* dst, u32 nBytes )
	{
		size_t distToEnd = bufSize_ - readOffset_;
		SPAD_ASSERT( distToEnd >= nBytes );
		memcpy( dst, buf_ + readOffset_, nBytes );
		readOffset_ += nBytes;
	}

private:
	const u8* buf_ = nullptr;
	const size_t bufSize_ = 0;
	size_t readOffset_ = 0; // current read offset
};

//int _ExtractVertexShaderInputMask( ID3D11ShaderReflection* vsReflection, VertexAttributeInputMask& vsInput )
//{
//	// extract and translate vertex attribute input mask
//	//
//	D3D11_SHADER_DESC vsDesc;
//	HRESULT hr = vsReflection->GetDesc( &vsDesc );
//	SPAD_ASSERT( SUCCEEDED( hr ) );
//
//	//bool instanceParams[6] = { false };
//
//	for ( UINT i = 0; i < vsDesc.InputParameters; ++i )
//	{
//		D3D11_SIGNATURE_PARAMETER_DESC e;
//		vsReflection->GetInputParameterDesc( i, &e );
//
//		e::VertexAttributeId::Type id = e::VertexAttributeId::Invalid;
//		//u32 sem = PICO_VERTEX_ATTRIBUTE_SLOT_COUNT;
//
//		if ( 0 == _stricmp( "POSITION", e.SemanticName ) )
//		{
//			//sem = PICO_VERTEX_ATTRIBUTE_SLOT_POSITION;
//			id = e::VertexAttributeId::Position;
//		}
//		else if ( 0 == _stricmp( "NORMAL", e.SemanticName ) )
//		{
//			//sem = PICO_VERTEX_ATTRIBUTE_SLOT_NORMAL;
//			id = e::VertexAttributeId::Normal;
//		}
//		else if ( 0 == _stricmp( "TEXCOORD", e.SemanticName ) )
//		{
//			SPAD_ASSERT( e.SemanticIndex < 2 );
//			//sem = PICO_VERTEX_ATTRIBUTE_SLOT_TEXCOORD0 + e.SemanticIndex;
//			//id = static_cast<picoVertexAttributeId>( PICO_VERTEX_ATTRIBUTE_ID_TEXCOORD0 + e.SemanticIndex );
//			id = static_cast<e::VertexAttributeId::Type>( e::VertexAttributeId::TexCoord0 + e.SemanticIndex );
//		}
//		else if ( 0 == _stricmp( "TANGENT", e.SemanticName ) )
//		{
//			//sem = PICO_VERTEX_ATTRIBUTE_SLOT_TANGENT;
//			id = e::VertexAttributeId::Tangent;
//		}
//		else if ( 0 == _stricmp( "COLOR", e.SemanticName ) )
//		{
//			//sem = PICO_VERTEX_ATTRIBUTE_SLOT_COLOR0;
//			//id = PICO_VERTEX_ATTRIBUTE_ID_COLOR;
//			id = e::VertexAttributeId::Color;
//		}
//		else if ( 0 == _stricmp( "PARTICLEDATA", e.SemanticName ) )
//		{
//			//sem = PICO_VERTEX_ATTRIBUTE_SLOT_COLOR0;
//			//id = PICO_VERTEX_ATTRIBUTE_ID_PARTICLEDATA;
//			id = e::VertexAttributeId::ParticleData;
//		}
//		//else if ( 0 == _stricmp( "BLENDWEIGHT", e.SemanticName ) )
//		//{
//		//	//sem = PICO_VERTEX_ATTRIBUTE_SLOT_BLENDWEIGHT;
//		//	//id = PICO_VERTEX_ATTRIBUTE_ID_BLENDWEIGHT;
//		//	id = VertexAttributeId::BlendWeight;
//		//}
//		//else if ( 0 == _stricmp( "BLENDINDICES", e.SemanticName ) )
//		//{
//		//	//sem = PICO_VERTEX_ATTRIBUTE_SLOT_BLENDINDICES;
//		//	id = PICO_VERTEX_ATTRIBUTE_ID_BLENDINDICES;
//		//}
//
//		//if ( sem != PICO_VERTEX_ATTRIBUTE_SLOT_COUNT )
//		//{
//		//	vsMask.attributes[vsMask.nAttributes] = (u8)id;
//		//	vsMask.attributesSlots[vsMask.nAttributes] = (u8)sem;
//		//	++vsMask.nAttributes;
//		//}
//		if ( id != e::VertexAttributeId::Invalid )
//		{
//			vsInput.attributes[vsInput.nAttributes] = id;
//		}
//		else
//		{
//			if ( 0 == _stricmp( e.SemanticName, "SV_InstanceID" ) )
//			{
//				// ok
//				//
//			}
//			else if ( 0 == _stricmp( e.SemanticName, "SV_VertexID" ) )
//			{
//				// nothing
//				//
//			}
//			//else if ( 0 == _stricmp( e.SemanticName, "WORLD_ROW" ) )
//			//{
//			//	SPAD_ASSERT( e.SemanticIndex < 3 );
//			//	instanceParams[e.SemanticIndex] = true;
//			//}
//			//else if ( 0 == _stricmp( e.SemanticName, "WORLDIT_ROW" ) )
//			//{
//			//	SPAD_ASSERT( e.SemanticIndex < 3 );
//			//	instanceParams[e.SemanticIndex + 3] = true;
//			//}
//			//else if ( 0 == _stricmp(e.SemanticName, "PICO_UBO_VBO_INSTANCE_ID") )
//			//{
//			//	pas.hasUBOVBOInstanceParams_ = 1;
//			//}
//			else if ( 0 == _stricmp( e.SemanticName, "PICO_DX11_VS_INSTANCING_DATA_INSTANCEID" ) )
//			{
//				//pas.hasSSBInstanceParams_ = 1;
//				vsInput.hasInstancingDataInstanceId = 1;
//			}
//			else
//			{
//				// nothing
//				//
//			}
//		}
//
//		++vsInput.nAttributes;
//	}
//
//	return 0;
//}


int HlslShader::_ParseHlsl( const u8* buf, size_t bufSize, const char* filename )
{
	FileReader fr( buf, bufSize );

	std::string magic;
	fr.readString( magic ); // "debu", "rele", "diag" or "ship"

	// read all unique programs
	const u32 nUniquePrograms = fr.readU32();

	std::vector<HlslUniqueProgram> uniquePrograms( nUniquePrograms );

	for ( u32 iprog = 0; iprog < nUniquePrograms; ++iprog )
	{
		HlslUniqueProgram& up = uniquePrograms[iprog];
		u32 stage = fr.readU32();
		up.stage = static_cast<fxlib::ShaderStage::Type>( stage );

		// debug name
		//fr.skipString();
		std::string upName;
		fr.readString( upName );

		ID3DBlobPtr shaderBlob;

		const u32 shaderBlobSize = fr.readU32();
		HRESULT hr = D3DCreateBlob( shaderBlobSize, &shaderBlob );
		if ( FAILED( hr ) )
		{
			SPAD_ASSERT( SUCCEEDED( hr ) );
			return -1;
		}

		fr.readChunk( shaderBlob->GetBufferPointer(), shaderBlobSize );

		ID3D11Device* device = gDx11Device;

		if ( up.stage == fxlib::ShaderStage::vertex )
		{
			hr = device->CreateVertexShader( shaderBlob->GetBufferPointer()
				, shaderBlob->GetBufferSize()
				, nullptr
				, &up.vs );

			if ( FAILED( hr ) )
			{
				SPAD_ASSERT( SUCCEEDED( hr ) );
				return -1;
			}

			debug::Dx11SetDebugName3( up.vs, "%s.%s", filename, upName.c_str() );

			const u32 vsInputSignatureSize = fr.readU32();
			hr = D3DCreateBlob( vsInputSignatureSize, &up.vsInputSignature );
			if ( FAILED( hr ) )
			{
				SPAD_ASSERT( SUCCEEDED( hr ) );
				return -1;
			}

			fr.readChunk( up.vsInputSignature->GetBufferPointer(), vsInputSignatureSize );

			hr = D3DGetInputSignatureBlob( shaderBlob->GetBufferPointer()
				, shaderBlob->GetBufferSize()
				, &up.vsInputSignature );

			if ( FAILED( hr ) )
			{
				SPAD_ASSERT( SUCCEEDED( hr ) );
				return -1;
			}

			//hr = D3DReflect( shaderBlob->GetBufferPointer(), shaderBlob->GetBufferSize(), IID_ID3D11ShaderReflection, (void**) &up.vsRefl );
			//if ( FAILED( hr ) )
			//{
			//	SPAD_ASSERT( SUCCEEDED( hr ) );
			//	return -1;
			//}

			//int ires = _ExtractVertexShaderInputMask( up.vsRefl, up.vsInput );
			//if ( ires )
			//{
			//	SPAD_ASSERT( false );
			//	return ires;
			//}

			up.vsInputSignatureHash = MurmurHash3( up.vsInputSignature->GetBufferPointer(), (int)up.vsInputSignature->GetBufferSize() );
		}
		else if ( up.stage == fxlib::ShaderStage::pixel )
		{
			hr = device->CreatePixelShader( shaderBlob->GetBufferPointer()
				, shaderBlob->GetBufferSize()
				, nullptr
				, &up.ps );

			if ( FAILED( hr ) )
			{
				SPAD_ASSERT( SUCCEEDED( hr ) );
				return -1;
			}

			debug::Dx11SetDebugName3( up.ps, "%s.%s", filename, upName.c_str() );
		}
		else if ( up.stage == fxlib::ShaderStage::geometry )
		{
			hr = device->CreateGeometryShader( shaderBlob->GetBufferPointer()
				, shaderBlob->GetBufferSize()
				, nullptr
				, &up.gs );

			if ( FAILED( hr ) )
			{
				SPAD_ASSERT( SUCCEEDED( hr ) );
				return -1;
			}

			debug::Dx11SetDebugName3( up.gs, "%s.%s", filename, upName.c_str() );
		}
		else if ( up.stage == fxlib::ShaderStage::compute )
		{
			hr = device->CreateComputeShader( shaderBlob->GetBufferPointer()
				, shaderBlob->GetBufferSize()
				, nullptr
				, &up.cs );

			if ( FAILED( hr ) )
			{
				SPAD_ASSERT( SUCCEEDED( hr ) );
				return -1;
			}

			debug::Dx11SetDebugName3( up.cs, "%s.%s", filename, upName.c_str() );
		}
	}

	// read passes
	const u32 nPasses = fr.readU32();
	this->passes_.resize( nPasses );

	for ( u32 ipass = 0; ipass < nPasses; ++ipass )
	{
		HlslShaderPassContainer& passContainer = this->passes_[ipass];
		fr.readString( passContainer.name_ );

		// render state
		u32 rsSize = fr.readU32();
		if ( rsSize != sizeof( spad::fxlib::RenderState ) )
		{
			SPAD_ASSERT( rsSize == sizeof( spad::fxlib::RenderState ) );
			return -1;
		}

		fxlib::RenderState rsd;
		fr.readChunk( &rsd, sizeof( rsd ) );

		// combinations
		u32 flatSize = fr.readU32();
		u32 numDims = fr.readU32();
		passContainer.dimSizes_.resize( numDims );
		passContainer.subtreeSize_.resize( numDims );
		passContainer.combinations_.resize( flatSize );
		fr.readU32Array( &passContainer.dimSizes_[0], numDims );

		// recalculate child sizes
		for ( u32 i = 0; i < numDims; ++i )
		{
			u32 siz = 1;
			for ( u32 k = i + 1; k < numDims; ++k )
				siz *= passContainer.dimSizes_[k];
			passContainer.subtreeSize_[i] = siz;
		}

		for ( u32 icomb = 0; icomb < flatSize; ++icomb )
		{
			HlslShaderPass& pass = passContainer.combinations_[icomb];
			std::string combName;
			fr.readString( combName );

			struct UniqueProgramIndex
			{
				u32 entryId_[static_cast<u32>( fxlib::ShaderStage::count )];
			} upi;

			fr.readChunk( &upi, sizeof( upi ) );

			u32 vsIndex = upi.entryId_[(size_t)fxlib::ShaderStage::vertex];
			u32 psIndex = upi.entryId_[(size_t)fxlib::ShaderStage::pixel];
			u32 gsIndex = upi.entryId_[(size_t)fxlib::ShaderStage::geometry];
			u32 csIndex = upi.entryId_[(size_t)fxlib::ShaderStage::compute];

			if ( vsIndex != 0xffffffff )
			{
				const HlslUniqueProgram& up = uniquePrograms[vsIndex];

				SPAD_ASSERT( up.stage == fxlib::ShaderStage::vertex );
				pass.vs_ = up.vs;
				pass.vs_->AddRef();
				pass.vsInputSignature_ = up.vsInputSignature;
				pass.vsInputSignature_->AddRef();
				pass.vsInputSignatureHash_ = up.vsInputSignatureHash;
				//pass.vsInput_ = up.vsInput;
			}

			if ( psIndex != 0xffffffff )
			{
				SPAD_ASSERT( uniquePrograms[psIndex].stage == fxlib::ShaderStage::pixel );
				pass.ps_ = uniquePrograms[psIndex].ps;
				pass.ps_->AddRef();
			}

			if ( gsIndex != 0xffffffff )
			{
				SPAD_ASSERT( uniquePrograms[gsIndex].stage == fxlib::ShaderStage::geometry );
				pass.gs_ = uniquePrograms[gsIndex].gs;
				pass.gs_->AddRef();
			}

			if ( csIndex != 0xffffffff )
			{
				SPAD_ASSERT( uniquePrograms[csIndex].stage == fxlib::ShaderStage::compute );
				pass.cs_ = uniquePrograms[csIndex].cs;
				pass.cs_->AddRef();
			}
		}
	}

	return 0;
}

HlslShaderPtr LoadCompiledFxFile( const char* filename )
{
	std::vector<u8> file = ReadFileAsVector( filename );

	HlslShaderPtr sh = std::make_unique<HlslShader>();
	if (0 == sh->_ParseHlsl( &file[0], file.size(), filename ))
		return sh;

	return nullptr;
}

} // namespace spad
