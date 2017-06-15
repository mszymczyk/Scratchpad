#pragma once

#include "Attributes.h"
//#include "generated/DependNode.generated.h"

class DependencyNode
{
public:
	//DECLARE_FIRST_ATTRIBUTE_DEPENDENCY_NODE( visibility, BoolAttribute )
	//DECLARE_ATTRIBUTES_END( DependencyNode, visibility )
	//DependencyNode()
	//	: DependencyNode( NodeAttributesNS::DependencyNodeAttributes::getAttributeSet() )
	//{	}

	//DependencyNode( const AttributeSet& attrSet )
	//	: attrSet_( attrSet )
	//{	}

	//DependencyNode()
	//	: DependencyNode( DependencyNode::sDependencyNodeAttrSet )
	//{	}

	//DependencyNode( const AttrSet& attrSet )
	//	: attrSet_( attrSet )
	//{	}

	DependencyNode( const NodeTypeDesc& ntd )
		: typeDesc_( ntd )
	{	}

private:
	//const AttributeSet& attrSet_;
	//const AttrSet& attrSet_;
	const NodeTypeDesc& typeDesc_;
	NodeAttrNetwork* network_ = nullptr;

public:

	const AttrSet& getAttrSet() const { return typeDesc_.attrSet_; }

	//DECLARE_ATTRIBUTES_BEGIN( DependencyNode );
	//DECLARE_BOOL_ATTRIBUTE( visibility, pvis );
	//DECLARE_ATTRIBUTES_END;

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

	//static constexpr const size_t nAttributes = __COUNTER__ - firstAttributeIndex - 1;

	//template<class T>
	//bool getBool( const T& attr ) const
	//{
	//	return attr.bValue;
	//}

	//bool getVisibility() const
	//{
	//	//return getBool( visibility );
	//	//return getBool( sortingPriority );
	//	return visibility.getBool();
	//}

	//bool _GetBool( const AttributeDesc& adesc )
	//{
	//	assert( adesc.type_ == AttributeType::Bool );
	//	return *reinterpret_cast<const bool*>( reinterpret_cast<const uint8_t*>( this ) + adesc.offset_ );
	//}

	//uint8_t _GetU8( const AttributeDesc& adesc )
	//{
	//	assert( adesc.type_ == AttributeType::UInt8 );
	//	return *(reinterpret_cast<const uint8_t*>( this ) + adesc.offset_);
	//}

	//template<class T>
	//void setBool( T& attr, bool b )
	//{
	//	void* addr = &attr.bValue - attr.offset;

	//	if ( attr.hasNetwork )
	//		_SetBool_destinations( attr.desc, b );
	//}


	//template<class T>
	//void _SetVector3( T& attr, float x, float y, float z )
	//{
	//	attr.x = x;
	//	attr.y = y;
	//	attr.z = z;

	//	if ( attr.hasNetwork )
	//		_SetVector3_destinations( attr.desc, x, y, z );
	//}



	void _SetVector3( const AttributeDesc& adesc, float x, float y, float z )
	{
		assert( adesc.type_ == AttributeType::Distance );

		float* attrMem = reinterpret_cast<float*>( reinterpret_cast<uint8_t*>( this ) + adesc.offset_ );

		attrMem[0] = x;
		attrMem[1] = y;
		attrMem[2] = z;

		if ( adesc.hasNetwork_ )
			_SetVector3_destinations( adesc, x, y, z );
	}

//private:
	//template<class T>
	//void _SetVector3Network( T& attr, float x, float y, float z )
	//{
	//	AttrNetwork& an = getNetwork( attr );
	//	for ( AttributeDesc& ad : an.destinations_ )

	//}
	void _SetVector3_destinations( const AttributeDesc& adesc, float x, float y, float z )
	{
		AttrNetwork& an = getAttrNetwork( adesc );
		for ( Attr& a : an.destinations_ )
			a.setVector3( x, y, z );
	}

	void _SetBool_destinations( const AttributeDesc& adesc, bool b )
	{
		AttrNetwork& an = getAttrNetwork( adesc );
		for ( Attr& a : an.destinations_ )
			a.setBool( b );
	}

	void _SetFloat_destinations( const AttributeDesc& adesc, float f )
	{
		AttrNetwork& an = getAttrNetwork( adesc );
		for ( Attr& a : an.destinations_ )
			a.setFloat( f );
	}

	//template<class T>
	//AttrNetwork& getNetwork( const T& attr )
	//{
	//	static_assert( attr.hasNetwork, "m" );
	//	return *reinterpret_cast<AttrNetwork*>( reinterpret_cast<unsigned char*>( this ) + attr.desc.networkOffset_ );
	//}

	AttrNetwork& getAttrNetwork( const AttributeDesc& adesc )
	{
		//return *reinterpret_cast<AttrNetwork*>( reinterpret_cast<unsigned char*>( this ) + adesc.networkOffset_ );
		return network_->getAttrNetwork( adesc );
	}

	//static DependencyNodeAttributes::AttributeType_visibility visibility;

	//static const AttrSet& getAttrSet();
	//typedef AttrSetStorage<DependencyNode::nAttributes> DependencyNodeAttrSetType;

private:
	//static DependencyNodeAttrSetType sDependencyNodeAttrSet;
	//friend struct DependencyNode_AttrSetFinisher;
public:
	//static const AttrSet& getDependencyNodeAttrSet() { return sDependencyNodeAttrSet; }
};


inline bool Attr::getBool() const
{
	assert( desc_.type_ == AttributeType::Bool );
	return *reinterpret_cast<const bool*>( reinterpret_cast<const unsigned char*>( node_ ) + desc_.offset_ );
}

inline void Attr::setBool( bool b )
{
	assert( desc_.type_ == AttributeType::Bool );
	*reinterpret_cast<bool*>( reinterpret_cast<uint8_t*>( node_ ) + desc_.offset_ ) = b;
}

inline void Attr::setVector3( float x, float y, float z )
{
	//assert( desc_->type_ == AttributeType::Distance );
	//float* f = reinterpret_cast<float*>( reinterpret_cast<unsigned char*>( node_ ) + desc_->offset_ );
	//f[0] = x;
	//f[1] = y;
	//f[2] = z;
	node_->_SetVector3( desc_, x, y, z );
}
