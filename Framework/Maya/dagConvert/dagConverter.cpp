#include "dagConverter.h"
#include <util/hashtable.h>

dagConverter gDagConverter;



class SampleSubsystemFlat : public GraphChangedCallback
{
public:
	//void onPathsDeleted( const PathsDeletedParam& param ) override
	//{
	//	// clear deleted objects
	//	for ( u32 ipath = 0; ipath < param.nDeletedPathsFlatIndices; ++ipath )
	//	{
	//		Impl* ob = objects_.clear( ipath );
	//		// clear ob
	//	}

	//	objects_.remapDelete( param );
	//}

	//void onPathsCreated( const PathsCreatedParam& param )
	//{
	//	objects_.remapCreate( param );

	//	// create new objects
	//	for ( u32 ipath = 0; ipath < param.nCreatedPathsFlatIndices; ++ipath )
	//	{
	//		// init object
	//		Impl* ob = reinterpret_cast<Impl*>( 0x4 );
	//		// insert it into list
	//		u32 pathFlatIndex = param.createdPathsFlatIndices[ipath];
	//		objects_.set( pathFlatIndex, ob );
	//	}
	//}

	struct Impl
	{
		void* actor_ = nullptr;
	};

	void onGraphChanged( const GraphChangedParam& param ) override
	{
		// clear deleted objects
		for ( u32 ipath = 0; ipath < param.nDeletedPaths; ++ipath )
		{
			u32 pathFlatIndex = param.deletedPathsFlatIndices[ipath];
			Impl* ob = objects_.clearFlat( ipath );
			// clear ob
		}

		objects_.remap( param );

		// create new objects
		for ( u32 ipath = 0; ipath < param.nCreatedPaths; ++ipath )
		{
			// init object
			Impl* ob = reinterpret_cast<Impl*>( 0x4 );
			// insert it into list
			u32 pathFlatIndex = param.createdPathsFlatIndices[ipath];
			objects_.setFlat( pathFlatIndex, ob );
		}
	}

	Impl* getObject( u32 pathId )
	{
		return objects_.getByPathId( pathId );
	}

private:

	GraphHelper<Impl*> objects_;
};

SampleSubsystemFlat gSampleSubsystemFlat;


class SampleSubsystemMap : public GraphChangedCallback
{
public:
	void onGraphChanged( const GraphChangedParam& param ) override
	{
		// clear deleted objects
		for ( u32 ipath = 0; ipath < param.nDeletedPaths; ++ipath )
		{
			u32 pathId = param.deletedPaths[ipath];
			HashTable::Cell* c = objects_.Lookup( pathId );
			// delete object
			objects_.Delete( c );
		}

		// create new objects
		for ( u32 ipath = 0; ipath < param.nCreatedPaths; ++ipath )
		{
			// init object
			Impl* ob = reinterpret_cast<Impl*>( 0x4 );
			// insert it into list
			u32 pathId = param.createdPaths[ipath];
			HashTable::Cell* c = objects_.Insert( pathId );
			c->value = reinterpret_cast<size_t>( ob );
		}
	}

private:
	struct Impl
	{
		void* actor_ = nullptr;
	};

	HashTable objects_;
};

SampleSubsystemMap gSampleSubsystemMap;

// Node added to model callback.
static void userNodeRemovedCB( MObject& node, void *clientData )
{
	if ( !node.isNull() ) {
		bool doDisplay = true;

		MStatus status;
		MFnDagNode dagNode( node, &status );
		if ( status.error() ) {
			doDisplay = false;
			MGlobal::displayInfo( "Error: failed to get dag node." );
		}

		if ( doDisplay ) {
			MString s = dagNode.name();
			MString info( "DAG Model -  Node removed: " );
			info += s;

			MGlobal::displayInfo( info );
		}
	}

	// remove the callback
	MCallbackId id = MMessage::currentCallbackId();
	MMessage::removeCallback( id );

	gDagConverter.requestSyncHierarchy();
}

