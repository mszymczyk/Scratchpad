#pragma once

#include <stdint.h>
#include <initializer_list>
#include <assert.h>
#include <list>
#include <algorithm>

#define TOKEN_CONCAT_IMPL(x, y) x ## y
#define TOKEN_CONCAT(x, y) TOKEN_CONCAT_IMPL(x, y)
//#define TOKEN_CONCAT_SUB(x, y) TOKEN_CONCAT(x, y-1)
//#define UNIQUE static void TOKEN_CONCAT(Unique_, __LINE__)(void) {}

constexpr size_t alignOffset( size_t offset, size_t alignment )
{
	return ( ( offset + (alignment-1) ) & ~(alignment-1) );
}

namespace AttributeType
{
enum Type : uint8_t
{
	Bool,
	Int8,
	Int16,
	Int,
	Int64,
	UInt8,
	UInt16,
	UInt,
	UInt64,
	Float,
	Vec2,
	Vec3,
	Vec4,
	Distance, // 3 floats
	Color, // 3 floats
	Count,
};

//uint8_t TypeSize[Count] = {
//	1,
//	8, 4, 2, 1,
//	8, 4, 2, 1,
//	4,
//	8,
//	12,
//	16
//};

//uint8_t TypeAlign[Count] = {
//	1,
//	8, 4, 2, 1,
//	8, 4, 2, 1,
//	4,
//	16,
//	16,
//	16
//};

//constexpr size_t getSize( Type t ) { return TypeSize[t]; }
//constexpr size_t getSize( Type t )
//{
//	if ( t == Bool )
//		return 1;
//	else if ( t == Float )
//		return 4;
//	else
//		return 0;
//}

//constexpr size_t getAlign( Type t ) { return TypeSize[t]; }

//constexpr bool getSizeIsConstexpr = noexcept( getSize(AttributeType::Bool) );

};

struct BoolAttribute
{
	static const size_t size = 1;
	static const size_t alignment = 1;
	static const AttributeType::Type type = AttributeType::Bool;
};

struct FloatAttribute
{
	static const size_t size = 4;
	static const size_t alignment = 4;
	static const AttributeType::Type type = AttributeType::Float;
};

struct DistanceAttribute
{
	static const size_t size = 12;
	static const size_t alignment = 4;
	static const AttributeType::Type type = AttributeType::Distance;
};

struct AttributeBase
{
	AttributeBase( const char* _shortName, size_t _dataOffset, AttributeType::Type aType )
		: shortName( _shortName )
		, doffset( (unsigned int)_dataOffset )
		, attrType_( aType )
	{	}

	const char* shortName = nullptr;
	const unsigned int doffset = 0;
	AttributeType::Type attrType_ = AttributeType::Count;
};

template<size_t dataOffset, class T, size_t attrNo>
struct Attribute : public AttributeBase
{
	Attribute( const char* _shortName, size_t _dataOffset )
		: AttributeBase( _shortName, _dataOffset, T::type )
	{	}

	static constexpr const size_t offset = dataOffset;
	static constexpr const size_t size = T::size;
	static constexpr const size_t offsetPlusSize = offset + size;
	static constexpr const size_t attrNumber = attrNo;
	static constexpr const AttributeType::Type aType = T::type;
	//constexpr std::size_t offset() const { return ( dataOffset ); }
};

struct AttrSet;

struct AttributeDesc
{
	AttributeDesc()
	{	}

	AttributeDesc( AttrSet* aset, size_t descNo, const char* name, const char* shortName, AttributeType::Type type, size_t offset, bool hasNetwork );

	const char* name_ = nullptr;
	const char* shortName_ = nullptr;
	uint64_t shortNamePacked_ = 0;
	unsigned int index_ = 0;
	unsigned int offset_ = 0;
	//unsigned int networkOffset_ = 0;
	unsigned int attrNetworkIndex_ = 0;
	AttributeType::Type type_ = AttributeType::Count;
	bool hasNetwork_ = false;

	static uint64_t packAttributeShorName( const char* shortName );

};


class DependencyNode;

struct Attr
{
	Attr( DependencyNode* node, const AttributeDesc& adesc )
		: node_( node )
		, desc_( adesc )
	{	}

	DependencyNode* node_ = nullptr;
	const AttributeDesc& desc_;

	bool getBool() const;

	void setBool( bool b );
	void setFloat( float f );
	void setVector3( float x, float y, float z );
};

struct AttrNetwork
{
	Attr src_;
	std::list<Attr> destinations_;
};

struct NodeAttrNetwork
{
	AttrNetwork& getAttrNetwork( const AttributeDesc& adesc )
	{
		assert( adesc.hasNetwork_ );
		return attrNetwork_[adesc.attrNetworkIndex_];
	}

