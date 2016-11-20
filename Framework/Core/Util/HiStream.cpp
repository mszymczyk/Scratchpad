//#include "Util_pch.h"
#include "HiStream.h"

namespace spad
{

namespace stream
{


//void OutputStream<>::begin()
//{
//	//_ReserveAtLeast( sizeof( NodeImpl ) );
//	//rootImpl_ = reinterpret_cast<NodeImpl*>(buf_);
//	rootImpl_ = _AddNode( 0xff );
//	//bufSize_ = sizeof( NodeImpl );
//
//	//HiStreamNode* root = reinterpret_cast<HiStreamNode*>(memForHiStreamNode_);
//	//*root = HiStreamNode( this, 0, 0xffffffff );
//
//	//return *root;
//	_PushStack( rootImpl_ );
//}
//
//void OutputStream<>::end()
//{
//	_PopStack();
//	assert( stackCount_ == 0 );
//}


void OutputStream<true>::pushChild( u8 tag )
{
	NodeImpl* n = _AddNode( tag );
	//n->offsetInBuf_ = _GetNodeOffset( n );

	NodeImpl*& prevSibling = prevSiblingStack_[stackCount_];
	if ( prevSibling )
		prevSibling->offsetToNextSibling_ = _GetNodeOffset( n, prevSibling );
	prevSibling = n;

	if ( curNode_ )
	{
		if ( curNode_->nChildren_ == 0 )
			curNode_->offsetToFirstChild_ = _GetNodeOffset( n, curNode_ );
		++curNode_->nChildren_;
	}

	_PushStack( n );

	curAttribute_ = nullptr;
}


void OutputStream<true>::popChild()
{
	_PopStack();
	curAttribute_ = nullptr;
}

u8* OutputStream<true>::_AllocateMemImpl( size_t nBytes, size_t alignment )
{
//#if HISTREAM_UNALIGNED_DATA
//	( void )alignment;
//	size_t bufSizeAligned = bufUsedSize_;
//#else
	size_t bufSizeAligned = alignPowerOfTwo( bufUsedSize_, alignment );
//#endif //

#if HISTREAN_COUNT_WASTED_MEMORY
	paddingWastedSize_ += bufSizeAligned - bufUsedSize_;
#endif //
	size_t newBufSize = bufSizeAligned + nBytes;;
	if (newBufSize <= bufCapacity_)
	{
		u8* p = buf_ + bufSizeAligned;
		bufUsedSize_ = newBufSize;
		return p;
	}

	bufCapacity_ = newBufSize + 1024;
	u8* newBuf = reinterpret_cast<u8*>(spadMallocAligned( bufCapacity_, 64 ));
	if (buf_)
		memcpy( newBuf, buf_, bufUsedSize_ );

	memset( newBuf + bufUsedSize_, 0, bufCapacity_ - bufUsedSize_ );

	spadFreeAligned( buf_ );
	buf_ = newBuf;

	u8* p = buf_ + bufSizeAligned;
	bufUsedSize_ = newBufSize;
	return p;
}

template<>
OutputStream<true>::NodeImpl* OutputStream<true>::_AddNode( u8 tag )
{
	NodeImpl* n = _AllocateNode();
	//n->offsetInBuf_ = bufSize_;
	n->offsetToNextSibling_ = 0;
	n->offsetToFirstChild_ = 0;
	n->nChildren_ = 0;
	n->nAttributes_ = 0;
	n->nodeTag_ = tag;
	n->_padding_ = 0;
	return n;
}


OutputStream<true>::AttributeImpl* OutputStream<true>::_AddAttr( u8 tag, AttributeType::Type typ )
{
	AttributeImpl* a = _AllocateAttr();
	a->attrType_ = typ;
	a->offsetToNextAttribute_ = 0;
	a->tag_ = tag;
	if (curAttribute_)
		curAttribute_->offsetToNextAttribute_ = _GetAttrOffset( a, curAttribute_ );
	curAttribute_ = a;
	++curNode_->nAttributes_;
	return a;
}

void OutputStream<true>::addFloat( u8 tag, float f )
{
	_AddAttr( tag, AttributeType::Float );
	u8* mem = (u8*)_AllocateMem<float>();
	//_SetFloat( mem, f );
	_SetNumber( mem, f );
}

void OutputStream<true>::addStringFloat( u8 tag, const char* str, size_t strLen, float f )
{
	_AddAttr( tag, AttributeType::stringFloat );
	//u32* m = _AllocateMem<u32>();
	//*m = (u32)strLen;
	//char* mc = _AllocateMem<char>( strLen + 1 );
	//memcpy( mc, str, strLen + 1 );
	//float* mf = _AllocateMem<float>();
	//*mf = f;

	size_t req = _CountAllocReq<u32>( 0 );
	req = _CountAllocReq<char>( req, strLen + 1 );
	req = _CountAllocReq<float>( req );

	u8* mem = _AllocateMem( req, alignof( u32 ) );
	mem = _SetString( mem, str, strLen );
	//mem = _SetFloat( mem, f );
	mem = _SetNumber( mem, f );
}


void OutputStream<true>::addStringDelimString( u8 tag, const char* strL, size_t strLLen, char delim, const char* strR, size_t strRLen )
{
	_AddAttr( tag, AttributeType::stringDelimString );

	size_t req = _CountAllocReq<u32>( 0, 1 );
	req = _CountAllocReq<char>( req, strLLen + 1 );
	req = _CountAllocReq<u32>( req );
	req = _CountAllocReq<char>( strRLen + 2 ); // +2 because... delim + null for strR

	u8* mem = _AllocateMem( req, alignof(u32) );
	//u32* sLR = reinterpret_cast<u32*>(mem);
	//sLR[0] = (u32)strLLen;
	//sLR[1] = (u32)strRLen;

	//char* c = reinterpret_cast<char*>( mem + 2 * sizeof( u32 ) );
	//memcpy( c, strL, strLLen + 1 );
	//c[strLLen + 1] = delim;
	//memcpy( c + strLLen + 2, strR, strRLen + 1 );
	mem = _SetString( mem, strL, strLLen );
	*mem = delim;
	mem += 1;
	_SetString( mem, strR, strRLen );
}


//u8* OutputStream::_SetFloat( u8* ptr, float f )
//{
//	u8* data = alignPowerOfTwo( ptr, alignof( u32 ) );
//	SPAD_ASSERT( data + 4 < buf_ + bufSize_ );
//	*reinterpret_cast<float*>( data ) = f;
//	return data + 4;
//}

//u8* OutputStream::_SetString( u8* ptr, const char* str, size_t strLen )
//{
//	u8* data = alignPowerOfTwo( ptr, alignof( u32 ) );
//	SPAD_ASSERT( data + 4 + strLen + 1 < buf_ + bufSize_ );
//	*reinterpret_cast<u32*>( data ) = (u32)strLen;
//	data += 4;
//	memcpy( data, str, strLen + 1 );
//	return data + strLen + 1;
//}

//HiStreamNode HiStreamNode::pushChild( u8 tag )
//{
//	NodeImpl* n = stream_->_AllocateNode(tag);
//	return HiStreamNode( stream_, (u32)( (size_t)n - (size_t)stream_->buf_ ), nodeImplOffset_ );
//}


InputStream::InputStream( const u8* buf, size_t bufSize )
	: buf_( buf )
	, bufSize_( bufSize )
	, rootNode_( reinterpret_cast<const NodeImpl*>( buf ) )
{

}


Node InputStream::getRoot() const
{
	return Node( rootNode_ );
}

//inline size_t spadAlignPowerOfTwo2( size_t val, size_t alignment )
//{
//	//SPAD_ASSERT2( ( alignment & ( alignment - 1 ) ) == 0, "alignment must be multiple of 2" );
//	alignment--;
//	size_t res = ( val + alignment ) & ~alignment;
//	return res;
//}

void Attribute::getFloat( float& f ) const
{
	assert( attr_->attrType_ == AttributeType::Float );
	const u8* data = _GetDataAddress();
	_GetFloat( data, f );
}

void Attribute::getStringFloat( const char*& str, size_t& strLen, float& f ) const
{
	assert( attr_->attrType_ == AttributeType::stringFloat );
	const u8* data = _GetDataAddress();
	data = _GetString( data, str, strLen );
	_GetFloat( data, f );
}


void Attribute::getStringDelimString( const char*& strL, size_t& strLLen, char& delim, const char* strR, size_t strRLen ) const
{
	assert( attr_->attrType_ == AttributeType::stringFloat );
	const u8* data = _GetDataAddress();
	data = _GetString( data, strL, strLLen );
	delim = static_cast<char>( *data );
	data += 1;
	_GetString( data, strR, strRLen );
}

void Attribute::getCustomData( const void*& data, u32& dataSize ) const
{
	assert( attr_->attrType_ == AttributeType::customData );
	const u8* ptr = _GetDataAddress();
	data = _GetU32( ptr, dataSize );
}


const u8* Attribute::_GetString( const u8* ptr, const char*& str, size_t& strLen ) const
{
	ptr = alignPowerOfTwo( ptr, alignof( u32 ) );
	strLen = *reinterpret_cast<const u32*>( ptr );
	ptr += sizeof( u32 );
	str = reinterpret_cast<const char*>( ptr );
	ptr += strLen + 1;
	return ptr;
}


const u8* Attribute::_GetFloat( const u8* ptr, float& f ) const
{
	const u8* data = alignPowerOfTwo( ptr, alignof( float ) );
	f = *reinterpret_cast<const float*>( data );
	return data + sizeof(float);
}


const u8* Attribute::_GetU32( const u8* ptr, u32& u ) const
{
	const u8* data = alignPowerOfTwo( ptr, alignof( u32 ) );
	u = *reinterpret_cast<const u32*>( data );
	return data + sizeof( u32 );
}

} // namespace stream

} // namespace spad
