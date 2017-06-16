#include "FxCompilerLib_pch.h"
#include "IncludeCache.h"

#if defined(_MSC_VER) && defined(_DEBUG)
#define new _DEBUG_NEW
#endif

namespace spad
{
namespace fxlib
{


IncludeCache::~IncludeCache()
{
	for ( auto& f : loadedIncludes_ )
		delete f.second;

	delete compilerInclude_;
}

const IncludeCache::File* IncludeCache::getFile( const char* absolutePathOrig/*, bool checkFileTimestamp*/ )
{
	std::string absolutePath = GetAbsolutePath( absolutePathOrig );

	std::lock_guard<std::mutex> lck( mutex_ );

	IncludeMap::const_iterator it = loadedIncludes_.find( absolutePath );
	if ( it != loadedIncludes_.end() )
	{
		const File* f = it->second;
		return f;
	}

	std::string sourceCode = ReadTextFileAsString( absolutePath.c_str() );
	if ( sourceCode.empty() )
		return nullptr;

	File* f = new File();
	loadedIncludes_[absolutePath] = f;

	f->absolutePath_ = absolutePath;
	f->sourceCode_ = std::move( sourceCode );
	return f;
}


const IncludeCache::File* IncludeCache::searchFile( const char* relativePathOrig )
{
	std::string relativePath = relativePathOrig;

	std::lock_guard<std::mutex> lck( mutex_ );

	const size_t nSearchPaths = searchPaths_.size();
	for ( size_t isp = 0; isp < nSearchPaths; ++isp )
	{
		const std::string& sp = searchPaths_[isp];


		std::string absolutePath = sp + relativePath;
		std::string prettyAbsolutePath = GetAbsolutePath( absolutePath );

		IncludeMap::const_iterator it = loadedIncludes_.find( prettyAbsolutePath );
		if ( it != loadedIncludes_.end() )
		{
			const File* f = it->second;
			return f;
		}

		std::string sourceCode = ReadTextFileAsString( prettyAbsolutePath.c_str() );
		if ( sourceCode.empty() )
			continue;

		File* f = new File();
		loadedIncludes_[prettyAbsolutePath] = f;

		f->absolutePath_ = prettyAbsolutePath;
		//bool fileFound = false;
		//f->timeStamp_ = getFileTimestamp( absolutePath.c_str(), fileFound );
		f->sourceCode_ = std::move( sourceCode );
		return f;
	}

	return nullptr;
}

const IncludeCache::File* IncludeCache::getFileByDataPtr( const void* dataPtr )
{
	std::lock_guard<std::mutex> lck( mutex_ );

	for ( const auto& it : loadedIncludes_ )
	{
		const File* f = it.second;
		if ( f->sourceCode_.c_str() == dataPtr )
			return f;
	}

	return nullptr;
}

void IncludeCache::AddSearchPath( const std::string& absolutePath )
{
	std::string d = GetAbsolutePath( absolutePath );
	AppendBackslashToDirectoryName( d );
	searchPaths_.push_back( d );
}


int IncludeCache::Load_AlwaysIncludedByFxCompiler()
{
	SPAD_ASSERT( !compilerInclude_ );

	const std::string& relativePath = "AlwaysIncludedByFxCompiler.h";

	const size_t nSearchPaths = searchPaths_.size();
	for ( size_t isp = 0; isp < nSearchPaths; ++isp )
	{
		const std::string& sp = searchPaths_[isp];

		std::string absolutePath = sp + relativePath;
		std::string sourceCode = ReadTextFileAsString( absolutePath.c_str() );
		if ( sourceCode.empty() )
			continue;

		compilerInclude_ = new File();
		compilerInclude_->absolutePath_ = GetAbsolutePath( absolutePath );

		// append #line directive so compiler can output right line names in diagnostics
		// without it, line numbers would be offset by several lines which is misleading for the programmer
		sourceCode.append( "#line 1\n" );

		compilerInclude_->sourceCode_ = sourceCode;
		return 0;
	}

	return -1;
}


void IncludeCache::clearLoadedIncludes()
{
	for ( auto& f : loadedIncludes_ )
		delete f.second;
	loadedIncludes_.clear();
}

} // namespace fxlib
} // namespace spad
