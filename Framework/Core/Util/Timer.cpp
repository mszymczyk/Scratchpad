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
	FR_ASSERT(timeQuery.queryStarted_);

	LARGE_INTEGER ct;
	QueryPerformanceCounter(&ct);
	timeQuery.stopTicks_ = ct.QuadPart;

	unsigned __int64 durUS = ((unsigned __int64)(timeQuery.stopTicks_ - timeQuery.startTicks_)) * 1000000;
	durUS /= _gQPF;

	timeQuery.durationUS_ = durUS;
	timeQuery.queryStarted_ = false;
	timeQuery.queryFinished_ = true;
}

}