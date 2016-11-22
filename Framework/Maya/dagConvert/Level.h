#pragma once

#include "Graph.h"

class Level
{
public:
	//PathId findPath( const char* containerAndNodeNames );
	//PathId findPath( const char* containerName, const char* nodeName );
	PathIndex getFlatIndex( PathId pathId ) const
	{
		return pathIdManager_.get( pathId ).indexIntoFlatDenseArray;
	}

	//const PathId* getChildrenPaths( PathId parent, u32& nChildren );
	DagNode* getNode( PathId pathId )
	{
		PathIndex flatIndex = getFlatIndex( pathId );
		return graphStructure_.nodes_[flatIndex];
	}
	DagNode* getNodeFlat( u32 flatIndex )
	{
		return graphStructure_.nodes_[flatIndex];
	}

	//void setWorldMatrix( PathId path, const Matrix4& worldPose );
	//void setWorldMatrixFlat( u32 flatIndex, const Matrix4& worldPose );

	// internal use only
	//NodeProxy* _FindProxyNode( const Uuid& uuid );
	//NodeProxy* _CreateProxyNode( const Uuid& uuid, const char* name );
	//void _DeleteProxyNode( const Uuid& uuid );

private:
	PathIdManager pathIdManager_;
	NodeIdManager nodeIdManager_; // remaps from NodeId to index into allDependencyNodes_
	//DependencyNodeArray allDependencyNodes_;
	GraphStructure graphStructure_;
	//ParentsArray parents_;
	//ChildCountsArray childCount_;
	//DependencyNodeArray nodes_;


	// development only data
	NodeProxyMap nodeProxies_;
	NodeProxy* world_;
	PathSet paths_;

	friend class dagConverter;
};