//// Node added to model callback.
//static void userNodeAddedCB( MObject& node, void *clientData )
//{
//	MStatus status;
//
//	if ( !node.isNull() ) {
//		bool doDisplay = true;
//
//		MDagPath path;
//		status = MDagPath::getAPathTo( node, path );
//		if ( status.error() ) {
//			doDisplay = false;
//			MGlobal::displayInfo( "Error: failed to get dag path to node." );
//		}
//
//		if ( doDisplay ) {
//			MString s = path.fullPathName();
//			MString info( "DAG Model -  Node added: " );
//			info += s;
//
//			path.transform( &status );
//			if ( MS::kInvalidParameter == status ) {
//				info += "(WORLD)";
//			}
//
//			MGlobal::displayInfo( info );
//		}
//
//		u32 pathCount = path.pathCount();
//		if ( pathCount > 0 )
//		{
//			status = path.pop();
//			SPAD_ASSERT( status );
//			MFnDependencyNode parentNode = path.node();
//			Node* p = gDagConverter.findNode( parentNode, false );
//			if ( !p )
//			{
//				MGlobal::displayError( "parent node not found" );
//				return;
//			}
//
//			MFnDependencyNode childNode( node );
//			Node* c = gDagConverter.findNode( childNode, true );
//			if ( c )
//			{
//				p->children_.push_back( c );
//			}
//			else
//			{
//				MGlobal::displayError( "child node not found" );
//			}
//		}
//	}
//
//	// remove the callback
//	MCallbackId id = MMessage::currentCallbackId();
//	MMessage::removeCallback( id );
//
//	// listen for removal message
//	/* MCallbackId id = */ MModelMessage::addNodeRemovedFromModelCallback( node, userNodeRemovedCB, 0, &status );
//	if ( status.error() ) {
//		MGlobal::displayError( "Failed to install node removed from model callback.\n" );
//		return;
//	}
//
//	gDagConverter.requestSyncHierarchy();
//}

//// Install a node added callback for the node specified
//// by dagPath.
//static void installNodeAddedCallback( MDagPath& dagPath )
//{
//	MStatus status;
//
//	MObject dagNode = dagPath.node();
//	if ( dagNode.isNull() )
//		return;
//
//	/* MCallbackId id = */ MModelMessage::addNodeAddedToModelCallback( dagNode, _UserNodeAddedCB_static, 0, &status );
//	if ( status.error() ) {
//		MGlobal::displayError( "Failed to install node added to model callback.\n" );
//		return;
//	}
//}

// Decide if the dag is in the model. Dag paths and names
// may not be setup if the dag has not been added to
// the model.
static bool dagNotInModel( MDagPath& dagPath )
{
	MStatus status;
	MFnDagNode dagFn( dagPath, &status );
	if ( status.error() )
		return false;
	bool inModel = dagFn.inModel( &status );
	if ( status.error() )
		return false;
	return ( inModel == false );
}

void dagConverter::startUp()
{
	if ( callbackIds.length() == 0 )
	{
		callbackIds.append( MDagMessage::addAllDagChangesCallback( _UserDAGGenericCB_static ) );
		//callbackIds.append( MDGMessage::addNodeAddedCallback( _NodeAddedCallback ) );
		callbackIds.append( MDGMessage::addNodeRemovedCallback( _NodeRemovedCallback_static ) );
	}

	_InitNodes();
	if ( level_.world_ )
		printHierarchy();

	requestSyncHierarchy();
	syncHierarchy();
}

void dagConverter::shutDown()
{
	for ( unsigned int i = 0; i < callbackIds.length(); i++ ) {
		MMessage::removeCallback( callbackIds[i] );
	}
	callbackIds.clear();

	for ( auto n : level_.nodeProxies_ )
		delete n.second;
	level_.nodeProxies_.clear();
	level_.world_ = nullptr;
}

NodeProxy* dagConverter::findNode( MFnDependencyNode& depNode, bool createIfNotFound )
{
	MUuid uuid = depNode.uuid();
	NodeProxy* nod = nullptr;
	MString uuidStr = uuid.asString();
	Uuid uuid2 = Uuid::fromString( uuidStr.asChar() );
	NodeProxyMap::iterator it = level_.nodeProxies_.find( uuid2 );
	if ( it != level_.nodeProxies_.end() )
	{
		return it->second;
	}
	else if ( createIfNotFound )
	{
		//NodeProxy* n = new NodeProxy( depNode.name().asChar() );
		NodeProxy* n = new NodeProxy();
		DependencyNode* dn = new DependencyNode();
		dn->name_ = depNode.name().asChar();
		dn->flatIndex_ = level_.nodeIdManager_.add( NodeIdData(dn) );
		n->depNode_ = dn;
		level_.nodeProxies_[uuid2] = n;
		return n;
	}

	return nullptr;
}

