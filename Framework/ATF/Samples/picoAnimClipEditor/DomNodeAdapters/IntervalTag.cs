using System.Drawing;

using Sce.Atf.Dom;
using Sce.Atf.Controls.Timelines;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Adaptation;
using pico.Controls.PropertyEditing;
using System.ComponentModel;

using pico.Timeline;

namespace picoAnimClipEditor.DomNodeAdapters
{
    /// <summary>
    /// Adapts DomNode to a Key</summary>
	public class IntervalTag : Interval, ITimelineValidationCallback, ITimelineObjectCreator
    {
		#region IEvent Members

		///// <summary>
		///// Gets and sets the event's color</summary>
		//public override Color Color
		//{
		//	get { return Color.Aqua; }
		//	set { }
		//}

		/// <summary>
		/// Gets and sets the event's color</summary>
		public override Color Color
		{
			get { return Color.FromArgb( (int) DomNode.GetAttribute( Schema.intervalTagType.colorAttribute ) ); }
			set { DomNode.SetAttribute( Schema.intervalTagType.colorAttribute, value.ToArgb() ); }
		}

		#endregion

		#region ITimelineObjectCreator Members
		ITimelineObject ITimelineObjectCreator.Create()
		{
			DomNodeType type = Schema.intervalTagType.Type;
			DomNode dn = new DomNode( type );
			IntervalTag i = dn.As<IntervalTag>();
			
			NodeTypePaletteItem paletteItem = type.GetTag<NodeTypePaletteItem>();
			AttributeInfo idAttribute = type.IdAttribute;
			if (paletteItem != null &&
                idAttribute != null)
			{
				i.DomNode.SetAttribute( idAttribute, paletteItem.Name );
			}

			return i;
		}
		#endregion

		public override bool CanParentTo( DomNode parent )
		{
			return ValidateImpl( parent, 0 );
		}

		public override bool Validate( DomNode parent )
		{
			return ValidateImpl( parent, 1 );
		}

		private bool ValidateImpl( DomNode parent, int validating )
		{
			if ( parent.Type != Schema.trackType.Type )
				return false;

			return true;
		}
	}
}




