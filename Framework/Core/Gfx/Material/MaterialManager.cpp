#include "Gfx_pch.h"
#include "MaterialManager.h"

#if defined(_MSC_VER) && defined(_DEBUG)
#define new _DEBUG_NEW
#endif

using namespace tinyxml2;

namespace spad
{

MaterialManager* gMaterialManager;

int MaterialManager::Initialize( ID3D11Device* device )
{
	FR_ASSERT( !gMaterialManager );
	gMaterialManager = new MaterialManager( device );
	try
	{
		gMaterialManager->_Initialize();
		return 0;
	}
	catch (Exception ex)
	{
		logError( "MaterialManager::Initialize failed. Err=%s", ex.GetMessage().c_str() );
		return -1;
	}
}

void MaterialManager::DeInitialize()
{
	if ( gMaterialManager )
	{
		gMaterialManager->_DeInitialize();
		delete gMaterialManager;
		gMaterialManager = nullptr;
	}
}

MaterialShaderPtr MaterialManager::LoadMaterialShader( const char* filePath )
{
	MaterialShader* ms = materialFactory_.CreateMaterialShader( filePath );
	if ( ms )
	{
		m_materials.insert( ms );
		ms->AddRef();
		return MaterialShaderPtr( ms );
	}

	return nullptr;
}

spad::MaterialShaderInstancePtr MaterialManager::LoadMaterialShaderInstance( const char* filePathOrig, const char* instanceName )
{
	std::string filePath = CanonicalizePathSimple( filePathOrig );
	MaterialShaderFilename msFake( std::move( filePath ) );
	MaterialShaderSet::iterator it = m_materials.find( &msFake );

	MaterialShader* ms;
	if ( it != m_materials.end() )
	{
		ms = static_cast<MaterialShader*>( *it );
	}
	else
	{
		ms = materialFactory_.CreateMaterialShader( msFake.m_filename.c_str() );
		if ( ms )
		{
			m_materials.insert( ms );
		}
		else
		{
			return nullptr;
		}
	}

	for ( auto& msin : ms->m_materialInstances )
	{
		if ( msin->m_instanceName == instanceName )
		{
			MaterialShaderInstance* msi = static_cast<MaterialShaderInstance*>( msin );
			msi->AddRef();
			return msi;
		}
	}

	MaterialShaderInstance* msi = new MaterialShaderInstance( instanceName );
	msi->AddRef();
	ms->m_materialInstances.insert( msi );
	msi->m_materialShader = ms;
	msi->_MaterialReloaded( m_device );
	return msi;
}

void MaterialManager::_Release( MaterialShader* materialShader )
{
	FR_ASSERT( materialShader->GetRefCount() == 1 );
	FR_ASSERT( materialShader->m_materialInstances.empty() );
	if ( !materialShader->m_filename.empty() )
	{
		// we sometimes delete fake materials - required for reloading
		MaterialShaderSet::iterator it = m_materials.find( materialShader );
		FR_ASSERT( it != m_materials.end() );
		m_materials.erase( it );
	}
}

void MaterialManager::_Initialize()
{
	registerMessageHandler( MessageHandler, nullptr );
}

void MaterialManager::_DeInitialize()
{
	unregisterMessageHandler( MessageHandler );
}

template <class T>
void my_swap( T& a, T& b )
{
	T c( std::move( a ) );
	a = std::move( b );
	b = std::move( c );
}

void MaterialManager::_Reload( const std::string& filePathOrig )
{
	std::string filePath = CanonicalizePathSimple( filePathOrig );
	MaterialShaderFilename msFake( std::move( filePath ) );
	MaterialShaderSet::iterator it = m_materials.find( &msFake );
	FR_ASSERT( it != m_materials.end() );

	MaterialShader* msNew = materialFactory_.CreateMaterialShader( msFake.m_filename.c_str() );
	if ( !msNew )
	{
		FR_NOT_IMPLEMENTED;
	}

	MaterialShader* msOld = static_cast<MaterialShader*>( *it );

	my_swap( msOld->m_uniforms, msNew->m_uniforms );
	my_swap( msOld->uniformsBuf_, msNew->uniformsBuf_ );
	msOld->uniformsBufDirty_ = true;
	my_swap( msOld->m_textures, msNew->m_textures );
	my_swap( msOld->m_samplers, msNew->m_samplers );
	my_swap( msOld->m_passes, msNew->m_passes );

	msNew->AddRef(); // artificially increase ref count so destructor is happy
	msNew->m_filename.clear(); // so it won't be deleted from manager's pool
	delete msNew;

	for ( auto& msin : msOld->m_materialInstances )
	{
		MaterialShaderInstance* msi = static_cast<MaterialShaderInstance*>( msin );
		msi->_MaterialReloaded( m_device );
	}
}

void MaterialManager::_RefreshNodes( const std::string& filePathOrig, const ZMQHubUtil::IncommingMessage& msg )
{
	std::string filePath = CanonicalizePathSimple( filePathOrig );
	MaterialShaderFilename msFake( std::move( filePath ) );
	MaterialShaderSet::iterator it = m_materials.find( &msFake );
	FR_ASSERT( it != m_materials.end() );
	MaterialShader* ms = static_cast<MaterialShader*>( *it );

	bool ok = true;
	int nNodes = msg.readInt( ok );
	std::vector<std::string> nodeTypes;
	nodeTypes.reserve( nNodes );
	std::vector<std::string> nodeNames;
	nodeNames.reserve( nNodes );

	for ( int i = 0; i < nNodes; ++i )
	{
		std::string typeName = msg.readString( ok );
		nodeTypes.push_back( typeName );
		std::string moduleId = msg.readString( ok );
		nodeNames.push_back( moduleId );
	}

	std::string xml = msg.readString( ok );

	tinyxml2::XMLDocument doc;

	XMLError err = doc.Parse( xml.c_str(), xml.size() );

	if ( err != XML_SUCCESS )
	{
		THROW_MESSAGE( "Couldn't load xml from file '%s'!", msFake.m_filename.c_str() );
	}

	size_t siz = nodeTypes.size();
	for ( size_t i = 0; i < siz; ++i )
	{
		materialFactory_._RefreshMaterialShader( ms, doc, nodeTypes[i], nodeNames[i] );
	}
}

void MaterialManager::_RefreshInstances( const std::string& filePathOrig, const ZMQHubUtil::IncommingMessage& msg )
{
	std::string filePath = CanonicalizePathSimple( filePathOrig );
	MaterialShaderFilename msFake( std::move( filePath ) );
	MaterialShaderSet::iterator it = m_materials.find( &msFake );
	FR_ASSERT( it != m_materials.end() );
	MaterialShader* ms = static_cast<MaterialShader*>( *it );

	bool ok = true;
	int nInstances = msg.readInt( ok );
	std::vector<std::string> instanceNames;
	instanceNames.reserve( nInstances );

	for ( int i = 0; i < nInstances; ++i )
	{
		std::string instanceName = msg.readString( ok );
		instanceNames.push_back( instanceName );
	}

	std::string xml = msg.readString( ok );

	tinyxml2::XMLDocument doc;

	XMLError err = doc.Parse( xml.c_str(), xml.size() );

	if ( err != XML_SUCCESS )
	{
		THROW_MESSAGE( "Couldn't load xml from file '%s'!", msFake.m_filename.c_str() );
	}

	for ( auto& instanceName : instanceNames )
	{
		MaterialShaderInstanceName msin( std::move( instanceName ) );
		MaterialShader::MaterialShaderInstanceSet::iterator iit = ms->m_materialInstances.find( &msin );
		FR_ASSERT( iit != ms->m_materialInstances.end() );
		MaterialShaderInstance* msi = static_cast<MaterialShaderInstance*>( *iit );
		msi->_RefreshParams( &doc, m_device );
	}
}

void MaterialManager::MessageHandler( const ZMQHubUtil::IncommingMessage& msg, void* /*userData*/, void* /*userData2*/ )
{
	if ( !msg.doTagsEqual( "material" ) )
		return;

	bool ok = true;
	std::string cmd = msg.readString( ok );

	if ( cmd == "reload" )
	{
		std::string materialFile = msg.readString( ok );
		gMaterialManager->_Reload( materialFile );
	}
	else if ( cmd == "refreshNodes" )
	{
		std::string materialFile = msg.readString( ok );
		gMaterialManager->_RefreshNodes( materialFile, msg );
	}
	else if ( cmd == "refreshInstances" )
	{
		std::string materialFile = msg.readString( ok );
		gMaterialManager->_RefreshInstances( materialFile, msg );
	}
}


} // namespace spad
