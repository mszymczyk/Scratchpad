#include "SettingsTest_pch.h"
#include "SettingsTestApp.h"
#include <AppBase/Input.h>
#include <Gfx\Dx11/Dx11DeviceStates.h>
#include <Gfx\DebugDraw.h>

#include "..\SampleSettings\SampleSettings_Group.h"
using namespace SampleSettingsNamespace;

namespace spad
{
	bool SettingsTestApp::StartUp()
	{
		ID3D11Device* dxDevice = dx11_->getDevice();

		font_consolas_.Initialize( dxDevice, "Data\\Fonts\\consolas.fnt" );
		textRenderer_.Initialize( dxDevice );

		mainDS_.Initialize( dxDevice, dx11_->getBackBufferWidth(), dx11_->getBackBufferHeight() );

		gSampleSettings = new SampleSettingsNamespace::SampleSettingsWrap( "SettingsTest\\SampleSettings.settings" );

		return true;
	}

	void SettingsTestApp::ShutDown()
	{
		delete gSampleSettings;
		gSampleSettings = nullptr;
	}

	void DrawAnimCurve( Dx11DeviceContext& c, float y, const SettingsEditor::AnimCurve& ac, u32 nSteps, u32 colorABGR )
	{
		std::vector<Vector3> verts( nSteps );
		float t = 0.0f;
		float deltaStep = 1.0f / ( nSteps - 1 );
		for ( u32 i = 0; i < nSteps; ++i )
		{
			verts[i] = Vector3( t * 2 - 1, y + ac.eval( t ) * 0.2f, 0.0f );
			t += deltaStep;
		}

		debugDraw::AddLineStripSS( std::move( verts ), colorABGR, 1.0f );
	}

	void DrawInner( Dx11DeviceContext& c, TextRenderer& tr, float y, const Group::Inner* preset )
	{
		float x = 0.05f;
		float dy = (float)tr.getCurrentFont()->getLineHeight() / (float)tr.getCanvasHeight();
		float fscale = 1.0f;
		u32 colorABGR = preset->color.toABGR();
		tr.printfPN( c, x, y, colorABGR, fscale, "floatParam: %3.3f", preset->floatParam );
		y += dy;
		tr.printfPN( c, x, y, colorABGR, fscale, "boolParam: %s", preset->boolParam ? "true" : "false" );
		y += dy;
		tr.printfPN( c, x, y, colorABGR, fscale, "intParam: %d", preset->intParam );
		y += dy;
		tr.printfPN( c, x, y, colorABGR, fscale, "enumParam: %d", (int)preset->enumParam );
		y += dy;
		tr.printfPN( c, x, y, colorABGR, fscale, "stringParam: %s", preset->stringParam );
		y += dy;
		DrawAnimCurve( c, ( 1 - y ) * 2 - 1 - 0.2f, preset->animCurve, 200, colorABGR );
	}

	void DrawFrameworkSettingsText( Dx11DeviceContext& c, TextRenderer& tr )
	{
		tr.printfPN( c, 0.1f, 0.1f, 0xffffffff, 0.5f, "sample string: %s", gSampleSettings->mGroup->sampleString );

		// preset
		float y = 0.05f;
		DrawInner( c, tr, y, &gSampleSettings->mGroup->mInner );
		//y += 0.3f;
		y = 0.5f;
		DrawInner( c, tr, y, gSampleSettings->mGroup->mInner.getPreset( "Preset0" ) );
	}

	//void DrawFrameworkSettingsCurves( Dx11DeviceContext& c )
	//{
	//	const SettingsEditor::AnimCurve& ac = gSampleSettings->mGroup->animCurve;
	//	const u32 nSteps = 100;
	//	std::vector<Vector3> verts;
	//	verts.resize( nSteps );
	//	float t = 0.0f;
	//	float deltaStep = 1.0f / (nSteps-1);
	//	for ( u32 i = 0; i < nSteps; ++i )
	//	{
	//		verts[i] = Vector3( t * 1.8f - 0.9f, ac.eval( t ) * 0.2f, 0.0f );;
	//		t += deltaStep;
	//	}

