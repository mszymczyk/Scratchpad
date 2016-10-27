#pragma once

#include "pugixml/pugixml.hpp"

namespace spad
{


// returns null if attribute can't be found or value is empty string
const char* attribute_get_string( const pugi::xml_node& node, const char* attributeName );

bool attribute_get_float_array( const pugi::xml_attribute& attr, float* dst, size_t nDst );


} // namespace spad
