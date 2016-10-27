using System;
using System.ComponentModel;
using Sce.Atf.Dom;
using pico.Controls.PropertyEditing;
using misz.Gui;

namespace SettingsEditor
{
    /// <summary>
    /// Node adapter to get PropertyDescriptors from from metadata
    /// Based on Atf's CustomTypeDescriptorNodeAdapter.
    /// Implements disabling inputs based on selected properties
    /// </summary>
    public class FlexibleFloatChildAttributePropertyDescriptor : ChildAttributePropertyDescriptor
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="name">Value's display name</param>
        /// <param name="attributeInfo">Attribute metadata</param>
        /// <param name="childInfo">ChildInfo identifying child</param>
        /// <param name="childIndex">Index into ChildInfo, if ChildInfo is a list</param>
        /// <param name="category">Category of property</param>
        /// <param name="description">Description of property</param>
        /// <param name="isReadOnly">Whether or not property is read-only</param>
        /// <param name="editor">The editor used to edit the property</param>
        /// <param name="typeConverter">The type converter used for this property</param>
        public FlexibleFloatChildAttributePropertyDescriptor(
            string name,
            AttributeInfo attributeInfo,
            AttributeInfo softMinAttribute,
            AttributeInfo softMaxAttribute,
            AttributeInfo stepAttribute,
            AttributeInfo extraNameAttribute,
            AttributeInfo checkedAttribute,
            ChildInfo childInfo,
            int childIndex,
            string category,
            string description,
            bool isReadOnly,
            object editor,
            TypeConverter typeConverter
            , ICustomEnableAttributePropertyDescriptorCallback callback
            )
            : base(name, attributeInfo, childInfo, childIndex, category, description, isReadOnly, editor, typeConverter)
        {
            m_callback = callback;
            m_attributeInfo = attributeInfo;
            m_softMinAttribute = softMinAttribute;
            m_softMaxAttribute = softMaxAttribute;
            m_stepAttribute = stepAttribute;
            m_extraNameAttribute = extraNameAttribute;
            m_checkedAttribute = checkedAttribute;
        }

        public override bool IsReadOnlyComponent( object component )
        {
            DomNode domNode = GetNode( component );
            if ( domNode == null )
                // descriptors are cached and reused, that's why we sometimes get null domNode
                return IsReadOnly;

            if ( m_callback != null )
            {
                bool readOnly = m_callback.IsReadOnly( domNode, this );
                return readOnly;
            }

            return IsReadOnly;
        }

        /// <summary>
        /// When overridden in a derived class, returns whether resetting an object changes its value</summary>
        /// <param name="component">The component to test for reset capability</param>
        /// <returns>True if resetting the component changes its value</returns>
        public override bool CanResetValue( object component )
        {
            //if ( IsReadOnlyComponent( component ) )
            //    return false;

            //return base.CanResetValue( component );
            return false;
        }

        /// <summary>
        /// When overridden in a derived class, resets the value for this property of the component to the default value</summary>
        /// <param name="component">The component with the property value that is to be reset to the default value</param>
        public override void ResetValue( object component )
        {
            //FlexibleFloatValue v = new FlexibleFloatValue();
            //v.value = 

            //SetValue( component, m_attributeInfo.DefaultValue );
        }

        /// <summary>
        /// When overridden in a derived class, gets the result value of the property on a component</summary>
        /// <param name="component">The component with the property for which to retrieve the value</param>
        /// <returns>The value of a property for a given component.</returns>
        public override object GetValue( object component )
        {
            object value = null;
            DomNode node = GetNode( component );
            if ( node != null )
            {
                //value = node.GetAttribute( m_attributeInfo );
                FlexibleFloatValue v = new FlexibleFloatValue();
                v.value = (float)node.GetAttribute( m_attributeInfo );
                v.softMin = (float)node.GetAttribute( m_softMinAttribute );
                v.softMax = (float)node.GetAttribute( m_softMaxAttribute );
                v.step = (float)node.GetAttribute( m_stepAttribute );
                v.extraName = (string)node.GetAttribute( m_extraNameAttribute );
                v.check = (bool)node.GetAttribute( m_checkedAttribute );
                value = v;
            }

            return value;
        }

        /// <summary>
        /// When overridden in a derived class, sets the value of the component to a different value</summary>
        /// <param name="component">The component with the property value that is to be set</param>
        /// <param name="value">The new value</param>
        public override void SetValue( object component, object value )
        {
            DomNode node = GetNode( component );
            if ( node != null )
            {
                //node.SetAttribute( m_attributeInfo, value );
                FlexibleFloatValue v = value as FlexibleFloatValue;
                if ( v != null )
                {
                    node.SetAttribute( m_attributeInfo, v.value );
                    node.SetAttribute( m_softMinAttribute, v.softMin );
                    node.SetAttribute( m_softMaxAttribute, v.softMax );
                    node.SetAttribute( m_stepAttribute, v.step );
                    node.SetAttribute( m_extraNameAttribute, v.extraName );
                    node.SetAttribute( m_checkedAttribute, v.check );
                }
                else
                {
                    node.SetAttribute( m_attributeInfo, value );
                }
            }
        }

        ICustomEnableAttributePropertyDescriptorCallback m_callback;
        AttributeInfo m_attributeInfo;
        AttributeInfo m_softMinAttribute;
        AttributeInfo m_softMaxAttribute;
        AttributeInfo m_stepAttribute;
        AttributeInfo m_extraNameAttribute;
        AttributeInfo m_checkedAttribute;
    };
}
