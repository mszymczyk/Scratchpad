using System;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Sce.Atf;

namespace SettingsEditor
{
    public enum SettingType
    {
		Bool,
        Int,
        Enum,
		Float,
        FloatBool,
        String,
		//Direction,
		//Orientation,
		Color,
		Float4
    }

    public struct EnumValue
    {
        public string Name;
        public string Label;

        public EnumValue(string name, string label)
        {
            this.Name = name;
            this.Label = label;
        }
    }


    /// <summary>
    /// Maps to dynamically created AttributeInfo
    /// </summary>
    public abstract class Setting
    {
        public SettingType Type;
        public SettingGroup Group;
        public string Name;
        public string DisplayName;
        public string Category = "General";
        public String HelpText = "";
        public List<Tuple<string, bool>> DependsOn { get { return m_dependsOn; } }

        public Setting(FieldInfo field, SettingType type, SettingGroup group)
        {
            Type = type;
            Group = group;
            Name = field.Name;
            DisplayName = Name;
            DisplayNameAttribute.GetDisplayName(field, ref DisplayName);
            CategoryAttribute.GetCategory( field, ref Category );
            HelpTextAttribute.GetHelpText(field, ref HelpText);
            DependsOnAttribute.GetDependsOn( field, out m_dependsOn );
        }

		public abstract void WriteDeclaration( List<string> lines );
        public abstract void WriteDeclaration( FileWriter fw );

        private List<Tuple<string, bool>> m_dependsOn;
    }


	/// <summary>
    /// Maps to dynamically created DomNodeType
    /// </summary>
    public class SettingGroup
	{
		/// <summary>
        /// 'Structure only' name
        /// </summary>
        public string Name;
        public bool HasPresets; // support for presets might be added in the future
        public SettingGroup ParentStructure { get { return m_parentStructure; } }

		public List<Setting> Settings { get { return m_settings; } }
        public List<SettingGroup> NestedStructures { get { return m_structures; } }
        public List<Type> ReflectedEnums { get { return m_reflectedEnums; } }

        /// <summary>
        /// Name used in 'c++' generated files. Structure names are separated with scope operator ::
        /// </summary>
        public string CppFullName { get { return m_cppFullName; } }

        /// <summary>
        /// Name used for declaring DomNodeTypes. Structure names are separated with . (dot)
        /// Root setting is not included in this one - it's redundant in this context
        /// </summary>
        public string DomNodeTypeFullName { get { return m_domNodeTypeFullName; } }

        public List<Tuple<string, bool>> DependsOn { get { return m_dependsOn; } }

        public bool IsRootLevel { get { return m_parentStructure != null ? false : true; } }
        public bool IsFirstLevel { get { return m_parentStructure == null ? false : m_parentStructure.ParentStructure == null ? true : false; } }

        public SettingGroup( string name, Type type, SettingGroup parentStructure )
		{
            Name = name;
            if ( type != null )
            {
                HasPresets = HasPresetsAttribute.GetHasPresets( type );
                DependsOnAttribute.GetDependsOn( type, out m_dependsOn );
            }
            m_parentStructure = parentStructure;

            m_cppFullName = name;
            m_domNodeTypeFullName = name;
            SettingGroup tmp = parentStructure;
            if (tmp != null)
            {
                while (tmp.ParentStructure != null)
                {
                    m_cppFullName = tmp.Name + "::" + m_cppFullName;
                    tmp = tmp.ParentStructure;
                }
            }

            tmp = parentStructure;
            while (tmp != null)
            {
                // don't include root in dom type name, it's redundant
                //
                if ( tmp.ParentStructure != null )
                    m_domNodeTypeFullName = tmp.Name + "." + m_domNodeTypeFullName;
                tmp = tmp.ParentStructure;
            }

            m_domNodeTypeFullName = "SettingsEditor:" + m_domNodeTypeFullName;
		}

        public Setting FindSetting( string name )
        {
            return m_settings.Find( n => n.Name == name );
        }

        public bool HasSetting( string name )
        {
            return FindSetting( name ) != null;
        }

        public bool HasGroup( string name )
        {
            return m_structures.Find( n => n.Name == name ) != null;
        }

        private SettingGroup m_parentStructure;
		private List<Setting> m_settings = new List<Setting>();
        private List<SettingGroup> m_structures = new List<SettingGroup>();
        private List<Type> m_reflectedEnums = new List<Type>();
        private string m_cppFullName;
        private string m_domNodeTypeFullName;
        private List<Tuple<string, bool>> m_dependsOn;
    }


    public class BoolSetting : Setting
	{
		public bool Value = false;

		public BoolSetting( bool value, FieldInfo field, SettingGroup group )
			: base( field, SettingType.Bool, group )
		{
			Value = value;
		}

		public override void WriteDeclaration( List<string> lines )
		{
			lines.Add( "\tbool " + Name + " = " + Value.ToString().ToLower() + ";" );
		}

        public override void WriteDeclaration( FileWriter fw )
        {
            fw.AddLine( "bool " + Name + " = " + Value.ToString().ToLower() + ";" );
        }
	}


