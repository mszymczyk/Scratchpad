#pragma once

#include "MaterialShader.h"
#include "IDynamicModule.h"
#include <tinyxml2/tinyxml2.h>

namespace spad
{
	class IModuleHandler
	{
	public:
		virtual void CreateDynamicModules( const tinyxml2::XMLElement* xmlModule, MaterialShader* materialShader, std::vector<IDynamicModule*>& dynamicModules ) const
		{
			(void)xmlModule;
			(void)materialShader;
			(void)dynamicModules;
		}
		virtual void Init( const tinyxml2::XMLElement* xmlModule, MaterialShader* materialShader ) const = 0;

	}; // class IModuleHandler

} // namespace spad
