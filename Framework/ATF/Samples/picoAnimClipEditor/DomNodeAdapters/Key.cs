//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.


using Sce.Atf.Dom;
using Sce.Atf.Controls.Timelines;

using pico.Timeline;

#pragma warning disable 0649 // suppress "field never set" warning

namespace picoAnimClipEditor.DomNodeAdapters
{
    /// <summary>
    /// Adapts DomNode to a Key</summary>
	public abstract class Key : BaseEvent, IKey, ITimelineValidationCallback
    {
        #region IKey Members

        /// <summary>
        /// Gets the track that contains the key</summary>
        public ITrack Track
        {
            get { return GetParentAs<ITrack>(); }
        }

        #endregion

		public virtual bool CanParentTo( DomNode parent )
		{
			return true;
		}

		public virtual bool Validate( DomNode parent )
		{
			return true;
		}
	}
}