	public class IntSetting : Setting
	{
		public int Value = 0;
		public int MinValue = 0;
		public int MaxValue = 100;

		public IntSetting( int value, FieldInfo field, SettingGroup group )
			: base( field, SettingType.Int, group )
		{
			Value = value;
			MinAttribute.GetMinValue( field, ref MinValue );
			MaxAttribute.GetMaxValue( field, ref MaxValue );
		}

		public override void WriteDeclaration( List<string> lines )
		{
			lines.Add( "\tint " + Name + " = " + Value.ToString() + ";" );
		}

        public override void WriteDeclaration( FileWriter fw )
        {
            fw.AddLine( "int " + Name + " = " + Value.ToString() + ";" );
        }
	}


	public class EnumSetting : Setting
	{
		public object Value;
		public Type EnumType;
		public string EnumTypeName;
		public int NumEnumValues = 0;

		public EnumSetting( object value, FieldInfo field, Type enumType, SettingGroup group )
			: base( field, SettingType.Enum, group )
		{
			Value = value;
			EnumType = enumType;
			NumEnumValues = EnumType.GetEnumValues().Length;
			EnumTypeName = EnumType.Name;
		}

		public override void WriteDeclaration( List<string> lines )
		{
			lines.Add( "\t" + EnumTypeName + " " + Name + " = " + EnumTypeName + "::" + Value.ToString() + ";" );
		}

        public override void WriteDeclaration( FileWriter fw )
        {
            fw.AddLine( EnumTypeName + " " + Name + " = " + EnumTypeName + "::" + Value.ToString() + ";" );
        }
	}


    public class FloatSetting : Setting
    {
        public float Value = 0.0f;
        public bool ValueChecked = false;
        public float MinValue = 0;
        public float MaxValue = 1;
		public float SoftMinValue = 0;
		public float SoftMaxValue = 1;
		public float StepSize = 0.01f;
        public bool HasCheckBox = false;

        public FloatSetting(float value, FieldInfo field, SettingGroup group)
            : base(field, SettingType.Float, group)
        {
            Value = value;
            MinAttribute.GetMinValue(field, ref MinValue);
            MaxAttribute.GetMaxValue(field, ref MaxValue);
            SoftMinValue = MinValue;
            SoftMaxValue = MaxValue;
            SoftMinAttribute.GetSoftMinValue( field, ref SoftMinValue );
            SoftMaxAttribute.GetSoftMaxValue( field, ref SoftMaxValue );
            SoftMinValue = MathUtil.Clamp<float>( SoftMinValue, MinValue, MaxValue );
            SoftMaxValue = MathUtil.Clamp<float>( SoftMaxValue, MinValue, MaxValue );
            StepSize = (SoftMaxValue - SoftMinValue) * 0.01f;
            StepAttribute.GetStepSize( field, ref StepSize );
            HasCheckBox = CheckBoxAttribute.GetCheckBox( field, ref ValueChecked );
            if ( HasCheckBox )
                Type = SettingType.FloatBool;
        }

		public override void WriteDeclaration( List<string> lines )
		{
			string val = Value.ToString( CultureInfo.InvariantCulture );
			if ( val.Contains( "." ) )
				val = val + 'f';
            if ( HasCheckBox )
                lines.Add( "\tSettingsEditor::FloatBool " + Name + " = SettingsEditor::FloatBool( " + val + ", " + ValueChecked + " );" );
            else
			    lines.Add( "\tfloat " + Name + " = " + val + ";" );
		}

        public override void WriteDeclaration( FileWriter fw )
        {
            string val = Value.ToString( CultureInfo.InvariantCulture );
            if ( val.Contains( "." ) )
                val = val + 'f';
            if ( HasCheckBox )
                fw.AddLine( "SettingsEditor::FloatBool " + Name + " = SettingsEditor::FloatBool( " + val + ", " + ValueChecked.ToString().ToLower() + " );" );
            else
                fw.AddLine( "float " + Name + " = " + val + ";" );
        }
    }


	public class StringSetting : Setting
	{
		public string Value = null;

		public StringSetting( string value, FieldInfo field, SettingGroup group )
			: base( field, SettingType.String, group )
		{
			Value = value;
		}

		public override void WriteDeclaration( List<string> lines )
		{
			if ( string.IsNullOrEmpty(Value) )
                lines.Add( "\tconst char* " + Name + " = nullptr;" );
			else
				lines.Add( "\tconst char* " + Name + " = \"" + Value + "\";" );
		}

        public override void WriteDeclaration( FileWriter fw )
        {
            if (string.IsNullOrEmpty( Value ))
                fw.AddLine( "const char* " + Name + " = nullptr;" );
            else
                fw.AddLine( "const char* " + Name + " = \"" + Value + "\";" );
        }
    }


	//public class DirectionSetting : Setting
	//{
	//	public Direction Value = new Direction(0.0f, 0.0f, 1.0f);

