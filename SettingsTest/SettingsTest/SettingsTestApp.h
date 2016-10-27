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
	class SettingsTestApp : public AppBase
	{
	public:
		~SettingsTestApp()
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

		BMFont font_consolas_;
		TextRenderer textRenderer_;
		FpsCounter fpsCounter_;
	};
}

