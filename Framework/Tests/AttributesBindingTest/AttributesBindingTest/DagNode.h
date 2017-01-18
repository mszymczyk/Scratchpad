#pragma once

#include "DependNode.h"

#define DECLARE_ATTRIBUTES_BEGIN \
static constexpr const size_t __firstAttributeIndex = __COUNTER__; \
static constexpr const size_t __nInheritedAttributes = 0; \


#define DECLARE_ATTRIBUTES_END(nodeName) \
static constexpr const size_t __nAttributes = __COUNTER__ - __firstAttributeIndex - 1 + __nInheritedAttributes; \
typedef AttrSetStorage<nodeName::__nAttributes> nodeName##AttrSetType; \
private: \
	static nodeName##AttrSetType __s##nodeName##AttrSet; \
public: \
	static const AttrSet& get##nodeName##AttrSet() { return __s##nodeName##AttrSet; } \


#define __DECLARE_ATTRIBUTE_BOOL_IMPL(attrName, defValue, networked) \
struct Attribute_##attrName \
{ \
bool valueBool = defValue; \
static const AttributeDesc desc; \
static constexpr const AttributeType::Type type = AttributeType::Bool; \
static constexpr const size_t ordinal = __COUNTER__ - __firstAttributeIndex - 1; \
static constexpr const unsigned int hasNetwork = networked; \
} attrName; \


#define DECLARE_ATTRIBUTE_BOOL(attrName, defValue) __DECLARE_ATTRIBUTE_BOOL_IMPL(attrName, defValue, false)
#define DECLARE_ATTRIBUTE_BOOL_NETWORKED(attrName, defValue) __DECLARE_ATTRIBUTE_BOOL_IMPL(attrName, defValue, true)



#define __DECLARE_ATTRIBUTE_U8_IMPL(attrName, defValue, networked) \
struct Attribute_##attrName \
{ \
unsigned char valueU8 = defValue; \
static const AttributeDesc desc; \
static constexpr const AttributeType::Type type = AttributeType::UInt8; \
static constexpr const size_t ordinal = __COUNTER__ - __firstAttributeIndex - 1; \
static constexpr const unsigned int hasNetwork = networked; \
} attrName; \


#define DECLARE_ATTRIBUTE_U8(attrName, defValue) __DECLARE_ATTRIBUTE_U8_IMPL(attrName, defValue, false)
#define DECLARE_ATTRIBUTE_U8_NETWORKED(attrName, defValue) __DECLARE_ATTRIBUTE_U8_IMPL(attrName, defValue, true)




class DagNode : public DependencyNode
{
public:
	DagNode()
		: DependencyNode( DagNode::__sDagNodeAttrSet )
	{	}

	DagNode( const AttrSet& attrSet )
		: DependencyNode( attrSet )
	{	}

	//bool getVisibility() const
	//{
	//	//return getBool( visibility );
	//	//return getBool( sortingPriority );
	//	//return visibility.getBool();
	//	return visibility.bValue;
	//}

private:

public:

	//static constexpr const size_t firstAttributeIndex = __COUNTER__;

	//struct AttributeType_visibility
	//{
	//private:
	//	bool bValue;

	//public:
	//	static const AttributeDesc desc;
	//	static constexpr const AttributeType::Type aType = AttributeType::Bool;
	//	static constexpr const size_t attrNo = __COUNTER__ - firstAttributeIndex - 1;
	//	static constexpr const unsigned int hasNetwork = false;

	//	bool getBool() const { return bValue; }

	//} visibility;

	//struct AttributeType_sortingPriority
	//{
	//private:
	//	unsigned char u8Value;

	//public:
	//	static const AttributeDesc desc;
	//	static constexpr const AttributeType::Type aType = AttributeType::UInt8;
	//	static constexpr const size_t attrNo = __COUNTER__ - firstAttributeIndex - 1;
	//	static constexpr const unsigned int hasNetwork = false;

	//	unsigned char getU8() const { return u8Value; }

	//} sortingPriority;

//	static constexpr const size_t nAttributes = __COUNTER__ - firstAttributeIndex - 1;
//
//	typedef AttrSetStorage<DagNode::nAttributes> DagNodeAttrSetType;
//
//private:
//	static DagNodeAttrSetType sDagNodeAttrSet;
//public:
//	static const AttrSet& getDagNodeAttrSet() { return sDagNodeAttrSet; }

	DECLARE_ATTRIBUTES_BEGIN;

	DECLARE_ATTRIBUTE_BOOL( visibility     , true );
	DECLARE_ATTRIBUTE_U8  ( sortingPriority, 0    );

	DECLARE_ATTRIBUTES_END(DagNode);
};
