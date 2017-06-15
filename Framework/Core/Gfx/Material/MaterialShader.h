#pragma once

#include "../Dx11/Dx11DeviceContext.h"
//#include <FxLib/FxLib.h>
#include <Tools/FxCompiler/FxCompilerLib/FxTypes.h>
#include "../Dx11/Dx11Wrappers.h"
#include <Util/ReferenceCounting.h>
#include <unordered_set>

namespace tinyxml2
{
class XMLDocument;
}

namespace spad
{
class MaterialShader;
class MaterialShaderInstance;

class ContextShaderBindings
{
public:
	enum
	{
		eMax_VS_SRVs = 4,
		eMax_GS_SRVs = 4,
		eMax_PS_SRVs = 24,

		eMax_VS_Samplers = 4,
		eMax_GS_Samplers = 4,
		eMax_PS_Samplers = 12,
	};

	ContextShaderBindings()
	{
		memset( this, 0, sizeof( *this ) );
	}

	void commit( Dx11DeviceContext& deviceContext ) const
	{
		ID3D11DeviceContext* context = deviceContext.context;

		context->VSSetShader( vs_, nullptr, 0 );
		context->GSSetShader( gs_, nullptr, 0 );
		context->PSSetShader( ps_, nullptr, 0 );

		context->VSSetShaderResources( 0, eMax_VS_SRVs, vsSRVs );
		context->VSSetSamplers( 0, eMax_VS_Samplers, vsSamplers );

		context->GSSetShaderResources( 0, eMax_GS_SRVs, gsSRVs );
		context->GSSetSamplers( 0, eMax_GS_Samplers, gsSamplers );

		context->PSSetShaderResources( 0, eMax_PS_SRVs, psSRVs );
		context->PSSetSamplers( 0, eMax_PS_Samplers, psSamplers );
	}

	ID3D11VertexShader* vs_;
	ID3D11GeometryShader* gs_;
	ID3D11PixelShader* ps_;

	ID3D11ShaderResourceView* vsSRVs[eMax_VS_SRVs];
	ID3D11SamplerState* vsSamplers[eMax_VS_Samplers];

	ID3D11ShaderResourceView* gsSRVs[eMax_GS_SRVs];
	ID3D11SamplerState* gsSamplers[eMax_GS_Samplers];

	ID3D11ShaderResourceView* psSRVs[eMax_PS_SRVs];
	ID3D11SamplerState* psSamplers[eMax_PS_Samplers];
};

class MaterialShaderPass
{
public:
	~MaterialShaderPass()
	{
		DX_SAFE_RELEASE( vs_ );
		DX_SAFE_RELEASE( ps_ );
		DX_SAFE_RELEASE( gs_ );
		DX_SAFE_RELEASE( cs_ );
		DX_SAFE_RELEASE( vsInputSignature_ );
	}

	//void fill( ContextShaderBindings& ctx ) const;

	//MaterialShader* materialShader_ = nullptr;

	ID3D11VertexShader* vs_ = nullptr;
	ID3D11PixelShader* ps_ = nullptr;
	ID3D11GeometryShader* gs_ = nullptr;
	ID3D11ComputeShader* cs_ = nullptr;
	ID3D10Blob* vsInputSignature_ = nullptr;
	uint32_t vsInputSignatureHash_ = 0;

	struct Tex
	{
		u16 index_;
		fxlib::ShaderStage::Type stage_;
		u16 bindingPoint_;
	};

	std::vector<Tex> textures_;

	struct Samp
	{
		u16 index_;
		fxlib::ShaderStage::Type stage_;
		u16 bindingPoint_;
	};

	std::vector<Samp> samplers_;

	std::string name_;

	friend class MaterialShader;
};

class IMaterialShader
{
public:
	virtual void fill( ContextShaderBindings& ctx, const MaterialShaderPass& pass ) const = 0;
	virtual const MaterialShaderPass* getPass( const char* name ) const = 0;
};

//typedef std::shared_ptr<IMaterialShader> IMaterialShaderPtr;
typedef RefCountHandle<IMaterialShader> IMaterialShaderPtr;


class MaterialShaderFilename
{
protected:
	MaterialShaderFilename( std::string&& filename )
		: m_filename( std::move( filename ) )
	{	}

	std::string m_filename;

	friend class MaterialManager;
};


class MaterialShaderInstanceName
{
protected:
	MaterialShaderInstanceName( const char* instanceName )
		: m_instanceName( instanceName )
	{	}
	
	MaterialShaderInstanceName( std::string&& instanceName )
		: m_instanceName( std::move( instanceName ) )
	{	}

	std::string m_instanceName;

	friend class MaterialManager;
	friend class MaterialShader;
};


class MaterialShader : public MaterialShaderFilename, public IMaterialShader
{
public:
	//MaterialShader( const char* filename )
	//	: m_filename( filename )
	//{	}

	MaterialShader( std::string&& filename )
		: MaterialShaderFilename( std::move(filename) )
	{	}

	~MaterialShader();

	void AddRef()
	{
		++refCount_;
	}

	void Release()
	{
		// manager will hold the last reference
		if ( --refCount_ == 1 )
			delete this;
	}

	uint32_t GetRefCount() const
	{
		return refCount_;
	}

