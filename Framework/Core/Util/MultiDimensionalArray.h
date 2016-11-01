#pragma once

#include <vector>

namespace spad
{


template<class T>
struct MultiDimensionalArray
{
	void grow( const u32* newDimSize, u32 newDim )
	{
		SPAD_ASSERT( newDim >= dimSizes_.size() );

		std::vector<u32> newDimSizes;
		newDimSizes.resize( newDim );
		std::vector<u32> newChildSize;
		newChildSize.resize( newDim );

		u32 newTotalSize = 1;

		for ( u32 i = 0; i < newDim; ++i )
		{
			SPAD_ASSERT( newDimSize[i] > 0 );
			SPAD_ASSERT( ( i >= dimSizes_.size() ) || newDimSize[i] >= dimSizes_[i] );

			newTotalSize *= newDimSize[i];

			newDimSizes[i] = newDimSize[i];

			u32 siz = 1;
			for ( u32 k = i + 1; k < newDim; ++k )
				siz *= newDimSize[k];

			newChildSize[i] = siz;
		}

		std::vector<T> newArray( newTotalSize );

		const size_t nDim = (u32)dimSizes_.size();

		if ( !array_.empty() )
		{
			std::vector<u32> dimIdx( dimSizes_.size() );
			bool done = false;
			while ( !done )
			{
				for ( size_t idim = 0; idim < nDim; ++idim )
				{
					u32 offset = 0;
					for ( u32 i = 0; i < nDim; ++i )
						offset += dimIdx[i] * childSize_[i];

					u32 newOffset = 0;
					for ( u32 i = 0; i < nDim; ++i )
						newOffset += dimIdx[i] * newChildSize[i];

					dimIdx[idim] += 1;

					if ( dimIdx[idim] < dimSizes_[idim] )
					{
						// next index
						//
						break;
					}
					else if ( idim == nDim - 1 )
					{
						// last array is done
						//
						done = true;
						break;
					}
					else
					{
						dimIdx[idim] = 0;
					}
				}
			}
		}

		dimSizes_ = std::move( newDimSizes );
		childSize_ = std::move( newChildSize );
		array_ = std::move( newArray );
	}

	T& element( const u32* index, u32 indexLen )
	{
		SPAD_ASSERT( indexLen == (u32)dimSizes_.size() );
		u32 offset = 0;

		for ( u32 i = 0; i < indexLen; ++i )
		{
			SPAD_ASSERT( index[i] < dimSizes_[i] );
			offset += index[i] * childSize_[i];
		}

		return array_[offset];
	}

	const size_t num_dims() const { return dimSizes_.size(); }
	const std::vector<u32>& dims() const { return dimSizes_; }

	size_t flat_size() const { return array_.size(); }
	const std::vector<T>& flat_array() const { return array_; }

private:
	std::vector<u32> dimSizes_;
	std::vector<u32> childSize_;
	std::vector<T> array_;
};


} // namespace spad
