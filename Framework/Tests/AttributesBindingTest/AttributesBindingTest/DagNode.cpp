// HiStreamTest.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "DagNode.h"
#include <algorithm>

//typedef AttrSetStorage<DagNode::nAttributes> DagNodeAttrSet;

#define DEFINE_ATTRIBUTES_BEGIN(nodeName) \
nodeName::nodeName##AttrSetType nodeName::__s##nodeName##AttrSet( nullptr ); \


#define DEFINE_ATTRIBUTE(nodeName, attrName, shortName ) \
const AttributeDesc nodeName::Attribute_##attrName::desc( &__s##nodeName##AttrSet, nodeName::Attribute_##attrName::ordinal, #attrName, shortName, nodeName::Attribute_##attrName::type, offsetof( nodeName, attrName ), nodeName::Attribute_##attrName::hasNetwork ); \


//DagNode::DagNodeAttrSetType DagNode::sDagNodeAttrSet( nullptr );

//const AttributeDesc DagNode::AttributeType_visibility::desc( &sDagNodeAttrSet, DagNode::AttributeType_visibility::attrNo, "visbility", "pvis", DagNode::AttributeType_visibility::aType, offsetof( DagNode, visibility ), DagNode::AttributeType_visibility::hasNetwork );
//const AttributeDesc DagNode::AttributeType_sortingPriority::desc( &sDagNodeAttrSet, DagNode::AttributeType_sortingPriority::attrNo, "sortingPriority", "srtp", DagNode::AttributeType_sortingPriority::aType, offsetof( DagNode, sortingPriority ), DagNode::AttributeType_sortingPriority::hasNetwork );

DEFINE_ATTRIBUTES_BEGIN( DagNode );

DEFINE_ATTRIBUTE( DagNode, visibility, "pvis" );
DEFINE_ATTRIBUTE( DagNode, sortingPriority, "srtp" );

//struct DagNode_AttrSetFinisher : public AttrSetFinisher
//{
//	DagNode_AttrSetFinisher()
//		: AttrSetFinisher( DagNode::sDagNodeAttrSet )
//	{	}
//};
//
//static DagNode_AttrSetFinisher gDagNodeAttrSetFinisher;
