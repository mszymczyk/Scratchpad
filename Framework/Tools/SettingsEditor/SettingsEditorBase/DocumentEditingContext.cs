using System;
using System.Collections.Generic;

using Sce.Atf.Adaptation;
using Sce.Atf;
using Sce.Atf.Dom;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Applications;
using System.Windows.Forms;
using System.IO;
using System.Text;

//#pragma warning disable 0169 //disable unused field warning
//#pragma warning disable 0067 //disable unused field warning

namespace SettingsEditor
{
    /// <summary>
    /// Used for updating PropertyEditor on Undo/Redo</summary>
    public class DocumentEditingContext : EditingContext, IObservableContext, INamingContext, IInstancingContext, IPropertyEditingContext
    {
        /// <summary>
        /// Performs initialization when the adapter's node is set.
        /// Subscribes to events for DomNode tree changes and raises Reloaded event.</summary>
        protected override void OnNodeSet()
        {
            DomNode.AttributeChanged += DomNode_AttributeChanged;
            DomNode.ChildInserted += DomNode_ChildInserted;
            DomNode.ChildRemoved += DomNode_ChildRemoved;

            SelectionChanged += DocumentEditingContext_SelectionChanged;

            Reloaded.Raise(this, EventArgs.Empty);
            base.OnNodeSet();

            m_saveFileTimer_.Interval = 300;
            m_saveFileTimer_.Tick += ( object sender, EventArgs e ) =>
                {
                    Document document = this.Cast<Document>();
                    document.SaveImpl();
                    m_saveFileTimer_.Stop();
                };

            m_itemChangedTimer.Interval = 30;
            m_itemChangedTimer.Tick += ( object sender, EventArgs e ) =>
            {
                // for instance, will refresh all PropertyViews that are displaying this property
                // doing it here improves performance considerably
                // DirectionPropertyEditor is very sensitive to slow PropertyView refreshing
                // don't know how passing null affects this event and it's observers
                // DomNode_AttributeChanged is the right place to do it, there we have access to actual node that's being changed
                ItemChanged.Raise( this, new ItemChangedEventArgs<object>( null ) );

                m_itemChangedTimer.Stop();
            };

        }

        private void DocumentEditingContext_SelectionChanged(object sender, EventArgs e)
        {
            Reloaded.Raise(this, EventArgs.Empty); // Reloaded event will refresh PropertyView
        }

        #region IPropertyEditingContext Members

        /// <summary>
        /// Gets an enumeration of the items with properties</summary>
        public IEnumerable<object> Items
        {
            get
            {
                // are there any problems with returning 'Selection' here
                // 'Selection' is required for grid property editor to work on multiple objects simultaneously
                //return new object[] { LastSelected };
                return Selection;
            }
        }

        /// <summary>
        /// Gets an enumeration of the property descriptors for the items</summary>
        public IEnumerable<System.ComponentModel.PropertyDescriptor> PropertyDescriptors
        {
            get { return PropertyUtils.GetSharedProperties( Items ); }
        }

        #endregion

        #region IObservableContext Members
        /// <summary>
        /// Event handler for node inserted in DomNode tree.</summary>
        public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;
        /// <summary>
        /// Event handler for node removed from DomNode tree.</summary>
        public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;
        /// <summary>
        /// Event handler for node changed in DomNode tree.</summary>
        public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;
        /// <summary>
        /// Event that is raised when the DomNode tree has been reloaded.</summary>
        public event EventHandler Reloaded;
        #endregion

        #region IInstancingContext Members

