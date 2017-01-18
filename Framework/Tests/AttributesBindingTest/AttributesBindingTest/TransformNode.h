#pragma once

#include "DagNode.h"
//#include "generated/TransformNode.generated.h"

#define DECLARE_ATTRIBUTES_BEGIN_INHERIT(inheritFrom) \
static constexpr const size_t __firstAttributeIndex = __COUNTER__; \
static constexpr const size_t __nInheritedAttributes = inheritFrom::__nAttributes; \


#define __DECLARE_ATTRIBUTE_DISTANCE_IMPL(attrName, defValueX, defValueY, defValueZ, networked) \
struct Attribute_##attrName \
{ \
	float valueX = defValueX; \
	float valueY = defValueY; \
	float valueZ = defValueZ; \
	static const AttributeDesc descX; \
	static const AttributeDesc descY; \
	static const AttributeDesc descZ; \
	static const AttributeDesc desc; \
	static constexpr const AttributeType::Type typeX = AttributeType::Float; \
	static constexpr const AttributeType::Type typeY = AttributeType::Float; \
	static constexpr const AttributeType::Type typeZ = AttributeType::Float; \
	static constexpr const AttributeType::Type type = AttributeType::Distance; \
	static constexpr const size_t ordinalX = __COUNTER__ - __firstAttributeIndex - 1 + __nInheritedAttributes; \
	static constexpr const size_t ordinalY = __COUNTER__ - __firstAttributeIndex - 1 + __nInheritedAttributes; \
	static constexpr const size_t ordinalZ = __COUNTER__ - __firstAttributeIndex - 1 + __nInheritedAttributes; \
	static constexpr const size_t ordinal  = __COUNTER__ - __firstAttributeIndex - 1 + __nInheritedAttributes; \
	static constexpr const unsigned int hasNetwork = networked; \
} attrName; \


#define DECLARE_ATTRIBUTE_DISTANCE(attrName, defValueX, defValueY, defValueZ) __DECLARE_ATTRIBUTE_DISTANCE_IMPL(attrName, defValueX, defValueY, defValueZ, false)
#define DECLARE_ATTRIBUTE_DISTANCE_NETWORKED(attrName, defValueX, defValueY, defValueZ) __DECLARE_ATTRIBUTE_DISTANCE_IMPL(attrName, defValueX, defValueY, defValueZ, true)

class TransformNode : public DagNode
{
public:
	//TransformNode()
	//	: DependencyNode( NodeAttributesNS::TransformNodeAttributes::getAttributeSet() )
	//{	}

	TransformNode()
		: DagNode( getTransformNodeAttrSet() )
	{	}

	////DECLARE_FIRST_ATTRIBUTE_DEPENDENCY_NODE( visibility, BoolAttribute )
	////DECLARE_ATTRIBUTES_END( DependencyNode, visibility )

	//DECLARE_ATTRIBUTES_BEGIN2( DependencyNode )
	//DECLARE_BOOL_ATTRIBUTE( visibility )
	//DECLARE_ATTRIBUTES_END2

	//DECLARE_ATTRIBUTES_BEGIN_INHERIT( TransformNode, DependencyNode );

	//static TransformNodeAttributes::AttributeType_translateX translateX;
	//static TransformNodeAttributes::AttributeType_translateY translateY;
	//static TransformNodeAttributes::AttributeType_translateZ translateZ;
	//static TransformNodeAttributes::AttributeType_translate translate;

	//DECLARE_ATTRIBUTE_DISTANCE2( translate, "t" );

	//DECLARE_ATTRIBUTES_END;

