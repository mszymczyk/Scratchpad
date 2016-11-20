using System;
using System.Reflection;
using System.Collections.Generic;

namespace SettingsEditor
{
    public struct Direction
    {
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Z { get; private set; }

        public Direction( float x, float y, float z, bool normalize = false )
        {
            X = x;
            Y = y;
            Z = z;

            if ( normalize )
                Normalize();
        }

        public Direction( float[] f, bool normalize = false )
        {
            X = f[0];
            Y = f[1];
            Z = f[2];

            if ( normalize )
                Normalize();
        }

        public Direction( Direction other )
        {
            X = other.X;
            Y = other.Y;
            Z = other.Z;
        }

        public void Normalize()
        {
            float len = (float)Math.Sqrt( X * X + Y * Y + Z * Z );
            if ( len > 0.00001f )
            {
                float lenRcp = 1.0f / len;
                X *= lenRcp;
                Y *= lenRcp;
                Z *= lenRcp;
            }
        }
    }

    public struct Color
    {
        public float R;
        public float G;
        public float B;

        public Color( float r, float g, float b )
        {
            R = r;
            G = g;
            B = b;
        }

        public Color( float[] rgb )
        {
            R = rgb[0];
            G = rgb[1];
            B = rgb[2];
        }

        public Color( Color other )
        {
            R = other.R;
            G = other.G;
            B = other.B;
        }
    }

    //public struct Orientation
    //{
    //	public float X;
    //	public float Y;
    //	public float Z;
    //	public float W;

    //	public static Orientation Identity = new Orientation( 0.0f, 0.0f, 0.0f, 1.0f );

    //	public Orientation( float x, float y, float z, float w )
    //	{
    //		X = x;
    //		Y = y;
    //		Z = z;
    //		W = w;
    //	}
    //}

    public struct Float4
    {
        public float X;
        public float Y;
        public float Z;
        public float W;

        public Float4( float x, float y, float z, float w )
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public Float4( float[] f )
        {
            X = f[0];
            Y = f[1];
            Z = f[2];
            W = f[3];
        }

        public Float4( Float4 other )
        {
            X = other.X;
            Y = other.Y;
            Z = other.Z;
            W = other.W;
        }
    }


    public struct AnimCurve
    {
    }

    [AttributeUsage( AttributeTargets.Field, Inherited = false, AllowMultiple = false )]
    public class CategoryAttribute : Attribute
    {
        public readonly string Category;

        public CategoryAttribute( string category )
        {
            this.Category = category;
        }

        public static void GetCategory( FieldInfo field, ref string category )
        {
            CategoryAttribute attr = field.GetCustomAttribute<CategoryAttribute>();
            if ( attr != null )
                category = attr.Category;
        }
    }


    [AttributeUsage( AttributeTargets.Field, Inherited = false, AllowMultiple = false )]
    public class DisplayNameAttribute : Attribute
    {
        public readonly string DisplayName;

        public DisplayNameAttribute( string displayName )
        {
            this.DisplayName = displayName;
        }

        public static void GetDisplayName( FieldInfo field, ref string displayName )
        {
            DisplayNameAttribute attr = field.GetCustomAttribute<DisplayNameAttribute>();
            if ( attr != null )
                displayName = attr.DisplayName;
        }
    }


    [AttributeUsage( AttributeTargets.Field | AttributeTargets.Class, Inherited = false, AllowMultiple = false )]
    public class HelpTextAttribute : Attribute
    {
        public readonly string HelpText;

        public HelpTextAttribute( string helpText )
        {
            this.HelpText = helpText;
        }

        public static void GetHelpText( FieldInfo field, ref string helpText )
        {
            HelpTextAttribute attr = field.GetCustomAttribute<HelpTextAttribute>();
            if ( attr != null )
                helpText = attr.HelpText;
        }
    }


    [AttributeUsage( AttributeTargets.Field, Inherited = false, AllowMultiple = false )]
    public class MinAttribute : Attribute
    {
        public readonly float MinValueFloat;
        public readonly int MinValueInt;

        public MinAttribute( float minValue )
        {
            this.MinValueFloat = minValue;
            this.MinValueInt = (int)minValue;
        }

