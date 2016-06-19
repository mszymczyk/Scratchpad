using System;
using SettingsEditor;

// The field 'xxx' is assigned but its value is never used
#pragma warning disable 414

namespace FrameworkSettings
{
    public class GeneratorConfig
    {
        public static readonly string SettingsEditorHeaderInclude = "#include <Util/SettingsEditor/SettingsEditor.h>";
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

        //int fakeInt = 0;

        [HasPresets]
        class Inner
        {
            float floatParam = 1;
            bool boolParam = true;
            int intParam = 1;
            e_Vsync vsync2 = e_Vsync.eVSync_15Hz;
        }

        class SampleGroup
        {

        }
    }
}
