#pragma once

#include "IModuleHandler.h"

namespace spad
{
	class TextureModulesHandler : public IModuleHandler
	{
	public:
		TextureModulesHandler( ID3D11Device* device )
			: m_device( device )
		{	}

		void Init( const tinyxml2::XMLElement* xmlModule, MaterialShader* materialShader ) const;

	private:
		ID3D11Device* m_device;
	}; // class TextureModulesHandler

} // namespace spad
