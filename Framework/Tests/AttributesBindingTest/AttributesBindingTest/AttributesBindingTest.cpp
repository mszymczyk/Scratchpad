// HiStreamTest.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <iostream>
#include <initializer_list>
#include <array>
#include "TransformNode.h"

#pragma region test

//struct Attribute
//{
//	constexpr unsigned int memOffset = 0;
//};

//#define DECLARE_ATTRIBUTE( shortName, type )
//struct Attribute_longName	\
//{ \
//static const unsigned int memOffset = 0;
//}; \

//struct AttributeDesc
//{
//	const char* shortName;
//	size_t size;
//};

//template<size_t dataSize>
//struct AttrSet
//{
//	constexprt size_t DataSize = dataSize;
//
//	char data_[DataSize];
//};

//constexpr 
//
//constexpr AttrSet createAttrSet( std::initializer_list<AttributeDesc> ad )
//{
//	AttrSet as;
//	as.sizeInBytes = 0;
//
//
//
//	return as;
//};
//
//constexpr AttrSet aset = createAttrSet( { {"tx", 4}, {"ty", 4}, {"tz", 5} } );

//typename<dataSize>
//const AttrSet<T> aset

#pragma endregion

//struct dupa
//{
//	static void ass();
//};

//#define STRINGIFY(x) #x
//#define TOSTRING(x) STRINGIFY(x)

//template<int N>
//struct flag {
//	friend constexpr int adl_flag( flag<N> );
//};
//template<int N>
//struct writer {
//	friend constexpr int adl_flag( flag<N> ) {
//		return N;
//	}
//
//	static constexpr int value = N;
//};
//template<int N, int = adl_flag( flag<N> {} ) >
//int constexpr reader( int, flag<N> ) {
//	return N;
//}
//
//template<int N>
//int constexpr reader( float, flag<N>, int R = reader( 0, flag<N - 1> {} ) ) {
//	return R;
//}
//
//int constexpr reader( float, flag<0> ) {
//	return 0;
//}
//template<int N = 1>
//int constexpr next( int R = writer < reader( 0, flag<32> {} ) + N > ::value ) {
//	return R;
//}

//int main() {
//	constexpr int a = next();
//	constexpr int b = next();
//	constexpr int c = next();
//
//	static_assert ( a == 1 && b == a + 1 && c == b + 1, "try again" );
//}

//#define TOKEN_CONCAT_IMPL(x, y) x ## y
//#define TOKEN_CONCAT(x, y) TOKEN_CONCAT_IMPL(x, y)
////#define TOKEN_CONCAT_SUB(x, y) TOKEN_CONCAT(x, y-1)
////#define UNIQUE static void TOKEN_CONCAT(Unique_, __LINE__)(void) {}
//
//constexpr size_t alignOffset( size_t offset, size_t alignment )
//{
//	return ( ( offset + (alignment-1) ) & ~(alignment-1) );
//}
//
//namespace AttributeType
//{
//enum Type : uint8_t
//{
//	Bool,
//	Int64,
//	Int,
//	Int16,
//	Int8,
//	UInt64,
//	UInt,
//	UInt16,
//	UInt8,
//	Float,
//	Vec2,
//	Vec3,
//	Vec4,
//	Distance, // 3 floats
//	Count,
//};
//
////uint8_t TypeSize[Count] = {
////	1,
////	8, 4, 2, 1,
////	8, 4, 2, 1,
////	4,
////	8,
////	12,
////	16
////};
//
//uint8_t TypeAlign[Count] = {
//	1,
//	8, 4, 2, 1,
//	8, 4, 2, 1,
//	4,
//	16,
//	16,
//	16
//};
//
////constexpr size_t getSize( Type t ) { return TypeSize[t]; }
////constexpr size_t getSize( Type t )
////{
////	if ( t == Bool )
////		return 1;
////	else if ( t == Float )
////		return 4;
////	else
////		return 0;
////}
//
////constexpr size_t getAlign( Type t ) { return TypeSize[t]; }
//
////constexpr bool getSizeIsConstexpr = noexcept( getSize(AttributeType::Bool) );
//
//};
//
//struct BoolAttribute
//{
//	static const std::size_t size = 1;
//	static const std::size_t alignment = 1;
//	static const AttributeType::Type type = AttributeType::Bool;
//};
//
//struct FloatAttribute
//{
//	static const std::size_t size = 4;
//	static const std::size_t alignment = 4;
//	static const AttributeType::Type type = AttributeType::Float;
//};
//
//struct DistanceAttribute
//{
//	static const std::size_t size = 12;
//	static const std::size_t alignment = 4;
//	static const AttributeType::Type type = AttributeType::Distance;
//};
//
//struct AttributeBase
//{
//	AttributeBase( const char* _shortName, size_t _dataOffset, AttributeType::Type aType )
//		: shortName( _shortName )
//		, doffset( (unsigned int)_dataOffset )
//		, attrType_( aType )
//	{	}
//
//	const char* shortName = nullptr;
//	const unsigned int doffset = 0;
//	AttributeType::Type attrType_ = AttributeType::Count;
//};
//
//template<std::size_t dataOffset, class T, size_t attrNo>
//struct Attribute : public AttributeBase
//{
//	Attribute( const char* _shortName, size_t _dataOffset )
//		: AttributeBase( _shortName, _dataOffset, T::type )
//	{	}
//
//	static constexpr const size_t offset = dataOffset;
//	static constexpr const size_t size = T::size;
//	static constexpr const size_t offsetPlusSize = offset + size;
//	static constexpr const size_t attrNumber = attrNo;
//	static constexpr const AttributeType::Type aType = T::type;
//	//constexpr std::size_t offset() const { return ( dataOffset ); }
//};
//
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
//#define DECLARE_ATTRIBUTES_END(clas, prevAttributeName) \
//typedef AttributeType_##prevAttributeName AttributeType_lastAttribute_##clas; \
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
//
//
//#define DEFINE_ATTRIBUTE(nodeType, name, shortName1) \
//nodeType::AttributeType##name nodeType::name( shortName1, nodeType::name.offset );
//
////nodeType::AttributeType##name nodeType::name.doffset = nodeType::name.offset
//
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

