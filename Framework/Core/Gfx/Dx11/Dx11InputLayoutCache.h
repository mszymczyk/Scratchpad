#pragma once

namespace spad
{

	// Associates shader with mesh.
	// This allows for dynamic reloading both shader and mesh without carrying about stale input layouts.
	// Every rendering thread should have one instance of this class.
	// Current implementation uses hashes to speedup looking for existing input layout but completely ignores hash collisions.
	// Debug build will assert if collision happens but I don't expect this to happen
	// Slower but more robust solution is available through std::unordered_map
	//
	class Dx11InputLayoutCache
	{
	public:
		Dx11InputLayoutCache()
		{	}

		ID3D11InputLayout* getLayout( u32 elementsHash, u32 inputSignatureHash, const D3D11_INPUT_ELEMENT_DESC* elements, u32 nElements, const u8* inputSignature, u32 inputSignatureSize );

		void setInputLayout( ID3D11DeviceContext* context, u32 elementsHash, u32 inputSignatureHash, const D3D11_INPUT_ELEMENT_DESC* elements, u32 nElements, const u8* inputSignature, u32 inputSignatureSize )
		{
			ID3D11InputLayout* inputLayout = getLayout( elementsHash, inputSignatureHash, elements, nElements, inputSignature, inputSignatureSize );
			context->IASetInputLayout( inputLayout );
		}

		void setDxDevice( ID3D11Device* device )
		{
			dxDevice_ = device;
		}

	private:

		ID3D11InputLayout* _AddLayout( u32 elementsHash, u32 inputSignatureHash, const D3D11_INPUT_ELEMENT_DESC* elements, u32 nElements, const u8* inputSignature, u32 inputSignatureSize );

		struct _Value
		{
			ID3D11InputLayoutPtr inputLayout_;

	#ifdef FR_ASSERTIONS_ENABLED
			std::vector<std::string> semanticNames_;
			std::vector<D3D11_INPUT_ELEMENT_DESC> elements_;
			std::vector<u8> vsInputSignature_;
	#endif //
		};

		typedef std::map<uint64_t, _Value> InputLayoutMap;
		InputLayoutMap inputLayouts_;

		ID3D11Device* dxDevice_ = nullptr;
	};


} // namespace spad