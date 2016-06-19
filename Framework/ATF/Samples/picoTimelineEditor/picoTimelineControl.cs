//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Dom;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.Timelines;
using Sce.Atf.Controls.Timelines.Direct2D;

namespace picoTimelineEditor
{
	/// <summary>
	/// Timeline document, as a DOM hierarchy and identified by an URI. Each TimelineControl has one
	/// or more TimelineDocuments.</summary>
	public class picoTimelineControl : D2dTimelineControl
	{
		/// <summary>
		/// Gets or sets the cursor step, in milliseconds</summary>
		public static float CursorStep
		{
			get { return s_cursorStep; }
			set { s_cursorStep = MathUtil.Clamp<float>(value, 1.0f, 1000.0f); }
		}

		private static float s_cursorStep = 100.0f;

		public picoTimelineControl(
			ITimelineDocument timelineDocument,
			D2dTimelineRenderer timelineRenderer,
			TimelineConstraints timelineConstraints,
			bool createDefaultManipulators )
			: base(timelineDocument, timelineRenderer, timelineConstraints, createDefaultManipulators)
		{
		}

		/// <summary>
		/// Constrains one world coordinate of a timeline object that might be moved or resized</summary>
		/// <param name="offset">Timeline world coordinate</param>
		/// <returns>Constrained frame offset</returns>
		/// <remarks>Default constrains offsets to integral values, forcing all move and resize
		/// operations to maintain integral start and length properties.</remarks>
		public override float ConstrainFrameOffset( float offset )
		{
			//return (float)Math.Round(offset);
			return (float) MathUtil.Snap( offset, CursorStep );
		}


		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Control.MouseUp"></see> event and performs custom actions</summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"></see> that contains the event data</param>
		protected override void OnMouseUp( MouseEventArgs e )
		{
			base.OnMouseUp( e );

			ISelectionContext selectionContext = this.TimelineDocument.As<ISelectionContext>();
			object lastSelected = selectionContext.LastSelected;
			if ( lastSelected == null )
			{
				// if nothing was selected by mouse click
				// then select timeline itself
				//
				selectionContext.Set( this.TimelineDocument );
			}

		}

	}
}