        /// <summary>
        /// Returns whether the context can copy the selection</summary>
        /// <returns>True iff the context can copy</returns>
        public bool CanCopy()
        {
            if ( !Selection.All<Preset>() )
                return false;

            Group g = null;

            foreach( object o in Selection )
            {
                Preset p = o.Cast<Preset>();

                if ( g == null )
                    g = p.Group;

                if ( g != p.Group )
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Copies the selection. Returns a data object representing the copied items.</summary>
        /// <returns>Data object representing the copied items; e.g., a
        /// System.Windows.Forms.IDataObject object</returns>
        public object Copy()
        {
            IEnumerable<DomNode> rootNodes = DomNode.GetRoots( Selection.AsIEnumerable<DomNode>() );
            Preset preset = Selection.LastSelected.Cast<Preset>();

            // store SettingGroup in new objects so we can block pasting these presets to wrong group
            List<object> copies = new List<object>( DomNode.Copy( rootNodes ) );
            foreach ( object o in copies )
            {
                Group g = o.Cast<Group>();
                g.SettingGroup = preset.Group.SettingGroup;
            }
            return new DataObject( copies.ToArray() );
        }

        /// <summary>
        /// Returns whether the context can insert the data object</summary>
        /// <param name="insertingObject">Data to insert; e.g., System.Windows.Forms.IDataObject</param>
        /// <returns>True iff the context can insert the data object</returns>
        public bool CanInsert( object insertingObject )
        {
            IDataObject dataObject = (IDataObject)insertingObject;
            object[] items = dataObject.GetData( typeof( object[] ) ) as object[];
            if ( items == null || items.Length == 0 )
                return false;

            if ( m_insertionParent == null )
                return false;

            IEnumerable<DomNode> childNodes = items.AsIEnumerable<DomNode>();
            DomNode parent = m_insertionParent.As<DomNode>();
            Group group = parent.Cast<Group>();

            if ( parent != null )
            {
                foreach ( DomNode child in childNodes )
                {
                    Group preset = child.Cast<Group>();
                    if ( group.SettingGroup != preset.SettingGroup )
                        // don't allow pasting presets to wrong group
                        return false;

                    // can't add child to parent if it will cause a cycle
                    foreach ( DomNode ancestor in parent.Lineage )
                        if ( ancestor == child )
                            return false;

                    // don't add child to the same parent
                    if ( parent == child.Parent )
                        return false;

                    // make sure parent can hold child of this type
                    if ( !CanParent( parent, child.Type ) )
                        return false;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Inserts the data object into the context</summary>
        /// <param name="insertingObject">Data to insert</param>
        public void Insert( object insertingObject )
        {
            IDataObject dataObject = (IDataObject)insertingObject;
            object[] items = dataObject.GetData( typeof( object[] ) ) as object[];
            if ( items == null || items.Length == 0 )
                return;

            IEnumerable<DomNode> childNodes = items.AsIEnumerable<DomNode>();
            // init extensions for copied DomNodes
            foreach ( DomNode child in childNodes )
                child.InitializeExtensions();

            DomNode parent = m_insertionParent.As<DomNode>();

            if ( parent != null )
            {
                ITransactionContext transactionContext = this.Cast<ITransactionContext>();
                transactionContext.DoTransaction(
                    delegate
                    {
                        foreach ( DomNode child in childNodes )
                        {
                            ChildInfo childInfo = GetChildInfo( parent, child.Type );
                            if ( childInfo != null )
                            {
                                if ( childInfo.IsList )
                                {
                                    IList<DomNode> list = parent.GetChildList( childInfo );
                                    list.Add( child );
                                }
                                else
                                {
                                    parent.SetChild( childInfo, child );
                                }
                            }
                        }
                    }, "Insert objects" );
            }
        }

        /// <summary>
        /// Tests if can delete selected items</summary>
        /// <returns>True iff can delete selected items</returns>
        public bool CanDelete()
        {
            if ( Selection.All<Preset>() )
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Deletes selected items</summary>
        public void Delete()
        {
            IEnumerable<DomNode> rootNodes = DomNode.GetRoots( Selection.AsIEnumerable<DomNode>() );
            foreach ( DomNode node in rootNodes )
                if ( node.Parent != null )
                    node.RemoveFromParent();

            Selection.Clear();
        }

        /// <summary>
        /// Use DOM type metadata to determine if we can parent a child type to a parent</summary>
        /// <param name="parent">Parent</param>
        /// <param name="childType">Child type</param>
        /// <returns></returns>
        private bool CanParent( DomNode parent, DomNodeType childType )
        {
            return GetChildInfo( parent, childType ) != null;
        }

        // use Dom type metadata to get matching child metadata
        private ChildInfo GetChildInfo( DomNode parent, DomNodeType childType )
        {
            foreach ( ChildInfo childInfo in parent.Type.Children )
                if ( childInfo.Type.IsAssignableFrom( childType ) )
                    return childInfo;
            return null;
        }

        /// <summary>
        /// Sets the insertion point as the user clicks and drags over the TreeControl. 
        /// The insertion point determines where paste and drag and drop operations insert new objects into the UI data.</summary>
        /// <param name="insertionParent">Parent where object is inserted</param>
        public void SetInsertionParent( object insertionParent )
        {
            if ( insertionParent.Is<Preset>() )
                m_insertionParent = insertionParent.Cast<Preset>().Group;
            else if ( insertionParent.Is<Group>() )
                m_insertionParent = insertionParent;
            else
                m_insertionParent = null;
        }

        private object m_insertionParent;

        #endregion

        #region TransactionContext Members
        /// <summary>
        /// Performs custom actions after a transaction ends</summary>
        protected override void OnEnded()
        {
            base.OnEnded();

            Document document = this.Cast<Document>();

            // must send info about preset in OnEnded because unique name validator will change preset name after childInserted event
            foreach ( Preset preset in m_addedPresets )
            {
                Group group = preset.Group;

                misz.HubMessageOut msg2 = new misz.HubMessageOut( "settings" );

                msg2.appendString( "setString" );
                msg2.appendString( document.PathRelativeToData );
                msg2.appendString( group.AbsoluteName );
                msg2.appendString( preset.PresetName );
                msg2.appendString( "%$newPreset%$" );
                msg2.appendString( "" );

                misz.HubService.Send( msg2 );

                Group presetGroup = preset.Cast<Group>(); // preset is also a Group
                foreach ( DynamicProperty prop in presetGroup.Properties )
                {
                    SendProperty( prop, document );
                }
            }

            foreach ( KeyValuePair<Preset, string> entry in m_presetRenames )
            {
				// note: rename can happen also during creating new preset
                if ( !m_addedPresets.Contains(entry.Key) )
                {
                    Preset preset = entry.Key;
                    if ( preset.PresetName == entry.Value )
                        continue;

                    Group group = preset.Group;

                    misz.HubMessageOut msg2 = new misz.HubMessageOut( "settings" );

                    msg2.appendString( "setString" );
                    msg2.appendString( document.PathRelativeToData );
                    msg2.appendString( group.AbsoluteName );
                    msg2.appendString( entry.Value );
                    msg2.appendString( "%$renPreset%$" );
                    msg2.appendString( preset.PresetName );

                    misz.HubService.Send( msg2 );
                }
            }

            m_presetRenames.Clear();
            m_addedPresets.Clear();

            if ( m_saveFileTimer_.Enabled )
                m_saveFileTimer_.Stop();

            m_saveFileTimer_.Start(); // delay file save (allows merging multiple slider operations to one)

            if ( m_itemChangedTimer.Enabled )
                m_itemChangedTimer.Stop();

            m_itemChangedTimer.Start(); // delay item changed event to improve performance
        }

        #endregion

        #region INamingContext Members

        /// <summary>
        /// Gets the item's name in the context, or null if none</summary>
        /// <param name="item">Item</param>
        /// <returns>Item's name in the context, or null if none</returns>
        string INamingContext.GetName( object item )
        {
            Preset preset = item.As<Preset>();
            if ( preset != null )
            {
                return preset.PresetName;
            }

            return null;
        }

        /// <summary>
        /// Returns whether the item can be named</summary>
        /// <param name="item">Item to name</param>
        /// <returns>True iff the item can be named</returns>
        bool INamingContext.CanSetName( object item )
        {
            return item.Is<Preset>();
        }

        /// <summary>
        /// Sets the item's name</summary>
        /// <param name="item">Item to name</param>
        /// <param name="name">New item name</param>
        void INamingContext.SetName( object item, string name )
        {
            Preset preset = item.Cast<Preset>();
            preset.PresetName = name;
        }

        #endregion

        bool IsCurveElement( DomNode node, out DynamicProperty prop )
        {
            Curve curve = node.As<Curve>();
            if ( curve != null )
            {
                prop = node.Parent.Cast<DynamicProperty>();
                return true;
            }

            ControlPoint controlPoint = node.As<ControlPoint>();
            if ( controlPoint != null )
            {
                prop = node.Parent.Parent.Cast<DynamicProperty>();
                return true;
            }

            prop = null;
            return false;
        }

        bool IsStringArrayElement(DomNode node, out DynamicProperty prop)
        {
            if (node.Type.Equals(Schema.stringType.Type))
            {
                prop = node.Parent.Cast<DynamicProperty>();
                return true;
            }

            prop = null;
            return false;
        }

        void DomNode_AttributeChanged( object sender, AttributeEventArgs e )
        {
            // for instance, will refresh all PropertyViews that are displaying this property
            // doing it here is the 'right' way to do it, but cases PropertyViews to get refreshed and is very slow
            // DirectionPropertyEditor (Dx11 3d arrow) feels jittery due to this low performance
            // With each mouse move it causes all PropertyViews to refresh
            // See OnNodeSet for another possible place (but with caveats) to raise it
            //if ( this.Is<HistoryContext>() )
            //{
            //    HistoryContext hc = this.Cast<HistoryContext>();
            //    if ( hc.UndoingOrRedoing )
            //        // this causes immediate controls refresh and allows for quick undoing/redoing
            //        ItemChanged.Raise( this, new ItemChangedEventArgs<object>( e.DomNode ) );
            //}
            //else
            //{
            //      ItemChanged.Raise( this, new ItemChangedEventArgs<object>( e.DomNode ) );
            //}

            Document document = this.Cast<Document>();

            if ( e.AttributeInfo.Equivalent( Schema.dynamicPropertyType.extraNameAttribute ) )
            {
                // when this attribute changes, we need to reload properties to make new name visible
                // this is editor only attribute, no need to send notification to client app
                Reloaded.Raise( this, EventArgs.Empty );
            }
            else if ( e.AttributeInfo.Equivalent( Schema.groupType.selectedPresetRefAttribute ) )
            {
                Reloaded.Raise( this, EventArgs.Empty );

				// selected preset has changed

                Group group = e.DomNode.Cast<Group>();

                misz.HubMessageOut msg2 = new misz.HubMessageOut( "settings" );

                msg2.appendString( "setString" );
                msg2.appendString( document.PathRelativeToData );
                msg2.appendString( group.AbsoluteName );
                if ( group.SelectedPresetRef == null )
                    msg2.appendString( "%$nullPreset%$" );
                else
                    msg2.appendString( group.SelectedPresetRef.PresetName );
                msg2.appendString( "%$curPreset%$" );
                msg2.appendString( "" );

                misz.HubService.Send( msg2 );
            }
            else if ( e.AttributeInfo.Equivalent( Schema.presetType.nameAttribute ) )
            {
                Preset preset = e.DomNode.As<Preset>();
                if ( preset != null && !m_presetRenames.ContainsKey(preset) )
                    m_presetRenames.Add( preset, e.OldValue as string );
            }

            if (e.NewValue is string)
            {
                if (e.AttributeInfo.Name == "shaderOutputFile")
                {
                    SettingsCompiler compiler = new SettingsCompiler();
                    string descFullPath = document.DescFilePath;
                    compiler.ReflectSettings( descFullPath );
                    compiler.GenerateHeaderIfChanged( descFullPath, misz.Paths.CodePathToAbsolutePath( (string) e.NewValue ) );
                    return;
                }
            }

            DynamicProperty prop = null;
            if (IsCurveElement(e.DomNode, out prop))
            {
            }
            else if (IsStringArrayElement(e.DomNode, out prop))
            {
            }
            else
            {
                prop = e.DomNode.As<DynamicProperty>();

                if (prop == null)
                    // not a DynamicProperty, ignore
                    return;

                if (e.AttributeInfo != Schema.dynamicPropertyType.bvalAttribute
                    && e.AttributeInfo != Schema.dynamicPropertyType.ivalAttribute
                    && e.AttributeInfo != Schema.dynamicPropertyType.evalAttribute
                    && e.AttributeInfo != Schema.dynamicPropertyType.fvalAttribute
                    && e.AttributeInfo != Schema.dynamicPropertyType.f4valAttribute
                    && e.AttributeInfo != Schema.dynamicPropertyType.colvalAttribute
                    && e.AttributeInfo != Schema.dynamicPropertyType.svalAttribute
                    && e.AttributeInfo != Schema.dynamicPropertyType.dirvalAttribute
                    )
                    return;
            }

            SendProperty( prop, document );
        }

        private void SendProperty( DynamicProperty prop, Document document )
        {
            Preset preset = prop.DomNode.Parent.As<Preset>();
            Group structure = null;
            if ( preset != null )
            {
                structure = preset.DomNode.Parent.Cast<Group>();
            }
            else
            {
                structure = prop.DomNode.Parent.As<Group>();
            }

            if ( structure == null )
                return;

            string structureName = structure.AbsoluteName;
            string presetName = preset != null ? preset.PresetName : "";

            misz.HubMessageOut msg = new misz.HubMessageOut( "settings" );

            if ( prop.PropertyType == SettingType.Bool )
            {
                msg.appendString( "setInt" );
                msg.appendString( document.PathRelativeToData );
                msg.appendString( structureName );
                msg.appendString( presetName );
                msg.appendString( prop.Name );
                msg.appendInt( 1 );
                bool bval = prop.BoolValue;
                msg.appendInt( bval ? 1 : 0 );
            }
            else if ( prop.PropertyType == SettingType.Int || prop.PropertyType == SettingType.Enum )
            {
                msg.appendString( "setInt" );
                msg.appendString( document.PathRelativeToData );
                msg.appendString( structureName );
                msg.appendString( presetName );
                msg.appendString( prop.Name );
                msg.appendInt( 1 );
                int ival = prop.PropertyType == SettingType.Int ? prop.IntValue :  prop.EnumValue;
                msg.appendInt( ival );
            }
            else if ( prop.PropertyType == SettingType.Float )
            {
                msg.appendString( "setFloat" );
                msg.appendString( document.PathRelativeToData );
                msg.appendString( structureName );
                msg.appendString( presetName );
                msg.appendString( prop.Name );
                msg.appendInt( 1 );
                float fval = prop.FloatValue;
                msg.appendFloat( fval );
            }
            else if ( prop.PropertyType == SettingType.FloatBool )
            {
                msg.appendString( "setFloat" );
                msg.appendString( document.PathRelativeToData );
                msg.appendString( structureName );
                msg.appendString( presetName );
                msg.appendString( prop.Name );
                msg.appendInt( 2 );
                float fval = prop.FloatValue;
                msg.appendFloat( fval );
                msg.appendFloat( prop.Checked ? 1 : 0 );
            }
            else if ( prop.PropertyType == SettingType.Float4 )
            {
                msg.appendString( "setFloat" );
                msg.appendString( document.PathRelativeToData );
                msg.appendString( structureName );
                msg.appendString( presetName );
                msg.appendString( prop.Name );
                msg.appendInt( 4 );
                msg.appendFloat( prop.Float4Value.X );
                msg.appendFloat( prop.Float4Value.Y );
                msg.appendFloat( prop.Float4Value.Z );
                msg.appendFloat( prop.Float4Value.W );
            }
            else if ( prop.PropertyType == SettingType.Color )
            {
                msg.appendString( "setFloat" );
                msg.appendString( document.PathRelativeToData );
                msg.appendString( structureName );
                msg.appendString( presetName );
                msg.appendString( prop.Name );
                msg.appendInt( 3 );
                msg.appendFloat( prop.ColorValue.R );
                msg.appendFloat( prop.ColorValue.G );
                msg.appendFloat( prop.ColorValue.B );
            }
            else if ( prop.PropertyType == SettingType.String )
            {
                msg.appendString( "setString" );
                msg.appendString( document.PathRelativeToData );
                msg.appendString( structureName );
                msg.appendString( presetName );
                msg.appendString( prop.Name );
                msg.appendString( prop.StringValue );
            }
            else if ( prop.PropertyType == SettingType.Direction )
            {
                msg.appendString( "setFloat" );
                msg.appendString( document.PathRelativeToData );
                msg.appendString( structureName );
                msg.appendString( presetName );
                msg.appendString( prop.Name );
                msg.appendInt( 3 );
                msg.appendFloat( prop.DirectionValue.X );
                msg.appendFloat( prop.DirectionValue.Y );
                msg.appendFloat( prop.DirectionValue.Z );
            }
            else if ( prop.PropertyType == SettingType.AnimCurve )
            {
                // send whole curve as a string
                // this may be optimized in the future to reduce amount of trafic

                msg.appendString( "setString" );
                msg.appendString( document.PathRelativeToData );
                msg.appendString( structureName );
                msg.appendString( presetName );
                msg.appendString( prop.Name );

                using ( MemoryStream stream = new MemoryStream() )
                {
                    DomXmlWriter writer = new DomXmlWriter( SchemaLoader.s_schemaLoader.TypeCollection );
                    //writer.PersistDefaultAttributes = true;

                    writer.Write( prop.AnimCurveValue.DomNode, stream, new Uri( "sync", UriKind.Relative ) );
                    byte[] bytes = stream.ToArray();
                    msg.appendInt( bytes.Length - 3 );
                    msg.appendBytes( bytes, 3, bytes.Length - 3 ); // skip BOM, 3 bytes, 0xEF, 0xBB, 0xBF
                }
            }
            else if (prop.PropertyType == SettingType.StringArray)
            {
                // send whole string array as a xml
                // this may be optimized in the future to reduce amount of trafic

                msg.appendString("setString");
                msg.appendString(document.PathRelativeToData);
                msg.appendString(structureName);
                msg.appendString(presetName);
                msg.appendString(prop.Name);

                using (MemoryStream stream = new MemoryStream())
                {
                    DomXmlWriter writer = new DomXmlWriter(SchemaLoader.s_schemaLoader.TypeCollection);
                    //writer.PersistDefaultAttributes = true;
                    writer.Write( prop.DomNode, stream, new Uri("sync", UriKind.Relative) );

                    byte[] bytes = stream.ToArray();
                    msg.appendInt(bytes.Length - 3);
                    msg.appendBytes(bytes, 3, bytes.Length - 3); // skip BOM, 3 bytes, 0xEF, 0xBB, 0xBF
                }
            }
            else
            {
                Outputs.WriteLine( OutputMessageType.Error, "Unsupported attribute type!" );
                return;
            }

            misz.HubService.Send( msg );
        }

        void DomNode_ChildInserted( object sender, ChildEventArgs e )
        {
            ItemInserted.Raise( this, new ItemInsertedEventArgs<object>( e.Index, e.Child, e.Parent ) );

            Preset preset = e.Child.As<Preset>();
            if ( preset != null )
            {
                // don't send info about new preset immediately, because near transaction end, unique name validator may change preset name to make it unique
                // just remember pointer to preset and do everything in transaction OnEnded
                m_addedPresets.Add( preset );
            }
        }

        private void DomNode_ChildRemoved( object sender, ChildEventArgs e )
        {
            ItemRemoved.Raise( this, new ItemRemovedEventArgs<object>( e.Index, e.Child, e.Parent ) );

            Preset preset = e.Child.As<Preset>();
            if ( preset != null )
            {
                Document document = this.Cast<Document>();
                Group group = e.Parent.Cast<Group>();

                misz.HubMessageOut msg2 = new misz.HubMessageOut( "settings" );

                msg2.appendString( "setString" );
                msg2.appendString( document.PathRelativeToData );
                msg2.appendString( group.AbsoluteName );
                msg2.appendString( preset.PresetName );
                msg2.appendString( "%$delPreset%$" );
                msg2.appendString( "" );

                misz.HubService.Send( msg2 );
            }
        }

        // timer used to defer saving settings to file, useful for reducing save operations while user is dragging sliders
        System.Windows.Forms.Timer m_saveFileTimer_ = new System.Windows.Forms.Timer();
        // timer for to defer ItemChanged event, improves perf while working with sliders and controls where smooth mouse move is required
        System.Windows.Forms.Timer m_itemChangedTimer = new System.Windows.Forms.Timer();

        private HashSet<Preset> m_addedPresets = new HashSet<Preset>();
        private Dictionary<Preset, string> m_presetRenames = new Dictionary<Preset, string>();
    }
}
