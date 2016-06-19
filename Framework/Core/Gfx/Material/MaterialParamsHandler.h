#pragma once

#include "IModuleHandler.h"

namespace spad
{
	class MaterialParamsHandler : public IModuleHandler
	{
	public:
		MaterialParamsHandler( ID3D11Device* device )
			: m_device( device )
		{	}

		void Init( const tinyxml2::XMLElement* xmlModule, MaterialShader* materialShader ) const;

	private:
		ID3D11Device* m_device;
	}; // class TextureModulesHandler

} // namespace spad
