#include "Gfx_pch.h"
#include "ViewFrustum.h"

#if defined(_MSC_VER) && defined(_DEBUG)
#define new _DEBUG_NEW
#endif


inline const Vector3 unprojectNormalized( const Vector3& screenPos01, const Matrix4& inverseMatrix )
{
	// z maps to  <-1,1>
	//
	const floatInVec one( 1.0f );
	Vector4 hpos = Vector4( screenPos01, one );
	hpos = mulPerElem( hpos, Vector4( 2.0f ) ) - Vector4( one );
	Vector4 worldPos = inverseMatrix * hpos;
	worldPos /= worldPos.getW();
	return worldPos.getXYZ();
}

inline const Vector3 unprojectNormalizedDx( const Vector3& screenPos01, const Matrix4& inverseMatrix )
{
	// z maps to <0,1>
	//
	const floatInVec one( 1.0f );
	const floatInVec zer( 0.0f );
	Vector4 hpos = Vector4( screenPos01, one );
	hpos = mulPerElem( hpos, Vector4( 2.0f, 2.0f, 1.0f, 1.0f ) ) - Vector4( one, one, zer, zer );
	Vector4 worldPos = inverseMatrix * hpos;
	worldPos /= worldPos.getW();
	return worldPos.getXYZ();
}

void extractFrustumCorners( Vector3 dst[8], const Matrix4& viewProjection, bool dxStyleZProjection )
{
	const Matrix4 vpInv = inverse( viewProjection );

	const Vector3 leftUpperNear = Vector3( 0, 1, 0 );
	const Vector3 leftUpperFar = Vector3( 0, 1, 1 );
	const Vector3 leftLowerNear = Vector3( 0, 0, 0 );
	const Vector3 leftLowerFar = Vector3( 0, 0, 1 );

	const Vector3 rightLowerNear = Vector3( 1, 0, 0 );
	const Vector3 rightLowerFar = Vector3( 1, 0, 1 );
	const Vector3 rightUpperNear = Vector3( 1, 1, 0 );
	const Vector3 rightUpperFar = Vector3( 1, 1, 1 );

	if (dxStyleZProjection)
	{
		dst[0] = unprojectNormalizedDx( leftUpperNear, vpInv );
		dst[1] = unprojectNormalizedDx( leftUpperFar, vpInv );
		dst[2] = unprojectNormalizedDx( leftLowerNear, vpInv );
		dst[3] = unprojectNormalizedDx( leftLowerFar, vpInv );

		dst[4] = unprojectNormalizedDx( rightLowerNear, vpInv );
		dst[5] = unprojectNormalizedDx( rightLowerFar, vpInv );
		dst[6] = unprojectNormalizedDx( rightUpperNear, vpInv );
		dst[7] = unprojectNormalizedDx( rightUpperFar, vpInv );
	}
	else
	{
		dst[0] = unprojectNormalized( leftUpperNear, vpInv );
		dst[1] = unprojectNormalized( leftUpperFar, vpInv );
		dst[2] = unprojectNormalized( leftLowerNear, vpInv );
		dst[3] = unprojectNormalized( leftLowerFar, vpInv );

		dst[4] = unprojectNormalized( rightLowerNear, vpInv );
		dst[5] = unprojectNormalized( rightLowerFar, vpInv );
		dst[6] = unprojectNormalized( rightUpperNear, vpInv );
		dst[7] = unprojectNormalized( rightUpperFar, vpInv );
	}
}

inline void toSoa( Vector4& xxxx, Vector4& yyyy, Vector4& zzzz, Vector4& wwww,
	const Vector4& vec0, const Vector4& vec1, const Vector4& vec2, const Vector4& vec3 )
{
	Vector4 tmp0, tmp1, tmp2, tmp3;
	tmp0 = Vector4( vec_mergeh( vec0.get128(), vec2.get128() ) );
	tmp1 = Vector4( vec_mergeh( vec1.get128(), vec3.get128() ) );
	tmp2 = Vector4( vec_mergel( vec0.get128(), vec2.get128() ) );
	tmp3 = Vector4( vec_mergel( vec1.get128(), vec3.get128() ) );
	xxxx = Vector4( vec_mergeh( tmp0.get128(), tmp1.get128() ) );
	yyyy = Vector4( vec_mergel( tmp0.get128(), tmp1.get128() ) );
	zzzz = Vector4( vec_mergeh( tmp2.get128(), tmp3.get128() ) );
	wwww = Vector4( vec_mergel( tmp2.get128(), tmp3.get128() ) );
}

