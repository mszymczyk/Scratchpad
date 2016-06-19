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

namespace pico.LogOutput
{
    /// <summary>
    /// Class to generate data items to be displayed in tree editors</summary>
    class DataItem
    {
        /// <summary>
        /// Constructor with parameters</summary>
        /// <param name="parent">Item parent</param>
        /// <param name="name">Item name</param>
        /// <param name="type">Item data type</param>
        /// <param name="value">Item value</param>
        public DataItem(DataItem parent, string name, object value)
        {
            m_parent = parent;

            Name = name;
            Value = value;
        }

        /// <summary>
        /// Gets item parent</summary>
        public DataItem Parent
        {
            get { return m_parent; }
        }

        /// <summary>
        /// Gets item name</summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets item value</summary>
        public object Value { get; set; }

        private readonly DataItem m_parent;
    }

    /// <summary>
    /// Class to generate collection of data items (DataItem) to be displayed in tree editors</summary>
	class LogDataContainer : ITreeListView, IItemView, IObservableContext
    {
        /// <summary>
        /// Constructor without parameters</summary>
		public LogDataContainer()
        {
			if (s_dataImageIndex == -1)
			{
				s_dataImageIndex =
					ResourceUtil.GetImageList16().Images.IndexOfKey(
						Resources.DataImage );
			}

			//if (s_folderImageIndex == -1)
			//{
			//	s_folderImageIndex =
			//		ResourceUtil.GetImageList16().Images.IndexOfKey(
			//			Resources.FolderImage);
			//}

			//GenerateVirtual();
			//GenerateFlat( null );

			//// Stop compiler warning
			//if (Cancelled == null) return;
			if (Reloaded == null) return;
			if (ItemRemoved == null) return;
			if (ItemChanged == null) return;
        }

        /// <summary>
        /// Gets data item at index</summary>
        /// <param name="index">Index of data item</param>
        /// <returns>Data item at index</returns>
        public DataItem this[int index]
        {
            get { return m_data[index]; }
        }

        /// <summary>
        /// Clears all data items</summary>
        public void Clear()
        {
            m_data.Clear();
        }

		///// <summary>
		///// Generates data for a virtual list</summary>
		///// <param name="view">Tree list view</param>
		//public static void GenerateVirtual(ITreeListView view)
		//{
		//	var container = view.As<LogDataContainer>();
		//	if (container == null)
		//		return;

		//	container.GenerateVirtual();
		//}

		///// <summary>
		///// Updates generated data items</summary>
		///// <param name="view">Tree list view</param>
		//public static void Reload(ITreeListView view)
		//{
		//	var container = view.As<DataContainer>();
		//	if (container == null)
		//		return;

		//	container.Reload();
		//}

        #region ITreeListView Interface

        /// <summary>
        /// Gets the root level objects of the tree view</summary>
        public IEnumerable<object> Roots
        {
            get { return m_data.AsIEnumerable<object>(); }
        }

        /// <summary>
        /// Gets enumeration of the children of the given parent object</summary>
        /// <param name="parent">Parent object</param>
        /// <returns>Enumeration of the children of the parent object</returns>
        public IEnumerable<object> GetChildren(object parent)
        {
			//var dataParent = parent.As<DataItem>();
			//if (dataParent == null)
			//	yield break;

			//if (!dataParent.HasChildren)
			//	yield break;

			//foreach (var data in dataParent.Children)
			//	yield return data;
			yield break;
        }

        /// <summary>
        /// Gets names for columns</summary>
        public string[] ColumnNames
        {
            get { return s_columnNames; }
        }

        #endregion

        #region IItemView Interface

        /// <summary>
        /// Fills in or modifies the given display info for the item</summary>
        /// <param name="obj">Item</param>
        /// <param name="info">Display info to update</param>
        public void GetInfo(object obj, ItemInfo info)
        {
            var data = obj.As<DataItem>();
            if (data == null)
                return;

            info.Label = data.Name;
            info.Properties =
                new object[]
                {
					//data.Type.ToString(),
                    data.Value
                };

			//info.IsLeaf = !data.HasChildren;
			//info.ImageIndex =
			//	data.HasChildren
			//		? s_folderImageIndex
			//		: s_dataImageIndex;
			info.ImageIndex = s_dataImageIndex;
        }

        #endregion

		#region IObservableContext Interface

		/// <summary>
		/// Event that is raised when an item is inserted</summary>
		public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

		/// <summary>
		/// Event that is raised when an item is removed</summary>
		public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

		/// <summary>
		/// Event that is raised when an item is changed</summary>
		public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

		/// <summary>
		/// Event that is raised when the collection has been reloaded</summary>
		public event EventHandler Reloaded;

		#endregion

        public void GenerateVirtual()
        {
            var items = s_random.Next(10000, 100001);

            Outputs.WriteLine(
                OutputMessageType.Info,
                "Adding {0} items to the virtual list.",
                items);

            var arrayItems = new object[items];
            for (var i = 0; i < items; i++)
            {
                var data = CreateItem(null);
                arrayItems[i] = data;
                m_data.Add(data);
            }

			// Can accept an array of objects or single object
			ItemInserted.Raise( this, new ItemInsertedEventArgs<object>( -1, arrayItems ) );
		}

