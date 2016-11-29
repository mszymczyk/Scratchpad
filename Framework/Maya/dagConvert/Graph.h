#pragma once

#include "HandleManager2.h"
#include <Util/StdHelp.h>




typedef unsigned int u32;
typedef unsigned int PathId;
typedef unsigned int NodeId;
typedef unsigned short PathIndex;
typedef unsigned short NodeIndex;

//class DagNode;

struct DependencyNode
{
	std::string name_;
	NodeIndex flatIndex_ = 0;
};

typedef DependencyNode DagNode;


// https://en.wikipedia.org/wiki/Universally_unique_identifier
// maya follows the same convention
struct Uuid
{
	Uuid() {}
	Uuid( uint64_t high, uint64_t low) : high_(high), low_(low) {}

	Uuid( const char* uuid )
	{
		*this = fromString( uuid );
	}

	uint64_t high_;
	uint64_t low_;

	static Uuid fromString( const char* uuid );
};

inline bool operator<( const Uuid& _Left, const Uuid& _Right ) noexcept
{	// return true if _Left precedes _Right
	if ( _Left.high_ < _Right.high_ )
		return true;
	else if ( _Left.high_ == _Right.high_ )
		return _Left.low_ < _Right.low_;

	return false;
}


struct NodeProxy
{
	//std::string name_;
	DependencyNode* depNode_;
	std::vector<NodeProxy*> children_;

	NodeProxy() {}
	//NodeProxy( const char* name ) : name_( name ) {}

	~NodeProxy()
	{
		clear();
	}

	void clear()
	{
		children_.clear();
		//name_.clear();
	}

	NodeProxy* shapeNode() const
	{
		// fake implementation
		for ( NodeProxy* c : children_ )
			if ( strstr( c->depNode_->name_.c_str(), "Shape" ) )
				return c;

		return nullptr;
	}
};

struct PathData
{
	u32 indexIntoFlatDenseArray = 0;
};

struct NodeIdData
{
	NodeIdData() : depNode_( nullptr ) {}
	NodeIdData( DependencyNode* dn ) : depNode_( dn ) { }

	//void* dependencyNode = nullptr;
	//NodeIndex indexIntoFlatDenseArray = 0;
	DependencyNode* depNode_ = nullptr;
};

struct PathInfo
{
	//std::string fullPathName;
	enum { eMaxStaticNodes = 7 };
	
	union PathNodes
	{
		uint64_t u_[2]; // for quick compare and to force alignment
		struct NIS
		{
			NodeIndex staticNodeIndices_[eMaxStaticNodes];
			u16 nNodeIndices_;
		} nis_;
		struct NID
		{
			NodeIndex* dynamicNodeIndices_;
			u16 _unused_[eMaxStaticNodes-4];
			u16 nNodeIndices_;
		} nid_;
	} nodes_;

	PathInfo() { nodes_.u_[0] = nodes_.u_[1] = 0; _padding_[0] = _padding_[1] = _padding_[2] = 0; }

	void setPathNodes( const NodeIndex* indices, u16 nIndices )
	{
		if ( nodes_.nis_.nNodeIndices_ > eMaxStaticNodes )
			delete[] nodes_.nid_.dynamicNodeIndices_;

		nodes_.u_[0] = nodes_.u_[1] = 0;

		if ( nIndices > eMaxStaticNodes )
		{
			nodes_.nid_.nNodeIndices_ = nIndices;
			nodes_.nid_.dynamicNodeIndices_ = new NodeIndex[nIndices];
			memcpy( nodes_.nid_.dynamicNodeIndices_, indices, nIndices * sizeof( NodeIndex ) );
		}
		else
		{
			nodes_.nis_.nNodeIndices_ = nIndices;
			memcpy( nodes_.nis_.staticNodeIndices_, indices, nIndices * sizeof( NodeIndex ) );
		}
	}

	PathId pathId = 0;
	bool alive = false;
	u8 _padding_[3];

	//const char* getName() const { return fullPathName.c_str(); }
	//size_t getNameLen() const { return fullPathName.length(); }

	static bool compare( const PathInfo* lhs, const PathInfo* rhs )
	{
		if ( lhs->nodes_.nis_.nNodeIndices_ < rhs->nodes_.nis_.nNodeIndices_ )
		{
			return true;
		}
		else if ( lhs->nodes_.nis_.nNodeIndices_ == rhs->nodes_.nis_.nNodeIndices_ )
		{
			if ( lhs->nodes_.nis_.nNodeIndices_ > eMaxStaticNodes )
			{
				return memcmp( lhs->nodes_.nid_.dynamicNodeIndices_, rhs->nodes_.nid_.dynamicNodeIndices_, lhs->nodes_.nid_.nNodeIndices_ * sizeof( NodeIndex ) ) < 0;
			}
			else
			{
				if ( lhs->nodes_.u_[0] < rhs->nodes_.u_[0] )
					return true;
				else if ( lhs->nodes_.u_[0] == rhs->nodes_.u_[0] )
					return lhs->nodes_.u_[1] < rhs->nodes_.u_[1];
			}
		}

		return false;
	}
};

struct PathRemap
{
	PathRemap( PathIndex oldIndex, PathIndex newIndex ) : oldFlatIndex_( oldIndex ), newFlatIndex_( newIndex ) {}
	PathIndex oldFlatIndex_ = 0;
	PathIndex newFlatIndex_ = 0;
};

//template<class L, class R>
struct PathInfoComparator
{
	bool operator() ( const PathInfo* lhs, const PathInfo* rhs ) const
	{
		//return stdutil::CompareLess( lhs->getName(), lhs->getNameLen(), rhs->getName(), rhs->getNameLen() );
		return PathInfo::compare( lhs, rhs );
	}
};


