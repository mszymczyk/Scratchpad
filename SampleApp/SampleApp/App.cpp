#include "SampleApp_pch.h"
#include "App.h"
#include <FxLib/FxLib.h>
#include <AppBase/Input.h>
#include <Gfx\Dx11/Dx11DeviceStates.h>
#include <Gfx\DebugDraw.h>
//#include <Gfx\Material\MaterialShaderFactory.h>
#include <Gfx/Material/MaterialManager.h>

#include <FrameworkSettings/FrameworkSettings_General.h>
using namespace FrameworkSettingsNamespace;

namespace spad
{
	using namespace FxLib;

	//ID3D11Buffer* vertexBuffer;
	//ID3D11InputLayout* vertexLayout;
	//ID3D11RasterizerStatePtr noCull;

	bool App::StartUp()
	{
		//FxLib::FxFile fxFile;
		//std::string fileFullPath = GetAbsolutePath( "code\\shaders\\hlsl\\font.hlsl" );
		//int ires = fxFile.loadCompiledFxFile( fileFullPath.c_str() );
		//SPAD_ASSERT( ires == 0 );
		ID3D11Device* dxDevice = dx11_->getDevice();

		//std::unique_ptr<FxLib::FxRuntime> fx = fxFile.createFxRuntime( dx11_->getDevice() );

		textureShader_ = FxLib::loadCompiledFxFile( dx11_->getDevice(), "DataWin\\Shaders\\Texture.hlsl" );
		meshShader_ = FxLib::loadCompiledFxFile( dx11_->getDevice(), "DataWin\\Shaders\\Mesh.hlsl" );

		//MaterialShaderFactory msf( dxDevice );
		//std::unique_ptr<MaterialShader> materialShader( msf.CreateMaterialShader( "Assets\\Materials\\mat1.material" ) );
		//materialShader_.reset( msf.CreateMaterialShader( "Assets\\Materials\\mat1.material" ) );
		MaterialManager::Initialize( dxDevice );
		materialShader_ = gMaterialManager->LoadMaterialShader( "Assets\\Materials\\mat1.material" );
		materialShaderInstance_ = gMaterialManager->LoadMaterialShaderInstance( "Assets\\Materials\\mat1.material", "MaterialInstance" );

		font_consolas_.Initialize( dxDevice, "Data\\Fonts\\consolas.fnt" );
		textRenderer_.Initialize( dxDevice );

		//Model model;
		//modelCharacter_.CreateFromSDKMeshFile( dx11_->getDevice(), "data\\Models\\Soldier\\Soldier.sdkmesh" );
		//modelCharacter_.CreateFromSDKMeshFile( dx11_->getDevice(), "data\\Models\\Powerplant\\Powerplant.sdkmesh" );
		//modelCharacter_.CreateFromSDKMeshFile( dx11_->getDevice(), "Data\\Models\\Tower\\Tower.sdkmesh" );
		modelBox_.GenerateBoxScene( dxDevice );

		//ID3D11Device* dxDevice = dx11_->getDevice();

		//XMFLOAT3 vertices[] =
		//{
		//	XMFLOAT3( 0.0f, 0.5f, 0.5f ),
		//	XMFLOAT3( 0.5f, -0.5f, 0.5f ),
		//	XMFLOAT3( -0.5f, -0.5f, 0.5f ),
		//};
		//D3D11_BUFFER_DESC bd;
		//ZeroMemory( &bd, sizeof( bd ) );
		//bd.Usage = D3D11_USAGE_DEFAULT;
		//bd.ByteWidth = sizeof( XMFLOAT3 ) * 3;
		//bd.BindFlags = D3D11_BIND_VERTEX_BUFFER;
		//bd.CPUAccessFlags = 0;
		//D3D11_SUBRESOURCE_DATA InitData;
		//ZeroMemory( &InitData, sizeof( InitData ) );
		//InitData.pSysMem = vertices;
		//HRESULT hr = dxDevice->CreateBuffer( &bd, &InitData, &vertexBuffer );
		//if ( FAILED( hr ) )
		//	return false;

		//const FxRuntimePass& fillPass = textureShader_->getPass( "Fill" );

		//// Define the input layout
		//D3D11_INPUT_ELEMENT_DESC layout[] =
		//{
		//	{ "POSITION", 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 0, D3D11_INPUT_PER_VERTEX_DATA, 0 },
		//};
		//UINT numElements = ARRAYSIZE( layout );

		//// Create the input layout
		//hr = dxDevice->CreateInputLayout( layout, numElements, fillPass.vsInputSignature_->GetBufferPointer(), fillPass.vsInputSignature_->GetBufferSize(), &vertexLayout );
		//if ( FAILED( hr ) )
		//	return false;

		passConstants_.Initialize( dx11_->getDevice() );
		objectConstants_.Initialize( dx11_->getDevice() );

		mainDS_.Initialize( dxDevice, dx11_->getBackBufferWidth(), dx11_->getBackBufferHeight() );

		//D3D11_RASTERIZER_DESC rastDesc;
		//rastDesc.AntialiasedLineEnable = false;
		//rastDesc.CullMode = D3D11_CULL_BACK;
		//rastDesc.DepthBias = 0;
		//rastDesc.DepthBiasClamp = 0.0f;
		//rastDesc.DepthClipEnable = true;
		////rastDesc.FillMode = D3D11_FILL_WIREFRAME;
		//rastDesc.FillMode = D3D11_FILL_SOLID;
		//rastDesc.FrontCounterClockwise = true;
		//rastDesc.MultisampleEnable = true;
		//rastDesc.ScissorEnable = false;
		//rastDesc.SlopeScaledDepthBias = 0;

		//DXCall( dxDevice->CreateRasterizerState( &rastDesc, &noCull) );

		// Camera setup
		//camera_.SetPosition( Vector3( 50.0f, 5.0f, 5.0f ) );
		//camera_.SetYRotation( XM_PIDIV2 );
		camera_.SetPosition( Vector3( 0, 1, 10 ) );

		shadowCamera_.SetLookAt( Vector3( 5.0f ), Vector3( 0.0f ), Vector3::yAxis() );

		return true;
	}

