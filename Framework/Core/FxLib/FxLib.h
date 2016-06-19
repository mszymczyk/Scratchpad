#pragma once

#include <Util/Def.h>
#include "FxLibHlsl.h"
#include "FxRuntime.h"

namespace spad
{
	namespace FxLib
	{
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
			std::string name;
			std::string value;
		};

		struct FxPass
		{
			FxPass()
				: hasCombinations_( false )
			{
				memset( entryIdx, 0xffffffff, sizeof( entryIdx ) );
			}

			std::string passName;
			u32 entryIdx[eProgramType_count];
			bool hasCombinations_;
		};

		typedef std::vector<std::unique_ptr<FxPass>> FxPassArray;

		struct FxProgramCompilerParams
		{
			FxProgramCompilerParams()
				: runtimeCompilation( -1 )
			{	}

			std::string cflags_hlsl; // string with all arguments passed to compiler, exactly same flags that are passed do command-line compilers, like fxc
			std::vector<FxProgDefine> cdefines;
			std::vector<FxProgDefine> cdefines2; // for use with permutations

			int runtimeCompilation; // dx only - true means that shader should be compiled from source code
		};

		struct FxProgram
		{
			FxProgram()
				: profile( eProgramType_count )
				, refCount_( 0 )
				, opaque_( nullptr )
			{	}

			//FxProgram( const FxProgram& rhs ) = delete;
			//FxProgram( FxProgram&& rhs ) = delete;

			//FxProgram& operator= ( const FxProgram& rhs ) = delete;
			//FxProgram& operator= ( FxProgram&& rhs ) = delete;

			std::string entryName;
			e_ProgramType profile;
			u16 refCount_;
			FxProgramCompilerParams compilerParams;

			HlslProgramData hlslProgramData_;

			void* opaque_;
		};

		typedef std::vector<std::unique_ptr<FxProgram>> FxProgramArray;



		struct FxFileCompileOptions
		{
			FxFileCompileOptions()
				: logProgress_( false )
				, multithreaded_( false )
				, compileForDebugging_( false )
			{	}

			std::string mainFilename_;
			std::vector<FxProgDefine> defines_;
			bool logProgress_;
			bool multithreaded_;
			bool compileForDebugging_;
		};

		//struct FxFileCompileEffectOptions
		//{
		//	FxFileCompileEffectOptions()
		//	{	}

		//	bool compileForDebugging_ = false;
		//};

		struct FxFileWriteOptions
		{
			FxFileWriteOptions()
				//: absoluteDstDirectory( NULL )
				: logProgress_( false )
				, writeSource_( true )
				, writeCompiled_( true )
				//, writeCompiledPacked( true )
				//, generateAssembly( false )
			{	}

			std::string outputDirectory_;
			std::string intermediateDirectory_;
			bool logProgress_;
			bool writeSource_;
			bool writeCompiled_;
			//bool writeCompiledPacked;
			//bool generateAssembly;
		};


		class FxFile
		{
		public:
			FxFile()
				: fxHeader_( NULL )
				, fxHeaderLength_( 0 )
				, fxPasses_( NULL )
				, fxPassesLength_( 0 )
			{	}

			~FxFile();

			FxFile( const FxFile& rhs ) = delete;
			FxFile& operator= ( const FxFile& rhs ) = delete;

			int compileFxFile( const char* srcFilePathAbsolute, const FxFileCompileOptions& options );
			int writeCompiledFxFile( const FxFileWriteOptions& options );
			int loadCompiledFxFile( const char* srcFilePathAbsolute );
			int createShaders( ID3D11Device* dxDevice );
			std::unique_ptr<FxRuntime> createFxRuntime( ID3D11Device* dxDevice ) const;

			int reflectHlslData();

			const FxProgramArray& getUniquePrograms() const
			{
				return uniquePrograms_;
			}

			const FxPassArray& getPasses() const
			{
				return passes_;
			}

		private:
			void _CompileFxFileImpl( const char* srcFilePathAbsolute, const FxFileCompileOptions& options );
			void _LoadFxFileImpl( const char* srcFilePathAbsolute );
			void _CreateShaders( ID3D11Device* dxDevice );
			std::unique_ptr<FxRuntime> _CreateFxRuntime( ID3D11Device* dxDevice ) const;

			void _Parse();
			void _FindHeader( const char* fileBuf, const size_t fileSize, const char** dstHeader, size_t* dstHeaderSize, const char** dstSourceCode, size_t* dstSourceCodeSize );
			void _FindPasses( const char* fxheader, const size_t fxheaderSize, const char** dstFxPasses, size_t* dstFxPassesSize );
			void _ExtractPasses();
			void _ReadPasses( const void* configPtr );
			void _GenCombinations( std::vector<u32>& combinations, const FxPass& fxPass );
			void _HandleCombinations();
			void _ReadProgram( const void* progPtr, e_ProgramType programProfile, FxPass& fxPass );

			int _FindMatchingProgram( const std::unique_ptr<FxProgram>& fxProg, const FxPass& fxPass );
			void _CompileHlsl( const FxFileCompileOptions& options, const FxFileHlslCompileOptions& dxOptions );
			void _CompileHlslProgram( size_t progIdx, const FxFileCompileOptions& options, const FxFileHlslCompileOptions& hlslOptions );

			void _WriteCompiledHlsl( const FxFileWriteOptions& options );
			void _ReadCompiledHlsl();

		private:
			std::string filename_;
			std::string sourceCode_;

			const char* fxHeader_; // fx header followed by source code, length of header only is stored in fxHeaderLength_
			size_t fxHeaderLength_;
			const char* fxPasses_;
			size_t fxPassesLength_;

			FxPassArray passes_;
			FxProgramArray uniquePrograms_;
		};

		std::unique_ptr<FxLib::FxRuntime> loadCompiledFxFile( ID3D11Device* dxDevice, const char* filename );

	} // namespace FxLib
} // namespace spad
