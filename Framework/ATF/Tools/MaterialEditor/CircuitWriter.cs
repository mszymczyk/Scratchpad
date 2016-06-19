//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    /// <summary>
    /// This custom writer only writes out the sub-circuits that are actually referenced 
    /// by a SubCircuitInstance</summary>
    internal class CircuitWriter : DomXmlWriter
    {
        public CircuitWriter(XmlSchemaTypeCollection typeCollection)
            : base(typeCollection)
        {
            PreserveSimpleElements = true;
            //PersistDefaultAttributes = true; // setting this causes null exceptions during writing (TemplateFolder referenceFile is null)
        }   

        protected override void WriteChildElementsRecursive(DomNode node, XmlWriter writer)
        {
            // Filter out external template file references that should not be in-lined
            if (node.Is<TemplateFolder>())
            {
                var pathUri = node.GetAttribute(Schema.templateFolderType.referenceFileAttribute) as Uri;
                if (pathUri != null)
                    return;
            }
            base.WriteChildElementsRecursive(node, writer);
        }

		protected override string Convert(DomNode node, AttributeInfo attributeInfo)
		{
			object value = node.GetAttribute(attributeInfo);
			if (attributeInfo.Type.Type == AttributeTypes.Reference && attributeInfo.Name == "guidRef")
			{
				// guidRef refers a template whose guid value should be persisted
				var templateNode = value as DomNode;
				return templateNode.GetAttribute(Schema.templateType.guidAttribute) as string;
			}
            else if ( attributeInfo.Type.Type == AttributeTypes.Uri )
            {
                Uri ur = (Uri)value;
                if ( ur.IsAbsoluteUri )
                {
                    //// use resource root to make it relative
                    //ur = misz.Gui.Paths.SCRATCHPAD_DIR_Uri.MakeRelativeUri( ur );

                    //ur = new Uri( Uri.UnescapeDataString( ur.ToString() ), UriKind.Relative );
                    //string valueString = ur.ToString();
                    //return valueString;
                    string relativeDir = misz.Gui.Paths.MakePathRelativeToScratchpad( ur );
                    return relativeDir;
                }
            }

            return base.Convert(node, attributeInfo);
		}
     }

}
