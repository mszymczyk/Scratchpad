#include "Util_pch.h"
#include "FileIO.h"
#include <direct.h>

#include "Shlwapi.h"

#if defined(_MSC_VER) && defined(_DEBUG)
#define new _DEBUG_NEW
#endif

namespace spad
{
	// Returns true if a file exits
	bool FileExists( const char* filePath )
	{
		DWORD fileAttr = GetFileAttributes( filePath );
		if ( fileAttr == INVALID_FILE_ATTRIBUTES )
			return false;

		return true;
	}

	//u64 getFileTimeStamp( const char* fileName, bool& fileFound )
	//{
	//	FILETIME fileTime;
	//	HANDLE file = CreateFile( fileName, GENERIC_READ, FILE_SHARE_READ, 0, OPEN_EXISTING, 0, 0 );
	//	if ( file == INVALID_HANDLE_VALUE )
	//	{
	//		fileFound = false;
	//		return 0;
	//	}

	//	GetFileTime( file, 0, 0, &fileTime );

	//	CloseHandle( file );

	//	fileFound = true;
	//	ULARGE_INTEGER li;
	//	li.LowPart = fileTime.dwLowDateTime;
	//	li.HighPart = fileTime.dwHighDateTime;
	//	return li.QuadPart;
	//}


	bool DirectoryExists( const char* filePath )
	{
		DWORD fileAttr = GetFileAttributes( filePath );
		if ( fileAttr == INVALID_FILE_ATTRIBUTES )
			return false;

		if ( fileAttr & FILE_ATTRIBUTE_DIRECTORY )
			return true;

		return false;
	}

	// Gets the last written timestamp of the file
	u64 GetFileTimestamp( const wchar_t* filePath, bool& fileFound )
	{
		WIN32_FILE_ATTRIBUTE_DATA attributes;
		BOOL bres = GetFileAttributesExW( filePath, GetFileExInfoStandard, &attributes );
		if ( bres )
		{
			fileFound = true;
			return attributes.ftLastWriteTime.dwLowDateTime | ( u64( attributes.ftLastWriteTime.dwHighDateTime ) << 32 );
		}
		else
		{
			fileFound = false;
			return 0;
		}
	}

	u64 GetFileTimestamp( const char* filePath, bool& fileFound )
	{
		WIN32_FILE_ATTRIBUTE_DATA attributes;
		BOOL bres = GetFileAttributesEx( filePath, GetFileExInfoStandard, &attributes );
		if ( bres )
		{
			fileFound = true;
			return attributes.ftLastWriteTime.dwLowDateTime | ( u64( attributes.ftLastWriteTime.dwHighDateTime ) << 32 );
		}
		else
		{
			fileFound = false;
			return 0;
		}
	}

	void CreateDirectoryRecursive( std::string file )
	{
		if ( file.empty() )
			return;

		std::string::size_type slash = file.find( '/' );
		std::string::size_type slash2 = file.find( '\\' );
		slash = std::min( slash, slash2 );

		std::string path;
		while ( slash != std::string::npos )
		{
			std::string fileWithoutSlash = file.substr( 0, slash );
			path += fileWithoutSlash;
			_mkdir( path.c_str() );
			file = file.substr( slash + 1 );
			slash = file.find( '/' );
			slash2 = file.find( '\\' );
			slash = std::min( slash, slash2 );
			path += "/";
		}
	}

	void CreateDirectoryRecursive( std::string root, std::string file )
	{
		if ( file.empty() )
			return;

		AppendBackslashToDirectoryName( root );

		std::string::size_type slash = file.find( '/' );
		std::string::size_type slash2 = file.find( '\\' );
		slash = std::min( slash, slash2 );

		std::string path;
		while ( slash != std::string::npos )
		{
			std::string fileWithoutSlash = file.substr( 0, slash );
			path += fileWithoutSlash;
			_mkdir( ( root + path ).c_str() );
			file = file.substr( slash + 1 );
			slash = file.find( '/' );
			slash2 = file.find( '\\' );
			slash = std::min( slash, slash2 );
			path += "/";
		}
	}