        public MinAttribute( int minValue )
        {
            this.MinValueFloat = (float)minValue;
            this.MinValueInt = minValue;
        }

        public static void GetMinValue( FieldInfo field, ref float minValue )
        {
            MinAttribute attr = field.GetCustomAttribute<MinAttribute>();
            if ( attr != null )
                minValue = attr.MinValueFloat;
        }

        public static void GetMinValue( FieldInfo field, ref int minValue )
        {
            MinAttribute attr = field.GetCustomAttribute<MinAttribute>();
            if ( attr != null )
                minValue = attr.MinValueInt;
        }
    }


    [AttributeUsage( AttributeTargets.Field, Inherited = false, AllowMultiple = false )]
    public class MaxAttribute : Attribute
    {
        public readonly float MaxValueFloat;
        public readonly int MaxValueInt;

        public MaxAttribute( float maxValue )
        {
            this.MaxValueFloat = maxValue;
            this.MaxValueInt = (int)maxValue;
        }

        public MaxAttribute( int maxValue )
        {
            this.MaxValueFloat = (float)maxValue;
            this.MaxValueInt = maxValue;
        }

        public static void GetMaxValue( FieldInfo field, ref float maxValue )
        {
            MaxAttribute attr = field.GetCustomAttribute<MaxAttribute>();
            if ( attr != null )
                maxValue = attr.MaxValueFloat;
        }

        public static void GetMaxValue( FieldInfo field, ref int maxValue )
        {
            MaxAttribute attr = field.GetCustomAttribute<MaxAttribute>();
            if ( attr != null )
                maxValue = attr.MaxValueInt;
        }
    }


    [AttributeUsage( AttributeTargets.Field, Inherited = false, AllowMultiple = false )]
    public class SoftMinAttribute : Attribute
    {
        public readonly float MinValueFloat;
        public readonly int MinValueInt;

        public SoftMinAttribute( float minValue )
        {
            this.MinValueFloat = minValue;
            this.MinValueInt = (int)minValue;
        }

        public SoftMinAttribute( int minValue )
        {
            this.MinValueFloat = (float)minValue;
            this.MinValueInt = minValue;
        }

        public static void GetSoftMinValue( FieldInfo field, ref float minValue )
        {
            SoftMinAttribute attr = field.GetCustomAttribute<SoftMinAttribute>();
            if ( attr != null )
                minValue = attr.MinValueFloat;
        }

        public static void GetSoftMinValue( FieldInfo field, ref int minValue )
        {
            SoftMinAttribute attr = field.GetCustomAttribute<SoftMinAttribute>();
            if ( attr != null )
                minValue = attr.MinValueInt;
        }
    }


    [AttributeUsage( AttributeTargets.Field, Inherited = false, AllowMultiple = false )]
    public class SoftMaxAttribute : Attribute
    {
        public readonly float MaxValueFloat;
        public readonly int MaxValueInt;

        public SoftMaxAttribute( float maxValue )
        {
            this.MaxValueFloat = maxValue;
            this.MaxValueInt = (int)maxValue;
        }

        public SoftMaxAttribute( int maxValue )
        {
            this.MaxValueFloat = (float)maxValue;
            this.MaxValueInt = maxValue;
        }

        public static void GetSoftMaxValue( FieldInfo field, ref float maxValue )
        {
            SoftMaxAttribute attr = field.GetCustomAttribute<SoftMaxAttribute>();
            if ( attr != null )
                maxValue = attr.MaxValueFloat;
        }

        public static void GetMaxValue( FieldInfo field, ref int maxValue )
        {
            SoftMaxAttribute attr = field.GetCustomAttribute<SoftMaxAttribute>();
            if ( attr != null )
                maxValue = attr.MaxValueInt;
        }
    }


    [AttributeUsage( AttributeTargets.Field, Inherited = false, AllowMultiple = false )]
    public class StepAttribute : System.Attribute
    {
        public readonly float StepSizeFloat;
        public readonly int StepSizeInt;

        public StepAttribute( float stepSize )
        {
            this.StepSizeFloat = stepSize;
            this.StepSizeInt = (int)stepSize;
        }

        public StepAttribute( int stepSize )
        {
            this.StepSizeFloat = (float)stepSize;
            this.StepSizeInt = stepSize;
        }