	AttrNetwork* attrNetwork_;
};

struct AttributeDescComparer
{
	bool operator() ( const AttributeDesc* lhs, const AttributeDesc* rhs )
	{
		//return strcmp( lhs->name_, rhs->name_ ) < 0;
		return lhs->shortNamePacked_ < rhs->shortNamePacked_;
	}
};

struct AttrSet
{
	//AttrSet()
	//{ }

	AttrSet( const AttributeDesc** storage, const AttributeDesc** storageSorted, size_t nAttributes )
		: attributesDesc_( storage )
		, attributesDescSorted_( storageSorted )
		, nAttributes_( nAttributes )
	{	}

	//void addAttr( const AttributeDesc* attr )
	//{
	//	attributesDesc_[nAddedAttributes_++] = attr;
	//}

	const AttributeDesc** begin() const { return attributesDesc_; }
	const AttributeDesc** end() const { return attributesDesc_ + nAttributes_; }

	const AttributeDesc** beginSorted() const { return attributesDescSorted_; }
	const AttributeDesc** endSorted() const { return attributesDescSorted_ + nAttributes_; }

	void set( const AttributeDesc* ad, size_t index )
	{
		assert( index < nAttributes_ );
		assert( attributesDesc_[index] == nullptr );
		attributesDesc_[index] = ad;
		attributesDescSorted_[index] = ad;
	}

	void finishAttrSetInitialization()
	{
		std::sort( beginSorted(), endSorted(), AttributeDescComparer() );
	}

	const AttributeDesc** attributesDesc_ = nullptr;
	const AttributeDesc** attributesDescSorted_ = nullptr;
	const size_t nAttributes_ = 0;
	//size_t nAddedAttributes_ = 0;
	//size_t networkOffset_ = 0; // for calculating network offsets
	//size_t nodeNetworkOffset_ = 0; // offset within node where information about node connections is stored
	unsigned int attrNetworkIndex_ = 0;
};


inline AttributeDesc::AttributeDesc( AttrSet* aset, size_t descNo, const char* name, const char* shortName, AttributeType::Type type, size_t offset, bool hasNetwork )
	: name_( name )
	, shortName_( shortName )
	, shortNamePacked_( packAttributeShorName(shortName) )
	, index_( (unsigned int)descNo )
	, offset_( (unsigned int)offset )
	//, networkOffset_( hasNetwork ? (unsigned int)aset->networkOffset_ : 0 )
	, attrNetworkIndex_( hasNetwork ? aset->attrNetworkIndex_ : 0xffffffff )
	, type_( type )
	, hasNetwork_( hasNetwork )
{
	aset->set( this, descNo );
	if ( hasNetwork )
		//aset->networkOffset_ += sizeof( AttrNetwork );
		++ aset->attrNetworkIndex_;
}

//struct AttrSetFinisher
//{
//	AttrSetFinisher( AttrSet& aset )
//	{
//		std::sort( aset.beginSorted(), aset.endSorted(), AttributeDescComparer() );
//	}
//};


template<size_t N_attr>
struct AttrSetStorage : public AttrSet
{
	//AttrSet( const AttrSet* inheritFrom, std::initializer_list<AttributeDesc*> attributes )
	//{
	//}

	AttrSetStorage( const AttrSet* inherited/*, std::initializer_list<AttributeDesc> descriptors*/ )
		: AttrSet( attributeDescStore_, attributeDescStoreSorted_, N_attr )
		//, attributeDescStore_( descriptors )
	{
		//size_t offs = 0;
		if ( inherited )
		{
			for ( size_t i = 0; i < inherited->nAttributes_; ++i )
				attributeDescStore_[i] = attributesDescSorted_[i] = inherited->attributesDesc_[i];

			//offs = inherited->nAttributes_;
		}

		//for ( size_t i = 0; i < descriptors.size(); ++i )
		//	attributeDescStore_[i + offs] = descriptors.begin()[i];

		//assert( N_attr == offs + descriptors.size() );
	}

	const AttributeDesc* attributeDescStore_[N_attr];
	const AttributeDesc* attributeDescStoreSorted_[N_attr];
};

//struct AttributeSet
//{
//	//AttributeSet( AttributeSet && ) = delete;
//	//AttributeSet( const AttributeSet & ) = delete;
//
//	//AttributeSet( const AttributeBase* attributes[], size_t nAttributes )
//	//	: attributes_( attributes )
//	//	, nAttributes_( nAttributes )
//	//{	}
//
//	AttributeSet( AttributeBase* attributesStorage[], const AttributeBase* baseAttributes[], size_t nBaseAttributes, const AttributeBase* attributes[], size_t nAttributes );
//
//	const AttributeBase* const* begin() const { return attributes_; }
//	const AttributeBase* const* end() const { return attributes_ + nAttributes_; }
//
//	const AttributeBase** attributes_;
//	const size_t nAttributes_;
//};

