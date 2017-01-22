#include <SampleSettings_pch.h>
#include "SampleSettings.h"

#include "SampleSettings_Group.h"

using namespace SettingsEditor;

namespace SampleSettingsNamespace
{


	namespace SampleSettings_Group_Inner_NS
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
			d.offset_ = offsetof( Group, mInner );
			d.sizeInBytes_ = sizeof(Group::Inner);
			Inner_Fields[0] = FieldDescription( "boolFirst", offsetof(Group::Inner, boolFirst), eParamType_bool );
			Inner_Fields[1] = FieldDescription( "floatFirst", offsetof(Group::Inner, floatFirst), eParamType_float );
			Inner_Fields[2] = FieldDescription( "boolSecond", offsetof(Group::Inner, boolSecond), eParamType_bool );
			Inner_Fields[3] = FieldDescription( "floatSecond", offsetof(Group::Inner, floatSecond), eParamType_float );
			return d;
		}
	} // namespace SampleSettings_Group_Inner_NS


	namespace SampleSettings_Group_NS
	{
		const unsigned int Group_Field_Count = 0;
		const unsigned int Group_NestedStructures_Count = 1;
		const StructDescription* Group_NestedStructures[Group_NestedStructures_Count];

		StructDescription Group_CreateDescription( const StructDescription* parent )
		{
			StructDescription d;
			d.name_ = "Group";
			d.parentStructure_ = parent; 
			d.fields_ = nullptr;
			d.nestedStructures_ = Group_NestedStructures;
			d.nFields_ = Group_Field_Count;
			d.nNestedStructures_ = Group_NestedStructures_Count;
			d.offset_ = offsetof( SampleSettings, mGroup );
			d.sizeInBytes_ = sizeof(Group);
			Group_NestedStructures[0] = Group::Inner::GetDesc();
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

