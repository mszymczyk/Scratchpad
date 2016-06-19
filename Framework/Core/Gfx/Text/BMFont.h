#pragma once

namespace spad
{

class BMFont
{
public:
	BMFont();

	~BMFont()
	{
		DeInitialize();
	}

	void Initialize( ID3D11Device* dxDevice, const char* fontFile );
	void DeInitialize();

	struct GlyphDesc
	{
		u16 x, y;
		u16 width, height;
		i16 xOffset, yOffset;
		i16 xAdvance;
		unsigned short page;

		GlyphDesc() : x( 0 ), y( 0 ), width( 0 ), height( 0 ), xOffset( 0 ), yOffset( 0 ),
			xAdvance( 0 ), page( 0 )
		{ }
	}; // struct GlyphDesc

	u32 getFontSize() const { return fontSize_; }
	u32 getLineHeight() const { return lineHeight_; }
	float getBaseLine() const { return baseLine_; }
	GlyphDesc getGlyph( const u32 code ) const;
	bool hasKerningInfo() const { return hasKerningInfo_; }
	GlyphDesc getGlyphWithKerning( const u32 code, const u32 nextCharCode, i16* dstAdjustment ) const;
	ID3D11ShaderResourceView* getTexture() const { return fontTex_; }
	ID3D11ShaderResourceView* const * getTextures() const { return &fontTex_; }

	u32 getTextureWidth() const { return texWidth_; }
	u32 getTextureHeight() const { return texHeight_; }

protected:
	enum { kNGlyphs = 95 };

	std::string fontFilename_;
	u32 refCounter_;
	u16 lineHeight_;
	u16 fontSize_;
	u16 baseLine_;
	u16 texWidth_;
	u16 texHeight_;
	bool hasKerningInfo_;

	struct _KernDesc
	{
		u32 first;
		u32 second;
		i16 amount;
	};

	struct _GlyphInfo
	{
		_GlyphInfo()
			: kernings( NULL )
			, nKernings( 0 )
		{}

		GlyphDesc g;
		_KernDesc* kernings; // this is just weak pointer to BMFont::kernings_
		u32 nKernings;
	};

	typedef std::map<u32, _GlyphInfo> GlyphMap;

	GlyphMap glyphs_;
	std::vector<_KernDesc> kernings_;
	ID3D11ShaderResourceView* fontTex_;
}; // BMFont

} // namespace fr
