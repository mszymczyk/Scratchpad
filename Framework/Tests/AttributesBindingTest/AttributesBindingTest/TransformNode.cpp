// HiStreamTest.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "TransformNode.h"
#include <algorithm>

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
