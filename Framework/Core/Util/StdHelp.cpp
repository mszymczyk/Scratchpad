#include "Util_pch.h"
#include "StdHelp.h"
//#include <string>
//#include <iostream>
//#include <clocale>
#include <locale>
//#include <vector>
#include <codecvt>

namespace spad
{
	std::string wstringToString( const std::wstring& wstring )
	{
		std::wstring_convert< std::codecvt_utf8<wchar_t>, wchar_t > converter;
		std::string converted_str = converter.to_bytes( wstring );
		return converted_str;
	}

	std::wstring stringToWstring( const std::string& str )
	{
		std::wstring_convert< std::codecvt_utf8<wchar_t>, wchar_t > converter;
		std::wstring converted_str = converter.from_bytes( str );
		return converted_str;
	}

	std::wstring stringToWstring( const char* str )
	{
		std::wstring_convert< std::codecvt_utf8<wchar_t>, wchar_t > converter;
		std::wstring converted_str = converter.from_bytes( str );
		return converted_str;
	}

}