//template<size_t nAttributes>
//struct AttributeSet : public AttributeSetBase
//{
//	AttributeSet( std::initializer_list<AttributeBase*> attributes )
//		: AttributeSetBase( attributes_, nAttributes
//	{
//		assert( attributes.size() == nAttributes );
//
//		size_t i = 0;
//		for ( AttributeBase* ab : attributes )
//		{
//			attributes_[i] = ab;
//			++i;
//		}
//	}
//
//	AttributeBase* attributes_[nAttributes];
//};

//#define DECLARE_FIRST_ATTRIBUTE_DEPENDENCY_NODE(name, atype) \
//typedef Attribute<0, atype, 0> AttributeType_##name; \
//static AttributeType_##name name;
//
//
//#define DECLARE_FIRST_ATTRIBUTE(name, atype) \
//typedef Attribute<alignOffset(AttributeType_baseLastAttribute::offsetPlusSize, atype::alignment), atype, AttributeType_baseLastAttribute::attrNumber+1> AttributeType_##name; \
//static AttributeType_##name name; \
//typedef AttributeType_##name AttributeType_lastAttribute;
//
//
////#define DECLARE_FIRST_ATTRIBUTE(name, atype) \
////static Attribute<0, atype, next()> name;
//
////#define DECLARE_FIRST_ATTRIBUTE(name, atype) \
////typedef Attribute<0, atype> TOKEN_CONCAT(AttributeType, __COUNTER__); \
////static TOKEN_CONCAT( AttributeType, __COUNTER__-1) name;
//
//
////#define DECLARE_ATTRIBUTE(name, size1, prevAttributeName) \
////typedef Attribute<AttributeType##prevAttributeName::offset + AttributeType##prevAttributeName::size, size1> AttributeType##name; \
////static AttributeType##name name;
//
////#define DECLARE_ATTRIBUTE(name, atype, prevAttributeName) \
////typedef Attribute<alignOffset(AttributeType##prevAttributeName::offsetPlusSize, AttributeType::getAlign(atype)), atype> AttributeType##name; \
////static AttributeType##name name;
//
//#define DECLARE_ATTRIBUTE(name, atype, prevAttributeName) \
//typedef Attribute<alignOffset(AttributeType_##prevAttributeName::offsetPlusSize, atype::alignment), atype> AttributeType_##name; \
//static AttributeType_##name name;
//
////#define DECLARE_ATTRIBUTE(name, atype, prevAttributeName) \
////typedef Attribute<alignOffset(Attribute<::offsetPlusSize, atype::alignment), atype> AttributeType##name; \
////static AttributeType##name name;
//
////#define DECLARE_ATTRIBUTES_END(prevAttributeName) \
////static constexpr size_t const attributes_size = AttributeType_##prevAttributeName::offsetPlusSize; \
//
////#define DECLARE_ATTRIBUTES_END(clas, prevAttributeName) \
////typedef AttributeType_##prevAttributeName AttributeType_lastAttribute_##clas; \
//
////#define DECLARE_INHERIT_ATTRIBUTES_FROM(clas) \
////static constexpr const size_t base_attributes_size = clas::attributes_size;
//#define DECLARE_INHERIT_ATTRIBUTES_FROM(clas) \
//typedef AttributeType_lastAttribute_##clas AttributeType_baseLastAttribute;
//
//
//#define DECLARE_ATTRIBUTE_DISTANCE(name, prevAttributeName) \
//typedef Attribute<alignOffset(AttributeType_##prevAttributeName::offsetPlusSize, FloatAttribute::alignment), FloatAttribute, AttributeType_##prevAttributeName::attrNumber+1> AttributeType_##name##X; \
//static AttributeType_##name##X name##X; \
//typedef Attribute<alignOffset( AttributeType_##name##X::offsetPlusSize, FloatAttribute::alignment ), FloatAttribute, AttributeType_##name##X::attrNumber+1> AttributeType_##name##Y; \
//static AttributeType_##name##Y name##Y; \
//typedef Attribute<alignOffset( AttributeType_##name##Y::offsetPlusSize, FloatAttribute::alignment ), FloatAttribute, AttributeType_##name##Y::attrNumber+1> AttributeType_##name##Z; \
//static AttributeType_##name##Z name##Z; \
//typedef Attribute<alignOffset( AttributeType_##name##X::offset, DistanceAttribute::alignment ), DistanceAttribute, AttributeType_##name##Z::attrNumber+1> AttributeType_##name; \
//static AttributeType_##name name; \
//typedef AttributeType_##name AttributeType_lastAttribute;


