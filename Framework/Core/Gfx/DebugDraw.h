#pragma once

#include "dx11/Dx11DeviceContext.h"
#include "Math/ViewFrustum.h"

namespace spad
{

	namespace debugDraw
	{
		void AddLineWS( const Vector3& from, const Vector3& to, const u32 colorABGR, float lineWidth = 1.0f, bool depthEnabled = true );
		void AddPlaneWS( const Vector4& plane, float xSize, float zSize, u32 xSubdivs, u32 zSubdivs, const u32 colorABGR, float lineWidth = 1.0f, bool depthEnabled = true );
		void AddFrustum( const ViewFrustum& frustum, const u32 colorABGR, float lineWidth = 1.0f, bool depthEnabled = true );

		namespace DontTouchThis
		{
			void Initialize( ID3D11Device* dxDevice );
			void DeInitialize();

			void Draw( Dx11DeviceContext& deviceContext, const Matrix4& view, const Matrix4& proj );
			void Clear();
		};
	} // namespace debug

} // namespace spad
