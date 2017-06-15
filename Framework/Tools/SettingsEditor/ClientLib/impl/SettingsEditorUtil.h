#pragma once

#include "..\SettingsEditor.h"
#include <mutex>
#include <memory>
#include <vector>
#include <new>
#include <stdarg.h>

#if defined(_DEBUG) && defined(_MSC_VER)
#define _CRTDBG_MAP_ALLOC
//#define _CRTDBG_MAP_ALLOC_NEW
#include <stdlib.h>
#include <crtdbg.h>
#else
//#include <malloc.h>
#include <stdlib.h>
#endif


namespace SettingsEditor
{
namespace _internal
{

	//typedef uint8_t u8;
	//typedef uint16_t u16;
	//typedef uint32_t uint32_t;
	//typedef uint64_t u64;
	//typedef int8_t i8;
	//typedef int16_t i16;
	//typedef int32_t i32;
	//typedef int64_t i64;

#if SETTINGS_EDITOR_ASSERT_ENABLED
	void assertPrintAndBreak( const char* text );

#define AT __FILE__ ":" TOSTRING(__LINE__)
#define STRINGIFY(x) #x
#define TOSTRING(x) STRINGIFY(x)

#define SETTINGS_EDITOR_ASSERT(expression)			(void)( (!!(expression)) || (_internal::assertPrintAndBreak( "ASSERTION FAILED " #expression " " AT "\n" ), 0) )
#define SETTINGS_EDITOR_ASSERT2(expression, msg)	(void)( (!!(expression)) || (_internal::assertPrintAndBreak( "ASSERTION FAILED " #expression " " AT " " msg "\n" ), 0) )
#define SETTINGS_EDITOR_NOT_IMPLEMENTED SETTINGS_EDITOR_ASSERT(false)

#else

#define SETTINGS_EDITOR_ASSERT (expression)
#define SETTINGS_EDITOR_ASSERT2(expression, msg)
#define SETTINGS_EDITOR_NOT_IMPLEMENTED

#endif //

	extern _internal::AllocFunc gAllocFunc;
extern _internal::FreeFunc gFreeFunc;

#if defined(_DEBUG) && defined(_MSC_VER)

inline void* aligned_malloc_dbg( size_t size, size_t align, const char * szFileName, int nLine )
{
	(void)szFileName;
	(void)nLine;

	if (gAllocFunc)
		return gAllocFunc( size, align );

#ifdef _MSC_VER 
	return _aligned_malloc_dbg( size, align, szFileName, nLine );
#else 
	return memalign( align, size );
#endif
}

inline void aligned_free_dbg( void * p )
{
	if (gFreeFunc)
	{
		gFreeFunc( p );
		return;
	}

#ifdef _MSC_VER 
	return _aligned_free_dbg( p );
#else 
	return free( p );
#endif
}

#define memAlloc( size, alignment ) aligned_malloc_dbg( size, alignment, __FILE__, __LINE__ )
#define memFree( p ) aligned_free_dbg( p )

#else

inline void* aligned_malloc_rel( size_t size, size_t align )
{
	if (gAllocFunc)
		return gAllocFunc( size, align );

#ifdef _MSC_VER 
	return _aligned_malloc( size, align );
#else 
	return memalign( align, size );
#endif
}

inline void aligned_free_rel( void * p )
{
	if (gFreeFunc)
	{
		gFreeFunc( p );
		return;
	}

#ifdef _MSC_VER 
	return _aligned_free( p );
#else 
	return free( p );
#endif
}

#define memAlloc( size, alignment ) aligned_malloc_rel( size, alignment )
#define memFree( p ) aligned_free_rel( p )

#endif //

#if defined(_DEBUG) && defined(_MSC_VER)

#define DECLARE_ALIGNED_NEW(boundary)			\
	inline void* operator new (size_t size, const char * szFileName, int nLine) {		\
		return aligned_malloc_dbg(size, boundary, szFileName, nLine);		\
	}												\
	\
	inline void* operator new[] (size_t size, const char * szFileName, int nLine) {		\
		return aligned_malloc_dbg(size, boundary, szFileName, nLine);		\
	}												\
	\
	inline void operator delete (void *p) {			\
		aligned_free_dbg(p);						\
	}												\
	\
	inline void operator delete[] (void *p) {		\
		aligned_free_dbg(p);						\
	}												\
	\
	inline void operator delete (void *p, const char * /*szFileName*/, int /*nLine*/) {			\
		aligned_free_dbg(p);						\
	}												\
	\
	inline void operator delete[] (void *p, const char * /*szFileName*/, int /*nLine*/) {		\
		aligned_free_dbg(p);						\
	}

#else // ! defined(_DEBUG)

#define DECLARE_ALIGNED_NEW(boundary)			\
	inline void* operator new (size_t size) {		\
		return aligned_malloc_rel(size, boundary);	\
	}												\
	\
	inline void* operator new[] (size_t size) {		\
		return aligned_malloc_rel(size, boundary);	\
	}												\
	\
	inline void operator delete (void *p) {			\
		aligned_free_rel(p);						\
	}												\
	\
	inline void operator delete[] (void *p) {		\
		aligned_free_rel(p);						\
	}

#endif // ! defined(_DEBUG)


inline void aligned_free( void *ptr )
{
	if (gFreeFunc)
	{
		gFreeFunc( ptr );
		return;
	}

#ifdef _MSC_VER 
	_aligned_free( ptr );
#else 
	free( ptr );
#endif
}


// https://msdn.microsoft.com/en-us/library/aa985953.aspx
template <class T>
struct SimpleStdAllocator
{
	typedef T value_type;
	SimpleStdAllocator() noexcept {} //default ctor not required by STL  

							 // A converting copy constructor:  
	template<class U> SimpleStdAllocator( const SimpleStdAllocator<U>& ) noexcept {}
	template<class U> bool operator==( const SimpleStdAllocator<U>& ) const noexcept {
		return true;
	}
	template<class U> bool operator!=( const SimpleStdAllocator<U>& ) const noexcept {
		return false;
	}
	T* allocate( const size_t n ) const;
	void deallocate( T* const p, size_t ) const noexcept;
};

template <class T>
T* SimpleStdAllocator<T>::allocate( const size_t n ) const
{
	if (n == 0)
		return nullptr;

	//if (n > static_cast<size_t>(-1) / sizeof( T ))
	//{
	//	throw std::bad_array_new_length();
	//}
	//void* const pv = malloc( n * sizeof( T ) );
	//if (!pv) {
	//	throw std::bad_alloc();
	//}

	return static_cast<T*>(memAlloc( n * sizeof( T ), alignof(T) ));
}

template<class T>
void SimpleStdAllocator<T>::deallocate( T * const p, size_t ) const noexcept
{
	memFree( p );
}


typedef std::basic_string<char, std::char_traits<char>, SimpleStdAllocator<char> >
String;


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

} // namespace _internal
} // namespace SettingsEditor
