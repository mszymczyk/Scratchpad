#pragma once

#include "Bits.h"
#include <assert.h>
#include <array>

#define HISTREAN_COUNT_WASTED_MEMORY 1
//#define HISTREAM_UNALIGNED_DATA 1

namespace spad
{

namespace stream
{

inline size_t alignPowerOfTwo( size_t val, size_t alignment )
{
	alignment--;
	return ( ( val + alignment ) & ~alignment );
}

inline u8* alignPowerOfTwo( u8* ptr, size_t alignment )
{
	alignment--;
	return reinterpret_cast<u8*>( ( (size_t)ptr + alignment ) & ~alignment );
}

inline const u8* alignPowerOfTwo( const u8* ptr, size_t alignment )
{
	alignment--;
	return reinterpret_cast<const u8*>( ( (size_t)ptr + alignment ) & ~alignment );
}

//template<typename T>
//inline const T* alignAndCast( const u8* ptr )
//{
//	//SPAD_ASSERT2( ( alignment & ( alignment - 1 ) ) == 0, "alignment must be multiple of 2" );
//	size_t alignment = alignof( T );
//	alignment--;
//	return reinterpret_cast<const u8*>( ( (size_t)ptr + alignment ) & ~alignment );
//}


typedef u32 NodeOffsetType;
typedef u16 AttributeOffsetType;

namespace AttributeType
{
enum Type : unsigned char
{
	invalid,

	Integer,
	Float,
	String,
	matrix4x4,
	
	int2,
	int3,
	int4,

	float2,
	float3,
	float4,

	stringInt, // string followed by float
	stringInt2,
	stringInt3,
	stringInt4,

	stringFloat, // string followed by float
	stringFloat2,
	stringFloat3,
	stringFloat4,

	stringDelimString,

	customData,

	count
};
} // namespace AttributeType


//class OutputStream;
//class HiStreamNode;




class StreamBase
{
protected:

	struct AttributeImpl
	{
		AttributeOffsetType offsetToNextAttribute_ = 0;
		AttributeType::Type attrType_ = AttributeType::invalid;
		u8 tag_ = 0; // user value
	};

	struct NodeImpl
	{
		//u32 offsetInBuf_ = 0;
		NodeOffsetType offsetToNextSibling_ = 0;
		NodeOffsetType offsetToFirstChild_ = 0;
		u32 nChildren_ = 0;
		u16 nAttributes_ = 0;
		u8 nodeTag_ = 0; // user value
		u8 _padding_ = 0;
	};

	friend class Node;
	friend class Attribute;
	friend class NodeIterator;
	friend class AttributeIterator;
};



template<bool EnableAligning = false>
class OutputStream : public StreamBase
{
public:
	OutputStream()
		//: root_( nullptr, nullptr, nullptr )
	{
		memset( nodeStack_, 0, sizeof( nodeStack_ ) );
		memset( prevSiblingStack_, 0, sizeof( prevSiblingStack_ ) );
	}

	~OutputStream()
	{
		spadFreeAligned( buf_ );
	}

	void begin()
	{
		rootImpl_ = _AddNode( 0xff );
		_PushStack( rootImpl_ );
	}
	
	void end()
	{
		_PopStack();
		assert( stackCount_ == 0 );
	}

	const u8* getBuffer() const { return buf_; }
	size_t getBufferSize() const { return bufUsedSize_; }

	void pushChild( u8 tag );
	void popChild();

	void addFloat( u8 tag, float f );
	void addStringFloat( u8 tag, const char* str, size_t strLen, float f );
	void addStringFloat( u8 tag, const char* str, float f ) { addStringFloat(tag, str, strlen(str), f ); }

	void addStringDelimString( u8 tag, const char* strL, size_t strLLen, char delim, const char* strR, size_t strRLen );
	void addStringDelimString( u8 tag, const char* strL, char delim, const char* strR ) { addStringDelimString(tag, strL, strlen(strL), delim, strR, strlen(strR) ); }

private:

