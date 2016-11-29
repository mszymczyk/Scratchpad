#pragma once

#include <Tools/SettingsEditor/ClientLib/SettingsEditor.h>

namespace FrameworkSettingsNamespace
{

struct General;

// FrameworkSettings
//////////////////////////////////////////////////////////////////////////////////
struct FrameworkSettings
{
	SETTINGS_EDITOR_STRUCT_DESC // this macro is required for SettingsEditor to work


	const General* mGeneral;
};

class FrameworkSettingsWrap : public FrameworkSettings
{
public:
	FrameworkSettingsWrap( const char* filePath)
	{
		load( filePath );
	}

	~FrameworkSettingsWrap()
	{
		unload();
	}
	
private:
	void load( const char* filePath );
	void unload();

		SettingsEditor::SettingsFile settingsFile_;
}; // class FrameworkSettingsWrap


} // namespace FrameworkSettingsNamespace

// Declared this variable in global scope to simplify usage and debugging
// Visual Studio's debugger doesn't see global variables declared within namespace :(
// Name clashes shouldn't be a big problem...
//
extern FrameworkSettingsNamespace::FrameworkSettingsWrap* gFrameworkSettings;
