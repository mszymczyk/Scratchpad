//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Drawing;

using Sce.Atf;
using Sce.Atf.Dom;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.Timelines;
using Sce.Atf.Controls.CurveEditing;
using Sce.Atf.VectorMath;

using pico.Timeline;

#pragma warning disable 0649 // suppress "field never set" warning

namespace picoTimelineEditor.DomNodeAdapters
{
    /// <summary>
    /// Adapts DomNode to an Interval</summary>
	public class IntervalText : Interval, ITimelineObjectCreator, ITimelineValidationCallback
    {
		/// <summary>
		/// Gets and sets the text node name</summary>
		public string TextNodeName
		{
			get { return (string)DomNode.GetAttribute( Schema.intervalTextType.textNodeNameAttribute ); }
			set { DomNode.SetAttribute( Schema.intervalTextType.textNodeNameAttribute, value ); }
		}
		/// <summary>
		/// Gets and sets text tag</summary>
		public string TextTag
		{
			get { return (string)DomNode.GetAttribute( Schema.intervalTextType.textTagAttribute ); }
			set { DomNode.SetAttribute( Schema.intervalTextType.textTagAttribute, value ); }
		}

		#region ITimelineObjectCreator Members
		ITimelineObject ITimelineObjectCreator.Create()
		{
			DomNodeType nodeType = Schema.intervalTextType.Type;
			DomNode dn = new DomNode( nodeType );

			NodeTypePaletteItem paletteItem = nodeType.GetTag<NodeTypePaletteItem>();
			if (paletteItem != null)
				dn.SetAttribute( nodeType.IdAttribute, paletteItem.Name );
			else
				dn.SetAttribute( nodeType.IdAttribute, "Text" );

			return dn.Cast<ITimelineObject>();
		}
		#endregion

		public bool CanParentTo( DomNode parent )
		{
			return ValidateImpl( parent, 0 );
		}

		public bool Validate( DomNode parent )
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





