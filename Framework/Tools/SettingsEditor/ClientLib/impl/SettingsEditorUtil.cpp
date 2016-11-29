#pragma once

#include "SettingsEditorUtil.h"
#define VC_EXTRALEAN
#define WIN32_LEAN_AND_MEAN
#include <Windows.h>

#if defined(_MSC_VER) && defined(_DEBUG)
#define new _DEBUG_NEW
#endif

namespace SettingsEditor
{
namespace _internal
{
#if SETTINGS_EDITOR_ASSERT_ENABLED

void assertPrintAndBreak( const char* text )
{
	OutputDebugString( text );
	__debugbreak();
}

#endif //

} // namespace _internal
} // namespace SettingsEditor
