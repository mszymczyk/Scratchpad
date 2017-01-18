// HiStreamTest.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "TransformNode.h"
#include <algorithm>

//typedef AttrSetStorage<TransformNode::nAttributes> TransformNodeAttrSet;

//struct DependencyNodeAttrSet : public AttrSetStorage<DependencyNode::nAttributes>
//{
//	DependencyNodeAttrSet( const AttrSet* inheritFrom, std::initializer_list<AttributeDesc> descriptors )
//		: AttrSetStorage( descriptors )
//	{
//		//static_assert( sizeof( DependencyNodeAttrSet ) == sizeof( AttrSet ) + 2 * sizeof( AttributeDesc ), "msg" );
//	}
//};

//template<unsigned int F>
//struct ReturnOffset
//{
//	//constexpr operator unsigned int() const { return F; }
//	static constexpr unsigned int value = F;
//};

//const TransformNode::TransformNodeAttrSetType TransformNode::sTransformNodeAttrSet( &DependencyNode::getDependencyNodeAttrSet(),
//{
//	AttributeDesc( "transform", "t", AttributeType::Distance, offsetof( TransformNode, translate ), TransformNode::AttributeType_translate::hasNetwork ? offsetof( TransformNode, TransformNode::translate_network ) : 0 ),
//	//AttributeDesc( "rotate", "r", AttributeType::Distance, offsetof( TransformNode, rotate ), TransformNode::AttributeType_rotate::hasNetwork ? offsetof( TransformNode, TransformNode::rotate_network ) : 0 ),
//
//	AttributeDesc( "rotate", "r", AttributeType::Distance, offsetof( TransformNode, rotate ), offsetof( TransformNode, TransformNode::rotate_network ) ),
//		//TransformNode::AttributeType_rotate::hasNetwork ? offsetof( TransformNode, TransformNode::rotate_network ) : 0 ),
//		//std::conditional< TransformNode::AttributeType_rotate::hasNetwork, ReturnOffset<offsetof( TransformNode, TransformNode::rotate_network )>, ReturnOffset<0> >::type::value ),
//
//	//AttributeDesc( "transform", "t", AttributeType::Distance, offsetof( TransformNode, translate ), TransformNode::AttributeType_translate::hasNetwork ),
//	//AttributeDesc( "rotate", "r", AttributeType::Distance, offsetof( TransformNode, rotate ), TransformNode::AttributeType_rotate::hasNetwork ),
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

//TransformNode::TransformNodeAttrSetType TransformNode::sTransformNodeAttrSet;

