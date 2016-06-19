//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.IO;

namespace pico
{
	/// <summary>
	/// Interface used for notifying nodes about file changes
	/// </summary>
	public interface IFileChangedNotification
	{
		void OnFileChanged( FileSystemEventArgs e, string ext, string picoDemoPath );
	}
}
