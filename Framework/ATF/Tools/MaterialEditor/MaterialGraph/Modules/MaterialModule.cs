using System;
using System.Collections.Generic;
using System.Xml;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Controls.Timelines;
using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    /// <summary>
    /// Adapts DomNode to a Track</summary>
    public class MaterialModule : MaterialGraphModuleAdapter
    {
        public static DomNodeType materialType;
        public static MaterialGraphPin pin_BaseColor;
        public static MaterialGraphPin pin_Metalic;
        public static MaterialGraphPin pin_Roughness;
        public static MaterialGraphPin pin_Normal;

        public static void DefineDomNodeType()
        {
            pin_BaseColor = new MaterialGraphPin( "BaseColor".Localize(), MaterialGraphPin.MaterialGraphPinTypeName, 0, MaterialGraphPin.ComponentType.RGB );
            pin_Metalic = new MaterialGraphPin( "Metallic".Localize(), MaterialGraphPin.MaterialGraphPinTypeName, 1, MaterialGraphPin.ComponentType.Red );
            pin_Roughness = new MaterialGraphPin( "Roughness".Localize(), MaterialGraphPin.MaterialGraphPinTypeName, 2, MaterialGraphPin.ComponentType.Red );
            pin_Normal = new MaterialGraphPin( "Normal".Localize(), MaterialGraphPin.MaterialGraphPinTypeName, 3, MaterialGraphPin.ComponentType.RGB );

            DomNodeType dnt = DefineModuleType(
                new XmlQualifiedName( "materialType", Schema.NS ),
                "Material".Localize(),
                "Material".Localize(),
                Resources.LightImage,
                new MaterialGraphPin[]
                {
                    pin_BaseColor,
                    pin_Metalic,
                    pin_Roughness,
                    pin_Normal
                },
                EmptyArray<ElementType.Pin>.Instance );

            materialType = dnt;
            materialType.Define( new ExtensionInfo<MaterialModule>() );
        }

        #region IMaterialGraphNode Members

        public override string EvaluatePin( MaterialGraphPin pin, ShaderSourceCode sourceCode )
        {
            if ( pin == pin_BaseColor )
            {

            }

            throw new MaterialEvaluationException( this, pin, sourceCode );
        }

        #endregion
    }
}



