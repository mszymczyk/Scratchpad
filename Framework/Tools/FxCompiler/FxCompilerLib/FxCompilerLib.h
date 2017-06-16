#pragma once

#include <libconfig/lib/libconfig.h>
#include <Util/MultiDimensionalArray.h>
#include "FxState.h"
#include "IncludeCache.h"

namespace spad
{
namespace fxlib
{

class FxFile;

enum e_ProgramType : u8
{
	eProgramType_vertexShader,
	eProgramType_pixelShader,
	eProgramType_geometryShader,
	eProgramType_computeShader,
	eProgramType_count
};

struct FxProgDefine
{
	FxProgDefine()
	{	}

	template<class T, class U>
	FxProgDefine( T&& _name, U&& _value )
		: name_( std::forward<T>( _name ) )
		, value_( std::forward<U>( _value ) )
	{	}

	std::string name_;
	std::string value_;
};

typedef std::vector<FxProgDefine> FxProgDefineArray;



struct FxProgDefineMultiValue
{
	std::string name_;
	std::vector<std::string> values_;
};



class FxPassCombination
{
public:
	FxPassCombination()
	{
		memset( entryIdx_, 0xffffffff, sizeof( entryIdx_ ) );
	}

	const std::string& getFullName() const { return fullName_; }
	template<class T>
	void setFullName( T&& val ) { fullName_ = std::forward<T>( val ); }

	u32 getUniqueProgramIndex( e_ProgramType progType ) const { return entryIdx_[progType]; }
	void setUniqueProgramIndex( e_ProgramType progType, u32 idx )
	{
		SPAD_ASSERT( progType < eProgramType_count );
		entryIdx_[progType] = idx;
	}

	const u32* getUniqeProgramIndices() const { return entryIdx_; }

private:
	std::string fullName_; // for debug
	u32 entryIdx_[eProgramType_count];
};

typedef MultiDimensionalArray<FxPassCombination> FxPassCombinationsMatrix;



class FxPass
{
public:
	template<class T>
	FxPass( const FxFile& fxFile, T&& name, const RenderState& rs, FxPassCombinationsMatrix&& comb )
		: fxFile_( fxFile )
		, name_( std::forward<T>( name ) )
		, renderState_( rs )
		, passCombinations_( std::move( comb ) )
	{
	}

	const std::string& getName() const { return name_; }

	const RenderState& getRenderState() const { return renderState_; }

	const FxPassCombinationsMatrix& getCombinations() const { return passCombinations_; }

private:
	const FxFile& fxFile_;
	std::string name_;

	RenderState renderState_;
	FxPassCombinationsMatrix passCombinations_;
};

typedef std::vector<std::unique_ptr<FxPass>> FxPassArray;



class FxProgram
{
public:
	static std::string MakeUniqueName( const std::string& entryName, e_ProgramType type, const std::string& cflags, const std::vector<FxProgDefine>& cdefines )
	{
		std::stringstream ss;
		ss << entryName << ';';
		ss << type << ';';
		ss << cflags << ';';
		for ( const auto& d : cdefines )
			ss << d.name_ << '=' << d.value_ << ';';

		return ss.str();
	}

	FxProgram( const std::string& entryName, e_ProgramType type, u32 index, const std::string& cflags, const std::vector<FxProgDefine>& cdefines, const config_setting_t* setting )
		: entryName_( entryName )
		, type_( type )
		, refCount_( 1 )
		, index_( index )
		//, cflags_( cflags )
		, cdefines_( cdefines )
		, setting_( setting )
	{
		uniqueName_ = MakeUniqueName( entryName, type, cflags, cdefines );
	}

	//FxProgram( const FxProgram& rhs ) = delete;
	//FxProgram( FxProgram&& rhs ) = delete;

	//FxProgram& operator= ( const FxProgram& rhs ) = delete;
	//FxProgram& operator= ( FxProgram&& rhs ) = delete;

	const std::string&			getEntryName()			const { return entryName_; }
	e_ProgramType				getProgramType()		const { return type_; }
	u32							getIndex()				const { return index_; }

	const FxProgDefineArray&	getCdefines()			const { return cdefines_; }
	const config_setting_t*		getSetting()			const { return setting_; }

	const std::string&			getUniqueName()		const { return uniqueName_; }

	void						incRefCount()		{ refCount_ += 1; }

private:
	std::string entryName_;
	e_ProgramType type_ = eProgramType_count;
	// padding
	u32 refCount_ = 0;
	u32 index_ = 0xffffffff; // index within FxFile
	// padding

	//std::string cflags_;
	FxProgDefineArray cdefines_;

	const config_setting_t* setting_ = nullptr; // weak pointer to setting that this program was parsed from, valid as long as FxFile::config_ is valid

	std::string uniqueName_;
};

typedef std::vector<std::unique_ptr<FxProgram>> FxProgramArray;


namespace FxCompileConfiguration
{
enum  Type
{
	debug,
	release,
	diagnostic,
	shipping
};
}


struct FxFileCompileOptions
{
	const char* const* argv_ = nullptr; // inputs to 'main' (first entry is program name)
	int argc_ = 0; // inputs to 'main', >= 1 (first entry is program name)

