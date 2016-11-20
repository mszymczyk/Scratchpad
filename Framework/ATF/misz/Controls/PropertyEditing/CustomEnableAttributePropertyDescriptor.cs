using System;
using System.ComponentModel;
using Sce.Atf.Dom;

namespace misz.Controls.PropertyEditing
{
	public interface ICustomEnableAttributePropertyDescriptorCallback
	{
		bool IsReadOnly( DomNode domNode, AttributePropertyDescriptor descriptor );
	};

	public class CustomEnableAttributePropertyDescriptorCallback : ICustomEnableAttributePropertyDescriptorCallback
	{
		public enum Condition
		{
			ReadOnlyIfSetToTrue,
			ReadOnlyIfSetToFalse
		};

		public CustomEnableAttributePropertyDescriptorCallback( AttributeInfo attributeInfo, Condition condition )
		{
			if ( attributeInfo.Type.Type != AttributeTypes.Boolean )
			{
				throw new ArgumentException( "attributeInfo must be Boolean type" );
			}
			m_attributeInfo = attributeInfo;
			m_condition = condition;
		}

		public CustomEnableAttributePropertyDescriptorCallback( Func<DomNode, AttributePropertyDescriptor, bool> isReadOnlyPredicate )
		{
			m_isReadOnlyPredicate = isReadOnlyPredicate;
		}

		public bool IsReadOnly( DomNode domNode, AttributePropertyDescriptor descriptor )
		{
			if ( m_isReadOnlyPredicate != null )
			{
				bool bval = m_isReadOnlyPredicate( domNode, descriptor );
				return bval;
			}
			else
			{
				object val = domNode.GetAttribute( m_attributeInfo );
				bool bval = (bool)val;

				if ( m_condition == Condition.ReadOnlyIfSetToTrue )
				{
					return bval;
				}
				else
				{
					return !bval;
				}
			}
		}

		Func<DomNode, AttributePropertyDescriptor, bool> m_isReadOnlyPredicate;
		private AttributeInfo m_attributeInfo;
		private Condition m_condition;
	}

    /// <summary>
    /// Node adapter to get PropertyDescriptors from from metadata
	/// Based on Atf's CustomTypeDescriptorNodeAdapter.
	/// Implements disabling inputs based on selected properties
	/// </summary>
	public class CustomEnableAttributePropertyDescriptor : AttributePropertyDescriptor
	{
		public CustomEnableAttributePropertyDescriptor(
			string name,
			AttributeInfo attribute,
			string category,
			string description,
			bool isReadOnly,
			object editor
			, ICustomEnableAttributePropertyDescriptorCallback callback
			)

			: base( name, attribute, category, description, isReadOnly, editor, null )
		{
			m_callback = callback;
		}

        public CustomEnableAttributePropertyDescriptor(
            string name,
            AttributeInfo attribute,
            string category,
            string description,
            bool isReadOnly,
            object editor,
            TypeConverter typeConverter
            , ICustomEnableAttributePropertyDescriptorCallback callback
            )

            : base( name, attribute, category, description, isReadOnly, editor, typeConverter )
        {
            m_callback = callback;
        }

        public override bool IsReadOnlyComponent( object component )
		{
			DomNode domNode = GetNode( component );
			bool readOnly =  m_callback.IsReadOnly( domNode, this );
			return readOnly;
		}

		/// <summary>
		/// When overridden in a derived class, returns whether resetting an object changes its value</summary>
		/// <param name="component">The component to test for reset capability</param>
		/// <returns>True iff resetting the component changes its value</returns>
		public override bool CanResetValue( object component )
		{
			if ( IsReadOnlyComponent(component) )
				return false;

			return base.CanResetValue( component );
		}

		ICustomEnableAttributePropertyDescriptorCallback m_callback;
	};


    /// <summary>
    /// Node adapter to get PropertyDescriptors from from metadata
	/// Based on Atf's CustomTypeDescriptorNodeAdapter.
	/// Implements disabling inputs based on selected properties
	/// </summary>
	public class CustomEnableChildAttributePropertyDescriptor : ChildAttributePropertyDescriptor
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
        public CustomEnableChildAttributePropertyDescriptor(
            string name,
            AttributeInfo attributeInfo,
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
        }

        public override bool IsReadOnlyComponent( object component )
        {
            DomNode domNode = GetNode( component );
            if ( domNode == null )
                // descriptors are cached and reused, that's why we sometimes get null domNode
                return IsReadOnly;

            bool readOnly = m_callback.IsReadOnly( domNode, this );
            return readOnly;
        }

        /// <summary>
        /// When overridden in a derived class, returns whether resetting an object changes its value</summary>
        /// <param name="component">The component to test for reset capability</param>
        /// <returns>True if resetting the component changes its value</returns>
        public override bool CanResetValue( object component )
        {
            if ( IsReadOnlyComponent( component ) )
                return false;

            return base.CanResetValue( component );
        }

        ICustomEnableAttributePropertyDescriptorCallback m_callback;
    };
}