	void App::ShutDown()
	{
		//textureShader_ = nullptr;
		//meshShader_ = nullptr;
		materialShader_ = nullptr;
		materialShaderInstance_ = nullptr;
		MaterialManager::DeInitialize();
	}

	void DrawFrameworkSettingsText( Dx11DeviceContext& c, TextRenderer& tr )
	{
		tr.printfPN( c, 0.1f, 0.1f, 0xffffffff, 0.5f, "sample string: %s", gFrameworkSettings->mGeneral->sampleString );
	}

	void DrawFrameworkSettingsCurves( Dx11DeviceContext& c )
	{
		const SettingsEditor::AnimCurve& ac = gFrameworkSettings->mGeneral->mInner.animCurvePreset;
		const u32 nSteps = 100;
		std::vector<Vector3> verts;
		verts.resize( nSteps );
		float t = 0.0f;
		float deltaStep = 1.0f / (nSteps-1);
		for ( u32 i = 0; i < nSteps; ++i )
		{
			verts[i] = Vector3( t * 1.8f - 0.9f, ac.eval( t ) * 0.2f, 0.0f );;
			t += deltaStep;
		}

		//debugDraw::AddLineListWS( std::move(verts), 0xff00ff00, 1.0f );
		debugDraw::AddLineStripSS( std::move( verts ), 0xff00ff00, 1.0f );
	}

