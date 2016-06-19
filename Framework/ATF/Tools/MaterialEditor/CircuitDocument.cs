//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    /// <summary>
    /// Adapts the circuit to IDocument and synchronizes URI and dirty bit changes to the
    /// ControlInfo instance used to register the viewing control in the UI</summary>
    public class CircuitDocument : Sce.Atf.Controls.Adaptable.Graphs.CircuitDocument
    {
        /// <summary>
        /// Performs initialization when the adapter is connected to the circuit's DomNode</summary>
        protected override void OnNodeSet()
        {
            // cache these list wrapper objects
            m_materialInstances = new DomNodeListAdapter<MaterialInstance>( DomNode, Schema.circuitDocumentType.instanceChild );
            base.OnNodeSet();
        }

        public IList<MaterialInstance> MaterialInstances
        {
            get { return m_materialInstances; }
        }

        private DomNodeListAdapter<MaterialInstance> m_materialInstances;
    }
}
