// HiStreamTest.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "DependNode.h"
#include <algorithm>


//typedef AttrSetStorage<DependencyNode::nAttributes> DependencyNodeAttrSet;

//struct DependencyNodeAttrSet : public AttrSetStorage<DependencyNode::nAttributes>
//{
//	DependencyNodeAttrSet( const AttrSet* inheritFrom, std::initializer_list<AttributeDesc> descriptors )
//		: AttrSetStorage( descriptors )
//	{
//		//static_assert( sizeof( DependencyNodeAttrSet ) == sizeof( AttrSet ) + 2 * sizeof( AttributeDesc ), "msg" );
//	}
//};

//static DependencyNodeAttrSet g_DependencyNodeAttrSet( nullptr,
//const DependencyNode::DependencyNodeAttrSetType DependencyNode::sDependencyNodeAttrSet( nullptr,
//{
//	AttributeDesc( "visbility", "pvis", offsetof( DependencyNode, visibility ) ),
//	AttributeDesc( "sortingPriority", "sort", offsetof( DependencyNode, sortingPriority ) ),
//}
//);

//typedef AttrSet<DependencyNode::nAttributes> DependencyNodeAttrSet;

//AttrSet DependencyNodeAttrSet( nullptr,
//
//{
//
//AttributeDesc DependencyNode::AttributeType_visibility::desc( "visbility", "pvis", offsetof( DependencyNode, visibility ) );
//dependencyNodeAttrSet.addAttr( &DependencyNode::AttributeType_visibility::desc ):

//
//}

//AttributeDesc AttrSet2::attributeDescStore_[2] = {
//	AttributeDesc( "visbility", "pvis", offsetof( DependencyNode, visibility ) )
//	, AttributeDesc( "sortingPriority", "sort", offsetof( DependencyNode, sortingPriority ) )
//
//};

//const AttrSet& DependencyNode::getAttrSet()
//{
//	return g_DependencyNodeAttrSet;
//}

//DependencyNode::DependencyNodeAttrSetType DependencyNode::sDependencyNodeAttrSet( nullptr );
//
//const AttributeDesc DependencyNode::AttributeType_visibility::desc( &sDependencyNodeAttrSet, DependencyNode::AttributeType_visibility::attrNo, "visbility", "pvis", DependencyNode::AttributeType_visibility::aType, offsetof( DependencyNode, visibility ), DependencyNode::AttributeType_visibility::hasNetwork );
//const AttributeDesc DependencyNode::AttributeType_sortingPriority::desc( &sDependencyNodeAttrSet, DependencyNode::AttributeType_sortingPriority::attrNo, "sortingPriority", "srtp", DependencyNode::AttributeType_sortingPriority::aType, offsetof( DependencyNode, sortingPriority ), DependencyNode::AttributeType_sortingPriority::hasNetwork );
//
//struct DependencyNode_AttrSetFinisher : public AttrSetFinisher
//{
//	DependencyNode_AttrSetFinisher()
//		: AttrSetFinisher( DependencyNode::sDependencyNodeAttrSet )
//	{	}
//};
//
//static DependencyNode_AttrSetFinisher gDependencyNodeAttrSetFinisher;

uint64_t AttributeDesc::packAttributeShorName( const char* shortName )
{
	const char* s = shortName;
	uint64_t res = *s;
	++s;
	while ( *s )
	{
		res <<= 8;
		res |= *s;
		++s;
	}

	return res;
}

void NodeTypeDesc::initializeType( NodeTypeDesc* inheritFrom )
{
	if ( inheritFrom )
	{
		for ( size_t i = 0; i < inheritFrom->attrSet_.nAttributes_; ++i )
			attrSet_.attributesDesc_[i] = attrSet_.attributesDescSorted_[i] = inheritFrom->attrSet_.attributesDesc_[i];

		hasLoadGraphicsData_ = inheritFrom->hasLoadGraphicsData_ || hasLoadGraphicsData_;
	}

	attrSet_.finishAttrSetInitialization();
}