//typedef std::map<std::string, NodeProxy*> NodeProxyMap;
typedef std::map<Uuid, NodeProxy*> NodeProxyMap;
typedef HandleManager2::HandleManager<PathData, 0xffff> PathIdManager;
typedef HandleManager2::HandleManager<NodeIdData, 0xffff> NodeIdManager;
typedef std::vector<DependencyNode*> DependencyNodeArray;
typedef std::set<PathInfo*, PathInfoComparator> PathSet;




class GraphStructure
{
public:
	//typedef std::vector<u32> PathIdsArray;
	typedef std::vector<PathIndex> ParentsArray;
	typedef std::vector<NodeIndex> ChildCountsArray;
	//typedef std::vector<u32> NodeIdsArray;
	//typedef std::vector<NodeProxy*> NodeArray;

	void printHierarchy() const
	{
		printf( "---------------- New graph structure\n" );
		u32 flatIndex = 0;
		_PrintHierarchyRecurse( flatIndex, "" );
		fflush( stdout );
	}

	//const PathIdManager* getPathIdManager() const { return pathIdManager_; }


	// see http://stackoverflow.com/questions/9322174/move-assignment-operator-and-if-this-rhs
	// to learn how to implement move assignment operator
	GraphStructure& operator=( GraphStructure&& other )
	{
		if ( this != &other )
		{
			// we have to leave 'other' in valid but unspecified state
			// we can free 'this' or we can swap 'this' with 'other'
			parents_ = std::move( other.parents_ );
			childCount_ = std::move( other.childCount_ );
			nodes_ = std::move( other.nodes_ );
		}
		return *this;
	}

private:
	void _PrintHierarchyRecurse( u32& flatIndex, std::string pathBase ) const;

private:
	//PathIdsArray pathIds_;
	ParentsArray parents_;
	ChildCountsArray childCount_;
	//NodeArray nodes_;
	DependencyNodeArray nodes_;
	//const PathIdManager* pathIdManager_ = nullptr;
	friend class dagConverter;
	friend class Level;
};



class Level;

struct GraphChangedParam
{
	const Level* level = nullptr;

	const u32* deletedPaths = nullptr;
	const u32* deletedPathsFlatIndices = nullptr;
	u32 nDeletedPaths = 0;

	const u32* createdPaths = nullptr;
	const u32* createdPathsFlatIndices = nullptr;
	u32 nCreatedPaths = 0;

	const PathRemap* pathsRemap = nullptr;
	u32 nPathsRemap = 0;

	//const GraphStructure* newGraphStructure = nullptr;
	//const GraphStructure* oldGraphStructure = nullptr;
};

class GraphChangedCallback
{
public:
	virtual void onGraphChanged( const GraphChangedParam& param ) = 0;
};




//
//template<typename T>
//class GraphHelper
//{
//private:
//	//typedef picoArray<T, 16, PICO_CACHELINE_SIZE> ObjectArray;
//	typedef std::vector<T> ObjectArray;
//
//	static_assert( std::is_pointer<T>::value, "Template parameter must be a pointer" );
//
//public:
//
//	const T getFlat( u32 index ) const { return array_[index]; }
//	T getFlat( u32 index ) { return array_[index]; }
//	T clearFlat( u32 index )
//	{
//		T ret = array_[index];
//		array_[index] = nullptr;
//		return ret;
//	}
//
//	void setFlat( u32 index, T t )
//	{
//		SPAD_ASSERT( nullptr == array_[index] );
//		array_[index] = t;
//	}
//
//	const T getByPathId( u32 pathId ) const
//	{
//		// this can be optimized in shipping builds
//		// in shipping builds pathId is equal to flatIndex
//		const PathData& pd = pathIdManager_->get( pathId );
//		return getFlat( pd.indexIntoFlatDenseArray );
//	}
//
//	//void remapDelete( const PathsDeletedParam& param )
//	//{
//	//	ObjectArray newArray;
//	//	newArray.resize( param.nPathsRemap );
//
//	//	for ( u32 i = 0; i < param.nPathsRemap; ++i )
//	//	{
//	//		const PathRemap& pr = param.pathsRemap[i];
//	//		newArray[pr.newFlatIndex_] = array_[pr.oldFlatIndex_];
//	//	}
//
//	//	array_ = std::move( newArray );
//	//}
//
//	//void remapCreate( const PathsCreatedParam& param )
//	//{
//	//	ObjectArray newArray;
//	//	newArray.resize( param.nPathsRemap );
//
//	//	for ( u32 i = 0; i < param.nPathsRemap; ++i )
//	//	{
//	//		const PathRemap& pr = param.pathsRemap[i];
//	//		newArray[pr.newFlatIndex_] = pr.oldFlatIndex_ != 0xffffffff ? array_[pr.oldFlatIndex_] : nullptr;
//	//	}
//
//	//	array_ = std::move( newArray );
//	//}
//
//	void remap( const GraphChangedParam& param )
//	{
//		ObjectArray newArray;
//		newArray.resize( param.nPathsRemap );
//
//		for ( u32 i = 0; i < param.nPathsRemap; ++i )
//		{
//			const PathRemap& pr = param.pathsRemap[i];
//			newArray[pr.newFlatIndex_] = pr.oldFlatIndex_ != 0xffffffff ? array_[pr.oldFlatIndex_] : nullptr;
//		}
//
//		array_ = std::move( newArray );
//		pathIdManager_ = param.newGraphStructure->getPathIdManager();
//	}
//
//private:
//	ObjectArray array_;
//	const PathIdManager* pathIdManager_ = nullptr;
//};
//