	void App::UpdateAndRender( const Timer& timer )
	{
		fpsCounter_.update(timer);

		float dt = (float)timer.getDeltaSeconds();

		Dx11DeviceContext& immediateContextWrapper = dx11_->getImmediateContextWrapper();
		ID3D11DeviceContext* immediateContext = immediateContextWrapper.context;
		immediateContext->ClearState();

		MouseState mouseState = MouseState::GetMouseState( hWnd_ );
		KeyboardState kbState = KeyboardState::GetKeyboardState( hWnd_ );

		//if ( kbState.IsKeyDown( KeyboardState::Escape ) )
		//	window.Destroy();

		//float CamMoveSpeed = 5.0f * dt;
		float CamMoveSpeed = 5.0f * dt;
		//const float CamRotSpeed = 0.180f * dt;
		const float CamRotSpeed = (0.180f * 1.0f) * dt;
		//const float MeshRotSpeed = 0.180f * dt;

		// Move the camera with keyboard input
		if ( kbState.IsKeyDown( KeyboardState::LeftShift ) )
			CamMoveSpeed *= 0.25f;

		camera_.SetAspectRatio( (float)dx11_->getBackBufferWidth() / (float)dx11_->getBackBufferHeight() );

		Vector3 camPos = camera_.Position();
		if ( kbState.IsKeyDown( KeyboardState::W ) )
			camPos += camera_.Forward() * CamMoveSpeed;
		else if ( kbState.IsKeyDown( KeyboardState::S ) )
			camPos += camera_.Back() * CamMoveSpeed;
		if ( kbState.IsKeyDown( KeyboardState::A ) )
			camPos += camera_.Left() * CamMoveSpeed;
		else if ( kbState.IsKeyDown( KeyboardState::D ) )
			camPos += camera_.Right() * CamMoveSpeed;
		if ( kbState.IsKeyDown( KeyboardState::Q ) )
			camPos += camera_.Up() * CamMoveSpeed;
		else if ( kbState.IsKeyDown( KeyboardState::E ) )
			camPos += camera_.Down() * CamMoveSpeed;

		camera_.SetPosition( camPos );

		// Rotate the camera with the mouse
		if ( mouseState.RButton.Pressed && mouseState.IsOverWindow )
		{
			float xRot = camera_.XRotation();
			float yRot = camera_.YRotation();
			xRot -= mouseState.DY * CamRotSpeed;
			yRot -= mouseState.DX * CamRotSpeed;
			camera_.SetXRotation( xRot );
			camera_.SetYRotation( yRot );
		}

		Matrix4 cameraWorld = Matrix4::translation( Vector3( 0, 5, 20 ) );
		//passConstants_.data.View = Matrix4::identity();
		//passConstants_.data.View = affineInverse( cameraWorld );
		//{
		//	Matrix4 proj = Matrix4::perspective( deg2rad( 60.0f ), (float)dx11_->getBackBufferWidth() / (float)dx11_->getBackBufferHeight(), 0.1f, 100.0f );
		//	Matrix4 sc = Matrix4::scale( Vector3( 1, 1, 0.5f ) );
		//	Matrix4 tr = Matrix4::translation( Vector3( 0, 0, 1 ) );
		//	Matrix4 newProj = sc * tr * proj;
		//	passConstants_.data.Projection = newProj;
		//}
		passConstants_.data.View = camera_.ViewMatrix();
		passConstants_.data.Projection = camera_.ProjectionMatrix();
		passConstants_.data.ViewProjection = passConstants_.data.Projection * passConstants_.data.View;
		passConstants_.updateGpu( immediateContext );
		passConstants_.setVS( immediateContext, 0 );


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

		//context->RSSetState( noCull );
		immediateContext->RSSetState( RasterizerStates::BackFaceCull() );
		immediateContext->OMSetDepthStencilState( DepthStencilStates::DepthWriteEnabled(), 0 );

		//// Set vertex buffer
		//UINT stride = sizeof( XMFLOAT3 );
		//UINT offset = 0;
		//immediateContext->IASetVertexBuffers( 0, 1, &vertexBuffer, &stride, &offset );

		//immediateContext->IASetInputLayout( vertexLayout );

		//RenderModel( immediateContextWrapper, modelCharacter_, meshShader_->getPass( "Wireframe" ) );

		materialShader_->updateMaterialParams( immediateContextWrapper );
		materialShader_->bindMaterialParamsUniformsVSPS( immediateContextWrapper );

		//RenderModel( immediateContextWrapper, modelBox_, *materialShader_->getPass( "Pass0" ) );
		RenderModel( immediateContextWrapper, modelBox_, Matrix4::translation( Vector3(-2,0,0) ) * Matrix4::scale( Vector3(0.5f) ), *materialShader_.get(), "Pass0" );

		materialShaderInstance_->updateMaterialInstanceParams( immediateContextWrapper );
		materialShaderInstance_->bindMaterialInstanceUniformsVSPS( immediateContextWrapper );

		RenderModel( immediateContextWrapper, modelBox_, Matrix4::translation( Vector3( 2, 0, 0 ) ) * Matrix4::scale( Vector3( 0.5f ) ), *materialShaderInstance_.get(), "Pass0" );

		//const FxRuntimePass& fillPass = textureShader_->getPass( "Fill" );
		//fillPass.setVS( immediateContext );
		//fillPass.setPS( immediateContext );

		//immediateContext->IASetPrimitiveTopology( D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST );
		//immediateContext->Draw( 3, 0 );

		//const float clearColor2[] = { 1, 0, 1, 1 };
		//immediateContext->ClearRenderTargetView( dx11_->getBackBufferRTV(), clearColor2 );

		//debugDraw::AddLineWS( Vector3( -1, 0, 0 ), Vector3( 1, 0, 0 ), 0xff0000ff, 1, true );
		// ground grid
		debugDraw::AddPlaneWS( Vector4::yAxis(), 10, 10, 10, 10, 0xffffffff, 1, true ); // ok

		//Vector3 dir = normalize( Vector3( 0, 2, 1 ) );
		//Vector4 plane( dir, 1 );
		//debugDraw::AddPlaneWS( plane, 5, 5, 5, 5, 0xff00ffff, 1, true );
		//debugDraw::AddPlaneWS( Vector4::zAxis(), 10, 10, 10, 10, 0xff00ffff, 1, true );

		ViewFrustum shadowViewFrustum = extractFrustum( shadowCamera_.ViewProjectionMatrix(), true );
		debugDraw::AddFrustum( shadowViewFrustum, 0xff0000ff, 1, true );

		DrawFrameworkSettingsCurves( immediateContextWrapper );

		debugDraw::DontTouchThis::Draw( immediateContextWrapper, camera_.ViewMatrix(), camera_.ProjectionMatrix() );
		debugDraw::DontTouchThis::Clear();

		textRenderer_.begin( immediateContextWrapper, &font_consolas_, dx11_->getBackBufferWidth(), dx11_->getBackBufferHeight() );
		//float x = 0;
		//float y = 25 * 5;
		//textRenderer_.printf( immediateContextWrapper, x, y, 0xff0000ff, 5.0f, "Aa12345" );
		//textRenderer_.printf( immediateContextWrapper, x, y, 0xffff0000, 1.0f, "Aa12345" );
		//textRenderer_.printfPN( immediateContextWrapper, 0.1f, 0.2f, 0xffff0000, 1.0f, "Aa12345" );
		dxmath::Float3 cp = camera_.Position();
		textRenderer_.printfPN( immediateContextWrapper, 0.01f, 0.015f, 0xffffffff, 1.0f
			, "Camera: Pos: %3.3f, %3.3f, %3.3f, Yaw: %3.3f, Pitch: %3.3f"
			, cp.x, cp.y, cp.z, camera_.YRotation(), camera_.XRotation() );
		//textRenderer_.printf( immediateContextWrapper, 
		textRenderer_.printfPN(immediateContextWrapper, 0.92f, 0.01f, 0xffffffff, 0.5f
			, "FPS: %3.1f", fpsCounter_.getFrameRate());

		DrawFrameworkSettingsText( immediateContextWrapper, textRenderer_ );

		textRenderer_.end( immediateContextWrapper );
	}

