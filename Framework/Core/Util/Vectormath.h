#pragma once

#if defined(_MSC_VER) && defined(_DEBUG)
#define _CRTDBG_MAP_ALLOC
#endif

#if !defined(_VECTORMATH_NO_SCALAR_CAST)
#define _VECTORMATH_NO_SCALAR_CAST
#endif

#ifndef PI
#define PI 3.141592653589793f
#endif

#ifndef __SSE__
#define __SSE__
#endif

#include "vectormathlibrary/include/vectormath/SSE/cpp/vectormath_aos.h"

#define vec_div(a,b) _mm_div_ps( a, b )
#define vec_min( a, b ) _mm_min_ps( a, b )
#define vec_max( a, b ) _mm_max_ps( a, b )
#define vec_cmplt( a, b ) _mm_cmplt_ps( a, b ) /* a < b */

inline vec_float4 copysignf4( vec_float4 x, vec_float4 y )
{
	return vec_sel( x, y, ( 0x80000000 ) );
}

#include "vectormathlibrary/include/vectormath/SSE/cpp/vectormath_soa.h"

using namespace Vectormath;
using namespace Vectormath::Aos;

//////////////////////////////////////////////////////////////////////////
// sse
//////////////////////////////////////////////////////////////////////////

namespace spad
{
	inline void loadXYZW( Vector4 & vec, const vec_float4 * quadptr )
	{
		vec = Vector4( *quadptr );
	}

	inline const floatInVec vec_minf( const floatInVec &v0, const floatInVec &v1 )
	{
		return floatInVec( _mm_min_ps( v0.get128(), v1.get128() ) );
	}

	inline const floatInVec vec_maxf( const floatInVec &v0, const floatInVec &v1 )
	{
		return floatInVec( _mm_max_ps( v0.get128(), v1.get128() ) );
	}

	inline floatInVec vec_clampf( const floatInVec& x, const floatInVec& a, const floatInVec& b )
	{
		return floatInVec( vec_maxf( a, vec_minf( b, x ) ) );
	}

	inline Vector3 vec_clamp( const Vector3& x, const Vector3& a, const Vector3& b )
	{
		return maxPerElem( a, minPerElem( b, x ) );
	}

	inline Vector4 vec_clamp( const Vector4& x, const Vector4& a, const Vector4& b )
	{
		return maxPerElem( a, minPerElem( b, x ) );
	}

	inline floatInVec vec_absf( const floatInVec& v )
	{
		return floatInVec( fabsf4( v.get128() ) );
	}

	inline Vector3 vec_abs3( const Vector3& x )
	{
		return Vector3( fabsf4( x.get128() ) );
	}

	inline vec_float4 maxElem( const vec_float4 &vec )
	{
		return _mm_max_ps(
			_mm_max_ps( vec_splat( vec, 0 ), vec_splat( vec, 1 ) ),
			_mm_max_ps( vec_splat( vec, 2 ), vec_splat( vec, 3 ) ) );
	}

	inline const floatInVec deg2rad( const floatInVec& deg )
	{
		return floatInVec( deg * floatInVec( PI / 180.0f ) );
	}

	inline const Vector3 deg2rad( const Vector3& deg )
	{
		return Vector3( deg * floatInVec( PI / 180.0f ) );
	}

	inline const floatInVec rad2deg( const floatInVec& rad )
	{
		return floatInVec( rad * floatInVec( 180.0f / PI ) );
	}

	inline const Vector3 rad2deg( const Vector3& rad )
	{
		return Vector3( rad * floatInVec( 180.0f / PI ) );
	}

#define deg2rad( x ) ((x) * (PI / 180.0f))
#define rad2deg( x ) ((x) * (180.0f / PI))

	static inline __m128 vec_fabsf( __m128 x )
	{
		int mask = 0x7fffffff;
		return _mm_and_ps( x, _mm_set1_ps( *(float *)&mask ) );
	}

