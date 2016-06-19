//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.IO;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.Timelines;
using Sce.Atf.Controls.Timelines.Direct2D;
using Sce.Atf.Dom;

namespace misz.StatsViewer
{
    /// <summary>
    /// Timeline document, as a DOM hierarchy and identified by an URI. Each StatsViewerD2dControl has one
    /// or more ProfilerDocuments.</summary>
    public class SessionDocument : DomDocument, ISessionDocument, IObservableContext
    {
        /// <summary>
        /// Parameterless constructor for DomNodeAdapters. Set Renderer before using this class.</summary>
        public SessionDocument()
        {
        }

		/// <summary>
		/// Constructor taking a timeline renderer</summary>
		public SessionDocument( SessionRenderer timelineRenderer )
		{
			Renderer = timelineRenderer;
		}

		/// <summary>
		/// Gets the timeline control for editing the document</summary>
		public SessionControl SessionControl
		{
			get { return m_sessionControl; }
		}

		/// <summary>
		/// Gets the document's root timeline</summary>
		public ISession Session
		{
			get { return DomNode.As<ISession>(); }
		}

		/// <summary>
		/// Gets or sets the document's timeline renderer. Creates a StatsViewerD2dControl.</summary>
		public SessionRenderer Renderer
		{
			get { return m_renderer; }
			set
			{
				if (m_renderer != null)
					throw new InvalidOperationException( "The timeline renderer can only be set once" );
				m_renderer = value;

				// Due to recursion, we need m_sessionControl to be valid before m_sessionControl.ProfilerDocument is set.
				// So, we pass in 'null' into StatsViewerD2dControl's constructor.
				m_sessionControl = new SessionControl( null, m_renderer );
				m_sessionControl.SessionDocument = this;

				m_sessionControl.SetZoomRange( 0.1f, 50f, 1f, 100f );
				//AttachManipulators();
			}
		}

		///// <summary>
		///// Gets an enumeration of all editing contexts in the document</summary>
		//public IEnumerable<EditingContext> EditingContexts
		//{
		//	get
		//	{
		//		yield return DomNode.As<EditingContext>();
		//	}
		//}

        #region IResource Members

        /// <summary>
        /// Gets a string identifying the type of the resource to the end-user</summary>
        public override string Type
        {
            get { return "StatsViewerSession".Localize(); }
        }

        #endregion

		///// <summary>
		///// Attaches manipulators to the StatsViewerD2dControl to provide additional capabilities.
		///// Order of adding manipulators matters.</summary>
		//protected virtual void AttachManipulators()
		//{
		//	// The order here determines the order of receiving Paint events and is the reverse
		//	//  order of receiving picking events. For example, a custom Control that is drawn
		//	//  on top of everything else and that can be clicked on should come last in this
		//	//  list so that it is drawn last and is picked first.
		//	new D2dSelectionManipulator(m_sessionControl);
		//	new D2dMoveManipulator(m_sessionControl);
		//	new D2dScaleManipulator(m_sessionControl);
		//	m_splitManipulator = new D2dSplitManipulator(m_sessionControl);
		//	D2dSnapManipulator snapManipulator = new D2dSnapManipulator(m_sessionControl);
		//	D2dScrubberManipulator scrubberManipulator = new ScrubberManipulator(m_sessionControl);

		//	//// Allow the snap manipulator to snap objects to the scrubber.
		//	snapManipulator.Scrubber = scrubberManipulator;
		//}

        /// <summary>
        /// Raises the DirtyChanged event and performs custom processing</summary>
        /// <param name="e">EventArgs containing event data</param>
        protected override void OnDirtyChanged(EventArgs e)
        {
            UpdateControlInfo();

            base.OnDirtyChanged(e);
        }

        /// <summary>
        /// Raises the UriChanged event and performs custom processing</summary>
        /// <param name="e">UriChangedEventArgs containing event data</param>
        protected override void OnUriChanged(UriChangedEventArgs e)
        {
            UpdateControlInfo();

            base.OnUriChanged(e);
        }

        private void UpdateControlInfo()
        {
			StatsViewerContext context = this.As<StatsViewerContext>();

            string filePath;
            if (Uri.IsAbsoluteUri)
                filePath = Uri.LocalPath;
            else
                filePath = Uri.OriginalString;

            string fileName = Path.GetFileName(filePath);
            if (Dirty)
                fileName += "*";

            context.ControlInfo.Name = fileName;
            context.ControlInfo.Description = filePath;
        }

		///// <summary>
		///// Gets the SplitManipulator that was attached to the StatsViewerD2dControl</summary>
		//public SplitManipulator SplitManipulator
		//{
		//	get { return m_splitManipulator; }
		//}

        #region IObservableContext Members

        /// <summary>
        /// Event that is raised when an item is inserted</summary>
        public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted
        {
			add { this.As<StatsViewerContext>().ItemInserted += value; }
			remove { this.As<StatsViewerContext>().ItemInserted -= value; }
        }

        /// <summary>
        /// Event that is raised when an item is removed</summary>
        public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved
        {
			add { this.As<StatsViewerContext>().ItemRemoved += value; }
			remove { this.As<StatsViewerContext>().ItemRemoved -= value; }
        }

        /// <summary>
        /// Event that is raised when an item is changed</summary>
        public event EventHandler<ItemChangedEventArgs<object>> ItemChanged
        {
			add { this.As<StatsViewerContext>().ItemChanged += value; }
			remove { this.As<StatsViewerContext>().ItemChanged -= value; }
        }

        /// <summary>
        /// Event that is raised when the collection has been reloaded</summary>
        public event EventHandler Reloaded
        {
			add { this.As<StatsViewerContext>().Reloaded += value; }
			remove { this.As<StatsViewerContext>().Reloaded -= value; }
        }

        #endregion

		private SessionControl m_sessionControl;
        private SessionRenderer m_renderer;
		//private SplitManipulator m_splitManipulator;
    }
}
