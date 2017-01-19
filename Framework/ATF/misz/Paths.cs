using System;
using System.IO;
using System.Text;

namespace misz
{
    public class Paths
    {
        public static void SetupPaths( string dataRootDir, string codeRootDir )
        {
            DATA_ROOT_DIR = FixDriveLetter( Path.GetFullPath( dataRootDir + "\\" ) );
            DATA_ROOT_DIR_Uri = new Uri( Uri.UnescapeDataString( DATA_ROOT_DIR ), UriKind.Absolute );

            //DATA_ROOT_DIR_data = DATA_ROOT_DIR + "data\\";
            //DATA_ROOT_DIR_data_Uri = new Uri( Uri.UnescapeDataString( DATA_ROOT_DIR_data ), UriKind.Absolute );

            CODE_ROOT_DIR = FixDriveLetter( Path.GetFullPath( codeRootDir + "\\" ) );
            CODE_ROOT_DIR_Uri = new Uri( Uri.UnescapeDataString( CODE_ROOT_DIR ), UriKind.Absolute );
        }

        /// <summary>
        /// Creates path relative to DATA_ROOT_DIR directory
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static string AbsoluteUriToDataPath( Uri uri )
        {
            return AbsolutePathToDataPath( uri.LocalPath );
        }

        /// <summary>
        /// Creates path relative to DATA_ROOT_DIR directory
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static Uri DataUriToAbsoluteUri( Uri uri )
        {
            return new Uri( DATA_ROOT_DIR_Uri, uri );
        }

        /// <summary>
        /// Takes absolute path and creates path relative to DATA_ROOT_DIR directory
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string AbsolutePathToDataPath( string path )
        {
            string fullPath = Path.GetFullPath( path );
            if ( fullPath.IndexOf( DATA_ROOT_DIR ) == -1 )
                throw new ArgumentException( string.Format( "Provided path {0} is not relative to data dir {1}", path, DATA_ROOT_DIR ) );

            return fullPath.Substring( DATA_ROOT_DIR.Length );
        }

        /// <summary>
        /// Takes absolute path and creates path relative to DATA_ROOT_DIR directory
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string AbsolutePathToDataPathTry( string path )
        {
            string fullPath = Path.GetFullPath( path );
            if ( fullPath.IndexOf( DATA_ROOT_DIR ) == -1 )
                return null;

            return fullPath.Substring( DATA_ROOT_DIR.Length );
        }

        /// <summary>
        /// Takes 'data' local path and produces absolute path
        /// </summary>
        /// <param name="localPath"></param>
        /// <returns></returns>
        public static string DataPathToAbsolutePath( string localPath )
        {
            return DATA_ROOT_DIR + localPath;
        }

        /// <summary>
        /// Takes absolute path and creates path relative to CODE_ROOT_DIR directory
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string AbsolutePathToCodePath( string path )
        {
            string fullPath = Path.GetFullPath( path );
            if ( fullPath.IndexOf( CODE_ROOT_DIR ) == -1 )
                throw new ArgumentException( string.Format( "Provided path {0} is not relative to code dir {1}", path, CODE_ROOT_DIR ) );

            return fullPath.Substring( CODE_ROOT_DIR.Length );
        }

        /// <summary>
        /// Takes absolute path and creates path relative to CODE_ROOT_DIR directory
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string AbsolutePathToCodePathTry( string path )
        {
            string fullPath = Path.GetFullPath( path );
            if ( fullPath.IndexOf( CODE_ROOT_DIR ) == -1 )
                return null;

            return fullPath.Substring( CODE_ROOT_DIR.Length );
        }

        /// <summary>
        /// Takes 'code' local path and produces absolute path
        /// </summary>
        /// <param name="localPath"></param>
        /// <returns></returns>
        public static string CodePathToAbsolutePath( string localPath )
        {
            return CODE_ROOT_DIR + localPath;
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

            return dst.ToString();
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
        public static int CompareFileDates( string file1, string file2 )
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

        public static string FixDriveLetter( string path )
        {
            if ( path.Length == 0 )
                return path;

            char[] ca = path.ToCharArray();
            ca[0] = char.ToUpper( ca[0] );
            string newPath = new string( ca );
            return newPath;
        }

        public static string DATA_ROOT_DIR { get; private set; }
        public static Uri DATA_ROOT_DIR_Uri { get; private set; }

        //public static string DATA_ROOT_DIR_data { get; private set; }
        //public static Uri DATA_ROOT_DIR_data_Uri { get; private set; }

        public static string CODE_ROOT_DIR { get; private set; }
        public static Uri CODE_ROOT_DIR_Uri { get; private set; }
    }
}