		public void GenerateFlat( DataItem parent )
		{
			//Beginning.Raise( this, EventArgs.Empty );

			//var items = s_random.Next( 5, 16 );
			int items = 1000;
			for (var i = 0; i < items; i++)
			{
				var data = CreateItem( parent );
				//data.ItemChanged += DataItemChanged;

				//if (parent != null)
				//	parent.Children.Add( data );
				//else
					m_data.Add( data );

				ItemInserted.Raise( this, new ItemInsertedEventArgs<object>( -1, data, parent ) );
			}

			//if (parent != null)
			//{
			//	ItemChanged.Raise( this, new ItemChangedEventArgs<object>( parent ) );
			//}

			//Ending.Raise( this, EventArgs.Empty );
			//Ended.Raise( this, EventArgs.Empty );
		}

        private void Reload()
        {
			//Beginning.Raise(this, EventArgs.Empty);
			//Reloaded.Raise(this, EventArgs.Empty);
			//Ending.Raise(this, EventArgs.Empty);
			//Ended.Raise(this, EventArgs.Empty);
        }

        private static DataItem CreateItem(DataItem parent)
        {
			//var enumLength = Enum.GetNames(typeof(DataType)).Length;
            var name = CreateString(s_random.Next(2, 11));
			//var type = (DataType)s_random.Next(0, enumLength);

            object value = CreateString(s_random.Next(5, 16));

            var data =
                new DataItem(
                    parent,
                    name,
                    value);

            return data;
        }

        private static string CreateString(int characters)
        {
            var sb = new StringBuilder();

            var max = Alphabet.Length;
            for (var i = 0; i < characters; i++)
            {
                var ch = Alphabet[s_random.Next(0, max)];
                sb.Append(ch);
            }

            return sb.ToString();
        }

        private readonly List<DataItem> m_data = new List<DataItem>();

        private static int s_dataImageIndex = -1;
		//private static int s_folderImageIndex = -1;

        private static readonly Random s_random = new Random();

        private static readonly string[] s_columnNames =
            new[]
            {
                "Name",
                "Value",
            };

        private const string Alphabet =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
    }

	///// <summary>
	///// Class to compare data items for sorting</summary>
	//class LogDataComparer : IComparer<TreeListView.Node>
	//{
	//	/// <summary>
	//	/// Constructor with TreeListView</summary>
	//	/// <param name="control">Tree list view</param>
	//	public LogDataComparer(TreeListView control)
	//	{
	//		m_control = control;
	//	}

	//	/// <summary>
	//	/// Compares two objects and returns a value indicating whether 
	//	/// one is less than, equal to, or greater than the other</summary>
	//	/// <param name="x">First object to compare</param>
	//	/// <param name="y">Second object to compare</param>
	//	/// <returns>Signed integer that indicates the relative values of x and y. 
	//	/// Less than zero: x is less than y. Zero: x equals y. Greater than zero: x is greater than y.</returns>
	//	public int Compare(TreeListView.Node x, TreeListView.Node y)
	//	{
	//		if ((x == null) && (y == null))
	//			return 0;

	//		if (x == null)
	//			return 1;

	//		if (y == null)
	//			return -1;

	//		if (ReferenceEquals(x, y))
	//			return 0;

	//		var lhs = x.Tag.As<DataItem>();
	//		var rhs = y.Tag.As<DataItem>();

	//		if ((lhs == null) && (rhs == null))
	//			return 0;

	//		if (lhs == null)
	//			return 1;

	//		if (rhs == null)
	//			return -1;

	//		CompareFunction[] sortFuncs;
	//		switch (m_control.SortColumn)
	//		{
	//			case 1: sortFuncs = s_column1Sort; break;
	//			case 2: sortFuncs = s_column2Sort; break;
	//			default: sortFuncs = s_column0Sort; break;
	//		}

	//		var result = 0;

	//		for (var i = 0; i < sortFuncs.Length; i++)
	//		{
	//			result = sortFuncs[i](lhs, rhs);
	//			if (result != 0)
	//				break;
	//		}

	//		if (m_control.SortOrder == SortOrder.Descending)
	//			result *= -1;

	//		return result;
	//	}

	//	private static int CompareNames(DataItem x, DataItem y)
	//	{
	//		return string.Compare(x.Name, y.Name);
	//	}

	//	//private static int CompareTypes(DataItem x, DataItem y)
	//	//{
	//	//	if (x.Type == y.Type)
	//	//		return 0;

	//	//	return (int)x.Type < (int)y.Type ? -1 : 1;
	//	//}

	//	private static int CompareValues(DataItem x, DataItem y)
	//	{
	//		return string.Compare(x.Value.ToString(), y.Value.ToString());
	//	}

	//	private delegate int CompareFunction(DataItem x, DataItem y);

	//	private static readonly CompareFunction[] s_column0Sort =
	//		new CompareFunction[] { CompareNames, CompareTypes, CompareValues };

	//	private static readonly CompareFunction[] s_column1Sort =
	//		new CompareFunction[] { CompareTypes, CompareNames, CompareValues };

	//	private static readonly CompareFunction[] s_column2Sort =
	//		new CompareFunction[] { CompareValues, CompareNames, CompareTypes };

	//	private readonly TreeListView m_control;
	//}
}