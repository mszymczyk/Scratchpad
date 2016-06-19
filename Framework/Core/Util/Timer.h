#pragma once

namespace spad
{

class Timer
{

public:

    Timer();
    ~Timer();

    void Update();

	double getDeltaSecondsD() const {
		return deltaSecondsD_;
	}
	float getDeltaSeconds() const {
		return deltaSecondsF_;
	}

protected:

    i64 frequency_;
    double frequencyD_;
	double frequencyDRcp_;

	i64 startTicks_;

    i64 elapsedTicks_;
	i64 deltaTicks_;

    double elapsedSecondsD_;
    double deltaSecondsD_;
	float deltaSecondsF_;
	// 4 bytes padding

    i64 elapsedMilliseconds_;
    i64 deltaMilliseconds_;

    i64 elapsedMicroseconds_;
    i64 deltaMicroseconds_;
};

class FpsCounter
{
public:
	FpsCounter(float timePeriod = 0.5f)
		: timePeriod_( std::max(timePeriod, 0.0f) )
	{	}

	void update(const Timer& timer)
	{
		timeElapsed_ += timer.getDeltaSeconds();
		nFrames_ += 1;
		if (timeElapsed_ >= timePeriod_)
		{
			frameRate_ = nFrames_ / timeElapsed_;
			timeElapsed_ = 0;
			nFrames_ = 0;
		}
	}

	float getFrameRate() const {
		return frameRate_;
	}

private:
	const float timePeriod_;
	float timeElapsed_ = 0;
	u32 nFrames_ = 0;
	float frameRate_ = 0;
};

struct CpuTimeQuery
{
	CpuTimeQuery()
		: startTicks_(0)
		, stopTicks_(0)
		, durationUS_(0)
		, queryStarted_(false)
		, queryFinished_(false)
	{	}

	uint64_t startTicks_;
	uint64_t stopTicks_;
	uint64_t durationUS_;
	bool queryStarted_;
	bool queryFinished_;
};

void BeginCpuTimeQuery(CpuTimeQuery& timeQuery);
void EndCpuTimeQuery(CpuTimeQuery& timeQuery);


}