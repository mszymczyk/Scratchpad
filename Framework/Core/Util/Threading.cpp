#include "Util_pch.h"
#include "Threading.h"

namespace spad
{


void ParallelFor( ParallelForProc proc, void* arg, size_t first, size_t count, bool multithreaded )
{
	if ( count < 1 )
		return;

	if ( !multithreaded || count == 1 )
	{
		// do it single-threaded
		for ( size_t i = first; i < first + count; ++i )
			proc( i, arg );
	}
	else
	{
		concurrency::parallel_for( first, first + count, [&](size_t index ) {
			proc( index, arg );
		} );
	}
}


} // namespace spad
