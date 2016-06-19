#include "Gfx_pch.h"
#include "Dx11Wrappers.h"
//#include "Dx11Util.h"
#include <Util\StdHelp.h>

namespace spad
{

RenderTarget2D::~RenderTarget2D()
{
	DeInitialize();
}

void RenderTarget2D::Initialize( ID3D11Device* device, u32 width, u32 height, DXGI_FORMAT format, u32 numMipLevels /*= 1*/, u32 multiSamples /*= 1*/, u32 msQuality /*= 0*/, u32 arraySize /*= 1*/, bool autoGenMipMaps /*= false*/, bool createUAV /*= false*/, bool cubeMap /*= false */ )
{
	FR_ASSERT2( !texture_, "RenderTarget2D already initialized" );

	D3D11_TEXTURE2D_DESC desc;
	desc.Width = width;
	desc.Height = height;
	desc.ArraySize = arraySize;
	desc.BindFlags = D3D11_BIND_SHADER_RESOURCE | D3D11_BIND_RENDER_TARGET;
	if ( createUAV )
		desc.BindFlags |= D3D11_BIND_UNORDERED_ACCESS;

	desc.CPUAccessFlags = 0;
	desc.Format = format;
	desc.MipLevels = numMipLevels;
	desc.MiscFlags = ( autoGenMipMaps && numMipLevels != 1 ) ? D3D11_RESOURCE_MISC_GENERATE_MIPS : 0;
	desc.SampleDesc.Count = multiSamples;
	desc.SampleDesc.Quality = msQuality;
	desc.Usage = D3D11_USAGE_DEFAULT;

	if ( cubeMap )
	{
		FR_ASSERT2( arraySize == 6, "array size must be 6 for cubemap rt" );
		desc.MiscFlags = D3D11_RESOURCE_MISC_TEXTURECUBE;
	}

	DXCall( device->CreateTexture2D( &desc, nullptr, &texture_ ) );

	Dx11SetDebugName3( texture_, "RenderTarget2D %s", debugName_.c_str() );

	for ( u32 i = 0; i < arraySize; ++i )
	{
		D3D11_RENDER_TARGET_VIEW_DESC rtDesc;
		rtDesc.Format = format;

		if ( arraySize == 1 )
		{
			if ( multiSamples > 1 )
			{
				rtDesc.ViewDimension = D3D11_RTV_DIMENSION_TEXTURE2DMS;
			}
			else
			{
				rtDesc.ViewDimension = D3D11_RTV_DIMENSION_TEXTURE2D;
				rtDesc.Texture2D.MipSlice = 0;
			}
		}
		else
		{
			if ( multiSamples > 1 )
			{
				rtDesc.ViewDimension = D3D11_RTV_DIMENSION_TEXTURE2DMSARRAY;
				rtDesc.Texture2DMSArray.ArraySize = 1;
				rtDesc.Texture2DMSArray.FirstArraySlice = i;
			}
			else
			{
				rtDesc.ViewDimension = D3D11_RTV_DIMENSION_TEXTURE2DARRAY;
				rtDesc.Texture2DArray.ArraySize = 1;
				rtDesc.Texture2DArray.FirstArraySlice = i;
				rtDesc.Texture2DArray.MipSlice = 0;
			}
		}

		ID3D11RenderTargetView* rtView;
		DXCall( device->CreateRenderTargetView( texture_, &rtDesc, &rtView ) );

		Dx11SetDebugName3( rtView, "RenderTarget2D %s, RTV(%u)", debugName_.c_str(), i );
		rtvArraySlices_.push_back( rtView );
	}

	rtv_ = rtvArraySlices_[0];
	rtv_->AddRef();

	DXCall( device->CreateShaderResourceView( texture_, nullptr, &srv_ ) );
	Dx11SetDebugName3( srv_, "RenderTarget2D %s, SRV", debugName_.c_str() );

	for ( u32 i = 0; i < arraySize; ++i )
	{
		D3D11_SHADER_RESOURCE_VIEW_DESC srvDesc;
		srvDesc.Format = format;

		if ( arraySize == 1 )
		{
			if ( multiSamples > 1 )
			{
				srvDesc.ViewDimension = D3D11_SRV_DIMENSION_TEXTURE2DMS;
			}
			else
			{
				srvDesc.ViewDimension = D3D11_SRV_DIMENSION_TEXTURE2D;
				srvDesc.Texture2D.MipLevels = (UINT)-1;
				srvDesc.Texture2D.MostDetailedMip = 0;
			}
		}
		else
		{
			if ( multiSamples > 1 )
			{
				srvDesc.ViewDimension = D3D11_SRV_DIMENSION_TEXTURE2DMSARRAY;
				srvDesc.Texture2DMSArray.ArraySize = 1;
				srvDesc.Texture2DMSArray.FirstArraySlice = i;
			}
			else
			{
				srvDesc.ViewDimension = D3D11_SRV_DIMENSION_TEXTURE2DARRAY;
				srvDesc.Texture2DArray.ArraySize = 1;
				srvDesc.Texture2DArray.FirstArraySlice = i;
				srvDesc.Texture2DArray.MipLevels = (UINT)-1;
				srvDesc.Texture2DArray.MostDetailedMip = 0;
			}
		}

		ID3D11ShaderResourceView* srView;
		DXCall( device->CreateShaderResourceView( texture_, &srvDesc, &srView ) );
		Dx11SetDebugName3( srView, "RenderTarget2D %s, SRV(%u)", debugName_.c_str(), i );
		srvArraySlices_.push_back( srView );
	}

	this->width_ = width;
	this->height_ = height;
	this->numMipLevels_ = numMipLevels;
	this->arraySize_ = arraySize;
	this->format_ = format;
	this->multiSamples_ = multiSamples;
	this->msQuality_ = msQuality;
	this->autoGenMipMaps_ = autoGenMipMaps;
	this->cubeMap_ = cubeMap;

	if ( createUAV )
	{
		DXCall( device->CreateUnorderedAccessView( texture_, nullptr, &uav_ ) );
		Dx11SetDebugName3( uav_, "RenderTarget2D %s, UAV", debugName_.c_str() );
	}
}

void RenderTarget2D::DeInitialize()
{
	DX_SAFE_RELEASE( texture_ );
	DX_SAFE_RELEASE( rtv_ );
	DX_SAFE_RELEASE( srv_ );
	DX_SAFE_RELEASE( uav_ );

	spad::clear_cont( rtvArraySlices_, [&]( ID3D11RenderTargetView* h ) { h->Release(); } );
	spad::clear_cont( srvArraySlices_, [&]( ID3D11ShaderResourceView* h ) { h->Release(); } );

	width_ = 0;
	height_ = 0;
	numMipLevels_ = 0;
	multiSamples_ = 0;
	msQuality_ = 0;
	format_ = DXGI_FORMAT_UNKNOWN;
	arraySize_ = 0;
	autoGenMipMaps_ = false;
	cubeMap_ = false;
}


DepthStencil::~DepthStencil()
{
	DeInitialize();
}

void DepthStencil::Initialize( ID3D11Device* device, u32 width, u32 height, u32 arraySize /*= 1*/, DXGI_FORMAT format /*= DXGI_FORMAT_D24_UNORM_S8_UINT*/, u32 multiSamples /*= 1*/, u32 msQuality /*= 0*/, bool useAsShaderResource /*= false */ )
{
	u32 bindFlags = D3D11_BIND_DEPTH_STENCIL;
	if ( useAsShaderResource )
		bindFlags |= D3D11_BIND_SHADER_RESOURCE;

	DXGI_FORMAT dsTexFormat;
	if ( !useAsShaderResource )
		dsTexFormat = format;
	else if ( format == DXGI_FORMAT_D16_UNORM )
		dsTexFormat = DXGI_FORMAT_R16_TYPELESS;
	else if ( format == DXGI_FORMAT_D24_UNORM_S8_UINT )
		dsTexFormat = DXGI_FORMAT_R24G8_TYPELESS;
	else
		dsTexFormat = DXGI_FORMAT_R32_TYPELESS;

	D3D11_TEXTURE2D_DESC desc;
	desc.Width = width;
	desc.Height = height;
	desc.ArraySize = arraySize;
	desc.BindFlags = bindFlags;
	desc.CPUAccessFlags = 0;
	desc.Format = dsTexFormat;
	desc.MipLevels = 1;
	desc.MiscFlags = 0;
	desc.SampleDesc.Count = multiSamples;
	desc.SampleDesc.Quality = msQuality;
	desc.Usage = D3D11_USAGE_DEFAULT;
	DXCall( device->CreateTexture2D( &desc, nullptr, &texture_ ) );
	Dx11SetDebugName3( texture_, "DepthStencil %s", debugName_.c_str() );

	for ( u32 i = 0; i < arraySize; ++i )
	{
		D3D11_DEPTH_STENCIL_VIEW_DESC dsvDesc;
		dsvDesc.Format = format;

		if ( arraySize == 1 )
		{
			dsvDesc.ViewDimension = multiSamples > 1 ? D3D11_DSV_DIMENSION_TEXTURE2DMS : D3D11_DSV_DIMENSION_TEXTURE2D;
			dsvDesc.Texture2D.MipSlice = 0;
		}
		else
		{
			if ( multiSamples > 1 )
			{
				dsvDesc.ViewDimension = D3D11_DSV_DIMENSION_TEXTURE2DMSARRAY;
				dsvDesc.Texture2DMSArray.ArraySize = 1;
				dsvDesc.Texture2DMSArray.FirstArraySlice = i;
			}
			else
			{
				dsvDesc.ViewDimension = D3D11_DSV_DIMENSION_TEXTURE2DARRAY;
				dsvDesc.Texture2DArray.ArraySize = 1;
				dsvDesc.Texture2DArray.FirstArraySlice = i;
				dsvDesc.Texture2DArray.MipSlice = 0;
			}
		}

		dsvDesc.Flags = 0;
		ID3D11DepthStencilView* dsView;
		DXCall( device->CreateDepthStencilView( texture_, &dsvDesc, &dsView ) );
		Dx11SetDebugName3( dsView, "DepthStencil %s DSV", debugName_.c_str() );
		dsvArraySlices_.push_back( dsView );

		//if ( i == 0 )
		//{
		//	// Also create a read-only DSV
		//	dsvDesc.Flags = D3D11_DSV_READ_ONLY_DEPTH;
		//	if ( format == DXGI_FORMAT_D24_UNORM_S8_UINT || format == DXGI_FORMAT_D32_FLOAT_S8X24_UINT )
		//		dsvDesc.Flags |= D3D11_DSV_READ_ONLY_STENCIL;
		//	DXCall( device->CreateDepthStencilView( texture, &dsvDesc, &dsvReadOnly ) );
		//	Dx11SetDebugName3( dsvReadOnly, "DepthStencil %s DSV ReadOnly", debugName.c_str() );
		//	dsvDesc.Flags = 0;
		//}
	}

	dsv_ = dsvArraySlices_[0];
	dsv_->AddRef();

	if ( useAsShaderResource )
	{
		DXGI_FORMAT dsSRVFormat;
		if ( format == DXGI_FORMAT_D16_UNORM )
			dsSRVFormat = DXGI_FORMAT_R16_UNORM;
		else if ( format == DXGI_FORMAT_D24_UNORM_S8_UINT )
			dsSRVFormat = DXGI_FORMAT_R24_UNORM_X8_TYPELESS;
		else
			dsSRVFormat = DXGI_FORMAT_R32_FLOAT;

		D3D11_SHADER_RESOURCE_VIEW_DESC srvDesc;
		srvDesc.Format = dsSRVFormat;

		if ( arraySize == 1 )
		{
			srvDesc.ViewDimension = multiSamples > 1 ? D3D11_SRV_DIMENSION_TEXTURE2DMS : D3D11_SRV_DIMENSION_TEXTURE2D;
			srvDesc.Texture2D.MipLevels = 1;
			srvDesc.Texture2D.MostDetailedMip = 0;
		}
		else
		{
			srvDesc.ViewDimension = multiSamples > 1 ? D3D11_SRV_DIMENSION_TEXTURE2DMSARRAY : D3D11_SRV_DIMENSION_TEXTURE2DARRAY;
			srvDesc.Texture2DArray.ArraySize = arraySize;
			srvDesc.Texture2DArray.FirstArraySlice = 0;
			srvDesc.Texture2DArray.MipLevels = 1;
			srvDesc.Texture2DArray.MostDetailedMip = 0;
		}

		DXCall( device->CreateShaderResourceView( texture_, &srvDesc, &srv_ ) );
		Dx11SetDebugName3( dsvReadOnly_, "DepthStencil %s SRV", debugName_.c_str() );
	}

	this->width_ = width;
	this->height_ = height;
	this->arraySize_ = arraySize;
	this->format_ = format;
	this->multiSamples_ = multiSamples;
	this->msQuality_ = msQuality;
}

void DepthStencil::DeInitialize()
{
	DX_SAFE_RELEASE( texture_ );
	DX_SAFE_RELEASE( dsv_ );
	DX_SAFE_RELEASE( dsvReadOnly_ );
	DX_SAFE_RELEASE( srv_ );

	spad::clear_cont( dsvArraySlices_, [&]( ID3D11DepthStencilView* h ) { h->Release(); } );

	width_ = 0;
	height_ = 0;
	arraySize_ = 0;
	format_ = DXGI_FORMAT_UNKNOWN;
	multiSamples_ = 0;
	msQuality_ = 0;
}


void RingBuffer::Initialize( ID3D11Device* dxDevice, u32 size )
{
	size_ = size;
	nextFreeOffset_ = 0;
	allocatedSize_ = 0;

	D3D11_BUFFER_DESC bd;
	ZeroMemory( &bd, sizeof( bd ) );
	bd.Usage = D3D11_USAGE_DYNAMIC;
	bd.ByteWidth = size_;
	bd.BindFlags = D3D11_BIND_VERTEX_BUFFER;
	bd.CPUAccessFlags = D3D11_CPU_ACCESS_WRITE;
	DXCall( dxDevice->CreateBuffer( &bd, NULL, &buffer_ ) );
}

void RingBuffer::DeInitialize()
{
	FR_ASSERT( !allocatedSize_ );

	DX_SAFE_RELEASE( buffer_ );
	size_ = 0;
	nextFreeOffset_ = 0;
	allocatedSize_ = 0;
}

void* RingBuffer::map( ID3D11DeviceContext* context, u32 nBytes )
{
	// make sure previous allocation finished
	FR_ASSERT( !allocatedSize_ && nBytes && nBytes <= size_ );

	void* ptr = NULL;
	if ( size_ >= nextFreeOffset_ + nBytes )
	{
		DXCall( context->Map( buffer_, 0, D3D11_MAP_WRITE_NO_OVERWRITE, 0, &mappedRes_ ) );
		ptr = reinterpret_cast<u8*>( mappedRes_.pData ) + nextFreeOffset_;
	}
	else
	{
		DXCall( context->Map( buffer_, 0, D3D11_MAP_WRITE_DISCARD, 0, &mappedRes_ ) );
		ptr = mappedRes_.pData;
		nextFreeOffset_ = 0;
	}

	allocatedSize_ = nBytes;
	return ptr;
}

void RingBuffer::unmap( ID3D11DeviceContext* context )
{
	FR_ASSERT( allocatedSize_ );
	lastAllocOffset_ = nextFreeOffset_;
	nextFreeOffset_ += allocatedSize_;
	allocatedSize_ = 0;
	nextFreeOffset_ = frAlignU32_2( nextFreeOffset_, 256 );
	mappedRes_.pData = nullptr;
	mappedRes_.RowPitch = 0;
	mappedRes_.DepthPitch = 0;
	context->Unmap( buffer_, 0 );
}

void RingBuffer::setVertexBuffer( ID3D11DeviceContext* context, u32 slot, u32 stride )
{
	context->IASetVertexBuffers( slot, 1, &buffer_, &stride, &lastAllocOffset_ );
}


void IndexBuffer::Initialize( ID3D11Device* dxDevice, DXGI_FORMAT format, u32 nIndices, void* initialData )
{
	FR_ASSERT( format == DXGI_FORMAT_R16_UINT || format == DXGI_FORMAT_R32_UINT );
	D3D11_BUFFER_DESC bd;
	u32 indexSize = format == DXGI_FORMAT_R16_UINT ? 2 : 4;
	bd.ByteWidth = nIndices * indexSize;
	bd.Usage = D3D11_USAGE_IMMUTABLE;
	bd.BindFlags = D3D11_BIND_INDEX_BUFFER;
	bd.CPUAccessFlags = 0;
	bd.MiscFlags = 0;
	bd.StructureByteStride = 0;

	D3D11_SUBRESOURCE_DATA initData;
	initData.pSysMem = initialData;
	initData.SysMemPitch = 0;
	initData.SysMemSlicePitch = 0;

	DXCall( dxDevice->CreateBuffer( &bd, &initData, &buffer_ ) );
	Dx11SetDebugName3( buffer_, "IndexBuffer %s", debugName_.c_str() );

	size_ = bd.ByteWidth;
	nIndices_ = nIndices;
	format_ = format;
}

void IndexBuffer::DeInitialize()
{
	DX_SAFE_RELEASE( buffer_ );
	size_ = 0;
	nIndices_ = 0;
	format_ = DXGI_FORMAT_UNKNOWN;
}

} // namespace spad
