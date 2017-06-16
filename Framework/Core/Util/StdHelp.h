#pragma once

#include "Def.h"
#include <algorithm>
#include <vector>

namespace spad
{
	template<typename Cont, typename Pred>
	void for_each( const Cont& c, Pred p )
	{
		std::for_each( std::begin( c ), std::end( c ), p );
	}

	template<typename Cont, typename Pred>
	void clear_cont( Cont& c, Pred p )
	{
		std::for_each( std::begin( c ), std::end( c ), p );
		c.clear();
	}

	template<class T>
	inline void freeVectorMemory( std::vector<T>& v )
	{
		// most implementations of std::vector fail to free memory on clear() or resize(0)
		// ... but if we swap the memory pointers, the memory will be freed once temp goes
		// out of scope

		std::vector<T> temp;
		temp.swap( v );
	}

	std::string wstringToString( const std::wstring& wstr );
	std::wstring stringToWstring( const std::string& str );
	std::wstring stringToWstring( const char* str );

	// very awkward implementation of memory stream that can grow
	// something more sophisticated would use custom std::streambuf with std::ostream
	//
	class memstream
	{
	public:
		void reserve( size_t nBytes )
		{
			buf_.resize( nBytes );
		}

		void write( const u8* bytes, size_t nBytes )
		{
			if ( nWritten_ + nBytes > buf_.capacity() )
			{
				size_t extraSize = 256;
				buf_.resize( nWritten_ + nBytes + extraSize );
			}

			memcpy( &buf_[nWritten_], bytes, nBytes );
			nWritten_ += nBytes;
		}

		void write( const char* str )
		{
			size_t strLen = strlen( str );
			write( reinterpret_cast<const u8*>( str ), strLen );
		}

		void write( const std::string& str )
		{
			write( reinterpret_cast<const u8*>( str.c_str() ), str.size() + 1 );
		}

		const u8* data() const
		{
			return &buf_[0];
		}

		const size_t size() const
		{
			return buf_.size();
		}

	private:
		std::vector<u8> buf_;
		size_t nWritten_ = 0;
	};
}


namespace stdutil
{

//std::vector<std::string> splitString( const std::string& str )
//{
//	std::vector<std::string> tokens { std::istream_iterator<std::string>{str}, std::istream_iterator<std::string>{} };
//	return tokens;
//}

inline void splitIntoTwoStrings( const char* str, size_t strLen, char delim, std::string& left, std::string& right )
{
	const char* delimPos = reinterpret_cast<const char*>( memchr( str, delim, strLen ) );
	if ( delimPos )
	{
		size_t leftLen = (size_t)delimPos - (size_t)str;
		size_t rightLen = strLen - leftLen - 1;
		left.assign( str, leftLen );
		right.assign( str, rightLen );
	}
	else
	{
		left.assign( str, strLen );
		right.clear();
	}
}

inline void splitIntoTwoStrings( const char* str, size_t strLen, char delim, size_t& leftLen, const char*& right, size_t& rightLen )
{
	const char* delimPos = reinterpret_cast<const char*>( memchr( str, delim, strLen ) );
	if ( delimPos )
	{
		leftLen = (size_t)delimPos - (size_t)str;
		rightLen = (size_t)strLen - (size_t)delimPos - 1;
		right = delimPos + 1;
	}
	else
	{
		leftLen = strLen;
		right = nullptr;
		rightLen = 0;
	}
}

inline void splitIntoTwoStrings( const char* str, char delim, size_t& leftLen, const char*& right, size_t& rightLen )
{
	splitIntoTwoStrings( str, strlen( str ), delim, leftLen, right, rightLen );
}

inline bool CompareLess( const char* lhsName, size_t lhsNameLen, const char* rhsName, size_t rhsNameLen )
{
	if ( lhsNameLen < rhsNameLen )
		return true;
	else if ( lhsNameLen == rhsNameLen )
		return memcmp( lhsName, rhsName, lhsNameLen ) < 0;

	return false;
}


template<class L, class R>
struct NameComparator
{
	bool operator() ( const L& lhs, const R& rhs ) const
	{
		//size_t lLen = lhs->getNameLen();
		//size_t rLen = rhs->getNameLen();
		//if ( lLen < rLen )
		//	return true;
		//else if ( lLen == rLen )
		//	return memcmp( lhs->getName(), rhs->getName(), lLen ) < 0;

		//return false;
		return CompareLess( lhs.getName(), lhs.getNameLen(), rhs.getName(), rhs.getNameLen() );
	}
};


template<class L, class R, class P, size_t LPrefixLenToSkip = 0, size_t RPrefixLenToSkip = 0>
struct NameComparatorWithParam
{
	const P& p_;

	NameComparatorWithParam( const P& p )
		: p_( p )
	{	}

	NameComparatorWithParam& operator=( const NameComparatorWithParam& rhs ) = delete;

	bool operator() ( const L& lhs, const R& rhs ) const
	{
		size_t lLen = lhs.getNameLength( p_ ) - LPrefixLenToSkip;
		size_t rLen = rhs.getNameLength( p_ ) - RPrefixLenToSkip;
		if ( lLen < rLen )
			return true;
		else if ( lLen == rLen )
			return memcmp( lhs.getName( p_ ) + LPrefixLenToSkip, rhs.getName( p_ ) + RPrefixLenToSkip, lLen ) < 0;

		//size_t lhsNameLen;
		//const char* lhsName = lhs.getNameAndLength( p_, lhsNameLen );
		//size_t rhsNameLen;
		//const char* rhsName = rhs.getNameAndLength( p_, rhsNameLen );

		//if ( lLen < rLen )
		//	return true;
		//else if ( lLen == rLen )
		//	return memcmp( lName, rName, lLen ) < 0;

		return false;

		//return CompareLess( lhsName, lhsNameLen, rhsName, rhsNameLen );
	}

	bool operator() ( const R& lhs, const L& rhs ) const
	{
		size_t lLen = lhs.getNameLength( p_ ) - RPrefixLenToSkip;
		size_t rLen = rhs.getNameLength( p_ ) - LPrefixLenToSkip;
		if ( lLen < rLen )
			return true;
		else if ( lLen == rLen )
			return memcmp( lhs.getName( p_ ) + RPrefixLenToSkip, rhs.getName( p_ ) + LPrefixLenToSkip, lLen ) < 0;

		//size_t lhsNameLen;
		//const char* lhsName = lhs.getNameAndLength( p_, lhsNameLen );
		//size_t rhsNameLen;
		//const char* rhsName = rhs.getNameAndLength( p_, rhsNameLen );

		//if ( lLen < rLen )
		//	return true;
		//else if ( lLen == rLen )
		//	return memcmp( lName, rName, lLen ) < 0;

		return false;

		//return CompareLess( lhsName, lhsNameLen, rhsName, rhsNameLen );
	}
};

template<typename R, typename Cont, typename Val, typename Pred>
R binary_search( const Cont& c, const Val& val, const Pred& p, const R invalidIndex )
{
	Cont::const_iterator it = std::lower_bound( std::cbegin( c ), std::cend( c ), val, p );

	if ( it != cend( c ) && !p( val, *it ) )
	{
		size_t idx = it - cbegin( c );
		SPAD_ASSERT( idx < std::numeric_limits<R>::max() );
		return (R)idx;
	}

	return invalidIndex;
}

} // namespace stdutil
