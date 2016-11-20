#pragma once

#include "Dx11Util/Dx11Util.h"

namespace spad
{

struct RenderTarget2D
{
	ID3D11Texture2D* texture_ = nullptr;
	ID3D11RenderTargetView* rtv_ = nullptr;
	ID3D11ShaderResourceView* srv_ = nullptr;
	ID3D11UnorderedAccessView* uav_ = nullptr;
	u32 width_ = 0;
	u32 height_ = 0;
	u32 numMipLevels_ = 0;
	u32 arraySize_ = 0;
	DXGI_FORMAT format_ = DXGI_FORMAT_UNKNOWN;
	u32 multiSamples_ = 0;
	u32 msQuality_ = 0;
	bool autoGenMipMaps_ = false;
	bool cubeMap_ = false;
	std::vector<ID3D11RenderTargetView*> rtvArraySlices_;
	std::vector<ID3D11ShaderResourceView*> srvArraySlices_;

	std::string debugName_;

	RenderTarget2D( const char* _debugName )
		: debugName_( _debugName )
	{	}

	~RenderTarget2D();

	void Initialize( ID3D11Device* device,
		u32 width,
		u32 height,
		DXGI_FORMAT format,
		u32 numMipLevels = 1,
		u32 multiSamples = 1,
		u32 msQuality = 0,
		u32 arraySize = 1,
		bool autoGenMipMaps = false,
		bool createUAV = false,
		bool cubeMap = false
		);

	void DeInitialize();

	//void setDebugName( const char* debugName );
};

struct DepthStencil
{
	ID3D11Texture2D* texture_ = nullptr;
	ID3D11DepthStencilView* dsv_ = nullptr;
	ID3D11DepthStencilView* dsvReadOnly_ = nullptr;
	ID3D11ShaderResourceView* srv_ = nullptr;
	u32 width_ = 0;
	u32 height_ = 0;
	u32 arraySize_;
	DXGI_FORMAT format_ = DXGI_FORMAT_UNKNOWN;
	u32 multiSamples_ = 0;
	u32 msQuality_ = 0;
	std::vector<ID3D11DepthStencilView*> dsvArraySlices_;

	std::string debugName_;

	DepthStencil( const char* _debugName )
		: debugName_( _debugName )
	{	}

	~DepthStencil();

	void Initialize( ID3D11Device* device,
		u32 width,
		u32 height,
		u32 arraySize = 1,
		DXGI_FORMAT format = DXGI_FORMAT_D24_UNORM_S8_UINT,
		u32 multiSamples = 1,
		u32 msQuality = 0,
		bool useAsShaderResource = false
		);

	void DeInitialize();

};


template<typename T>
class ConstantBuffer
{
public:

	ConstantBuffer()
	{
		ZeroMemory( &data, sizeof( T ) );
	}

	~ConstantBuffer()
	{
		DX_SAFE_RELEASE( dxBuffer_ );
	}

	void Initialize( ID3D11Device* device )
	{
		D3D11_BUFFER_DESC desc;
		desc.ByteWidth = sizeof( T );
		desc.Usage = D3D11_USAGE_DEFAULT;
		desc.BindFlags = D3D11_BIND_CONSTANT_BUFFER;
		desc.CPUAccessFlags = 0;
		desc.MiscFlags = 0;
		desc.StructureByteStride = 0;

		D3D11_SUBRESOURCE_DATA initData;
		ZeroMemory( &initData, sizeof( initData ) );
		initData.pSysMem = &data;
		DXCall( device->CreateBuffer( &desc, &initData, &dxBuffer_ ) );
	}

	void updateGpu( ID3D11DeviceContext* deviceContext )
	{
		SPAD_ASSERT( dxBuffer_ != nullptr );

		deviceContext->UpdateSubresource( dxBuffer_, 0, nullptr, &data, 0, 0 );
	}

	void setVS( ID3D11DeviceContext* deviceContext, u32 slot ) const
	{
		SPAD_ASSERT( dxBuffer_ != nullptr );

		ID3D11Buffer* bufferArray[1];
		bufferArray[0] = dxBuffer_;
		deviceContext->VSSetConstantBuffers( slot, 1, bufferArray );
	}

	void setPS( ID3D11DeviceContext* deviceContext, u32 slot ) const
	{
		SPAD_ASSERT( dxBuffer_ != nullptr );

		ID3D11Buffer* bufferArray[1];
		bufferArray[0] = dxBuffer_;
		deviceContext->PSSetConstantBuffers( slot, 1, bufferArray );
	}

	void setGS( ID3D11DeviceContext* deviceContext, u32 slot ) const
	{
		SPAD_ASSERT( dxBuffer_ != nullptr );

		ID3D11Buffer* bufferArray[1];
		bufferArray[0] = dxBuffer_;
		deviceContext->GSSetConstantBuffers( slot, 1, bufferArray );
	}

