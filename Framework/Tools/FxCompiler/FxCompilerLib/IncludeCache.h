#pragma once

namespace spad
{
namespace fxlib
{


struct IncludeCache
{
	IncludeCache()
	{	}
	~IncludeCache();

	struct File
	{
		File()
		{	}

		std::string absolutePath_; // nice canonical name
		//u64 timeStamp_;
		std::string sourceCode_; // file contents
	};

	const File* getFile( const char* absolutePath/*, bool checkFileTimestamp*/ );
	const File* searchFile( const char* relativePath );
	const File* getFileByDataPtr( const void* dataPtr );

	// not thread safe, should be called during startup
	void AddSearchPath( const std::string& absolutePath );

	// not thread safe, should be called during startup
	int Load_AlwaysIncludedByFxCompiler();
	const File* Get_AlwaysIncludedByFxCompiler() const { return compilerInclude_; }

	void clearLoadedIncludes();

private:
	std::mutex mutex_;

	std::vector<std::string> searchPaths_;

	// general solution to comparing two different path for equality is quite complicated
	// here's simple and 'quite' robust solution (fails in some network related cases)
	// http://stackoverflow.com/questions/562701/best-way-to-determine-if-two-path-reference-to-same-file-in-windows/562773#562773
	// it requires asking OS's file system for file info each time we do the comparison
	// file info cannot be cached because it can change between calls to CreateFile
	// our needs and usage scenarios are quite simple and using absolute, normalized paths for comparisons seems sufficient
	typedef std::map<std::string, File*> IncludeMap;
	IncludeMap loadedIncludes_;

	File* compilerInclude_ = nullptr;
};


} // namespace fxlib
} // namespace spad
