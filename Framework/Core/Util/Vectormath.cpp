#include "Util_pch.h"
#include "Vectormath.h"
#include "Bits.h"

#if defined(_MSC_VER) && defined(_DEBUG)
#define new _DEBUG_NEW
#endif

namespace spad
{

	Vector3 extractEulerAnglesZYX( const Matrix3& pureRotation )
	{
		Matrix3 r = pureRotation;
		union
		{
			vec_float4 vec;
			float f[4];
		} col0;
		col0.vec = r.getCol0().get128();

		const float minusCol0z = clamp( -col0.f[2], -1.0f, 1.0f );
		// this clamp is needed because from time to time, test was sth like 1.00001
		// and result was y = -1.#IND00
		float y = asinf( minusCol0z );

		if ( fabsf( minusCol0z ) < 0.99999f )
		{
			float z = atan2f( col0.f[1], col0.f[0] );
			float x = atan2f( r.getCol1().getZ().getAsFloat(), r.getCol2().getZ().getAsFloat() );
			return Vector3( x, y, z );
		}
		else
		{
			// not a unique solution ('x + z' constant or 'x - z' constant)

			float sign = select( minusCol0z, 1.0f, -1.0f );

			float z = 0;
			float x = atan2f( sign * r.getCol1().getX().getAsFloat(), sign * r.getCol2().getX().getAsFloat() );
			return Vector3( x, y, z );
		}
	}

	Vector3 extractEulerAnglesZYX( const Quat& rotation )
	{
		union
		{
			vec_float4 vec;
			float f[4];
		} q;
		q.vec = rotation.get128();
		const float x = q.f[0];
		const float y = q.f[1];
		const float z = q.f[2];
		const float w = q.f[3];

		// this clamp is needed because from time to time, test was sth like 1.00001
		// and result was y = -1.#IND00
		const float test = clamp( 2 * ( w*y - x*z ), -1.0f, 1.0f ); // equals - 2 * ( x*z - w*y );
		const float ry = asinf( test );

		if ( fabsf( test ) < 0.99999f )
		{
			// from extractEulerAnglesZYX float z = atan2f( col0.f[1], col0.f[0] );
			// from extractEulerAnglesZYX float x = atan2f( r.getCol1().getZ().getAsFloat(), r.getCol2().getZ().getAsFloat() );
			const float yy = y*y;
			const float rz = atan2f( 2 * ( x*y + w*z ), 1 - 2 * ( yy + z*z ) );
			const float rx = atan2f( 2 * ( y*z + w*x ), 1 - 2 * ( x*x + yy ) );
			return Vector3( rx, ry, rz );
		}
		else
		{
			// not a unique solution ('x + z' constant or 'x - z' constant)

			// from extractEulerAnglesZYX float x = atan2f( sign * r.getCol1().getX().getAsFloat(), sign * r.getCol2().getX().getAsFloat() );

			const float sign = select( test, 1.0f, -1.0f );
			const float rz = 0;
			const float rx = atan2f( sign * 2 * ( x*y - w*z ), sign * 2 * ( x*z + w*y ) );
			//char buffer[64];
			//sprintf( buffer, "%f", ry );
			//if ( strcmp("-1.#IND00", buffer) == 0 )
			//{
			//	int x = 0;
			//}
			return Vector3( rx, ry, rz );
		}

	} // extractEulerAnglesZYX


