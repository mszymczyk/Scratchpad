#pragma once

#include "FxCompilerLib.h"

namespace spad
{
namespace fxlib
{


struct IncludeDependencies
{
	void addDependencyNoLock( const std::string& filename )
	{
		filenames_.insert( filename );
	}

	void addDependency( const std::string& filename )
	{
		std::lock_guard<std::mutex> lck( mutex_ );
		filenames_.insert( filename );
	}

	void merge( const IncludeDependencies& other )
	{
		std::lock_guard<std::mutex> lck( mutex_ );
		filenames_.insert( other.filenames_.begin(), other.filenames_.end() );
	}

	int writeToFile( const std::string& filename, FxCompileConfiguration::Type configuration );

private:
	std::set<std::string> filenames_;
	std::mutex mutex_;
};


} // namespace fxlib
} // namespace spad
