//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.IO;

namespace SettingsEditor
{
	public static class Globals
	{
		public static void ParseCommandLine( string[] args )
		{
			SetCodeDir( @"H:\_git\scratchpad\code\shadows\" );
			SetDataDir( @"H:\_git\scratchpad\data\" );
		}

		public static void SetCodeDir( string path )
		{
			if (Directory.Exists( path ))
				CodeDir = FixDriveLetter( System.IO.Path.GetFullPath( path + Path.DirectorySeparatorChar ) );
		}

		public static void SetDataDir( string path )
		{
			if ( Directory.Exists(path) )
				DataDir = FixDriveLetter( System.IO.Path.GetFullPath( path + Path.DirectorySeparatorChar ) );
		}

		// http://stackoverflow.com/questions/703281/getting-path-relative-to-the-current-working-directory
		static string GetRelativePath(string filespec, string folder)
		{
			Uri pathUri = new Uri(filespec);
			// Folders must end in a slash
			if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
			{
				folder += Path.DirectorySeparatorChar;
			}
			Uri folderUri = new Uri(folder);
			return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
		}

		public static string GetPathRelativeToCode( string path )
		{
			string fullPath = Path.GetFullPath( path );
			int index = fullPath.IndexOf( CodeDir );
			if (index == -1)
				return string.Empty;

			string localPath = fullPath.Substring( CodeDir.Length );
			return localPath;
		}

		public static string GetCodeFullPath( string localPath )
		{
			return Path.GetFullPath( CodeDir + localPath );
		}


		public static string GetPathRelativeToData( string path )
		{
			string fullPath = Path.GetFullPath( path );
			int index = fullPath.IndexOf( DataDir );
			if (index == -1)
				return string.Empty;

			string localPath = fullPath.Substring( DataDir.Length );
			return localPath;
		}

		public static string GetDataFullPath( string localPath )
		{
			return Path.GetFullPath( DataDir + localPath );
		}


		/// <summary>
		/// Gets or sets the root source code directory for finding settings description files
		/// </summary>
		public static string CodeDirectory
		{
			get
			{
				return CodeDirUser;
			}
			set
			{
				CodeDirUser = value;

				string path = TryExtractPathFromEnvVariable( value );
				if (path != null)
				{
					SetCodeDir( path );
					return;
				}

				SetCodeDir( value );
			}
		}

		/// <summary>
		/// Gets or sets the root data (or assets) directory for finding setting files
		/// </summary>
		public static string DataDirectory
		{
			get
			{
				return DataDirUser;
			}
			set
			{
				DataDirUser = value;

				string path = TryExtractPathFromEnvVariable( value );
				if (path != null)
				{
					SetDataDir( path );
					return;
				}

				SetDataDir( value );
			}
		}

		private static string TryExtractPathFromEnvVariable( string value )
		{
			if (value != null && value.Length > 2)
			{
				int firstPercent = value.IndexOf( '%' );
				int secondPercent = value.IndexOf( '%', firstPercent + 1 );
				if (firstPercent != -1 && secondPercent != -1)
				{
					// environment variable
					//
					string envVarName = value.Substring( firstPercent + 1, secondPercent - firstPercent - 1 );
					string envVarValue = Environment.GetEnvironmentVariable( envVarName );
					if (!string.IsNullOrEmpty( envVarValue ))
					{
						string path = value.Substring( 0, firstPercent ) + envVarValue + value.Substring( secondPercent + 1 );
						return path;
					}
				}
			}

			return null;
		}

		//public static string PathToPicoCodePath( string path )
		//{
		//	string fullPath = Path.GetFullPath( path );
		//	int index = fullPath.IndexOf( PICO_ROOT );
		//	if (index == -1)
		//		return "";

		//	string localPath = fullPath.Substring( PICO_ROOT.Length );
		//	return localPath;
		//}

		private static string CodeDir = Path.GetFullPath( Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar );
		private static string DataDir = Path.GetFullPath( Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar );
		private static string CodeDirUser;
		private static string DataDirUser;

		private static string FixDriveLetter( string path )
		{
			if (path.Length == 0)
				return path;

			char[] ca = path.ToCharArray();
			ca[0] = char.ToUpper( ca[0] );
			string newPath = new string( ca );
			return newPath;
		}
		//static string PICO_ROOT = FixDriveLetter( Path.GetFullPath( PICO_ROOT_env + "\\" ) );
	}
}
