#include "SettingsEditorUtil.h"

#if _MSC_VER

#define VC_EXTRALEAN
#define WIN32_LEAN_AND_MEAN
#include <Windows.h>

#endif

#if defined(_MSC_VER) && defined(_DEBUG)
#define new _DEBUG_NEW
#endif

namespace SettingsEditor
{
namespace _internal
{
#if SETTINGS_EDITOR_ASSERT_ENABLED && _MSC_VER

void assertPrintAndBreak( const char* text )
{
	OutputDebugString( text );
	__debugbreak();
}

#endif //

} // namespace _internal
} // namespace SettingsEditor