//TransformNode::TransformNodeAttrSetType TransformNode::sTransformNodeAttrSet( &DagNode::getDagNodeAttrSet() );
//
//const AttributeDesc TransformNode::AttributeType_translate::descX( &sTransformNodeAttrSet, TransformNode::AttributeType_translate::attrNoX, "translateX", "tx", TransformNode::AttributeType_translate::aTypeX, offsetof( TransformNode, translate.x ), TransformNode::AttributeType_translate::hasNetwork );
//const AttributeDesc TransformNode::AttributeType_translate::descY( &sTransformNodeAttrSet, TransformNode::AttributeType_translate::attrNoY, "translateY", "ty", TransformNode::AttributeType_translate::aTypeY, offsetof( TransformNode, translate.y ), TransformNode::AttributeType_translate::hasNetwork );
//const AttributeDesc TransformNode::AttributeType_translate::descZ( &sTransformNodeAttrSet, TransformNode::AttributeType_translate::attrNoZ, "translateZ", "tz", TransformNode::AttributeType_translate::aTypeZ, offsetof( TransformNode, translate.z ), TransformNode::AttributeType_translate::hasNetwork );
//const AttributeDesc TransformNode::AttributeType_translate::desc( &sTransformNodeAttrSet, TransformNode::AttributeType_translate::attrNo, "translate", "t", TransformNode::AttributeType_translate::aType, offsetof( TransformNode, translate ), TransformNode::AttributeType_translate::hasNetwork );
//
//const AttributeDesc TransformNode::AttributeType_rotate::descX( &sTransformNodeAttrSet, TransformNode::AttributeType_rotate::attrNoX, "rotateX", "tx", TransformNode::AttributeType_rotate::aTypeX, offsetof( TransformNode, rotate.x ), TransformNode::AttributeType_rotate::hasNetwork );
//const AttributeDesc TransformNode::AttributeType_rotate::descY( &sTransformNodeAttrSet, TransformNode::AttributeType_rotate::attrNoY, "rotateY", "ty", TransformNode::AttributeType_rotate::aTypeY, offsetof( TransformNode, rotate.y ), TransformNode::AttributeType_rotate::hasNetwork );
//const AttributeDesc TransformNode::AttributeType_rotate::descZ( &sTransformNodeAttrSet, TransformNode::AttributeType_rotate::attrNoZ, "rotateZ", "tz", TransformNode::AttributeType_rotate::aTypeZ, offsetof( TransformNode, rotate.z ), TransformNode::AttributeType_rotate::hasNetwork );
//const AttributeDesc TransformNode::AttributeType_rotate::desc( &sTransformNodeAttrSet, TransformNode::AttributeType_rotate::attrNo, "rotate", "r", TransformNode::AttributeType_rotate::aType, offsetof( TransformNode, rotate ), TransformNode::AttributeType_rotate::hasNetwork );

//struct TransformNode_AttrSetFinisher : public AttrSetFinisher
//{
//	TransformNode_AttrSetFinisher()
//		: AttrSetFinisher( TransformNode::sTransformNodeAttrSet )
//	{	}
//};
//
//static TransformNode_AttrSetFinisher gTransformNodeAttrSetFinisher;

#define DEFINE_ATTRIBUTES_BEGIN_INHERIT(nodeName, inheritFrom) \
nodeName::nodeName##AttrSetType nodeName::__s##nodeName##AttrSet( &inheritFrom::get##inheritFrom##AttrSet() ); \

#define DEFINE_ATTRIBUTE_DISTANCE(nodeName, attrName, shortName ) \
const AttributeDesc nodeName::Attribute_##attrName::descX( &__s##nodeName##AttrSet, nodeName::Attribute_##attrName::ordinalX, #attrName "X", shortName "x", nodeName::Attribute_##attrName::typeX, offsetof( nodeName, attrName.valueX ), nodeName::Attribute_##attrName::hasNetwork ); \
const AttributeDesc nodeName::Attribute_##attrName::descY( &__s##nodeName##AttrSet, nodeName::Attribute_##attrName::ordinalY, #attrName "Y", shortName "y", nodeName::Attribute_##attrName::typeY, offsetof( nodeName, attrName.valueY ), nodeName::Attribute_##attrName::hasNetwork ); \
const AttributeDesc nodeName::Attribute_##attrName::descZ( &__s##nodeName##AttrSet, nodeName::Attribute_##attrName::ordinalZ, #attrName "Z", shortName "z", nodeName::Attribute_##attrName::typeZ, offsetof( nodeName, attrName.valueZ ), nodeName::Attribute_##attrName::hasNetwork ); \
const AttributeDesc nodeName::Attribute_##attrName::desc ( &__s##nodeName##AttrSet, nodeName::Attribute_##attrName::ordinal,  #attrName    , shortName    , nodeName::Attribute_##attrName::type,  offsetof( nodeName, attrName ),		  nodeName::Attribute_##attrName::hasNetwork ); \

DEFINE_ATTRIBUTES_BEGIN_INHERIT( TransformNode, DagNode );

DEFINE_ATTRIBUTE_DISTANCE( TransformNode, translate, "t" );
DEFINE_ATTRIBUTE_DISTANCE( TransformNode, rotate, "r" );

