//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;
using System.Globalization;
using Sce.Atf;

namespace misz.Controls.PropertyEditing
{
    /// <summary>
    /// TypeConverter for use with ColorEditor; converts color stored in DOM as an ARGB int to/from color or string types</summary>
    public class Float3ColorConverter : TypeConverter
    {
        /// <summary>
        /// Determines whether this instance can convert from the specified context</summary>
        /// <param name="context">An System.ComponentModel.ITypeDescriptorContext that provides a format context</param>
        /// <param name="sourceType">A System.Type that represents the type you want to convert from</param>
        /// <returns>True iff this instance can convert from the specified context</returns>
        public override bool CanConvertFrom( ITypeDescriptorContext context, Type sourceType )
        {
            return (sourceType == typeof( string ) ||
                    sourceType == typeof( System.Drawing.Color ));
        }

        /// <summary>
        /// Determines whether this instance can convert to the specified context</summary>
        /// <param name="context">An System.ComponentModel.ITypeDescriptorContext that provides a format context</param>
        /// <param name="destType">Type of the destination</param>
        /// <returns>True iff this instance can convert to the specified context</returns>
        public override bool CanConvertTo( ITypeDescriptorContext context, Type destType )
        {
            return (destType == typeof( string ) ||
                    destType == typeof( System.Drawing.Color ));
        }

        /// <summary>
        /// Converts the given object to the type of this converter, using the specified context and culture information</summary>
        /// <param name="context">An System.ComponentModel.ITypeDescriptorContext that provides a format context</param>
        /// <param name="culture">The <see cref="T:System.Globalization.CultureInfo"></see> to use as the current culture.</param>
        /// <param name="value">The object to convert</param>
        /// <returns>An object that represents the converted value</returns>
        /// <exception cref="T:System.NotSupportedException">The conversion cannot be performed</exception>
        public override object ConvertFrom( ITypeDescriptorContext context, CultureInfo culture, object value )
        {
            if ( value == null )
                return null;

            if ( value is string )
            {
                // string -> float[3]
                string svalue = value as string;
                string[] s = svalue.Split( ';' );
                if ( s.Length == 3 )
                {
                    try
                    {
                        // try parse three strings
                        float[] f = new float[3];
                        f[0] = float.Parse( s[0], CultureInfo.InvariantCulture );
                        f[1] = float.Parse( s[1], CultureInfo.InvariantCulture );
                        f[2] = float.Parse( s[2], CultureInfo.InvariantCulture );
                        return f;
                    }
                    catch
                    {
                        // catch all exceptions and return base implementation
                    }
                }
            }
            else if ( value is System.Drawing.Color )
            {
                // System.Drawing.Color -> float[3]
                System.Drawing.Color color = (System.Drawing.Color)value;
                float m = 1.0f / 255.0f;
                float r = (float)(color.R * m);
                float g = (float)(color.G * m);
                float b = (float)(color.B * m);
                return new float[3] { r, g, b };
            }

            return base.ConvertFrom( context, culture, value );
        }

        /// <summary>Converts the given value object to the specified type, using the specified
        /// context and culture information</summary>
        /// <param name="context">An System.ComponentModel.ITypeDescriptorContext that provides a format context</param>
        /// <param name="culture">A System.Globalization.CultureInfo. If null is passed, the current culture is assumed</param>
        /// <param name="value">The System.Object to convert</param>
        /// <param name="destType">The System.Type to convert the value parameter to</param>
        /// <returns>An object that represents the converted value</returns>
        public override object ConvertTo( ITypeDescriptorContext context,
                CultureInfo culture,
                object value,
                Type destType )
        {
            if ( value == null )
                return null;

            float[] f3 = value as float[];

            if ( f3 != null && f3.Length == 3 )
            {
                if ( destType == typeof( string ) )
                {
                    // float[3] -> string
                    string s0 = f3[0].ToString( "R", CultureInfo.InvariantCulture );
                    string s1 = f3[1].ToString( "R", CultureInfo.InvariantCulture );
                    string s2 = f3[2].ToString( "R", CultureInfo.InvariantCulture );
                    return s0 + "; " + s1 + "; " + s2;
                }
                else if ( destType == typeof( System.Drawing.Color ) )
                {
                    // float[3] -> System.Drawing.Color
                    int r = (int)(MathUtil.Clamp<float>( f3[0], 0, 1 ) * 255);
                    int g = (int)(MathUtil.Clamp<float>( f3[1], 0, 1 ) * 255);
                    int b = (int)(MathUtil.Clamp<float>( f3[2], 0, 1 ) * 255);
                    return System.Drawing.Color.FromArgb( r, g, b );
                }
            }

            return base.ConvertTo( context, culture, value, destType );
        }
    }
}

