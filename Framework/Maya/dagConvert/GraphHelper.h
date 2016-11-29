#pragma once

#include "Graph.h"
#include "Level.h"

template<typename T>
class GraphHelper
{
private:
	//typedef picoArray<T, 16, PICO_CACHELINE_SIZE> ObjectArray;
	typedef std::vector<T> ObjectArray;

	static_assert( std::is_pointer<T>::value, "Template parameter must be a pointer" );

public:

	const T getFlat( u32 index ) const { return array_[index]; }
	T getFlat( u32 index ) { return array_[index]; }
	T clearFlat( u32 index )
	{
		T ret = array_[index];
		array_[index] = nullptr;
		return ret;
	}

	void setFlat( u32 index, T t )
	{
		SPAD_ASSERT( nullptr == array_[index] );
		array_[index] = t;
	}

	const T getByPathId( u32 pathId ) const
	{
		// this can be optimized in shipping builds
		// in shipping builds pathId is equal to flatIndex
		//const PathData& pd = pathIdManager_->get( pathId );
		//return getFlat( pd.indexIntoFlatDenseArray );
		u32 flatIndex = level_->getFlatIndex( pathId );
		return getFlat( flatIndex );
	}

	//void remapDelete( const PathsDeletedParam& param )
	//{
	//	ObjectArray newArray;
	//	newArray.resize( param.nPathsRemap );

	//	for ( u32 i = 0; i < param.nPathsRemap; ++i )
	//	{
	//		const PathRemap& pr = param.pathsRemap[i];
	//		newArray[pr.newFlatIndex_] = array_[pr.oldFlatIndex_];
	//	}

	//	array_ = std::move( newArray );
	//}

	//void remapCreate( const PathsCreatedParam& param )
	//{
	//	ObjectArray newArray;
	//	newArray.resize( param.nPathsRemap );

	//	for ( u32 i = 0; i < param.nPathsRemap; ++i )
	//	{
	//		const PathRemap& pr = param.pathsRemap[i];
	//		newArray[pr.newFlatIndex_] = pr.oldFlatIndex_ != 0xffffffff ? array_[pr.oldFlatIndex_] : nullptr;
	//	}

	//	array_ = std::move( newArray );
	//}

	void remap( const GraphChangedParam& param )
	{
		ObjectArray newArray;
		newArray.resize( param.nPathsRemap );

		for ( u32 i = 0; i < param.nPathsRemap; ++i )
		{
			const PathRemap& pr = param.pathsRemap[i];
			newArray[pr.newFlatIndex_] = pr.oldFlatIndex_ != 0xffff ? array_[pr.oldFlatIndex_] : nullptr;
		}

		array_ = std::move( newArray );
		//pathIdManager_ = param.newGraphStructure->getPathIdManager();
		level_ = param.level;
	}

private:
	ObjectArray array_;
	//const PathIdManager* pathIdManager_ = nullptr;
	const Level* level_ = nullptr;
};




