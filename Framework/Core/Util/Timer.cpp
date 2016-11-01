#include "Util_pch.h"
#include "Timer.h"
#include "Exceptions.h"

namespace spad
{

static i64 getQueryPerformanceFrequency()
{
	// Query for the performance counter frequency
	// https://msdn.microsoft.com/en-us/library/ee417693.aspx
	// frequency won't change for the duration of a program
	//
	LARGE_INTEGER tf;
	Win32Call(QueryPerformanceFrequency(&tf));
	return tf.QuadPart;
}

static i64 _gQPF = getQueryPerformanceFrequency();


Timer::Timer()
{
    frequency_ = _gQPF;
    frequencyD_ = static_cast<double>(frequency_);
	frequencyDRcp_ = 1.0 / frequencyD_;

    // Init the elapsed time
	LARGE_INTEGER largeInt;
	Win32Call(QueryPerformanceCounter(&largeInt));
    startTicks_ = largeInt.QuadPart;

    elapsedTicks_ = 0;
	deltaTicks_ = 0;

    elapsedSecondsD_ = 0;
	deltaSecondsD_ = 0;
	deltaSecondsF_ = 0;

    elapsedMilliseconds_ = 0;
	deltaMilliseconds_ = 0;

	elapsedMicroseconds_ = 0;
    deltaMicroseconds_ = 0;
}

Timer::~Timer()
{
}

void Timer::Update()
{
    LARGE_INTEGER largeInt;
    Win32Call(QueryPerformanceCounter(&largeInt));
    i64 currentTicks = largeInt.QuadPart - startTicks_;

	deltaTicks_ = currentTicks - elapsedTicks_;
	elapsedTicks_ = currentTicks;

	elapsedSecondsD_ = elapsedTicks_ * frequencyDRcp_;
    deltaSecondsD_ = deltaTicks_ * frequencyDRcp_;
    deltaSecondsF_ = static_cast<float>(deltaSecondsD_);

	elapsedMilliseconds_ = static_cast<i64>(elapsedSecondsD_ * 1000);
	deltaMilliseconds_ = static_cast<i64>(deltaSecondsD_ * 1000);

	elapsedMicroseconds_ = static_cast<i64>(elapsedSecondsD_ * (1000 * 1000) );
	deltaMicroseconds_ = static_cast<i64>(deltaSecondsD_ * (1000 * 1000) );
}

void BeginCpuTimeQuery(CpuTimeQuery& timeQuery)
{
	LARGE_INTEGER ct;
	QueryPerformanceCounter(&ct);
	timeQuery.startTicks_ = ct.QuadPart;
	timeQuery.queryStarted_ = true;
	timeQuery.queryFinished_ = false;
}

void EndCpuTimeQuery(CpuTimeQuery& timeQuery)
{
	SPAD_ASSERT(timeQuery.queryStarted_);

	LARGE_INTEGER ct;
	QueryPerformanceCounter(&ct);
	timeQuery.stopTicks_ = ct.QuadPart;

	unsigned __int64 durUS = ((unsigned __int64)(timeQuery.stopTicks_ - timeQuery.startTicks_)) * 1000000;
	durUS /= _gQPF;

	timeQuery.durationUS_ = durUS;
	timeQuery.queryStarted_ = false;
	timeQuery.queryFinished_ = true;
}

void ConvertTime( u64 microSeconds, u32* dstMilliseconds, u32* dstSeconds, u32* dstMinutes, u32* dstHours )
{
	u32 timeMS = (u32)( microSeconds / 1000 );
	u32 seconds = ( timeMS / 1000 ) % 60;
	u32 minutes = ( seconds / 60 ) % 60;

	if ( dstMilliseconds )
		*dstMilliseconds = timeMS % 1000;
	if ( dstSeconds )
		*dstSeconds = seconds;
	if ( dstMinutes )
		*dstMinutes = minutes;
	if ( dstHours )
		*dstHours = minutes / 60;
}

static void _ConvertToTwoDigits( u32 t, char* textBuf )
{
	SPAD_ASSERT( t >= 0 && t <= 99 );
	int a = t / 10;
	int b = t % 10;
	textBuf[0] = (char)( '0' + a );
	textBuf[1] = (char)( '0' + b );
}

static void _ConvertToThreeDigits( u32 milliseconds, char* textBuf )
{
	SPAD_ASSERT( milliseconds >= 0 && milliseconds <= 999 );
	int a = milliseconds / 100;
	int t2 = milliseconds % 100;
	int b = t2 / 10;
	int c = t2 % 10;

	textBuf[0] = (char)( '0' + a );
	textBuf[1] = (char)( '0' + b );
	textBuf[2] = (char)( '0' + c );
}

std::string FormatTime( u64 microSeconds )
{
	u32 ms, s, m, h;
	ConvertTime( microSeconds, &ms, &s, &m, &h );

	std::string ret;
	ret.resize( 12 );
	char* p = const_cast<char*>( ret.c_str() );
	_ConvertToTwoDigits( h, p + 0 );
	p[2] = ':';
	_ConvertToTwoDigits( m, p + 3 );
	p[5] = ':';
	_ConvertToTwoDigits( s, p + 6 );
	p[8] = '.';
	_ConvertToThreeDigits( ms, p + 9 );

	return ret;
}

}