	inline Vector3 getRotationVector( const Matrix3& rot )
	{
#ifdef _VECTORMATH_NO_SCALAR_CAST
		const float xy = rot.getCol0().getY().getAsFloat();
		const float yy = rot.getCol1().getY().getAsFloat();
		const float xx = rot.getCol0().getX().getAsFloat();
		const float yx = rot.getCol1().getX().getAsFloat();
		const float xz = rot.getCol0().getZ().getAsFloat();
		const float yz = rot.getCol1().getZ().getAsFloat();
		const float zz = rot.getCol2().getZ().getAsFloat();
#else
		const float xx = rot.getCol0().getX();
		const float yx = rot.getCol1().getX();
		const float xy = rot.getCol0().getY();
		const float yy = rot.getCol1().getY();
		const float xz = rot.getCol0().getZ();
		const float yz = rot.getCol1().getZ();
		const float zz = rot.getCol2().getZ();
#endif


		float rotx = 0.f;
		float roty = 0.f;
		float rotz = 0.f;

		roty = asin( -xz );
		const float C = cos( roty );
		const float Cinv = 1.f / C;

		if ( fabs( C ) > 0.0001f )
		{
			rotx = atan2( -( yz * Cinv ), zz * Cinv );
			rotz = atan2( -( xy * Cinv ), xx * Cinv );
		}
		else
		{
			rotx = 0.f;
			rotz = atan2( yx, yy );
		}

		return Vector3( rotx, roty, rotz );
	}

	inline Vector3 projectPointOnPlane( const Vector3& point, const Vector4& plane )
	{
		const Vector3& n = plane.getXYZ();
		const Vector3& Q = point;

		const Vector3 Qp = Q - ( dot( Q, n ) + plane.getW() ) * n;
		return Qp;
	}

	inline Vector3 projectVectorOnPlane( const Vector3& vec, const Vector4& plane )
	{
		const Vector3& n = plane.getXYZ();
		const Vector3& V = vec;
		const Vector3 W = V - dot( V, n ) * n;
		return W;
	}

	inline float getAngleBetween( const Vector3& v1, const Vector3& v2 )
	{
#ifdef _VECTORMATH_NO_SCALAR_CAST
		float cosine = dot( v1, v2 ).getAsFloat();
#else
		float cosine = dot( v1, v2 );
#endif
		float angle;
		if ( cosine >= 1.0 )
			angle = 0.0;
		else if ( cosine <= -1.0 )
			angle = PI;
		else
		{
			if ( cosine > 1.0f )
				cosine = 1.0f;
			if ( cosine < -1.0f )
				cosine = -1.0f;
			angle = acosf( cosine );
		}

		return angle;
	}

	/**
	 @remarks
	 http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToEuler/index.htm
	 Geometric Tools for Computer Graphics, Philip Schneider and David Eberly (section A.3.1., rotation order zyx)

	 Implementation based on pseudo code from the book.

	 Vector3 getEulerAnglesZYXReference( const Matrix3& rr )
	 {
	 Matrix3 r = rr;

	 float y = asinf( -r.getCol0().getZ().getAsFloat() );
	 if ( y < PI / 2 )
	 {
	 if ( y > -PI / 2 )
	 {
	 float z = atan2f( r.getCol0().getY().getAsFloat(), r.getCol0().getX().getAsFloat() );
	 float x = atan2f( r.getCol1().getZ().getAsFloat(), r.getCol2().getZ().getAsFloat() );
	 return Vector3( x, y, z );
	 }
	 else
	 {
	 // not a unique solution (x + z constant)
	 float z = atan2f( -r.getCol1().getX().getAsFloat(), -r.getCol2().getX().getAsFloat() );
	 float x = 0;
	 return Vector3( x, y, z );
	 }
	 }
	 else
	 {
	 // not a unique solution (x - z constant)
	 float z = -atan2f( r.getCol1().getX().getAsFloat(), r.getCol2().getX().getAsFloat() );
	 float x = 0;
	 return Vector3( x, y, z );
	 }
	 }

	 My implementation is a bit different, it should be more efficient, especially on powerpc.
	 It also behaves bit different in situation of singularities (y angle -PI/2 or PI/2). My choice is z = 0 instead x = 0.
	 This is consistent with maya. We have to remember to remove 'minus' found in front of atan2f from the original implementation.

	 This is right handed coordinate system.
	 This method behaves correctly only for x - y - z rotation order: first rotation around x-axis, then rotation around y-axis,
	 and finally rotation around z axis. This is consistent with Matrix3::rotationZYX method.
	 Matrix3::rotationZYX == Matrix3::rotationZ(z) * Matrix3::rotationY(y) * Matrix3::rotationX(x)

	 It uses floating point processor, so LHS (load-hits-store) is possible when there will be need to transfer from
	 vector registers to floating point registers.

	 @returns Vectormath::Aos::Vector3
	 Returned vector contains in its components:
	 * x - rotation around x axis
	 * y - rotation around y axis
	 * z - rotation around z axis
	 @param const Matrix3 & pureRotation
	 */
	Vector3 extractEulerAnglesZYX( const Matrix3& pureRotation );

