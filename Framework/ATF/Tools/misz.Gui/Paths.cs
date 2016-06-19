using System;
using System.IO;
using System.Text;

namespace misz.Gui
{
	public class Paths
	{
		static Paths()
		{
			string SCRATCHPAD_DIR_env = Environment.GetEnvironmentVariable( "SCRATCHPAD_DIR" );
            SCRATCHPAD_DIR = FixDriveLetter( Path.GetFullPath( SCRATCHPAD_DIR_env + "\\" ) );

            SCRATCHPAD_DIR_data = SCRATCHPAD_DIR + "data\\";
            SCRATCHPAD_DIR_dataWin = SCRATCHPAD_DIR + "dataWin\\";

            SCRATCHPAD_DIR_Uri = new Uri( Uri.UnescapeDataString( SCRATCHPAD_DIR ), UriKind.Absolute );
        }

		public static string UriToScratchpadPath( Uri uri )
		{
			string path = uri.LocalPath;
			return PathToScratchpadPath( path );
		}

		public static string PathToScratchpadPath( string path )
		{
			string fullPath = Path.GetFullPath( path );
			int index = fullPath.IndexOf( SCRATCHPAD_DIR_data );
			if ( index == -1 )
				return "";

			string localPath = fullPath.Substring( SCRATCHPAD_DIR_data.Length );
			return localPath;
		}

		public static string LocalPathToScratchpadAbsolutePath( string localPath )
		{
			string absPath = SCRATCHPAD_DIR_data + localPath;
			return absPath;
		}

		public static string CanonicalizePathSimple( string srcPath )
		{
			StringBuilder dst = new StringBuilder();

			char lastC = '\\'; // this will remove leading '\' or '/'
			for ( int i = 0; i < srcPath.Length; ++i )
			{
				char c = srcPath[i];
				if ( c == '/' )
					c = '\\';

				if ( lastC == '\\' && c == '\\' )
				{
					// skip
				}
				else
				{
					lastC = c;
					dst.Append( c );
				}
			}


			string str = dst.ToString();
			return str;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="?"></param>
		/// <returns>
		/// 1 if file1 is newer than file2
		/// 0 when files have same dates
		/// -1 if file2 is newer than file1
		///	-1 is also returned when one or both files don't exist
		/// </returns>
		public static int compareFileDates( string file1, string file2 )
		{
			DateTime dt1;
			if ( File.Exists( file1 ) )
				dt1 = File.GetLastWriteTime( file1 );
			else
				return -1;

			DateTime dt2;
			if ( File.Exists( file2 ) )
				dt2 = File.GetLastWriteTime( file2 );
			else
				return -1;

			if ( dt1 > dt2 )
				return 1;
			else if ( dt2 > dt1 )
				return -1;
			else
				return 0;
		}

        public static string MakePathRelativeToScratchpad( Uri ur )
        {
            if ( ur.IsAbsoluteUri )
            {
                // use resource root to make it relative
                ur = SCRATCHPAD_DIR_Uri.MakeRelativeUri( ur );

                ur = new Uri( Uri.UnescapeDataString( ur.ToString() ), UriKind.Relative );
            }

            string localUri = ur.ToString();
            localUri = CanonicalizePathSimple( localUri );
            return localUri;
        }

        private static string FixDriveLetter( string path )
		{
			if ( path.Length == 0 )
				return path;

			char[] ca = path.ToCharArray();
			ca[0] = char.ToUpper( ca[0] );
			string newPath = new string( ca );
			return newPath;
		}

		public static readonly string SCRATCHPAD_DIR;
		public static readonly string SCRATCHPAD_DIR_data;
		public static readonly string SCRATCHPAD_DIR_dataWin;
        public static readonly Uri SCRATCHPAD_DIR_Uri;
    }
}
