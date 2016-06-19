#pragma once

#include "../Dx11/Dx11DeviceContext.h"
#include <FxLib/FxLib.h>
#include "MaterialShader.h"
#include "IModuleHandler.h"
#include <tinyxml2/tinyxml2.h>

namespace spad
{
class MaterialShaderFactory
{
public:
	MaterialShaderFactory( ID3D11Device* device );

	MaterialShader* CreateMaterialShader( const char* filePath );

private:
	void _CreateMaterialShaderImpl( const char* filePath, MaterialShader* materialShader );
	void _SetupMaterialShader( MaterialShader* materialShader, const tinyxml2::XMLDocument& doc );
	void _ExtractPassTextures( const MaterialShader* materialShader, const FxLib::FxProgram& prog, MaterialShaderPass& pass );
	void _ExtractPassSamplers( const MaterialShader* materialShader, const FxLib::FxProgram& prog, MaterialShaderPass& pass );

	void _RefreshMaterialShader( MaterialShader* materialShader, const tinyxml2::XMLDocument& doc, const std::string& moduleType, const std::string& moduleId );

	struct MaterialParamsUniform
	{
		D3D11_SHADER_VARIABLE_DESC desc;
	};

	struct TextureBinding
	{
		D3D11_SHADER_INPUT_BIND_DESC desc;
	};

	struct SamplerBinding
	{
		D3D11_SHADER_INPUT_BIND_DESC desc;
	};

	typedef std::vector<MaterialParamsUniform> MaterialParamsUniformArray;
	typedef std::map<std::string, TextureBinding> TextureBindingMap;
	typedef std::map<std::string, SamplerBinding> SamplerBindingMap;

	struct _ReflectionData
	{
		MaterialParamsUniformArray materialParams_;
		u32 materialParamsBufSize_ = 0;
		TextureBindingMap textures_;
		SamplerBindingMap samplers_;
	};

	void _ReflectShader( const FxLib::FxProgram& prog, _ReflectionData& refData );

	ID3D11Device* m_device = nullptr;

	typedef std::map<std::string, std::unique_ptr<IModuleHandler>> ModuleHandlerMap;
	ModuleHandlerMap m_moduleHandlers;

	friend class MaterialManager;
}; // class MaterialShaderFactory

} // namespace spad