	int RemoveDirectoryRecursively( const std::string& directory )
	{
		WIN32_FIND_DATA ffd;
		HANDLE hFind = INVALID_HANDLE_VALUE;
		DWORD dwError = 0;

		// check if dir length plus NULL character is less-equal to MAX_PATH
		if ( directory.length() > ( MAX_PATH - 1 ) )
			return -10;

		// Find the first file in the directory.

		hFind = FindFirstFile( ( directory + '*' ).c_str(), &ffd );

		if ( INVALID_HANDLE_VALUE == hFind )
			return -20;

		// List all the files in the directory with some info about them.

		do
		{
			if ( ( ffd.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY ) )
			{
				std::string newDir = ffd.cFileName;
				if ( newDir != "." && newDir != ".." )
				{
					std::string dir = directory + newDir + "\\";
					int ires = RemoveDirectoryRecursively( dir );
					if ( ires )
						return ires;
				}
			}
			else
			{
				std::string file = directory + ffd.cFileName;
				BOOL bres = DeleteFile( file.c_str() );
				if ( !bres )
					return -2;
			}

		} while ( FindNextFile( hFind, &ffd ) != 0 );

		dwError = GetLastError();
		if ( dwError != ERROR_NO_MORE_FILES )
			return -30;

		FindClose( hFind );

		BOOL bres = RemoveDirectory( directory.c_str() );
		if ( !bres )
			return -1;

		return 0;
	}

	int readTextFileSync( const char* filename, char** outBuffer, uint32_t* outBufSize )
	{
		FILE* f = fopen( filename, "rb" );
		if ( !f )
		{
			return -1;
		}

		fseek( f, 0, SEEK_END );
		size_t sizeInBytes = ftell( f );
		fseek( f, 0, SEEK_SET );

		char* buf = new char[ sizeInBytes + 1 ];
		if ( !buf )
			return -2;

		size_t readBytes = fread( buf, 1, sizeInBytes, f );
		(void)readBytes;
		SPAD_ASSERT( readBytes == sizeInBytes );
		int ret = fclose( f );
		(void)ret;
		SPAD_ASSERT( ret != EOF );

		buf[sizeInBytes] = 0;

		*outBuffer = buf;
		*outBufSize = static_cast<uint32_t>(sizeInBytes);

		return 0;
	}

	int WriteFileSync( const char* filename, const void* srcBuffer, size_t srcBufSize )
	{
		FILE* f = fopen( filename, "wb" );
		if ( !f )
		{
			logError( "Cannot open file '%s' for writing!", filename );
			return -1;
		}

		size_t written = fwrite( srcBuffer, 1, srcBufSize, f );
		(void)written;
		SPAD_ASSERT( written == srcBufSize );
		int ret = fclose( f );
		SPAD_ASSERT( ret != EOF );
		(void)ret;

		return 0;
	}

	std::string ReadTextFileAsString( const char* filename )
	{
		std::string ret;

		HANDLE fileHandle = CreateFile( filename, GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL );
		if ( fileHandle == INVALID_HANDLE_VALUE )
		{
			return ret;
		}

		LARGE_INTEGER fileSize;
		BOOL bres = GetFileSizeEx( fileHandle, &fileSize );
		if ( !bres )
		{
			CloseHandle( fileHandle );
			return ret;
		}

		if ( fileSize.QuadPart > 0xffffffff )
		{
			CloseHandle( fileHandle );
			return ret;
		}

		ret.resize( fileSize.QuadPart );

		DWORD bytesRead = 0;
		bres = ReadFile( fileHandle, &ret[0], static_cast<DWORD>( fileSize.QuadPart ), &bytesRead, NULL );
		if ( !bres )
		{
			CloseHandle( fileHandle );
			ret.clear();
			return ret;
		}

		CloseHandle( fileHandle );

		return ret;
	}

	std::vector<uint8_t> ReadFileAsVector( const char* filename )
	{
		std::vector<uint8_t> ret;

		HANDLE fileHandle = CreateFile( filename, GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL );
		if ( fileHandle == INVALID_HANDLE_VALUE )
		{
			return ret;
		}

		LARGE_INTEGER fileSize;
		BOOL bres = GetFileSizeEx( fileHandle, &fileSize );
		if ( !bres )
		{
			CloseHandle( fileHandle );
			return ret;
		}

		if ( fileSize.QuadPart > 0xffffffff )
		{
			CloseHandle( fileHandle );
			return ret;
		}

		ret.resize( fileSize.QuadPart );

		DWORD bytesRead = 0;
		bres = ReadFile( fileHandle, &ret[0], static_cast<DWORD>( fileSize.QuadPart ), &bytesRead, NULL );
		if ( !bres )
		{
			CloseHandle( fileHandle );
			ret.clear();
			return ret;
		}

		CloseHandle( fileHandle );

		return ret;
	}


