#pragma once

#include <Tools/SettingsEditor/ClientLib/SettingsEditor.h>

namespace SampleSettingsNamespace
{

struct Group;

// SampleSettings
//////////////////////////////////////////////////////////////////////////////////
struct SampleSettings
{
	SETTINGS_EDITOR_STRUCT_DESC // this macro is required for SettingsEditor to work


	const Group* mGroup;
};

class SampleSettingsWrap : public SampleSettings
{
public:
	SampleSettingsWrap( const char* filePath)
	{
		load( filePath );
	}

	~SampleSettingsWrap()
	{
		unload();
	}
	
private:
	void load( const char* filePath );
	void unload();

		SettingsEditor::SettingsFile settingsFile_;
}; // class SampleSettingsWrap


} // namespace SampleSettingsNamespace

// Declared this variable in global scope to simplify usage and debugging
// Visual Studio's debugger doesn't see global variables declared within namespace :(
// Name clashes shouldn't be a big problem...
//
extern SampleSettingsNamespace::SampleSettingsWrap* gSampleSettings;