	void extractRotationAndTranslation( Vector3 &jointTranslation, Quat& jointRotation, const Matrix4 &mat )
	{
		//const float* pMatrix4x4 = reinterpret_cast<const float*>( &mat );

		// Translation
		//jointTranslation[0] = pMatrix4x4[4 * 3 + 0];
		//jointTranslation[1] = pMatrix4x4[4 * 3 + 1];
		//jointTranslation[2] = pMatrix4x4[4 * 3 + 2];
		jointTranslation = mat.getTranslation();
		//jointTranslation[ 3 ] = 1.0f;

		//// Scale
		//jointScale[0] = sqrtf( Sq( pMatrix4x4[4 * 0 + 0] ) + Sq( pMatrix4x4[4 * 0 + 1] ) + Sq( pMatrix4x4[4 * 0 + 2] ) );
		//jointScale[1] = sqrtf( Sq( pMatrix4x4[4 * 1 + 0] ) + Sq( pMatrix4x4[4 * 1 + 1] ) + Sq( pMatrix4x4[4 * 1 + 2] ) );
		//jointScale[2] = sqrtf( Sq( pMatrix4x4[4 * 2 + 0] ) + Sq( pMatrix4x4[4 * 2 + 1] ) + Sq( pMatrix4x4[4 * 2 + 2] ) );
		//jointScale[ 3 ] = 1.0f;

		//// Ortho normalize rotation
		//float rotMat[3][3];
		//rotMat[0][0] = pMatrix4x4[4 * 0 + 0];
		//rotMat[0][1] = pMatrix4x4[4 * 0 + 1];
		//rotMat[0][2] = pMatrix4x4[4 * 0 + 2];
		//Normalize3( rotMat[0] );
		//Cross3( rotMat[2], rotMat[0], &pMatrix4x4[4 * 1 + 0] );
		//Normalize3( rotMat[2] );
		//Cross3( rotMat[1], rotMat[2], rotMat[0] );
		//Normalize3( rotMat[1] );
		Vector3 rotMat0 = normalize( mat.getCol0().getXYZ() );
		Vector3 rotMat2 = normalize( cross( rotMat0, mat.getCol1().getXYZ() ) );
		Vector3 rotMat1 = normalize( cross( rotMat2, rotMat0 ) );

		jointRotation = Quat( Matrix3( rotMat0, rotMat1, rotMat2 ) );

		//// Get quat from rotation (see target/common/Vectormath)
		//float xx = rotMat[0][0];
		//float yx = rotMat[0][1];
		//float zx = rotMat[0][2];
		//float xy = rotMat[1][0];
		//float yy = rotMat[1][1];
		//float zy = rotMat[1][2];
		//float xz = rotMat[2][0];
		//float yz = rotMat[2][1];
		//float zz = rotMat[2][2];
		//float trace = ( ( xx + yy ) + zz );
		//bool negTrace = ( trace < 0.0f );
		//bool ZgtX = zz > xx;
		//bool ZgtY = zz > yy;
		//bool YgtX = yy > xx;
		//bool largestXorY = ( !ZgtX || !ZgtY ) && negTrace;
		//bool largestYorZ = ( YgtX || ZgtX ) && negTrace;
		//bool largestZorX = ( ZgtY || !YgtX ) && negTrace;

		//if ( largestXorY ) {
		//	zz = -zz;
		//	xy = -xy;
		//}
		//if ( largestYorZ ) {
		//	xx = -xx;
		//	yz = -yz;
		//}
		//if ( largestZorX ) {
		//	yy = -yy;
		//	zx = -zx;
		//}

		//float radicand = ( ( ( xx + yy ) + zz ) + 1.0f );
		//float scale = ( 0.5f * ( 1.0f / sqrtf( radicand ) ) );

		//float tmpx = ( ( zy - yz ) * scale );
		//float tmpy = ( ( xz - zx ) * scale );
		//float tmpz = ( ( yx - xy ) * scale );
		//float tmpw = ( radicand * scale );
		//float qx = tmpx;
		//float qy = tmpy;
		//float qz = tmpz;
		//float qw = tmpw;

		//if ( largestXorY ) {
		//	qx = tmpw;
		//	qy = tmpz;
		//	qz = tmpy;
		//	qw = tmpx;
		//}
		//if ( largestYorZ ) {
		//	tmpx = qx;
		//	tmpz = qz;
		//	qx = qy;
		//	qy = tmpx;
		//	qz = qw;
		//	qw = tmpz;
		//}

		//jointRotation[0] = qx;
		//jointRotation[1] = qy;
		//jointRotation[2] = qz;
		//jointRotation[3] = qw;
	}

} // namespace spad
