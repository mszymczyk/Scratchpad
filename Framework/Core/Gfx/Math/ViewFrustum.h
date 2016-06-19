#pragma once

#include <Util\vectormath.h>

// planes point inward, into center of frustum
struct ViewFrustum
{
	// 8 planes in SOA format
	//
	Vector4 xPlaneLRBT, yPlaneLRBT, zPlaneLRBT, wPlaneLRBT;
	Vector4 xPlaneNFNF, yPlaneNFNF, zPlaneNFNF, wPlaneNFNF;

	enum e_Plane
	{
		ePlane_left,
		ePlane_right,
		ePlane_bottom,
		ePlane_top,
		ePlane_near,
		ePlane_far,
		ePlane_ext0,
		ePlane_ext1,
		ePlane_count
	};

	enum e_Corner
	{
		eCorner_leftTopNear,
		eCorner_leftTopFar,
		eCorner_leftBottomNear,
		eCorner_leftBottomFar,
		eCorner_rightBottomNear,
		eCorner_rightBottomFar,
		eCorner_rightTopNear,
		eCorner_rightTopFar,
	};

	Vector4 planes[ePlane_count]; // planes in AOS format
	Vector3 corners[8];

}; // struct ViewFrustum

ViewFrustum extractFrustum( const Matrix4& viewProjection, bool dxStyleProjection );