	u8* _AllocateMemImpl( size_t nBytes, size_t alignment );
	u8* _AllocateMem( size_t nBytes, size_t alignment ) { return _AllocateMemImpl( nBytes, EnableAligning ? alignment : 1 ); }
	template<typename T>
	T* _AllocateMem( size_t num = 1 ) {	return reinterpret_cast<T*>( _AllocateMemImpl( sizeof( T ) * num, EnableAligning ? alignof(T) : 1 ) ); }

	template<typename T>
	size_t _CountAllocReq( size_t curSiz, size_t num = 1 )
	{
		size_t alignedCurSize = EnableAligning ? spadAlignU64_2( curSiz, alignof( T ) ) : curSiz;
#if HISTREAN_COUNT_WASTED_MEMORY
		paddingWastedSize_ += alignedCurSize - curSiz;
#endif //
		return alignedCurSize + sizeof(T) * num;
	}

	//NodeImpl* _AllocateNode() {	return _AllocateMem<NodeImpl>(); }
	NodeImpl* _AllocateNode() { return reinterpret_cast<NodeImpl*>( _AllocateMemImpl( sizeof(NodeImpl), alignof(NodeImpl) ) ); }
	NodeImpl* _AddNode( u8 tag );

	AttributeImpl* _AllocateAttr()
	{
		// attributes must be added before any children
		assert( curNode_->nChildren_ == 0 );
		return reinterpret_cast<AttributeImpl*>( _AllocateMemImpl( sizeof(AttributeImpl), alignof(AttributeImpl) ) );
	}
	AttributeImpl* _AddAttr( u8 tag, AttributeType::Type typ );

	//u8* _SetFloat( u8* ptr, float f );
	template<typename T>
	u8* _SetNumber( u8* ptr, T t )
	{
		u8* data = EnableAligning ? alignPowerOfTwo( ptr, alignof( T ) ) : ptr;
		SPAD_ASSERT( data + sizeof(T) <= buf_ + bufUsedSize_ );
		if ( EnableAligning )
			*reinterpret_cast<T*>( data ) = t;
		else
			memcpy( data, &t, sizeof( T ) );
		return data + sizeof(T);
	}
	u8* _SetString( u8* ptr, const char* str, size_t strLen )
	{
		u8* data = EnableAligning ? alignPowerOfTwo( ptr, alignof( u32 ) ) : ptr;
		SPAD_ASSERT( data + 4 + strLen + 1 <= buf_ + bufUsedSize_ );
		if ( EnableAligning )
			*reinterpret_cast<u32*>( data ) = (u32)strLen;
		else
		{
			u32 sl = (u32)strLen;
			memcpy( data, &sl, sizeof( u32 ) );
		}
		data += 4;
		memcpy( data, str, strLen + 1 );
		return data + strLen + 1;
	}

	void _PushStack( NodeImpl* n )
	{
		assert( stackCount_ <= eStackDepth );
		nodeStack_[stackCount_] = n;
		if ( curNode_ && curNode_->nChildren_== 0 )
			prevSiblingStack_[stackCount_] = nullptr;
		curNode_ = n;
		++ stackCount_;
	}

	void _PopStack()
	{
		assert( stackCount_ > 0 );
		--stackCount_;
		nodeStack_[stackCount_] = nullptr;
		if (stackCount_ > 0)
			curNode_ = nodeStack_[stackCount_ - 1];
		else
			curNode_ = nullptr;
	}

	NodeOffsetType _GetNodeOffset( NodeImpl* n, NodeImpl* prevN ) const
	{
		size_t o = (size_t)n - (size_t)prevN;
		assert( o < std::numeric_limits<NodeOffsetType>::max() );
		return (NodeOffsetType)( o );
	}

	AttributeOffsetType _GetAttrOffset( AttributeImpl* a, AttributeImpl* prevA ) const
	{
		size_t o = (size_t)a - (size_t)prevA;
		assert( o < std::numeric_limits<AttributeOffsetType>::max() );
		return (AttributeOffsetType)(o);
	}

private:
	u8* buf_ = nullptr;
	size_t bufUsedSize_ = 0;
	size_t bufCapacity_ = 0;
#if HISTREAN_COUNT_WASTED_MEMORY
	size_t paddingWastedSize_ = 0;
#endif //
	NodeImpl* rootImpl_ = nullptr;
	//void* memForHiStreamNode_[2];

