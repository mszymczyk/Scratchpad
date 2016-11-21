#pragma once

#include "Def.h"

namespace spad
{

	//	
	// Increment wrapping value
	//
	// val = { ( val == max ), min 
	// = { otherwise, val + 1 
	//
	// uint8_t wrap_inc_u8 ( const uint8_t val, const uint8_t min, const uint8_t max );
	// uint16_t wrap_inc_u16( const uint16_t val, const uint16_t min, const uint16_t max );
	// uint32_t wrap_inc_u32( const uint32_t val, const uint32_t min, const uint32_t max );
	// uint64_t wrap_inc_u64( const uint64_t val, const uint64_t min, const uint64_t max );
	// int8_t wrap_inc_s8 ( const int8_t val, const int8_t min, const int8_t max );
	// int16_t wrap_inc_s16( const int16_t val, const int16_t min, const int16_t max );
	// int32_t wrap_inc_s32( const int32_t val, const int32_t min, const int32_t max );
	// int64_t wrap_inc_s64( const int64_t val, const int64_t min, const int64_t max );
#ifdef _MSC_VER // microsoft compiler
#define DECL_WRAP_INC( type_name, type, stype, bit_mask ) \
	static inline type wrap_inc_##type_name( const type val, const type min, const type max ) \
{ \
	__pragma(warning(push))	\
	__pragma(warning(disable:4146))	\
	const type result_inc = val + 1; \
	const type max_diff = max - val; \
	const type max_diff_nz = (type)( (stype)( max_diff | -max_diff ) >> bit_mask ); \
	const type max_diff_eqz = ~max_diff_nz; \
	const type result = ( result_inc & max_diff_nz ) | ( min & max_diff_eqz ); \
	\
	return (result); \
	__pragma(warning(pop))	\
}

#else

#define DECL_WRAP_INC( type_name, type, stype, bit_mask ) \
	static inline type wrap_inc_##type_name( const type val, const type min, const type max ) \
{ \
	const type result_inc = val + 1; \
	const type max_diff = max - val; \
	const type max_diff_nz = (type)( (stype)( max_diff | -max_diff ) >> bit_mask ); \
	const type max_diff_eqz = ~max_diff_nz; \
	const type result = ( result_inc & max_diff_nz ) | ( min & max_diff_eqz ); \
	\
	return (result); \
}
#endif // _MSC_VER

	DECL_WRAP_INC( u8, uint8_t, int8_t, 7 )
		DECL_WRAP_INC( u16, uint16_t, int16_t, 15 )
		DECL_WRAP_INC( u32, uint32_t, int32_t, 31 )
		DECL_WRAP_INC( u64, uint64_t, int64_t, 63 )
		DECL_WRAP_INC( s8, int8_t, int8_t, 7 )
		DECL_WRAP_INC( s16, int16_t, int16_t, 15 )
		DECL_WRAP_INC( s32, int32_t, int32_t, 31 )
		DECL_WRAP_INC( s64, int64_t, int64_t, 63 )

		// 
		// Decrementing wrapping value 
		// 
		// val = { ( val == min ), max 
		// = { otherwise, val - 1 
		// 
		// uint8_t wrap_dec_u8 ( const uint8_t val, const uint8_t min, const uint8_t max ); 
		// uint16_t wrap_dec_u16( const uint16_t val, const uint16_t min, const uint16_t max ); 
		// uint32_t wrap_dec_u32( const uint32_t val, const uint32_t min, const uint32_t max ); 
		// uint64_t wrap_dec_u64( const uint64_t val, const uint64_t min, const uint64_t max ); 
		// int8_t wrap_dec_s8 ( const int8_t val, const int8_t min, const int8_t max ); 
		// int16_t wrap_dec_s16( const int16_t val, const int16_t min, const int16_t max ); 
		// int32_t wrap_dec_s32( const int32_t val, const int32_t min, const int32_t max ); 
		// int64_t wrap_dec_s64( const int64_t val, const int64_t min, const int64_t max ); 
#ifdef _MSC_VER // microsoft compiler

#define DECL_WRAP_DEC( type_name, type, stype, bit_mask ) \
	static inline type wrap_dec_##type_name( const type val, const type min, const type max ) \
	{ \
	__pragma(warning(push))	\
	__pragma(warning(disable:4146))	\
	const type result_dec = val - 1; \
	const type min_diff = min - val; \
	const type min_diff_nz = (type)( (stype)( min_diff | -min_diff ) >> bit_mask ); \
	const type min_diff_eqz = ~min_diff_nz; \
	const type result = ( result_dec & min_diff_nz ) | ( max & min_diff_eqz ); \
	\
	return (result); \
	__pragma(warning(pop))	\
	} 

#else

#define DECL_WRAP_DEC( type_name, type, stype, bit_mask ) \
	static inline type wrap_dec_##type_name( const type val, const type min, const type max ) \
	{ \
	const type result_dec = val - 1; \
	const type min_diff = min - val; \
	const type min_diff_nz = (type)( (stype)( min_diff | -min_diff ) >> bit_mask ); \
	const type min_diff_eqz = ~min_diff_nz; \
	const type result = ( result_dec & min_diff_nz ) | ( max & min_diff_eqz ); \
	\
	return (result); \
	} 
#endif // _MSC_VER

		DECL_WRAP_DEC( u8, uint8_t, int8_t, 7 )
		DECL_WRAP_DEC( u16, uint16_t, int16_t, 15 )
		DECL_WRAP_DEC( u32, uint32_t, int32_t, 31 )
		DECL_WRAP_DEC( u64, uint64_t, int64_t, 63 )
		DECL_WRAP_DEC( s8, int8_t, int8_t, 7 )
		DECL_WRAP_DEC( s16, int16_t, int16_t, 15 )
		DECL_WRAP_DEC( s32, int32_t, int32_t, 31 )
		DECL_WRAP_DEC( s64, int64_t, int64_t, 63 )


