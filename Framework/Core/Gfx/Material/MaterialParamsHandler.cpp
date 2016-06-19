#include "Gfx_pch.h"
#include "MaterialParamsHandler.h"
#include <Util/Vectormath.h>

namespace spad
{

void MaterialParamsHandler::Init( const tinyxml2::XMLElement* xmlModule, MaterialShader* materialShader ) const
{
	if ( xmlModule->Attribute( "xsi:type", "colorParameterType" ) )
	{
		const char* name = xmlModule->Attribute( "name" );
		FR_ASSERT( name );

		u16 index = materialShader->getUniformIndex( name );
		if ( index != 0xffff )
		{
			int color = -1;
			xmlModule->QueryIntAttribute( "color", &color );

			Vector4 rgba = argbToRgba( (u32)color );
			materialShader->setUniform3f( index, reinterpret_cast<const float*>( &rgba ) );
		}
	}
	else if ( xmlModule->Attribute( "xsi:type", "floatParameterType" ) )
	{
		const char* name = xmlModule->Attribute( "name" );
		FR_ASSERT( name );

		u16 index = materialShader->getUniformIndex( name );
		if ( index != 0xffff )
		{
			float value = 0.0f;
			xmlModule->QueryFloatAttribute( "value", &value );

			materialShader->setUniform1f( index, value );
		}
	}
	else
	{
		THROW_MESSAGE( "MaterialParamsHandler: unsupported module type! (%s)", materialShader->getFilename() );
	}
}

} // namespace spad
