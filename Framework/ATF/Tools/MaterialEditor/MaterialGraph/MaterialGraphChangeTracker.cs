using System.Collections.Generic;
using System.IO;
using System;
using Sce.Atf.Dom;
using Sce.Atf.Adaptation;
using System.Linq;

namespace CircuitEditorSample
{
    /// <summary>
    /// Validator that checks DOM changes against attribute and child rules contained in metadata.
    /// Checks are only made within validations, which are signaled by IValidationContexts within
    /// the DOM data. InvalidTransactionExceptions are thrown, if necessary, in the Ended event
    /// of the IValidationContext.</summary>
    public class MaterialGraphChangeTracker : Validator
    {
        /// <summary>
        /// Performs custom actions after an attribute in the DOM subtree changes</summary>
        /// <param name="sender">Sender (root DOM node)</param>
        /// <param name="e">Attribute change event args</param>
        protected override void OnAttributeChanged(object sender, AttributeEventArgs e)
        {
            if ( Validating )
            {
                if ( e.DomNode.Is<IMaterialInstance>() )
                {
                    IMaterialInstance materialInstance = e.DomNode.As<IMaterialInstance>();
                    // only refresh parameter values
                    m_instancesToRefresh.Add( materialInstance );
                }
                else if ( e.DomNode.Is<MaterialInstanceParameter>() )
                {
                    IMaterialInstance materialInstance = e.DomNode.Parent.Cast<IMaterialInstance>();
                    m_instancesToRefresh.Add( materialInstance );
                }
                else if ( e.DomNode.Is<IMaterialGraphModule>() )
                {
                    IMaterialGraphModule m = e.DomNode.As<IMaterialGraphModule>();
                    if ( m != null )
                    {
                        // check if this module is connected to MaterialModule
                        if ( MaterialGraphUtil.IsConnectedToMaterial( m ) )
                        {
                            if ( m.DoesRequireRecompile( e.AttributeInfo ) )
                            {
                                m_reloadNeeded = true;
                            }
                            else if ( m.DoesRequireRefresh( e.AttributeInfo ) )
                            {
                                m_modulesToRefresh.Add( m.Cast<MaterialGraphModuleAdapter>() );
                            }
                            //m_attributeChanges[e.AttributeInfo] = e.NewValue;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Performs custom actions after a child is inserted into the DOM subtree</summary>
        /// <param name="sender">Sender (root DOM node)</param>
        /// <param name="e">Child event args</param>
        protected override void OnChildInserted(object sender, ChildEventArgs e)
        {
            if ( Validating )
            {
                //m_childChanges.Add( new Pair<Pair<DomNode, DomNode>, ChildInfo>( new Pair<DomNode, DomNode>( e.Parent, e.Child ), e.ChildInfo ) );
                Connection connection = e.Child.As<Connection>();
                if ( connection != null )
                {
                    IMaterialGraphModule materialModule = connection.InputPinTarget.LeafDomNode.As<IMaterialGraphModule>();
                    if ( materialModule != null && MaterialGraphUtil.IsConnectedToMaterial(materialModule) )
                    {
                        m_reloadNeeded = true;
                    }
                }
            }
        }

        /// <summary>
        /// Performs custom actions after a child is removed from the DOM subtree</summary>
        /// <param name="sender">Sender (root DOM node)</param>
        /// <param name="e">Child event args</param>
        protected override void OnChildRemoved(object sender, ChildEventArgs e)
        {
            if ( Validating )
            {
                //    m_childChanges.Add(new Pair<Pair<DomNode, DomNode>, ChildInfo>(new Pair<DomNode, DomNode>(e.Parent, e.Child), e.ChildInfo));
                Connection connection = e.Child.As<Connection>();
                if ( connection != null )
                {
                    IMaterialGraphModule materialModule = connection.InputPinTarget.LeafDomNode.As<IMaterialGraphModule>();
                    if ( materialModule != null && MaterialGraphUtil.IsConnectedToMaterial( materialModule ) )
                    {
                        m_reloadNeeded = true;
                    }
                }
            }
        }

        /// <summary>
        /// Performs custom actions on validation Ending events</summary>
        /// <param name="sender">Validation context</param>
        /// <param name="e">Event args</param>
        protected override void OnEnding( object sender, EventArgs e )
        {
            if ( m_reloadNeeded )
            {
                // update instances
                UpdateMaterialInstances();
            }
        }

        /// <summary>
        /// Performs custom actions after validation finished</summary>
        /// <param name="sender">Sender (root DOM node)</param>
        /// <param name="e">Attribute event args</param>
        protected override void OnEnded(object sender, System.EventArgs e)
        {
            //foreach (KeyValuePair<AttributeInfo, object> keyValuePair in m_attributeChanges)
            //{
            //    AttributeInfo info = keyValuePair.Key;
            //    object newValue = keyValuePair.Value;
            //    if (!info.Validate(newValue))
            //        throw new InvalidTransactionException("invalid attribute value");
            //}
            //m_attributeChanges.Clear();

            //foreach (Pair<Pair<DomNode, DomNode>, ChildInfo> pair in m_childChanges)
            //{
            //    DomNode parent = pair.First.First;
            //    DomNode child = pair.First.Second;
            //    ChildInfo info = pair.Second;
            //    if (!info.Validate(parent, child))
            //        throw new InvalidTransactionException("invalid child removal or insertion");
            //}
            //m_childChanges.Clear();

            if ( m_reloadNeeded )
            {
                //CircuitDocument document = DomNode.GetRoot().Cast<CircuitDocument>();
                //Circuit circuit = document.Cast<Circuit>();

                //MaterialGenerator matGen = new MaterialGenerator( circuit );
                //matGen.Generate();

                misz.HubMessageOut msg = new misz.HubMessageOut( "material" );
                msg.appendString( "reload" );
                string relativeUri = misz.Gui.Paths.MakePathRelativeToScratchpad( document.Uri );
                msg.appendString( relativeUri );

                misz.HubService.Send( msg );
            }

            if ( m_modulesToRefresh.Count > 0 )
            {
                //CircuitDocument document = DomNode.GetRoot().Cast<CircuitDocument>();

                misz.HubMessageOut msg = new misz.HubMessageOut( "material" );
                msg.appendString( "refreshNodes" );
                string relativeUri = misz.Gui.Paths.MakePathRelativeToScratchpad( document.Uri );
                msg.appendString( relativeUri );
                msg.appendInt( m_modulesToRefresh.Count );

                //foreach ( MaterialGraphModuleAdapter m in m_modulesToRefresh )
                //{
                //    Module module = m.Cast<Module>();
                //    DomNodeType type = m.DomNode.Type;
                //    int index = type.Name.LastIndexOf( ':' );
                //    string typeName = type.Name.Substring( index + 1, type.Name.Length - index - 1 );
                //    msg.appendString( typeName );
                //    msg.appendString( module.Id );
                //}

                //MemoryStream stream = new MemoryStream();
                //var writer = new CircuitWriter( MaterialModulePlugin.SchemaLoader.TypeCollection );
                //writer.Write( document.DomNode, stream, document.Uri );

                //msg.appendInt( (int)stream.Length );
                //msg.appendBytes( stream.ToArray() );

                misz.HubService.Send( msg );
            }

            if ( m_instancesToRefresh.Count > 0 )
            {
                //CircuitDocument document = DomNode.GetRoot().Cast<CircuitDocument>();

                misz.HubMessageOut msg = new misz.HubMessageOut( "material" );
                msg.appendString( "refreshInstances" );
                string relativeUri = misz.Gui.Paths.MakePathRelativeToScratchpad( document.Uri );
                msg.appendString( relativeUri );
                msg.appendInt( m_instancesToRefresh.Count );

                //foreach ( IMaterialInstance m in m_instancesToRefresh )
                //{
                //    MaterialInstance mi = m.Cast<MaterialInstance>();
                //    msg.appendString( mi.Name );
                //}

                //MemoryStream stream = new MemoryStream();
                //var writer = new CircuitWriter( MaterialModulePlugin.SchemaLoader.TypeCollection );
                //writer.Write( document.DomNode, stream, document.Uri );

                //msg.appendInt( (int)stream.Length );
                //msg.appendBytes( stream.ToArray() );

                misz.HubService.Send( msg );
            }

            m_reloadNeeded = false;
            m_modulesToRefresh.Clear();
            m_instancesToRefresh.Clear();
        }

        /// <summary>
        /// Performs custom actions when validation canceled</summary>
        /// <param name="sender">Sender (root DOM node)</param>
        /// <param name="e">Attribute event args</param>
        protected override void OnCancelled(object sender, System.EventArgs e)
        {
            //m_childChanges.Clear();
            //m_attributeChanges.Clear();
            m_modulesToRefresh.Clear();
        }

        private void UpdateMaterialInstances()
        {
            CircuitDocument circuitDocument = DomNode.GetRoot().Cast<CircuitDocument>();
            if ( circuitDocument.MaterialInstances.Count > 0 )
            {
                Circuit circuit = circuitDocument.As<Circuit>();

                MaterialModule materialModule = MaterialGraphUtil.GetMaterialModule( circuit.Cast<Circuit>() );
                IList<IMaterialParameterModule> materialParameterModules = MaterialGraphUtil.GetMaterialParameterModules( materialModule );

                foreach ( IMaterialInstance materialInstance in circuitDocument.MaterialInstances )
                {
                    UpdateMaterialInstance( materialInstance.Cast<MaterialInstance>(), materialModule, materialParameterModules );
                }
            }

        }

        private void UpdateMaterialInstance( MaterialInstance materialInstance, MaterialModule materialModule, IList<IMaterialParameterModule> materialParameterModules )
        {
            //List<MaterialInstanceParameter> mipsToDelete = new List<MaterialInstanceParameter>();

            // delete parameters that are referring to non existing parameter modules
            // it's being done automatically by ReferenceValidator
            //foreach ( MaterialInstanceParameter mip in materialInstance.Parameters )
            //{
            //    //bool found = false;
            //    //foreach ( IMaterialParameterModule parameterModule in materialParameterModules )
            //    //{
            //    //    Module pm = parameterModule.As<Module>();
            //    //    if ( mip.Module.Equals( pm ) )
            //    //    {
            //    //        found = true;
            //    //        break;
            //    //    }
            //    //}

            //    //if ( !found )
            //    if ( !materialParameterModules.Any<IMaterialParameterModule>( mpm => mip.Module.Equals( mpm.Cast<Module>() ) ) )
            //        mipsToDelete.Add( mip );
            //}

            //mipsToDelete.ForEach( mip => materialInstance.Parameters.Remove( mip ) );
            //foreach ( MaterialInstanceParameter mip in mipsToDelete )
            //    materialInstance.Parameters.Remove( mip );

            // add new parameters to material instance
            foreach ( IMaterialParameterModule parameterModule in materialParameterModules )
            {
                if ( !materialInstance.Parameters.Any<MaterialInstanceParameter>( mip => mip.Module.Equals( parameterModule.Cast<Module>() ) ) )
                {
                    MaterialInstanceParameter mip = MaterialInstanceParameter.CreateFromParameterModule( parameterModule );
                    materialInstance.Parameters.Add( mip );
                }
            }
        }

        ////pairs of parent and child; todo: use the Tuple in .Net 4.0
        //private HashSet<Pair<Pair<DomNode, DomNode>,ChildInfo>> m_childChanges =
        //    new HashSet<Pair<Pair<DomNode, DomNode>,ChildInfo>>();

        ////pairs of DomNode and its attribute, with the new value
        //private Dictionary<AttributeInfo, object> m_attributeChanges =
        //    new Dictionary<AttributeInfo, object>();

        //pairs of parent and child; todo: use the Tuple in .Net 4.0
        private HashSet<IMaterialGraphModule> m_modulesToRefresh = new HashSet<IMaterialGraphModule>();
        private HashSet<IMaterialInstance> m_instancesToRefresh = new HashSet<IMaterialInstance>();

        private bool m_reloadNeeded = false;
        //private Uri m_resourceRoot = new Uri( System.Environment.GetEnvironmentVariable( "SCRATCHPAD_DIR" ) );
    }
}
