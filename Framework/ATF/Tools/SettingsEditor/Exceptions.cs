//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.IO;

namespace SettingsEditor
{
	public class InvalidSettingsPathException : Exception
	{
		public InvalidSettingsPathException( Uri uri )
			: base( string.Format( "Path {0} is not relative to SettingsRoot ({1})", uri.LocalPath, Globals.DataDirectory ) )
		{
		}
	}
}
