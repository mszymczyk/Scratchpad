using System.Drawing;

namespace misz.StatsViewer
{
    /// <summary>
    /// Interface for Events, base interface for IInterval, IKey and IMarker</summary>
    public interface IStatValue
    {
        /// <summary>
        /// Gets and sets the event's start time</summary>
        float Time { get; set; }

        /// <summary>
        /// Gets and sets the event's length (duration)</summary>
        float Value { get; set; }

		///// <summary>
		///// Gets and sets the event's color</summary>
		//Color Color { get; set; }

		///// <summary>
		///// Gets and sets the event's name</summary>
		//string Name { get; set; }
    }
}

