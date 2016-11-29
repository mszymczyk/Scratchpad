using System;
using SettingsEditor;

// The field 'xxx' is assigned but its value is never used
#pragma warning disable 414
    
namespace FrameworkSettings
{
    public class GeneratorConfig
    {
        public static readonly string SettingsEditorHeaderInclude = "#include <Tools/SettingsEditor/ClientLib/SettingsEditor.h>";
        public static readonly string SettingsEditorCppInclude = "#include <FrameworkSettings_pch.h>";
    }

    class General
    {
        enum e_Vsync
        {
            eVSync_disabled = 0,
            eVSync_60Hz,
            eVSync_30Hz,
            eVSync_15Hz
        }

        [DisplayName( "VSync" )]
        e_Vsync vsync = e_Vsync.eVSync_60Hz;
    }
}
