#pragma once

#include "DependNode.h"

#define DECLARE_ATTRIBUTES_BEGIN(nodeName) \
static constexpr const size_t __firstAttributeIndex = __COUNTER__; \
static constexpr const size_t __nInheritedAttributes = 0; \
typedef nodeName ThisNodeType; \



//#define DECLARE_ATTRIBUTES_END(nodeName) \
//static constexpr const size_t __nAttributes = __COUNTER__ - __firstAttributeIndex - 1 + __nInheritedAttributes; \
//typedef AttrSetStorage<nodeName::__nAttributes> nodeName##AttrSetType; \
//private: \
//	static nodeName##AttrSetType __s##nodeName##AttrSet; \
//public: \
//	static const AttrSet& get##nodeName##AttrSet() { return __s##nodeName##AttrSet; } \

//#define DECLARE_ATTRIBUTES_END(nodeName) \
//static constexpr const size_t __nAttributes = __COUNTER__ - __firstAttributeIndex - 1 + __nInheritedAttributes; \
//typedef NodeTypeDescImpl<nodeName::__nAttributes> nodeName##TypeDesc; \
//private: \
//	static nodeName##TypeDesc __s##nodeName##TypeDesc; \
//public: \
//	static NodeTypeDesc& get##nodeName##TypeDesc() { return __s##nodeName##TypeDesc; } \


#define DECLARE_ATTRIBUTES_END(nodeName) \
static constexpr const size_t __nAttributes = __COUNTER__ - __firstAttributeIndex - 1 + __nInheritedAttributes; \
typedef NodeTypeDescImpl<nodeName> nodeName##TypeDesc; \
private: \
	static nodeName##TypeDesc __s##nodeName##TypeDesc; \
public: \
	static NodeTypeDesc& get##nodeName##TypeDesc() { return __s##nodeName##TypeDesc; } \


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


#define DEFINE_ATTRIBUTES_BEGIN(nodeName, typeName) \
nodeName::nodeName##TypeDesc nodeName::__s##nodeName##TypeDesc(typeName, nodeName::typeID); \


#define DEFINE_ATTRIBUTE(nodeName, attrName, shortName ) \
const AttributeDesc nodeName::Attribute_##attrName::desc( &__s##nodeName##TypeDesc.attrSet_, nodeName::Attribute_##attrName::ordinal, #attrName, shortName, nodeName::Attribute_##attrName::type, offsetof( nodeName, attrName ), nodeName::Attribute_##attrName::hasNetwork ); \


class DagNode : public DependencyNode
{
public:
	static const uint32_t typeID = 0x2;

	DagNode()
		//: DependencyNode( DagNode::__sDagNodeAttrSet )
		: DependencyNode( DagNode::__sDagNodeTypeDesc )
	{	}

	//DagNode( const AttrSet& attrSet )
	//	: DependencyNode( attrSet )
	//{	}
	DagNode( const NodeTypeDesc& ntd )
		: DependencyNode( ntd )
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

	//struct Attribute_visibility
	//{
	//	bool valueBool;
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

	DECLARE_ATTRIBUTES_BEGIN( DagNode );

	//DECLARE_ATTRIBUTE_BOOL( visibility, true );



	struct Attribute_visibility
	{
		bool valueBool;
		static const AttributeDesc desc;
		static constexpr const AttributeType::Type type = AttributeType::Bool;
		static constexpr const size_t ordinal = __COUNTER__ - __firstAttributeIndex - 1;
		static constexpr const unsigned int hasNetwork = false;

		bool getBool() const { return valueBool; }
		void setBool( bool val )
		{
			valueBool = val;
			ThisNodeType* dn = reinterpret_cast<ThisNodeType*>( reinterpret_cast<uint8_t*>( &valueBool ) - ( offsetof( ThisNodeType, visibility) + offsetof( Attribute_visibility, valueBool ) ) );
			if ( hasNetwork )
				dn->_SetBool_destinations( desc, val );
		}

	} visibility;

	DECLARE_ATTRIBUTE_U8( sortingPriority, 100 );

	DECLARE_ATTRIBUTES_END( DagNode );



	//typedef NodeTypeDescImpl<DagNode::__nAttributes> DagNodeTypeDesc;

	//private:
	//	static DagNodeTypeDesc __sDagNodeTypeDesc;
	//public:
	//	static DagNodeTypeDesc& getDagNodeTypeDesc() { return __sDagNodeTypeDesc; }



	void setVisibility( bool onOff )
	{
		 //visibility.valueBool = onOff;
		
	}
};
