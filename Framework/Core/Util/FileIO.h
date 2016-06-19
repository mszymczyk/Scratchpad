#pragma once

namespace spad
{
	bool FileExists( const char* filePath );

	//u64 getFileTimeStamp( const char* fileName, bool& fileFound );
	u64 getFileTimestamp( const wchar_t* filePath, bool& fileFound );

	void CreateDirectoryRecursive( const std::string& absFilePath );
	//int readTextFile( const char* filename, char** outBuffer, uint32_t* outBufSize );
	std::string ReadTextFileAsString( const char* filename );
	std::vector<uint8_t> ReadFileAsVector( const char* filename );
	int WriteFileSync( const char* filename, const void* srcBuffer, uint32_t srcBufSize );

	std::string GetAbsolutePath( const char* filePath );
	std::string GetAbsolutePath( const std::string& filePath );
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
	void AppendSlashToDirectoryName( std::string& dirPath );
	std::string CanonicalizePathSimple( const std::string& srcPath );

} // namespace spad
