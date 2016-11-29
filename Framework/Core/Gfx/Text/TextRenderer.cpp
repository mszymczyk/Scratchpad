#include "Gfx_pch.h"
#include "TextRenderer.h"
#include "../dx11/Dx11DeviceStates.h"

#if defined(_MSC_VER) && defined(_DEBUG)
#define new _DEBUG_NEW
#endif

namespace spad
{
	struct _BMFontVertex
	{
		float x, y;
		float tx, ty;
	};

	void _AppendGlyph( _BMFontVertex* vertices, float x, float y, const BMFont::GlyphDesc& g )
	{
		_BMFontVertex* v = vertices;

		// left bottom
		v->x = x + g.xOffset;
		v->y = y + g.yOffset + g.height;
		v->tx = g.x;
		v->ty = g.y + (float)g.height;
		++v;

		// right bottom
		v->x = x + g.xOffset + g.width;
		v->y = y + g.yOffset + g.height;
		v->tx = g.x + (float)g.width;
		v->ty = g.y + (float)g.height;
		++v;

		// right top
		v->x = x + g.xOffset + g.width;
		v->y = y + g.yOffset;
		v->tx = g.x + (float)g.width;
		v->ty = g.y;
		++v;

		// left top
		v->x = x + g.xOffset;
		v->y = y + g.yOffset;
		v->tx = g.x;
		v->ty = g.y;
		++v;
	}

	TextRenderer::TextRenderer()
		//: shi_( NULL )
		//, font_( NULL )
		//, colorABGR_( 0xffffffff )
		//, fontSize_( 24.0f )
		//: enableKerning_( true )
	{	}

//fr::Result TextRenderer::startUp( ResourceManager* resourceManager, GdiContextThread* gdiThread, BMFont* font, TextRenderer** outText )
//{
//	TextRenderer* t = new TextRenderer();
//	t->shi_ = resourceManager->loadAbstractShader( gdiThread, "font", "font" );
//	if ( ! t->shi_ )
//	{
//		delete t;
//		return kFailure;
//	}
//
//	t->font_ = font;
//
//	*outText = t;
//	return kSuccess;
//}
//
//void TextRenderer::shutDown( ResourceManager* resourceManager, GdiContextThread* gdiThread, TextRenderer*& text )
//{
//	if ( text )
//	{
//		resourceManager->releaseShaderAndInstance( gdiThread, text->shi_ );
//		delete text;
//		text = NULL;
//	}
//}
//
//u32 TextRenderer::setColor( u32 colorABGR )
//{
//	u32 oldColor = colorABGR_;
//	colorABGR_ = colorABGR;
//	return oldColor;
//}
//
//float TextRenderer::setFontSize( float scale )
//{
//	float oldScale = fontSize_;
//	fontSize_ = scale;
//	return oldScale;
//}
//
//void TextRenderer::setKerning( bool enabledOrNot )
//{
//	enableKerning_ = enabledOrNot;
//}

//void TextRenderer::printf( GdiCommandQueue* gdiContext, float x, float y, const char* format, ... )
//{
//	va_list	args;
//	va_start( args, format );
//	char st_buffer[512];
//	vsnprintf( st_buffer, 512, format, args);
//	const char* ascii = st_buffer;
//	va_end( args );
//
//	if ( ! (ascii[0]) )
//		return;
//
//	_Draw( gdiContext, x, y, colorABGR_, fontSize_, ascii );
//}

	void TextRenderer::Initialize( ID3D11Device* device )
	{
		shi_ = LoadCompiledFxFile( "DataWin\\Shaders\\hlsl\\compiled\\TextRenderer.hlslc_packed" );

		const u32 nChars = 1024;

		// each character is 4 vertices
		vertices_.Initialize( device, nChars * 4 );

		std::vector<u16> indices;
		// each character is two triangles
		indices.resize( nChars * 6 );
		u16* tmpIndices = &indices[0];
		u16 baseVertex = 0;
		for ( u32 i = 0; i < nChars; ++i )
		{
			tmpIndices[0] = baseVertex;
			tmpIndices[1] = baseVertex + 1;
			tmpIndices[2] = baseVertex + 3;

			tmpIndices[3] = baseVertex + 3;
			tmpIndices[4] = baseVertex + 1;
			tmpIndices[5] = baseVertex + 2;

			baseVertex += 4;
			tmpIndices += 6;
		}
		indices_.Initialize( device, DXGI_FORMAT_R16_UINT, (u32)indices.size(), &indices[0] );

		textRendererConstants_.Initialize( device );
	}

