#pragma once

#include "BMFont.h"
#include "..\dx11\Dx11Wrappers.h"
#include "..\dx11\Dx11DeviceContext.h"
#include "..\Dx11\Dx11Shader.h"
#include <Shaders/hlsl/TextRendererConstants.h>

namespace spad
{

class TextRenderer
{
public:
	TextRenderer();
	~TextRenderer() {}

	void Initialize( ID3D11Device* device );

	//u32 setColor( u32 colorABGR );
	//float setFontSize( float newSizeInPixels );
	void setKerning( bool enabledOrNot )
	{
		enableKerning_ = enabledOrNot;
	}
	//bool getKerning() const { return enableKerning_; }
	//void printf( GdiCommandQueue* gdiContext, float x, float y, const char* format, ... );

	void begin( Dx11DeviceContext& deviceContext, const BMFont* bmFont, u32 canvasWidth, u32 canvasHeight );
	void end( Dx11DeviceContext& deviceContext );
	void printf( Dx11DeviceContext& deviceContext, float x, float y, u32 colorABGR, float fontScale, const char* format, ... );

	// x01 and y01 are in <0,1> range
	//
	void printfPN( Dx11DeviceContext& deviceContext, float x01, float y01, u32 colorABGR, float fontScale, const char* format, ... );
	//void puts( GdiCommandQueue* gdiContext, float x, float y, const char* text );
	//void puts( GdiCommandQueue* gdiContext, float x, float y, u32 colorABGR, float fontSize, const char* text );

	const BMFont* getCurrentFont() const { return currentFont_; }
	u32 getCanvasWidth() const { return canvasWidth_; }
	u32 getCanvasHeight() const { return canvasHeight_; }

protected:

	void _Draw( Dx11DeviceContext& deviceContext, float x, float y, u32 colorABGR, float fontScale, const char* textUtf8 );

protected:
	HlslShaderPtr shi_;
	//std::unique_ptr<BMFont> font_;
	RingBuffer vertices_;
	IndexBuffer indices_ = IndexBuffer( "TextRendererIndices" );
	ConstantBuffer<CbTextRendererConstants> textRendererConstants_;
	const BMFont* currentFont_ = nullptr;
	u32 canvasWidth_ = 0;
	u32 canvasHeight_ = 0;
	//u32 colorABGR_;
	//float fontSize_;
	bool enableKerning_ = false;
}; // TextRenderer

} // namespace spad
