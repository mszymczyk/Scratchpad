using System;
using SettingsEditor;

// The field 'xxx' is assigned but its value is never used
#pragma warning disable 414

namespace SampleSettings
{
    public class GeneratorConfig
    {
        public static readonly string SettingsEditorHeaderInclude = "#include <Tools/SettingsEditor/ClientLib/SettingsEditor.h>";
        public static readonly string SettingsEditorCppInclude = "#include <SampleSettings_pch.h>";
    }

    class Group
    {
        enum e_Vsync
        {
            eVSync_disabled = 0,
            eVSync_60Hz,
            eVSync_30Hz,
            eVSync_15Hz
        }

        enum e_SampleEnum
        {
            e_first,
            e_second,
            e_third,
        }

        [DisplayName( "VSync" )]
        e_Vsync vsync = e_Vsync.eVSync_60Hz;

        [DisplayName( "Sample Float" )]
        [Min( -2.0f )]
        [Max( 10.0f )]
        [HelpText( "Value between -2 and 10" )]
        float sampleFloat = 3.0f;

        [DisplayName( "Check Float" )]
        [Min( -1.0f )]
        [Max( 1.0f )]
        [HelpText( "Value between -1 and 1" )]
        [CheckBox(true)]
        float checkFloat = 3.0f;

        Color color = new Color( 1, 0, 0 );

        string sampleString = "string";

        [DisplayName( "SampleDir" )]
        Direction sampleDir = new Direction( 0, 0, 1 );

        AnimCurve animCurve = new AnimCurve();

        [HasPresets]
        class Inner
        {
            bool boolParam = true;
            int intParam = 1;
            e_SampleEnum enumParam = e_SampleEnum.e_first;
            float floatParam = 1;
            Float4 float4Param = new Float4( 1, 2, 3, 4 );
            Color color = new Color( 1, 0, 0 );
            AnimCurve animCurve = new AnimCurve();
            string stringParam = "config.txt";
        }

        class SampleGroup
        {

        }
    }
}
