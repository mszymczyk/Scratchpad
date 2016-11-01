#pragma once

#include <atomic>

namespace spad
{

class RefCountBase
{
public:
	RefCountBase()
		: refCount_( 0 )
	{	}

	void AddRef()
	{
		++refCount_;
	}

	void Release()
	{
		if ( --refCount_ == 0 )
			delete this;
	}

	uint32_t GetRefCount() const
	{
		return refCount_;
	}

private:
	std::atomic<uint32_t> refCount_;
};


// based on sample at
// https://isocpp.org/wiki/faq/freestore-mgmt#ref-count-simple
template<class T>
class RefCountHandle
{
public:
	T* operator-> ( ) { return p_; }
	const T* operator-> () const { return p_; }

	RefCountHandle()
		: p_( nullptr )
	{	}

	RefCountHandle( T* p )
		: p_( p )
	{
		if ( p_ )
			p_->AddRef();
	}  // p must not be null

	~RefCountHandle()
	{
		reset();
	}

	RefCountHandle( const RefCountHandle& p )
		: p_( p.p_ )
	{
		if ( p_ )
			p_->AddRef();
	}

	RefCountHandle( RefCountHandle&& p )
		: p_( nullptr )
	{
		if ( this != &p )
			std::swap( p_, p.p_ );
	}

	RefCountHandle& operator= ( const RefCountHandle& p )
	{
		// DO NOT CHANGE THE ORDER OF THESE STATEMENTS!
		// (This order properly handles self-assignment)
		// (This order also properly handles recursion)
		T* const old = p_;
		p_ = p.p_;
		if ( p_ )
			p_->AddRef();
		if ( old )
			old->Release();
		return *this;
	}

	RefCountHandle& operator= ( RefCountHandle&& p )
	{
		std::swap( this->p_, p.p_ );
		return *this;
	}

	void reset()
	{
		if ( p_ )
			p_->Release();
	}

	T* get() const
	{
		return p_;
	}

	bool isNull() const
	{
		return p_ == nullptr;
	}

	bool isNonNull() const
	{
		return p_ != nullptr;
	}

	explicit operator bool() const
	{	// test if handle owns no resource
		return isNonNull();
	}

private:
	T* p_;    // p_ is never NULL
};

} // namespace spad

