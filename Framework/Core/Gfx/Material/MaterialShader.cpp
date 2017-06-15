#include "Gfx_pch.h"
#include "MaterialShader.h"
#include <tinyxml2/tinyxml2.h>
#include <Util/Vectormath.h>
#include "MaterialManager.h"

#if defined(_MSC_VER) && defined(_DEBUG)
#define new _DEBUG_NEW
#endif

namespace spad
{

//void MaterialShaderPass::fill( ContextShaderBindings& c ) const
//{
//	c.vs_ = vs_;
//	c.gs_ = gs_;
//	c.ps_ = ps_;
//
//	ID3D11ShaderResourceView** SRVsAllStages[FxLib::eProgramType_computeShader] = {
//		  c.vsSRVs
//		, c.psSRVs
//		, c.gsSRVs
//	};
//	ID3D11SamplerState** samplersAllStages[FxLib::eProgramType_computeShader] = {
//		  c.vsSamplers
//		, c.psSamplers
//		, c.gsSamplers
//	};
//
//	for ( const auto& t : textures_ )
//	{
//		const MaterialShader::Tex& mt = materialShader_->m_textures[t.index_];
//		SRVsAllStages[t.stage_][t.bindingPoint_] = mt.srv;
//	}
//
//	for ( const auto& s : samplers_ )
//	{
//		const MaterialShader::Samp& ms = materialShader_->m_samplers[s.index_];
//		samplersAllStages[s.stage_][s.bindingPoint_] = ms.samp;
//	}
//}

MaterialShader::~MaterialShader()
{
	gMaterialManager->_Release( this );
	delete m_doc;
}

u16 MaterialShader::getUniformIndex( const char* name ) const
{
	size_t n = m_uniforms.size();
	for ( size_t i = 0; i < n; ++i )
	{
		if ( m_uniforms[i].name == name )
		{
			return (u16)i;
		}
	}

	return InvalidIndex;
}

void MaterialShader::setUniform1f( u16 index, float x )
{
	Uniform& u = m_uniforms[index];
	SPAD_ASSERT( u.size == 4 );
	float* f = reinterpret_cast<float*>( uniformsBuf_.data + u.offset );
	if ( *f != x )
	{
		*f = x;
		++ uniformsBufVersion_;
		uniformsBufDirty_ = true;
	}
}

void MaterialShader::setUniform3f( u16 index, const float* xyz )
{
	Uniform& u = m_uniforms[index];
	SPAD_ASSERT( u.size == 12 );
	float* f = reinterpret_cast<float*>( uniformsBuf_.data + u.offset );
	if ( f[0] != xyz[0] || f[1] != xyz[1] || f[2] != xyz[2] )
	{
		f[0] = xyz[0];
		f[1] = xyz[1];
		f[2] = xyz[2];
		++ uniformsBufVersion_;
		uniformsBufDirty_ = true;
	}
}

u16 MaterialShader::getTextureIndex( const char* name ) const
{
	size_t n = m_textures.size();
	for ( size_t i = 0; i < n; ++i )
	{
		if ( m_textures[i].name == name )
		{
			return (u16)i;
		}
	}

	return InvalidIndex;
}

void MaterialShader::setTexture( const char* name, ID3D11ShaderResourceView* srv )
{
	u16 texId = getTextureIndex( name );
	SPAD_ASSERT( texId != 0xffff );
	if ( texId == 0xffff )
		return;

	Tex& t = m_textures[texId];

	if ( srv )
	{
		D3D11_SHADER_RESOURCE_VIEW_DESC srvDesc;
		srv->GetDesc( &srvDesc );

		SPAD_ASSERT( srvDesc.ViewDimension == t.dimension );

		t.srv = srv;
	}
	else
	{
		t.srv = nullptr;
	}
}

u16 MaterialShader::getSamplerIndex( const char* name ) const
{
	size_t n = m_samplers.size();
	for ( size_t i = 0; i < n; ++i )
	{
		if ( m_samplers[i].name == name )
		{
			return (u16)i;
		}
	}

	return InvalidIndex;
}

void MaterialShader::setSampler( const char* name, ID3D11SamplerState* samp )
{
	u16 sampId = getSamplerIndex( name );
	SPAD_ASSERT( sampId != 0xffff );
	if ( sampId == 0xffff )
		return;

	Samp& s = m_samplers[sampId];

	if ( samp )
	{
		s.samp = samp;
	}
	else
	{
		s.samp = nullptr;
	}
}

const MaterialShaderPass* MaterialShader::getPass( const char* name ) const
{
	for ( const auto& p : m_passes )
	{
		if ( p.name_ == name )
			return &p;
	}

	return nullptr;
}

void MaterialShader::updateMaterialParams( Dx11DeviceContext& deviceContext )
{
	if ( uniformsBufDirty_ )
	{
		uniformsBufDirty_ = false;
		ID3D11DeviceContext* context = deviceContext.context;
		uniformsBuf_.updateGpu( context );
	}
}

void MaterialShader::bindMaterialParamsUniformsVSPS( Dx11DeviceContext& deviceContext )
{
	ID3D11DeviceContext* context = deviceContext.context;

	ID3D11Buffer* bufferArray[1];
	bufferArray[0] = uniformsBuf_.getDxBuffer();
	context->VSSetConstantBuffers( 2, 1, bufferArray );
	context->PSSetConstantBuffers( 2, 1, bufferArray );
}

void MaterialShader::fill( ContextShaderBindings& c, const MaterialShaderPass& pass ) const
{
	//const MaterialShaderPass* pass = getPass( passName );
	//SPAD_ASSERT( pass );

	c.vs_ = pass.vs_;
	c.gs_ = pass.gs_;
	c.ps_ = pass.ps_;

	ID3D11ShaderResourceView** SRVsAllStages[FxLib::eProgramType_computeShader] = {
		c.vsSRVs
		, c.psSRVs
		, c.gsSRVs
	};
	ID3D11SamplerState** samplersAllStages[FxLib::eProgramType_computeShader] = {
		c.vsSamplers
		, c.psSamplers
		, c.gsSamplers
	};

	for ( const auto& t : pass.textures_ )
	{
		SRVsAllStages[t.stage_][t.bindingPoint_] = m_textures[t.index_].srv;
	}

	for ( const auto& s : pass.samplers_ )
	{
		samplersAllStages[s.stage_][s.bindingPoint_] = m_samplers[s.index_].samp;
	}
}

void MaterialShader::_Release( MaterialShaderInstance* materialInstance )
{
	SPAD_ASSERT( materialInstance->GetRefCount() == 1 );
	MaterialShaderInstanceSet::iterator it = m_materialInstances.find( materialInstance );
	SPAD_ASSERT( it != m_materialInstances.end() );
	m_materialInstances.erase( it );
}

MaterialShaderInstance::~MaterialShaderInstance()
{
	m_materialShader->_Release( this );
}

void MaterialShaderInstance::setUniform1f( u16 index, float x )
{
	const MaterialShader::Uniform& u = m_materialShader->m_uniforms[index];
	SPAD_ASSERT( u.size == 4 );
	m_uniforms[index] = true;
	uniformsBufDirty_ = true;
	*reinterpret_cast<float*>(uniformsBuf_.data + u.offset) = x;
}

void MaterialShaderInstance::setUniform3f( u16 index, const float* xyz )
{
	const MaterialShader::Uniform& u = m_materialShader->m_uniforms[index];
	SPAD_ASSERT( u.size == 12 );
	m_uniforms[index] = true;
	uniformsBufDirty_ = true;
	float* f = reinterpret_cast<float*>( uniformsBuf_.data + u.offset );
	f[0] = xyz[0];
	f[1] = xyz[1];
	f[2] = xyz[2];
}

void MaterialShaderInstance::setTexture( const char* name, ID3D11ShaderResourceView* srv )
{
	u16 texId = getTextureIndex( name );
	SPAD_ASSERT( texId != 0xffff );
	if ( texId == 0xffff )
		return;

	if ( srv )
	{
		D3D11_SHADER_RESOURCE_VIEW_DESC srvDesc;
		srv->GetDesc( &srvDesc );

		SPAD_ASSERT( srvDesc.ViewDimension == m_materialShader->m_textures[texId].dimension );

		m_textures[texId] = srv;
	}
	else
	{
		m_textures[texId] = nullptr;
	}
}

void MaterialShaderInstance::updateMaterialInstanceParams( Dx11DeviceContext& deviceContext )
{
	if ( materialUniformsBufVersion_ != m_materialShader->uniformsBufVersion_ )
	{
		materialUniformsBufVersion_ = m_materialShader->uniformsBufVersion_;

		const ConstantBuffer2& materialUniformsBuf = m_materialShader->uniformsBuf_;
		const std::vector<MaterialShader::Uniform>& materialUniforms = m_materialShader->m_uniforms;
		const size_t n = materialUniforms.size();
		for ( size_t i = 0; i < n; ++i )
		{
			if ( !m_uniforms[i] )
			{
				// take value from material
				const MaterialShader::Uniform& mu = materialUniforms[i];
				uniformsBufDirty_ = true;
				memcpy( uniformsBuf_.data + mu.offset, materialUniformsBuf.data + mu.offset, mu.size );
			}
		}
	}

	if ( uniformsBufDirty_ )
	{
		uniformsBufDirty_ = false;
		ID3D11DeviceContext* context = deviceContext.context;
		uniformsBuf_.updateGpu( context );
	}
}

void MaterialShaderInstance::bindMaterialInstanceUniformsVSPS( Dx11DeviceContext& deviceContext )
{
	ID3D11DeviceContext* context = deviceContext.context;

	ID3D11Buffer* bufferArray[1];
	bufferArray[0] = uniformsBuf_.getDxBuffer();
	context->VSSetConstantBuffers( 2, 1, bufferArray );
	context->PSSetConstantBuffers( 2, 1, bufferArray );
}

void MaterialShaderInstance::fill( ContextShaderBindings& c, const MaterialShaderPass& pass ) const
{
	//const MaterialShaderPass* pass = m_materialShader->getPass( passName );
	//SPAD_ASSERT( pass );

	c.vs_ = pass.vs_;
	c.gs_ = pass.gs_;
	c.ps_ = pass.ps_;

	ID3D11ShaderResourceView** SRVsAllStages[FxLib::eProgramType_computeShader] = {
		c.vsSRVs
		, c.psSRVs
		, c.gsSRVs
	};
	ID3D11SamplerState** samplersAllStages[FxLib::eProgramType_computeShader] = {
		c.vsSamplers
		, c.psSamplers
		, c.gsSamplers
	};

	const std::vector<MaterialShader::Tex>& materialTextures = m_materialShader->m_textures;
	const std::vector<ID3D11ShaderResourceViewPtr>& materialInstanceTextures = m_textures;

	for ( const auto& t : pass.textures_ )
	{
		if ( materialInstanceTextures[t.index_] )
			SRVsAllStages[t.stage_][t.bindingPoint_] = materialInstanceTextures[t.index_];
		else
			SRVsAllStages[t.stage_][t.bindingPoint_] = materialTextures[t.index_].srv;
	}

	const std::vector<MaterialShader::Samp>& materialSamplers = m_materialShader->m_samplers;

	for ( const auto& s : pass.samplers_ )
	{
		samplersAllStages[s.stage_][s.bindingPoint_] = materialSamplers[s.index_].samp;
	}
}

void MaterialShaderInstance::_MaterialReloaded( ID3D11Device* device )
{
	if ( uniformsBuf_.size != m_materialShader->uniformsBuf_.size )
		uniformsBuf_.ReInitilize( device, m_materialShader->uniformsBuf_ );
	else
	{
		uniformsBufDirty_ = true;
		materialUniformsBufVersion_ = m_materialShader->uniformsBufVersion_;
		memcpy( uniformsBuf_.data, m_materialShader->uniformsBuf_.data, uniformsBuf_.size );
	}

	// clear and resize, "override" flags might have changed for this instance
	m_uniforms.resize( m_materialShader->m_uniforms.size(), false );
	m_textures.resize( m_materialShader->m_textures.size(), nullptr );

	const tinyxml2::XMLDocument* doc = m_materialShader->m_doc;
	_RefreshParams( doc, device );
}

void MaterialShaderInstance::_RefreshParams( const tinyxml2::XMLDocument* doc, ID3D11Device* device )
{
	for ( auto b : m_uniforms )
		b = false;

	uniformsBufDirty_ = true;
	materialUniformsBufVersion_ = m_materialShader->uniformsBufVersion_;
	memcpy( uniformsBuf_.data, m_materialShader->uniformsBuf_.data, uniformsBuf_.size );

	// if we had texture manager, old pointers should be kept for a while to prevent texture from unloading
	// and released only after _RefreshParams has finished
	std::vector<ID3D11ShaderResourceViewPtr> texturesCopy = m_textures; // keeping texture handles will prevent unloading

	for ( auto t : m_textures )
		t = nullptr;

	const tinyxml2::XMLElement* rootElement = doc->RootElement();

	const tinyxml2::XMLElement* instance = NULL;
	for ( instance = rootElement->FirstChildElement( "instance" ); instance; instance = instance->NextSiblingElement( "instance" ) )
	{
		const char* instanceName = instance->Attribute( "name" );
		if ( m_instanceName == instanceName )
		{
			const tinyxml2::XMLElement* parameter = NULL;
			for ( parameter = instance->FirstChildElement( "parameter" ); parameter; parameter = parameter->NextSiblingElement( "parameter" ) )
			{
				const char* overrideAttr = parameter->Attribute( "override" );
				if ( !overrideAttr || strcmp(overrideAttr, "true") )
					continue;

				const char* module = parameter->Attribute( "module" );
				u16 index = getUniformIndex( module );
				if ( index != MaterialShader::InvalidIndex )
				{
					//m_uniforms_[index] = true;

					const char* valueType = parameter->Attribute( "valueType" );
					if ( !strcmp( valueType, "colorValue" ) )
					{
						int color = -1;
						parameter->QueryIntAttribute( "colorValue", &color );

						Vector4 rgba = argbToRgba( (u32)color );
						setUniform3f( index, reinterpret_cast<const float*>( &rgba ) );
					}
					else if ( !strcmp( valueType, "floatValue" ) )
					{
						float value = 0.0f;
						parameter->QueryFloatAttribute( "floatValue", &value );

						setUniform1f( index, value );
					}
					else
					{
						SPAD_NOT_IMPLEMENTED;
					}
				}
				else
				{
					const char* valueType = parameter->Attribute( "valueType" );

					if ( !strcmp( valueType, "uriValue" ) )
					{
						std::string fullName = module;
						fullName.append( "_tex" );
						u16 textureIndex = getTextureIndex( fullName.c_str() );
						if ( textureIndex != MaterialShader::InvalidIndex )
						{
							const char* filename = parameter->Attribute( "uriValue" );

							if ( filename && filename[0] )
							{
								ID3D11ShaderResourceView* srv = LoadTexturePtr( device, filename );
								if ( srv )
								{
									Dx11SetDebugName( srv, filename );
									setTexture( fullName.c_str(), srv );
									srv->Release();
								}
							}
						}
					}
				}
			}

			break;
		}
	}
}

} // namespace spad
