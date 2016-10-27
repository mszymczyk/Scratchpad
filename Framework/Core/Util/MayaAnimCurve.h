#pragma once

// modified animEngine.h and animEngine.c from maya sdk
// simplified kTangentFixed (not used in ATF)
// thread safe curve evaluation

//-
// ==========================================================================
// Copyright 1995,2006,2008 Autodesk, Inc. All rights reserved.
//
// Use of this software is subject to the terms of the Autodesk
// license agreement provided at the time of installation or download,
// or which otherwise accompanies this software in either electronic
// or hard copy form.
// ==========================================================================
//+

namespace spad
{

/* Common types =========================================================== */
typedef int EtInt;			/* natural int representation					*/
typedef float EtFloat;		/* natural float representation					*/
typedef void EtVoid;		/* void											*/
typedef char EtBoolean;		/* boolean										*/
typedef unsigned char EtByte;		/* 1 byte								*/

/* Anim Engine ============================================================ */
typedef float EtTime;		/* type used for key times (in seconds)			*/
typedef float EtValue;		/* type used for key values (in internal units)	*/


/* tangent types */
enum EtTangentType : uint8_t
{
	kTangentFixed,
	kTangentLinear,
	kTangentFlat,
	kTangentStep,
	kTangentStepNext,
	kTangentSlow,
	kTangentFast,
	kTangentSmooth,
	kTangentClamped,
	kTangentPlateau,
	kTangentAuto
};
typedef enum EtTangentType EtTangentType;


/* a heavy-weight structure to read a key */
struct EtReadKey
{
	EtTime			time = 0;
	EtValue			value = 0;
	// inAngle, inWeightX, inWeightY
	// outAngle, outWeightX, outWeightY
	// are used only when tangent is set to kTangentFixed, all zeros are reasonable values and are used in ATF CurveEditor
	//EtFloat			inAngle = 0;
	//EtFloat			inWeightX = 0;
	//EtFloat			inWeightY = 0;
	//EtFloat			outAngle = 0;
	//EtFloat			outWeightX = 0;
	//EtFloat			outWeightY = 0;
	EtTangentType	inTangentType = kTangentSmooth;
	EtTangentType	outTangentType = kTangentSmooth;
};
typedef struct EtReadKey EtReadKey;

struct EtKey
{
	EtTime	time = 0;			/* key time (in seconds)						*/
	EtValue	value = 0;			/* key value (in internal units)				*/
	EtFloat	inTanX = 0;			/* key in-tangent x value						*/
	EtFloat	inTanY = 0;			/* key in-tangent y value						*/
	EtFloat	outTanX = 0;		/* key out-tangent x value						*/
	EtFloat	outTanY = 0;		/* key out-tangent y value						*/
};
typedef struct EtKey EtKey;

enum EtInfinityType : uint8_t
{
	kInfinityConstant,
	kInfinityLinear,
	kInfinityCycle,
	kInfinityCycleRelative,
	kInfinityOscillate
};
typedef enum EtInfinityType EtInfinityType;

struct EtCurve {
	EtInt		numKeys = 0;	/* The number of keys in the anim curve			*/
	EtBoolean	isWeighted = false;	/* whether or not this curve has weighted tangents */
	EtBoolean	isStatic = false;	/* whether or not all the keys have the same value */
	EtInfinityType preInfinity = kInfinityConstant;		/* how to evaluate pre-infinity			*/
	EtInfinityType postInfinity = kInfinityConstant;	/* how to evaluate post-infinity		*/

	/* evaluate cache - moved to separate structure for thread safe evaluation */
	EtKey*		keyList = nullptr;
};
typedef struct EtCurve EtCurve;

struct EtCurveEvalCache
{
	EtCurveEvalCache()
	{
		fCoeff[0] = fCoeff[1] = fCoeff[2] = fCoeff[3] = 0;
		fPolyY[0] = fPolyY[1] = fPolyY[2] = fPolyY[3] = 0;
	}

	EtKey *		lastKey = nullptr;	/* lastKey evaluated							*/
	EtInt		lastIndex = -1;	/* last index evaluated							*/
	EtInt		lastInterval = -1;	/* last interval evaluated					*/
	EtBoolean	isStep = false;		/* whether or not this interval is a step interval */
	EtBoolean	isStepNext = false;		/* whether or not this interval is a step interval */
	EtBoolean	isLinear = false;	/* whether or not this interval is linear		*/
	EtValue		fX1 = 0;		/* start x of the segment						*/
	EtValue		fX4 = 0;		/* end x of the segment							*/
	EtValue		fCoeff[4];	/* bezier x parameters (only used for weighted curves */
	EtValue		fPolyY[4];	/* bezier y parameters							*/
};
typedef struct EtCurveEvalCache EtCurveEvalCache;

class MayaAnimCurve
{
public:
	MayaAnimCurve( const EtReadKey* keys, const size_t numKeys, EtInfinityType preInfinity, EtInfinityType postInfinity, bool isWeighted = false )
	{
		_Init( keys, numKeys, preInfinity, postInfinity, isWeighted );
	}

	MayaAnimCurve( std::vector<EtKey>&& keys, EtInfinityType preInfinity, EtInfinityType postInfinity, bool isWeighted = false )
		: keyList_( std::move(keys) )
	{
		curve_.numKeys = (EtInt)keyList_.size();
		curve_.isWeighted = isWeighted;
		curve_.isStatic = false;
		curve_.preInfinity = preInfinity;
		curve_.postInfinity = postInfinity;
		curve_.keyList = &keyList_[0];
	}

	// user must provide cache to use for evaluation
	// use same context each time if possible
	float evaluate( float time, EtCurveEvalCache* evalCache ) const;

private:
	void _Init( const EtReadKey* keys, const size_t numKeys, EtInfinityType preInfinity, EtInfinityType postInfinity, bool isWeighted = false );

private:
	EtCurve curve_;
	std::vector<EtKey> keyList_;
};

} // namespace spad
