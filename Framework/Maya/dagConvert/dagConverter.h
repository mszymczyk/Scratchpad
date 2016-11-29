#pragma once

#include <maya/MGlobal.h>
#include <maya/MPlug.h>
#include <maya/MDagMessage.h>
#include <maya/MDGMessage.h>
#include <maya/MModelMessage.h>
#include <maya/MDagPath.h>
#include <maya/MFnDagNode.h>
#include <maya/MCallbackIdArray.h>
#include <maya/MObjectArray.h>
#include <maya/MItDag.h>
#include <maya/MUuid.h>
#include <vector>
#include <algorithm>
#include <map>
#include <set>
#include "Graph.h"
#include "Level.h"
#include "GraphHelper.h"

class dagConverter
{
public:
	void startUp();
	void shutDown();

	NodeProxy* findNode( MFnDependencyNode& depNode, bool createIfNotFound );
	void deleteNode( MFnDependencyNode& depNode );
	void printHierarchy();

	void requestSyncHierarchy() { syncHierarchyRequested_ = true; }
	void syncHierarchy();

private:
	void _InitNodes();
	void _PrintHierarchyRecurse( const NodeProxy* root, std::string pathBase );

	struct _SyncContext
	{
		_SyncContext( const GraphStructure& oldStruct, GraphStructure& newStruct )
			: oldStructure_( &oldStruct )
			, newStructure_( &newStruct )
		{	}

		const GraphStructure* oldStructure_ = nullptr;
		GraphStructure* newStructure_ = nullptr;
		//const PathSet* pathSet_ = nullptr;
		std::string pathBase_;
		std::vector<NodeIndex> pathNodes_;
		u32 pathFlatIndex_ = 0;
		u32 parentPathFlatIndex_ = 0xffffffff;

		std::vector<PathRemap> pathsRemap_;
		std::vector<u32> addedPaths_;
		std::vector<u32> addedPathsFlatIndices_;
		std::vector<u32> removedPaths_;
		//std::vector<PathRemap> pathsRemap_;
	};

	void _SyncHierarchyRecurse( _SyncContext& sc, NodeProxy* parent );


	static void _NodeRemovedCallback_static( MObject& node, void *clientData );
	void _NodeRemovedCallback( MObject& node, void *clientData );

	// Node added to model callback.
	static void _UserNodeAddedCB_static( MObject& node, void *clientData );
	void _UserNodeAddedCB( MObject& node, void *clientData );

	static void _UserDAGGenericCB_static( MDagMessage::DagMessage msg, MDagPath &child, MDagPath &parent, void * );
	void _UserDAGGenericCB( MDagMessage::DagMessage msg, MDagPath &child, MDagPath &parent, void * );

	static void _InstallNodeAddedCallback( MDagPath& dagPath );

private:
	MCallbackIdArray callbackIds;

	//NodeProxyMap nodeProxies_;
	//NodeProxy* world_;

	//PathSet existingPaths_;
	//PathIdManager paths_;
	//NodeIdManager nodes_;
	Level level_;

	bool syncHierarchyRequested_ = true;
};

extern dagConverter gDagConverter;

