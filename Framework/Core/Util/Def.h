#pragma once

#include <stdint.h>
#include <stdio.h>
#include <tchar.h>
#include <string>
#include <vector>
#include <map>
#include <sstream>
#include <memory>
#include <mutex>
#include <thread>
#include <iostream>
#include <algorithm>

#define FR_ASSERTIONS_ENABLED 1

//#ifdef __cplusplus
//extern "C"
//{
//#endif //

#ifdef FR_ASSERTIONS_ENABLED
	#define AT __FILE__ ":" TOSTRING(__LINE__)
	#define STRINGIFY(x) #x
	#define TOSTRING(x) STRINGIFY(x)

	void assertPrintAndBreak( const char* text );
	void assertPrintAndBreak2( const char* text, const char* msg );

	#if defined(_MSC_VER)

		#define FR_ASSERT(expression)		\
			__pragma(warning(push))					\
			__pragma(warning(disable:4127))			\
			if ( (expression) )						\
			{										\
			}										\
			else									\
			{										\
				assertPrintAndBreak( "ASSERTION FAILED " #expression " " AT "\n" );		\
			}										\
			__pragma(warning(pop))

		#define FR_ASSERT2(expression, msg)		\
			__pragma(warning(push))					\
			__pragma(warning(disable:4127))			\
			if ( (expression) )						\
			{										\
			}										\
			else									\
			{										\
				assertPrintAndBreak2( "ASSERTION FAILED " #expression " " AT, (msg) );		\
			}										\
			__pragma(warning(pop))

	#endif //

	#define FR_ASSERT_ALWAYS(expression, msg) ((void)0)

#else // ! FR_ASSERTIONS_ENABLED
	#define FR_ASSERT(expression) ((void)0)
	#define FR_ASSERT2(expression, msg) ((void)0)
#endif // FR_ASSERTIONS_ENABLED

#define FR_STATIC_ASSERT(expr, msg) static_assert( expr, "static assertion failed: " #expr ": " msg )

#define FR_NOT_IMPLEMENTED assertPrintAndBreak( "NOT IMPLEMENTED!" AT )

#if defined(_MSC_VER)

	#define FR_ALIGNMENT(alignment) __declspec(align(alignment))

#else	//PPU, SPU, other?

	// Macro for declaring aligned structures
	#define FR_ALIGNMENT(alignment) __attribute__((aligned(alignment)))

#endif // _MSC_VER


#define FR_CACHELINE_SIZE 64



#if defined _DEBUG

/**
* operator new():
*  Here is the overloaded new operator, responsible for allocating and tracking the requested
*  memory.
*
*  Return Type: void* -> A pointer to the requested memory.
*  Arguments:
*  	size_t size	     : The size of memory requested in BYTES
*  	const char *szFileName : The file responsible for requesting the allocation.
*  	int nLine	       : The line number within the file requesting the allocation.
*/
__forceinline void* operator new( size_t size, const char *szFileName, int nLine ) {
	return _malloc_dbg( size, _NORMAL_BLOCK, szFileName, nLine );
}

__forceinline void* operator new[]( size_t size, const char *szFileName, int nLine ) {
	return _malloc_dbg( size, _NORMAL_BLOCK, szFileName, nLine );
}

/**
* operator delete():
*  This routine is responsible for de-allocating the requested memory.
*
*  Return Type: void
*  Arguments:
*  	void *address	: A pointer to the memory to be de-allocated.
*/
void operator delete( void *address );
void operator delete[]( void *address );

// ***** These two routines should never get called, unless an error occures during the 
// ***** allocation process.  These need to be defined to make Visual C++ 6.0 happy.
// ***** If there was an allocation problem these method would be called automatically by 
// ***** the operating system.  C/C++ Users Journal (Vol. 19 No. 4 -> April 2001 pg. 60)  
// ***** has an excellent explanation of what is going on here.
__forceinline void operator delete( void *address, const char * /*szFileName*/, int /*nLine*/ ) {
	_free_dbg( address, _NORMAL_BLOCK );
}
__forceinline void operator delete[]( void *address, const char * /*szFileName*/, int /*nLine*/ ) {
	_free_dbg( address, _NORMAL_BLOCK );
}

#endif // _DEBUG
 

inline void* frMallocAligned( size_t size, size_t alignment )
{
	void* ptr = _aligned_malloc( size, alignment );
	FR_ASSERT( ptr );
	return ptr;
}

inline void frFreeAligned( void* address )
{
	_aligned_free( address );
}

#define frFreeAligned2(x) { frFreeAligned(x); x = nullptr; }

#if defined(_MSC_VER) && defined(_DEBUG)
/**
*	Be aware to add:
*		#if defined(_MSC_VER) && defined(_DEBUG)
*		#define new _DEBUG_NEW
*		#endif
*	at the beginning of a file that uses 'new' operator. Only then overloaded versions for tracking allocation
*	will work!!!
*/
#define _DEBUG_NEW   new(__FILE__, __LINE__)
#else
#define _DEBUG_NEW
#endif //

// see http://jmabille.github.io/blog/2014/12/06/aligned-memory-allocator/
// for implementing allocator for stl containers

typedef uint8_t u8;
typedef uint16_t u16;
typedef uint32_t u32;
typedef uint64_t u64;
typedef int8_t i8;
typedef int16_t i16;
typedef int32_t i32;
typedef int64_t i64;
typedef wchar_t wchar;

/**
	It differs from MS implementation and from standard c/c++.
	Buffer is filled with characters up to it's capacity (max bufferSize-1 characters). Null character is always written at the end.
	If output string fits buffer, number of characters written is returned, excluding terminating null character.
	If output string was too long, then negative value is returned.
*/
int fr_snprintf( char* buffer, size_t bufferSize, const char* format, ... );

/**
* Returns value aligned on requested boundry
* @remarks   alignment must be power of '2'
* @sa        frAlignPtr2
* @return    uint32_t aligned value
* @param     uint32_t value value to align
* @param     uint32_t alignment must be power of '2'
*/
inline uint32_t frAlignU32_2( uint32_t value, uint32_t alignment )
{
	FR_ASSERT2((alignment & (alignment-1)) == 0, "alignment must be multiple of 2");
	alignment--;
	return ( (value + alignment) & ~alignment );
}

/**
 * Returns value aligned on requested boundry
 * @remarks   alignment don't have to be power of '2'
 * @sa        
 * @return    uint32_t
 * @param     uint32_t size
 * @param     uint32_t alignment any value, non power of '2' are also valid
 */
inline uint32_t frAlignU32(uint32_t size, uint32_t alignment)
{
	return ( (uint32_t)((size - 1) / alignment) + 1) * alignment;
}

inline uint64_t frAlignU64_2( uint64_t val, uint64_t alignment )
{
	FR_ASSERT2( ( alignment & ( alignment - 1 ) ) == 0, "alignment must be multiple of 2" );
	alignment--;
	return ( (val + alignment) & ~alignment );
}

//#ifdef __cplusplus
//}
//#endif //

