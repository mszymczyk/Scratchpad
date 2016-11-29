#include "level.h"

//NodeProxy* Level::_FindProxyNode( const Uuid& uuid )
//{
//	NodeProxyMap::iterator it = nodeProxies_.find( uuid );
//	if ( it != nodeProxies_.end() )
//		return it->second;
//
//	return nullptr;
//}

//NodeProxy* Level::_CreateProxyNode( const Uuid& uuid, const char* name )
//{
//	NodeProxy* n = new NodeProxy( name );
//	nodes_[uuid] = n;
//	return n;
//}

//void Level::_DeleteProxyNode( const Uuid& uuid )
//{
//	NodeProxyMap::iterator it = nodeProxies_.find( uuid );
//	if ( it != nodeProxies_.end() )
//	{
//		delete it->second;
//		nodeProxies_.erase( it );
//	}
//}