	/**
	 @remarks
	 See extractEulerAnglesZYX. All of concepts from extractEulerAnglesZYX apply also to this function.

	 http://www.euclideanspace.com/maths/geometry/rotations/conversions/quaternionToEuler/index.htm
	 Geometric Tools for Computer Graphics, Philip Schneider and David Eberly (section A.4.3.)

	 It seems that there is an error in Geometric Tools, so I used coversion from quaternion to matrix from
	 www.euclideanspace.com ( but derivation of equation in Geometric Tools looks good ).

	 @returns Vectormath::Aos::Vector3
	 @param const Quat & rotation
	 */
	Vector3 extractEulerAnglesZYX( const Quat& rotation );

	void extractRotationAndTranslation( Vector3 &jointTranslation, Quat& jointRotation, const Matrix4 &mat );

	inline floatInVec lerp( const floatInVec& t, const floatInVec& a, const floatInVec& b )
	{
		return a + t*( b - a );
	}

	inline Matrix4 blendPoses( const floatInVec& alpha, const Matrix4& a, const Matrix4& b )
	{
		const Vector3 pos = lerp( alpha, a.getTranslation(), b.getTranslation() );
		const Quat rot = slerp( alpha, Quat( a.getUpper3x3() ), Quat( b.getUpper3x3() ) );

		return Matrix4( rot, pos );
	}

	inline Vector3 fastRotate( const Quat& unitQuat, const Vector3& v )
	{
		const Vector3 qXYZ = unitQuat.getXYZ();
		const Vector3 t = cross( qXYZ, v ) * 2.f;
		return v + unitQuat.getW() * t + cross( qXYZ, t );
	}
	inline Vector3 fastRotateInv( const Quat& unitQuat, const Vector3& v )
	{
		const Vector3 qXYZ = unitQuat.getXYZ();
		const Vector3 t = cross( qXYZ, v ) * 2.f;
		return v - unitQuat.getW() * t + cross( qXYZ, t );
	}

	inline const Point3 fastTransform( const Quat& rot, const Point3& trans, const Point3& point )
	{
		return trans + fastRotate( rot, Vector3( point ) );
	}

	inline Soa::Vector3 fastRotate_Soa( const Soa::Quat& unitQuat, const Soa::Vector3& v )
	{
		const vec_float4 two = { 2.f, 2.f, 2.f, 2.f };
		const Soa::Vector3 qXYZ = unitQuat.getXYZ();
		const Soa::Vector3 t = cross( qXYZ, v ) * two;
		return v + unitQuat.getW() * t + cross( qXYZ, t );
	}

	inline Soa::Vector3 fastTransform_Soa( const Soa::Quat& rot, const Soa::Vector3& trans, const Soa::Vector3& point )
	{
		return trans + fastRotate_Soa( rot, point );
	}



	// provided vector will be z axis of the frame
	//
	inline Matrix3 createBasisZAxis( const Vector3& n )
	{
		// http://orbit.dtu.dk/fedora/objects/orbit:113874/datastreams/file_75b66578-222e-4c7d-abdf-f7e255100209/content
		//
		if ( n.getZ().getAsFloat() < -0.9999999f )
		{
			return Matrix3( Vector3( 0.f, -1.f, 0.f ), Vector3( -1.f, 0.f, 0.f ), n );
		}

		const floatInVec oneInVec( 1.0f );
		const floatInVec a = oneInVec / ( oneInVec + n.getZ() );
		const floatInVec b = -n.getX()*n.getY()*a;
		const Vector3 x = Vector3( oneInVec - n.getX()*n.getX()*a, b, -n.getX() );
		const Vector3 y = Vector3( b, oneInVec - n.getY()*n.getY()*a, -n.getY() );
		return Matrix3( x, y, n );
	}