inline void toAos( Vector4& vec0, Vector4& vec1, Vector4& vec2, Vector4& vec3,
	const Vector4& xxxx, const Vector4& yyyy, const Vector4& zzzz, const Vector4& wwww )
{
	vec0 = Vector4( xxxx.getX(), yyyy.getX(), zzzz.getX(), wwww.getX() );
	vec1 = Vector4( xxxx.getY(), yyyy.getY(), zzzz.getY(), wwww.getY() );
	vec2 = Vector4( xxxx.getZ(), yyyy.getZ(), zzzz.getZ(), wwww.getZ() );
	vec3 = Vector4( xxxx.getW(), yyyy.getW(), zzzz.getW(), wwww.getW() );
}

void extractFrustumPlanes( Vector4 planes[6], const Matrix4& vp )
{
	floatInVec lengthInv;

	// each plane must be normalized, normalization formula is quite self explanatory

	planes[0] = vp.getRow( 0 ) + vp.getRow( 3 );
	lengthInv = floatInVec( 1.0f ) / length( planes[0].getXYZ() );
	planes[0] = Vector4( planes[0].getXYZ() * lengthInv, planes[0].getW() * lengthInv );

	planes[1] = -vp.getRow( 0 ) + vp.getRow( 3 );
	lengthInv = floatInVec( 1.0f ) / length( planes[1].getXYZ() );
	planes[1] = Vector4( planes[1].getXYZ() * lengthInv, planes[1].getW() * lengthInv );

	planes[2] = vp.getRow( 1 ) + vp.getRow( 3 );
	lengthInv = floatInVec( 1.0f ) / length( planes[2].getXYZ() );
	planes[2] = Vector4( planes[2].getXYZ() * lengthInv, planes[2].getW() * lengthInv );

	planes[3] = -vp.getRow( 1 ) + vp.getRow( 3 );
	lengthInv = floatInVec( 1.0f ) / length( planes[3].getXYZ() );
	planes[3] = Vector4( planes[3].getXYZ() * lengthInv, planes[3].getW() * lengthInv );

	planes[4] = vp.getRow( 2 ) + vp.getRow( 3 );
	lengthInv = floatInVec( 1.0f ) / length( planes[4].getXYZ() );
	planes[4] = Vector4( planes[4].getXYZ() * lengthInv, planes[4].getW() * lengthInv );

	planes[5] = -vp.getRow( 2 ) + vp.getRow( 3 );
	lengthInv = floatInVec( 1.0f ) / length( planes[5].getXYZ() );
	planes[5] = Vector4( planes[5].getXYZ() * lengthInv, planes[5].getW() * lengthInv );
}

ViewFrustum extractFrustum( const Matrix4& vp, bool dxStyleProjection )
{
	// http://www.lighthouse3d.com/opengl/viewfrustum/index.php?clipspace
	//
	ViewFrustum f;

	extractFrustumPlanes( f.planes, vp );

	f.planes[ViewFrustum::ePlane_ext0] = f.planes[ViewFrustum::ePlane_near];
	f.planes[ViewFrustum::ePlane_ext1] = f.planes[ViewFrustum::ePlane_far];

	toSoa( f.xPlaneLRBT, f.yPlaneLRBT, f.zPlaneLRBT, f.wPlaneLRBT,
		f.planes[0], f.planes[1], f.planes[2], f.planes[3] );
	toSoa( f.xPlaneNFNF, f.yPlaneNFNF, f.zPlaneNFNF, f.wPlaneNFNF,
		f.planes[4], f.planes[5], f.planes[6], f.planes[7] );

	extractFrustumCorners( f.corners, vp, dxStyleProjection );

	return f;
}
