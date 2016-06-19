using System;
using System.IO;
using System.Text;

namespace pico
{
	public class Paths
	{
		static Paths()
		{
			string PICO_ROOT_env = Environment.GetEnvironmentVariable( "PICO_ROOT" );
			PICO_ROOT = FixDriveLetter( Path.GetFullPath( PICO_ROOT_env + "\\" ) );
			string PICO_DEMO_env = Environment.GetEnvironmentVariable( "PICO_DEMO" );
			PICO_DEMO = FixDriveLetter( Path.GetFullPath( PICO_DEMO_env + "\\" ) );

			PICO_DEMO_data = PICO_DEMO + "data\\";
			PICO_DEMO_dataWin = PICO_DEMO + "dataWin\\";
			texconv_exe = PICO_ROOT + "bin64\\texconv.exe";
			nvcompress_exe = PICO_ROOT + "bin64\\nvcompress.exe";
			picoServicesLauncher_exe = PICO_ROOT + "bin\\picoServicesLauncher.exe";
			picoHub_exe = PICO_ROOT + "bin64\\picoHub.exe";
			picoLogOutput_exe = PICO_ROOT + "bin\\picoLogOutput.exe";

			//string sdkDirEnvVar = Environment.GetEnvironmentVariable( "SCE_ORBIS_SDK_DIR" );
			//if ( sdkDirEnvVar != null && sdkDirEnvVar.Length > 0 )
			//{
			//	PICO_DEMO_dataPS4 = PICO_DEMO + "dataPS4\\";

			//	SCE_ORBIS_SDK_DIR = Path.GetFullPath( sdkDirEnvVar + "\\" );
			//	orbis_image2gnf_exe = SCE_ORBIS_SDK_DIR + "host_tools\\bin\\orbis-image2gnf.exe";

			//	HAS_PS4_SDK_INSTALLED = true;
			//}
		}

		public static string UriToPicoDemoPath( Uri uri )
		{
			string path = uri.LocalPath;
			return PathToPicoDemoPath( path );
		}

		public static string PathToPicoDemoPath( string path )
		{
			string fullPath = Path.GetFullPath( path );
			int index = fullPath.IndexOf( PICO_DEMO_data );
			if ( index == -1 )
				return "";

			string localPath = fullPath.Substring( PICO_DEMO_data.Length );
			return localPath;
		}

		public static string LocalPathToPicoDataAbsolutePath( string localPath )
		{
			string absPath = PICO_DEMO_data + localPath;
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

		private static string FixDriveLetter( string path )
		{
			if ( path.Length == 0 )
				return path;

			char[] ca = path.ToCharArray();
			ca[0] = char.ToUpper( ca[0] );
			string newPath = new string( ca );
			return newPath;
		}

		public static readonly string PICO_ROOT;
		public static readonly string PICO_DEMO;
		public static readonly string PICO_DEMO_data;
		public static readonly string PICO_DEMO_dataWin;
		public static readonly string texconv_exe;
		public static readonly string nvcompress_exe;
		public static readonly string picoServicesLauncher_exe;
		public static readonly string picoHub_exe;
		public static readonly string picoLogOutput_exe;

		//// ps4 stuff
		////
		//public static readonly bool HAS_PS4_SDK_INSTALLED;

		//public static readonly string PICO_DEMO_dataPS4;
		//public static readonly string SCE_ORBIS_SDK_DIR;
		//public static readonly string orbis_image2gnf_exe;
	}
}
