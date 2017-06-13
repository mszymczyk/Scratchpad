//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Dom;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace misz.Controls.PropertyEditing
{
    /// <summary>
    /// PropertyDescriptor for a child of a child of a node
    /// Based on ChildPropertyDescriptor
    /// Handles case when we want to display properties of child node that has list of children (like list of strings in SettingsEditor and EmbeddedCollectionEditor)
    /// </summary>
    public class ChildOfChildPropertyDescriptor : Sce.Atf.Dom.PropertyDescriptor
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="name">Property's display name</param>
        /// <param name="childInfo">ChildInfo identifying child</param>
        /// <param name="category">Category of property</param>
        /// <param name="description">Description of property</param>
        /// <param name="isReadOnly">Whether or not property is read-only</param>
        public ChildOfChildPropertyDescriptor(
            string name,
            int childIndex,
            ChildInfo childInfo,
            ChildInfo childOfChildInfo,
            string category,
            string description,
            bool isReadOnly,
            object editor
            )

            : base(name, typeof(DomNode), category, description, isReadOnly, editor, null )
        {
            m_childIndex = childIndex;
            m_childInfo = childInfo;
            m_childOfChildInfo = childOfChildInfo;
        }

        #region Overrides

        /// <summary>
        /// Tests equality of property descriptor with object</summary>
        /// <param name="obj">Object to compare to</param>
        /// <returns>True iff property descriptors are identical</returns>
        /// <remarks>Implements Equals() for organizing descriptors in grid controls</remarks>
        public override bool Equals(object obj)
        {
            ChildOfChildPropertyDescriptor other = obj as ChildOfChildPropertyDescriptor;
            if (other == null)
                return false;

            if (m_childIndex != other.m_childIndex)
                return false;

            if (m_childInfo != other.m_childInfo)
                return false;

            if (m_childOfChildInfo != other.m_childOfChildInfo)
                return false;

            return base.Equals(obj);
        }

        /// <summary>
        /// Gets hash code for property descriptor</summary>
        /// <returns>Hash code</returns>
        /// <remarks>Implements GetHashCode() for organizing descriptors in grid controls</remarks>
        public override int GetHashCode()
        {
            int result = base.GetHashCode();
            //result ^= m_childIndex;
            result ^= m_childInfo.GetHashCode();
            result ^= m_childOfChildInfo.GetHashCode();
            return result;
        }

        /// <summary>
        /// When overridden in a derived class, returns whether resetting an object changes its value</summary>
        /// <param name="component">The component to test for reset capability</param>
        /// <returns>True iff resetting the component changes its value</returns>
        public override bool CanResetValue(object component)
        {
            return false;
        }

        /// <summary>
        /// When overridden in a derived class, resets the value for this property of the component to the default value</summary>
        /// <param name="component">The component with the property value that is to be reset to the default value</param>
        /// <exception cref="InvalidOperationException">Can't reset value</exception>
        public override void ResetValue(object component)
        {
            throw new InvalidOperationException("Can't reset value");
        }

        /// <summary>
        /// When overridden in a derived class, gets the result value of the property on a component</summary>
        /// <param name="component">The component with the property for which to retrieve the value</param>
        /// <returns>The value of a property for a given component.</returns>
        public override object GetValue(object component)
        {
            DomNode parentNode = GetNode(component);
            if (parentNode == null)
                return null;

            IList<DomNode> children = parentNode.GetChildList(m_childInfo);
            if ( m_childIndex >= children.Count )
                throw new InvalidOperationException("Descriptor incorrectly reused?");

            DomNode childNode = children[m_childIndex];
            if ( !childNode.Type.IsValid(m_childOfChildInfo) )
                throw new InvalidOperationException("Descriptor incorrectly reused?");

            return childNode.GetChildList(m_childOfChildInfo);
        }

        /// <summary>
        /// When overridden in a derived class, sets the value of the component to a different value</summary>
        /// <param name="component">The component with the property value that is to be set</param>
        /// <param name="value">The new value</param>
        public override void SetValue(object component, object value)
        {
            throw new InvalidOperationException("Can't set value. Should be done via dedicated descriptor");
        }

        #endregion

        /// <summary>
        /// Gets the DOM ChildInfo</summary>
        public ChildInfo ChildOfChildInfo { get { return m_childOfChildInfo; } }

        private readonly int m_childIndex;
        private readonly ChildInfo m_childInfo;
        private readonly ChildInfo m_childOfChildInfo;
    }
}