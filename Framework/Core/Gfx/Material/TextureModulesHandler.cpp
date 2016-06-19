#include "Gfx_pch.h"
#include "TextureModulesHandler.h"

namespace spad
{

void TextureModulesHandler::Init( const tinyxml2::XMLElement* xmlModule, MaterialShader* materialShader ) const
{
	if ( xmlModule->Attribute( "xsi:type", "texture2DType" ) || xmlModule->Attribute( "xsi:type", "texture2DParameterType" ) )
	{
		const char* name = xmlModule->Attribute( "name" );
		FR_ASSERT( name );
		{
			// setup texture
			std::string fullName = name;
			fullName.append( "_tex" );
			u16 index = materialShader->getTextureIndex( fullName.c_str() );
			if ( index != 0xffff )
			{
				const char* filename = xmlModule->Attribute( "filename" );
				if ( filename )
				{
					ID3D11ShaderResourceView* srv = LoadTexturePtr( m_device, filename );
					if ( srv )
					{
						Dx11SetDebugName( srv, filename );
						materialShader->setTexture( fullName.c_str(), srv );
						srv->Release();
					}
				}
			}
		}
		{
			// setup sampler
			std::string fullName = name;
			fullName.append( "_samp" );
			u16 index = materialShader->getSamplerIndex( fullName.c_str() );
			if ( index != 0xffff )
			{
				// fill this
				//
			}
		}
	}
	else
	{
		THROW_MESSAGE( "TextureModulesHandler: unsupported module type! (%s)", materialShader->getFilename() );
	}
}

} // namespace spad