	//void App::RenderModel( Dx11DeviceContext& deviceContext, Model& model, const FxRuntimePass& fxPass )
	void App::RenderModel( Dx11DeviceContext& deviceContext, Model& model, const Matrix4& world, const IMaterialShader& material, const char* passName )
	{
		ID3D11DeviceContext* context = deviceContext.context;

		const MaterialShaderPass& fxPass = *material.getPass( passName );
		//fxPass.setVS( context );
		//fxPass.setPS( context );
		ContextShaderBindings csb;
		//fxPass.fill( csb );
		material.fill( csb, fxPass );
		csb.commit( deviceContext );

		//Matrix4 cameraWorld = Matrix4::translation( Vector3( 0, 5, 20 ) );
		////passConstants_.data.View = Matrix4::identity();
		//passConstants_.data.View = affineInverse( cameraWorld );
		//{
		//	Matrix4 proj = Matrix4::perspective( deg2rad(60.0f), (float)dx11_->getBackBufferWidth() / (float)dx11_->getBackBufferHeight(), 0.1f, 100.0f );
		//	Matrix4 sc = Matrix4::scale( Vector3( 1, 1, 0.5f ) );
		//	Matrix4 tr = Matrix4::translation( Vector3( 0, 0, 1 ) );
		//	Matrix4 newProj = sc * tr * proj;
		//	passConstants_.data.Projection = newProj;
		//}
		//passConstants_.data.ViewProjection = passConstants_.data.Projection * passConstants_.data.View;
		//passConstants_.updateGpu( context );
		//passConstants_.setVS( context, 0 );

		objectConstants_.data.World = world;
		//objectConstants_.data.World = Matrix4::translation( Vector3( 0, 0, -5 ) );
		objectConstants_.data.WorldIT = transpose( affineInverse( objectConstants_.data.World ) );
		objectConstants_.updateGpu( context );
		objectConstants_.setVS( context, 1 );

		context->PSSetSamplers( 0, 1, &SamplerStates::anisotropic );

		Dx11InputLayoutCache& inputLayoutCache = deviceContext.inputLayoutCache;

		for ( u32 meshIdx = 0; meshIdx < model.Meshes().size(); ++meshIdx )
		{
			Mesh& mesh = model.Meshes()[meshIdx];

			// Set the vertices and indices
			ID3D11Buffer* vertexBuffers[1] = { mesh.VertexBuffer() };
			u32 vertexStrides[1] = { mesh.VertexStride() };
			u32 offsets[1] = { 0 };
			context->IASetVertexBuffers( 0, 1, vertexBuffers, vertexStrides, offsets );
			context->IASetIndexBuffer( mesh.IndexBuffer(), mesh.IndexBufferFormat(), 0 );
			context->IASetPrimitiveTopology( D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST );

			// Set the input layout
			//context->IASetInputLayout( meshData.InputLayouts[meshIdx] );
			inputLayoutCache.setInputLayout( context, mesh.InputElementsHash(), fxPass.vsInputSignatureHash_
				, mesh.InputElements(), mesh.NumInputElements(), reinterpret_cast<const u8*>( fxPass.vsInputSignature_->GetBufferPointer() ), (u32)fxPass.vsInputSignature_->GetBufferSize() );

			// Draw all parts
			for ( size_t partIdx = 0; partIdx < mesh.MeshParts().size(); ++partIdx )
			{
				const MeshPart& part = mesh.MeshParts()[partIdx];
				//const MeshMaterial& material = model.Materials()[part.MaterialIdx];

				//// Set the textures
				//ID3D11ShaderResourceView* psTextures[3] =
				//{
				//	material.DiffuseMap,
				//	shadowMap.SRView,
				//	randomRotations,
				//};

				//if ( psTextures[0] == nullptr )
				//	psTextures[0] = defaultTexture;

				//if ( AppSettings::UseFilterableShadows() )
				//	psTextures[1] = varianceShadowMap.SRView;

				//context->PSSetShaderResources( 0, 3, psTextures );
				//ID3D11ShaderResourceView* psTextures[1] = { material.DiffuseMap };
				//context->PSSetShaderResources( 0, 1, psTextures );
				context->DrawIndexed( part.IndexCount, part.IndexStart, 0 );
			}
		}
	}

}