void dagConverter::deleteNode( MFnDependencyNode& depNode )
{
	MUuid uuid = depNode.uuid();
	NodeProxy* nod = nullptr;
	MString uuidStr = uuid.asString();
	NodeProxyMap::iterator it = level_.nodeProxies_.find( uuidStr.asChar() );
	if ( it != level_.nodeProxies_.end() )
	{
		NodeProxy* n = it->second;
		level_.nodeIdManager_.removeIndex( n->depNode_->flatIndex_ );
		delete n->depNode_;
		delete n;
		level_.nodeProxies_.erase( it );
	}
}

void dagConverter::printHierarchy()
{
	static u32 printNo = 0;
	printNo += 1;

	printf( "---------------------- print hierarchy no %u -----\n", printNo );

	for ( const NodeProxy* n : level_.world_->children_ )
		_PrintHierarchyRecurse( n, "" );

	fflush( stdout );

}

void dagConverter::syncHierarchy()
{
	if ( !syncHierarchyRequested_ )
		return;

	syncHierarchyRequested_ = false;

	if ( level_.world_ )
		printHierarchy();

	for ( auto& p : level_.paths_ )
		p->alive = false;

	GraphStructure oldStruct;
	//oldStruct.pathIdManager_ = &paths_;
	GraphStructure newStruct;
	//oldStruct.pathIdManager_ = &paths_;

	_SyncContext sc( oldStruct, newStruct );

	_SyncHierarchyRecurse( sc, level_.world_ );

	level_.graphStructure_ = std::move( newStruct );

	for ( PathSet::iterator it = level_.paths_.begin(); it != level_.paths_.end(); )
	{
		PathInfo* p = *it;
		if ( p->alive )
			++it;
		else
		{
			sc.removedPaths_.push_back( p->pathId );
			it = level_.paths_.erase( it );
		}
	}

	level_.graphStructure_.printHierarchy();

	if ( !sc.removedPaths_.empty() || !sc.addedPaths_.empty() )
	{
		std::vector<u32> deletedPathsFlatIndices;

		for ( auto i : sc.removedPaths_ )
		{
			printf( "removed path: %u\n", i );
			PathData& pd = level_.pathIdManager_.get( i );
			deletedPathsFlatIndices.push_back( pd.indexIntoFlatDenseArray );
			level_.pathIdManager_.remove( i );
		}

		printf( "--------------------------\n" );

		for ( auto i : sc.addedPaths_ )
		{
			printf( "added path: %u\n", i );
		}

		printf( "--------------------------\n" );

		// inform users that paths have been removed
		//

		GraphChangedParam p;

		p.level = &level_;

		p.deletedPaths = sc.removedPaths_.empty() ? nullptr : &sc.removedPaths_[0];
		p.deletedPathsFlatIndices = deletedPathsFlatIndices.empty() ? nullptr : &deletedPathsFlatIndices[0];
		p.nDeletedPaths = (u32)sc.removedPaths_.size();

		p.createdPaths = sc.addedPaths_.empty() ? nullptr : &sc.addedPaths_[0];
		p.createdPathsFlatIndices = sc.addedPathsFlatIndices_.empty() ? nullptr : &sc.addedPathsFlatIndices_[0];
		p.nCreatedPaths = (u32)sc.addedPaths_.size();

		p.pathsRemap = &sc.pathsRemap_[0];
		p.nPathsRemap = (u32)sc.pathsRemap_.size();

		//p.oldGraphStructure = &oldStruct;
		//p.newGraphStructure = &newStruct;

		gSampleSubsystemFlat.onGraphChanged( p );
		gSampleSubsystemMap.onGraphChanged( p );
	}

	fflush( stdout );
}