	//const AttributeSet& getAttributeSet();


//	static constexpr const size_t firstAttributeIndex = __COUNTER__;
//	static constexpr const size_t nInheritedAttributes = DagNode::nAttributes;
//	//static const AttrNetwork fake_network;
//
//	struct AttributeType_translate
//	{
//	private:
//		float x;
//		float y;
//		float z;
//
//	public:
//		static const AttributeDesc descX;
//		static const AttributeDesc descY;
//		static const AttributeDesc descZ;
//		static const AttributeDesc desc;
//		static constexpr const AttributeType::Type aTypeX = AttributeType::Float;
//		static constexpr const AttributeType::Type aTypeY = AttributeType::Float;
//		static constexpr const AttributeType::Type aTypeZ = AttributeType::Float;
//		static constexpr const AttributeType::Type aType = AttributeType::Distance;
//		static constexpr const size_t attrNoX = __COUNTER__ - firstAttributeIndex - 1 + nInheritedAttributes;
//		static constexpr const size_t attrNoY = __COUNTER__ - firstAttributeIndex - 1 + nInheritedAttributes;
//		static constexpr const size_t attrNoZ = __COUNTER__ - firstAttributeIndex - 1 + nInheritedAttributes;
//		static constexpr const size_t attrNo = __COUNTER__ - firstAttributeIndex - 1 + nInheritedAttributes;
//		static constexpr const unsigned int hasNetwork = true;
//
//	} translate;
//
//	struct AttributeType_rotate
//	{
//	private:
//		float x;
//		float y;
//		float z;
//
//	public:
//		static const AttributeDesc descX;
//		static const AttributeDesc descY;
//		static const AttributeDesc descZ;
//		static const AttributeDesc desc;
//		static constexpr const AttributeType::Type aTypeX = AttributeType::Float;
//		static constexpr const AttributeType::Type aTypeY = AttributeType::Float;
//		static constexpr const AttributeType::Type aTypeZ = AttributeType::Float;
//		static constexpr const AttributeType::Type aType = AttributeType::Distance;
//		static constexpr const size_t attrNoX = __COUNTER__ - firstAttributeIndex - 1 + nInheritedAttributes;
//		static constexpr const size_t attrNoY = __COUNTER__ - firstAttributeIndex - 1 + nInheritedAttributes;
//		static constexpr const size_t attrNoZ = __COUNTER__ - firstAttributeIndex - 1 + nInheritedAttributes;
//		static constexpr const size_t attrNo = __COUNTER__ - firstAttributeIndex - 1 + nInheritedAttributes;
//		static constexpr const bool hasNetwork = false;
//
//		//bool getBool() const { return bValue; }
//
//	} rotate;
//	//static constexpr const bool rotate_network = false;
//
////private:
////	NodeAttrNetwork* nodeAttrNetwork_ = nullptr;
////public:
////	NodeAttrNetwork* getNodeAttrNetwork() const { return nodeAttrNetwork_; }
//
////private:
////	AttrNetwork translate_network;
//public:
//
//	static constexpr const size_t nAttributes = __COUNTER__ - firstAttributeIndex - 1 + nInheritedAttributes;
//
//	typedef AttrSetStorage<TransformNode::nAttributes> TransformNodeAttrSetType;
//
//private:
//	static TransformNodeAttrSetType sTransformNodeAttrSet;
//	//friend struct TransformNode_AttrSetFinisher;
//public:
//	static const AttrSet& getTransformNodeAttrSet() { return sTransformNodeAttrSet; }

	DECLARE_ATTRIBUTES_BEGIN_INHERIT(DagNode);

	//DECLARE_ATTRIBUTE_DISTANCE_NETWORKED( translate, 0, 0, 0 );

	struct Attribute_translate
	{
	private:
		float valueX = 0;
		float valueY = 0;
		float valueZ = 0;

	public:
		static const AttributeDesc descX;
		static const AttributeDesc descY;
		static const AttributeDesc descZ;
		static const AttributeDesc desc;
		static constexpr const AttributeType::Type typeX = AttributeType::Float;
		static constexpr const AttributeType::Type typeY = AttributeType::Float;
		static constexpr const AttributeType::Type typeZ = AttributeType::Float;
		static constexpr const AttributeType::Type type = AttributeType::Distance;
		static constexpr const size_t ordinalX = __COUNTER__ - __firstAttributeIndex - 1 + __nInheritedAttributes;
		static constexpr const size_t ordinalY = __COUNTER__ - __firstAttributeIndex - 1 + __nInheritedAttributes;
		static constexpr const size_t ordinalZ = __COUNTER__ - __firstAttributeIndex - 1 + __nInheritedAttributes;
		static constexpr const size_t ordinal = __COUNTER__ - __firstAttributeIndex - 1 + __nInheritedAttributes;
		static constexpr const unsigned int hasNetwork = true;

		void setX( float val )
		{
			valueX = val;
			TransformNode* dn = reinterpret_cast<TransformNode*>( reinterpret_cast<uint8_t*>( &valueX ) - ( offsetof( TransformNode, translate ) + offsetof( Attribute_translate, valueX ) ) );
			if ( hasNetwork )
				dn->_SetFloat_destinations( descX, val );
		}

		void setXYZ( float x, float y, float z )
		{
			valueX = x;
			valueY = y;
			valueZ = z;
			TransformNode* dn = reinterpret_cast<TransformNode*>( reinterpret_cast<uint8_t*>( &valueX ) - ( offsetof( TransformNode, translate ) + offsetof( Attribute_translate, valueX ) ) );
			if ( hasNetwork )
				dn->_SetVector3_destinations( descX, x, y, z );
		}

	} translate;


	DECLARE_ATTRIBUTE_DISTANCE_NETWORKED( rotate, 0, 0, 0 );

	DECLARE_ATTRIBUTES_END( TransformNode );

};

