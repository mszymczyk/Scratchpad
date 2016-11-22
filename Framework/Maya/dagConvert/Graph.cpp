#include "Graph.h"

uint64_t charToHex( char c )
{
	// according to spec, uuid contains lower case letters, maya uses upper case letters

	if ( c >= '0' && c <= '9' )
		return c - '0';
	else if ( c >= 'A' && c <= 'F' )
		return c - 'A' + 10;
	else if ( c >= 'a' && c <= 'f' )
		return c - 'a' + 10;
	else
	{
		SPAD_ASSERT( false );
		return 0;
	}
}

Uuid Uuid::fromString( const char* str )
{
	// example uuid string
	// 123e4567-e89b-12d3-a456-426655440000

	Uuid uuid = { 0, 0 };

	const char* strPtr = str;
	for ( size_t i = 0; i < 8; ++i )
		uuid.high_ = uuid.high_ | ( charToHex( strPtr[i] ) << ( 60 - i * 4 ) );
	strPtr += 9; // skip digits and hyphen

	for ( size_t i = 0; i < 4; ++i )
		uuid.high_ = uuid.high_ | ( charToHex( strPtr[i] ) << ( 28 - i * 4 ) );
	strPtr += 5; // skip digits and hyphen

	for ( size_t i = 0; i < 4; ++i )
		uuid.high_ = uuid.high_ | ( charToHex( strPtr[i] ) << ( 12 - i * 4 ) );
	strPtr += 5; // skip digits and hyphen

	for ( size_t i = 0; i < 4; ++i )
		uuid.low_ = uuid.low_ | ( charToHex( strPtr[i] ) << ( 60 - i * 4 ) );
	strPtr += 5; // skip digits and hyphen

	for ( size_t i = 0; i < 12; ++i )
		uuid.low_ = uuid.low_ | ( charToHex( strPtr[i] ) << ( 44 - i * 4 ) );

	return uuid;
}
