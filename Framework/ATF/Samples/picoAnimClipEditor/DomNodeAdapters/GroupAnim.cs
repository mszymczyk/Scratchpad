//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

using Sce.Atf.Adaptation;
using Sce.Atf.Controls.Timelines;
using Sce.Atf.Dom;

using pico.Timeline;

#pragma warning disable 0649 // suppress "field never set" warning

namespace picoAnimClipEditor.DomNodeAdapters
{
    /// <summary>
    /// Adapts DomNode to a group of anim tracks</summary>
	public class GroupAnim : DomNodeAdapter, ITimelineValidationCallback
    {
		public virtual bool CanParentTo( DomNode parent )
		{
			return true;
		}

		public virtual bool Validate( DomNode parent )
		{
			return true;
		}

		/// <summary>
		/// Creates a new group of given type</summary>
		/// <returns>New group</returns>
		public ITrack CreateTrack( DomNodeType type )
		{
			Track track = new DomNode( type ).As<Track>();

			NodeTypePaletteItem paletteItem = type.GetTag<NodeTypePaletteItem>();
			AttributeInfo idAttribute = type.IdAttribute;
			if ( paletteItem != null &&
                idAttribute != null )
			{
				track.DomNode.SetAttribute( idAttribute, paletteItem.Name );
			}

			return track;
		}
    }
}




