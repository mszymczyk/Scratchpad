#include "Gfx_pch.h"

#include "Dx11DeviceStates.h"

namespace spad
{
	ID3D11BlendState* BlendStates::blendDisabled;
	ID3D11BlendState* BlendStates::additiveBlend;
	ID3D11BlendState* BlendStates::alphaBlend;
	ID3D11BlendState* BlendStates::pmAlphaBlend;
	ID3D11BlendState* BlendStates::noColor;
	ID3D11BlendState* BlendStates::alphaToCoverage;
	ID3D11BlendState* BlendStates::opacityBlend;

	// create it through wrapper func to make compiler happy
	//
	void CreateBlendStateWrap( ID3D11Device* device, const D3D11_BLEND_DESC& desc, ID3D11BlendState** ss )
	{
		DXCall( device->CreateBlendState( &desc, ss ) );
	}

void BlendStates::Initialize(ID3D11Device* device)
{
	CreateBlendStateWrap( device, BlendDisabledDesc(), &blendDisabled );
	CreateBlendStateWrap( device, AdditiveBlendDesc(), &additiveBlend );
	CreateBlendStateWrap( device, AlphaBlendDesc(), &alphaBlend );
	CreateBlendStateWrap( device, PreMultipliedAlphaBlendDesc(), &pmAlphaBlend );
	CreateBlendStateWrap( device, ColorWriteDisabledDesc(), &noColor );
	CreateBlendStateWrap( device, AlphaToCoverageDesc(), &alphaToCoverage );
	CreateBlendStateWrap( device, OpacityBlendDesc(), &opacityBlend );
}

void BlendStates::DeInitialize()
{
	DX_SAFE_RELEASE( blendDisabled );
	DX_SAFE_RELEASE( additiveBlend );
	DX_SAFE_RELEASE( alphaBlend );
	DX_SAFE_RELEASE( pmAlphaBlend );

	DX_SAFE_RELEASE( noColor );
	DX_SAFE_RELEASE( alphaToCoverage );
	DX_SAFE_RELEASE( opacityBlend );
}

D3D11_BLEND_DESC BlendStates::BlendDisabledDesc()
{
    D3D11_BLEND_DESC blendDesc;
    blendDesc.AlphaToCoverageEnable = false;
    blendDesc.IndependentBlendEnable = false;
    for(UINT i = 0; i < 8; ++i)
    {
        blendDesc.RenderTarget[i].BlendEnable = false;
        blendDesc.RenderTarget[i].BlendOp = D3D11_BLEND_OP_ADD;
        blendDesc.RenderTarget[i].BlendOpAlpha = D3D11_BLEND_OP_ADD;
        blendDesc.RenderTarget[i].DestBlend = D3D11_BLEND_INV_SRC_ALPHA;
        blendDesc.RenderTarget[i].DestBlendAlpha = D3D11_BLEND_ONE;
        blendDesc.RenderTarget[i].RenderTargetWriteMask = D3D11_COLOR_WRITE_ENABLE_ALL;
        blendDesc.RenderTarget[i].SrcBlend = D3D11_BLEND_SRC_ALPHA;
        blendDesc.RenderTarget[i].SrcBlendAlpha = D3D11_BLEND_ONE;
    }

    return blendDesc;
}

D3D11_BLEND_DESC BlendStates::AdditiveBlendDesc()
{
    D3D11_BLEND_DESC blendDesc;
    blendDesc.AlphaToCoverageEnable = false;
    blendDesc.IndependentBlendEnable = false;
    for (u32 i = 0; i < 8; ++i)
    {
        blendDesc.RenderTarget[i].BlendEnable = true;
        blendDesc.RenderTarget[i].BlendOp = D3D11_BLEND_OP_ADD;
        blendDesc.RenderTarget[i].BlendOpAlpha = D3D11_BLEND_OP_ADD;
        blendDesc.RenderTarget[i].DestBlend = D3D11_BLEND_ONE;
        blendDesc.RenderTarget[i].DestBlendAlpha = D3D11_BLEND_ONE;
        blendDesc.RenderTarget[i].RenderTargetWriteMask = D3D11_COLOR_WRITE_ENABLE_ALL;
        blendDesc.RenderTarget[i].SrcBlend = D3D11_BLEND_ONE;
        blendDesc.RenderTarget[i].SrcBlendAlpha = D3D11_BLEND_ONE;
    }

    return blendDesc;
}

D3D11_BLEND_DESC BlendStates::AlphaBlendDesc()
{
    D3D11_BLEND_DESC blendDesc;
    blendDesc.AlphaToCoverageEnable = false;
    blendDesc.IndependentBlendEnable = true;
    for (u32 i = 0; i < 8; ++i)
    {
        blendDesc.RenderTarget[i].BlendEnable = true;
        blendDesc.RenderTarget[i].BlendOp = D3D11_BLEND_OP_ADD;
        blendDesc.RenderTarget[i].BlendOpAlpha = D3D11_BLEND_OP_ADD;
        blendDesc.RenderTarget[i].DestBlend = D3D11_BLEND_INV_SRC_ALPHA;
        blendDesc.RenderTarget[i].DestBlendAlpha = D3D11_BLEND_ONE;
        blendDesc.RenderTarget[i].RenderTargetWriteMask = D3D11_COLOR_WRITE_ENABLE_ALL;
        blendDesc.RenderTarget[i].SrcBlend = D3D11_BLEND_SRC_ALPHA;
        blendDesc.RenderTarget[i].SrcBlendAlpha = D3D11_BLEND_ONE;
    }

    blendDesc.RenderTarget[0].BlendEnable = true;

    return blendDesc;
}

D3D11_BLEND_DESC BlendStates::PreMultipliedAlphaBlendDesc()
{
    D3D11_BLEND_DESC blendDesc;
    blendDesc.AlphaToCoverageEnable = false;
    blendDesc.IndependentBlendEnable = false;
    for (u32 i = 0; i < 8; ++i)
    {
        blendDesc.RenderTarget[i].BlendEnable = false;
        blendDesc.RenderTarget[i].BlendOp = D3D11_BLEND_OP_ADD;
        blendDesc.RenderTarget[i].BlendOpAlpha = D3D11_BLEND_OP_ADD;
        blendDesc.RenderTarget[i].DestBlend = D3D11_BLEND_INV_SRC_ALPHA;
        blendDesc.RenderTarget[i].DestBlendAlpha = D3D11_BLEND_ONE;
        blendDesc.RenderTarget[i].RenderTargetWriteMask = D3D11_COLOR_WRITE_ENABLE_ALL;
        blendDesc.RenderTarget[i].SrcBlend = D3D11_BLEND_ONE;
        blendDesc.RenderTarget[i].SrcBlendAlpha = D3D11_BLEND_ONE;
    }

    return blendDesc;
}

D3D11_BLEND_DESC BlendStates::ColorWriteDisabledDesc()
{
    D3D11_BLEND_DESC blendDesc;
    blendDesc.AlphaToCoverageEnable = false;
    blendDesc.IndependentBlendEnable = false;
    for (u32 i = 0; i < 8; ++i)
    {
        blendDesc.RenderTarget[i].BlendEnable = false;
        blendDesc.RenderTarget[i].BlendOp = D3D11_BLEND_OP_ADD;
        blendDesc.RenderTarget[i].BlendOpAlpha = D3D11_BLEND_OP_ADD;
        blendDesc.RenderTarget[i].DestBlend = D3D11_BLEND_INV_SRC_ALPHA;
        blendDesc.RenderTarget[i].DestBlendAlpha = D3D11_BLEND_ONE;
        blendDesc.RenderTarget[i].RenderTargetWriteMask = 0;
        blendDesc.RenderTarget[i].SrcBlend = D3D11_BLEND_SRC_ALPHA;
        blendDesc.RenderTarget[i].SrcBlendAlpha = D3D11_BLEND_ONE;
    }

    return blendDesc;
}

D3D11_BLEND_DESC BlendStates::AlphaToCoverageDesc()
{
    D3D11_BLEND_DESC blendDesc;
    blendDesc.AlphaToCoverageEnable = true;
    blendDesc.IndependentBlendEnable = false;
    for (u32 i = 0; i < 8; ++i)
    {
        blendDesc.RenderTarget[i].BlendEnable = false;
        blendDesc.RenderTarget[i].BlendOp = D3D11_BLEND_OP_ADD;
        blendDesc.RenderTarget[i].BlendOpAlpha = D3D11_BLEND_OP_ADD;
        blendDesc.RenderTarget[i].DestBlend = D3D11_BLEND_INV_SRC_ALPHA;
        blendDesc.RenderTarget[i].DestBlendAlpha = D3D11_BLEND_ONE;
        blendDesc.RenderTarget[i].RenderTargetWriteMask = D3D11_COLOR_WRITE_ENABLE_ALL;
        blendDesc.RenderTarget[i].SrcBlend = D3D11_BLEND_SRC_ALPHA;
        blendDesc.RenderTarget[i].SrcBlendAlpha = D3D11_BLEND_ONE;
    }

    return blendDesc;
}

D3D11_BLEND_DESC BlendStates::OpacityBlendDesc()
{
    D3D11_BLEND_DESC blendDesc;
    blendDesc.AlphaToCoverageEnable = false;
    blendDesc.IndependentBlendEnable = false;
    for (u32 i = 0; i < 8; ++i)
    {
        blendDesc.RenderTarget[i].BlendEnable = true;
		blendDesc.RenderTarget[i].SrcBlend = D3D11_BLEND_ONE;
		blendDesc.RenderTarget[i].DestBlend = D3D11_BLEND_INV_SRC1_COLOR;
		blendDesc.RenderTarget[i].BlendOp = D3D11_BLEND_OP_ADD;
		blendDesc.RenderTarget[i].SrcBlendAlpha = D3D11_BLEND_ONE;
		blendDesc.RenderTarget[i].DestBlendAlpha = D3D11_BLEND_ONE;
		blendDesc.RenderTarget[i].BlendOpAlpha = D3D11_BLEND_OP_ADD;
        blendDesc.RenderTarget[i].RenderTargetWriteMask = D3D11_COLOR_WRITE_ENABLE_ALL;
    }

    return blendDesc;
}

ID3D11RasterizerState* RasterizerStates::noCull;
ID3D11RasterizerState* RasterizerStates::cullBackFaces;
ID3D11RasterizerState* RasterizerStates::cullBackFacesScissor;
ID3D11RasterizerState* RasterizerStates::cullFrontFaces;
ID3D11RasterizerState* RasterizerStates::cullFrontFacesScissor;
ID3D11RasterizerState* RasterizerStates::noCullNoMS;
ID3D11RasterizerState* RasterizerStates::noCullScissor;
ID3D11RasterizerState* RasterizerStates::wireframe;

// create it through wrapper func to make compiler happy
//
void CreateRasterizerStateWrap( ID3D11Device* device, const D3D11_RASTERIZER_DESC& desc, ID3D11RasterizerState** rs )
{
	DXCall( device->CreateRasterizerState( &desc, rs ) );
}

void RasterizerStates::Initialize(ID3D11Device* device)
{
    //DXCall(device->CreateRasterizerState(&NoCullDesc(), &noCull));
	CreateRasterizerStateWrap( device, NoCullDesc(), &noCull );
	CreateRasterizerStateWrap( device, FrontFaceCullDesc(), &cullFrontFaces );
	CreateRasterizerStateWrap( device, FrontFaceCullScissorDesc(), &cullFrontFacesScissor );
	CreateRasterizerStateWrap( device, BackFaceCullDesc(), &cullBackFaces );

	CreateRasterizerStateWrap( device, BackFaceCullScissorDesc(), &cullBackFacesScissor );
	CreateRasterizerStateWrap( device, NoCullNoMSDesc(), &noCullNoMS );
	CreateRasterizerStateWrap( device, NoCullScissorDesc(), &noCullScissor );
	CreateRasterizerStateWrap( device, WireframeDesc(), &wireframe );
}

void RasterizerStates::DeInitialize()
{
	DX_SAFE_RELEASE( noCull );
	DX_SAFE_RELEASE( cullFrontFaces );
	DX_SAFE_RELEASE( cullFrontFacesScissor );
	DX_SAFE_RELEASE( cullBackFaces );

	DX_SAFE_RELEASE( cullBackFacesScissor );
	DX_SAFE_RELEASE( noCullNoMS );
	DX_SAFE_RELEASE( noCullScissor );
	DX_SAFE_RELEASE( wireframe );
}

D3D11_RASTERIZER_DESC RasterizerStates::NoCullDesc()
{
    D3D11_RASTERIZER_DESC rastDesc;

    rastDesc.AntialiasedLineEnable = false;
    rastDesc.CullMode = D3D11_CULL_NONE;
    rastDesc.DepthBias = 0;
    rastDesc.DepthBiasClamp = 0.0f;
    rastDesc.DepthClipEnable = true;
    rastDesc.FillMode = D3D11_FILL_SOLID;
    rastDesc.FrontCounterClockwise = true;
    rastDesc.MultisampleEnable = true;
    rastDesc.ScissorEnable = false;
    rastDesc.SlopeScaledDepthBias = 0;

    return rastDesc;
}

D3D11_RASTERIZER_DESC RasterizerStates::FrontFaceCullDesc()
{
    D3D11_RASTERIZER_DESC rastDesc;

    rastDesc.AntialiasedLineEnable = false;
    rastDesc.CullMode = D3D11_CULL_FRONT;
    rastDesc.DepthBias = 0;
    rastDesc.DepthBiasClamp = 0.0f;
    rastDesc.DepthClipEnable = true;
    rastDesc.FillMode = D3D11_FILL_SOLID;
	rastDesc.FrontCounterClockwise = true;
    rastDesc.MultisampleEnable = true;
    rastDesc.ScissorEnable = false;
    rastDesc.SlopeScaledDepthBias = 0;

    return rastDesc;
}

D3D11_RASTERIZER_DESC RasterizerStates::FrontFaceCullScissorDesc()
{
    D3D11_RASTERIZER_DESC rastDesc;

    rastDesc.AntialiasedLineEnable = false;
    rastDesc.CullMode = D3D11_CULL_FRONT;
    rastDesc.DepthBias = 0;
    rastDesc.DepthBiasClamp = 0.0f;
    rastDesc.DepthClipEnable = true;
    rastDesc.FillMode = D3D11_FILL_SOLID;
    rastDesc.FrontCounterClockwise = true;
    rastDesc.MultisampleEnable = true;
    rastDesc.ScissorEnable = true;
    rastDesc.SlopeScaledDepthBias = 0;

    return rastDesc;
}

D3D11_RASTERIZER_DESC RasterizerStates::BackFaceCullDesc()
{
    D3D11_RASTERIZER_DESC rastDesc;

    rastDesc.AntialiasedLineEnable = false;
    rastDesc.CullMode = D3D11_CULL_BACK;
    rastDesc.DepthBias = 0;
    rastDesc.DepthBiasClamp = 0.0f;
    rastDesc.DepthClipEnable = true;
    rastDesc.FillMode = D3D11_FILL_SOLID;
	rastDesc.FrontCounterClockwise = true;
    rastDesc.MultisampleEnable = true;
    rastDesc.ScissorEnable = false;
    rastDesc.SlopeScaledDepthBias = 0;

    return rastDesc;
}

D3D11_RASTERIZER_DESC RasterizerStates::BackFaceCullScissorDesc()
{
    D3D11_RASTERIZER_DESC rastDesc;

    rastDesc.AntialiasedLineEnable = false;
    rastDesc.CullMode = D3D11_CULL_BACK;
    rastDesc.DepthBias = 0;
    rastDesc.DepthBiasClamp = 0.0f;
    rastDesc.DepthClipEnable = true;
    rastDesc.FillMode = D3D11_FILL_SOLID;
    rastDesc.FrontCounterClockwise = true;
    rastDesc.MultisampleEnable = true;
    rastDesc.ScissorEnable = true;
    rastDesc.SlopeScaledDepthBias = 0;

    return rastDesc;
}

D3D11_RASTERIZER_DESC RasterizerStates::NoCullNoMSDesc()
{
    D3D11_RASTERIZER_DESC rastDesc;

    rastDesc.AntialiasedLineEnable = false;
    rastDesc.CullMode = D3D11_CULL_NONE;
    rastDesc.DepthBias = 0;
    rastDesc.DepthBiasClamp = 0.0f;
    rastDesc.DepthClipEnable = true;
    rastDesc.FillMode = D3D11_FILL_SOLID;
    rastDesc.FrontCounterClockwise = true;
    rastDesc.MultisampleEnable = false;
    rastDesc.ScissorEnable = false;
    rastDesc.SlopeScaledDepthBias = 0;

    return rastDesc;
}

D3D11_RASTERIZER_DESC RasterizerStates::NoCullScissorDesc()
{
    D3D11_RASTERIZER_DESC rastDesc;

    rastDesc.AntialiasedLineEnable = false;
    rastDesc.CullMode = D3D11_CULL_NONE;
    rastDesc.DepthBias = 0;
    rastDesc.DepthBiasClamp = 0.0f;
    rastDesc.DepthClipEnable = true;
    rastDesc.FillMode = D3D11_FILL_SOLID;
    rastDesc.FrontCounterClockwise = true;
    rastDesc.MultisampleEnable = true;
    rastDesc.ScissorEnable = true;
    rastDesc.SlopeScaledDepthBias = 0;

    return rastDesc;
}

D3D11_RASTERIZER_DESC RasterizerStates::WireframeDesc()
{
    D3D11_RASTERIZER_DESC rastDesc;

    rastDesc.AntialiasedLineEnable = false;
    rastDesc.CullMode = D3D11_CULL_NONE;
    rastDesc.DepthBias = 0;
    rastDesc.DepthBiasClamp = 0.0f;
    rastDesc.DepthClipEnable = true;
    rastDesc.FillMode = D3D11_FILL_WIREFRAME;
    rastDesc.FrontCounterClockwise = true;
    rastDesc.MultisampleEnable = true;
    rastDesc.ScissorEnable = false;
    rastDesc.SlopeScaledDepthBias = 0;

    return rastDesc;
}

ID3D11DepthStencilState* DepthStencilStates::depthDisabled;
ID3D11DepthStencilState* DepthStencilStates::depthEnabled;
ID3D11DepthStencilState* DepthStencilStates::revDepthEnabled;
ID3D11DepthStencilState* DepthStencilStates::depthWriteEnabled;
ID3D11DepthStencilState* DepthStencilStates::revDepthWriteEnabled;
ID3D11DepthStencilState* DepthStencilStates::depthStencilWriteEnabled;
ID3D11DepthStencilState* DepthStencilStates::stencilEnabled;

// create it through wrapper func to make compiler happy
//
void CreateDepthStencilStateWrap( ID3D11Device* device, const D3D11_DEPTH_STENCIL_DESC& desc, ID3D11DepthStencilState** dss )
{
	DXCall( device->CreateDepthStencilState( &desc, dss ) );
}

void DepthStencilStates::Initialize(ID3D11Device* device)
{
	CreateDepthStencilStateWrap( device, DepthDisabledDesc(), &depthDisabled );
	CreateDepthStencilStateWrap( device, DepthEnabledDesc(), &depthEnabled );
	CreateDepthStencilStateWrap( device, ReverseDepthEnabledDesc(), &revDepthEnabled );
	CreateDepthStencilStateWrap( device, DepthWriteEnabledDesc(), &depthWriteEnabled );
	CreateDepthStencilStateWrap( device, ReverseDepthWriteEnabledDesc(), &revDepthWriteEnabled );
	CreateDepthStencilStateWrap( device, DepthStencilWriteEnabledDesc(), &depthStencilWriteEnabled );
	CreateDepthStencilStateWrap( device, StencilEnabledDesc(), &stencilEnabled );
}

void DepthStencilStates::DeInitialize()
{
	DX_SAFE_RELEASE( depthDisabled );
	DX_SAFE_RELEASE( depthEnabled );
	DX_SAFE_RELEASE( revDepthEnabled );
	DX_SAFE_RELEASE( depthWriteEnabled );

	DX_SAFE_RELEASE( revDepthWriteEnabled );
	DX_SAFE_RELEASE( depthStencilWriteEnabled );
	DX_SAFE_RELEASE( stencilEnabled );
}

D3D11_DEPTH_STENCIL_DESC DepthStencilStates::DepthDisabledDesc()
{
    D3D11_DEPTH_STENCIL_DESC dsDesc;
    dsDesc.DepthEnable = false;
    dsDesc.DepthWriteMask = D3D11_DEPTH_WRITE_MASK_ZERO;
    dsDesc.DepthFunc = D3D11_COMPARISON_LESS_EQUAL;
    dsDesc.StencilEnable = false;
    dsDesc.StencilReadMask = D3D11_DEFAULT_STENCIL_READ_MASK;
    dsDesc.StencilWriteMask = D3D11_DEFAULT_STENCIL_WRITE_MASK;
    dsDesc.FrontFace.StencilDepthFailOp = D3D11_STENCIL_OP_KEEP;
    dsDesc.FrontFace.StencilFailOp = D3D11_STENCIL_OP_KEEP;
    dsDesc.FrontFace.StencilPassOp = D3D11_STENCIL_OP_REPLACE;
    dsDesc.FrontFace.StencilFunc = D3D11_COMPARISON_ALWAYS;
    dsDesc.BackFace = dsDesc.FrontFace;

    return dsDesc;
}

D3D11_DEPTH_STENCIL_DESC DepthStencilStates::DepthEnabledDesc()
{
    D3D11_DEPTH_STENCIL_DESC dsDesc;
    dsDesc.DepthEnable = true;
    dsDesc.DepthWriteMask = D3D11_DEPTH_WRITE_MASK_ZERO;
    dsDesc.DepthFunc = D3D11_COMPARISON_LESS_EQUAL;
    dsDesc.StencilEnable = false;
    dsDesc.StencilReadMask = D3D11_DEFAULT_STENCIL_READ_MASK;
    dsDesc.StencilWriteMask = D3D11_DEFAULT_STENCIL_WRITE_MASK;
    dsDesc.FrontFace.StencilDepthFailOp = D3D11_STENCIL_OP_KEEP;
    dsDesc.FrontFace.StencilFailOp = D3D11_STENCIL_OP_KEEP;
    dsDesc.FrontFace.StencilPassOp = D3D11_STENCIL_OP_REPLACE;
    dsDesc.FrontFace.StencilFunc = D3D11_COMPARISON_ALWAYS;
    dsDesc.BackFace = dsDesc.FrontFace;

    return dsDesc;
}

D3D11_DEPTH_STENCIL_DESC DepthStencilStates::ReverseDepthEnabledDesc()
{
    D3D11_DEPTH_STENCIL_DESC dsDesc;
    dsDesc.DepthEnable = true;
    dsDesc.DepthWriteMask = D3D11_DEPTH_WRITE_MASK_ZERO;
    dsDesc.DepthFunc = D3D11_COMPARISON_GREATER_EQUAL;
    dsDesc.StencilEnable = false;
    dsDesc.StencilReadMask = D3D11_DEFAULT_STENCIL_READ_MASK;
    dsDesc.StencilWriteMask = D3D11_DEFAULT_STENCIL_WRITE_MASK;
    dsDesc.FrontFace.StencilDepthFailOp = D3D11_STENCIL_OP_KEEP;
    dsDesc.FrontFace.StencilFailOp = D3D11_STENCIL_OP_KEEP;
    dsDesc.FrontFace.StencilPassOp = D3D11_STENCIL_OP_REPLACE;
    dsDesc.FrontFace.StencilFunc = D3D11_COMPARISON_ALWAYS;
    dsDesc.BackFace = dsDesc.FrontFace;

    return dsDesc;
}

D3D11_DEPTH_STENCIL_DESC DepthStencilStates::DepthWriteEnabledDesc()
{
    D3D11_DEPTH_STENCIL_DESC dsDesc;
    dsDesc.DepthEnable = true;
    dsDesc.DepthWriteMask = D3D11_DEPTH_WRITE_MASK_ALL;
    dsDesc.DepthFunc = D3D11_COMPARISON_LESS_EQUAL;
    dsDesc.StencilEnable = false;
    dsDesc.StencilReadMask = D3D11_DEFAULT_STENCIL_READ_MASK;
    dsDesc.StencilWriteMask = D3D11_DEFAULT_STENCIL_WRITE_MASK;
    dsDesc.FrontFace.StencilDepthFailOp = D3D11_STENCIL_OP_KEEP;
    dsDesc.FrontFace.StencilFailOp = D3D11_STENCIL_OP_KEEP;
    dsDesc.FrontFace.StencilPassOp = D3D11_STENCIL_OP_REPLACE;
    dsDesc.FrontFace.StencilFunc = D3D11_COMPARISON_ALWAYS;
    dsDesc.BackFace = dsDesc.FrontFace;

    return dsDesc;
}

D3D11_DEPTH_STENCIL_DESC DepthStencilStates::ReverseDepthWriteEnabledDesc()
{
    D3D11_DEPTH_STENCIL_DESC dsDesc;
    dsDesc.DepthEnable = true;
    dsDesc.DepthWriteMask = D3D11_DEPTH_WRITE_MASK_ALL;
    dsDesc.DepthFunc = D3D11_COMPARISON_GREATER_EQUAL;
    dsDesc.StencilEnable = false;
    dsDesc.StencilReadMask = D3D11_DEFAULT_STENCIL_READ_MASK;
    dsDesc.StencilWriteMask = D3D11_DEFAULT_STENCIL_WRITE_MASK;
    dsDesc.FrontFace.StencilDepthFailOp = D3D11_STENCIL_OP_KEEP;
    dsDesc.FrontFace.StencilFailOp = D3D11_STENCIL_OP_KEEP;
    dsDesc.FrontFace.StencilPassOp = D3D11_STENCIL_OP_REPLACE;
    dsDesc.FrontFace.StencilFunc = D3D11_COMPARISON_ALWAYS;
    dsDesc.BackFace = dsDesc.FrontFace;

    return dsDesc;
}

D3D11_DEPTH_STENCIL_DESC DepthStencilStates::DepthStencilWriteEnabledDesc()
{
    D3D11_DEPTH_STENCIL_DESC dsDesc;
    dsDesc.DepthEnable = true;
    dsDesc.DepthWriteMask = D3D11_DEPTH_WRITE_MASK_ALL;
    dsDesc.DepthFunc = D3D11_COMPARISON_LESS_EQUAL;
    dsDesc.StencilEnable = true;
    dsDesc.StencilReadMask = D3D11_DEFAULT_STENCIL_READ_MASK;
    dsDesc.StencilWriteMask = D3D11_DEFAULT_STENCIL_WRITE_MASK;
    dsDesc.FrontFace.StencilDepthFailOp = D3D11_STENCIL_OP_KEEP;
    dsDesc.FrontFace.StencilFailOp = D3D11_STENCIL_OP_KEEP;
    dsDesc.FrontFace.StencilPassOp = D3D11_STENCIL_OP_REPLACE;
    dsDesc.FrontFace.StencilFunc = D3D11_COMPARISON_ALWAYS;
    dsDesc.BackFace = dsDesc.FrontFace;

    return dsDesc;
}

D3D11_DEPTH_STENCIL_DESC DepthStencilStates::StencilEnabledDesc()
{
    D3D11_DEPTH_STENCIL_DESC dsDesc;
    dsDesc.DepthEnable = true;
    dsDesc.DepthWriteMask = D3D11_DEPTH_WRITE_MASK_ALL;
    dsDesc.DepthFunc = D3D11_COMPARISON_LESS_EQUAL;
    dsDesc.StencilEnable = true;
    dsDesc.StencilReadMask = D3D11_DEFAULT_STENCIL_READ_MASK;
    dsDesc.StencilWriteMask = 0;
    dsDesc.FrontFace.StencilDepthFailOp = D3D11_STENCIL_OP_KEEP;
    dsDesc.FrontFace.StencilFailOp = D3D11_STENCIL_OP_KEEP;
    dsDesc.FrontFace.StencilPassOp = D3D11_STENCIL_OP_KEEP;
    dsDesc.FrontFace.StencilFunc = D3D11_COMPARISON_EQUAL;
    dsDesc.BackFace = dsDesc.FrontFace;

    return dsDesc;
}

ID3D11SamplerState* SamplerStates::linear;
ID3D11SamplerState* SamplerStates::linearClamp;
ID3D11SamplerState* SamplerStates::linearBorder;
ID3D11SamplerState* SamplerStates::point;
ID3D11SamplerState* SamplerStates::anisotropic;
ID3D11SamplerState* SamplerStates::shadowMap;
ID3D11SamplerState* SamplerStates::shadowMapPCF;

// create it through wrapper func to make compiler happy
//
void CreateSamplerStateWrap( ID3D11Device* device, const D3D11_SAMPLER_DESC& desc, ID3D11SamplerState** ss )
{
	DXCall( device->CreateSamplerState( &desc, ss ) );
}

void SamplerStates::Initialize(ID3D11Device* device)
{
	CreateSamplerStateWrap( device, LinearDesc(), &linear );
	CreateSamplerStateWrap( device, LinearClampDesc(), &linearClamp );
	CreateSamplerStateWrap( device, LinearBorderDesc(), &linearBorder );
	CreateSamplerStateWrap( device, PointDesc(), &point );
	CreateSamplerStateWrap( device, AnisotropicDesc(), &anisotropic );
	CreateSamplerStateWrap( device, ShadowMapDesc(), &shadowMap );
	CreateSamplerStateWrap( device, ShadowMapPCFDesc(), &shadowMapPCF );
}

void SamplerStates::DeInitialize()
{
	DX_SAFE_RELEASE( linear );
	DX_SAFE_RELEASE( linearClamp );
	DX_SAFE_RELEASE( linearBorder );
	DX_SAFE_RELEASE( point );

	DX_SAFE_RELEASE( anisotropic );
	DX_SAFE_RELEASE( shadowMap );
	DX_SAFE_RELEASE( shadowMapPCF );
}

D3D11_SAMPLER_DESC SamplerStates::LinearDesc()
{
    D3D11_SAMPLER_DESC sampDesc;

    sampDesc.Filter = D3D11_FILTER_MIN_MAG_MIP_LINEAR;
    sampDesc.AddressU = D3D11_TEXTURE_ADDRESS_WRAP;
    sampDesc.AddressV = D3D11_TEXTURE_ADDRESS_WRAP;
    sampDesc.AddressW = D3D11_TEXTURE_ADDRESS_WRAP;
    sampDesc.MipLODBias = 0.0f;
    sampDesc.MaxAnisotropy = 1;
    sampDesc.ComparisonFunc = D3D11_COMPARISON_ALWAYS;
    sampDesc.BorderColor[0] = sampDesc.BorderColor[1] = sampDesc.BorderColor[2] = sampDesc.BorderColor[3] = 0;
    sampDesc.MinLOD = 0;
    sampDesc.MaxLOD = D3D11_FLOAT32_MAX;

    return sampDesc;
}

D3D11_SAMPLER_DESC SamplerStates::LinearClampDesc()
{
    D3D11_SAMPLER_DESC sampDesc;

    sampDesc.Filter = D3D11_FILTER_MIN_MAG_MIP_LINEAR;
    sampDesc.AddressU = D3D11_TEXTURE_ADDRESS_CLAMP;
    sampDesc.AddressV = D3D11_TEXTURE_ADDRESS_CLAMP;
    sampDesc.AddressW = D3D11_TEXTURE_ADDRESS_CLAMP;
    sampDesc.MipLODBias = 0.0f;
    sampDesc.MaxAnisotropy = 1;
    sampDesc.ComparisonFunc = D3D11_COMPARISON_ALWAYS;
    sampDesc.BorderColor[0] = sampDesc.BorderColor[1] = sampDesc.BorderColor[2] = sampDesc.BorderColor[3] = 0;
    sampDesc.MinLOD = 0;
    sampDesc.MaxLOD = D3D11_FLOAT32_MAX;

    return sampDesc;
}

D3D11_SAMPLER_DESC SamplerStates::LinearBorderDesc()
{
    D3D11_SAMPLER_DESC sampDesc;

    sampDesc.Filter = D3D11_FILTER_MIN_MAG_MIP_LINEAR;
    sampDesc.AddressU = D3D11_TEXTURE_ADDRESS_BORDER;
    sampDesc.AddressV = D3D11_TEXTURE_ADDRESS_BORDER;
    sampDesc.AddressW = D3D11_TEXTURE_ADDRESS_BORDER;
    sampDesc.MipLODBias = 0.0f;
    sampDesc.MaxAnisotropy = 1;
    sampDesc.ComparisonFunc = D3D11_COMPARISON_ALWAYS;
    sampDesc.BorderColor[0] = sampDesc.BorderColor[1] = sampDesc.BorderColor[2] = sampDesc.BorderColor[3] = 0;
    sampDesc.MinLOD = 0;
    sampDesc.MaxLOD = D3D11_FLOAT32_MAX;

    return sampDesc;
}

D3D11_SAMPLER_DESC SamplerStates::PointDesc()
{
    D3D11_SAMPLER_DESC sampDesc;

    sampDesc.Filter = D3D11_FILTER_MIN_MAG_MIP_POINT;
    sampDesc.AddressU = D3D11_TEXTURE_ADDRESS_CLAMP;
    sampDesc.AddressV = D3D11_TEXTURE_ADDRESS_CLAMP;
    sampDesc.AddressW = D3D11_TEXTURE_ADDRESS_CLAMP;
    sampDesc.MipLODBias = 0.0f;
    sampDesc.MaxAnisotropy = 1;
    sampDesc.ComparisonFunc = D3D11_COMPARISON_ALWAYS;
    sampDesc.BorderColor[0] = sampDesc.BorderColor[1] = sampDesc.BorderColor[2] = sampDesc.BorderColor[3] = 0;
    sampDesc.MinLOD = 0;
    sampDesc.MaxLOD = D3D11_FLOAT32_MAX;

    return sampDesc;
}

D3D11_SAMPLER_DESC SamplerStates::AnisotropicDesc()
{
    D3D11_SAMPLER_DESC sampDesc;

    sampDesc.Filter = D3D11_FILTER_ANISOTROPIC;
    sampDesc.AddressU = D3D11_TEXTURE_ADDRESS_WRAP;
    sampDesc.AddressV = D3D11_TEXTURE_ADDRESS_WRAP;
    sampDesc.AddressW = D3D11_TEXTURE_ADDRESS_WRAP;
    sampDesc.MipLODBias = 0.0f;
    sampDesc.MaxAnisotropy = 16;
    sampDesc.ComparisonFunc = D3D11_COMPARISON_ALWAYS;
    sampDesc.BorderColor[0] = sampDesc.BorderColor[1] = sampDesc.BorderColor[2] = sampDesc.BorderColor[3] = 0;
    sampDesc.MinLOD = 0;
    sampDesc.MaxLOD = D3D11_FLOAT32_MAX;

    return sampDesc;
}

D3D11_SAMPLER_DESC SamplerStates::ShadowMapDesc()
{
    D3D11_SAMPLER_DESC sampDesc;

    sampDesc.Filter = D3D11_FILTER_COMPARISON_MIN_MAG_MIP_POINT;
    sampDesc.AddressU = D3D11_TEXTURE_ADDRESS_CLAMP;
    sampDesc.AddressV = D3D11_TEXTURE_ADDRESS_CLAMP;
    sampDesc.AddressW = D3D11_TEXTURE_ADDRESS_CLAMP;
    sampDesc.MipLODBias = 0.0f;
    sampDesc.MaxAnisotropy = 1;
    sampDesc.ComparisonFunc = D3D11_COMPARISON_LESS_EQUAL;
    sampDesc.BorderColor[0] = sampDesc.BorderColor[1] = sampDesc.BorderColor[2] = sampDesc.BorderColor[3] = 0;
    sampDesc.MinLOD = 0;
    sampDesc.MaxLOD = D3D11_FLOAT32_MAX;

    return sampDesc;
}

D3D11_SAMPLER_DESC SamplerStates::ShadowMapPCFDesc()
{
    D3D11_SAMPLER_DESC sampDesc;

    sampDesc.Filter = D3D11_FILTER_COMPARISON_MIN_MAG_MIP_LINEAR;
    sampDesc.AddressU = D3D11_TEXTURE_ADDRESS_CLAMP;
    sampDesc.AddressV = D3D11_TEXTURE_ADDRESS_CLAMP;
    sampDesc.AddressW = D3D11_TEXTURE_ADDRESS_CLAMP;
    sampDesc.MipLODBias = 0.0f;
    sampDesc.MaxAnisotropy = 1;
    sampDesc.ComparisonFunc = D3D11_COMPARISON_LESS_EQUAL;
    sampDesc.BorderColor[0] = sampDesc.BorderColor[1] = sampDesc.BorderColor[2] = sampDesc.BorderColor[3] = 0;
    sampDesc.MinLOD = 0;
    sampDesc.MaxLOD = D3D11_FLOAT32_MAX;

    return sampDesc;
}

}