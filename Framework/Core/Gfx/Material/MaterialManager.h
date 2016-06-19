#pragma once

#include "MaterialShaderFactory.h"
#include <Util/ZMQHubUtil.h>
#include <unordered_set>

namespace spad
{
class MaterialManager
{
private:
	MaterialManager( ID3D11Device* device )
		: materialFactory_( device )
		, m_device( device )
	{	}

	~MaterialManager()
	{
	}

public:
	static int Initialize( ID3D11Device* device );
	static void DeInitialize();

	MaterialShaderPtr LoadMaterialShader( const char* filePath );
	MaterialShaderInstancePtr LoadMaterialShaderInstance( const char* filePath, const char* instanceName );

	void _Release( MaterialShader* materialShader );

private:
	void _Initialize();
	void _DeInitialize();
	void _Reload( const std::string& filePath );
	void _RefreshNodes( const std::string& filePath, const ZMQHubUtil::IncommingMessage& msg );
	void _RefreshInstances( const std::string& filePath, const ZMQHubUtil::IncommingMessage& msg );

	static void MessageHandler( const ZMQHubUtil::IncommingMessage& msg, void* /*userData*/, void* /*userData2*/ );

	MaterialShaderFactory materialFactory_;
	
	struct MaterialShaderHash
	{
		std::size_t operator()( const MaterialShaderFilename* k ) const
		{
			return std::hash<std::string>()( k->m_filename );
		}
	};

	struct MaterialShaderEqual
	{
		bool operator()( const MaterialShaderFilename* lhs, const MaterialShaderFilename* rhs ) const
		{
			return lhs->m_filename == rhs->m_filename;
		}
	};

	typedef std::unordered_set<MaterialShaderFilename*, MaterialShaderHash, MaterialShaderEqual> MaterialShaderSet;
	MaterialShaderSet m_materials;

	ID3D11Device* m_device = nullptr;
}; // class MaterialManager

extern MaterialManager* gMaterialManager;

} // namespace spad