	std::string GetAbsolutePath( const char* filePath )
	{
		std::string fileFullPath;
		fileFullPath.resize( 1024 );
		DWORD pathLen = GetFullPathName( filePath, (DWORD)fileFullPath.size(), &fileFullPath[0], nullptr );
		if ( pathLen == 0 )
		{
			logError( "GetFullPathName failed!" );
			return "";
		}
		else if ( pathLen > fileFullPath.size() )
		{
			fileFullPath.resize( pathLen );
			pathLen = GetFullPathName( filePath, (DWORD)fileFullPath.size(), &fileFullPath[0], nullptr );
			if ( pathLen == 0 )
			{
				logError( "GetFullPathName failed!" );
				return "";
			}
		}

		//std::string fileFullPathOut;
		//fileFullPathOut.resize( pathLen );
		//BOOL res = PathCanonicalize( &fileFullPathOut[0], fileFullPath.c_str() );
		//if ( !res )
		//{
		//	logError( "PathCanonicalize failed!" );
		//	return "";
		//}
		char dst[MAX_PATH] = { 0 };
		BOOL res = PathCanonicalize( dst, fileFullPath.c_str() );
		if ( !res )
		{
			logError( "PathCanonicalize failed!" );
			return "";
		}

		// fix drive letter to be upper case
		// windows supports only single letter drive names:)
		if ( dst[0] && dst[1] == ':' )
			dst[0] = (char)toupper( dst[0] );

		//HANDLE fileHandle = CreateFile( filePath, GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL );
		//if ( fileHandle == INVALID_HANDLE_VALUE )
		//{
		//	return "";
		//}

		//// FILE_NAME_NORMALIZED - Return the normalized drive name.This is the default.
		//// VOLUME_NAME_DOS - Return the path with the drive letter. This is the default.
		//DWORD flags = FILE_NAME_NORMALIZED | VOLUME_NAME_DOS;
		//DWORD pathLen = GetFinalPathNameByHandle( fileHandle, &fileFullPath[0], (DWORD)fileFullPath.size(), flags );
		//if ( pathLen == 0 )
		//{
		//	logError( "GetFinalPathNameByHandle failed!" );
		//	CloseHandle( fileHandle );
		//	return "";
		//}
		//else if ( pathLen > fileFullPath.size() )
		//{
		//	fileFullPath.resize( pathLen );
		//	pathLen = GetFinalPathNameByHandle( fileHandle, &fileFullPath[0], (DWORD)fileFullPath.size(), flags );
		//	CloseHandle( fileHandle );
		//	if ( pathLen == 0 )
		//	{
		//		logError( "GetFinalPathNameByHandle failed!" );
		//		return "";
		//	}
		//}

		return dst;
	}

	std::string GetAbsolutePath( const std::string& filePath )
	{
		return GetAbsolutePath( filePath.c_str() );
	}

	std::string GetDirectoryFromFilePath( const char* filePath )
	{
		std::string filePathCopy( filePath );
		return GetDirectoryFromFilePath( filePathCopy );
	}

	std::string GetDirectoryFromFilePath( const std::string& filePath )
	{
		size_t idx0 = filePath.rfind( '\\' );
		size_t idx1 = filePath.rfind( '/' );

		if ( idx0 != std::string::npos && idx1 != std::string::npos )
		{
			size_t idxMax = std::max( idx0, idx1 );
			return filePath.substr( 0, idxMax + 1 );
		}
		else if ( idx0 != std::string::npos )
		{
			return filePath.substr( 0, idx0 + 1 );
		}
		else if ( idx1 != std::string::npos )
		{
			return filePath.substr( 0, idx1 + 1 );
		}

		SPAD_ASSERT2( false, "filePath is invalid!" );
		return "";
	}

	std::string GetFileName( const char* filePath )
	{
		std::string filePathCopy( filePath );
		return GetFileName( filePathCopy );
	}

	std::string GetFileName( const std::string& filePath )
	{
		std::string::size_type idx0 = filePath.rfind( '\\' );
		std::string::size_type idx1 = filePath.rfind( '/' );

		if ( idx0 != std::string::npos && idx1 != std::string::npos )
		{
			std::string::size_type idxMax = std::max( idx0, idx1 );
			return filePath.substr( idxMax + 1 );
		}
		else if ( idx0 != std::string::npos )
		{
			return filePath.substr( idx0 + 1 );
		}
		else if ( idx1 != std::string::npos )
		{
			return filePath.substr( idx1 + 1 );
		}

		SPAD_ASSERT2( !filePath.empty(), "filePath is invalid!" );
		return filePath;
	}