	const char* getFilename() const
	{
		return m_filename.c_str();
	}

	enum { InvalidIndex = 0xffff };

	u16 getUniformIndex( const char* name ) const;
	void setUniform1f( u16 index, float x );
	void setUniform3f( u16 index, const float* xyz );

	u16 getTextureIndex( const char* name ) const;
	void setTexture( const char* name, ID3D11ShaderResourceView* srv );

	u16 getSamplerIndex( const char* name ) const;
	void setSampler( const char* name, ID3D11SamplerState* samp );

	const MaterialShaderPass* getPass( const char* name ) const;

	void updateMaterialParams( Dx11DeviceContext& deviceContext );
	void bindMaterialParamsUniformsVSPS( Dx11DeviceContext& deviceContext );

	void fill( ContextShaderBindings& ctx, const MaterialShaderPass& pass ) const;

private:
	//std::string m_filename;

	ConstantBuffer2 uniformsBuf_;
	u32 uniformsBufVersion_ = 0;
	bool uniformsBufDirty_ = true;

	struct Uniform
	{
		std::string name;
		u32 offset = 0;
		u32 size = 0;
	};

	struct Tex
	{
		std::string name;
		ID3D11ShaderResourceViewPtr srv;
		//D3D_SHADER_INPUT_TYPE type;
		D3D_SRV_DIMENSION dimension = D3D_SRV_DIMENSION_UNKNOWN;
	};

	struct Samp
	{
		std::string name;
		ID3D11SamplerStatePtr samp;
	};

	std::vector<Uniform> m_uniforms;
	std::vector<Tex> m_textures;
	std::vector<Samp> m_samplers;

	std::vector<MaterialShaderPass> m_passes;

	tinyxml2::XMLDocument* m_doc = nullptr; // document it was loaded from

	struct MaterialShaderInstanceHash
	{
		std::size_t operator()( const MaterialShaderInstanceName* k ) const
		{
			return std::hash<std::string>()( k->m_instanceName );
		}
	};

	struct MaterialShaderInstanceEqual
	{
		bool operator()( const MaterialShaderInstanceName* lhs, const MaterialShaderInstanceName* rhs ) const
		{
			return lhs->m_instanceName == rhs->m_instanceName;
		}
	};

	typedef std::unordered_set<MaterialShaderInstanceName*, MaterialShaderInstanceHash, MaterialShaderInstanceEqual> MaterialShaderInstanceSet;
	MaterialShaderInstanceSet m_materialInstances;

	void _Release( MaterialShaderInstance* materialInstance );

private:
	std::atomic<uint32_t> refCount_ = 0;

	friend class MaterialManager;
	friend class MaterialShaderFactory;
	friend class MaterialShaderPass;
	friend class MaterialShaderInstance;
}; // class MaterialShader

//typedef std::shared_ptr<MaterialShader> MaterialShaderPtr;
typedef RefCountHandle<MaterialShader> MaterialShaderPtr;

class MaterialShaderInstance : public MaterialShaderInstanceName, public IMaterialShader
{
public:
	MaterialShaderInstance( const char* instanceName )
		: MaterialShaderInstanceName( instanceName )
	{	}

	~MaterialShaderInstance();

	void AddRef()
	{
		++refCount_;
	}

	void Release()
	{
		// shader will hold the last reference
		if ( --refCount_ == 1 )
			delete this;
	}

	uint32_t GetRefCount() const
	{
		return refCount_;
	}

	u16 getUniformIndex( const char* name ) const
	{
		return m_materialShader->getUniformIndex( name );
	}

	void setUniform1f( u16 index, float x );
	void setUniform3f( u16 index, const float* xyz );

	u16 getTextureIndex( const char* name ) const
	{
		return m_materialShader->getTextureIndex( name );
	}

	void setTexture( const char* name, ID3D11ShaderResourceView* srv );

	const MaterialShaderPass* getPass( const char* name ) const
	{
		return m_materialShader->getPass( name );
	}

	void updateMaterialInstanceParams( Dx11DeviceContext& deviceContext );
	void bindMaterialInstanceUniformsVSPS( Dx11DeviceContext& deviceContext );

	void fill( ContextShaderBindings& ctx, const MaterialShaderPass& pass ) const;

private:
	void _MaterialReloaded( ID3D11Device* device );
	void _RefreshParams( const tinyxml2::XMLDocument* doc, ID3D11Device* device );

	MaterialShaderPtr m_materialShader;
	//std::string m_instanceName;

	ConstantBuffer2 uniformsBuf_;
	u32 materialUniformsBufVersion_ = 0;
	bool uniformsBufDirty_ = true;

	std::vector<bool> m_uniforms; // flag for each uniform saying if given uniform is overridden by instance

	std::vector<ID3D11ShaderResourceViewPtr> m_textures;

private:
	std::atomic<uint32_t> refCount_ = 0;

	friend class MaterialManager;
	friend class MaterialShader;
	friend class MaterialShaderFactory;
	friend class MaterialShaderPass;
};

//typedef std::shared_ptr<MaterialShaderInstance> MaterialShaderInstancePtr;
typedef RefCountHandle<MaterialShaderInstance> MaterialShaderInstancePtr;

} // namespace spad
