#include "Gfx_pch.h"
#include "BMFont.h"
#include <Util\FileIO.h>
#include <tinyxml2\tinyxml2.h>

#if defined(_MSC_VER) && defined(_DEBUG)
#define new _DEBUG_NEW
#endif

using namespace tinyxml2;

namespace spad
{

BMFont::BMFont()
	: refCounter_( 1 )
	, lineHeight_( 0 )
	, fontSize_( 0 )
	, baseLine_( 0 )
	, texWidth_( 0 )
	, texHeight_( 0 )
	, hasKerningInfo_( false )
	, kernings_( NULL )
	, fontTex_( NULL )
{
}

BMFont::GlyphDesc BMFont::getGlyph( const u32 code ) const
{
	GlyphMap::const_iterator it = glyphs_.find( code );
	if ( it != glyphs_.end() )
		return it->second.g;

	return GlyphDesc();
}

BMFont::GlyphDesc BMFont::getGlyphWithKerning( const u32 code, const u32 nextCharCode, i16* dstAdjustment ) const
{
	GlyphMap::const_iterator it = glyphs_.find( code );
	if ( it != glyphs_.end() )
	{
		const _GlyphInfo& gi = it->second;
		for ( u32 i = 0; i < gi.nKernings; ++i )
		{
			const _KernDesc& kd = kernings_[i];
			if ( kd.second == nextCharCode )
			{
				*dstAdjustment = kd.amount;
				return gi.g;
			}
		}

		*dstAdjustment = 0;
		return gi.g;
	}

	*dstAdjustment = 0;
	return GlyphDesc();
}

static u32 _ReadGlyph( XMLElement* charElem, BMFont::GlyphDesc& glyph )
{
	int ival, ires;
	ires = charElem->QueryIntAttribute( "x", &ival );
	if ( ires != XML_SUCCESS )
		return ires;
	glyph.x = (u16)ival;

	ires = charElem->QueryIntAttribute( "y", &ival );
	if ( ires != XML_SUCCESS )
		return ires;
	glyph.y = (u16)ival;

	ires = charElem->QueryIntAttribute( "width", &ival );
	if ( ires != XML_SUCCESS )
		return ires;
	glyph.width = (u16)ival;

	ires = charElem->QueryIntAttribute( "height", &ival );
	if ( ires != XML_SUCCESS )
		return ires;
	glyph.height = (u16)ival;

	ires = charElem->QueryIntAttribute( "xoffset", &ival );
	if ( ires != XML_SUCCESS )
		return ires;
	glyph.xOffset = (i16)ival;

	ires = charElem->QueryIntAttribute( "yoffset", &ival );
	if ( ires != XML_SUCCESS )
		return ires;
	glyph.yOffset = (i16)ival;

	ires = charElem->QueryIntAttribute( "xadvance", &ival );
	if ( ires != XML_SUCCESS )
		return ires;
	glyph.xAdvance = (i16)ival;

	ires = charElem->QueryIntAttribute( "page", &ival );
	if ( ires != XML_SUCCESS )
		return ires;
	glyph.page = (u16)ival;

	return 0;
}

void BMFont::Initialize( ID3D11Device* dxDevice, const char* fontFile )
{
	std::string fontFileContents = ReadTextFileAsString( fontFile );

	if ( fontFileContents.empty() )
	{
		THROW_MESSAGE( "Couldn't load file '%s'!", fontFile );
	}

	tinyxml2::XMLDocument doc;

	XMLError err = doc.Parse( fontFileContents.c_str(), fontFileContents.size() );

	if ( err != XML_SUCCESS )
	{
		THROW_MESSAGE( "Couldn't load xml from file '%s'!", fontFile );
	}

	XMLElement* root = doc.FirstChildElement( "font" );
	if( ! root )
	{
		THROW_MESSAGE( "Couldn't find 'font' element in file '%s'!", fontFile );
	}

	XMLElement* info = root->FirstChildElement( "info" );
	if( ! info )
	{
		THROW_MESSAGE( "Couldn't find 'info' element in file '%s'!", fontFile );
	}

	int ival, ires;
	ires = info->QueryIntAttribute( "size", &ival );
	if ( ires != XML_SUCCESS )
	{
		THROW_MESSAGE( "Couldn't find 'size' attribute! (%s)", fontFile );
	}
	fontSize_ = (u16)ival;

	XMLElement* common = root->FirstChildElement( "common" );
	if( ! common )
	{
		THROW_MESSAGE( "Couldn't find 'common' element in file '%s'!", fontFile );
	}

	ires = common->QueryIntAttribute( "lineHeight", &ival );
	if ( ires != XML_SUCCESS )
	{
		THROW_MESSAGE( "Couldn't find 'lineHeight' attribute! (%s)", fontFile );
	}

	lineHeight_ = (u16)ival;

	ires = common->QueryIntAttribute( "base", &ival );
	if ( ires != XML_SUCCESS )
	{
		THROW_MESSAGE( "Couldn't find 'base' attribute! (%s)", fontFile );
	}

	baseLine_ = (u16)ival;

	ires = common->QueryIntAttribute( "scaleW", &ival );
	if ( ires != XML_SUCCESS )
	{
		THROW_MESSAGE( "Couldn't find 'scaleW' attribute! (%s)", fontFile );
	}
	texWidth_ = (u16)ival;

	ires = common->QueryIntAttribute( "scaleH", &ival );
	if ( ires != XML_SUCCESS )
	{
		THROW_MESSAGE( "Couldn't find 'scaleH' attribute! (%s)", fontFile );
	}
	texHeight_ = (u16)ival;

	ires = common->QueryIntAttribute( "pages", &ival );
	if ( ires != XML_SUCCESS )
	{
		THROW_MESSAGE( "Couldn't find 'base' attribute! (%s)", fontFile );
	}

	if ( ival != 1 )
	{
		THROW_MESSAGE( "Unsupported number of pages! (%s)", fontFile );
	}

	XMLElement* pages = root->FirstChildElement( "pages" );
	if( ! pages )
	{
		THROW_MESSAGE( "Couldn't find 'pages' element in file '%s'!", fontFile );
	}

	std::string fontFileDir = GetDirectoryFromFilePath( fontFile );

	u32 nPages = 0;
	XMLElement* page = NULL;
	for ( page = pages->FirstChildElement("page"); page; page = page->NextSiblingElement("page") )
	{
		if ( nPages >= 1 )
		{
			THROW_MESSAGE( "Only single page fonts are supported! '%s'!", fontFile );
		}

		++ nPages;

		SPAD_ASSERT( ! fontTex_ );
		const char* filename = page->Attribute( "file" );
		std::string fullFilename = fontFileDir;
		fullFilename.append( filename );
		fullFilename = GetFilePathWithoutExtension( fullFilename ) + ".dds";

		fontTex_ = LoadTexturePtr( dxDevice, fullFilename.c_str() );
	}

	XMLElement* chars = root->FirstChildElement( "chars" );
	if( ! chars )
	{
		THROW_MESSAGE( "Couldn't find 'chars' element in file '%s'!", fontFile );
	}

	XMLElement* character = NULL;
	for ( character = chars->FirstChildElement("char"); character; character = character->NextSiblingElement("char") )
	{
		ires = character->QueryIntAttribute( "id", &ival );
		if ( ires != XML_SUCCESS )
		{
			THROW_MESSAGE( "Couldn't find 'char.id' attribute! (%s)", fontFile );
		}

		GlyphDesc g;
		u32 ures = _ReadGlyph( character, g );
		if ( ures )
		{
			THROW_MESSAGE( "Couldn't read glyph '%d'! (%s)", ival, fontFile );
		}
		_GlyphInfo gi;
		gi.g = g;
		glyphs_[ival] = gi;
	}


	XMLElement* kernings = root->FirstChildElement( "kernings" );
	if( kernings )
	{
		int nKernings = 0;
		ires = kernings->QueryIntAttribute( "count", &nKernings );
		if ( ires != XML_SUCCESS )
		{
			THROW_MESSAGE( "Couldn't find 'kernings.count' attribute! Kerning will be disabled (%s)", fontFile );
		}

		kernings_.reserve( nKernings );

		XMLElement* kern = NULL;
		for ( kern = kernings->FirstChildElement("kerning"); kern; kern = kern->NextSiblingElement("kerning") )
		{
			int first = 0, second = 0;
			int amount;

			ires = kern->QueryIntAttribute( "first", &first );
			if ( ires != XML_SUCCESS )
			{
				THROW_MESSAGE( "Couldn't find 'kerning.first' attribute! Kerning will be disabled (%s)(%s)", fontFile );
			}

			ires = kern->QueryIntAttribute( "second", &second );
			if ( ires != XML_SUCCESS )
			{
				THROW_MESSAGE( "Couldn't find 'kerning.second' attribute! Kerning will be disabled (%s)(%s)", fontFile );
			}

			ires = kern->QueryIntAttribute( "amount", &amount );
			if ( ires != XML_SUCCESS )
			{
				THROW_MESSAGE( "Couldn't find 'kerning.amount' attribute! Kerning will be disabled (%s)(%s)", fontFile );
			}

			_KernDesc kd;
			kd.first = (u32)first;
			kd.second = (u32)second;
			kd.amount = (i16)amount;
			kernings_.push_back( kd );
		}

		hasKerningInfo_ = true;
		nKernings = (u32)nKernings;

		for ( GlyphMap::iterator it = glyphs_.begin(); it != glyphs_.end(); ++it )
		{
			u32 gcode = it->first;
			_GlyphInfo& gi = it->second;

			for ( u32 i = 0; i < (u32)nKernings; ++i )
			{
				if ( kernings_[i].first == gcode && !gi.kernings )
				{
					u32 gcodeKernings = 1;
					for ( u32 k = i+1; k < (u32)nKernings; ++k, ++gcodeKernings )
					{
						if ( kernings_[k].first != gcode )
							break;
					}

					gi.kernings = &kernings_[i];
					gi.nKernings = gcodeKernings;
				}
			}

		}
	}
}

void BMFont::DeInitialize()
{
	DX_SAFE_RELEASE( fontTex_ );
}


} // namespace fr