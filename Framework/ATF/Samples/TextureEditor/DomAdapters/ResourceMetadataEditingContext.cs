//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.
using System;
using System.Collections.Generic;

using Sce.Atf;
using Sce.Atf.Dom;
using Sce.Atf.Applications;
using Sce.Atf.Adaptation;

namespace TextureEditor
{
    /// <summary>
    /// Context for manipulating resource metadata. Don't want undo/redo so custom implementation instead of inheriting from EditingContext</summary>
	//public class ResourceMetadataEditingContext : EditingContext, IObservableContext
	public class ResourceMetadataEditingContext : DomNodeAdapter, IObservableContext, ISelectionContext
	{
        /// <summary>
        /// Performs initialization when the adapter's node is set.
        /// Subscribes to events for DomNode tree changes and raises Reloaded event.</summary>
        protected override void OnNodeSet()
        {
            DomNode.AttributeChanged += (sender, e) => ItemChanged.Raise(this, new ItemChangedEventArgs<object>(e.DomNode));
            DomNode.ChildInserted += (sender, e) => ItemInserted.Raise(this, new ItemInsertedEventArgs<object>(e.Index, e.Child, e.Parent));
            DomNode.ChildRemoved += (sender,e)=> ItemRemoved.Raise(this, new ItemRemovedEventArgs<object>(e.Index, e.Child, e.Parent));

			m_selection.Changing += TheSelectionChanging;
			m_selection.Changed += TheSelectionChanged;

            Reloaded.Raise(this, EventArgs.Empty);
            base.OnNodeSet();
        }

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

		#region ISelectionContext Members

		/// <summary>
		/// Gets or sets the selected items</summary>
		public IEnumerable<object> Selection
		{
			get { return m_selection; }
			set { m_selection.SetRange(value); }
		}

		/// <summary>
		/// Gets all selected items of the given type</summary>
		/// <typeparam name="T">Desired item type</typeparam>
		/// <returns>All selected items of the given type</returns>
		public IEnumerable<T> GetSelection<T>() where T : class
		{
			return m_selection.AsIEnumerable<T>();
		}

		/// <summary>
		/// Gets the last selected item as object</summary>
		public object LastSelected
		{
			get { return m_selection.LastSelected; }
		}

		/// <summary>
		/// Gets the last selected item of the given type, which may not be the same
		/// as the LastSelected item</summary>
		/// <typeparam name="T">Desired item type</typeparam>
		/// <returns>Last selected item of the given type</returns>
		public T GetLastSelected<T>() where T : class
		{
			return m_selection.GetLastSelected<T>();
		}

		/// <summary>
		/// Returns whether the selection contains the given item</summary>
		/// <param name="item">Item</param>
		/// <returns>True iff the selection contains the given item</returns>
		public bool SelectionContains(object item)
		{
			return m_selection.Contains(item);
		}

		/// <summary>
		/// Gets the number of items in the current selection</summary>
		public int SelectionCount
		{
			get { return m_selection.Count; }
		}

		/// <summary>
		/// Event that is raised before the selection changes</summary>
		public event EventHandler SelectionChanging;

		/// <summary>
		/// Event that is raised after the selection changes</summary>
		public event EventHandler SelectionChanged;

		#endregion

		private void TheSelectionChanging(object sender, EventArgs e)
		{
			SelectionChanging.Raise(this, EventArgs.Empty);
		}

		private void TheSelectionChanged(object sender, EventArgs e)
		{
			SelectionChanged.Raise(this, EventArgs.Empty);
		}

		private readonly AdaptableSelection<object> m_selection = new AdaptableSelection<object>();
    }
}