void printAttrInfo( const char* name, const DependencyNode& nod )
{
	//const AttributeSet& aset = nod.getAttributeSet();
	//for ( const AttributeBase* a : aset )
	//{
	//	std::cout << name << ": name=" << a->shortName << ", offset=" << a->doffset << std::endl;
	//}

	const AttrSet& aset = nod.getAttrSet();
	for ( const AttributeDesc* a : aset )
	{
		std::cout << name << ": name=" << a->name_ << ", shortName=" << a->shortName_ << ", offset=" << a->offset_ << ", index=" << a->index_ << std::endl;
	}
}

//void setBoolAttribute( DependencyNode& dn, const AttributeDesc& ad

int main()
{
	//TransformNode::translateX::offset;
	//std::cout << "visibility::offset " << TransformNode::visibility.offset << std::endl;
	//std::cout << "sortingPriority::offset " << TransformNode::sortingPriority.offset << std::endl;
	//std::cout << "translateX::offset " << TransformNode::translateX.offset << std::endl;
	//std::cout << "translateY::offset " << TransformNode::translateY.offset << std::endl;
	//std::cout << "translateZ::offset " << TransformNode::translateZ.offset << std::endl;
	//std::cout << "translate::offset " << TransformNode::translate.offset << std::endl;

	//printAttrInfo( "visibility",      TransformNode::visibility     .offset, TransformNode::visibility     .size, TransformNode::visibility     .attrNumber );
	//printAttrInfo( "sortingPriority", TransformNode::sortingPriority.offset, TransformNode::sortingPriority.size, TransformNode::sortingPriority.attrNumber );
	//printAttrInfo( "translateX",      TransformNode::translateX     .offset, TransformNode::translateX     .size, TransformNode::translateX     .attrNumber );
	//printAttrInfo( "translateY",      TransformNode::translateY     .offset, TransformNode::translateY     .size, TransformNode::translateY     .attrNumber );
	//printAttrInfo( "translateZ",      TransformNode::translateZ     .offset, TransformNode::translateZ     .size, TransformNode::translateZ     .attrNumber );
	//printAttrInfo( "translate",       TransformNode::translate      .offset, TransformNode::translate      .size, TransformNode::translate      .attrNumber );

	//const size_t* p = &TransformNode::translateX.offset;
	//printOffset( "translateXName::offset ", TransformNode::translateX.offset );
	//printOffset( "translateY::offset ", TransformNode::translateY.offset );
	//printOffset( "translateY::offset ", 32 );
	//printOffset( "translateY::offset ", offs );
	//printOffset( "translateY::offset ", Fake::offs );

	//dupa::ass();

	//std::array<int, 10> a;
	//a::max_size

	//DagNode::getDagNodeAttrSet().finishAttrSetInitialization();

	DagNode dn;
	printAttrInfo( "dagNode", dn );

	//dn.visibility.valueBool;
	//dn.setBool( dn.visibility, true );
	dn.visibility.setBool( true );

	Attr a( &dn, dn.visibility.desc );
	a.setBool( false );

	//bool b = dn.getVisibility();

	//static_assert( sizeof( DependencyNode ) == 2, "incorrect size" );

	////const char* attrName = DependencyNode::AttributeType_visibility::desc.name;
	//const char* attrName = dn.visibility.desc.name;

	TransformNode tn;
	printAttrInfo( "transform", tn );

	return 0;
}

//AttributeSet::AttributeSet( AttributeBase* attributesStorage[]
//	, const AttributeBase* baseAttributes[], size_t nBaseAttributes
//	, const AttributeBase* attributes[], size_t nAttributes
//)
//	: attributes_( attributesStorage )
//	, nAttributes_( nBaseAttributes + nAttributes )
//{
//	for ( size_t i = 0; i < nBaseAttributes; ++i )
//		attributes_[i] = baseAttributes[i];
//
//	for ( size_t i = 0; i < nAttributes; ++i )
//		attributes_[i + nBaseAttributes] = attributes[i];
//}
