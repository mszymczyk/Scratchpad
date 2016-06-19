#include "Util_pch.h"
#include "FileIO.h"
#include <direct.h>

#include <Pathcch.h>

#if defined(_MSC_VER) && defined(_DEBUG)
#define new _DEBUG_NEW
#endif

namespace spad
{
	// Returns true if a file exits
	bool FileExists( const char* filePath )
	{
		if ( filePath == NULL )
			return false;

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


	// Gets the last written timestamp of the file
	u64 getFileTimestamp( const wchar_t* filePath, bool& fileFound )
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

	void CreateDirectoryRecursive( const std::string& absFilePath )
	{
		FR_ASSERT( !absFilePath.empty() )

		std::string file = absFilePath;

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
		FR_ASSERT( readBytes == sizeInBytes );
		int ret = fclose( f );
		(void)ret;
		FR_ASSERT( ret != EOF );

		buf[sizeInBytes] = 0;

		*outBuffer = buf;
		*outBufSize = static_cast<uint32_t>(sizeInBytes);

		return 0;
	}

	int WriteFileSync( const char* filename, const void* srcBuffer, uint32_t srcBufSize )
	{
		FILE* f = fopen( filename, "wb" );
		if ( !f )
		{
			logError( "Cannot open file '%s' for writing!", filename );
			return -1;
		}

		size_t written = fwrite( srcBuffer, 1, srcBufSize, f );
		(void)written;
		FR_ASSERT( written == srcBufSize );
		int ret = fclose( f );
		FR_ASSERT( ret != EOF );
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

		return fileFullPath;
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

		FR_ASSERT2( false, "filePath is invalid!" );
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

		FR_ASSERT2( false, "filePath is invalid!" );
		return "";
	}

	std::string GetFileNameWithoutExtension( const char* filePath )
	{
		std::string filename = GetFileName( filePath );
		std::string::size_type idx = filename.rfind( '.' );
		if ( idx != std::string::npos )
			return filename.substr( 0, idx );

		FR_ASSERT2( false, "filePath is invalid!" );
		return "";
	}

	std::string GetFileNameWithoutExtension( const std::string& filePath )
	{
		std::string filename = GetFileName( filePath );
		std::string::size_type idx = filename.rfind( '.' );
		if ( idx != std::string::npos )
			return filename.substr( 0, idx );

		FR_ASSERT2( false, "filePath is invalid!" );
		return "";
	}

	std::string GetFileExtension( const char* filePath )
	{
		std::string filename = GetFileName( filePath );
		std::string::size_type idx = filename.rfind( '.' );
		if ( idx != std::string::npos )
			return filename.substr( idx + 1 );

		FR_ASSERT2( false, "filePath is invalid!" );
		return "";
	}

	std::string GetFileExtension( const std::string& filePath )
	{
		std::string filename = GetFileName( filePath );
		std::string::size_type idx = filename.rfind( '.' );
		if ( idx != std::string::npos )
			return filename.substr( idx + 1 );

		FR_ASSERT2( false, "filePath is invalid!" );
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

		FR_ASSERT2( false, "filePath is invalid!" );
		return "";
	}

	void AppendSlashToDirectoryName( std::string& dirPath )
	{
		if (dirPath.empty())
			dirPath.append( 1, '/' );
		else
		{
			if (dirPath.back() != '/' && dirPath.back() != '\\')
				dirPath.append( 1, '\\' );
		}
	}

	std::string CanonicalizePathSimple( const std::string& srcPath )
	{
		std::string dst;
		const size_t srcPathLen = srcPath.length();
		dst.reserve( srcPathLen + 1 );

		char lastC = '\\'; // this will remove leading '\' or '/'
		for ( size_t i = 0; i < srcPathLen; ++i )
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
				dst.append( 1, c );
			}
		}

		return dst;
	}

} // namespace spad
