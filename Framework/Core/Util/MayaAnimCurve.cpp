//-
// ==========================================================================
// Copyright 2015 Autodesk, Inc.  All rights reserved.
//
// Use of this software is subject to the terms of the Autodesk
// license agreement provided at the time of installation or download,
// or which otherwise accompanies this software in either electronic
// or hard copy form.
// ==========================================================================
//+

#include <Util_pch.h>
#include "MayaAnimCurve.h"

#if defined(_MSC_VER) && defined(_DEBUG)
#define new _DEBUG_NEW
#endif

namespace spad
{

/* Common types =========================================================== */
#define kEngineFloatMax (FLT_MAX)	/* Opposite of 0						*/
#define kEngineNULL (0)		/* NULL											*/
#define kEngineTRUE ((EtBoolean)1)
#define kEngineFALSE ((EtBoolean)0)


/* local constants */
#define kDegRad 0.0174532925199432958f
#define kFourThirds (4.0f / 3.0f)
#define kTwoThirds (2.0f / 3.0f)
#define kOneThird (1.0f / 3.0f)
#define kMaxTan 5729577.9485111479f
//

EtValue engineAnimEvaluate (const EtCurve *animCurve, EtCurveEvalCache* animCurveEvalCache, EtTime time);

/*
// statics for sMachineTolerance computation
*/
static const EtValue sMachineTolerance = 0.000001f; // orig code lazy calculated this guy

/*
//	Description:
//		We want to ensure that (x1, x2) is inside the ellipse
//		(x1^2 + x2^2 - 2(x1 +x2) + x1*x2 + 1) given that we know
//		x1 is within the x bounds of the ellipse.
*/
static EtVoid
constrainInsideBounds (EtValue *x1, EtValue *x2)
{
	EtValue b, c,  discr,  root;

	if ((*x1 + sMachineTolerance) < kFourThirds) {
		b = *x1 - 2.0f;
		c = *x1 - 1.0f;
		discr = sqrtf (b * b - 4 * c * c);
		root = (-b + discr) * 0.5f;
		if ((*x2 + sMachineTolerance) > root) {
			*x2 = root - sMachineTolerance;
		}
		else {
			root = (-b - discr) * 0.5f;
			if (*x2 < (root + sMachineTolerance)) {
				*x2 = root + sMachineTolerance;
			}
		}
	}
	else {
		*x1 = kFourThirds - sMachineTolerance;
		*x2 = kOneThird - sMachineTolerance;
	}
}

/*
//	Description:
//
//		Given the bezier curve
//			 B(t) = [t^3 t^2 t 1] * | -1  3 -3  1 | * | 0  |
//									|  3 -6  3  0 |   | x1 |
//									| -3  3  0  0 |   | x2 |
//									|  1  0  0  0 |   | 1  |
//
//		We want to ensure that the B(t) is a monotonically increasing function.
//		We can do this by computing
//			 B'(t) = [3t^2 2t 1 0] * | -1  3 -3  1 | * | 0  |
//									 |  3 -6  3  0 |   | x1 |
//									 | -3  3  0  0 |   | x2 |
//									 |  1  0  0  0 |   | 1  |
//
//		and finding the roots where B'(t) = 0.  If there is at most one root
//		in the interval [0, 1], then the curve B(t) is monotonically increasing.
//
//		It is easier if we use the control vector [ 0 x1 (1-x2) 1 ] since
//		this provides more symmetry, yields better equations and constrains
//		x1 and x2 to be positive.
//
//		Therefore:
//			 B'(t) = [3t^2 2t 1 0] * | -1  3 -3  1 | * | 0    |
//									 |  3 -6  3  0 |   | x1   |
//									 | -3  3  0  0 |   | 1-x2 |
//									 |  1  0  0  0 |   | 1    |
//
//				   = [t^2 t 1 0] * | 3*(3*x1 + 3*x2 - 2)  |
//								   | 2*(-6*x1 - 3*x2 + 3) |
//								   | 3*x1                 |
//								   | 0                    |
//
//		gives t = (2*x1 + x2 -1) +/- sqrtf(x1^2 + x2^2 + x1*x2 - 2*(x1 + x2) + 1)
//				  --------------------------------------------------------------
//								3*x1 + 3* x2 - 2
//
//		If the ellipse [x1^2 + x2^2 + x1*x2 - 2*(x1 + x2) + 1] <= 0, (Note
//		the symmetry) x1 and x2 are valid control values and the curve is
//		monotonic.  Otherwise, x1 and x2 are invalid and have to be projected
//		onto the ellipse.
//
//		It happens that the maximum value that x1 or x2 can be is 4/3.
//		If one of the values is less than 4/3, we can determine the
//		boundary constraints for the other value.
*/
static EtVoid
checkMonotonic (EtValue *x1, EtValue *x2)
{
	EtValue d;

	/*
	// We want a control vector of [ 0 x1 (1-x2) 1 ] since this provides
	// more symmetry. (This yields better equations and constrains x1 and x2
	// to be positive.)
	*/
	*x2 = 1.0f - *x2;

	/* x1 and x2 must always be positive */
	if (*x1 < 0.0f) *x1 = 0.0f;
	if (*x2 < 0.0f) *x2 = 0.0f;

	/*
	// If x1 or x2 are greater than 1.0f, then they must be inside the
	// ellipse (x1^2 + x2^2 - 2(x1 +x2) + x1*x2 + 1).
	// x1 and x2 are invalid if x1^2 + x2^2 - 2(x1 +x2) + x1*x2 + 1 > 0.0f
	*/
	if ((*x1 > 1.0f) || (*x2 > 1.0f)) {
		d = *x1 * (*x1 - 2.0f + *x2) + *x2 * (*x2 - 2.0f) + 1.0f;
		if ((d + sMachineTolerance) > 0.0f) {
			constrainInsideBounds (x1, x2);
		}
	}

	*x2 = 1.0f - *x2;
}

/*
//	Description:
//		Convert the control values for a polynomial defined in the Bezier
//		basis to a polynomial defined in the power basis (t^3 t^2 t 1).
*/
static EtVoid
bezierToPower (
	EtValue a1, EtValue b1, EtValue c1, EtValue d1,
	EtValue *a2, EtValue *b2, EtValue *c2, EtValue *d2)
{
	EtValue a = b1 - a1;
	EtValue b = c1 - b1;
	EtValue c = d1 - c1;
	EtValue d = b - a;
	*a2 = c - b - d;
	*b2 = d + d + d;
	*c2 = a + a + a;
	*d2 = a1;
}

/*
//   Evaluate a polynomial in array form ( value only )
//   input:
//      P               array 
//      deg             degree
//      s               parameter
//   output:
//      ag_horner1      evaluated polynomial
//   process: 
//      ans = sum (i from 0 to deg) of P[i]*s^i
//   restrictions: 
//      deg >= 0           
*/
static EtValue
ag_horner1 (EtValue P[], EtInt deg, EtValue s)
{
	EtValue h = P[deg];
	while (--deg >= 0) h = (s * h) + P[deg];
	return (h);
}

typedef struct ag_polynomial {
	EtValue *p;
	EtInt deg;
} AG_POLYNOMIAL;

/*
//	Description
//   Compute parameter value at zero of a function between limits
//       with function values at limits
//   input:
//       a, b      real interval
//       fa, fb    double values of f at a, b
//       f         real valued function of t and pars
//       tol       tolerance
//       pars      pointer to a structure
//   output:
//       ag_zeroin2   a <= zero of function <= b
//   process:
//       We find the zeroes of the function f(t, pars).  t is
//       restricted to the interval [a, b].  pars is passed in as
//       a pointer to a structure which contains parameters
//       for the function f.
//   restrictions:
//       fa and fb are of opposite sign.
//       Note that since pars comes at the end of both the
//       call to ag_zeroin and to f, it is an optional parameter.
*/
static EtValue
ag_zeroin2 (EtValue a, EtValue b, EtValue fa, EtValue fb, EtValue tol, AG_POLYNOMIAL *pars)
{
	EtInt test;
	EtValue c, d, e, fc, del, m, machtol, p, q, r, s;

	/* initialization */
	machtol = sMachineTolerance;

	/* start iteration */
label1:
	c = a;  fc = fa;  d = b-a;  e = d;
label2:
	if (fabs(fc) < fabs(fb)) {
		a = b;   b = c;   c = a;   fa = fb;   fb = fc;   fc = fa;
	}

	/* convergence test */
	del = 2.0f * machtol * fabs(b) + 0.5f*tol;
	m = 0.5f * (c - b);
	test = ((fabs(m) > del) && (fb != 0.0f));
	if (test) {
		if ((fabs(e) < del) || (fabs(fa) <= fabs(fb))) {
			/* bisection */
			d = m;  e= d;
		}
		else {
			s = fb / fa;
			if (a == c) {
				/* linear interpolation */
				p = 2.0f*m*s;    q = 1.0f - s;
			}
			else {
				/* inverse quadratic interpolation */
				q = fa/fc;
				r = fb/fc;
				p = s*(2.0f*m*q*(q-r)-(b-a)*(r-1.0f));
				q = (q-1.0f)*(r-1.0f)*(s-1.0f);
			}
			/* adjust the sign */
			if (p > 0.0f) q = -q;  else p = -p;
			/* check if interpolation is acceptable */
			s = e;   e = d;
			if ((2.0f*p < 3.0*m*q-fabs(del*q))&&(p < fabs(0.5f*s*q))) {
				d = p/q;
			}
			else {
				d = m;	e = d;
			}
		}
		/* complete step */
		a = b;	fa = fb;
		if ( fabs(d) > del )   b += d;
		else if (m > 0.0f) b += del;  else b -= del;
		fb = ag_horner1 (pars->p, pars->deg, b);
		if (fb*(fc/fabs(fc)) > 0.0f ) {
			goto label1;
		}
		else {
			goto label2;
		}
	}
	return (b);
}

/*
//	Description:
//   Compute parameter value at zero of a function between limits
//   input:
//       a, b            real interval
//       f               real valued function of t and pars
//       tol             tolerance
//       pars            pointer to a structure
//   output:
//       ag_zeroin       zero of function
//   process:
//       Call ag_zeroin2 to find the zeroes of the function f(t, pars).
//       t is restricted to the interval [a, b].
//       pars is passed in as a pointer to a structure which contains
//       parameters for the function f.
//   restrictions:
//       f(a) and f(b) are of opposite sign.
//       Note that since pars comes at the end of both the
//         call to ag_zeroin and to f, it is an optional parameter.
//       If you already have values for fa,fb use ag_zeroin2 directly
*/
static EtValue
ag_zeroin (EtValue a, EtValue b, EtValue tol, AG_POLYNOMIAL *pars)
{
	EtValue fa, fb;

	fa = ag_horner1 (pars->p, pars->deg, a);
	if (fabs(fa) < sMachineTolerance) return(a);

	fb = ag_horner1 (pars->p, pars->deg, b);
	if (fabs(fb) < sMachineTolerance) return(b);

	return (ag_zeroin2 (a, b, fa, fb, tol, pars));
} 

/*
// Description:
//   Find the zeros of a polynomial function on an interval
//   input:
//       Poly                 array of coefficients of polynomial
//       deg                  degree of polynomial
//       a, b                 interval of definition a < b
//       a_closed             include a in interval (TRUE or FALSE)
//       b_closed             include b in interval (TRUE or FALSE)
//   output: 
//       polyzero             number of roots 
//                            -1 indicates Poly == 0.0f
//       Roots                zeroes of the polynomial on the interval
//   process:
//       Find all zeroes of the function on the open interval by 
//       recursively finding all of the zeroes of the derivative
//       to isolate the zeroes of the function.  Return all of the 
//       zeroes found adding the end points if the corresponding side
//       of the interval is closed and the value of the function 
//       is indeed 0 there.
//   restrictions:
//       The polynomial p is simply an array of deg+1 doubles.
//       p[0] is the constant term and p[deg] is the coef 
//       of t^deg.
//       The array roots should be dimensioned to deg+2. If the number
//       of roots returned is greater than deg, suspect numerical
//       instabilities caused by some nearly flat portion of Poly.
*/
static EtInt
polyZeroes (EtValue Poly[], EtInt deg, EtValue a, EtInt a_closed, EtValue b, EtInt b_closed, EtValue Roots[])
{
	EtInt i, left_ok, right_ok, nr, ndr, skip;
	EtValue e, f, s, pe, ps, tol, *p, p_x[22], *d, d_x[22], *dr, dr_x[22];
	AG_POLYNOMIAL ply;

	e = pe = 0.0f;  
	f = 0.0f;

	for (i = 0 ; i < deg + 1; ++i) {
		f += fabs(Poly[i]);
	}
	tol = (fabs(a) + fabs(b))*(deg+1)*sMachineTolerance;

	/* Zero polynomial to tolerance? */
	if (f <= tol)  return(-1);

	p = p_x;  d = d_x;  dr = dr_x;
	for (i = 0 ; i < deg + 1; ++i) {
		p[i] = 1.0f/f * Poly[i];
	}

	/* determine true degree */
	while ( fabs(p[deg]) < tol) deg--;

	/* Identically zero poly already caught so constant fn != 0 */
	nr = 0;
	if (deg == 0) return (nr);

	/* check for linear case */
	if (deg == 1) {
		Roots[0] = -p[0] / p[1];
		left_ok  = (a_closed) ? (a<Roots[0]+tol) : (a<Roots[0]-tol);
		right_ok = (b_closed) ? (b>Roots[0]-tol) : (b>Roots[0]+tol);
		nr = (left_ok && right_ok) ? 1 : 0;
		if (nr) {
			if (a_closed && Roots[0]<a) Roots[0] = a;
			else if (b_closed && Roots[0]>b) Roots[0] = b;
		}
		return (nr);
	}
	/* handle non-linear case */
	else {
		ply.p = p;  ply.deg = deg;

		/* compute derivative */
		for (i=1; i<=deg; i++) d[i-1] = i*p[i];

		/* find roots of derivative */
		ndr = polyZeroes ( d, deg-1, a, 0, b, 0, dr );
		if (ndr == -1) return (0);

		/* find roots between roots of the derivative */
		for (i=skip=0; i<=ndr; i++) {
			if (nr>deg) return (nr);
			if (i==0) {
				s=a; ps = ag_horner1( p, deg, s);
				if ( fabs(ps)<=tol && a_closed) Roots[nr++]=a;
			}
			else { s=e; ps=pe; }
			if (i==ndr) { e = b; skip = 0;}
			else e=dr[i];
			pe = ag_horner1( p, deg, e );
			if (skip) skip = 0;
			else {
				if ( fabs(pe) < tol ) {
					if (i!=ndr || b_closed) {
						Roots[nr++] = e;
						skip = 1;
					}
				}
				else if ((ps<0 && pe>0)||(ps>0 && pe<0)) {
					Roots[nr++] = ag_zeroin(s, e, 0.0f, &ply );
					if ((nr>1) && Roots[nr-2]>=Roots[nr-1]-tol) { 
						Roots[nr-2] = (Roots[nr-2]+Roots[nr-1]) * 0.5f;
						nr--;
					}
				}
			}
		}
	}

	return (nr);
} 

/*
//	Description:
//		Create a constrained single span cubic 2d bezier curve using the
//		specified control points.  The curve interpolates the first and
//		last control point.  The internal two control points may be
//		adjusted to ensure that the curve is monotonic.
*/
static EtVoid
engineBezierCreate ( const EtCurve *animCurve, EtCurveEvalCache* animCurveEvalCache, EtValue x[4], EtValue y[4])
{
	//static EtBoolean sInited = kEngineFALSE;
	EtValue rangeX, dx1, dx2, nX1, nX2, oldX1, oldX2;

	//if (!sInited) {
	//    init_tolerance ();
	//    sInited = kEngineTRUE;
	//}

	if (animCurve == kEngineNULL) {
		return;
	}
	rangeX = x[3] - x[0];
	if (rangeX == 0.0f) {
		return;
	}
	dx1 = x[1] - x[0];
	dx2 = x[2] - x[0];

	/* normalize X control values */
	nX1 = dx1 / rangeX;
	nX2 = dx2 / rangeX;

	/* if all 4 CVs equally spaced, polynomial will be linear */
	if ((nX1 == kOneThird) && (nX2 == kTwoThirds)) {
		animCurveEvalCache->isLinear = kEngineTRUE;
	} else {
		animCurveEvalCache->isLinear = kEngineFALSE;
	}

	/* save the orig normalized control values */
	oldX1 = nX1;
	oldX2 = nX2;

	/*
	// check the inside control values yield a monotonic function.
	// if they don't correct them with preference given to one of them.
	//
	// Most of the time we are monotonic, so do some simple checks first
	*/
	if (nX1 < 0.0f) nX1 = 0.0f;
	if (nX2 > 1.0f) nX2 = 1.0f;
	if ((nX1 > 1.0f) || (nX2 < -1.0f)) {
		checkMonotonic (&nX1, &nX2);
	}

	/* compute the new control points */
	if (nX1 != oldX1) {
		x[1] = x[0] + nX1 * rangeX;
		if (oldX1 != 0.0f) {
			y[1] = y[0] + (y[1] - y[0]) * nX1 / oldX1;
		}
	}
	if (nX2 != oldX2) {
		x[2] = x[0] + nX2 * rangeX;
		if (oldX2 != 1.0f) {
			y[2] = y[3] - (y[3] - y[2]) * (1.0f - nX2) / (1.0f - oldX2);
		}
	}

	/* save the control points */
	animCurveEvalCache->fX1 = x[0];
	animCurveEvalCache->fX4 = x[3];

	/* convert Bezier basis to power basis */
	bezierToPower (
		0.0f, nX1, nX2, 1.0f,
		&( animCurveEvalCache->fCoeff[3]), &( animCurveEvalCache->fCoeff[2]), &( animCurveEvalCache->fCoeff[1]), &( animCurveEvalCache->fCoeff[0])
	);
	bezierToPower (
		y[0], y[1], y[2], y[3],
		&( animCurveEvalCache->fPolyY[3]), &( animCurveEvalCache->fPolyY[2]), &( animCurveEvalCache->fPolyY[1]), &( animCurveEvalCache->fPolyY[0])
	);
}

/*
// Description:
//		Given the time between fX1 and fX4, return the
//		value of the curve at that time.
*/
static EtValue
engineBezierEvaluate (const EtCurve *animCurve, EtCurveEvalCache* animCurveEvalCache, EtTime time)
{
	EtValue t, s, poly[4], roots[5];
	EtInt numRoots;

	if (animCurve == kEngineNULL) {
		return (0.0f);
	}

	if ( animCurveEvalCache->fX1 == time) {
		s = 0.0f;
	}
	else if ( animCurveEvalCache->fX4 == time) {
		s = 1.0f;
	}
	else {
		s = (time - animCurveEvalCache->fX1) / ( animCurveEvalCache->fX4 - animCurveEvalCache->fX1);
	}

	if ( animCurveEvalCache->isLinear) {
		t = s;
	}
	else {
		poly[3] = animCurveEvalCache->fCoeff[3];
		poly[2] = animCurveEvalCache->fCoeff[2];
		poly[1] = animCurveEvalCache->fCoeff[1];
		poly[0] = animCurveEvalCache->fCoeff[0] - s;

		numRoots = polyZeroes (poly, 3, 0.0f, 1, 1.0f, 1, roots);
		if (numRoots == 1) {
			t = roots[0];
		}
		else {
			t = 0.0f;
		}
	}
	return (t * (t * (t * animCurveEvalCache->fPolyY[3] + animCurveEvalCache->fPolyY[2]) + animCurveEvalCache->fPolyY[1]) + animCurveEvalCache->fPolyY[0]);
}

static EtVoid
engineHermiteCreate (const EtCurve *animCurve, EtCurveEvalCache* animCurveEvalCache, EtValue x[4], EtValue y[4])
{
	EtValue dx, dy, tan_x, m1, m2, length, d1, d2;

	if (animCurve == kEngineNULL) {
		return;
	}

	/* save the control points */
	animCurveEvalCache->fX1 = x[0];

	/*	
	 *	Compute the difference between the 2 keyframes.					
	 */
	dx = x[3] - x[0];
	dy = y[3] - y[0];

	/* 
	 * 	Compute the tangent at the start of the curve segment.			
	 */
	tan_x = x[1] - x[0];
	m1 = m2 = 0.0f;
	if (tan_x != 0.0f) {
		m1 = (y[1] - y[0]) / tan_x;
	}

	tan_x = x[3] - x[2];
	if (tan_x != 0.0f) {
		m2 = (y[3] - y[2]) / tan_x;
	}

	length = 1.0f / (dx * dx);
	d1 = dx * m1;
	d2 = dx * m2;
	animCurveEvalCache->fCoeff[0] = (d1 + d2 - dy - dy) * length / dx;
	animCurveEvalCache->fCoeff[1] = (dy + dy + dy - d1 - d1 - d2) * length;
	animCurveEvalCache->fCoeff[2] = m1;
	animCurveEvalCache->fCoeff[3] = y[0];
}

/*
// Description:
//		Given the time between fX1 and fX2, return the function
//		value of the curve
*/
static EtValue
engineHermiteEvaluate (const EtCurve *animCurve, EtCurveEvalCache* animCurveEvalCache, EtTime time)
{
	EtValue t;
	if (animCurve == kEngineNULL) {
		return (0.0f);
	}
	t = time - animCurveEvalCache->fX1;
	return (t * (t * (t * animCurveEvalCache->fCoeff[0] + animCurveEvalCache->fCoeff[1]) + animCurveEvalCache->fCoeff[2]) + animCurveEvalCache->fCoeff[3]);
}

/*
//	Function Name:
//		evaluateInfinities
//
//	Description:
//		A static helper function to evaluate the infinity portion of an
//	animation curve.  The infinity portion is the parts of the animation
//	curve outside the range of keys.
//
//  Input Arguments:
//		EtCurve *animCurve			The animation curve to evaluate
//		EtTime time					The time (in seconds) to evaluate
//		EtBoolean evalPre
//			kEngineTRUE				evaluate the pre-infinity portion
//			kEngineFALSE			evaluate the post-infinity portion
//
//  Return Value:
//		EtValue value				The evaluated value of the curve at time
*/
static EtValue
evaluateInfinities (const EtCurve *animCurve, EtCurveEvalCache* animCurveEvalCache, EtTime time, EtBoolean evalPre)
{
	EtValue value = 0.0f;
	EtValue	valueRange;
	EtTime	factoredTime, firstTime, lastTime, timeRange;
	EtFloat	remainder, tanX, tanY;
	float numCycles, notUsed;

	/* make sure we have something to evaluate */
	if ((animCurve == kEngineNULL) || (animCurve->numKeys == 0)) {
		return (value);
	}

	/* find the number of cycles of the base animation curve */
	firstTime = animCurve->keyList[0].time;
	lastTime = animCurve->keyList[animCurve->numKeys - 1].time;
	timeRange = lastTime - firstTime;
	if (timeRange == 0.0f) {
		/*
		// Means that there is only one key in the curve.. Return the value
		// of that key..
		*/
		return (animCurve->keyList[0].value);
	}
	if (time > lastTime) {
		remainder = fabs (modf ((time - lastTime) / timeRange, &numCycles));
	}
	else {
		remainder = fabs (modf ((time - firstTime) / timeRange, &numCycles));
	}
	factoredTime = timeRange * remainder;
	numCycles = fabs (numCycles) + 1;

	if (evalPre) {
		/* evaluate the pre-infinity */
		if (animCurve->preInfinity == kInfinityOscillate) {
			if ((remainder = modf (numCycles / 2.0f, &notUsed)) != 0.0f) {
				factoredTime = firstTime + factoredTime;
			}
			else {
				factoredTime = lastTime - factoredTime;
			}
		}
		else if ((animCurve->preInfinity == kInfinityCycle)
		||	(animCurve->preInfinity == kInfinityCycleRelative)) {
			factoredTime = lastTime - factoredTime;
		}
		else if (animCurve->preInfinity == kInfinityLinear) {
			factoredTime = firstTime - time;
			tanX = animCurve->keyList[0].inTanX;
			tanY = animCurve->keyList[0].inTanY;
			value = animCurve->keyList[0].value;
			if (tanX != 0.0f) {
				value -= ((factoredTime * tanY) / tanX);
			}
			return (value);
		}
	}
	else {
		/* evaluate the post-infinity */
		if (animCurve->postInfinity == kInfinityOscillate) {
			if ((remainder = modf (numCycles / 2.0f, &notUsed)) != 0.0f) {
				factoredTime = lastTime - factoredTime;
			}
			else {
				factoredTime = firstTime + factoredTime;
			}
		}
		else if ((animCurve->postInfinity == kInfinityCycle)
		||	(animCurve->postInfinity == kInfinityCycleRelative)) {
			factoredTime = firstTime + factoredTime;
		}
		else if (animCurve->postInfinity == kInfinityLinear) {
			factoredTime = time - lastTime;
			tanX = animCurve->keyList[animCurve->numKeys - 1].outTanX;
			tanY = animCurve->keyList[animCurve->numKeys - 1].outTanY;
			value = animCurve->keyList[animCurve->numKeys - 1].value;
			if (tanX != 0.0f) {
				value += ((factoredTime * tanY) / tanX);
			}
			return (value);
		}
	}

	value = engineAnimEvaluate (animCurve, animCurveEvalCache, factoredTime);

	/* Modify the value if infinityType is cycleRelative */
	if (evalPre && (animCurve->preInfinity == kInfinityCycleRelative)) {
		valueRange = animCurve->keyList[animCurve->numKeys - 1].value -
						animCurve->keyList[0].value;
		value -= (numCycles * valueRange);
	}
	else if (!evalPre && (animCurve->postInfinity == kInfinityCycleRelative)) {
		valueRange = animCurve->keyList[animCurve->numKeys - 1].value -
						animCurve->keyList[0].value;
		value += (numCycles * valueRange);
	}
	return (value);
}

/*
//	Function Name:
//		find
//
//	Description:
//		A static helper method to find a key prior to a specified time
//
//  Input Arguments:
//		EtCurve *animCurve			The animation curve to search
//		EtTime time					The time (in seconds) to find
//		EtInt *index				The index of the key prior to time
//
//  Return Value:
//      EtBoolean result
//			kEngineTRUE				time is represented by an actual key
//										(with the index in index)
//			kEngineFALSE			the index key is the key less than time
//
//	Note:
//		keys are sorted by ascending time, which means we can use a binary
//	search to find the key
*/
static EtBoolean
find (const EtCurve *animCurve, EtTime time, EtInt *index)
{
	/* make sure we have something to search */
	if ((animCurve == kEngineNULL) || (index == kEngineNULL)) {
		return (kEngineFALSE);
	}

	/* use a binary search to find the key */
	*index = 0;
	EtInt len = animCurve->numKeys;
	if (len > 0) {
		EtInt mid, low, high;

		low = 0;
		high = len - 1;
		do {
			mid = (low + high) >> 1;
			if (time < animCurve->keyList[mid].time) {
				high = mid - 1;			/* Search lower half */
			} else if (time > animCurve->keyList[mid].time) {
				low  = mid + 1;			/* Search upper half */
			}
			else {
				*index = mid;	/* Found item! */
				return (kEngineTRUE);
			}
		} while (low <= high);
		*index = low;
	}
	return (kEngineFALSE);
}

/*
//	Function Name:
//		engineAnimEvaluate
//
//	Description:
//		A function to evaluate an animation curve at a specified time
//
//  Input Arguments:
//		EtCurve *animCurve			The animation curve to evaluate
//		EtTime time					The time (in seconds) to evaluate
//
//  Return Value:
//		EtValue value				The evaluated value of the curve at time
*/
EtValue
engineAnimEvaluate (const EtCurve *animCurve, EtCurveEvalCache* animCurveEvalCache, EtTime time)
{
	EtBoolean withinInterval = kEngineFALSE;
	EtKey *nextKey = nullptr;
	EtInt index = -1;
	EtValue value = 0.0f;
	EtValue x[4];
	EtValue y[4];

	/* make sure we have something to evaluate */
	if ((animCurve == kEngineNULL) || (animCurve->numKeys == 0)) {
		return (value);
	}

	/* check if the time falls into the pre-infinity */
	if (time < animCurve->keyList[0].time) {
		if (animCurve->preInfinity == kInfinityConstant) {
			return (animCurve->keyList[0].value);
		}
		return (evaluateInfinities (animCurve, animCurveEvalCache, time, kEngineTRUE));
	}

	/* check if the time falls into the post-infinity */
	if (time > animCurve->keyList[animCurve->numKeys - 1].time) {
		if (animCurve->postInfinity == kInfinityConstant) {
			return (animCurve->keyList[animCurve->numKeys - 1].value);
		}
		return (evaluateInfinities (animCurve, animCurveEvalCache, time, kEngineFALSE));
	}

	/* check if the animation curve is static */
	if (animCurve->isStatic) {
		return (animCurve->keyList[0].value);
	}

	/* check to see if the time falls within the last segment we evaluated */
	if ( animCurveEvalCache->lastKey != kEngineNULL) {
		if (( animCurveEvalCache->lastIndex < (animCurve->numKeys - 1))
		&&	(time > animCurveEvalCache->lastKey->time)) {
			nextKey = &(animCurve->keyList[animCurveEvalCache->lastIndex + 1]);
			if (time == nextKey->time) {
				animCurveEvalCache->lastKey = nextKey;
				animCurveEvalCache->lastIndex++;
				return ( animCurveEvalCache->lastKey->value);
			}
			if (time < nextKey->time ) {
				index = animCurveEvalCache->lastIndex + 1;
				withinInterval = kEngineTRUE;
			}
		}
		else if (( animCurveEvalCache->lastIndex > 0)
			&&	(time < animCurveEvalCache->lastKey->time)) {
			nextKey = &(animCurve->keyList[animCurveEvalCache->lastIndex - 1]);
			if (time > nextKey->time) {
				index = animCurveEvalCache->lastIndex;
				withinInterval = kEngineTRUE;
			}
			if (time == nextKey->time) {
				animCurveEvalCache->lastKey = nextKey;
				animCurveEvalCache->lastIndex--;
				return ( animCurveEvalCache->lastKey->value);
			}
		}
	}

	/* it does not, so find the new segment */
	if (!withinInterval) {
		if (find (animCurve, time, &index) || (index == 0)) {
			/*
			//	Exact match or before range of this action,
			//	return exact keyframe value.
			*/
			animCurveEvalCache->lastKey = &(animCurve->keyList[index]);
			animCurveEvalCache->lastIndex = index;
			return ( animCurveEvalCache->lastKey->value);
		}
		else if (index == animCurve->numKeys) {
			/* Beyond range of this action return end keyframe value */
			animCurveEvalCache->lastKey = &(animCurve->keyList[0]);
			animCurveEvalCache->lastIndex = 0;
			return (animCurve->keyList[animCurve->numKeys - 1].value);
		}
	}

	/* if we are in a new segment, pre-compute and cache the bezier parameters */
	if (animCurveEvalCache->lastInterval != (index - 1)) {
		animCurveEvalCache->lastInterval = index - 1;
		animCurveEvalCache->lastIndex = animCurveEvalCache->lastInterval;
		animCurveEvalCache->lastKey = &(animCurve->keyList[animCurveEvalCache->lastInterval]);
		if (( animCurveEvalCache->lastKey->outTanX == 0.0f)
		&&	( animCurveEvalCache->lastKey->outTanY == 0.0f)) {
			animCurveEvalCache->isStep = kEngineTRUE;
		}
		else if (( animCurveEvalCache->lastKey->outTanX == kEngineFloatMax)
		&&	( animCurveEvalCache->lastKey->outTanY == kEngineFloatMax)) {
			animCurveEvalCache->isStepNext = kEngineTRUE;
			nextKey = &(animCurve->keyList[index]);
		}
		else {
			animCurveEvalCache->isStep = kEngineFALSE;
			animCurveEvalCache->isStepNext = kEngineFALSE;
			x[0] = animCurveEvalCache->lastKey->time;
			y[0] = animCurveEvalCache->lastKey->value;
			x[1] = x[0] + ( animCurveEvalCache->lastKey->outTanX * kOneThird);
			y[1] = y[0] + ( animCurveEvalCache->lastKey->outTanY * kOneThird);

			nextKey = &(animCurve->keyList[index]);
			x[3] = nextKey->time;
			y[3] = nextKey->value;
			x[2] = x[3] - (nextKey->inTanX * kOneThird);
			y[2] = y[3] - (nextKey->inTanY * kOneThird);

			if (animCurve->isWeighted) {
				engineBezierCreate (animCurve, animCurveEvalCache, x, y);
			}
			else {
				engineHermiteCreate (animCurve, animCurveEvalCache, x, y);
			}
		}
	}

	/* finally we can evaluate the segment */
	if ( animCurveEvalCache->isStep) {
		value = animCurveEvalCache->lastKey->value;
	}
	else if ( animCurveEvalCache->isStepNext) {
		value = nextKey->value;
	}
	else if (animCurve->isWeighted) {
		value = engineBezierEvaluate (animCurve, animCurveEvalCache, time);
	}
	else {
		value = engineHermiteEvaluate (animCurve, animCurveEvalCache, time);
	}
	return (value);
}

float MayaAnimCurve::evaluate( float time, EtCurveEvalCache* evalCache ) const
{
	SPAD_ASSERT( !keyList_.empty() );
	return engineAnimEvaluate( &curve_, evalCache, time );
}

//float MayaAnimCurve::evaluateNotThreadSafe( float time )
//{
//	return evaluate( time, &evalCache_ );
//}

/*
//	Function Name:
//		assembleAnimCurve
//
//	Description:
//		A static helper function to assemble an EtCurve animation curve
//	from a linked list of heavy-weights keys
//
//  Input Arguments:
//		EtReadKey *firstKey			The linked list of keys
//		EtInt numKeys				The number of keyss
//		EtBoolean isWeighted		Whether or not the curve has weighted tangents
//		EtBoolean useOldSmooth		Whether or not to use pre-Maya2.0 smooth
//									tangent computation
//
//  Return Value:
//		EtCurve *animCurve			The assembled animation curve
//
//	Note:
//		This function will also free the memory used by the heavy-weight keys
*/
void MayaAnimCurve::_Init( const EtReadKey* keys, const size_t numKeys, EtInfinityType preInfinity, EtInfinityType postInfinity, bool isWeighted /*= false*/ )
{
	SPAD_ASSERT( numKeys > 0 );

	EtCurve *	animCurve = &curve_;
	const EtReadKey *	prevKey = kEngineNULL;
	const EtReadKey *	key = kEngineNULL;
	const EtReadKey *	nextKey = kEngineNULL;
	EtInt		index;
	EtKey *		thisKey;
	EtValue		py, ny, dx, cpy;
	EtBoolean	hasSmooth;
	EtFloat		length;
	EtFloat		inTanX = 0, inTanY = 0, outTanX = 0, outTanY = 0;
	EtFloat		inTanXs, inTanYs, outTanXs, outTanYs;

	keyList_.resize( numKeys );
	animCurve->keyList = &keyList_[0];

	/* initialize the animation curve parameters */
	animCurve->numKeys = (int)numKeys;
	animCurve->isWeighted = isWeighted;
	animCurve->isStatic = kEngineTRUE;
	animCurve->preInfinity = preInfinity;
	animCurve->postInfinity = postInfinity;

	/* compute tangents */
	nextKey = &keys[0];
	index = 0;
	u32 nextKeyIndex = 0;
	while( nextKeyIndex < numKeys )
	{
		prevKey = key;
		key = nextKey;
		if ( ++nextKeyIndex == numKeys )
			nextKey = kEngineNULL;
		else
			nextKey = &keys[nextKeyIndex];

		/* construct the final EtKey (light-weight key) */
		thisKey = &( animCurve->keyList[index++] );
		thisKey->time = key->time;
		thisKey->value = key->value;

		EtTangentType inTangentType = key->inTangentType;

		/* compute the in-tangent values */
		/* kTangentClamped */
		if ( ( inTangentType == kTangentClamped ) && ( prevKey != kEngineNULL ) ) {
			py = prevKey->value - key->value;
			if ( py < 0.0f ) py = -py;
			ny = ( nextKey == kEngineNULL ? py : nextKey->value - key->value );
			if ( ny < 0.0f ) ny = -ny;
			if ( ( ny <= 0.05 ) || ( py <= 0.05 ) ) {
				inTangentType = kTangentFlat;
			}
		}


		if ( inTangentType == kTangentPlateau || inTangentType == kTangentAuto )
		{

			if ( prevKey == kEngineNULL || nextKey == kEngineNULL )
			{
				/* No previous or next key, first and last keys are always flat */
				inTangentType = kTangentFlat;
			}
			else
			{
				/* Compute control point (tangent end) Y position because if this point goes beyond the
				previous Y key position then the tangent must be readjusted                           */
				py = prevKey->value - key->value;
				ny = nextKey->value - key->value;

				if ( py*ny >= 0.0f )
				{
					/* both py and ny have the same sign, key is a maxima or a minima */
					inTangentType = kTangentFlat;
				}
				else
				{


					/*
					Compute the smooth tangent control point (tangent end) Y position(cpy) because if
					this point goes beyond the previous Y or next -Y key position then the tangent
					must be readjusted/clamped
					*/

					cpy = ( py - ny )*( key->time - prevKey->time ) / ( 3 * ( nextKey->time - prevKey->time ) );

					/*
					Check if the slope to the next key is more gentle than the slope to
					previous key
					*/
					if ( ( -ny / py )<( ( nextKey->time - key->time ) / ( key->time - prevKey->time ) ) )
					{
						/* Adjust previous key value to match the slope of the out tangent */
						py = -ny*( key->time - prevKey->time ) / ( nextKey->time - key->time );
					}

					/*
					Clamp the computed smooth tangent control point value if it goes above the absolute
					previous key value
					*/
					if ( ( py >= 0.0f && cpy > py ) || ( py <= 0.0f && cpy < py ) )
					{
						if ( inTangentType == kTangentPlateau )
						{
							inTangentType = kTangentFlat;
						}
						else /*(inTangentType == kTangentAuto)*/
						{
							inTanY = -py*3.0f;
							inTanX = key->time - prevKey->time;
						}
					}
					else
					{
						/* by default these tangents behave as smooth tangents */
						inTangentType = kTangentSmooth;
					}
				}
			}
		}

		hasSmooth = kEngineFALSE;
		switch ( inTangentType ) {
		case kTangentFixed:
			//inTanX = key->inWeightX * cos( key->inAngle ) * 3.0f;
			//inTanY = key->inWeightY * sin( key->inAngle ) * 3.0f;
			inTanX = 0;
			inTanY = 0;
			break;
		case kTangentLinear:
			if ( prevKey == kEngineNULL ) {
				inTanX = 1.0f;
				inTanY = 0.0f;
			}
			else {
				inTanX = key->time - prevKey->time;
				inTanY = key->value - prevKey->value;
			}
			break;
		case kTangentFlat:
			if ( prevKey == kEngineNULL ) {
				inTanX = ( nextKey == kEngineNULL ? 0.0f : nextKey->time - key->time );
				inTanY = 0.0f;
			}
			else {
				inTanX = key->time - prevKey->time;
				inTanY = 0.0f;
			}
			break;
		case kTangentStep:
			inTanX = 0.0f;
			inTanY = 0.0f;
			break;
		case kTangentStepNext:
			inTanX = kEngineFloatMax;
			inTanY = kEngineFloatMax;
			break;
		case kTangentSlow:
		case kTangentFast:
			inTangentType = kTangentSmooth;
			if ( prevKey == kEngineNULL ) {
				inTanX = 1.0f;
				inTanY = 0.0f;
			}
			else {
				inTanX = key->time - prevKey->time;
				inTanY = key->value - prevKey->value;
			}
			break;
		case kTangentSmooth:
		case kTangentClamped:
			inTangentType = kTangentSmooth;
			hasSmooth = kEngineTRUE;
			break;
		}

		EtTangentType outTangentType = key->outTangentType;

		/* compute the out-tangent values */
		/* kTangentClamped */
		if ( ( outTangentType == kTangentClamped ) && ( nextKey != kEngineNULL ) ) {
			ny = nextKey->value - key->value;
			if ( ny < 0.0f ) ny = -ny;
			py = ( prevKey == kEngineNULL ? ny : prevKey->value - key->value );
			if ( py < 0.0f ) py = -py;
			if ( ( ny <= 0.05 ) || ( py <= 0.05 ) ) {
				outTangentType = kTangentFlat;
			}
		}
		if ( outTangentType == kTangentPlateau || outTangentType == kTangentAuto )
		{

			if ( prevKey == kEngineNULL || nextKey == kEngineNULL )
			{
				/* First and last keys are always flat */
				outTangentType = kTangentFlat;
			}
			else
			{
				py = prevKey->value - key->value;
				ny = nextKey->value - key->value;

				if ( py*ny >= 0.0f )
				{
					/* both py and ny have the same sign, key is a maxima or a minima */
					outTangentType = kTangentFlat;
				}
				else
				{


					/*
					Compute the smooth tangent control point (tangent end) Y position(cpy) because if
					this point goes beyond the previous Y or next -Y key position then the tangent
					must be readjusted/clamped
					*/

					cpy = ( ny - py )*( nextKey->time - key->time ) / ( 3 * ( nextKey->time - prevKey->time ) );

					if ( ( -py / ny )<( ( key->time - prevKey->time ) / ( nextKey->time - key->time ) ) )
					{
						/* Adjust next key value to match the slope of the in tangent */
						ny = -py*( nextKey->time - key->time ) / ( key->time - prevKey->time );
					}

					if ( ( ny >= 0.0f && cpy > ny ) || ( ny <= 0.0f && cpy < ny ) )
					{
						if ( outTangentType == kTangentPlateau )
						{
							outTangentType = kTangentFlat;
						}
						else /*(outTangentType == kTangentAuto)*/
						{
							outTanY = ny*3.0f;
							outTanX = nextKey->time - key->time;
						}
					}
					else
					{
						/* by default these tangents behave as smooth tangents */
						outTangentType = kTangentSmooth;
					}
				}
			}
		}


		switch ( outTangentType ) {
		case kTangentFixed:
			//outTanX = key->outWeightX * cos( key->outAngle ) * 3.0f;
			//outTanY = key->outWeightY * sin( key->outAngle ) * 3.0f;
			outTanX = 0;
			outTanY = 0;
			break;
		case kTangentLinear:
			if ( nextKey == kEngineNULL ) {
				outTanX = 1.0f;
				outTanY = 0.0f;
			}
			else {
				outTanX = nextKey->time - key->time;
				outTanY = nextKey->value - key->value;
			}
			break;
		case kTangentFlat:
			if ( nextKey == kEngineNULL ) {
				outTanX = ( prevKey == kEngineNULL ? 0.0f : key->time - prevKey->time );
				outTanY = 0.0f;
			}
			else {
				outTanX = nextKey->time - key->time;
				outTanY = 0.0f;
			}
			break;
		case kTangentStep:
			outTanX = 0.0f;
			outTanY = 0.0f;
			break;
		case kTangentStepNext:
			outTanX = kEngineFloatMax;
			outTanY = kEngineFloatMax;
			break;
		case kTangentSlow:
		case kTangentFast:
			outTangentType = kTangentSmooth;
			if ( nextKey == kEngineNULL ) {
				outTanX = 1.0f;
				outTanY = 0.0f;
			}
			else {
				outTanX = nextKey->time - key->time;
				outTanY = nextKey->value - key->value;
			}
			break;
		case kTangentSmooth:
		case kTangentClamped:
			outTangentType = kTangentSmooth;
			hasSmooth = kEngineTRUE;
			break;
		}

		/* compute smooth tangents (if necessary) */
		if ( hasSmooth ) {
			/* Maya 2.0f smooth tangents */
			if ( ( prevKey == kEngineNULL ) && ( nextKey != kEngineNULL ) ) {
				outTanXs = nextKey->time - key->time;
				outTanYs = nextKey->value - key->value;
				inTanXs = outTanXs;
				inTanYs = outTanYs;
			}
			else if ( ( prevKey != kEngineNULL ) && ( nextKey == kEngineNULL ) ) {
				outTanXs = key->time - prevKey->time;
				outTanYs = key->value - prevKey->value;
				inTanXs = outTanXs;
				inTanYs = outTanYs;
			}
			else if ( ( prevKey != kEngineNULL ) && ( nextKey != kEngineNULL ) ) {
				/* There is a CV before and after this one*/
				/* Find average of the adjacent in and out tangents. */

				dx = nextKey->time - prevKey->time;
				if ( dx < 0.0001 ) {
					outTanYs = kMaxTan;
				}
				else {
					outTanYs = ( nextKey->value - prevKey->value ) / dx;
				}

				outTanXs = nextKey->time - key->time;
				inTanXs = key->time - prevKey->time;
				inTanYs = outTanYs * inTanXs;
				outTanYs *= outTanXs;
			}
			else {
				inTanXs = 1.0f;
				inTanYs = 0.0f;
				outTanXs = 1.0f;
				outTanYs = 0.0f;
			}

			if ( key->inTangentType == kTangentSmooth ) {
				inTanX = inTanXs;
				inTanY = inTanYs;
			}
			if ( key->outTangentType == kTangentSmooth ) {
				outTanX = outTanXs;
				outTanY = outTanYs;
			}
		}

		/* make sure the computed tangents are valid */
		if ( animCurve->isWeighted ) {
			if ( inTanX < 0.0f ) inTanX = 0.0f;
			if ( outTanX < 0.0f ) outTanX = 0.0f;
		}
		else if ( ( inTanX == kEngineFloatMax && inTanY == kEngineFloatMax )
			|| ( outTanX == kEngineFloatMax && outTanY == kEngineFloatMax ) )
		{
			// SPecial case for step next tangents, do nothing
		}
		else {
			if ( inTanX < 0.0f ) {
				inTanX = 0.0f;
			}
			length = sqrtf( ( inTanX * inTanX ) + ( inTanY * inTanY ) );
			if ( length != 0.0f ) {	/* zero lengths can come from step tangents */
				inTanX /= length;
				inTanY /= length;
			}
			if ( ( inTanX == 0.0f ) && ( inTanY != 0.0f ) ) {
				inTanX = 0.0001f;
				inTanY = ( inTanY < 0.0f ? -1.0f : 1.0f ) * ( inTanX * kMaxTan );
			}
			if ( outTanX < 0.0f ) {
				outTanX = 0.0f;
			}
			length = sqrtf( ( outTanX * outTanX ) + ( outTanY * outTanY ) );
			if ( length != 0.0f ) {	/* zero lengths can come from step tangents */
				outTanX /= length;
				outTanY /= length;
			}
			if ( ( outTanX == 0.0f ) && ( outTanY != 0.0f ) ) {
				outTanX = 0.0001f;
				outTanY = ( outTanY < 0.0f ? -1.0f : 1.0f ) * ( outTanX * kMaxTan );
			}
		}

		thisKey->inTanX = inTanX;
		thisKey->inTanY = inTanY;
		thisKey->outTanX = outTanX;
		thisKey->outTanY = outTanY;

		/*
		// check whether or not this animation curve is static (i.e. all the
		// key values are the same)
		*/
		if ( animCurve->isStatic ) {
			if ( ( prevKey != kEngineNULL ) && ( prevKey->value != key->value ) ) {
				animCurve->isStatic = kEngineFALSE;
			}
			else if ( ( inTanY != 0.0f ) || ( outTanY != 0.0f ) ) {
				animCurve->isStatic = kEngineFALSE;
			}
		}
	}
	if ( animCurve->isStatic ) {
		if ( ( prevKey != kEngineNULL ) && ( key != kEngineNULL ) && ( prevKey->value != key->value ) ) {
			animCurve->isStatic = kEngineFALSE;
		}
	}
}


} // namespace spad