		// Compare Not Zero for 32 bit integer values
		// http://cellperformance.beyond3d.com/articles/2006/04/using-masks-to-accelerate-integer-code.html
		// returns 0xffffffff if arg is != 0 and 0x00000000 if arg == 0
		static inline uint32_t cmpnz_u32( const uint32_t arg )
	{
		/* Set MSB if arg is positive */
#ifdef _MSC_VER
#pragma warning(push)
#pragma warning(disable:4146)
#endif //
		const uint32_t arg_neg = -arg;
#ifdef _MSC_VER
#pragma warning(pop)
#endif //
		/* Set MSB if arg is not zero */
		const uint32_t arg_snz = arg | arg_neg;
		/* Set all bits to MSB */
		const uint32_t arg_snz_sat = (uint32_t)( ( (int32_t)arg_snz ) >> 31 );
		return ( arg_snz_sat );
	}

	static inline uint64_t cmpnz_u64( const uint64_t arg )
	{
		/* Set MSB if arg is positive */
#ifdef _MSC_VER
#pragma warning(push)
#pragma warning(disable:4146)
#endif //
		const uint64_t arg_neg = -arg;
#ifdef _MSC_VER
#pragma warning(pop)
#endif //
		/* Set MSB if arg is not zero */
		const uint64_t arg_snz = arg | arg_neg;
		/* Set all bits to MSB */
		const uint64_t arg_snz_sat = (uint64_t)( ( (int64_t)arg_snz ) >> 63 );
		return ( arg_snz_sat );
	}

	// Select between 32 bit integer values from mask
	// http://cellperformance.beyond3d.com/articles/2006/04/using-masks-to-accelerate-integer-code.html
	// This version differs a bit from one presented on site. This one is more like vec_sel from altivec.
	// Selects bits from arg0 and arg1 according to bits in mask. If bit is '0' then corresponding bit
	// from arg0 is selected - if mask's bit is '1' then bit from arg1 is selected.
	static inline uint32_t sel_u32( const uint32_t arg0, const uint32_t arg1, const uint32_t mask )
	{
		/* Set to arg0 only if mask is clear */
		const uint32_t arg0_result = arg0 & ( ~mask );
		/* Set to arg1 only if mask is set */
		const uint32_t arg1_result = arg1 & mask;
		/* Only one result can be non-zero. */
		const uint32_t result = arg0_result | arg1_result;
		return ( result );
	}

	static inline uint64_t sel_u64( const uint64_t arg0, const uint64_t arg1, const uint64_t mask )
	{
		/* Set to arg0 only if mask is clear */
		const uint64_t arg0_result = arg0 & ( ~mask );
		/* Set to arg1 only if mask is set */
		const uint64_t arg1_result = arg1 & mask;
		/* Only one result can be non-zero. */
		const uint64_t result = arg0_result | arg1_result;
		return ( result );
	}

	/** Computes modulo value when divider is power-of-2 number. Works for negative values.
	 @return int
	 @param int x
	 @param unsigned int divider must be power of 2
	 */
	inline int fastModulo2( int x, unsigned int divider )
	{
		return ( x & ( divider - 1 ) );
	}
	inline i32 iceil( const i32 x, const i32 y )
	{
		return ( 1 + ( ( x - 1 ) / y ) );
	}

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


	//////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////
	// minOfTripple, maxOfTripple, clamp
	//////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////
	template< typename T > inline T minOfTriple( const T a, const  T b, const T c )
	{
		return minOfPair( a, minOfPair( b, c ) );
	}

	template< typename T > inline T maxOfTriple( const T a, const  T b, const T c )
	{
		return maxOfPair( a, maxOfPair( b, c ) );
	}

	//inline int32_t minOfTriple( const int32_t a, const int32_t b, const int32_t c )
	//{
	//	return minOfPair( a, minOfPair(b,c) );
	//}
	//
	//inline int32_t maxOfTriple( const int32_t a, const int32_t b, const int32_t c )
	//{
	//	return maxOfPair( a, maxOfPair(b,c) );
	//}
	//
	//
	//inline float minOfTriple( float a, float b, float c )
	//{
	//	return minOfPair( a, minOfPair(b,c) );
	//}
	//
	//inline float maxOfTriple( float a, float b, float c )
	//{
	//	return maxOfPair( a, maxOfPair(b,c) );
	//}

	template <typename T>
	static T clamp( const T value, const T minimum, const T maximum )
	{
		return maxOfPair( minimum, minOfPair( value, maximum ) );
	}

	//inline float clamp( const float x, const float a, const float b )
	//{
	//	return maxOfPair( a, minOfPair(b,x) );
	//}


	inline float select( float x, float a, float b )
	{
		return ( x >= 0 ) ? a : b;
	}

	/** Select a if a is non-NULL, select b otherwise
	 */
	template <typename T>
	inline T *selectPtr( T *a, T *b )
	{
		return a ? a : b;
	}

	/** check if float is sane
	*/
	void checkFloat( float x );

	static inline int asint( float val )
	{
		return *reinterpret_cast<int*>( &val );
	}

	static inline u32 asuint( float val )
	{
		return *reinterpret_cast<u32*>( &val );
	}

	static inline float asfloat( int val )
	{
		return *reinterpret_cast<float*>( &val );
	}

	static inline float asfloat( u32 val )
	{
		return *reinterpret_cast<float*>( &val );
	}

} // namespace spad
