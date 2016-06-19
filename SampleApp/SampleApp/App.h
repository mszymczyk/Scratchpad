#include <Core/AppBase/AppBase.h>
#include <Core/Gfx/Mesh/Model.h>
//#include <Util/HlslTypes.h>
#include <shaders/hlsl/PassConstants.h>
#include <Core/Gfx/Camera.h>
#include <Core/Gfx/Text/TextRenderer.h>
//#include <Gfx/Material/MaterialManager.h>
#include <Gfx/Material/MaterialShader.h>

namespace spad
{
	class App : public AppBase
	{
	public:
		~App()
		{
			ShutDown();
		}

		bool StartUp();

	protected:
		void ShutDown();
		void UpdateAndRender( const Timer& timer ) override;
		//void RenderModel( Dx11DeviceContext& deviceContext, Model& model, const FxLib::FxRuntimePass& fxPass );
		void RenderModel( Dx11DeviceContext& deviceContext, Model& model, const Matrix4& world, const IMaterialShader& material, const char* passName );

		RenderTarget2D mainRT_ = RenderTarget2D( "mainRT_" );
		DepthStencil mainDS_ = DepthStencil( "mainDS_" );
		Model modelCharacter_;
		Model modelBox_;

		FxRuntimePtr textureShader_;
		FxRuntimePtr meshShader_;
		MaterialShaderPtr materialShader_;
		MaterialShaderInstancePtr materialShaderInstance_;
		ConstantBuffer<CbPassConstants> passConstants_;
		ConstantBuffer<CbObjectConstants> objectConstants_;

		FirstPersonCamera camera_ = FirstPersonCamera( 1, deg2rad(60.0f), 0.1f, 200.0f );
		OrthographicCamera shadowCamera_ = OrthographicCamera( -10, -10, 10, 10, 1, 20 );

		BMFont font_consolas_;
		TextRenderer textRenderer_;
		FpsCounter fpsCounter_;
	};
}

