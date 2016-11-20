//-
// ==========================================================================
// Copyright 1995,2006,2008 Autodesk, Inc. All rights reserved.
//
// Use of this software is subject to the terms of the Autodesk
// license agreement provided at the time of installation or download,
// or which otherwise accompanies this software in either electronic
// or hard copy form.
// ==========================================================================
//+

//
// dagMessageCmd.cpp
//
// Description:
//     Sample plug-in that demonstrates how to register/de-register
//     a callback with the MDagMessage class.
//
//     This plug-in will register a new command in maya called
//     "dagMessage" which adds a callback for the all nodes on
//     the active selection list. A message is printed to stdout 
//     whenever a connection is made or broken for those nodes. If
//		nothing is selected, the callback will be for all nodes.
//
//	   dagMessage -help will list the options.
//
#include <maya/MIOStream.h>
#include <maya/MPxCommand.h>
#include <maya/MFnPlugin.h>
#include <maya/MArgList.h>
#include <maya/MArgDatabase.h>
#include <maya/MIntArray.h>
#include <maya/MSelectionList.h>
#include <maya/MGlobal.h>
#include <maya/MPlug.h>
#include <maya/MSyntax.h>
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

#define kCmdName					"dagMessage"

#define kAllDagFlag 				"-ad"
#define kAllDagFlagLong				"-allDag"
#define kParentAddedFlag			"-pa"
#define kParentAddedFlagLong		"-parentAdded"
#define kParentRemovedFlag			"-pr"
#define kParentRemovedFlagLong		"-parentRemoved"
#define kChildAddedFlag				"-ca"
#define kChildAddedFlagLong			"-childAdded"
#define kChildRemovedFlag			"-cr"
#define kChildRemovedFlagLong		"-childRemoved"
#define kChildReorderedFlag			"-cro"
#define kChildReorderedFlagLong		"-childReordered"

#define kHelpFlag					"-h"
#define kHelpFlagLong				"-help"

#define CheckErrorContinue(stat, msg)	\
	if (MS::kSuccess != stat) {			\
		displayError(msg);				\
		continue;						\
	}							

#define CheckErrorReturn(stat, msg)		\
	if (MS::kSuccess != stat) {			\
		displayError(msg);				\
		return;							\
	}

// This table will keep track of the registered callbacks
// so they can be removed when the plug-ins is unloaded.
//
MCallbackIdArray callbackIds;

struct Node
{
	std::string name_;
	//std::vector<Node*> children_;

	Node() {}
	Node( const char* name ) : name_( name ) {}

	//~Node()
	//{
	//	clear();
	//}

	//void clear()
	//{
	//	for (auto n : children_)
	//		delete n;

	//	children_.clear();
	//	name_.clear();
	//}
};

struct Node2
{
	std::string name_;
	std::vector<Node2*> children_;

	Node2() {}
	Node2( const char* name ) : name_( name ) {}

	~Node2()
	{
		clear();
	}

	void clear()
	{
		//for (auto n : children_)
		//	delete n;

		children_.clear();
		name_.clear();
	}
};

struct NodePath
{
	NodePath( Node* node, NodePath* parent )
		: node_( node )
		, parentPath_( parent )
	{ }

	~NodePath()
	{
		clear();
	}

	void clear()
	{
		for (auto n : childrenPaths_)
			delete n;

		childrenPaths_.clear();
	}

	Node* node_ = nullptr;
	NodePath* parentPath_ = nullptr;
	std::vector<NodePath*> childrenPaths_;
};

//Node gWorld;

typedef unsigned int u32;

typedef std::map<std::string, Node*> NodeMap;
typedef std::map<std::string, Node2*> Node2Map;

NodeMap gNodes;
NodePath* gWorldPath = nullptr;

Node2Map gNodes2;
Node2* gWorld2;


