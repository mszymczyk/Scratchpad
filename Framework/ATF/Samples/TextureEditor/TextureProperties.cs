//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;

using SharpDX.Direct3D11;

namespace TextureEditor
{
	/// <summary>
	/// Class describing properties of texture resource
	public class TextureProperties : CustomTypeDescriptor
	{
		/// <summary>
		/// Constructor with parameters</summary>
		/// <param name="name">Item name</param>
		/// <param name="type">Item data type</param>
		/// <param name="value">Item value</param>
		//public TextureProperties( string name, DataType type, object value )
		public TextureProperties( TexturePreviewWindowSharpDX previewWindow, TextureWrap tex, TextureWrap texExp )
		{
			m_previewWindow = previewWindow;

			SourceTexture = tex;
			ExportedTexture = texExp;
			FileUri = new Uri( tex.Filename );

			Format = tex.Format.ToString();
			Width = tex.Width;
			Height = tex.Height;
			Depth = tex.Depth;
			MipLevels = tex.MipLevels;
			ArraySize = tex.ArraySize;
			CubeMap = tex.IsCubeMap;

			//if ( tex.IsSrgbFormat )
			//{
			//	DoGammaToLinearConversion = false;
			//	m_isSrgbFormat = true;
			//}
			//else
			//{
			//	DoGammaToLinearConversion = true;
			//}
		}

		public TextureWrap SourceTexture { get; set; }
		public TextureWrap ExportedTexture { get; set; }

		/// <summary>
		/// Uri of a texture</summary>
		[PropertyEditingAttribute]
		public Uri FileUri { get; set; }

		/// <summary>
		/// Format of texture</summary>
		[PropertyEditingAttribute]
		public string Format { get; set; }

		/// <summary>
		/// Width of texture</summary>
		[PropertyEditingAttribute]
		public int Width { get; set; }

		/// <summary>
		/// Height of texture</summary>
		[PropertyEditingAttribute]
		public int Height { get; set; }

		/// <summary>
		/// Depth of texture</summary>
		[PropertyEditingAttribute]
		public int Depth { get; set; }

		/// <summary>
		/// Width of texture</summary>
		[PropertyEditingAttribute]
		public int MipLevels { get; set; }

		/// <summary>
		/// Width of texture</summary>
		[PropertyEditingAttribute]
		public int ArraySize { get; set; }

		/// <summary>
		/// Width of texture</summary>
		[PropertyEditingAttribute]
		public bool CubeMap { get; set; }

		///// <summary>
		///// Mip to display </summary>
		//[PropertyEditingAttribute(false)]
		//public int VisibleMip { get; set; }

		///// <summary>
		///// Slice to display </summary>
		//[PropertyEditingAttribute(false)]
		//public int VisibleSlice { get; set; }

		///// <summary>
		///// Converts texture for preview </summary>
		//public bool DoGammaToLinearConversion { get; set; }

		#region Property Editing

		/// <summary>
		/// Attribute of DataItem</summary>
		[AttributeUsage( AttributeTargets.Property, AllowMultiple = true )]
		public class PropertyEditingAttribute : Attribute
		{
			public bool ReadOnly { get; set; }
			public PropertyEditingAttribute()
			{
				ReadOnly = true;
			}

			public PropertyEditingAttribute( bool readOnly )
			{
				ReadOnly = readOnly;
			}
		}

		/// <summary>
		/// PropertyDescriptor with additional information for a property</summary>
		public class PropertyPropertyDescriptor : System.ComponentModel.PropertyDescriptor
		{
			/// <summary>
			/// Constructor with parameters</summary>
			/// <param name="property">PropertyInfo for property</param>
			/// <param name="ownerType">Owning type</param>
			public PropertyPropertyDescriptor( PropertyInfo property, Type ownerType )
				: base( property.Name, (Attribute[]) property.GetCustomAttributes( typeof( Attribute ), true ) )
			{
				m_property = property;
				m_ownerType = ownerType;
				m_readOnly = true;

				foreach (Attribute attr in property.GetCustomAttributes(typeof(PropertyEditingAttribute), false) )
				{
					PropertyEditingAttribute p = attr as PropertyEditingAttribute;
					m_readOnly = p.ReadOnly;
				}

				m_category = m_readOnly ? "SrcInfo" : "Misc";
			}