//#define DEFINE_ATTRIBUTE(nodeType, name, shortName1) \
//nodeType::AttributeType##name nodeType::name( shortName1, nodeType::name.offset );

//nodeType::AttributeType##name nodeType::name.doffset = nodeType::name.offset

//class DependencyNode
//{
//public:
//	DECLARE_FIRST_ATTRIBUTE_DEPENDENCY_NODE( visibility, BoolAttribute )
//	DECLARE_ATTRIBUTES_END( DependencyNode, visibility )
//};
//
//class TransformNode : public DependencyNode
//{
//public:
//
//	DECLARE_INHERIT_ATTRIBUTES_FROM( DependencyNode );
//	DECLARE_FIRST_ATTRIBUTE( sortingPriority, BoolAttribute )
//	DECLARE_ATTRIBUTE_DISTANCE( translate, sortingPriority )
//
//	//typedef Attribute<0, 4> AttributeType;
//	//static AttributeType translateX;
//	//typedef Attribute<0> translateX;
//
//};
//
////DEFINE_ATTRIBUTE( TransformNode, translateX, "tx" )
////TransformNode::AttributeType TransformNode::translateX( "tx", TransformNode::translateX.offset );
//
//void printOffset( const char* name, size_t offset )
//{
//	std::cout << name << offset << std::endl;
//}
//
//static constexpr const size_t offs = 48;
//
//struct Fake
//{
//	static constexpr const size_t offs = 64;
//};
//
//void printAttrInfo( const char* name, size_t offset, size_t size, size_t attrNo )
//{
//	std::cout << name << ": offset=" << offset << ", size=" << size << " attrNo=" << attrNo << std::endl;
//}
//
//int main()
//{
//	//TransformNode tn;
//	//TransformNode::translateX::offset;
//	//std::cout << "visibility::offset " << TransformNode::visibility.offset << std::endl;
//	//std::cout << "sortingPriority::offset " << TransformNode::sortingPriority.offset << std::endl;
//	//std::cout << "translateX::offset " << TransformNode::translateX.offset << std::endl;
//	//std::cout << "translateY::offset " << TransformNode::translateY.offset << std::endl;
//	//std::cout << "translateZ::offset " << TransformNode::translateZ.offset << std::endl;
//	//std::cout << "translate::offset " << TransformNode::translate.offset << std::endl;
//
//	printAttrInfo( "visibility",      TransformNode::visibility     .offset, TransformNode::visibility     .size, TransformNode::visibility     .attrNumber );
//	printAttrInfo( "sortingPriority", TransformNode::sortingPriority.offset, TransformNode::sortingPriority.size, TransformNode::sortingPriority.attrNumber );
//	printAttrInfo( "translateX",      TransformNode::translateX     .offset, TransformNode::translateX     .size, TransformNode::translateX     .attrNumber );
//	printAttrInfo( "translateY",      TransformNode::translateY     .offset, TransformNode::translateY     .size, TransformNode::translateY     .attrNumber );
//	printAttrInfo( "translateZ",      TransformNode::translateZ     .offset, TransformNode::translateZ     .size, TransformNode::translateZ     .attrNumber );
//	printAttrInfo( "translate",       TransformNode::translate      .offset, TransformNode::translate      .size, TransformNode::translate      .attrNumber );
//
//	//const size_t* p = &TransformNode::translateX.offset;
//	//printOffset( "translateXName::offset ", TransformNode::translateX.offset );
//	//printOffset( "translateY::offset ", TransformNode::translateY.offset );
//	//printOffset( "translateY::offset ", 32 );
//	//printOffset( "translateY::offset ", offs );
//	//printOffset( "translateY::offset ", Fake::offs );
//
//	//dupa::ass();
//
//	//std::array<int, 10> a;
//	//a::max_size
//
//	return 0;
//}

//#define DECLARE_ATTRIBUTES_BEGIN(clas)
//
//#define DECLARE_ATTRIBUTES_END
//
//#define DECLARE_BOOL_ATTRIBUTE(name, shortName) \
//static NodeAttributesNS::AttributeType_##name name;
//
//
//#define DECLARE_ATTRIBUTES_BEGIN_INHERIT(clas, inheritedFrom)
//
//#define DECLARE_ATTRIBUTE_DISTANCE2(name, shortName) \
//static NodeAttributesNS::AttributeType_##name##X name##X; \
//static NodeAttributesNS::AttributeType_##name##Y name##Y; \
//static NodeAttributesNS::AttributeType_##name##Z name##Z; \
//static NodeAttributesNS::AttributeType_##name name;

//#define NETWORK_DECLARE(cond, offs, 