// Node added to model callback.
static void userNodeRemovedCB(MObject& node,void *clientData)
{
	if (! node.isNull()) {
		bool doDisplay = true;

		MStatus status;
		MFnDagNode dagNode(node,&status);
		if ( status.error() ) {
			doDisplay = false;
			MGlobal::displayInfo("Error: failed to get dag node.");
		}

		if ( doDisplay ) {
			MString s = dagNode.name();
			MString info("DAG Model -  Node removed: ");
			info+= s;

			MGlobal::displayInfo(info);	
		}
	}

	// remove the callback
	MCallbackId id = MMessage::currentCallbackId();
	MMessage::removeCallback(id);
}

// Node added to model callback.
static void userNodeAddedCB(MObject& node,void *clientData)
{
	MStatus status;

	if (! node.isNull()) {
		bool doDisplay = true;

		MDagPath path;
		status = MDagPath::getAPathTo(node,path);
		if ( status.error() ) {
			doDisplay = false;
			MGlobal::displayInfo("Error: failed to get dag path to node.");
		}

		if ( doDisplay ) {
			MString s = path.fullPathName();
			MString info("DAG Model -  Node added: ");
			info+= s;

			path.transform(&status);
			if (MS::kInvalidParameter == status) {
				info += "(WORLD)";
			}

			MGlobal::displayInfo(info);	
		}
	}

	// remove the callback
	MCallbackId id = MMessage::currentCallbackId();
	MMessage::removeCallback(id);

	// listen for removal message
	/* MCallbackId id = */ MModelMessage::addNodeRemovedFromModelCallback( node, userNodeRemovedCB, 0, &status );
	if ( status.error() ) {
		MGlobal::displayError("Failed to install node removed from model callback.\n");
		return;
	}
}

// Install a node added callback for the node specified
// by dagPath.
static void installNodeAddedCallback( MDagPath& dagPath )
{
	MStatus status;

	MObject dagNode = dagPath.node();
	if ( dagNode.isNull() )
		return;

	/* MCallbackId id = */ MModelMessage::addNodeAddedToModelCallback( dagNode, userNodeAddedCB, 0, &status );
	if ( status.error() ) {
		MGlobal::displayError("Failed to install node added to model callback.\n");
		return;
	}
}

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

Node2* findNode( MFnDependencyNode& depNode, bool createIfNotFound )
{
	MUuid uuid = depNode.uuid();
	Node2* nod = nullptr;
	MString uuidStr = uuid.asString();
	Node2Map::iterator it = gNodes2.find( uuidStr.asChar() );
	if (it != gNodes2.end())
	{
		return it->second;
	}
	else if ( createIfNotFound )
	{
		return new Node2( depNode.name().asChar() );
	}

	return nullptr;
}

