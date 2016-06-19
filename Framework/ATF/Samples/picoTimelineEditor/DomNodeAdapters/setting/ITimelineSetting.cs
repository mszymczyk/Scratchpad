using Sce.Atf.Dom;

namespace picoTimelineEditor.DomNodeAdapters
{
    /// <summary>
    /// Interface for Events, base interface for IInterval, IKey and IMarker</summary>
    public abstract class TimelineSetting : DomNodeAdapter
    {
		///// <summary>
		///// Gets and sets the event's name</summary>
		//string Name { get; set; }

		/// <summary>
		/// Gets setting's ui display name
		/// </summary>
		public abstract string Label { get; }
    }
}

