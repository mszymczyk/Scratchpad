using System;
using System.IO;
using System.Text;

namespace misz
{
    public class Paths
    {
        public static void SetupPaths( string dataRootDir, string codeRootDir, string materialTemplatePath )
        {
            DATA_ROOT_DIR = FixDriveLetter( Path.GetFullPath( dataRootDir + "\\" ) );
            DATA_ROOT_DIR_Uri = new Uri( Uri.UnescapeDataString( DATA_ROOT_DIR ), UriKind.Absolute );

            DATA_ROOT_DIR_data = DATA_ROOT_DIR + "data\\";
            DATA_ROOT_DIR_data_Uri = new Uri( Uri.UnescapeDataString( DATA_ROOT_DIR_data ), UriKind.Absolute );

            MATERIAL_TEMPLATE_PATH_ABSOLUTE = FixDriveLetter( Path.GetFullPath( materialTemplatePath ) ); ;
        }

        /// <summary>
        /// Creates path relative to DATA_ROOT_DIR_data directory
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static string UriToDataPath( Uri uri )
        {
            string path = uri.LocalPath;
            return PathToDataPath( path );
        }

        /// <summary>
        /// Creates path relative to DATA_ROOT_DIR_data directory
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static Uri DataUriToAbsoluteUri( Uri uri )
        {
            return new Uri( DATA_ROOT_DIR_data_Uri, uri );
        }

        /// <summary>
        /// Takes absolute path and creates path relative to DATA_ROOT_DIR_data directory
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string PathToDataPath( string path )
        {
            string fullPath = Path.GetFullPath( path );
            int index = fullPath.IndexOf( DATA_ROOT_DIR_data );
            if ( index == -1 )
                return "";

            string localPath = fullPath.Substring( DATA_ROOT_DIR_data.Length );
            return localPath;
        }

        /// <summary>
        /// Takes 'data' local path and produces absolute path
        /// </summary>
        /// <param name="localPath"></param>
        /// <returns></returns>
        public static string LocalPathToDataAbsolutePath( string localPath )
        {
            string absPath = DATA_ROOT_DIR_data + localPath;
            return absPath;
        }

        public static string CanonicalizePathSimple(string srcPath)
        {
            StringBuilder dst = new StringBuilder();

            char lastC = '\\'; // this will remove leading '\' or '/'
            for (int i = 0; i < srcPath.Length; ++i)
            {
                char c = srcPath[i];
                if (c == '/')
                    c = '\\';

                if (lastC == '\\' && c == '\\')
                {
                    // skip
                }
                else
                {
                    lastC = c;
                    dst.Append(c);
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
        public static int CompareFileDates(string file1, string file2)
        {
            DateTime dt1;
            if (File.Exists(file1))
                dt1 = File.GetLastWriteTime(file1);
            else
                return -1;

            DateTime dt2;
            if (File.Exists(file2))
                dt2 = File.GetLastWriteTime(file2);
            else
                return -1;

            if (dt1 > dt2)
                return 1;
            else if (dt2 > dt1)
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

        public static string DATA_ROOT_DIR { get; private set; }
        public static Uri DATA_ROOT_DIR_Uri { get; private set; }

        public static string DATA_ROOT_DIR_data { get; private set; }
        public static Uri DATA_ROOT_DIR_data_Uri { get; private set; }

        public static string MATERIAL_TEMPLATE_PATH_ABSOLUTE { get; private set; }
    }
}
