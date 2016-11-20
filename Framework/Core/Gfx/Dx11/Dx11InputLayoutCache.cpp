#include "Gfx_pch.h"
#include "Dx11InputLayoutCache.h"

namespace spad
{
	ID3D11InputLayout* Dx11InputLayoutCache::getLayout( u32 elementsHash, u32 inputSignatureHash, const D3D11_INPUT_ELEMENT_DESC* elements, u32 nElements, const u8* inputSignature, u32 inputSignatureSize )
	{
		uint64_t hash = (u64)elementsHash << 32 | inputSignatureHash;
		InputLayoutMap::const_iterator it = inputLayouts_.find( hash );
		if ( it != inputLayouts_.end() )
		{
			const _Value& val = it->second;

#ifdef FR_ASSERTIONS_ENABLED
			SPAD_ASSERT( nElements == val.elements_.size() );
			SPAD_ASSERT( inputSignatureSize == val.vsInputSignature_.size() );
			SPAD_ASSERT( !memcmp( inputSignature, &val.vsInputSignature_[0], inputSignatureSize ) );
			u32 SemanticIndex_offset = offsetof( D3D11_INPUT_ELEMENT_DESC, SemanticIndex );
			u32 InstanceDataStepRate_offset = offsetof( D3D11_INPUT_ELEMENT_DESC, InstanceDataStepRate );
			u32 elemSize = InstanceDataStepRate_offset - SemanticIndex_offset + sizeof( UINT );
			for ( u32 ielem = 0; ielem < nElements; ++ielem )
			{
				SPAD_ASSERT( elements[ielem].SemanticName == val.semanticNames_[ielem] );
				SPAD_ASSERT( !memcmp( &elements[ielem].SemanticIndex, &val.elements_[ielem].SemanticIndex, elemSize ) );
			}
#endif //

			return val.inputLayout_;
		}
		else
		{
			return _AddLayout( elementsHash, inputSignatureHash, elements, nElements, inputSignature, inputSignatureSize );
		}
	}

	ID3D11InputLayout* Dx11InputLayoutCache::_AddLayout( u32 elementsHash, u32 inputSignatureHash, const D3D11_INPUT_ELEMENT_DESC* elements, u32 nElements, const u8* inputSignature, u32 inputSignatureSize )
	{
		_Value val;
		DXCall( dxDevice_->CreateInputLayout( elements, nElements, inputSignature, inputSignatureSize, &val.inputLayout_ ) );

#ifdef FR_ASSERTIONS_ENABLED
		val.semanticNames_.reserve( nElements );
		val.elements_.reserve( nElements );
		for ( u32 ielem = 0; ielem < nElements; ++ielem )
		{
			val.semanticNames_.emplace_back( elements[ielem].SemanticName );
			val.elements_.push_back( elements[ielem] );
			val.elements_.back().SemanticName = val.semanticNames_[ielem].c_str();
		}
		val.vsInputSignature_.resize( inputSignatureSize );
		memcpy( &val.vsInputSignature_[0], inputSignature, inputSignatureSize );
#endif //

		uint64_t hash = (u64)elementsHash << 32 | inputSignatureHash;
		inputLayouts_[hash] = val;

		return val.inputLayout_;
	}

} // namespace spad