	// provided vector will be x axis of the frame
	//
	inline Matrix3 createBasisXAxis( const Vector3& n )
	{
		// http://orbit.dtu.dk/fedora/objects/orbit:113874/datastreams/file_75b66578-222e-4c7d-abdf-f7e255100209/content
		//
		if ( n.getZ().getAsFloat() < -0.9999999f )
		{
			return Matrix3( Vector3( 0.f, -1.f, 0.f ), Vector3( -1.f, 0.f, 0.f ), n );
		}

		const floatInVec oneInVec( 1.0f );
		const floatInVec a = oneInVec / ( oneInVec + n.getZ() );
		const floatInVec b = -n.getX()*n.getY()*a;
		const Vector3 x = Vector3( oneInVec - n.getX()*n.getX()*a, b, -n.getX() );
		const Vector3 y = Vector3( b, oneInVec - n.getY()*n.getY()*a, -n.getY() );
		return Matrix3( n, y, -x );
	}

	// provided vector will be y axis of the frame
	//
	inline Matrix3 createBasisYAxis( const Vector3& n )
	{
		// http://orbit.dtu.dk/fedora/objects/orbit:113874/datastreams/file_75b66578-222e-4c7d-abdf-f7e255100209/content
		//
		if ( n.getZ().getAsFloat() < -0.9999999f )
		{
			return Matrix3( Vector3( 0.f, -1.f, 0.f ), Vector3( -1.f, 0.f, 0.f ), n );
		}

		const floatInVec oneInVec( 1.0f );
		const floatInVec a = oneInVec / ( oneInVec + n.getZ() );
		const floatInVec b = -n.getX()*n.getY()*a;
		const Vector3 x = Vector3( oneInVec - n.getX()*n.getX()*a, b, -n.getX() );
		const Vector3 y = Vector3( b, oneInVec - n.getY()*n.getY()*a, -n.getY() );
		return Matrix3( x, n, -y );
	}

	// provided vector will be z axis of the frame
	//
	inline Quat createBasisQuatZAxis( const Vector3& n )
	{
		// http://orbit.dtu.dk/fedora/objects/orbit:113874/datastreams/file_75b66578-222e-4c7d-abdf-f7e255100209/content
		//
		if ( n.getZ().getAsFloat() < -0.9999999f )
		{
			// Matrix3( Vector3( 0.f, -1.f, 0.f ), Vector3( -1.f, 0.f, 0.f ), n );
			// this quat corresponds to a matrix above
			//
			return Quat( floatInVec( 0.70710671f ), floatInVec( -0.70710671f ), floatInVec( 0.0f ), floatInVec( 0.0f ) );
		}

		floatInVec a( sqrtf4( ( floatInVec( 2.0f ) * ( floatInVec( 1.0f ) + n.getZ() ) ).get128() ) );
		Vector3 b( -n.getY(), n.getX(), floatInVec( 0.0f ) );
		b /= a;
		floatInVec w = floatInVec( 0.5f ) * a;
		return Quat( b, w );
	}



	inline Vector4 abgrToRgba( u32 abgr )
	{
		u32 r = abgr & 0xff;
		u32 g = ( abgr >> 8 ) & 0xff;
		u32 b = ( abgr >> 16 ) & 0xff;
		u32 a = ( abgr >> 24 ) & 0xff;
		const float m = 1.0f / 255.0f;

		return Vector4( (float)r * m, (float)g * m, (float)b * m, (float)a * m );
	};

	inline Vector4 argbToRgba( u32 argb )
	{
		u32 b = argb & 0xff;
		u32 g = ( argb >> 8 ) & 0xff;
		u32 r = ( argb >> 16 ) & 0xff;
		u32 a = ( argb >> 24 ) & 0xff;
		const float m = 1.0f / 255.0f;

		return Vector4( (float)r * m, (float)g * m, (float)b * m, (float)a * m );
	};

} // spad
