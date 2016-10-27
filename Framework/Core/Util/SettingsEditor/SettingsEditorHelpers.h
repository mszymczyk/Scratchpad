#pragma once

namespace SettingsEditor
{


typedef uint8_t u8;
typedef uint16_t u16;
typedef uint32_t u32;
typedef uint64_t u64;
typedef int8_t i8;
typedef int16_t i16;
typedef int32_t i32;
typedef int64_t i64;


//////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////
// minOfPair
//////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////

template <typename T> inline T minOfPair( const T a, const T b ) { return ( a < b ) ? a : b; }

template <> inline uint32_t minOfPair <uint32_t>( const uint32_t a, const uint32_t b )
{
	int64_t cmp = ( (int64_t)a - (int64_t)b );
	cmp >>= 63;
	return ( (uint32_t)cmp & a ) | ( ~(uint32_t)cmp & b );
}

template <> inline int32_t minOfPair <int32_t>( const int32_t a, const int32_t b )
{
	int64_t cmp = ( (int64_t)a - (int64_t)b );
	cmp >>= 63;
	return ( (int32_t)cmp & a ) | ( ~(int32_t)cmp & b );
}

template <> inline u16 minOfPair <u16>( const u16 a, const u16 b )
{
	signed int cmp = ( (i32)a - (i32)b );
	cmp >>= 31;
	return ( (u16)cmp & a ) | ( ~(u16)cmp & b );
}
template <> inline i16 minOfPair <i16>( const i16 a, const i16 b )
{
	signed int cmp = ( (i32)a - (i32)b );
	cmp >>= 31;
	return ( (i16)cmp & a ) | ( ~(i16)cmp & b );
}
template <> inline u8 minOfPair <u8>( const u8 a, const u8 b )
{
	signed int cmp = ( (i32)a - (i32)b );
	cmp >>= 31;
	return ( (u8)cmp & a ) | ( ~(u8)cmp & b );
}
template <> inline i8 minOfPair <i8>( const i8 a, const i8 b )
{
	signed int cmp = ( (i32)a - (i32)b );
	cmp >>= 31;
	return ( (i8)cmp & a ) | ( ~(i8)cmp & b );
}

template <> inline float minOfPair <float>( float a, float b )
{
	return ( a < b ) ? a : b;
}

//////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////
// maxOfPair
//////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////
template <typename T> inline T maxOfPair( const T a, const T b ) { return ( a > b ) ? a : b; }

inline uint32_t maxOfPair( const uint32_t a, const uint32_t b )
{
	int64_t cmp = ( (int64_t)b - (int64_t)a );
	cmp >>= 63;
	return ( (uint32_t)cmp & a ) | ( ~(uint32_t)cmp & b );
}

inline int32_t maxOfPair( const int32_t a, const int32_t b )
{
	int64_t cmp = ( (int64_t)b - (int64_t)a );
	cmp >>= 63;
	return ( (int32_t)cmp & a ) | ( ~(int32_t)cmp & b );
}

template <> inline u16 maxOfPair <u16>( const u16 a, const u16 b )
{
	signed int cmp = ( (i32)b - (i32)a );
	cmp >>= 31;
	return ( (u16)cmp & a ) | ( ~(u16)cmp & b );
}

template <> inline i16 maxOfPair <i16>( const i16 a, const i16 b )
{
	signed int cmp = ( (i32)b - (i32)a );
	cmp >>= 31;
	return ( (i16)cmp & a ) | ( ~(i16)cmp & b );
}

template <> inline u8 maxOfPair <u8>( const u8 a, const u8 b )
{
	signed int cmp = ( (i32)b - (i32)a );
	cmp >>= 31;
	return ( (u8)cmp & a ) | ( ~(u8)cmp & b );
}

template <> inline i8 maxOfPair <i8>( const i8 a, const i8 b )
{
	signed int cmp = ( (i32)b - (i32)a );
	cmp >>= 31;
	return ( (i8)cmp & a ) | ( ~(i8)cmp & b );
}


template <> inline float maxOfPair <float>( float a, float b )
{
	return ( b < a ) ? a : b;
}


template <typename T>
static T clamp( const T value, const T minimum, const T maximum )
{
	return maxOfPair( minimum, minOfPair( value, maximum ) );
}


} // namespace SettingsEditor
