#include <SampleSettings_pch.h>
#include "SampleSettings.h"

#include "SampleSettings_Group.h"

using namespace SettingsEditor;

namespace SampleSettingsNamespace
{


	namespace SampleSettings_Group_Inner_NS
	{
		const unsigned int Inner_Field_Count = 8;
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
			d.offset_ = offsetof( Group, mInner );
			d.sizeInBytes_ = sizeof(Group::Inner);
			Inner_Fields[0] = FieldDescription( "boolParam", offsetof(Group::Inner, boolParam), eParamType_bool );
			Inner_Fields[1] = FieldDescription( "intParam", offsetof(Group::Inner, intParam), eParamType_int );
			Inner_Fields[2] = FieldDescription( "enumParam", offsetof(Group::Inner, enumParam), eParamType_enum );
			Inner_Fields[3] = FieldDescription( "floatParam", offsetof(Group::Inner, floatParam), eParamType_float );
			Inner_Fields[4] = FieldDescription( "float4Param", offsetof(Group::Inner, float4Param), eParamType_float4 );
			Inner_Fields[5] = FieldDescription( "color", offsetof(Group::Inner, color), eParamType_color );
			Inner_Fields[6] = FieldDescription( "animCurve", offsetof(Group::Inner, animCurve), eParamType_animCurve );
			Inner_Fields[7] = FieldDescription( "stringParam", offsetof(Group::Inner, stringParam), eParamType_string );
			return d;
		}
	} // namespace SampleSettings_Group_Inner_NS


	namespace SampleSettings_Group_SampleGroup_NS
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
			d.offset_ = offsetof( Group, mSampleGroup );
			d.sizeInBytes_ = sizeof(Group::SampleGroup);
			return d;
		}
	} // namespace SampleSettings_Group_SampleGroup_NS


	namespace SampleSettings_Group_NS
	{
		const unsigned int Group_Field_Count = 7;
		const unsigned int Group_NestedStructures_Count = 2;
		FieldDescription Group_Fields[Group_Field_Count];
		const StructDescription* Group_NestedStructures[Group_NestedStructures_Count];

		StructDescription Group_CreateDescription( const StructDescription* parent )
		{
			StructDescription d;
			d.name_ = "Group";
			d.parentStructure_ = parent; 
			d.fields_ = Group_Fields;
			d.nestedStructures_ = Group_NestedStructures;
			d.nFields_ = Group_Field_Count;
			d.nNestedStructures_ = Group_NestedStructures_Count;
			d.offset_ = offsetof( SampleSettings, mGroup );
			d.sizeInBytes_ = sizeof(Group);
			Group_Fields[0] = FieldDescription( "vsync", offsetof(Group, vsync), eParamType_enum );
			Group_Fields[1] = FieldDescription( "sampleFloat", offsetof(Group, sampleFloat), eParamType_float );
			Group_Fields[2] = FieldDescription( "checkFloat", offsetof(Group, checkFloat), eParamType_floatBool );
			Group_Fields[3] = FieldDescription( "color", offsetof(Group, color), eParamType_color );
			Group_Fields[4] = FieldDescription( "sampleString", offsetof(Group, sampleString), eParamType_string );
			Group_Fields[5] = FieldDescription( "sampleDir", offsetof(Group, sampleDir), eParamType_direction );
			Group_Fields[6] = FieldDescription( "animCurve", offsetof(Group, animCurve), eParamType_animCurve );
			Group_NestedStructures[0] = Group::Inner::GetDesc();
			Group_NestedStructures[1] = Group::SampleGroup::GetDesc();
			return d;
		}
	} // namespace SampleSettings_Group_NS


	namespace SampleSettings_NS
	{
		const unsigned int SampleSettings_Field_Count = 0;
		const unsigned int SampleSettings_NestedStructures_Count = 1;
		const StructDescription* SampleSettings_NestedStructures[SampleSettings_NestedStructures_Count];

		StructDescription SampleSettings_CreateDescription( const StructDescription* parent )
		{
			StructDescription d;
			d.name_ = "SampleSettings";
			d.parentStructure_ = parent; 
			d.fields_ = nullptr;
			d.nestedStructures_ = SampleSettings_NestedStructures;
			d.nFields_ = SampleSettings_Field_Count;
			d.nNestedStructures_ = SampleSettings_NestedStructures_Count;
			d.offset_ = 0;
			d.sizeInBytes_ = sizeof(SampleSettings);
			SampleSettings_NestedStructures[0] = Group::GetDesc();
			return d;
		}
	} // namespace SampleSettings_NS


	StructDescription SampleSettings::__desc = SampleSettings_NS::SampleSettings_CreateDescription( nullptr );
	StructDescription Group::__desc = SampleSettings_Group_NS::Group_CreateDescription( SampleSettings::GetDesc() );
	StructDescription Group::Inner::__desc = SampleSettings_Group_Inner_NS::Inner_CreateDescription( Group::GetDesc() );
	StructDescription Group::SampleGroup::__desc = SampleSettings_Group_SampleGroup_NS::SampleGroup_CreateDescription( Group::GetDesc() );

	void SampleSettingsWrap::load( const char* filePath )
	{
		if ( settingsFile_.isValid() )
			return;
		
		mGroup = new ( SettingsEditor::_internal::allocGroup(sizeof(Group), alignof(Group)) ) Group();

		const void* addresses[1];
		addresses[0] = mGroup;

		settingsFile_ = SettingsEditor::createSettingsFile( filePath, GetDesc(), addresses );
	}

	void SampleSettingsWrap::unload()
	{
		SettingsEditor::releaseSettingsFile( settingsFile_ );

		if ( mGroup ) { mGroup->~Group(); SettingsEditor::_internal::freeGroup( const_cast<Group*>( mGroup ) ); mGroup = nullptr; }
	}
	

} // namespace SampleSettingsNamespace

SampleSettingsNamespace::SampleSettingsWrap* gSampleSettings = nullptr;

