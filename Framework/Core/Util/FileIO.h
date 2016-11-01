#pragma once

namespace spad
{
	bool FileExists( const char* filePath );
	inline bool FileExists( const std::string& filePath )
	{
		return FileExists( filePath.c_str() );
	}

	bool DirectoryExists( const char* filePath );
	inline bool DirectoryExists( const std::string& filePath )
	{
		return DirectoryExists( filePath.c_str() );
	}

	//u64 getFileTimeStamp( const char* fileName, bool& fileFound );
	u64 GetFileTimestamp( const wchar_t* filePath, bool& fileFound );
	u64 GetFileTimestamp( const char* filePath, bool& fileFound );
	inline u64 GetFileTimestamp( const std::string& filePath, bool& fileFound )
	{
		return GetFileTimestamp( filePath.c_str(), fileFound );
	}

	// we have to create copies of these strings in function and by passing by values certain situations can be optimized by compiler
	void CreateDirectoryRecursive( std::string absFilePath );
	void CreateDirectoryRecursive( std::string rootDirectory, std::string file );

	int RemoveDirectoryRecursively( const std::string& directory );
	//int readTextFile( const char* filename, char** outBuffer, uint32_t* outBufSize );
	std::string ReadTextFileAsString( const char* filename );
	std::vector<uint8_t> ReadFileAsVector( const char* filename );
	int WriteFileSync( const char* filename, const void* srcBuffer, size_t srcBufSize );
	inline int WriteFileSync( const std::string& filename, const std::string& text )
	{
		return WriteFileSync( filename.c_str(), text.c_str(), text.length() );
	}

	// GetAbsolutePath return absolute, canonical path (without '..' and '.'), with consistent use of slashes/backslashes
	// Sets drive letter to upper case
	std::string GetAbsolutePath( const char* filePath );
	std::string GetAbsolutePath( const std::string& filePath );

	// adds backslash at the end
	std::string GetDirectoryFromFilePath( const char* filePath );
	std::string GetDirectoryFromFilePath( const std::string& filePath );

	std::string GetFileName( const char* filePath );
	std::string GetFileName( const std::string& filePath );
	std::string GetFileNameWithoutExtension( const char* filePath );
	std::string GetFileNameWithoutExtension( const std::string& filePath );
	std::string GetFileExtension( const char* filePath );
	std::string GetFileExtension( const std::string& filePath );
	std::string GetFilePathWithoutExtension( const char* filePath );
	std::string GetFilePathWithoutExtension( const std::string& filePath );

	bool PathsPointToTheSameFile( const char* path1, const char* path2 );
	inline bool PathsPointToTheSameFile( const std::string& path1, const std::string& path2 )
	{
		return PathsPointToTheSameFile( path1.c_str(), path2.c_str() );
	}

	// Doesn't append backslash if dirPath is empty
	void AppendBackslashToDirectoryName( std::string& dirPath );

	std::string CanonicalizePathSimple( const std::string& srcPath );

	// utils

	inline void AppendString( FILE* f, const std::string& str )
	{
		u32 len = (u32)str.length() + 1;
		fwrite( &len, 4, 1, f );
		fwrite( str.c_str(), len, 1, f );
	}

	inline void AppendU32( FILE* f, u32 x )
	{
		fwrite( &x, 4, 1, f );
	}

	inline void AppendArray( FILE* f, const u32* x, u32 nX )
	{
		fwrite( x, nX * 4, 1, f );
	}

} // namespace spad