        public static void GetStepSize( FieldInfo field, ref float stepSize )
        {
            StepAttribute attr = field.GetCustomAttribute<StepAttribute>();
            if ( attr != null )
                stepSize = attr.StepSizeFloat;
        }

        public static void GetStepSize( FieldInfo field, ref int stepSize )
        {
            StepAttribute attr = field.GetCustomAttribute<StepAttribute>();
            if ( attr != null )
                stepSize = attr.StepSizeInt;
        }
    }


    [AttributeUsage( AttributeTargets.Field, Inherited = false, AllowMultiple = false )]
    public class EnumLabelAttribute : System.Attribute
    {
        public readonly string Label;
        public EnumLabelAttribute( string label )
        {
            Label = label;
        }
    }


    [AttributeUsage( AttributeTargets.Class, Inherited = false, AllowMultiple = false )]
    public class HasPresetsAttribute : Attribute
    {
        public HasPresetsAttribute()
        {
        }

        public static bool GetHasPresets( Type type )
        {
            HasPresetsAttribute attr = type.GetCustomAttribute<HasPresetsAttribute>();
            if ( attr != null )
                return true;
            else
                return false;
        }
    }


    [AttributeUsage( AttributeTargets.Field, Inherited = false, AllowMultiple = false )]
    public class CheckBoxAttribute : Attribute
    {
        public readonly bool Enabled = true;

        public CheckBoxAttribute(bool enabled)
        {
            this.Enabled = enabled;
        }

        public static bool GetCheckBox( FieldInfo field, ref bool enabled )
        {
            CheckBoxAttribute attr = field.GetCustomAttribute<CheckBoxAttribute>();
            if ( attr != null )
            {
                enabled = attr.Enabled;
                return true;
            }
            else
            {
                enabled = false;
                return false;
            }
        }
    }


    [AttributeUsage( AttributeTargets.Field | AttributeTargets.Class, Inherited = false, AllowMultiple = true )]
    public class DependsOnAttribute : Attribute
    {
        public readonly string SettingName;
        public readonly bool Condition = true;

        public DependsOnAttribute( string settingName, bool condition = true )
        {
            this.SettingName = settingName;
            this.Condition = condition;
        }

        public static void GetDependsOn( FieldInfo field, out List<Tuple<string, bool>> settings )
        {
            settings = new List<Tuple<string, bool>>();

            foreach ( DependsOnAttribute doa in field.GetCustomAttributes<DependsOnAttribute>() )
            {
                settings.Add( new Tuple<string, bool>( doa.SettingName, doa.Condition ) );
            }
        }

        public static void GetDependsOn( Type type, out List<Tuple<string, bool>> settings )
        {
            settings = new List<Tuple<string, bool>>();

            foreach ( DependsOnAttribute doa in type.GetCustomAttributes<DependsOnAttribute>() )
            {
                settings.Add( new Tuple<string, bool>( doa.SettingName, doa.Condition ) );
            }
        }
    }

    //[AttributeUsage(AttributeTargets.Field | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    //public class UseAsShaderConstantAttribute : Attribute
    //{
    //	public readonly bool UseAsShaderConstant = true;

    //	public UseAsShaderConstantAttribute(bool useAsShaderConstant)
    //	{
    //		this.UseAsShaderConstant = useAsShaderConstant;
    //	}

    //	public static bool UseFieldAsShaderConstant(FieldInfo field)
    //	{
    //		UseAsShaderConstantAttribute attr = field.GetCustomAttribute<UseAsShaderConstantAttribute>();
    //		if(attr != null)
    //			return attr.UseAsShaderConstant;
    //		else
    //			return true;
    //	}
    //}

    //[AttributeUsage(AttributeTargets.Field | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    //public class HDRAttribute : Attribute
    //{
    //	public readonly bool HDR = false;

    //	public HDRAttribute(bool hdr)
    //	{
    //		this.HDR = hdr;
    //	}

    //	public static bool HDRColor(FieldInfo field)
    //	{
    //		HDRAttribute attr = field.GetCustomAttribute<HDRAttribute>();
    //		if(attr != null)
    //			return attr.HDR;
    //		else
    //			return false;
    //	}
    //}
}