	enum { eStackDepth = 64 };
	NodeImpl* nodeStack_[eStackDepth];
	NodeImpl* prevSiblingStack_[eStackDepth];
	u32 stackCount_ = 0;

	NodeImpl* curNode_ = nullptr;
	AttributeImpl* curAttribute_ = nullptr;
};


class Node;


class InputStream : public StreamBase
{
public:
	InputStream( const u8* buf, size_t bufSize );

	Node getRoot() const;

private:
	const u8* buf_ = nullptr;
	const size_t bufSize_ = 0;
	const NodeImpl* rootNode_ = nullptr;
};




// Range-based for loop support
template <typename It>
class ObjectRange
{
public:
	typedef It const_iterator;
	typedef It iterator;

	ObjectRange( It b, It e ) : _begin( b ), _end( e )
	{
	}

	It begin() const {
		return _begin;
	}
	It end() const {
		return _end;
	}

private:
	It _begin, _end;
};




class Node
{
public:
	//HiStreamNode pushChild( u8 tag );

	//void add( u8 tag, float f );
	//void add( u8 tag, const char* str, float f );

	// Child nodes iterators
	typedef NodeIterator children_iterator;

	// Attribute iterators
	typedef AttributeIterator attribute_iterator;

	children_iterator childrenBegin() const;
	children_iterator childrenEnd() const;

	u32 numChildren() const { return node_->nChildren_; }
	ObjectRange<NodeIterator> children() const;

	attribute_iterator attributesBegin() const;
	attribute_iterator attributesEnd() const;

	u32 numAttributes() const { return node_->nAttributes_; }
	ObjectRange<AttributeIterator> attributes() const;

	u8 tag() const { return node_->nodeTag_; }

private:
	//HiStreamNode( HiStreamNode* parent, NodeImpl* impl, OutputStream* stream )
	//	: parent_( parent )
	//	, impl_( impl )
	//	, stream_( stream )
	//{	}
	//HiStreamNode() { }
	//HiStreamNode( OutputStream* stream, u32 nodeImplOffset, u32 parentNodeImplOffset )
	//	: stream_( stream )
	//	, nodeImplOffset_( nodeImplOffset )
	//	, parentNodeImplOffset_( parentNodeImplOffset )
	//{	}

	//u32 _GetCurrentParentOffset

	Node() : node_( nullptr ) {}
	Node( const StreamBase::NodeImpl* node ) : node_( node ) {}

private:
	//HiStreamNode* parent_ = nullptr;
	//NodeImpl* impl_ = nullptr;
	//OutputStream* stream_ = nullptr;
	//u32 nodeImplOffset_ = 0;
	//u32 parentNodeImplOffset_ = 0;
	//u32 currentParentOffset_ = 0;

	const StreamBase::NodeImpl* node_ = nullptr;

	friend class InputStream;
	friend class NodeIterator;
	friend class AttributeIterator;
};




class Attribute
{
public:

	AttributeType::Type type() const { return attr_->attrType_; }
	u8 tag() const { return attr_->tag_; }

	void getFloat( float& f ) const;
	void getStringFloat( const char*& str, size_t& strLen, float& f ) const;
	void getStringDelimString( const char*& strL, size_t& strLLen, char& delim, const char* strR, size_t strRLen ) const;
	void getCustomData( const void*& data, u32& dataSize ) const;


private:
	Attribute() : attr_( nullptr ) {}
	Attribute( const StreamBase::AttributeImpl* attr ) : attr_( attr ) {}

	const u8* _GetDataAddress() const { return reinterpret_cast<const u8*>( attr_ + 1 ); }
	const u8* _GetString( const u8* ptr, const char*& str, size_t& strLen ) const;
	const u8* _GetFloat( const u8* ptr, float& f ) const;
	const u8* _GetU32( const u8* ptr, u32& u ) const;

private:
	const StreamBase::AttributeImpl* attr_ = nullptr;

	//friend class InputStream;
	friend class Node;
	friend class AttributeIterator;
};




class NodeIterator
{
	friend class Node;

private:
	Node node_;
	u32 childIndex_ = 0;

