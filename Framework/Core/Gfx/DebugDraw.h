#pragma once

#include "dx11/Dx11DeviceContext.h"
#include "Math/ViewFrustum.h"

namespace spad
{

	namespace debugDraw
	{
		void AddLineWS( const Vector3& from, const Vector3& to, const u32 colorABGR, float lineWidth = 1.0f, bool depthEnabled = true );
		// creates plane on ZX axis, with normal pointing along Y axis (right handed system)
		void AddPlaneWS( const Vector4& plane, float xSize, float ySize, u32 xSubdivs, u32 ySubdivs, const u32 colorABGR, float lineWidth = 1.0f, bool depthEnabled = true );
		void AddFrustum( const ViewFrustum& frustum, const u32 colorABGR, float lineWidth = 1.0f, bool depthEnabled = true );
		void AddLineListWS( std::vector<Vector3>&& verts, const u32 colorABGR, float lineWidth = 1.0f, bool depthEnabled = true );
		void AddLineListSS( std::vector<Vector3>&& verts, const u32 colorABGR, float lineWidth = 1.0f );
		void AddLineStripWS( std::vector<Vector3>&& verts, const u32 colorABGR, float lineWidth = 1.0f, bool depthEnabled = true );
		void AddLineStripSS( std::vector<Vector3>&& verts, const u32 colorABGR, float lineWidth = 1.0f );

		namespace DontTouchThis
		{
			void Initialize( ID3D11Device* dxDevice );
			void DeInitialize();

			void Draw( Dx11DeviceContext& deviceContext, const Matrix4& view, const Matrix4& proj );
			void Clear();
		};
	} // namespace debug

} // namespace spad
