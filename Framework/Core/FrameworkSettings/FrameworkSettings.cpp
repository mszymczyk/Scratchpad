#include <FrameworkSettings_pch.h>
#include "FrameworkSettings.h"

#include "FrameworkSettings_General.h"

using namespace SettingsEditor;

namespace FrameworkSettingsNamespace
{


	namespace FrameworkSettings_General_Inner_NS
	{
		const unsigned int Inner_Field_Count = 4;
		const unsigned int Inner_NestedStructures_Count = 0;
		FieldDescription Inner_Fields[Inner_Field_Count];

		StructDescription Inner_CreateDescription( const StructDescription* parent )
		{
			StructDescription d;
			d.name_ = "Inner";
			d.parentStructure_ = parent; 
			d.fields_ = Inner_Fields;
			d.nestedStructures_ = nullptr;
			d.nFields_ = Inner_Field_Count;
			d.nNestedStructures_ = Inner_NestedStructures_Count;
			d.offset_ = offsetof( General, mInner );
			d.sizeInBytes_ = sizeof(General::Inner);
			Inner_Fields[0] = FieldDescription( "floatParam", offsetof(General::Inner, floatParam), eParamType_float );
			Inner_Fields[1] = FieldDescription( "boolParam", offsetof(General::Inner, boolParam), eParamType_bool );
			Inner_Fields[2] = FieldDescription( "intParam", offsetof(General::Inner, intParam), eParamType_int );
			Inner_Fields[3] = FieldDescription( "vsync2", offsetof(General::Inner, vsync2), eParamType_enum );
			return d;
		}
	} // namespace FrameworkSettings_General_Inner_NS


	namespace FrameworkSettings_General_SampleGroup_NS
	{
		const unsigned int SampleGroup_Field_Count = 0;
		const unsigned int SampleGroup_NestedStructures_Count = 0;

		StructDescription SampleGroup_CreateDescription( const StructDescription* parent )
		{
			StructDescription d;
			d.name_ = "SampleGroup";
			d.parentStructure_ = parent; 
			d.fields_ = nullptr;
			d.nestedStructures_ = nullptr;
			d.nFields_ = SampleGroup_Field_Count;
			d.nNestedStructures_ = SampleGroup_NestedStructures_Count;
			d.offset_ = offsetof( General, mSampleGroup );
			d.sizeInBytes_ = sizeof(General::SampleGroup);
			return d;
		}
	} // namespace FrameworkSettings_General_SampleGroup_NS


	namespace FrameworkSettings_General_NS
	{
		const unsigned int General_Field_Count = 1;
		const unsigned int General_NestedStructures_Count = 2;
		FieldDescription General_Fields[General_Field_Count];
		const StructDescription* General_NestedStructures[General_NestedStructures_Count];

		StructDescription General_CreateDescription( const StructDescription* parent )
		{
			StructDescription d;
			d.name_ = "General";
			d.parentStructure_ = parent; 
			d.fields_ = General_Fields;
			d.nestedStructures_ = General_NestedStructures;
			d.nFields_ = General_Field_Count;
			d.nNestedStructures_ = General_NestedStructures_Count;
			d.offset_ = offsetof( FrameworkSettings, mGeneral );
			d.sizeInBytes_ = sizeof(General);
			General_Fields[0] = FieldDescription( "vsync", offsetof(General, vsync), eParamType_enum );
			General_NestedStructures[0] = General::Inner::GetDesc();
			General_NestedStructures[1] = General::SampleGroup::GetDesc();
			return d;
		}
	} // namespace FrameworkSettings_General_NS


	namespace FrameworkSettings_NS
	{
		const unsigned int FrameworkSettings_Field_Count = 0;
		const unsigned int FrameworkSettings_NestedStructures_Count = 1;
		const StructDescription* FrameworkSettings_NestedStructures[FrameworkSettings_NestedStructures_Count];

		StructDescription FrameworkSettings_CreateDescription( const StructDescription* parent )
		{
			StructDescription d;
			d.name_ = "FrameworkSettings";
			d.parentStructure_ = parent; 
			d.fields_ = nullptr;
			d.nestedStructures_ = FrameworkSettings_NestedStructures;
			d.nFields_ = FrameworkSettings_Field_Count;
			d.nNestedStructures_ = FrameworkSettings_NestedStructures_Count;
			d.offset_ = 0;
			d.sizeInBytes_ = sizeof(FrameworkSettings);
			FrameworkSettings_NestedStructures[0] = General::GetDesc();
			return d;
		}
	} // namespace FrameworkSettings_NS


	StructDescription FrameworkSettings::__desc = FrameworkSettings_NS::FrameworkSettings_CreateDescription( nullptr );
	StructDescription General::__desc = FrameworkSettings_General_NS::General_CreateDescription( FrameworkSettings::GetDesc() );
	StructDescription General::Inner::__desc = FrameworkSettings_General_Inner_NS::Inner_CreateDescription( General::GetDesc() );
	StructDescription General::SampleGroup::__desc = FrameworkSettings_General_SampleGroup_NS::SampleGroup_CreateDescription( General::GetDesc() );

	void FrameworkSettingsWrap::load( const char* filePath )
	{
		if ( __settingsFile_ )
			return;
		
		mGeneral = new General;

		const void* addresses[1];
		addresses[0] = mGeneral;

		__settingsFile_ = SettingsEditor::createSettingsFile( filePath, GetDesc(), addresses );
	}

	void FrameworkSettingsWrap::unload()
	{
		SettingsEditor::releaseSettingsFile( __settingsFile_ );

		delete mGeneral; mGeneral = nullptr;
	}
	

} // namespace FrameworkSettingsNamespace

FrameworkSettingsNamespace::FrameworkSettingsWrap* gFrameworkSettings = nullptr;