void dagConverter::_InitNodes()
{
	u32 currentDepth = 0;
	u32 maxDepthFound = 0;

	enum {
		eMaxDepth = 256
	};
	NodeProxy* currentParents[eMaxDepth];
	for ( NodeProxy*& p : currentParents )
		p = nullptr;

	//currentParents[0] = &gWorld;

	MItDag dagIt;
	//dagIt.next(); // skip world

	while ( !dagIt.isDone() )
	{
		MObject mnodeObj = dagIt.currentItem();
		MFnDependencyNode curNode( mnodeObj );
		MString curNodeName = curNode.name();
		//MUuid uuid = curNode.uuid();
		//Node2* nod = nullptr;
		//MString uuidStr = uuid.asString();
		//Node2Map::iterator it = gNodes2.find( uuidStr.asChar() );
		//if ( it != gNodes2.end() )
		//{
		//	nod = it->second;
		//}
		//else
		//{
		//	nod = new Node2( curNodeName.asChar() );
		//	gNodes2[uuidStr.asChar()] = nod;
		//}
		NodeProxy* nod = findNode( curNode, true );

		u32 depth = dagIt.depth();
		maxDepthFound = std::max( depth, maxDepthFound );

		if ( depth > currentDepth )
		{
			// one level down
		}
		else if ( depth < currentDepth )
		{
			// one level up
			for ( u32 i = depth; i < maxDepthFound; ++i )
				currentParents[i] = nullptr;
		}

		currentDepth = depth;

		//pathIds.push_back( pathId );
		//childCount.push_back( 0 );

		//NodePath* path = new NodePath( nod, nullptr );

		if ( currentDepth > 0 )
		{
			//u32 parentPathFlatIndex = currentParents[currentDepth - 1];
			//++childCount[parentPathFlatIndex];
			//parents.push_back( parentPathFlatIndex );
			NodeProxy* parent = currentParents[currentDepth - 1];
			parent->children_.push_back( nod );
		}
		else
		{
			level_.world_ = nod;
		}

		//currentParents[currentDepth] = pathFlatIndex;
		currentParents[currentDepth] = nod;

		//++pathFlatIndex;

		dagIt.next();
	}
}

void dagConverter::_PrintHierarchyRecurse( const NodeProxy* root, std::string pathBase )
{
	std::string fullPathName = pathBase + "|" + root->depNode_->name_;
	//MGlobal::displayInfo( fullPathName.c_str() );
	printf( "%s\n", fullPathName.c_str() );

	for ( const NodeProxy* c : root->children_ )
		_PrintHierarchyRecurse( c, fullPathName );
}

void dagConverter::_SyncHierarchyRecurse( _SyncContext& sc, NodeProxy* nod )
{
	std::string pathBaseOrig = sc.pathBase_;
	PathInfo pi;

	NodeProxy* shapeNode = nullptr;
	if ( sc.pathFlatIndex_ > 0 )
	{
		// skip world
		sc.pathBase_ = sc.pathBase_ + "|" + nod->depNode_->name_;

		shapeNode = nod->shapeNode();
		if ( shapeNode )
		{
			//pi.fullPathName = sc.pathBase_ + "|" + shapeNode->depNode_->name_;
			sc.pathBase_ = sc.pathBase_ + "|" + shapeNode->depNode_->name_;
			sc.pathNodes_.push_back( shapeNode->depNode_->flatIndex_ );
		}
		else
		{
			//pi.fullPathName = sc.pathBase_;
			sc.pathNodes_.push_back( nod->depNode_->flatIndex_ );
		}

		pi.setPathNodes( &sc.pathNodes_[0], (u16)sc.pathNodes_.size() );
	}

	u32 pathId;
	PathSet::iterator it = level_.paths_.find( &pi );
	if ( it != level_.paths_.end() )
	{
		PathInfo* p = *it;
		p->alive = true;
		pathId = p->pathId;
		PathData& pathData = level_.pathIdManager_.get( pathId );
		sc.pathsRemap_.emplace_back( pathData.indexIntoFlatDenseArray, sc.pathFlatIndex_ );
	}
	else
	{
		// create new path
		PathData np;
		pathId = level_.pathIdManager_.add( np );
		sc.addedPaths_.push_back( pathId );
		sc.addedPathsFlatIndices_.push_back( sc.pathFlatIndex_ );

		PathInfo* mp = new PathInfo();
		//mp->fullPathName = mpForComparisons.fullPathName;
		//mp->fullPathName = pi.fullPathName;
		if ( sc.pathNodes_.size() )
			mp->setPathNodes( &sc.pathNodes_[0], (u16)sc.pathNodes_.size() );

		mp->alive = true;
		mp->pathId = pathId;		level_.paths_.insert( mp );
		sc.pathsRemap_.emplace_back( 0xffff, sc.pathFlatIndex_ );
	}

	PathData& pathData = level_.pathIdManager_.get( pathId );
	pathData.indexIntoFlatDenseArray = sc.pathFlatIndex_;

	//sc.newStructure_->pathIds_.push_back( pathId );
	u32 childCountIndex = (u32)sc.newStructure_->childCount_.size();
	sc.newStructure_->childCount_.push_back( 0 );
	//sc.newStructure_->nodes_.push_back( shapeNode ? shapeNode : nod );
	sc.newStructure_->nodes_.push_back( shapeNode ? shapeNode->depNode_ : nod->depNode_ );
	//if ( currentDepth > 0 )
	//{
	//	u32 parentPathFlatIndex = currentParents[currentDepth - 1];
	//	++childCount[parentPathFlatIndex];
	//	parents.push_back( parentPathFlatIndex );
	//}
	//else
	//{
	//	parents.push_back( 0xffffffff );
	//}

	//if ( sc.parentPathFlatIndex_ == 0 )
	//	sc.newStructure_->parents_.push_back( 0xffffffff );
	//else
		sc.newStructure_->parents_.push_back( sc.parentPathFlatIndex_ );

	//currentParents[currentDepth] = pathFlatIndex;

	//++pathFlatIndex;
	u32 parentPathFlatIndex = sc.parentPathFlatIndex_;
	sc.parentPathFlatIndex_ = sc.pathFlatIndex_;
	//u32 firstPathFlatIndex = sc.pathFlatIndex_;
	++sc.pathFlatIndex_;

	for ( NodeProxy* n : nod->children_ )
	{
		if ( n != shapeNode )
		{
			//sc.parentPathFlatIndex_ = firstPathFlatIndex;
			//sc.
			_SyncHierarchyRecurse( sc, n );
			++sc.newStructure_->childCount_[childCountIndex];
		}
	}

	sc.parentPathFlatIndex_ = parentPathFlatIndex;
	sc.pathBase_ = pathBaseOrig;
	if ( sc.pathNodes_.size() )
		sc.pathNodes_.pop_back();
}

