using System.Collections.Generic;
using Sce.Atf.Dom;
using Sce.Atf.Adaptation;

namespace CircuitEditorSample
{
    /// <summary>
    /// Adapts DomNode to a MaterialInstance</summary>
    public class MaterialInstance : DomNodeAdapter, IMaterialInstance
    {
        /// <summary>
        /// Creates empty material instance. Sets it's name to default value.
        /// </summary>
        public static MaterialInstance CreateMaterialInstance()
        {
            MaterialInstance mi = new DomNode( Schema.materialInstanceType.Type ).Cast<MaterialInstance>();
            mi.DomNode.SetAttribute( Schema.materialInstanceType.Type.IdAttribute, "MaterialInstance" );
            return mi;
        }

        public string Name
        {
            get { return (string)DomNode.GetAttribute( Schema.materialInstanceType.nameAttribute ); }
            set { DomNode.SetAttribute( Schema.materialInstanceType.nameAttribute, value ); }
        }

        /// <summary>
        /// Returns list of instance parameters
        /// </summary>
        public IList<MaterialInstanceParameter> Parameters
        {
            get { return GetChildList<MaterialInstanceParameter>( Schema.materialInstanceType.parameterChild); }
        }
    }
}