			/// <summary>
			/// Gets owning type</summary>
			public Type OwnerType
			{
				get { return m_ownerType; }
			}

			/// <summary>
			/// Gets PropertyInfo for property</summary>
			public PropertyInfo Property
			{
				get { return m_property; }
			}

			/// <summary>
			/// Gets whether this property is read-only</summary>
			public override bool IsReadOnly
			{
				//get { return GetChildProperties().Count <= 0; }
				//get { return true; }
				get { return m_readOnly; }
				//set { m_readOnly = value; }
			}

			public void SetReadOnly( bool readOnly )
			{
				m_readOnly = readOnly;
			}

			/// <summary>
			/// Gets the name of the category to which the member belongs, as specified in the <see cref="T:System.ComponentModel.CategoryAttribute"></see></summary>
			public override string Category
			{
				get { return m_category; }
			}

			public void SetCategory( string category )
			{
				m_category = category;
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
				return m_property.GetValue( component, null );
			}

			/// <summary>
			/// Sets the value of the component to a different value</summary>
			/// <param name="component">Component with the property value that is to be set</param>
			/// <param name="value">New value</param>
			public override void SetValue( object component, object value )
			{
				m_property.SetValue( component, value, null );

				ItemChanged.Raise( component, EventArgs.Empty );
			}

			/// <summary>
			/// Event that is raised after an item changed</summary>
			public event EventHandler ItemChanged;

			private readonly Type m_ownerType;
			private readonly PropertyInfo m_property;
			private string m_category;
			private bool m_readOnly;
		}

		/// <summary>
		/// Returns a collection of property descriptors for the object represented by this type descriptor</summary>
		/// <returns>System.ComponentModel.PropertyDescriptorCollection containing the property descriptions for the object 
		/// represented by this type descriptor. The default is System.ComponentModel.PropertyDescriptorCollection.Empty.</returns>
		public override PropertyDescriptorCollection GetProperties()
		{
			var props = new PropertyDescriptorCollection( null );

			foreach (var property in GetType().GetProperties())
			{
				var propertyDesc =
                    new PropertyPropertyDescriptor( property, GetType() );

				propertyDesc.ItemChanged += PropertyDescItemChanged;

				foreach (Attribute attr in propertyDesc.Attributes)
				{
					if (attr.GetType().Equals( typeof( PropertyEditingAttribute ) ))
						props.Add( propertyDesc );
				}
			}

			//PropertyInfo DoGammaToLinearConversionProp = GetType().GetProperty( "DoGammaToLinearConversion" );
			//PropertyPropertyDescriptor ppd = new PropertyPropertyDescriptor( DoGammaToLinearConversionProp, GetType() );
			//ppd.ItemChanged += PropertyDescItemChanged;
			//ppd.SetCategory( "Misc" );
			//if ( m_isSrgbFormat )
			//	ppd.SetReadOnly( true );
			//else
			//	ppd.SetReadOnly( false );
			//props.Add( ppd );

			return props;
		}

		/// <summary>
		/// Returns an object that contains the property described by the specified property descriptor</summary>
		/// <param name="pd">Property descriptor for which to retrieve the owning object</param>
		/// <returns>System.Object that owns the given property specified by the type descriptor. The default is null.</returns>
		public override object GetPropertyOwner( System.ComponentModel.PropertyDescriptor pd )
		{
			return this;
		}

		private void PropertyDescItemChanged( object sender, EventArgs e )
		{
			m_previewWindow.Invalidate();
			ItemChanged.Raise( this, EventArgs.Empty );
		}

		#endregion

		/// <summary>
		/// Event that is raised after an item changed</summary>
		public event EventHandler ItemChanged;

		private TexturePreviewWindowSharpDX m_previewWindow;
		//private bool m_isSrgbFormat;
	}
}