void dagConverter::_NodeRemovedCallback_static( MObject& node, void *clientData )
{
	gDagConverter._NodeRemovedCallback( node, nullptr );
}

void dagConverter::_NodeRemovedCallback( MObject& nodeObject, void *clientData )
{
	MFnDependencyNode node( nodeObject );
	MString nodeName = node.name();
	MGlobal::displayInfo( "Node removed: " + nodeName );
	deleteNode( node );
	requestSyncHierarchy();
}

void dagConverter::_UserNodeAddedCB_static( MObject& node, void *clientData )
{
	gDagConverter._UserNodeAddedCB( node, nullptr );
}

void dagConverter::_UserNodeAddedCB( MObject& node, void *clientData )
{
	MStatus status;

	if ( !node.isNull() ) {
		bool doDisplay = true;

		MDagPath path;
		status = MDagPath::getAPathTo( node, path );
		if ( status.error() ) {
			doDisplay = false;
			MGlobal::displayInfo( "Error: failed to get dag path to node." );
		}

		if ( doDisplay ) {
			MString s = path.fullPathName();
			MString info( "DAG Model -  Node added: " );
			info += s;

			path.transform( &status );
			if ( MS::kInvalidParameter == status ) {
				info += "(WORLD)";
			}

			MGlobal::displayInfo( info );
		}

		u32 pathCount = path.pathCount();
		if ( pathCount > 0 )
		{
			status = path.pop();
			SPAD_ASSERT( status );
			MFnDependencyNode parentNode = path.node();
			NodeProxy* p = findNode( parentNode, false );
			if ( !p )
			{
				MGlobal::displayError( "parent node not found" );
				return;
			}

			MFnDependencyNode childNode( node );
			NodeProxy* c = findNode( childNode, true );
			if ( c )
			{
				// this callback can be called multiple times? wtf? it's only in test plugin I think
				// make sure this node is not a child already
				auto it = std::find( p->children_.begin(), p->children_.end(), c );
				if ( it == p->children_.end() )
					p->children_.push_back( c );
			}
			else
			{
				MGlobal::displayError( "child node not found" );
			}
		}
	}

	// remove the callback
	MCallbackId id = MMessage::currentCallbackId();
	MMessage::removeCallback( id );

	// listen for removal message
	/* MCallbackId id = */ MModelMessage::addNodeRemovedFromModelCallback( node, userNodeRemovedCB, 0, &status );
	if ( status.error() ) {
		MGlobal::displayError( "Failed to install node removed from model callback.\n" );
		return;
	}

	requestSyncHierarchy();
}

