//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

using Sce.Atf;
using Sce.Atf.Dom;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.PropertyEditing;

namespace pico.Controls.PropertyEditing
{
	/// <summary>
	/// Attribute of DataItem</summary>
	[AttributeUsage( AttributeTargets.Property, AllowMultiple = false )]
	public class CutomPropertyEditingAttribute : Attribute
	{
		public bool ReadOnly { get; set; }
		public string Name { get; set; }
		public string Category { get; set; }
		public string Description { get; set; }
		
		public CutomPropertyEditingAttribute()
		{
			ReadOnly = true;
			Name = null;
			Category = null;
			Description = null;
		}

		public CutomPropertyEditingAttribute( bool readOnly )
		{
			ReadOnly = readOnly;
			Name = null;
			Category = null;
			Description = null;
		}

		public CutomPropertyEditingAttribute( bool readOnly, string name, string category, string description )
		{
			ReadOnly = readOnly;
			Name = name;
			Category = category;
			Description = description;
		}
	}
	/// <summary>
	/// PropertyDescriptor with additional information for a property</summary>
	public class CustomPropertyDescriptor<T> : System.ComponentModel.PropertyDescriptor
		where T : DomNodeAdapter
	{
		public static CustomPropertyDescriptor<T> CreateDescriptor( PropertyInfo property, Type ownerType )
		{
			//bool readOnly = true;
			string name = null;
			string category = null;
			string description = null;

			foreach ( Attribute attr in property.GetCustomAttributes( typeof( CutomPropertyEditingAttribute ), false ) )
			{
				CutomPropertyEditingAttribute p = attr as CutomPropertyEditingAttribute;
				//readOnly = p.ReadOnly;
				name = p.Name;
				category = p.Category;
				description = p.Description;
			}

			CustomPropertyDescriptor<T> desc = new CustomPropertyDescriptor<T>(
				property, ownerType, name, category, description
				);
			return desc;
		}

		public static void CreateDescriptors( PropertyDescriptorCollection propDescCollection )
		{
			System.Type ownerType = typeof( T );

			foreach ( var property in ownerType.GetProperties() )
			{
				Attribute[] attributes = (Attribute[]) property.GetCustomAttributes( typeof( CutomPropertyEditingAttribute ), false );

				foreach ( Attribute attr in attributes )
				{
					if ( attr.GetType().Equals( typeof( CutomPropertyEditingAttribute ) ) )
					{
						propDescCollection.Add( CreateDescriptor(property, ownerType) );
						break;
					}
				}
			}
		}

		/// <summary>
		/// Constructor with parameters</summary>
		/// <param name="property">PropertyInfo for property</param>
		/// <param name="ownerType">Owning type</param>
		public CustomPropertyDescriptor(
			PropertyInfo property,
			Type ownerType,
			string name,
			string category,
			string description
			)
			: base( name != null ? name : property.Name, (Attribute[])property.GetCustomAttributes( typeof( CutomPropertyEditingAttribute ), true ) )
		{
			m_property = property;
			m_ownerType = ownerType;
			m_readOnly = false;

			foreach ( Attribute attr in property.GetCustomAttributes( typeof( CutomPropertyEditingAttribute ), false ) )
			{
				CutomPropertyEditingAttribute p = attr as CutomPropertyEditingAttribute;
				m_readOnly = m_readOnly || p.ReadOnly;
			}

			m_category = category;
			m_description = description;
		}

		///// <summary>
		///// Gets owning type</summary>
		//public Type OwnerType
		//{
		//	get { return m_ownerType; }
		//}

		///// <summary>
		///// Gets PropertyInfo for property</summary>
		//public PropertyInfo Property
		//{
		//	get { return m_property; }
		//}

		/// <summary>
		/// Gets whether this property is read-only</summary>
		public override bool IsReadOnly
		{
			get { return m_readOnly; }
		}

		/// <summary>
		/// Gets the name of the category to which the member belongs, as specified in the <see cref="T:System.ComponentModel.CategoryAttribute"></see></summary>
		public override string Category
		{
			get { return m_category; }
		}

        /// <summary>
        /// Gets the description of the member, as specified in the <see cref="T:System.ComponentModel.DescriptionAttribute"></see></summary>
        public override string Description
        {
            get { return m_description; }
        }

		/// <summary>
		/// Returns whether resetting an object changes its value</summary>
		/// <param name="component">Component to test for reset capability</param>
		/// <returns>Whether resetting a component changes its value</returns>
		public override bool CanResetValue( object component )
		{
			return false;
		}

		/// <summary>
		/// Resets the value for this property of the component to the default value</summary>
		/// <param name="component"></param>
		public override void ResetValue( object component )
		{
		}

		/// <summary>
		/// Determines whether the value of this property needs to be persisted</summary>
		/// <param name="component">Component with the property to be examined for persistence</param>
		/// <returns>True iff the property should be persisted</returns>
		public override bool ShouldSerializeValue( object component )
		{
			return false;
		}

		/// <summary>
		/// Gets the type of the component this property is bound to</summary>
		public override Type ComponentType
		{
			get { return m_property.DeclaringType; }
		}

		/// <summary>
		/// Gets the type of the property</summary>
		public override Type PropertyType
		{
			get { return m_property.PropertyType; }
		}

		/// <summary>
		/// Gets the current value of property on component</summary>
		/// <param name="component">Component with the property value that is to be set</param>
		/// <returns>New value</returns>
		public override object GetValue( object component )
		{
			//return m_property.GetValue( component, null );
			//return null;
			//var node = component.As<DomNode>();
			//if ( node != null )
			//{
			//	return m_property.GetValue( node, null );
			//}
			var t = component.As<T>();
			if ( t != null )
			{
				return m_property.GetValue( t, null );
			}

			return null;
		}

		/// <summary>
		/// Sets the value of the component to a different value</summary>
		/// <param name="component">Component with the property value that is to be set</param>
		/// <param name="value">New value</param>
		public override void SetValue( object component, object value )
		{
			m_property.SetValue( component, value, null );

			//ItemChanged.Raise( component, EventArgs.Empty );
		}

		///// <summary>
		///// Event that is raised after an item changed</summary>
		//public event EventHandler ItemChanged;

		private readonly Type m_ownerType;
		private readonly PropertyInfo m_property;
		private string m_category;
		private string m_description;
		private bool m_readOnly;
	}
}