	FxCompileConfiguration::Type configuration_ = FxCompileConfiguration::shipping;

	//std::string mainFilename_;
	std::vector<FxProgDefine> defines_;
	bool logProgress_ = false; // essential info
	bool logProgressVerbose_ = false; // more info
	bool multithreaded_ = true; // multiple programs compiled simultaneously
	bool multithreadedFiles_ = true; // multiple files processed simultaneously
	bool compileForDebugging_ = false;
	bool forceRecompile_ = false; // don't check dependency information, always compile and overwrite

	// output options
	//std::string outputDirectory_;
	//std::string intermediateDirectory_;
	bool writeSource_ = false;
	bool writeCompiledPacked_ = true;
	bool writeCompiled_ = false;

	u64 compilerTimestamp_ = 0;
};




class FxFile
{
public:
	FxFile();
	~FxFile();

	FxFile( const FxFile& rhs ) = delete;
	FxFile& operator= ( const FxFile& rhs ) = delete;

	const FxProgramArray& getUniquePrograms() const { return uniquePrograms_; }
	const FxPassArray& getPasses() const { return passes_; }

	const std::string& getFilename() const { return filename_; }
	const std::string& getFileAbsolutePath() const { return fileAbsolutePath_; }
	// returns source code with AlwaysIncludedByFxCompiler.h prepended
	const std::string& getSourceCode() const { return sourceCode_; }
	//void getFxHeader( const char*& fxHeader, size_t& fxHeaderLength ) const
	//{
	//	fxHeader = fxHeader_;
	//	fxHeaderLength = fxHeaderLength_;
	//}

	// returns source code WITHOUT AlwaysIncludedByFxCompiler.h prepended
	void getOrigSourceCode( const char*& sourceCode, size_t& sourceCodeLength ) const
	{
		sourceCode = sourceCode_.c_str() + fileSourceCodeOffset_;
		sourceCodeLength = sourceCode_.size() - fileSourceCodeOffset_;
	}

private:
	void _ReadAndParseFxFile( const char* srcFile, const char* srcFileDir, const FxFileCompileOptions& options, IncludeCache& includeCache );

	std::string _ReadSourceFile( const char* filename, IncludeCache& includeCache );
	void _Parse();
	void _FindHeader( const char* fileBuf, const size_t fileSize, const char** dstHeader, size_t* dstHeaderSize, const char** dstSourceCode, size_t* dstSourceCodeSize );
	void _FindPasses( const char* fxheader, const size_t fxheaderSize, const char** dstFxPasses, size_t* dstFxPassesSize );
	void _ExtractPasses();
	void _ReadPasses( const void* configPtr );
	void _ReadProgram( const config_setting_t* prog, std::string& dstCflags, std::string& dstEntryName, std::vector<FxProgDefineMultiValue>& dstProgCDefines );
	int _FindMatchingProgram( std::string& entryName, e_ProgramType programProfile, const std::string& cflags, const std::vector<FxProgDefine>& cdefines );
	bool _ReadBoolState( const config_setting_t* sett, const char* passName );
	void _EnsureSettingIsString( const config_setting_t* sett, const char* passName );
	void _ReadState( const config_setting_t* pass, RenderState& rs, const char* passName );

private:
	std::string filename_; // this is filename as passed to the compiler
	std::string fileAbsolutePath_;
	std::string sourceCode_;
	size_t fileSourceCodeOffset_ = 0;

	const char* fxHeader_ = nullptr; // fx header followed by source code, length of header only is stored in fxHeaderLength_
	size_t fxHeaderLength_ = 0;
	const char* fxPasses_ = nullptr;
	size_t fxPassesLength_ = 0;

	config_t config_;
	FxPassArray passes_;
	FxProgramArray uniquePrograms_;

	friend std::unique_ptr<spad::fxlib::FxFile> ParseFxFile( const char* srcFile, const char* srcFileDir, const FxFileCompileOptions& options, IncludeCache& includeCache, int* err /*= nullptr*/ );
};



struct CompileContext
{
	IncludeCache* includeCache = nullptr;
};

// srcFilePath is relative to current working directory
// srcFileDir is directory relative to current working directory where srcFile physically resides
// this is for MaterialEditor generated files, that are placed outside demo directory
std::unique_ptr<FxFile> ParseFxFile( const char* srcFile, const char* srcFileDir, const FxFileCompileOptions& options, IncludeCache& includeCache, int* err = nullptr );

// util
bool CheckIfRecompilationIsRequired( const char* srcPath, const char* dependPath, const char* outputPaths[], size_t nOutputPaths, u64 compilerTimestamp, FxCompileConfiguration::Type configuration );
bool ExtractCompilerFlags( const FxProgram& fxProg, const char* settingName, std::string& flags, std::vector<std::string>& flagsStorage, std::vector<const char*>& flagsPointers );
void WriteCompiledFxFileHeader( FILE* ofs, const FxFile& fxFile, FxCompileConfiguration::Type configuration );
void WriteCompiledFxFilePasses( FILE* ofs, const FxFile& fxFile );

} // namespace fxlib
} // namespace spad
