//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.Timelines;
using Sce.Atf.Controls.Timelines.Direct2D;
using Sce.Atf.Dom;

//using misz.StatsViewer.DomNodeAdapters;

namespace misz.StatsViewer
{
    /// <summary>
    /// This class provides a history context (for undo/redo) plus selection state. There is one of
    /// these for each TimelineControl and for each "main" ProfilerDocument. (There could be sub-documents that
    /// are referenced by the main document.)</summary>
    public class StatsViewerContext :
        EditingContext,
		//IEnumerableContext,
        IObservableContext
		//INamingContext,
		//IInstancingContext
    {
        /// <summary>
        /// Performs initialization when the adapter is connected to the editing context's DomNode.
        /// Raises the EditingContext NodeSet event and performs custom processing to adapt objects
        /// and subscribe to DomNode change and drag events.</summary>
        protected override void OnNodeSet()
        {
            m_sessionDocument = DomNode.Cast<SessionDocument>();
			m_sessionControl = m_sessionDocument.SessionControl;

            DomNode.AttributeChanged += DomNode_AttributeChanged;
            DomNode.ChildInserted += DomNode_ChildInserted;
            DomNode.ChildRemoved += DomNode_ChildRemoved;

			//m_timelineControl.DragEnter += timelineControl_DragEnter;
			//m_timelineControl.DragOver += timelineControl_DragOver; 
			//m_timelineControl.DragDrop += timelineControl_DragDrop;
			//m_timelineControl.DragLeave += timelineControl_DragLeave;

            base.OnNodeSet();
        }

        /// <summary>
        /// Gets the ProfilerDocument instance</summary>
        public SessionDocument ProfilerDocument
        {
            get { return m_sessionDocument; }
        }

        /// <summary>
        /// Gets the StatsViewerControl instance</summary>
		public SessionControl SessionControl
        {
            get { return m_sessionControl; }
        }

        /// <summary>
        /// Gets or sets the ControlInfo object that was passed to ControlHostService by the TimelineEditor
        /// when opening or creating a document. Setting the Name or Description property on
        /// this ControlInfo object updates the document tab.</summary>
        public ControlInfo ControlInfo
        {
            get { return m_controlInfo; }
            set { m_controlInfo = value; }
        }

		///// <summary>
		///// Performs custom actions on DragEnter events</summary>
		///// <param name="e">DragEventArgs containing event data</param>
		//protected virtual void OnDragEnter(DragEventArgs e)
		//{
		//	var converted = new List<ITimelineObject>(ConvertDrop(e));
		//	m_timelineControl.DragDropObjects = converted;
            
		//	Selection.Clear();
		//	foreach (IEvent draggedEvent in converted.AsIEnumerable<IEvent>())
		//		Selection.Add(new TimelinePath(draggedEvent));
		//}

		///// <summary>
		///// Performs custom actions on DragOver events</summary>
		///// <param name="e">DragEventArgs containing event data</param>
		//protected virtual void OnDragOver(DragEventArgs e)
		//{
		//	e.Effect = DragDropEffects.Copy;
		//}

		///// <summary>
		///// Performs custom actions on DragDrop events</summary>
		///// <param name="e">DragEventArgs containing event data</param>
		//protected virtual void OnDragDrop(DragEventArgs e)
		//{
		//	string name = "Drag and Drop".Localize();
		//	Selection.Clear();
		//	this.DoTransaction(() => Insert(e), name);
		//	m_timelineControl.DragDropObjects = null;
		//	m_timelineControl.Focus();
		//}

		///// <summary>
		///// Performs custom actions on DragLeave events</summary>
		//protected virtual void OnDragLeave()
		//{
		//	m_timelineControl.DragDropObjects = null;
		//	Selection.Clear();
		//}

		//#region IEnumerableContext Members

		///// <summary>
		///// Gets an enumeration of all of the items of this context</summary>
		//IEnumerable<object> IEnumerableContext.Items
		//{
		//	get
		//	{
		//		foreach (TimelinePath path in DomNode.As<ProfilerDocument>().TimelineControl.AllEvents)
		//			yield return path;
		//	}
		//}

		//#endregion

        #region IObservableContext Members

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
        /// Event that is raised when collection has been reloaded</summary>
        public event EventHandler Reloaded;

        #endregion

		//#region INamingContext Members

		///// <summary>
		///// Gets the item's name in the context, or null if none</summary>
		///// <param name="item">Item</param>
		///// <returns>Item's name in the context, or null if none</returns>
		//string INamingContext.GetName(object item)
		//{
		//	IEvent e = item.As<IEvent>();
		//	if (e != null)
		//		return e.Name;

		//	IMarker marker = item.As<IMarker>();
		//	if (marker != null)
		//		return marker.Name;

		//	IGroup group = item.As<IGroup>();
		//	if (group != null)
		//		return group.Name;

		//	ITrack track = item.As<ITrack>();
		//	if (track != null)
		//		return track.Name;

		//	return null;
		//}

		///// <summary>
		///// Returns whether the item can be named</summary>
		///// <param name="item">Item to name</param>
		///// <returns>True iff the item can be named</returns>
		//bool INamingContext.CanSetName(object item)
		//{
		//	IEvent e = item.As<IEvent>();
		//	if (e is IKey)
		//		return false;//Keys.Name currently is always the empty string
		//	if (e != null)
		//		return IsEditable(e);

		//	IMarker marker = item.As<IMarker>();
		//	if (marker != null)
		//		return IsEditable(marker);

		//	IGroup group = item.As<IGroup>();
		//	if (group != null)
		//		return IsEditable(group);

		//	ITrack track = item.As<ITrack>();
		//	if (track != null)
		//		return IsEditable(track);

		//	return false;
		//}

		///// <summary>
		///// Sets the item's name</summary>
		///// <param name="item">Item to name</param>
		///// <param name="name">New item name</param>
		//void INamingContext.SetName(object item, string name)
		//{
		//	IEvent e = item.As<IEvent>();
		//	if (e != null)
		//	{
		//		e.Name = name;
		//		return;
		//	}

		//	IMarker marker = item.As<IMarker>();
		//	if (marker != null)
		//	{
		//		marker.Name = name;
		//		return;
		//	}

		//	IGroup group = item.As<IGroup>();
		//	if (group != null)
		//	{
		//		group.Name = name;
		//		return;
		//	}

		//	ITrack track = item.As<ITrack>();
		//	if (track != null)
		//	{
		//		track.Name = name;
		//		return;
		//	}
		//}

		//#endregion

		//#region IInstancingContext Members

		///// <summary>
		///// Returns whether the context can copy the selection</summary>
		///// <returns>True iff the context can copy</returns>
		//public bool CanCopy()
		//{
		//	return Selection.Count > 0;
		//}

		///// <summary>
		///// Copies the selection. Returns a data object representing the copied items.</summary>
		///// <returns>Data object representing the copied items; e.g., a
		///// System.Windows.Forms.IDataObject object</returns>
		//public object Copy()
		//{
		//	object[] selection = Selection.GetSnapshot();

		//	// Cut + Paste needs to know the original tracks of the cut objects.
		//	m_copyObjToTrack = new Dictionary<ITimelineObject, ITrack>(selection.Length);
		//	foreach (ITimelineObject source in selection.AsIEnumerable<ITimelineObject>())
		//	{
		//		ITrack sourceTrack;
		//		IGroup sourceGroup;
		//		GetTrackAndGroup(source, out sourceTrack, out sourceGroup);
		//		if (sourceTrack != null)
		//			m_copyObjToTrack[source] = sourceTrack;
		//	}

		//	return new DataObject(selection);
		//}

		///// <summary>
		///// Returns whether the context can insert the data object</summary>
		///// <param name="insertingObject">Data to insert; e.g., System.Windows.Forms.IDataObject</param>
		///// <returns>True iff the context can insert the data object</returns>
		//public bool CanInsert(object insertingObject)
		//{
		//	IDataObject dataObject = (IDataObject)insertingObject;
		//	object[] items = dataObject.GetData(typeof(object[])) as object[];
		//	return
		//		items != null &&
		//		AreTimelineItems(items) &&
		//		(TimelineControl.TargetGroup == null ||
		//			TimelineControl.IsEditable(TimelineControl.TargetGroup));
		//}

		///// <summary>
		///// Inserts the data object into the context.
		///// Generic insert via IInstancingContext. Called from, for example, the standard paste command.</summary>
		///// <param name="insertingObject">Data to insert; e.g., System.Windows.Forms.IDataObject</param>
		//public void Insert(object insertingObject)
		//{
		//	IDataObject dataObject = (IDataObject)insertingObject;
		//	// use current document control to center the elements
		//	object[] items = dataObject.GetData(typeof(object[])) as object[];
		//	if (items == null)
		//		return;
		//	IEnumerable<DomNode> sourceDomNodes = PathsToDomNodes(items);
		//	DomNode[] nodeCopies = DomNode.Copy(sourceDomNodes);
		//	var itemSources = new List<ITimelineObject>(sourceDomNodes.AsIEnumerable<ITimelineObject>());
		//	var itemCopies = new List<ITimelineObject>(nodeCopies.AsIEnumerable<ITimelineObject>());

		//	ProfilerDocument document = DomNode.Cast<ProfilerDocument>();
		//	StatsViewerControl timelineControl = document.TimelineControl;

		//	Rectangle clientRect = m_timelineControl.VisibleClientRectangle;
		//	Point clientPoint = new Point(
		//		clientRect.Left + clientRect.Width / 2,
		//		clientRect.Top + clientRect.Height / 2);
		//	CenterEvents(itemCopies.AsIEnumerable<IEvent>(), clientPoint, timelineControl.Transform);

		//	var sourceTargetPairs = new List<Tuple<ITimelineObject, ITimelineObject>>(itemSources.Count);
		//	for (int i = 0; i < itemSources.Count; i++)
		//		sourceTargetPairs.Add(new Tuple<ITimelineObject, ITimelineObject>(itemSources[i], itemCopies[i]));

		//	// Prepare the mapping of source objects to their tracks. These may be known already (from
		//	//  a previous Copy operation), but we can't be sure, so let's augment m_copyObjToTrack if
		//	//  possible.
		//	if (m_copyObjToTrack == null)
		//		m_copyObjToTrack = new Dictionary<ITimelineObject, ITrack>();
		//	foreach (var source in itemSources)
		//	{
		//		IGroup group;
		//		ITrack track;
		//		GetTrackAndGroup(source, out track, out group);
		//		if (track != null)
		//			m_copyObjToTrack[source] = track;
		//	}

		//	// Guess where the user wants to paste. Priority:
		//	// 1. The timeline control's target (currently selected) track.
		//	ITrack targetTrack = timelineControl.TargetTrack != null ? (ITrack)m_timelineControl.TargetTrack.Last : null;
		//	// But only if it's visible.
		//	if (targetTrack != null)
		//	{
		//		if (!IsTrackVisible(targetTrack))
		//			targetTrack = null;
		//	}
		//	// 2. The track in the center of the view.
		//	if (targetTrack == null)
		//	{
		//		ITimelineObject centerObject = Pick(clientPoint);
		//		IGroup centerGroup;
		//		GetTrackAndGroup(centerObject, out targetTrack, out centerGroup);
		//	}
		//	// 3. The first visible track
		//	if (targetTrack == null)
		//	{
		//		foreach (IGroup group in Timeline.Groups)
		//		{
		//			foreach (ITrack track in group.Tracks)
		//			{
		//				if (IsTrackVisible(track))
		//				{
		//					targetTrack = track;
		//					break;
		//				}
		//			}
		//			if (targetTrack != null)
		//				break;
		//		}
		//	}

		//	Dictionary<ITimelineObject, ITrack> copiesToTracks = CreateTrackMappings(
		//		m_ProfilerDocument.Timeline, sourceTargetPairs, targetTrack, m_copyObjToTrack);

		//	foreach (ITimelineObject item in itemCopies)
		//	{
		//		// Not all items will have a track. The item could be a group, for example.
		//		ITrack dropTarget;
		//		copiesToTracks.TryGetValue(item, out dropTarget);
		//		Insert(item, dropTarget);
		//	}

		//	List<TimelinePath> newSelection = new List<TimelinePath>();
		//	foreach(ITimelineObject copy in itemCopies)
		//	{
		//		// Would need a way to get the path of ITimelineReference objects....
		//		if (TimelineControl.GetOwningTimeline(copy) != Timeline)
		//			throw new NotImplementedException("We haven't implemented the ability to insert timeline objects into a sub-document");
		//		newSelection.Add(new TimelinePath(copy));
		//	}

		//	Selection.SetRange(newSelection.AsIEnumerable<object>());
		//}

		///// <summary>
		///// Returns whether the context can delete the selection</summary>
		///// <returns>True iff the context can delete</returns>
		//public bool CanDelete()
		//{
		//	foreach (TimelinePath timelineObject in Selection.AsIEnumerable<TimelinePath>())
		//		if (!TimelineControl.IsEditable(timelineObject))
		//			return false;
		//	return Selection.Count > 0;
		//}

		///// <summary>
		///// Deletes the selection</summary>
		//public void Delete()
		//{
		//	foreach (DomNode node in Selection.AsIEnumerable<DomNode>())
		//		node.RemoveFromParent();

		//	Selection.Clear();
		//}

		//#endregion

        private static void CenterEvents(IEnumerable<IEvent> events, PointF clientPoint, Matrix worldToClient)
        {
            // Calculate the world start and end points for the events on the x-axis.
            float start = float.MaxValue;
            float end = 0;
            foreach (IEvent _event in events)
            {
                start = Math.Min(start, _event.Start);
                end = Math.Max(end, _event.Start + _event.Length);
            }
            float width = end - start;

            // Determine the new world starting point. 'start' ==> newStart
            float newStart = GdiUtil.InverseTransform(worldToClient, clientPoint.X);
            newStart -= width / 2;
            if (newStart < 0)
                newStart = 0;
            newStart = (float)MathUtil.Snap(newStart, 1.0);   // snapped to nearest integral frame number

            // move each event a delta world distance so that the earliest event ('start')
            //  becomes the center point minus the bounding width (newStart)
            float delta = newStart - start;
            foreach (IEvent _event in events)
                _event.Start += delta;
        }

        private bool IsTrackVisible(ITrack track)
        {
            SessionLayout layout = SessionControl.GetLayout();
            RectangleF trackBounds;
            if (layout.TryGetBounds(new SessionPath(track), out trackBounds))
            {
                RectangleF clientRectF = m_sessionControl.VisibleClientRectangle;
                if (clientRectF.Top <= trackBounds.Bottom &&
                    clientRectF.Bottom >= trackBounds.Top)
                    return true;
            }
            return false;
        }

        #region Event Handlers

        private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
        {
			//if (IsTimelineItem(e.DomNode))
			//	OnObjectChanged(new ItemChangedEventArgs<object>(e.DomNode));
        }

        private void DomNode_ChildInserted(object sender, ChildEventArgs e)
        {
			//if (IsTimelineItem(e.Child))
			//	OnObjectInserted(new ItemInsertedEventArgs<object>(e.Index, e.Child, e.Parent));
        }

        private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
        {
			//if (IsTimelineItem(e.Child))
			//	OnObjectRemoved(new ItemRemovedEventArgs<object>(e.Index, e.Child, e.Parent));
        }

		//private void timelineControl_DragEnter(object sender, DragEventArgs e)
		//{
		//	e.Effect = DragDropEffects.None;
		//	if (CanInsert(e.Data))
		//	{
		//		OnDragEnter(e);
		//	}
		//}

		//private void timelineControl_DragOver(object sender, DragEventArgs e)
		//{
		//	e.Effect = DragDropEffects.None;
		//	if (CanInsert(e.Data))
		//	{
		//		OnDragOver(e);
		//	}
		//}

		//private void timelineControl_DragDrop(object sender, DragEventArgs e)
		//{
		//	if (CanInsert(e.Data))
		//	{
		//		OnDragDrop(e);
		//	}
		//}

		//private void timelineControl_DragLeave(object sender, EventArgs e)
		//{
		//	OnDragLeave();
		//}

        #endregion

        /// <summary>
        /// Raises the ItemInserted event and performs custom processing</summary>
        /// <param name="e">ItemInsertedEventArgs containing event data</param>
        protected virtual void OnObjectInserted(ItemInsertedEventArgs<object> e)
        {
            ItemInserted.Raise(this, e);
        }

        /// <summary>
        /// Raises the ItemRemoved event and performs custom processing</summary>
        /// <param name="e">ItemRemovedEventArgs containing event data</param>
        protected virtual void OnObjectRemoved(ItemRemovedEventArgs<object> e)
        {
            ItemRemoved.Raise(this, e);
        }

        /// <summary>
        /// Raises the ItemChanged event and performs custom processing</summary>
        /// <param name="e">ItemChangedEventArgs containing event data</param>
        protected virtual void OnObjectChanged(ItemChangedEventArgs<object> e)
        {
            ItemChanged.Raise(this, e);
        }

        /// <summary>
        /// Raises the Reloaded event and performs custom processing</summary>
        /// <param name="e">EventArgs containing event data</param>
        protected virtual void OnReloaded(EventArgs e)
        {
            Reloaded.Raise(this, e);
        }

		//private static bool AreTimelineItems(IEnumerable<object> items)
		//{
		//	return items.Any(IsTimelineItem);
		//}

		//private static bool IsTimelineItem(object item)
		//{
		//	return
		//		item.Is<ITimelineObject>() ||
		//		item.Is<TimelinePath>();
		//}

		//private ITimelineObject Pick(PointF clientPoint)
		//{
		//	HitRecord hitRecord = m_timelineControl.Pick(clientPoint);
		//	return hitRecord.HitTimelineObject;
		//}

		//private bool IsEditable(ITimelineObject item)
		//{
		//	var path = new TimelinePath(item);
		//	ProfilerDocument document = (ProfilerDocument)TimelineEditor.ProfilerDocumentRegistry.ActiveDocument;
		//	if (document != null)
		//		return document.TimelineControl.IsEditable(path);
		//	return false;
		//}

		//private Timeline m_timeline;
        private SessionDocument m_sessionDocument;
        private SessionControl m_sessionControl;
        private ControlInfo m_controlInfo;
		//private Dictionary<ITimelineObject,ITrack> m_copyObjToTrack;
    }
}
