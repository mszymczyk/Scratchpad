#pragma once

#include <picoCore/SettingsEditor.h>

namespace PicoSettingsNamespace
{

struct Renderer;
struct HMD;
struct Stats;
struct PS4Gnm;
struct TextUtil;
struct Recording;

// PicoSettings
//////////////////////////////////////////////////////////////////////////////////
struct PicoSettings
{
	SETTINGS_EDITOR_STRUCT_DESC // this macro is required for SettingsEditor to work


	const Renderer* mRenderer;
	const HMD* mHMD;
	const Stats* mStats;
	const PS4Gnm* mPS4Gnm;
	const TextUtil* mTextUtil;
	const Recording* mRecording;
};

class PicoSettingsWrap : public PicoSettings
{
public:
	PicoSettingsWrap( const char* filePath)
	{
		load( filePath );
	}

	~PicoSettingsWrap()
	{
		unload();
	}
	
private:
	void load( const char* filePath );
	void unload();

		SettingsEditor::SettingsFile* __settingsFile_ = nullptr;
}; // class PicoSettingsWrap


} // namespace PicoSettingsNamespace

// Declared this variable in global scope to simplify usage and debugging
// Visual Studio's debugger doesn't see global variables declared within namespace :(
// Name clashes shouldn't be a big problem...
//
extern PicoSettingsNamespace::PicoSettingsWrap* gPicoSettings;
