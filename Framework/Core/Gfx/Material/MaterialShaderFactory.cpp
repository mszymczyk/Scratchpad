#include "Gfx_pch.h"
#include "MaterialShaderFactory.h"
#include "TextureModulesHandler.h"
#include "MaterialParamsHandler.h"
#include <Util/FileIO.h>
//#include <FxLib/FxLib.h>

#if defined(_MSC_VER) && defined(_DEBUG)
#define new _DEBUG_NEW
#endif

using namespace tinyxml2;

namespace spad
{

using namespace fxlib;

MaterialShaderFactory::MaterialShaderFactory( ID3D11Device* device )
	: m_device( device )
{
	m_moduleHandlers["texture2DType"] = std::make_unique<TextureModulesHandler>(m_device);
	m_moduleHandlers["texture2DParameterType"] = std::make_unique<TextureModulesHandler>( m_device );
	m_moduleHandlers["colorParameterType"] = std::make_unique<MaterialParamsHandler>( m_device );
	m_moduleHandlers["floatParameterType"] = std::make_unique<MaterialParamsHandler>( m_device );
}

MaterialShader* MaterialShaderFactory::CreateMaterialShader( const char* filePath )
{
	MaterialShader* materialShader = new MaterialShader( filePath );

	try
	{
		_CreateMaterialShaderImpl( filePath, materialShader );

		return materialShader;
	}
	catch ( Exception ex )
	{
		logError( "CreateMaterialShader failed. Err=%s", ex.GetMessage().c_str() );
		delete materialShader;
		return  nullptr;
	}
}

void MaterialShaderFactory::_CreateMaterialShaderImpl( const char* filePath, MaterialShader* materialShader )
{
	std::string filePathWithoutExt = GetFilePathWithoutExtension( filePath );
	std::string hlslFile = filePathWithoutExt + ".hlsl";

	FxFile fxFile;
	int ires = fxFile.loadCompiledFxFile( hlslFile.c_str() );
	if ( ires )
	{
		THROW_MESSAGE( "Couldn't loadCompiledFxFile '%s'!", hlslFile.c_str() );
	}

	ires = fxFile.reflectHlslData();
	if ( ires )
	{
		THROW_MESSAGE( "Couldn't reflectHlslData '%s'!", hlslFile.c_str() );
	}

	ires = fxFile.createShaders( m_device );
	if ( ires )
	{
		THROW_MESSAGE( "Couldn't createShaders '%s'!", hlslFile.c_str() );
	}

	const FxProgramArray& uniquePrograms = fxFile.getUniquePrograms();

	_ReflectionData reflectionData;
	for ( const auto& prog : uniquePrograms )
	{
		_ReflectShader( *prog, reflectionData );
	}

	materialShader->m_uniforms.resize( reflectionData.materialParams_.size() );
	for ( size_t impu = 0; impu < reflectionData.materialParams_.size(); ++impu )
	{
		const MaterialParamsUniform& mpu = reflectionData.materialParams_[impu];
		MaterialShader::Uniform& u = materialShader->m_uniforms[impu];
		u.name = mpu.desc.Name;
		u.offset = mpu.desc.StartOffset;
		u.size = mpu.desc.Size;
	}

	materialShader->uniformsBuf_.Initialize( m_device, reflectionData.materialParamsBufSize_, nullptr );

	materialShader->m_textures.resize( reflectionData.textures_.size() );
	size_t itex = 0;
	for ( const auto& t : reflectionData.textures_ )
	{
		MaterialShader::Tex& tex = materialShader->m_textures[itex];
		++itex;

		tex.name = t.first;
		//tex.type = t.second.desc.Type;
		tex.dimension = t.second.desc.Dimension;
	}

	materialShader->m_samplers.resize( reflectionData.samplers_.size() );
	size_t isamp = 0;
	for ( const auto& t : reflectionData.samplers_ )
	{
		MaterialShader::Samp& samp = materialShader->m_samplers[isamp];
		++isamp;

		samp.name = t.first;
	}

	const FxPassArray& passes = fxFile.getPasses();

	materialShader->m_passes.resize( passes.size() );

	size_t nPasses = materialShader->m_passes.size();
	for ( size_t ipass = 0; ipass < nPasses; ++ipass )
	{
		const FxPass& pass = *passes[ipass];
		MaterialShaderPass& msp = materialShader->m_passes[ipass];
		//msp.materialShader_ = materialShader;
		msp.name_ = pass.passName;

		for ( u32 istage = 0; istage < eProgramType_count; ++istage )
		{
			if ( pass.entryIdx[istage] != 0xffffffff )
			{
				const FxProgram& prog = *uniquePrograms[istage];

				if ( prog.profile == eProgramType_vertexShader )
				{
					msp.vs_ = prog.hlslProgramData_.vs_;
					msp.vs_->AddRef();
					msp.vsInputSignature_ = prog.hlslProgramData_.vsInputSignature_;
					msp.vsInputSignature_->AddRef();
					msp.vsInputSignatureHash_ = prog.hlslProgramData_.vsInputSignatureHash_;
				}
				else if ( prog.profile == eProgramType_pixelShader )
				{
					msp.ps_ = prog.hlslProgramData_.ps_;
					msp.ps_->AddRef();
				}
				else if ( prog.profile == eProgramType_geometryShader )
				{
					msp.gs_ = prog.hlslProgramData_.gs_;
					msp.gs_->AddRef();
				}
				else
				{
					THROW_MESSAGE( "Unsupported profile! %s - %s:%s", materialShader->getFilename(), pass.passName.c_str(), prog.entryName.c_str() );
				}

				_ExtractPassTextures( materialShader, prog, msp );
				_ExtractPassSamplers( materialShader, prog, msp );
			}
		}
	}

	std::string materialFileContents = ReadTextFileAsString( filePath );

	if ( materialFileContents.empty() )
	{
		THROW_MESSAGE( "Couldn't load file '%s'!", filePath );
	}

	materialShader->m_doc = new tinyxml2::XMLDocument();
	XMLError err = materialShader->m_doc->Parse( materialFileContents.c_str(), materialFileContents.size() );

	if ( err != XML_SUCCESS )
	{
		THROW_MESSAGE( "Couldn't load xml from file '%s'!", filePath );
	}

	_SetupMaterialShader( materialShader, *materialShader->m_doc );
}

void MaterialShaderFactory::_SetupMaterialShader( MaterialShader* materialShader, const tinyxml2::XMLDocument& doc )
{
	const XMLElement* rootElement = doc.RootElement();

	const XMLElement* module = NULL;
	for ( module = rootElement->FirstChildElement( "module" ); module; module = module->NextSiblingElement( "module" ) )
	{
		const char* moduleType = module->Attribute( "xsi:type" );
		SPAD_ASSERT( moduleType );
		ModuleHandlerMap::const_iterator it = m_moduleHandlers.find( moduleType );
		if ( it != m_moduleHandlers.cend() )
		{
			//it->second->CreateDynamicModules
			it->second->Init( module, materialShader );
		}
	}
}

void MaterialShaderFactory::_ExtractPassTextures( const MaterialShader* materialShader, const FxLib::FxProgram& prog, MaterialShaderPass& pass )
{
	ID3D11ShaderReflection* reflection = prog.hlslProgramData_.reflection_;

	D3D11_SHADER_DESC progDesc;
	reflection->GetDesc( &progDesc );

	for ( u32 br = 0; br < progDesc.BoundResources; ++br )
	{
		D3D11_SHADER_INPUT_BIND_DESC bindDesc;
		reflection->GetResourceBindingDesc( br, &bindDesc );

		if ( bindDesc.Type == D3D10_SIT_TEXTURE )
		{
			u16 index = materialShader->getTextureIndex( bindDesc.Name );
			SPAD_ASSERT( index != 0xffff );
			MaterialShaderPass::Tex t;
			t.index_ = index;
			t.stage_ = prog.profile;
			SPAD_ASSERT( bindDesc.BindPoint < 0xffff );
			t.bindingPoint_ = (u16)bindDesc.BindPoint;
			SPAD_ASSERT( bindDesc.BindCount == 1 );
			pass.textures_.push_back( t );
		}
	}
}

void MaterialShaderFactory::_ExtractPassSamplers( const MaterialShader* materialShader, const FxLib::FxProgram& prog, MaterialShaderPass& pass )
{
	ID3D11ShaderReflection* reflection = prog.hlslProgramData_.reflection_;

	D3D11_SHADER_DESC progDesc;
	reflection->GetDesc( &progDesc );

	for ( u32 br = 0; br < progDesc.BoundResources; ++br )
	{
		D3D11_SHADER_INPUT_BIND_DESC bindDesc;
		reflection->GetResourceBindingDesc( br, &bindDesc );

		if ( bindDesc.Type == D3D10_SIT_SAMPLER )
		{
			u16 index = materialShader->getSamplerIndex( bindDesc.Name );
			SPAD_ASSERT( index != 0xffff );
			MaterialShaderPass::Samp s;
			s.index_ = index;
			s.stage_ = prog.profile;
			SPAD_ASSERT( bindDesc.BindPoint < 0xffff );
			s.bindingPoint_ = (u16)bindDesc.BindPoint;
			SPAD_ASSERT( bindDesc.BindCount == 1 );
			pass.samplers_.push_back( s );
		}
	}
}

void MaterialShaderFactory::_RefreshMaterialShader( MaterialShader* materialShader, const tinyxml2::XMLDocument& doc, const std::string& moduleTypeOrig, const std::string& moduleIdOrig )
{
	const XMLElement* rootElement = doc.RootElement();

	const XMLElement* module = NULL;
	for ( module = rootElement->FirstChildElement( "module" ); module; module = module->NextSiblingElement( "module" ) )
	{
		const char* name = module->Attribute( "name" );
		if ( moduleIdOrig == name )
		{
			const char* moduleType = module->Attribute( "xsi:type" );
			SPAD_ASSERT( moduleType );
			SPAD_ASSERT( moduleTypeOrig == moduleType );

			ModuleHandlerMap::const_iterator it = m_moduleHandlers.find( moduleType );
			if ( it != m_moduleHandlers.cend() )
			{
				it->second->Init( module, materialShader );
			}
		}
	}
}

void MaterialShaderFactory::_ReflectShader( const FxLib::FxProgram& prog, _ReflectionData& refData )
{
	ID3D11ShaderReflection* reflection = prog.hlslProgramData_.reflection_;
	D3D11_SHADER_DESC progDesc;
	DXCall( reflection->GetDesc( &progDesc ) );

	for ( UINT iboundResource = 0; iboundResource < progDesc.BoundResources; ++iboundResource )
	{
		D3D11_SHADER_INPUT_BIND_DESC desc;
		DXCall( reflection->GetResourceBindingDesc( iboundResource, &desc ) );

		if ( desc.Type == D3D_SIT_TEXTURE )
		{
			TextureBindingMap::const_iterator it = refData.textures_.find( desc.Name );
			if ( it == refData.textures_.cend() )
			{
				// not found, add it to the list
				TextureBinding bi;
				bi.desc = desc;
				refData.textures_[desc.Name] = bi;
			}
			else
			{
				if ( memcmp( &it->second.desc, &desc, sizeof( desc ) ) )
				{
					THROW_MESSAGE( "Inconsistent desc for resource '%s'", desc.Name );
				}
			}
		}
		else if ( desc.Type == D3D_SIT_SAMPLER )
		{
			SamplerBindingMap::const_iterator it = refData.samplers_.find( desc.Name );
			if ( it == refData.samplers_.cend() )
			{
				// not found, add it to the list
				SamplerBinding bi;
				bi.desc = desc;
				refData.samplers_[desc.Name] = bi;
			}
			else
			{
				if ( memcmp( &it->second.desc, &desc, sizeof( desc ) ) )
				{
					THROW_MESSAGE( "Inconsistent desc for sampler '%s'", desc.Name );
				}
			}
		}
		else if ( desc.Type == D3D_SIT_CBUFFER && desc.BindPoint == 2 )
		{
			SPAD_ASSERT( !strcmp( desc.Name, "MaterialParams" ) );
			if ( refData.materialParams_.empty() )
			{
				ID3D11ShaderReflectionConstantBuffer* cbuf = reflection->GetConstantBufferByName( desc.Name );
				if ( cbuf )
				{
					D3D11_SHADER_BUFFER_DESC cbufDesc;
					cbuf->GetDesc( &cbufDesc );

					refData.materialParamsBufSize_ = cbufDesc.Size;
					refData.materialParams_.resize( cbufDesc.Variables );

					for ( u32 i = 0; i < cbufDesc.Variables; ++i )
					{
						ID3D11ShaderReflectionVariable* var = cbuf->GetVariableByIndex( i );
						MaterialParamsUniform& mpu = refData.materialParams_[i];
						var->GetDesc( &mpu.desc );
					}
				}
			}
		}
	}
}

} // namespace spad
