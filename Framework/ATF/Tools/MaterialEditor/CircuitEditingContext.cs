//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.


using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.Adaptable;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    /// <summary>
    /// Class that defines a circuit editing context. Each context represents a circuit,
    /// with a history, selection, and editing capabilities. There may be multiple
    /// contexts within a single circuit document, because each sub-circuit has its own
    /// editing context.</summary>
    public class CircuitEditingContext : Sce.Atf.Controls.Adaptable.Graphs.CircuitEditingContext,
        IEditableGraphContainer<Module, Connection, ICircuitPin>
    {
        /// <summary>
        /// Performs initialization when the adapter is connected to the editing context's DomNode.
        /// Raises the CircuitEditingContext NodeSet event and performs custom processing.</summary>
        protected override void OnNodeSet()
        {
            base.OnNodeSet();

            // for triggering IObservableContext events
            // Sce.Atf.Controls.Adaptable.Graphs.CircuitEditingContext first checks if DomNode's parent IsCircuitItem
            // and then triggers IObservableContext events 
            // MaterialInstance is not Circuit item and in consequence event is not fired, SelectionPropertyEditingContext is not notified about change,
            // and PropertyEditor is not refreshed
            // for CustomEnableAttributePropertyDescriptor and CustomEnableChildAttributePropertyDescriptor to work, this refresh is a must
            // without it, properties won't be rebuild and IsReadOnlyComponent won't be called
            DomNode.AttributeChanged += DomNode_AttributeChanged;
            DomNode.ChildInserted += DomNode_ChildInserted;
            DomNode.ChildRemoved += DomNode_ChildRemoved;
        }

        /// <summary>
        /// Gets DomNodeType of Wire</summary>
        protected override DomNodeType WireType
        {
            get { return Schema.connectionType.Type; }
        }


        #region IEditableGraphContainer<Module, Connection, ICircuitPin>
        /// <summary>
        /// Can given modules be moved into a new container</summary>
        /// <param name="newParent">New module parent</param>
        /// <param name="movingObjects">Objects being moved</param>
        /// <returns>True iff objects can be moved to new parent</returns>
        bool IEditableGraphContainer<Module, Connection, ICircuitPin>.CanMove( object newParent, IEnumerable<object> movingObjects )
        {
            if ( newParent.Is<IReference<Module>>() )
                return false;
            var editableGraphContainer =
                DomNode.Cast<CircuitEditingContext>() as IEditableGraphContainer<Element, Wire, ICircuitPin>;
            return editableGraphContainer.CanMove( newParent, movingObjects );
        }

        /// <summary>
        /// Move the given nodes into the container</summary>
        /// <param name="newParent">New container</param>
        /// <param name="movingObjects">Nodes to move</param>
        void IEditableGraphContainer<Module, Connection, ICircuitPin>.Move( object newParent, IEnumerable<object> movingObjects )
        {
            var editableGraphContainer =
               DomNode.Cast<CircuitEditingContext>() as IEditableGraphContainer<Element, Wire, ICircuitPin>;
            editableGraphContainer.Move( newParent, movingObjects );
        }

        /// <summary>
        /// Can a container be resized</summary>
        /// <param name="container">Container to resize</param>
        /// <param name="borderPart">Part of border to resize</param>
        /// <returns>True iff the container border can be resized</returns>
        bool IEditableGraphContainer<Module, Connection, ICircuitPin>.CanResize( object container, DiagramBorder borderPart )
        {
            var editableGraphContainer =
               DomNode.Cast<CircuitEditingContext>() as IEditableGraphContainer<Element, Wire, ICircuitPin>;
            return editableGraphContainer.CanResize( container, borderPart );
        }

        /// <summary>
        /// Resize a container</summary>
        /// <param name="container">Container to resize</param>
        /// <param name="newWidth">New container width</param>
        /// <param name="newHeight">New container height</param>
        void IEditableGraphContainer<Module, Connection, ICircuitPin>.Resize( object container, int newWidth, int newHeight )
        {
            var editableGraphContainer =
                 DomNode.Cast<CircuitEditingContext>() as IEditableGraphContainer<Element, Wire, ICircuitPin>;
            editableGraphContainer.Resize( container, newWidth, newHeight );
        }

        #endregion

        /// <summary>
        /// Returns whether two nodes can be connected. "from" and "to" refer to the corresponding
        /// properties in IGraphEdge, not to a dragging operation, for example.</summary>
        /// <param name="fromNode">"From" node</param>
        /// <param name="fromRoute">"From" edge route</param>
        /// <param name="toNode">"To" node</param>
        /// <param name="toRoute">"To" edge route</param>
        /// <returns>Whether the "from" node/route can be connected to the "to" node/route</returns>
        public bool CanConnect( Module fromNode, ICircuitPin fromRoute, Module toNode, ICircuitPin toRoute )
        {
            var editableGraphContainer =
               DomNode.Cast<CircuitEditingContext>() as IEditableGraphContainer<Element, Wire, ICircuitPin>;
            return editableGraphContainer.CanConnect( fromNode, fromRoute, toNode, toRoute );
        }

        /// <summary>
        /// Connects the "from" node/route to the "to" node/route by creating an IGraphEdge whose
        /// "from" node is "fromNode", "to" node is "toNode", etc.</summary>
        /// <param name="fromNode">"From" node</param>
        /// <param name="fromRoute">"From" edge route</param>
        /// <param name="toNode">"To" node</param>
        /// <param name="toRoute">"To" edge route</param>
        /// <param name="existingEdge">Existing edge that is being reconnected, or null if new edge</param>
        /// <returns>New edge connecting the "from" node/route to the "to" node/route</returns>
        public Connection Connect( Module fromNode, ICircuitPin fromRoute, Module toNode, ICircuitPin toRoute, Connection existingEdge )
        {
            var editableGraphContainer =
            DomNode.Cast<CircuitEditingContext>() as IEditableGraphContainer<Element, Wire, ICircuitPin>;
            return editableGraphContainer.Connect( fromNode, fromRoute, toNode, toRoute, existingEdge ).Cast<Connection>();
        }

        /// <summary>
        /// Gets whether the edge can be disconnected</summary>
        /// <param name="edge">Edge to disconnect</param>
        /// <returns>Whether the edge can be disconnected</returns>
        public bool CanDisconnect( Connection edge )
        {
            var editableGraphContainer =
              DomNode.Cast<CircuitEditingContext>() as IEditableGraphContainer<Element, Wire, ICircuitPin>;
            return editableGraphContainer.CanDisconnect( edge );
        }

        /// <summary>
        /// Disconnects the edge</summary>
        /// <param name="edge">Edge to disconnect</param>
        public void Disconnect( Connection edge )
        {
            var editableGraphContainer =
                DomNode.Cast<CircuitEditingContext>() as IEditableGraphContainer<Element, Wire, ICircuitPin>;
            editableGraphContainer.Disconnect( edge );
        }

        /// <summary>
        /// Finds element, edge or pin hit by the given point</summary>
        /// <param name="point">point in client space</param>
        /// <remarks>Implement the pick method to support drag & drop onto a group directly</remarks>
        protected override GraphHitRecord<Element, Wire, ICircuitPin> Pick( Point point )
        {
            var viewingContext = DomNode.Cast<IViewingContext>();
            var control = viewingContext.As<AdaptableControl>();
            if ( viewingContext == null || control == null )
                return null;

            var graphAdapter = control.As<D2dGraphAdapter<Module, Connection, ICircuitPin>>();
            GraphHitRecord<Module, Connection, ICircuitPin> hitRecord = graphAdapter.Pick( point );
            var result = new GraphHitRecord<Element, Wire, ICircuitPin>( hitRecord.Node, hitRecord.Part );
            result.SubItem = hitRecord.SubItem;
            result.SubPart = hitRecord.SubPart;

            return result;
        }

        private void DomNode_AttributeChanged( object sender, AttributeEventArgs e )
        {
            NotifyObjectChanged( e.DomNode );
        }

        private void DomNode_ChildInserted( object sender, ChildEventArgs e )
        {
            OnObjectInserted( new ItemInsertedEventArgs<object>( e.Index, e.Child, e.Parent ) );
        }

        private void DomNode_ChildRemoved( object sender, ChildEventArgs e )
        {
            OnObjectRemoved( new ItemRemovedEventArgs<object>( e.Index, e.Child, e.Parent ) );
        }

        #region IInstancingContext Members

        /// <summary>
        /// Tests if can copy selection from the circuit</summary>
        /// <returns>True iff there are items to copy</returns>
        public override bool CanCopy()
        {
            if ( Selection.Any<MaterialInstance>() )
            {
                if ( Selection.All<MaterialInstance>() )
                    return true;

                return false;
            }

            return base.CanCopy();
        }

        /// <summary>
        /// Copies selected items from the circuit</summary>
        /// <returns>DataObject containing an enumeration of selected items</returns>
        public override object Copy()
        {
            if ( Selection.Any<MaterialInstance>() )
            {
                HashSet<object> itemsToCopy = new HashSet<object>();

                // add annotations
                foreach ( MaterialInstance mi in Selection.AsIEnumerable<MaterialInstance>() )
                    itemsToCopy.Add( mi.As<DomNode>() );

                // create format for local use
                DataObject dataObject = new DataObject( itemsToCopy.ToArray() );

                // add a serializable format for the system clipboard
                DomNodeSerializer serializer = new DomNodeSerializer();
                byte[] data = serializer.Serialize( itemsToCopy.AsIEnumerable<DomNode>() );
                dataObject.SetData( CircuitFormat, data );

                return dataObject;
            }

            return base.Copy();
        }

        /// <summary>
        /// Tests if can insert a given object into the circuit</summary>
        /// <param name="insertingObject">Object to insert</param>
        /// <returns>True iff can insert object into the circuit</returns>
        public override bool CanInsert( object insertingObject )
        {
            IDataObject dataObject = (IDataObject)insertingObject;
            IEnumerable<object> items = GetMaterialInstances( dataObject );
            if ( items != null )
                return true;

            return base.CanInsert( insertingObject );
        }

        /// <summary>
        /// Inserts object into circuit at the center of the canvas</summary>
        /// <param name="insertingObject">Object to insert</param>
        public override void Insert( object insertingObject )
        {
            var dataObject = (IDataObject)insertingObject;
            IEnumerable<object> items = GetMaterialInstances( dataObject );
            if ( items == null )
            {
                base.Insert( insertingObject );
                return;
            }

            var itemCopies = DomNode.Copy( items.AsIEnumerable<DomNode>() );

            CircuitDocument circuitDocument = this.Cast<CircuitDocument>();

            foreach ( DomNode dn in itemCopies )
                circuitDocument.MaterialInstances.Add( dn.Cast<MaterialInstance>() );

            Selection.SetRange( itemCopies );
        }

        private bool AreMaterialInstances( IEnumerable<object> items )
        {
            return items.All( o => o.Is<MaterialInstance>() );
        }

        private IEnumerable<object> GetMaterialInstances( IDataObject dataObject )
        {
            // try the local format first
            IEnumerable<object> items = dataObject.GetData( typeof( object[] ) ) as object[];
            if ( items == null )
                return null;

            if ( AreMaterialInstances(items) )
                return items;

            // try serialized format
            byte[] data = dataObject.GetData( CircuitFormat ) as byte[];
            if ( data != null )
            {
                try
                {
                    DomNodeSerializer serializer = new DomNodeSerializer();
                    IEnumerable<DomNode> deserialized = serializer.Deserialize( data, SchemaLoader.GetNodeType );
                    items = deserialized.AsIEnumerable<object>();
                    if ( AreMaterialInstances( items ) )
                        return items;
                }
                catch /*(Exception ex)*/
                {
                    // the app cannot recover when using output service                   
                    //Outputs.WriteLine(OutputMessageType.Warning, ex.Message);
                }
            }

            return null;
        }

        #endregion
    }
}