	//	public DirectionSetting(Direction value, FieldInfo field, string group)
	//		: base(field, SettingType.Direction, group)
	//	{
	//		Value = value;
	//	}

	//	public override void WriteDeclaration(List<string> lines)
	//	{
	//		lines.Add("    extern DirectionSetting " + Name + ";");
	//	}

	//	public override void WriteDefinition(List<string> lines)
	//	{
	//		lines.Add("    DirectionSetting " + Name + ";");
	//	}

	//	public override void WriteInitialization(List<string> lines)
	//	{
	//		string paramString = "tweakBar";
	//		paramString += MakeParameter(Name);
	//		paramString += MakeParameter(Group);
	//		paramString += MakeParameter(DisplayName);
	//		paramString += MakeParameter(HelpText);
	//		paramString += MakeParameter(Value);
	//		lines.Add("        " + Name + ".Initialize(" + paramString + ");");
	//		lines.Add("        Settings.AddSetting(&" + Name + ");");
	//		lines.Add("");
	//	}
	//}


	//public class OrientationSetting : Setting
	//{
	//	public Orientation Value = Orientation.Identity;

	//	public OrientationSetting(Orientation value, FieldInfo field, string group)
	//		: base(field, SettingType.Orientation, group)
	//	{
	//		Value = value;
	//	}

	//	public override void WriteDeclaration(List<string> lines)
	//	{
	//		lines.Add("    extern OrientationSetting " + Name + ";");
	//	}

	//	public override void WriteDefinition(List<string> lines)
	//	{
	//		lines.Add("    OrientationSetting " + Name + ";");
	//	}

	//	public override void WriteInitialization(List<string> lines)
	//	{
	//		string paramString = "tweakBar";
	//		paramString += MakeParameter(Name);
	//		paramString += MakeParameter(Group);
	//		paramString += MakeParameter(DisplayName);
	//		paramString += MakeParameter(HelpText);
	//		paramString += MakeParameter(Value);
	//		lines.Add("        " + Name + ".Initialize(" + paramString + ");");
	//		lines.Add("        Settings.AddSetting(&" + Name + ");");
	//		lines.Add("");
	//	}
	//}

	public class ColorSetting : Setting
	{
		public Color Value = new Color( 1.0f, 1.0f, 1.0f );
		//public bool HDR = false;
		//public float MinValue = float.MinValue;
		//public float MaxValue = float.MaxValue;
		//public float StepSize = 0.01f;

		public ColorSetting( Color value, FieldInfo field, SettingGroup group )
			: base( field, SettingType.Color, group )
		{
			Value = value;
			//HDR = HDRAttribute.HDRColor( field );
			//MinAttribute.GetMinValue( field, ref MinValue );
			//MaxAttribute.GetMaxValue( field, ref MaxValue );
			//StepAttribute.GetStepSize( field, ref StepSize );
		}

		public int asInt()
		{
			int r = Sce.Atf.MathUtil.Clamp<int>( (int) (Value.R * 255), 0, 255 );
			int g = Sce.Atf.MathUtil.Clamp<int>( (int) (Value.G * 255), 0, 255 );
			int b = Sce.Atf.MathUtil.Clamp<int>( (int) (Value.B * 255), 0, 255 );
			int result = (0xff << 24) | (r << 16) | (g << 8) | b;
			return result;
		}

		public override void WriteDeclaration( List<string> lines )
		{
			lines.Add( "\tSettingsEditor::Color " + Name + " = SettingsEditor::Color(" + Value.R + ", " + Value.G + ", " + Value.B + ");" );
		}

        public override void WriteDeclaration( FileWriter fw )
        {
            fw.AddLine( "SettingsEditor::Color " + Name + " = SettingsEditor::Color(" + Value.R + ", " + Value.G + ", " + Value.B + ");" );
        }
    }


	public class Float4Setting : Setting
	{
		public Float4 Value = new Float4( 0, 0, 0, 0 );
		//public bool HDR = false;
		//public float MinValue = float.MinValue;
		//public float MaxValue = float.MaxValue;
		//public float StepSize = 0.01f;

		public Float4Setting( Float4 value, FieldInfo field, SettingGroup group )
			: base( field, SettingType.Float4, group )
		{
			Value = value;
			//HDR = HDRAttribute.HDRColor( field );
			//MinAttribute.GetMinValue( field, ref MinValue );
			//MaxAttribute.GetMaxValue( field, ref MaxValue );
			//StepAttribute.GetStepSize( field, ref StepSize );
		}

		public override void WriteDeclaration( List<string> lines )
		{
			lines.Add( "\tSettingsEditor::Float4 " + Name + " = SettingsEditor::Float4(" + Value.X + ", " + Value.Y + ", " + Value.Z + ", " + Value.W + ");" );
		}

        public override void WriteDeclaration( FileWriter fw )
        {
            fw.AddLine( "SettingsEditor::Float4 " + Name + " = SettingsEditor::Float4(" + Value.X + ", " + Value.Y + ", " + Value.Z + ", " + Value.W + ");" );
        }
    }
}