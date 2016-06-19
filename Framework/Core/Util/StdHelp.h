#pragma once

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