	std::string GetFileNameWithoutExtension( const char* filePath )
	{
		std::string filename = GetFileName( filePath );
		std::string::size_type idx = filename.rfind( '.' );
		if ( idx != std::string::npos )
			return filename.substr( 0, idx );

		SPAD_ASSERT2( false, "filePath is invalid!" );
		return "";
	}

	std::string GetFileNameWithoutExtension( const std::string& filePath )
	{
		std::string filename = GetFileName( filePath );
		std::string::size_type idx = filename.rfind( '.' );
		if ( idx != std::string::npos )
			return filename.substr( 0, idx );

		SPAD_ASSERT2( false, "filePath is invalid!" );
		return "";
	}

	std::string GetFileExtension( const char* filePath )
	{
		std::string filename = GetFileName( filePath );
		std::string::size_type idx = filename.rfind( '.' );
		if ( idx != std::string::npos )
			return filename.substr( idx + 1 );

		SPAD_ASSERT2( false, "filePath is invalid!" );
		return "";
	}

	std::string GetFileExtension( const std::string& filePath )
	{
		std::string filename = GetFileName( filePath );
		std::string::size_type idx = filename.rfind( '.' );
		if ( idx != std::string::npos )
			return filename.substr( idx + 1 );

		SPAD_ASSERT2( false, "filePath is invalid!" );
		return "";
	}

	std::string GetFilePathWithoutExtension( const char* filePath )
	{
		return GetFilePathWithoutExtension( std::string(filePath) );
	}

	std::string GetFilePathWithoutExtension( const std::string& filePath )
	{
		std::string::size_type idx = filePath.rfind( '.' );
		if ( idx != std::string::npos )
			return filePath.substr( 0, idx );

		SPAD_ASSERT2( false, "filePath is invalid!" );
		return "";
	}


	bool PathsPointToTheSameFile( const char* path1, const char* path2 )
	{
		HANDLE fileHandle1 = CreateFile( path1, GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL );
		if ( fileHandle1 == INVALID_HANDLE_VALUE )
			return false;

		HANDLE fileHandle2 = CreateFile( path2, GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL );
		if ( fileHandle2 == INVALID_HANDLE_VALUE )
		{
			CloseHandle( fileHandle1 );
			return false;
		}

		BY_HANDLE_FILE_INFORMATION fi1, fi2;
		memset( &fi1, 0, sizeof( fi1 ) );
		memset( &fi2, 0, sizeof( fi2 ) );
		BOOL b1 = GetFileInformationByHandle( fileHandle1, &fi1 );
		BOOL b2 = GetFileInformationByHandle( fileHandle2, &fi2 );

		CloseHandle( fileHandle2 );
		CloseHandle( fileHandle1 );

		if ( b1 && b2 )
		{
			if (	fi1.dwVolumeSerialNumber == fi2.dwVolumeSerialNumber
				&&	fi1.nFileIndexLow == fi2.nFileIndexHigh
				&&	fi1.nFileIndexHigh == fi2.nFileIndexHigh
				)
				return true;
		}

		return false;
	}

	void AppendBackslashToDirectoryName( std::string& dirPath )
	{
		if ( !dirPath.empty() )
		{
			if (dirPath.back() != '/' && dirPath.back() != '\\')
				dirPath.append( 1, '\\' );
		}
	}

	std::string CanonicalizePathSimple( const std::string& srcPath )
	{
		//std::string dst;
		//const size_t srcPathLen = srcPath.length();
		//dst.resize( srcPathLen + 1 );

		//char lastC = '\\'; // this will remove leading '\' or '/'
		//for ( size_t i = 0; i < srcPathLen; ++i )
		//{
		//	char c = srcPath[i];
		//	if ( c == '/' )
		//		c = '\\';

		//	if ( lastC == '\\' && c == '\\' )
		//	{
		//		// skip
		//	}
		//	else
		//	{
		//		lastC = c;
		//		dst.append( 1, c );
		//	}
		//}

		//return dst;
		char dst[MAX_PATH];
		BOOL res = PathCanonicalize( dst, srcPath.c_str() );
		if ( !res )
		{
			logError( "PathCanonicalize failed!" );
			return "";
		}

		return dst;
	}

} // namespace spad
