#include "pugixmlHelpers.h"
#include <stdlib.h>
#include <string.h>

#if defined(_MSC_VER) && defined(_DEBUG)
#define new _DEBUG_NEW
#endif

namespace SettingsEditor
{


const char* attribute_get_string( const pugi::xml_node& node, const char* attributeName )
{
	pugi::xml_attribute attr = node.attribute( attributeName );
	if ( attr.empty() )
		return nullptr;

	const char* val = attr.value();
	if ( val[0] )
		return val;

	return nullptr;
}

bool attribute_get_float_array( const pugi::xml_attribute& attr, float* dst, size_t nDst )
{
	const char* s = attr.value();
	const size_t sLen = strlen( s );
	const char* sEnd = s + sLen;

	size_t nRead = 0;

	while ( s < sEnd )
	{
		char* e = nullptr;
		float f = strtof( s, &e );
		s = e + 1; // skip space
		dst[nRead] = f;
		++ nRead;
	}

	return nRead == nDst;
}


} // namespace SettingsEditor