void dagConverter::_UserDAGGenericCB_static( MDagMessage::DagMessage msg, MDagPath &child, MDagPath &parent, void * )
{
	gDagConverter._UserDAGGenericCB( msg, child, parent, nullptr );
}

void dagConverter::_UserDAGGenericCB( MDagMessage::DagMessage msg, MDagPath &child, MDagPath &parent, void * )
{
	MString dagStr( "DAG Changed - " );
	switch ( msg ) {
	case MDagMessage::kParentAdded:
		dagStr += "Parent Added: ";
		break;
	case MDagMessage::kParentRemoved:
		dagStr += "Parent Removed: ";
		break;
	case MDagMessage::kChildAdded:
		dagStr += "Child Added: ";
		break;
	case MDagMessage::kChildRemoved:
		dagStr += "Child Removed: ";
		break;
	case MDagMessage::kChildReordered:
		dagStr += "Child Reordered: ";
		break;
	default:
		dagStr += "Unknown Type: ";
		break;
	}

	dagStr += "child = ";
	dagStr += child.fullPathName();
	dagStr += ", parent = ";
	dagStr += parent.fullPathName();

	// Check to see if the parent is the world object.
	//
	MStatus pStat;
	parent.transform( &pStat );
	if ( MS::kInvalidParameter == pStat ) {
		dagStr += "(WORLD)";
	}

	// Install callbacks if node is not in the model.
	// Callback is for node added to model.
	bool incomplete = false;
	if ( dagNotInModel( child ) ) {
		_InstallNodeAddedCallback( child );
		incomplete = true;
	}
	if ( dagNotInModel( parent ) ) {
		_InstallNodeAddedCallback( parent );
		incomplete = true;
	}

	// Warn user that dag path info may be
	// incomplete
	if ( incomplete )
		dagStr += "\t// May be incomplete!";

	MGlobal::displayInfo( dagStr );

	if ( !incomplete )
	{
		if ( msg == MDagMessage::kChildAdded )
		{
			MFnDependencyNode parentNode( parent.node() );
			NodeProxy* p = findNode( parentNode, false );
			if ( !p )
			{
				MGlobal::displayError( "parent node not found" );
				return;
			}

			MFnDependencyNode childNode( child.node() );
			NodeProxy* c = findNode( childNode, true );
			if ( c )
			{
				p->children_.push_back( c );
			}
			else
			{
				MGlobal::displayError( "child node not found" );
			}

			requestSyncHierarchy();
		}
		else if ( msg == MDagMessage::kChildRemoved )
		{
			MFnDependencyNode parentNode( parent.node() );
			NodeProxy* p = findNode( parentNode, false );
			if ( !p )
			{
				MGlobal::displayError( "parent node not found" );
				return;
			}

			MFnDependencyNode childNode( child.node() );
			NodeProxy* c = findNode( childNode, false );
			if ( c )
			{
				auto it = std::find( p->children_.begin(), p->children_.end(), c );
				if ( it != p->children_.end() )
				{
					p->children_.erase( it );
				}
			}
			else
			{
				MGlobal::displayError( "child node not found" );
			}

			requestSyncHierarchy();
		}
	}
}

void dagConverter::_InstallNodeAddedCallback( MDagPath& dagPath )
{
	MStatus status;

	MObject dagNode = dagPath.node();
	if ( dagNode.isNull() )
		return;

	/* MCallbackId id = */ MModelMessage::addNodeAddedToModelCallback( dagNode, _UserNodeAddedCB_static, 0, &status );
	if ( status.error() ) {
		MGlobal::displayError( "Failed to install node added to model callback.\n" );
		return;
	}
}

void GraphStructure::_PrintHierarchyRecurse( u32& flatIndex, std::string pathBase ) const
{
	std::string path;
	if ( flatIndex > 0 )
	{
		const DependencyNode* n = nodes_[flatIndex];
		path = pathBase + "|" + n->name_;
		printf( "%s\n", path.c_str() );
	}

	const u32 nChildren = childCount_[flatIndex];
	++flatIndex;

	for ( u32 ichild = 0; ichild < nChildren; ++ichild )
	{
		_PrintHierarchyRecurse( flatIndex, path );
	}
}
