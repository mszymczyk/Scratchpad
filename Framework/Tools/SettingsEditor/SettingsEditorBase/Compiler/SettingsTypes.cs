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
        Float4,
        Color,
        String,
        Direction,
        //Orientation,
        AnimCurve,
        StringArray,
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

        public abstract void WriteDeclaration( FileWriter fw );

        private List<Tuple<string, bool>> m_dependsOn;

        public static string FloatToString( float v )
        {
            string s = v.ToString( CultureInfo.InvariantCulture );
            if ( s.Contains( "." ) )
                s = s + "f";
            return s;
        }
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

        public override void WriteDeclaration( FileWriter fw )
        {
            if ( HasCheckBox )
                fw.AddLine( "SettingsEditor::FloatBool " + Name + " = SettingsEditor::FloatBool( " + FloatToString(Value) + ", " + ValueChecked.ToString().ToLower() + " );" );
            else
                fw.AddLine( "float " + Name + " = " + FloatToString(Value) + ";" );
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

        public override void WriteDeclaration( FileWriter fw )
        {
            if (string.IsNullOrEmpty( Value ))
                fw.AddLine( "const char* " + Name + " = nullptr;" );
            else
            {
                string str = Value.Replace( "\\", "\\\\" );
                fw.AddLine( "const char* " + Name + " = \"" + str + "\";" );
            }
        }
    }


    public class DirectionSetting : Setting
    {
        public Direction Value = new Direction( 0.0f, 0.0f, 1.0f );

        public DirectionSetting( Direction value, FieldInfo field, SettingGroup group )
            : base( field, SettingType.Direction, group )
        {
            Value = value;
        }

        public override void WriteDeclaration( FileWriter fw )
        {
            fw.AddLine( "SettingsEditor::Direction " + Name + " = SettingsEditor::Direction(" + FloatToString(Value.X) + ", " + FloatToString(Value.Y) + ", " + FloatToString(Value.Z) + ");" );
        }
    }


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
    //}

    public class ColorSetting : Setting
    {
        public Color Value = new Color( 1.0f, 1.0f, 1.0f );

        public ColorSetting( Color value, FieldInfo field, SettingGroup group )
            : base( field, SettingType.Color, group )
        {
            Value = value;
        }

        public int asInt()
        {
            int r = Sce.Atf.MathUtil.Clamp<int>( (int) (Value.R * 255), 0, 255 );
            int g = Sce.Atf.MathUtil.Clamp<int>( (int) (Value.G * 255), 0, 255 );
            int b = Sce.Atf.MathUtil.Clamp<int>( (int) (Value.B * 255), 0, 255 );
            int result = (0xff << 24) | (r << 16) | (g << 8) | b;
            return result;
        }

        public override void WriteDeclaration( FileWriter fw )
        {
            fw.AddLine( "SettingsEditor::Color " + Name + " = SettingsEditor::Color(" + FloatToString(Value.R) + ", " + FloatToString(Value.G) + ", " + FloatToString(Value.B) + ");" );
        }
    }


    public class Float4Setting : Setting
    {
        public Float4 Value = new Float4( 0, 0, 0, 0 );

        public Float4Setting( Float4 value, FieldInfo field, SettingGroup group )
            : base( field, SettingType.Float4, group )
        {
            Value = value;
        }

        public override void WriteDeclaration( FileWriter fw )
        {
            fw.AddLine( "SettingsEditor::Float4 " + Name + " = SettingsEditor::Float4(" + FloatToString(Value.X) + ", " + FloatToString(Value.Y) + ", " + FloatToString(Value.Z) + ", " + FloatToString(Value.W) + ");" );
        }
    }

    public class AnimCurveSetting : Setting
    {
        public AnimCurveSetting( FieldInfo field, SettingGroup group )
            : base( field, SettingType.AnimCurve, group )
        {
        }

        public override void WriteDeclaration( FileWriter fw )
        {
            fw.AddLine( "SettingsEditor::AnimCurve " + Name + ";" );
        }
    }

    public class StringArraySetting : Setting
    {
        public StringArraySetting(FieldInfo field, SettingGroup group)
            : base(field, SettingType.StringArray, group)
        {
        }

        public override void WriteDeclaration(FileWriter fw)
        {
            fw.AddLine("SettingsEditor::StringArray " + Name + ";");
        }
    }
}