	NodeIterator( const Node& node, u32 childIndex )
		: node_( node )
		, childIndex_( childIndex )
	{ }

public:
	// Iterator traits
	typedef ptrdiff_t difference_type;
	typedef Node value_type;
	typedef Node* pointer;
	typedef Node& reference;

	//// Default constructor
	//NodeIterator();

	//// Construct an iterator which points to the specified node
	//NodeIterator( const xml_node& node );

	// Iterator operators
	bool operator==( const NodeIterator& rhs ) const
	{
		return /*node_.node_ == rhs.node_.node_ &&*/ childIndex_ == rhs.childIndex_;
	}
	bool operator!=( const NodeIterator& rhs ) const
	{
		return /*node_.node_ != rhs.node_.node_ ||*/ childIndex_ != rhs.childIndex_;
	}

	const Node& operator*() const
	{
		return node_;
	}
	//xml_node* operator->() const;

	const NodeIterator& operator++()
	{
		const u8* base = reinterpret_cast<const u8*>(node_.node_);
		node_.node_ = reinterpret_cast<const StreamBase::NodeImpl*>(base + node_.node_->offsetToNextSibling_);
		++ childIndex_;
		return *this;
	}
	//xml_node_iterator operator++( int );

	//const xml_node_iterator& operator--();
	//xml_node_iterator operator--( int );
};




class AttributeIterator
{
	friend class Attribute;
	friend class Node;

private:
	Attribute attr_;
	u32 attrIndex_ = 0;

	AttributeIterator( const Attribute& attr, u32 attrIndex )
		: attr_( attr )
		, attrIndex_( attrIndex )
	{ }

public:
	//// Iterator traits
	//typedef ptrdiff_t difference_type;
	//typedef Attribute value_type;
	//typedef Attribute* pointer;
	//typedef Attribute& reference;

	// Iterator operators
	bool operator==( const AttributeIterator& rhs ) const
	{
		return /*node_.node_ == rhs.node_.node_ &&*/ attrIndex_ == rhs.attrIndex_;
	}
	bool operator!=( const AttributeIterator& rhs ) const
	{
		return /*node_.node_ != rhs.node_.node_ ||*/ attrIndex_ != rhs.attrIndex_;
	}

	const Attribute& operator*() const
	{
		return attr_;
	}
	//xml_node* operator->() const;

	const AttributeIterator& operator++()
	{
		const u8* base = reinterpret_cast<const u8*>( attr_.attr_ );
		attr_.attr_ = reinterpret_cast<const StreamBase::AttributeImpl*>( base + attr_.attr_->offsetToNextAttribute_ );
		++attrIndex_;
		return *this;
	}
	//xml_node_iterator operator++( int );

	//const xml_node_iterator& operator--();
	//xml_node_iterator operator--( int );
};



inline Node::children_iterator Node::childrenBegin() const
{
	const u8* base = reinterpret_cast<const u8*>( node_ );
	const StreamBase::NodeImpl* firstChild = reinterpret_cast<const StreamBase::NodeImpl*>( base + node_->offsetToFirstChild_ );
	return NodeIterator( Node( firstChild ), 0 );
}
inline Node::children_iterator Node::childrenEnd() const
{
	return NodeIterator( Node(), node_->nChildren_ );
}

inline ObjectRange<NodeIterator> Node::children() const
{
	return ObjectRange<NodeIterator>( childrenBegin(), childrenEnd() );
}



inline Node::attribute_iterator Node::attributesBegin() const
{
	const u8* base = reinterpret_cast<const u8*>( node_ );
	// attributes start just after NodeImpl
	const StreamBase::AttributeImpl* firstAttribute = reinterpret_cast<const StreamBase::AttributeImpl*>( base + sizeof(StreamBase::NodeImpl) );
	return AttributeIterator( Attribute( firstAttribute ), 0 );
}
inline Node::attribute_iterator Node::attributesEnd() const
{
	return AttributeIterator( Attribute(), node_->nAttributes_ );
}

inline ObjectRange<AttributeIterator> Node::attributes() const
{
	return ObjectRange<AttributeIterator>( attributesBegin(), attributesEnd() );
}



} // namespace stream


} // namespace spad
