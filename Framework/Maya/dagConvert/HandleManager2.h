#pragma once

// inspired by http://bitsquid.blogspot.com/2011/09/managing-decoupling-part-4-id-lookup.html
//
#include <util/Def.h>

#include <stdint.h>
#include <limits.h>
//#include <SPAD_ASSERT.h>

namespace HandleManager2
{
	typedef uint32_t Handle;

	struct HandleDebug
	{
	public:
		union
		{
#ifdef _MSC_VER
#pragma warning(push)
#pragma warning(disable:4201)
#endif
			struct
			{
				uint16_t index;
				uint16_t innerId;
			};
#ifdef _MSC_VER
#pragma warning(pop)
#endif
			uint32_t raw;
		};
	};

	//#define INDEX_MASK 0xffff
	//#define NEW_OBJECT_ID_ADD 0x10000
	const uint32_t INDEX_MASK = 0xffff;
	const uint32_t NEW_OBJECT_ID_ADD = 0x10000;

	template<class TpClass, uint32_t TpMaxObjects>
	class HandleManager
	{
	public:
		enum { MaxObjectsAvailable = TpMaxObjects - 2 };
		enum { MaxObjectIndex = TpMaxObjects - 2 };

		TpClass objects_[TpMaxObjects];
		uint32_t ids_[TpMaxObjects];
		uint16_t next_[TpMaxObjects];
		uint32_t nObjects_;
		uint32_t freelist_enqueue_;
		uint32_t freelist_dequeue_;

		HandleManager()
		{
			static_assert( TpMaxObjects <= 0xffff, "TpMaxObjects exceeded" );

			nObjects_ = 0;

			for ( uint32_t i = 0; i < TpMaxObjects; ++i )
			{
				ids_[i] = i + NEW_OBJECT_ID_ADD;
				objects_[i] = TpClass();
				next_[i] = (uint16_t)( i + 1 );
			}

			// index 0 is reserved for 'null' object
			// id==0 is always invalid
			//
			freelist_dequeue_ = 1;
			freelist_enqueue_ = TpMaxObjects - 1;
		}

		HandleManager& operator=( const HandleManager& rhs ) = delete;
		HandleManager( const HandleManager& rhs ) = delete;

		bool isValid( Handle id ) const
		{
			uint32_t index = id & INDEX_MASK;
			SPAD_ASSERT( index <= MaxObjectIndex );
			//return in.id == id;// && in.index != USHRT_MAX;
			return id == ids_[index];// && objects_[index];
		}

		const TpClass& get( Handle id ) const
		{
			uint32_t index = id & INDEX_MASK;
			SPAD_ASSERT( index <= MaxObjectIndex );
			SPAD_ASSERT( id == ids_[index] ); // make sure it's valid
			return objects_[index];
		}

		TpClass& get( Handle id )
		{
			uint32_t index = id & INDEX_MASK;
			SPAD_ASSERT( index <= MaxObjectIndex );
			SPAD_ASSERT( id == ids_[index] ); // make sure it's valid
			return objects_[index];
		}

		void set( Handle id, TpClass& ob )
		{
			uint32_t index = id & INDEX_MASK;
			SPAD_ASSERT( index <= MaxObjectIndex );
			SPAD_ASSERT( id == ids_[index] ); // make sure it's valid
			objects_[index] = ob;
		}

		// this may trigger unnecessary copy constructor
		// but is very convenient when TpClass is a pointer
		//
		bool tryGet( Handle id, TpClass& ob ) const
		{
			uint32_t index = id & INDEX_MASK;
			SPAD_ASSERT( index <= MaxObjectIndex );
			if ( id == ids_[index] ) // make sure it's valid
			{
				ob = objects_[index];
				return true;
			}

			return false;
		}

		Handle add( TpClass& ob )
		{
			SPAD_ASSERT( nObjects_ < MaxObjectsAvailable );
			++nObjects_;
			//SPAD_ASSERT(!_active[_freelist_dequeue]);
			//Index &in = _indices[_freelist_dequeue];
			////_active[_freelist_dequeue] = true;
			//_freelist_dequeue = in.next;
			//in.id += NEW_OBJECT_ID_ADD;
			objects_[freelist_dequeue_] = ob;
			uint32_t& id = ids_[freelist_dequeue_];
			//id += NEW_OBJECT_ID_ADD;
			freelist_dequeue_ = next_[freelist_dequeue_];
			//in.index = _num_objects++;
			//Object &o = _objects[in.index];
			//o.id = in.id;
			//return o.id;
			return id;
		}

		void remove( Handle id )
		{
			SPAD_ASSERT( nObjects_ > 0 );
			SPAD_ASSERT( isValid( id ) );
			--nObjects_;

			//Object &o = _objects[in.index];
			//o = _objects[--_num_objects];
			//_indices[id & INDEX_MASK].index = in.index;

			//in.index = USHRT_MAX;
			uint32_t index = id & INDEX_MASK;
			ids_[index] += NEW_OBJECT_ID_ADD;
			objects_[index] = TpClass();
			//_active[index] = false;
			next_[freelist_enqueue_] = (uint16_t)index;
			freelist_enqueue_ = (uint16_t)index;
		}

		void removeIndex( uint16_t index )
		{
			SPAD_ASSERT( nObjects_ > 0 );
			--nObjects_;

			ids_[index] += NEW_OBJECT_ID_ADD;
			objects_[index] = TpClass();
			next_[freelist_enqueue_] = index;
			freelist_enqueue_ = index;
		}

		uint32_t count() const
		{
			return nObjects_;
		}
	};

} // namespace HandleManager2

