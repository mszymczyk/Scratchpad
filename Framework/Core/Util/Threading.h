#pragma once
#include <ppl.h>

namespace spad
{


typedef int( *ParallelForProc )( size_t index, void* userPtr );

void ParallelFor( ParallelForProc proc, void* arg, size_t first, size_t count, bool multithreaded );

inline u32 GetNumHardwareThreads()
{
	return std::thread::hardware_concurrency();
}

// If multiple exceptions occur simultaneously in a parallel loop body, the runtime propagates only one of the exceptions to the thread that called parallel_for.
// In addition, when one loop iteration throws an exception, the runtime does not immediately stop the overall loop.
// Instead, the loop is placed in the canceled state and the runtime discards any tasks that have not yet started.
template<typename Proc>
int ParallelFor( size_t first, size_t count, int maxThreadsUser /*= -1 */, Proc proc )
{
	if ( count < 1 )
		return -1;

	bool doSingleThreaded = true;
	if ( maxThreadsUser == 0 || maxThreadsUser == 1 || count == 1 )
	{
		// do single-threaded
		//
	}
	else
	{
		size_t nHardwareThreads = std::thread::hardware_concurrency();
		size_t nThreads;
		if ( maxThreadsUser < 0 )
			nThreads = nHardwareThreads;
		else
			nThreads = std::min( nHardwareThreads, (size_t)maxThreadsUser );

		if ( nThreads > 1 )
		{
			const size_t nMaxThreads = 32;

			nThreads = std::min( nThreads, count );
			nThreads = std::min( nThreads, nMaxThreads );
			nThreads -= 1; // will use this thread as a worker too

			doSingleThreaded = false;

			#ifdef _MSC_VER
			#pragma warning(push)
			#pragma warning(disable:4324)
			#endif //
			
			struct _ParallelFor2Ctx
			{
				_ParallelFor2Ctx()
					: atomicVal( 0 )
					, exceptionFlag_( 0 )
				{	}

				SPAD_ALIGNMENT( SPAD_CACHELINE_SIZE ) std::atomic<size_t> atomicVal;
				SPAD_ALIGNMENT( SPAD_CACHELINE_SIZE ) std::atomic<bool> exceptionFlag_;
				std::exception_ptr exception_; // first exception that was triggered
				size_t first;
				size_t count;
				int err;
			};
			
			#ifdef _MSC_VER
			#pragma warning(pop)
			#endif //

			//auto deleter = []( _ParallelFor2Ctx* ctx ) { spadFreeAligned( ctx ); };
			//std::unique_ptr < _ParallelFor2Ctx, decltype( deleter ) > ctx( new( spadMallocAligned( sizeof( _ParallelFor2Ctx ), SPAD_CACHELINE_SIZE ) ) _ParallelFor2Ctx(), deleter );
			_ParallelFor2Ctx ctxOnStack;
			_ParallelFor2Ctx* ctx = &ctxOnStack;
			ctx->atomicVal = first;
			ctx->first = first;
			ctx->count = count;
			ctx->err = 0;

			auto thread_proc = [&]
			{
				size_t lastIndex = ctx->first + ctx->count;
				for ( ;; )
				{
					if ( ctx->exceptionFlag_.load() || ctx->err )
						return;

					size_t index = ctx->atomicVal.fetch_add( 1 );
					if ( index >= lastIndex )
						return;

					try
					{
						//int ires = ctx->proc( index, ctx->userPtr );
						int ires = proc( index );
						if ( ires )
						{
							ctx->err = ires;
							return;
						}
					}
					catch ( ... )
					{
						bool testVal = false;
						if ( ctx->exceptionFlag_.compare_exchange_strong( testVal, true ) )
						{
							ctx->exception_ = std::current_exception();
						}
					}
				}
			};

			//std::vector<std::thread> threads( nThreads );
			std::thread threads[nMaxThreads];
			for ( size_t i = 0; i < nThreads; ++i )
			{
				//std::thread t( thread_proc );
				//threads.emplace_back( std::move( t ) );
				threads[i] = std::thread( thread_proc );
			}

			// do processing also in this thread
			thread_proc();

			//std::for_each( threads.begin(), threads.end(), std::mem_fn( &std::thread::join ) );
			for ( size_t i = 0; i < nThreads; ++i )
				threads[i].join();

			// re throw first exception
			if ( ctx->exceptionFlag_.load() )
				std::rethrow_exception( ctx->exception_ );

			return ctx->err;
		}
	}

	if ( doSingleThreaded )
	{
		for ( size_t i = first; i < first + count; ++i )
		{
			//int ires = proc( i, arg );
			int ires = proc( i );
			if ( ires )
				return ires;
		}
	}

	return 0;
}


template<class Proc>
inline void ParallelFor_threadPool( size_t first, size_t count, bool multithreaded, Proc proc )
{
	if ( count < 1 )
		return;

	if ( !multithreaded || count == 1 )
	{
		// do it single-threaded
		for ( size_t i = first; i < first + count; ++i )
			proc( i );
	}
	else
	{
		concurrency::parallel_for( first, first + count, [&]( size_t index ) {
			proc( index );
		} );
	}
}


template<class Cont, class Proc>
inline void ParallelFor_threadPool( const Cont& container, bool multithreaded, Proc proc )
{
	const size_t count = container.size();
	if ( count < 1 )
		return;

	const size_t first = 0;

	if ( !multithreaded || count == 1 )
	{
		// do it single-threaded
		for ( size_t i = first; i < first + count; ++i )
			proc( i );
	}
	else
	{
		concurrency::parallel_for( first, first + count, [&]( size_t index ) {
			proc( index );
		} );
	}
}


} // namespace spad