	void TextRenderer::begin( Dx11DeviceContext& deviceContext, const BMFont* bmFont, u32 canvasWidth, u32 canvasHeight )
	{
		SPAD_ASSERT( !currentFont_ ); // has previous batch completed?
		currentFont_ = bmFont;

		const HlslShaderPass& fxPass = *shi_->getPass( "Text" );

		ID3D11DeviceContext* context = deviceContext.context;

		fxPass.setVS( context );
		fxPass.setPS( context );

		context->RSSetState( RasterizerStates::BackFaceCull() );
		//context->RSSetState( RasterizerStates::NoCull() );
		context->OMSetDepthStencilState( DepthStencilStates::DepthDisabled(), 0 );
		context->OMSetBlendState( BlendStates::alphaBlend, nullptr, 0xffffffff );

		context->PSSetShaderResources( 0, 1, bmFont->getTextures() );
		context->PSSetSamplers( 0, 1, &SamplerStates::linearClamp );

		context->IASetPrimitiveTopology( D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST );

		context->IASetIndexBuffer( indices_.buffer_, indices_.format_, 0 );

		D3D11_INPUT_ELEMENT_DESC layout[] =
		{
			{ "POSITION", 0, DXGI_FORMAT_R32G32_FLOAT, 0, 0, D3D11_INPUT_PER_VERTEX_DATA, 0 },
			{ "TEXCOORD", 0, DXGI_FORMAT_R32G32_FLOAT, 0, 8, D3D11_INPUT_PER_VERTEX_DATA, 0 },
			//{ "TRANSFORM", 0, DXGI_FORMAT_R32G32B32A32_FLOAT, 1, 0, D3D11_INPUT_PER_INSTANCE_DATA, 1 },
			//{ "TRANSFORM", 1, DXGI_FORMAT_R32G32B32A32_FLOAT, 1, 16, D3D11_INPUT_PER_INSTANCE_DATA, 1 },
			//{ "TRANSFORM", 2, DXGI_FORMAT_R32G32B32A32_FLOAT, 1, 32, D3D11_INPUT_PER_INSTANCE_DATA, 1 },
			//{ "TRANSFORM", 3, DXGI_FORMAT_R32G32B32A32_FLOAT, 1, 48, D3D11_INPUT_PER_INSTANCE_DATA, 1 },
			//{ "COLOR", 0, DXGI_FORMAT_R32G32B32A32_FLOAT, 1, 64, D3D11_INPUT_PER_INSTANCE_DATA, 1 },
			//{ "SOURCERECT", 0, DXGI_FORMAT_R32G32B32A32_FLOAT, 1, 80, D3D11_INPUT_PER_INSTANCE_DATA, 1 }
		};

		u32 layoutHash = Dx11HashInputElementDescriptions( layout, 2 );

		// Set the input layout
		deviceContext.inputLayoutCache.setInputLayout( context, layoutHash, fxPass.vsInputSignatureHash_
			, layout, 2, reinterpret_cast<const u8*>( fxPass.vsInputSignature_->GetBufferPointer() ), (u32)fxPass.vsInputSignature_->GetBufferSize() );

		textRendererConstants_.data.Color = Vector4( 1, 1, 1, 1 );
		textRendererConstants_.data.ViewportSize = Vector4( (float)canvasWidth, (float)canvasHeight, 1.0f / (float)canvasWidth, 1.0f / (float)canvasHeight );
		float texW = (float) bmFont->getTextureWidth();
		float texH = (float) bmFont->getTextureHeight();
		textRendererConstants_.data.TextureSize = Vector4( texW, texH, 1.0f / texW, 1.0f / texH );

		canvasWidth_ = canvasWidth;
		canvasHeight_ = canvasHeight;
	}