	//	//debugDraw::AddLineListWS( std::move(verts), 0xff00ff00, 1.0f );
	//	debugDraw::AddLineStripSS( std::move( verts ), gSampleSettings->mGroup->color.toABGR(), 1.0f );
	//}

	void SettingsTestApp::UpdateAndRender( const Timer& timer )
	{
		fpsCounter_.update(timer);

		float dt = (float)timer.getDeltaSeconds();

		Dx11DeviceContext& immediateContextWrapper = dx11_->getImmediateContextWrapper();
		ID3D11DeviceContext* immediateContext = immediateContextWrapper.context;
		immediateContext->ClearState();

		MouseState mouseState = MouseState::GetMouseState( hWnd_ );
		KeyboardState kbState = KeyboardState::GetKeyboardState( hWnd_ );

		float CamMoveSpeed = 5.0f * dt;
		const float CamRotSpeed = (0.180f * 1.0f) * dt;

		// Move the camera with keyboard input
		if ( kbState.IsKeyDown( KeyboardState::LeftShift ) )
			CamMoveSpeed *= 0.25f;

		dx11_->SetBackBufferRT();
		// Set default render targets
		ID3D11RenderTargetView* rtviews[1] = { dx11_->getBackBufferRTV() };
		immediateContext->OMSetRenderTargets( 1, rtviews, mainDS_.dsv_ );

		// Setup the viewport
		D3D11_VIEWPORT vp;
		vp.Width = static_cast<float>( dx11_->getBackBufferWidth() );
		vp.Height = static_cast<float>( dx11_->getBackBufferHeight() );
		vp.MinDepth = 0.0f;
		vp.MaxDepth = 1.0f;
		vp.TopLeftX = 0;
		vp.TopLeftY = 0;
		immediateContext->RSSetViewports( 1, &vp );


		const float clearColor[] = { 0.2f, 0.2f, 0.2f, 1 };
		immediateContext->ClearRenderTargetView( dx11_->getBackBufferRTV(), clearColor );
		immediateContext->ClearDepthStencilView( mainDS_.dsv_, D3D11_CLEAR_DEPTH | D3D11_CLEAR_STENCIL, 1, 0 );

		immediateContext->RSSetState( RasterizerStates::BackFaceCull() );
		immediateContext->OMSetDepthStencilState( DepthStencilStates::DepthWriteEnabled(), 0 );

		debugDraw::AddPlaneWS( Vector4::yAxis(), 10, 10, 10, 10, 0xffffffff, 1, true ); // ok

		Vector3 dir = Vector3( gSampleSettings->mGroup->sampleDir.x, gSampleSettings->mGroup->sampleDir.y, gSampleSettings->mGroup->sampleDir.z );
		Vector4 plane( dir, 0 );
		debugDraw::AddPlaneWS( plane, 5, 5, 5, 5, 0xff0000ff, 1, true );
		//debugDraw::AddPlaneWS( Vector4::xAxis(), 10, 10, 10, 10, 0xff00ffff, 1, true );

		//DrawFrameworkSettingsCurves( immediateContextWrapper );

		Matrix4 view = ( Matrix4::lookAt( Point3( 6, 3, 6 ), Point3( 0, 0, 0 ), Vector3::yAxis() ) );

		float aspect = (float)dx11_->getBackBufferWidth() / (float)dx11_->getBackBufferHeight();
		debugDraw::DontTouchThis::Draw( immediateContextWrapper, view, Matrix4::perspective( deg2rad(60.0f), aspect, 0.1f, 100 ) );
		debugDraw::DontTouchThis::Clear();

		textRenderer_.begin( immediateContextWrapper, &font_consolas_, dx11_->getBackBufferWidth(), dx11_->getBackBufferHeight() );

		DrawFrameworkSettingsText( immediateContextWrapper, textRenderer_ );

		textRenderer_.end( immediateContextWrapper );
	}

}
