using System.Drawing;
using Sce.Atf.Dom;
using Sce.Atf.Adaptation;
using System;

namespace CircuitEditorSample
{
    /// <summary>
    /// Adapts DomNode to a MaterialInstanceParameter</summary>
    public class MaterialInstanceParameter : DomNodeAdapter
    {
        /// <summary>
        /// Creates parameter based on given parameter module
        /// </summary>
        public static MaterialInstanceParameter CreateFromParameterModule( IMaterialParameterModule parameterModule )
        {
            DomNode node = new DomNode( Schema.materialInstanceParameterType.Type, Schema.materialInstanceType.parameterChild );
            MaterialInstanceParameter mip = node.As<MaterialInstanceParameter>();

            if ( parameterModule.Is<FloatParameterModule>() )
            {
                FloatParameterModule fpm = parameterModule.Cast<FloatParameterModule>();
                mip.Module = parameterModule.Cast<Module>();
                mip.DisplayName = "FloatValue";
                mip.ValueType = Schema.materialInstanceParameterType.floatValueAttribute.Name;
                mip.FloatValue = fpm.FloatValue;
            }
            else if ( parameterModule.Is<ColorParameterModule>() )
            {
                ColorParameterModule cpm = parameterModule.Cast<ColorParameterModule>();
                mip.Module = parameterModule.Cast<Module>();
                mip.DisplayName = "Color";
                mip.ValueType = Schema.materialInstanceParameterType.colorValueAttribute.Name;
                mip.Color = cpm.Color;
            }
            else if ( parameterModule.Is<Texture2DParameterModule>() )
            {
                Texture2DParameterModule pm = parameterModule.Cast<Texture2DParameterModule>();
                mip.Module = parameterModule.Cast<Module>();
                mip.DisplayName = "Filename";
                mip.ValueType = Schema.materialInstanceParameterType.uriValueAttribute.Name;
                mip.Filename = pm.Filename;
            }
            else
            {
                throw new InvalidOperationException( "Unsupported parameter module" );
            }

            return mip;
        }

        /// <summary>
        /// </summary>
        public Module Module
        {
            get { return GetReference<Module>( Schema.materialInstanceParameterType.moduleAttribute ); }
            set { SetReference( Schema.materialInstanceParameterType.moduleAttribute, value ); }
        }

        public string DisplayName
        {
            get { return (string)DomNode.GetAttribute( Schema.materialInstanceParameterType.displayNameAttribute ); }
            set { DomNode.SetAttribute( Schema.materialInstanceParameterType.displayNameAttribute, value ); }
        }

        public string Category
        {
            get { return (string)DomNode.GetAttribute( Schema.materialInstanceParameterType.categoryAttribute ); }
            set { DomNode.SetAttribute( Schema.materialInstanceParameterType.categoryAttribute, value ); }
        }

        public string Description
        {
            get { return (string)DomNode.GetAttribute( Schema.materialInstanceParameterType.descriptionAttribute ); }
            set { DomNode.SetAttribute( Schema.materialInstanceParameterType.descriptionAttribute, value ); }
        }

        public string ValueType
        {
            get { return (string)DomNode.GetAttribute( Schema.materialInstanceParameterType.valueTypeAttribute ); }
            set { DomNode.SetAttribute( Schema.materialInstanceParameterType.valueTypeAttribute, value ); }
        }

        public bool Override
        {
            get { return (bool)DomNode.GetAttribute( Schema.materialInstanceParameterType.overrideAttribute ); }
            set { DomNode.SetAttribute( Schema.materialInstanceParameterType.overrideAttribute, value ); }
        }

        public float FloatValue
        {
            get { return (float)DomNode.GetAttribute( Schema.materialInstanceParameterType.floatValueAttribute ); }
            set { DomNode.SetAttribute( Schema.materialInstanceParameterType.floatValueAttribute, value ); }
        }

        /// <summary>
        /// Gets and sets the parameter's color</summary>
        public Color Color
        {
            get { return Color.FromArgb( (int)DomNode.GetAttribute( Schema.materialInstanceParameterType.colorValueAttribute ) ); }
            set { DomNode.SetAttribute( Schema.materialInstanceParameterType.colorValueAttribute, value.ToArgb() ); }
        }

        /// <summary>
        /// Gets and sets the parameter's filename (textures, etc)</summary>
        public Uri Filename
        {
            get { return (Uri)DomNode.GetAttribute( Schema.materialInstanceParameterType.uriValueAttribute ); }
            set { DomNode.SetAttribute( Schema.materialInstanceParameterType.uriValueAttribute, value ); }
        }
    }
}
