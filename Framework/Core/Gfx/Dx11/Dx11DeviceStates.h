#pragma once

namespace spad
{

class BlendStates
{
public:
    static void Initialize(ID3D11Device* device);
	static void DeInitialize();

	static ID3D11BlendState* blendDisabled;
	static ID3D11BlendState* additiveBlend;
	static ID3D11BlendState* alphaBlend;
	static ID3D11BlendState* pmAlphaBlend;
	static ID3D11BlendState* noColor;
	static ID3D11BlendState* alphaToCoverage;
	static ID3D11BlendState* opacityBlend;

    //ID3D11BlendState* BlendDisabled () { return blendDisabled; };
    //ID3D11BlendState* AdditiveBlend () { return additiveBlend; };
    //ID3D11BlendState* AlphaBlend () { return alphaBlend; };
    //ID3D11BlendState* PreMultipliedAlphaBlend () { return pmAlphaBlend; };
    //ID3D11BlendState* ColorWriteDisabled () { return noColor; };
    //ID3D11BlendState* AlphaToCoverage () { return alphaToCoverage; };
    //ID3D11BlendState* OpacityBlend() { return opacityBlend; };

    static D3D11_BLEND_DESC BlendDisabledDesc();
    static D3D11_BLEND_DESC AdditiveBlendDesc();
    static D3D11_BLEND_DESC AlphaBlendDesc();
    static D3D11_BLEND_DESC PreMultipliedAlphaBlendDesc();
    static D3D11_BLEND_DESC ColorWriteDisabledDesc();
    static D3D11_BLEND_DESC AlphaToCoverageDesc();
    static D3D11_BLEND_DESC OpacityBlendDesc();
};


class RasterizerStates
{

protected:

    static ID3D11RasterizerState* noCull;
    static ID3D11RasterizerState* cullBackFaces;
    static ID3D11RasterizerState* cullBackFacesScissor;
    static ID3D11RasterizerState* cullFrontFaces;
    static ID3D11RasterizerState* cullFrontFacesScissor;
    static ID3D11RasterizerState* noCullNoMS;
    static ID3D11RasterizerState* noCullScissor;
    static ID3D11RasterizerState* wireframe;

public:
    static void Initialize(ID3D11Device* device);
	static void DeInitialize();

    static ID3D11RasterizerState* NoCull() { return noCull; };
    static ID3D11RasterizerState* BackFaceCull() { return cullBackFaces; };
    static ID3D11RasterizerState* BackFaceCullScissor() { return cullBackFacesScissor; };
    static ID3D11RasterizerState* FrontFaceCull() { return cullFrontFaces; };
    static ID3D11RasterizerState* FrontFaceCullScissor() { return cullFrontFacesScissor; };
    static ID3D11RasterizerState* NoCullNoMS() { return noCullNoMS; };
    static ID3D11RasterizerState* NoCullScissor() { return noCullScissor; };
    static ID3D11RasterizerState* Wireframe() { return wireframe; };

    static D3D11_RASTERIZER_DESC NoCullDesc();
    static D3D11_RASTERIZER_DESC FrontFaceCullDesc();
    static D3D11_RASTERIZER_DESC FrontFaceCullScissorDesc();
    static D3D11_RASTERIZER_DESC BackFaceCullDesc();
    static D3D11_RASTERIZER_DESC BackFaceCullScissorDesc();
    static D3D11_RASTERIZER_DESC NoCullNoMSDesc();
    static D3D11_RASTERIZER_DESC NoCullScissorDesc();
    static D3D11_RASTERIZER_DESC WireframeDesc();
};


class DepthStencilStates
{
	static ID3D11DepthStencilState* depthDisabled;
	static ID3D11DepthStencilState* depthEnabled;
	static ID3D11DepthStencilState* revDepthEnabled;
	static ID3D11DepthStencilState* depthWriteEnabled;
	static ID3D11DepthStencilState* revDepthWriteEnabled;
	static ID3D11DepthStencilState* depthStencilWriteEnabled;
	static ID3D11DepthStencilState* stencilEnabled;

public:
    static void Initialize(ID3D11Device* device);
	static void DeInitialize();

    static ID3D11DepthStencilState* DepthDisabled() { return depthDisabled; };
    static ID3D11DepthStencilState* DepthEnabled() { return depthEnabled; };
    static ID3D11DepthStencilState* ReverseDepthEnabled() { return revDepthEnabled; };
    static ID3D11DepthStencilState* DepthWriteEnabled() { return depthWriteEnabled; };
    static ID3D11DepthStencilState* ReverseDepthWriteEnabled() { return revDepthWriteEnabled; };
    static ID3D11DepthStencilState* DepthStencilWriteEnabled() { return depthStencilWriteEnabled; };
    static ID3D11DepthStencilState* StencilTestEnabled() { return depthStencilWriteEnabled; };

    static D3D11_DEPTH_STENCIL_DESC DepthDisabledDesc();
    static D3D11_DEPTH_STENCIL_DESC DepthEnabledDesc();
    static D3D11_DEPTH_STENCIL_DESC ReverseDepthEnabledDesc();
    static D3D11_DEPTH_STENCIL_DESC DepthWriteEnabledDesc();
    static D3D11_DEPTH_STENCIL_DESC ReverseDepthWriteEnabledDesc();
    static D3D11_DEPTH_STENCIL_DESC DepthStencilWriteEnabledDesc();
    static D3D11_DEPTH_STENCIL_DESC StencilEnabledDesc();
};


class SamplerStates
{
public:
    static void Initialize(ID3D11Device* device);
	static void DeInitialize();

	static ID3D11SamplerState* linear;
	static ID3D11SamplerState* linearClamp;
	static ID3D11SamplerState* linearBorder;
	static ID3D11SamplerState* point;
	static ID3D11SamplerState* anisotropic;
	static ID3D11SamplerState* shadowMap;
	static ID3D11SamplerState* shadowMapPCF;

 //   static ID3D11SamplerState* Linear() { return linear; };
	//static ID3D11SamplerState* const * LinearArray() { return &linear; };
	//static ID3D11SamplerState* LinearClamp() { return linearClamp; };
	//static ID3D11SamplerState* const * LinearClampArray() { return &linearClamp; };
	//static ID3D11SamplerState* LinearBorder() { return linearBorder; };
 //   static ID3D11SamplerState* Point() { return point; };
 //   static ID3D11SamplerState* Anisotropic() { return anisotropic; };
 //   static ID3D11SamplerState* ShadowMap() { return shadowMap; };
 //   static ID3D11SamplerState* ShadowMapPCF() { return shadowMapPCF; };

    static D3D11_SAMPLER_DESC LinearDesc();
    static D3D11_SAMPLER_DESC LinearClampDesc();
    static D3D11_SAMPLER_DESC LinearBorderDesc();
    static D3D11_SAMPLER_DESC PointDesc();
    static D3D11_SAMPLER_DESC AnisotropicDesc();
    static D3D11_SAMPLER_DESC ShadowMapDesc();
    static D3D11_SAMPLER_DESC ShadowMapPCFDesc();
};

}
