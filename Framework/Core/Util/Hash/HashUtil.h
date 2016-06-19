#pragma once

#include "MurmurHash3.h"

namespace spad
{
	// https://primes.utm.edu/lists/small/10000.txt
	//

	inline uint32_t MurmurHash3( const void * key, int len, uint32_t seed = 1973 )
	{
		uint32_t res;
		MurmurHash3_x86_32( key, len, seed, &res );
		return res;
	}
} // namespace spad
