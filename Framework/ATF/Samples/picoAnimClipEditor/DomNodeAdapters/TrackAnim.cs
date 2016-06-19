//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

using Sce.Atf.Adaptation;
using Sce.Atf.Controls.Timelines;
using Sce.Atf.Dom;

using pico;
using pico.Timeline;

namespace picoAnimClipEditor.DomNodeAdapters
{
	/// <summary>
	/// Adapts DomNode to a Track</summary>
	public class TrackAnim : DomNodeAdapter, ITimelineValidationCallback, IFileChangedNotification
	{
		/// <summary>
		/// Returns the Name property. Useful for debugging purposes.</summary>
		/// <returns>Name property</returns>
		public override string ToString()
		{
			Track track = DomNode.Cast<Track>();
			return track.Name;
		}

		public virtual bool CanParentTo( DomNode parent )
		{
			return true;
		}

		public virtual bool Validate( DomNode parent )
		{
			return true;
		}

		public string AnimFile
		{
			get { return m_animFile; }
			set
			{
				if ( value != m_animFile )
				{
					m_animFile = value;
					if ( m_animFile != null && m_animFile.Length > 0 )
						m_afh = pico.Anim.AnimFileHeader.ReadFromFile2( m_animFile );
				}
			}
		}


		#region IFileChangedNotification

		void IFileChangedNotification.OnFileChanged( System.IO.FileSystemEventArgs e, string ext, string picoDemoPath )
		{
			if ( ext != ".anim" )
				return;

			if ( picoDemoPath != m_animFile )
				return;

			ReloadAnimFile();
		}

		#endregion

		public void ReloadAnimFile()
		{
			if ( m_animFile != null && m_animFile.Length > 0 )
			{
				pico.Anim.AnimFileHeader afh = pico.Anim.AnimFileHeader.ReadFromFile2( m_animFile );
				if ( afh != null )
				{
					m_afh = afh;

					Track track = this.As<Track>();
					foreach( IInterval interval in track.Intervals )
					{
						interval.Length = m_afh.durationMilliseconds;
					}
				}
			}
		}

		/// <summary>
		/// Gets animation's duration in milliseconds
		/// </summary>
		[pico.Controls.PropertyEditing.CutomPropertyEditingAttribute( true, "AnimDuration", "AnimFileInfo", "Animation duration in milliseconds" )]
		public float AnimDuration
		{
			get { return (m_afh != null) ? m_afh.durationMilliseconds : 0; }
		}

		private string m_animFile;
		private pico.Anim.AnimFileHeader m_afh;
	}
}