	void setCS( ID3D11DeviceContext* deviceContext, u32 slot ) const
	{
		SPAD_ASSERT( dxBuffer_ != nullptr );

		ID3D11Buffer* bufferArray[1];
		bufferArray[0] = dxBuffer_;
		deviceContext->CSSetConstantBuffers( slot, 1, bufferArray );
	}

	ID3D11Buffer* getDxBuffer() const
	{
		return dxBuffer_;
	}

public:
	T data;

private:
	ID3D11Buffer* dxBuffer_ = nullptr;
};

// constant buffer with dynamically allocated data buffer, rather than templated one above
class ConstantBuffer2
{
public:

	ConstantBuffer2()
	{	}

	~ConstantBuffer2()
	{
		DX_SAFE_RELEASE( dxBuffer_ );
		spadFreeAligned( data );
	}

	ConstantBuffer2( ConstantBuffer2&& other )
		: data( other.data )
		, size( other.size )
		, dxBuffer_( other.dxBuffer_ )
	{
		other.data = nullptr;
		other.size = 0;
		other.dxBuffer_ = nullptr;
	}

	ConstantBuffer2& operator=( ConstantBuffer2 && other )
	{
		spadFreeAligned( data );
		data = other.data;
		other.data = nullptr;

		size = other.size;
		other.size = 0;

		DX_SAFE_RELEASE( dxBuffer_ );
		dxBuffer_ = other.dxBuffer_;
		other.dxBuffer_ = nullptr;

		return *this;
	}

	void Initialize( ID3D11Device* device, u32 dataSize, u8* initialData = nullptr )
	{
		DX_SAFE_RELEASE( dxBuffer_ );
		spadFreeAligned2( data );

		ReInitilize( device, dataSize, initialData );
	}

	void ReInitilize( ID3D11Device* device, u32 dataSize, u8* initialData = nullptr )
	{
		D3D11_BUFFER_DESC desc;
		desc.ByteWidth = dataSize;
		desc.Usage = D3D11_USAGE_DEFAULT;
		desc.BindFlags = D3D11_BIND_CONSTANT_BUFFER;
		desc.CPUAccessFlags = 0;
		desc.MiscFlags = 0;
		desc.StructureByteStride = 0;

		D3D11_SUBRESOURCE_DATA initData;
		ZeroMemory( &initData, sizeof( initData ) );
		data = reinterpret_cast<u8*>( spadMallocAligned( dataSize, SPAD_CACHELINE_SIZE ) );
		if ( initialData )
			memcpy( data, initialData, dataSize );
		else
			memset( data, 0, dataSize );
		initData.pSysMem = data;
		DXCall( device->CreateBuffer( &desc, &initData, &dxBuffer_ ) );
		size = dataSize;
	}

	void ReInitilize( ID3D11Device* device, const ConstantBuffer2& otherBuf )
	{
		ReInitilize( device, otherBuf.size, otherBuf.data );
	}

	void updateGpu( ID3D11DeviceContext* deviceContext )
	{
		SPAD_ASSERT( dxBuffer_ != nullptr );

		deviceContext->UpdateSubresource( dxBuffer_, 0, nullptr, data, 0, 0 );
	}

	ID3D11Buffer* getDxBuffer() const
	{
		return dxBuffer_;
	}

public:
	u8* data = nullptr;
	u32 size = 0;

private:
	ID3D11Buffer* dxBuffer_ = nullptr;
};


class RingBuffer
{
public:
	RingBuffer()
	{
		mappedRes_.pData = nullptr;
		mappedRes_.RowPitch = 0;
		mappedRes_.DepthPitch = 0;
	}

	~RingBuffer()
	{
		DeInitialize();
	}

	void Initialize( ID3D11Device* dxDevice, u32 size );
	void DeInitialize();

	ID3D11Buffer* getBuffer() const { return buffer_; }
	u32 getLastMapOffset() const { return lastAllocOffset_; }

	ID3D11Buffer* const * getBuffers() { return &buffer_; }
	const u32* getLastMapOffsets() const { return &lastAllocOffset_; }

	void* map( ID3D11DeviceContext* context, u32 nBytes );
	void unmap( ID3D11DeviceContext* context );

	void setVertexBuffer( ID3D11DeviceContext* context, u32 slot, u32 stride );

private:

	ID3D11Buffer* buffer_ = nullptr;
	D3D11_MAPPED_SUBRESOURCE mappedRes_;
	u32 size_ = 0;
	u32 nextFreeOffset_ = 0;
	u32 allocatedSize_ = 0;
	u32 lastAllocOffset_ = 0;
};

struct IndexBuffer
{
	ID3D11Buffer* buffer_ = nullptr;
	u32 size_ = 0;
	u32 nIndices_ = 0;
	DXGI_FORMAT format_ = DXGI_FORMAT_UNKNOWN;

	IndexBuffer( const char* _debugName )
		: debugName_( _debugName )
	{	}

	~IndexBuffer()
	{
		DeInitialize();
	}

	void Initialize( ID3D11Device* dxDevice, DXGI_FORMAT format, u32 nIndices, void* initialData );
	void DeInitialize();

private:
	std::string debugName_;
};


} // namespace spad