void userDAGGenericCB(MDagMessage::DagMessage msg, MDagPath &child,
					  MDagPath &parent, void *)
{	
	MString dagStr("DAG Changed - ");
	switch (msg) {
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
	parent.transform(&pStat);
	if (MS::kInvalidParameter == pStat) {
		dagStr += "(WORLD)";
	}

	// Install callbacks if node is not in the model.
	// Callback is for node added to model.
	bool incomplete = false;
	if ( dagNotInModel( child ) ) {
		installNodeAddedCallback( child );
		incomplete = true;
	}
	if ( dagNotInModel( parent ) ) {
		installNodeAddedCallback( parent);
		incomplete = true;
	}

	// Warn user that dag path info may be
	// incomplete
	if (incomplete)
		dagStr += "\t// May be incomplete!";	

	MGlobal::displayInfo(dagStr);

	if (!incomplete)
	{
		if (msg == MDagMessage::kChildAdded)
		{
			MFnDependencyNode parentNode( parent.node() );
			Node2* p = findNode( parentNode, false );
			if (!p)
			{
				MGlobal::displayError( "parent node not found" );
				return;
			}

			MFnDependencyNode childNode( child.node() );
			Node2* c = findNode( childNode, true );
			p->children_.push_back( p );
		}
		else if ( msg == MDagMessage::kChildRemoved )
		{
			MFnDependencyNode parentNode( parent.node() );
			Node2* p = findNode( parentNode, false );
			if (!p)
			{
				MGlobal::displayError( "parent node not found" );
				return;
			}

			MFnDependencyNode childNode( child.node() );
			Node2* c = findNode( childNode, false );
			if (c)
			{
				auto it = std::find( p->children_.begin(), p->children_.end(), c );
				if (it != p->children_.end())
				{
					p->children_.erase( it );
				}
			}
		}
	}
}


//////////////////////////////////////////////////////////////////////////
//
// Command class declaration
//
//////////////////////////////////////////////////////////////////////////

class dagMessageCmd : public MPxCommand
{
public:
					dagMessageCmd() {};
	virtual			~dagMessageCmd(); 
	MStatus			doIt( const MArgList& args );

	//static MSyntax	newSyntax();
	static void*	creator();

private:

	//MStatus			addGenericCallback(MDagPath *dagPath, 
	//								   MDagMessage::DagMessage msg, 
	//								   MString cbName);	
};

//////////////////////////////////////////////////////////////////////////
//
// Command class implementation
//
//////////////////////////////////////////////////////////////////////////

dagMessageCmd::~dagMessageCmd() {}

void* dagMessageCmd::creator()
{
	return new dagMessageCmd();
}

//MSyntax dagMessageCmd::newSyntax()
//{
//	MSyntax syntax;
//
//	syntax.useSelectionAsDefault(true);
//	syntax.setObjectType(MSyntax::kSelectionList);
//	syntax.setMinObjects(0);
//
//	syntax.addFlag(kAllDagFlag, kAllDagFlagLong);
//	syntax.addFlag(kParentAddedFlag, kParentAddedFlagLong);
//	syntax.addFlag(kParentRemovedFlag, kParentRemovedFlagLong);
//	syntax.addFlag(kChildAddedFlag, kChildAddedFlagLong);
//	syntax.addFlag(kChildRemovedFlag, kChildRemovedFlagLong);
//	syntax.addFlag(kChildReorderedFlag, kChildReorderedFlagLong);
//	syntax.addFlag(kHelpFlag, kHelpFlagLong);
//	return syntax;
//}

//MStatus dagMessageCmd::addGenericCallback(MDagPath *dagPath, 
//										  MDagMessage::DagMessage msg, 
//										  MString cbName)
//{
//	MStatus status = MS::kFailure;
//
//	if (NULL == dagPath) {
//		MCallbackId id = MDagMessage::addDagCallback(	msg,
//														userDAGGenericCB, 
//														NULL, 
//														&status);
//		if (MS::kSuccess == status) {
//			MString info("Adding a callback for");
//			info += cbName;
//			info += "on all nodes";
//			MGlobal::displayInfo(info);			
//			callbackIds.append( id );
//		} else {
//			MString err("Could not add callback to");
//			err += dagPath->fullPathName();
//			MGlobal::displayError(err);
//		}
//	} else {
//		MCallbackId id = MDagMessage::addDagCallback(*dagPath,
//													msg,
//													userDAGGenericCB, 
//													NULL, 
//													&status);
//		if (MS::kSuccess == status) {
//			MString info("Adding a callback for");
//			info += cbName;
//			info += "on ";
//			info += dagPath->fullPathName();
//			MGlobal::displayInfo(info);			
//			callbackIds.append( id );
//		} else {
//			MString err("Could not add callback to");
//			err += dagPath->fullPathName();
//			MGlobal::displayError(err);
//		}
//	}
//
//	return status;
//}

MStatus dagMessageCmd::doIt( const MArgList& args)
//
// Takes the  nodes that are on the active selection list and adds an
// attriubte changed callback to each one.
//
{
	//MStatus 		status;
	//MSelectionList 	list;
 //   MArgDatabase argData(syntax(), args);

	//status = argData.getObjects(list);
	//if (MS::kSuccess != status) {
	//	MGlobal::displayError("Error getting objects");
	//	return status;
	//}

	////	Get the flags
	////
	//bool allDagUsed = argData.isFlagSet(kAllDagFlag);
	//bool parentAddedUsed = argData.isFlagSet(kParentAddedFlag);
	//bool parentRemovedUsed = argData.isFlagSet(kParentRemovedFlag);
	//bool childAddedUsed = argData.isFlagSet(kChildAddedFlag);
	//bool childRemovedUsed = argData.isFlagSet(kChildRemovedFlag);
	//bool childReorderedUsed = argData.isFlagSet(kChildReorderedFlag);
	//bool helpUsed = argData.isFlagSet(kHelpFlag);

	//bool nothingSet = (	!allDagUsed && !parentAddedUsed && 
	//					!parentRemovedUsed && !childAddedUsed && 
	//					!childRemovedUsed && !childReorderedUsed && 
	//					!helpUsed);

	//if (nothingSet) {
	//	MGlobal::displayError("A flag must be used. dagMessage -help for availible flags.");
	//	return MS::kFailure;
	//}

	//if (argData.isFlagSet(kHelpFlag)) {
	//	MGlobal::displayInfo("dagMessage -help");
	//	MGlobal::displayInfo("\tdagMessage adds a callback to the selected nodes,");
	//	MGlobal::displayInfo("\tor if no nodes are selected, to all nodes. The callback");
	//	MGlobal::displayInfo("\tprints a message when called. When the plug-in is unloaded");
	//	MGlobal::displayInfo("\tthe callbacks are removed.");
	//	MGlobal::displayInfo("");
	//	MGlobal::displayInfo("\t-h -help : This message is printed");
	//	MGlobal::displayInfo("\t-ad -allDag : parent changes and child reorders");
	//	MGlobal::displayInfo("\t-pa -parentAdded : A parent is added");
	//	MGlobal::displayInfo("\t-pr -parentRemoved : A parent is removed");
	//	MGlobal::displayInfo("\t-ca -childAdded : A child is added (only for individual nodes)");
	//	MGlobal::displayInfo("\t-cr -childRemoved : A child is removed (only for individual nodes)");
	//	MGlobal::displayInfo("\t-cro -childReordered : A child is reordered");
	//	MGlobal::displayInfo("");
	//}

	//unsigned nObjs = list.length();
	//if (nObjs == 0) {
	//	//	Add the callback for all changes of the specified type.
	//	//
	//	if (allDagUsed) {
	//		MCallbackId id = MDagMessage::addAllDagChangesCallback(userDAGGenericCB, NULL, &status);
	//		if (status) {
	//			callbackIds.append( id );
	//			MGlobal::displayInfo("Added a callback for all Dag changes on all nodes.\n");
	//		} else {
	//			MGlobal::displayError("Could not add a -allDag callback");
	//			return status;
	//		}
	//	}

	//	if (parentAddedUsed) {
	//		status = addGenericCallback(NULL, 
	//									MDagMessage::kParentAdded,
	//									MString(" parent added "));
	//		if (MS::kSuccess != status) {
	//			return status;
	//		}
	//	}

	//	if (parentRemovedUsed) {
	//		status = addGenericCallback(NULL, 
	//									MDagMessage::kParentRemoved,
	//									MString(" parent removed "));
	//		if (MS::kSuccess != status) {
	//			return status;
	//		}
	//	}		

	//	if (childAddedUsed) {
	//		MGlobal::displayError("-childAdded can only be used when a node is selected");
	//		status = MS::kFailure;
	//		return status;
	//	}

	//	if (childRemovedUsed) {
	//		MGlobal::displayError("-childRemoved can only be used when a node is selected");
	//		status = MS::kFailure;
	//		return status;
	//	}	

	//	if (childReorderedUsed) {
	//		status = addGenericCallback(NULL, 
	//									MDagMessage::kChildReordered,
	//									MString(" child reordered "));
	//		if (MS::kSuccess != status) {
	//			return status;
	//		}
	//	}	
	//} else {
	//	for (unsigned int i=0; i< nObjs; i++) {
	//		MDagPath dagPath;
	//		list.getDagPath(i, dagPath);

	//		if (!dagPath.isValid()) {
	//			continue;
	//		}

	//		//	Add the callback for all changes of the specified type.
	//		//
	//		if (allDagUsed) {
	//			MCallbackId id = MDagMessage::addAllDagChangesCallback(dagPath, userDAGGenericCB, NULL, &status);
	//			if (status) {
	//				callbackIds.append( id );
	//				MString infoStr("Added a callback for all Dag changes on ");
	//				infoStr += dagPath.fullPathName();
	//				MGlobal::displayInfo(infoStr);
	//			} else {
	//				MGlobal::displayError("Could not add a -allDag callback");
	//				return status;
	//			}
	//		}

	//		if (parentAddedUsed) {
	//			status = addGenericCallback(&dagPath, 
	//										MDagMessage::kParentAdded,
	//										MString(" parent added "));
	//			if (MS::kSuccess != status) {
	//				return status;
	//			}
	//		}

	//		if (parentRemovedUsed) {
	//			status = addGenericCallback(&dagPath, 
	//										MDagMessage::kParentRemoved,
	//										MString(" parent removed "));
	//			if (MS::kSuccess != status) {
	//				return status;
	//			}
	//		}		

	//		if (childAddedUsed) {
	//			status = addGenericCallback(&dagPath, 
	//										MDagMessage::kChildAdded,
	//										MString(" child added "));
	//			if (MS::kSuccess != status) {
	//				return status;
	//			}
	//		}

	//		if (childRemovedUsed) {
	//			status = addGenericCallback(&dagPath, 
	//										MDagMessage::kChildRemoved,
	//										MString(" child removed "));
	//			if (MS::kSuccess != status) {
	//				return status;
	//			}
	//		}	

	//		if (childReorderedUsed) {
	//			status = addGenericCallback(&dagPath, 
	//										MDagMessage::kChildReordered,
	//										MString(" child reordered "));
	//			if (MS::kSuccess != status) {
	//				return status;
	//			}
	//		}	
	//	}
	//}
	//
	//return status;

	if ( callbackIds.length() == 0 )
		callbackIds.append( MDagMessage::addAllDagChangesCallback( userDAGGenericCB ) );

	return MStatus::kSuccess;
}

//////////////////////////////////////////////////////////////////////////
//
// Plugin registration
//
//////////////////////////////////////////////////////////////////////////

void initNodes()
{
	u32 currentDepth = 0;
	u32 maxDepthFound = 0;

	enum { eMaxDepth = 256 };
	NodePath* currentParents[eMaxDepth];
	for ( NodePath*& p : currentParents )
		p = nullptr;

	//currentParents[0] = &gWorld;

	MItDag dagIt;
	//dagIt.next(); // skip world

	while (!dagIt.isDone())
	{
		MObject mnodeObj = dagIt.currentItem();
		MFnDependencyNode curNode( mnodeObj );
		MString curNodeName = curNode.name();
		MUuid uuid = curNode.uuid();
		Node* nod = nullptr;
		MString uuidStr = uuid.asString();
		NodeMap::iterator it = gNodes.find( uuidStr.asChar() );
		if (it != gNodes.end())
		{
			nod = it->second;
		}
		else
		{
			nod = new Node( curNodeName.asChar() );
			gNodes[uuidStr.asChar()] = nod;
		}

		u32 depth = dagIt.depth();
		maxDepthFound = std::max( depth, maxDepthFound );

		if (depth > currentDepth)
		{
			// one level down
		}
		else if (depth < currentDepth)
		{
			// one level up
			for (u32 i = depth; i < maxDepthFound; ++i)
				currentParents[i] = nullptr;
		}

		currentDepth = depth;

		//pathIds.push_back( pathId );
		//childCount.push_back( 0 );

		NodePath* path = new NodePath( nod, nullptr );

		if (currentDepth > 0)
		{
			//u32 parentPathFlatIndex = currentParents[currentDepth - 1];
			//++childCount[parentPathFlatIndex];
			//parents.push_back( parentPathFlatIndex );
			NodePath* parent = currentParents[currentDepth - 1];
			path->parentPath_ = parent;
			parent->childrenPaths_.push_back( path );
		}
		else
		{
			//parents.push_back( 0xffffffff );
			//parent = &gWorld;
			gWorldPath = path;
		}

		//currentParents[currentDepth] = pathFlatIndex;
		currentParents[currentDepth] = path;

		//++pathFlatIndex;

		dagIt.next();
	}
}

void initNodes2()
{
	u32 currentDepth = 0;
	u32 maxDepthFound = 0;

	enum {
		eMaxDepth = 256
	};
	Node2* currentParents[eMaxDepth];
	for (Node2*& p : currentParents)
		p = nullptr;

	//currentParents[0] = &gWorld;

	MItDag dagIt;
	//dagIt.next(); // skip world

	while (!dagIt.isDone())
	{
		MObject mnodeObj = dagIt.currentItem();
		MFnDependencyNode curNode( mnodeObj );
		MString curNodeName = curNode.name();
		MUuid uuid = curNode.uuid();
		Node2* nod = nullptr;
		MString uuidStr = uuid.asString();
		Node2Map::iterator it = gNodes2.find( uuidStr.asChar() );
		if (it != gNodes2.end())
		{
			nod = it->second;
		}
		else
		{
			nod = new Node2( curNodeName.asChar() );
			gNodes2[uuidStr.asChar()] = nod;
		}

		u32 depth = dagIt.depth();
		maxDepthFound = std::max( depth, maxDepthFound );

		if (depth > currentDepth)
		{
			// one level down
		}
		else if (depth < currentDepth)
		{
			// one level up
			for (u32 i = depth; i < maxDepthFound; ++i)
				currentParents[i] = nullptr;
		}

		currentDepth = depth;

		//pathIds.push_back( pathId );
		//childCount.push_back( 0 );

		//NodePath* path = new NodePath( nod, nullptr );

		if (currentDepth > 0)
		{
			//u32 parentPathFlatIndex = currentParents[currentDepth - 1];
			//++childCount[parentPathFlatIndex];
			//parents.push_back( parentPathFlatIndex );
			Node2* parent = currentParents[currentDepth - 1];
			parent->children_.push_back( nod );
		}
		else
		{
			gWorld2 = nod;
		}

		//currentParents[currentDepth] = pathFlatIndex;
		currentParents[currentDepth] = nod;

		//++pathFlatIndex;

		dagIt.next();
	}
}

void printPath( const NodePath* leaf, std::string& fullPathName )
{
	if (leaf->parentPath_)
	{
		fullPathName = "|" + leaf->node_->name_ + fullPathName;
		printPath( leaf->parentPath_, fullPathName );
	}
};

void printNodes( const NodePath* root )
{
	std::string fullPathName;
	printPath( root, fullPathName );

	//printf( "path: %s\n", fullPathName.c_str() );
	MGlobal::displayInfo( fullPathName.c_str() );

	for (const NodePath* cp : root->childrenPaths_)
		printNodes( cp );
}

void printNodes2( const Node2* root, std::string pathBase )
{
	//std::string fullPathName;
	//printPath( root, fullPathName );

	//printf( "path: %s\n", fullPathName.c_str() );
	std::string fullPathName = pathBase + "|" + root->name_;
	MGlobal::displayInfo( fullPathName.c_str() );

	for (const Node2* c : root->children_ )
		printNodes2( c, fullPathName );
}

MStatus initializePlugin( MObject obj )
{
	MFnPlugin plugin( obj );

	//initNodes();
	//if (gWorldPath)
	//	printNodes( gWorldPath );

	initNodes2();
	if (gWorld2)
	{
		for ( const Node2* n : gWorld2->children_ )
			printNodes2( n, "" );
	}

	//fflush( stdout );

	return plugin.registerCommand( kCmdName,
									dagMessageCmd::creator
									//, dagMessageCmd::newSyntax
	);
}

MStatus uninitializePlugin( MObject obj)
{
	// Remove callbacks
	//
	for (unsigned int i=0; i<callbackIds.length(); i++ ) {
		MMessage::removeCallback( callbackIds[i] );
	}
	callbackIds.clear();

	//gWorld.clear();
	//for (auto n : gNodes)
	//	delete n.second;
	//gNodes.clear();

	//if (gWorldPath)
	//{
	//	delete gWorldPath;
	//	gWorldPath = nullptr;
	//}
	//delete gWorld2;
	//gWorld2 = nullptr;
	for (auto n : gNodes2)
		delete n.second;
	gNodes2.clear();
	gWorld2 = nullptr;

	MFnPlugin plugin( obj );
	return plugin.deregisterCommand( kCmdName );
}