	void TextRenderer::end( Dx11DeviceContext& deviceContext )
	{
		currentFont_ = nullptr;
		ID3D11DeviceContext* context = deviceContext.context;
		context->OMSetBlendState( BlendStates::blendDisabled, nullptr, 0xffffffff );
	}

	void TextRenderer::printf( Dx11DeviceContext& deviceContext, float x, float y, u32 colorABGR, float scale, const char* format, ... )
	{
		va_list	args;
		va_start( args, format );
		const int st_buffer_size = 512;
		char st_buffer[st_buffer_size];

		int nWritten = vsnprintf( st_buffer, st_buffer_size, format, args );
		if ( nWritten < 0 )
		{
			THROW_MESSAGE( "vsnprintf failed!" );
		}
		else if ( nWritten == st_buffer_size )
		{
			// truncated
			//
			st_buffer[st_buffer_size-1] = 0;
		}

		const char* ascii = st_buffer;
		va_end( args );

		if ( ! ascii[0] )
			return;

		_Draw( deviceContext, x, y, colorABGR, scale, ascii );
	}


	void TextRenderer::printfPN( Dx11DeviceContext& deviceContext, float x01, float y01, u32 colorABGR, float fontScale, const char* format, ... )
	{
		va_list	args;
		va_start( args, format );
		const int st_buffer_size = 512;
		char st_buffer[st_buffer_size];

		int nWritten = vsnprintf( st_buffer, st_buffer_size, format, args );
		if ( nWritten < 0 )
		{
			THROW_MESSAGE( "vsnprintf failed!" );
		}
		else if ( nWritten == st_buffer_size )
		{
			// truncated
			//
			st_buffer[st_buffer_size - 1] = 0;
		}

		const char* ascii = st_buffer;
		va_end( args );

		if ( !ascii[0] )
			return;

		float x = textRendererConstants_.data.ViewportSize.getX().getAsFloat() * x01;
		float y = textRendererConstants_.data.ViewportSize.getY().getAsFloat() * y01;

		_Draw( deviceContext, x, y, colorABGR, fontScale, ascii );
	}

//void TextRenderer::puts( GdiCommandQueue* gdiContext, float x, float y, const char* text )
//{
//	_Draw( gdiContext, x, y, colorABGR_, fontSize_, text );
//}
//
//void TextRenderer::puts( GdiCommandQueue* gdiContext, float x, float y, u32 colorABGR, float scale, const char* text )
//{
//	_Draw( gdiContext, x, y, colorABGR, scale, text );
//}


uint32_t getUcs4FromUtf8( const uint8_t* utf8, uint32_t* ucs4, uint32_t alterCode )
{
	uint64_t code = 0L;
	uint32_t len = 0;

	code = (uint64_t)*utf8;

	if ( code ) {
		utf8++;
		len++;
		if ( code >= 0x80 ) {
			//while (1) { // conditional expression is constant warning
			for ( ;; ) {
				if ( code & 0x40 ) { 
					uint64_t mask = 0x20L;
					uint64_t encode;
					uint32_t n;

					for ( n=2;;n++ ) {
						if ( (code & mask) == 0 ) {
							len = n;
							mask--;
							if ( mask == 0 ) { // 0xFE or 0xFF 
								*ucs4 = 0x00000000;
								return 0;
							}
							break;
						}
						mask = (mask >> 1);
					}
					code &= mask;

					for ( n=1; n<len; n++ ) {
						encode = (uint64_t)*utf8;
						if ( (encode & 0xc0) != 0x80 ) {
							if ( ucs4 ) *ucs4 = alterCode;
							return n;
						}
						code = ( ( code << 6 ) | (encode & 0x3f) );
						utf8++;
					}
					break;
				}
				else {
					for( ;; utf8++ ) {
						code = (uint64_t)*utf8;
						if ( code < 0x80 ) break;
						if ( code & 0x40 ) break;
						len++;
					}
					if ( code < 0x80 ) break;
				}
			}
		}
	}
	if ( ucs4 )  *ucs4 = (uint32_t)code;

	return len;
}

void TextRenderer::_Draw( Dx11DeviceContext& deviceContext, float userX, float userY, u32 colorABGR, float fontScale, const char* textUtf8Orig )
{
	u32 nVisibleCharacters = 0;
	const u8* utf8Tmp = reinterpret_cast<const u8*>( textUtf8Orig );
	u32 code = 0;
	do
	{
		u32 offs = getUcs4FromUtf8( utf8Tmp, &code, 0x3000 );
		nVisibleCharacters += (code != ' ') && (code != '\n') && (code != '\t') && (code != 0);
		utf8Tmp += offs;
	} while ( code );

	if ( ! nVisibleCharacters )
		return;

	SPAD_ASSERT( nVisibleCharacters * 6 <= indices_.nIndices_ );

	ID3D11DeviceContext* context = deviceContext.context;
	const BMFont* bmFont = currentFont_;

	_BMFontVertex* ptr = reinterpret_cast<_BMFontVertex*>( vertices_.map( context, nVisibleCharacters * 4 * sizeof( _BMFontVertex ) ) );

	SPAD_STATIC_ASSERT( sizeof( _BMFontVertex ) == 16, "_BMFontVertex must be 16 byte wide" );


	textRendererConstants_.data.Color = abgrToRgba( colorABGR );
	textRendererConstants_.data.Transform = Matrix4::translation( Vector3( userX, userY, 0 ) ) * Matrix4::scale( Vector3( fontScale, fontScale, 1 ) );

	const float textStartX = 0;
	const float textStartY = -bmFont->getBaseLine();

	float x = textStartX, y = textStartY;

	const bool hasKerningInfo = bmFont->hasKerningInfo();

	utf8Tmp = reinterpret_cast<const u8*>( textUtf8Orig );

	u32 nextCode = 0;
	u32 offs = getUcs4FromUtf8( utf8Tmp, &nextCode, 0x3000 );
	utf8Tmp += offs;

	do
	{
		code = nextCode;
		u32 offsNext = getUcs4FromUtf8( utf8Tmp, &nextCode, 0x3000 );
		utf8Tmp += offsNext;

		if ( ! code )
			break;

		if ( code == '\n' )
		{
			x = textStartX;
			y += bmFont->getLineHeight();
			continue;
		}
		else if ( code == ' ' || code == '\t' )
		{
			if ( hasKerningInfo && enableKerning_ && nextCode )
			{
				i16 adjustment;
				BMFont::GlyphDesc g = bmFont->getGlyphWithKerning( ' ', nextCode, &adjustment );
				if ( code == ' ' )
					x += g.xAdvance + adjustment;
				else
					x += g.xAdvance * 4 + adjustment;
			}
			else
			{
				BMFont::GlyphDesc g = bmFont->getGlyph( ' ' );
				if ( code == ' ' )
					x += g.xAdvance;
				else
					x += g.xAdvance * 4;
			}
			continue;
		}
		else
		{
#ifdef _DEBUG
			char c = 0;
			if ( code < 128 )
				c = (char)code;
#endif // _DEBUG

			if ( hasKerningInfo && enableKerning_ && nextCode )
			{
				i16 adjustment;
				BMFont::GlyphDesc g = bmFont->getGlyphWithKerning( code, nextCode, &adjustment );

				_AppendGlyph( ptr, x, y, g );

				int shift = g.xAdvance + adjustment;
				x += shift;
			}
			else
			{
				BMFont::GlyphDesc g = bmFont->getGlyph( code );

				_AppendGlyph( ptr, x, y, g );

				int shift = g.xAdvance;
				x += shift;
			}

			ptr += 4;
		}


	} while ( code );

	vertices_.unmap( context );

	textRendererConstants_.updateGpu( context );
	textRendererConstants_.setVS( context, 0 );
	textRendererConstants_.setPS( context, 0 );

	const u32 stride = sizeof( _BMFontVertex );
	const u32 verticesOffset = vertices_.getLastMapOffset();
	context->IASetVertexBuffers( 0, 1, vertices_.getBuffers(), &stride, &verticesOffset );
	context->DrawIndexed( nVisibleCharacters * 6, 0, 0 );
}

} // namespace spad
