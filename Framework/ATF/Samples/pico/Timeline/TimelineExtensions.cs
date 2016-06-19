using System;
using System.IO;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.Timelines;

namespace pico.Timeline
{
	/// <summary>
	/// Useful methods for operating on ITimeline
	/// </summary>
	public static class TimelineExtensions
	{
		/// <summary>
		/// Clears the selection</summary>
		/// <param name="selectionContext">Selection context</param>
		public static void NotifyFileChange( this ITimeline timeline, FileSystemEventArgs e, string ext, string picoDemoPath )
		{
			foreach( IGroup group in timeline.Groups )
			{
				IFileChangedNotification fcnGroup = group.As<IFileChangedNotification>();
				if ( fcnGroup != null )
					fcnGroup.OnFileChanged( e, ext, picoDemoPath );

				foreach( ITrack track in group.Tracks )
				{
					IFileChangedNotification fcnTrack = track.As<IFileChangedNotification>();
					if ( fcnTrack != null )
						fcnTrack.OnFileChanged( e, ext, picoDemoPath );

					foreach( IInterval interval in track.Intervals )
					{
						IFileChangedNotification fcnInterval = interval.As<IFileChangedNotification>();
						if ( fcnInterval != null )
							fcnInterval.OnFileChanged( e, ext, picoDemoPath );
					}

					foreach ( IKey key in track.Keys )
					{
						IFileChangedNotification fcnKey = key.As<IFileChangedNotification>();
						if ( fcnKey != null )
							fcnKey.OnFileChanged( e, ext, picoDemoPath );
					}
				}
			}

			foreach( IMarker marker in timeline.Markers )
			{
				IFileChangedNotification fcnMarker = marker.As<IFileChangedNotification>();
				if ( fcnMarker != null )
					fcnMarker.OnFileChanged( e, ext, picoDemoPath );
			}
		}
	}
}
