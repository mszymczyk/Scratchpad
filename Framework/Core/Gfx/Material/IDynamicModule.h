#pragma once

namespace spad
{
	class IDynamicModule
	{
	public:
		virtual void Update( float deltaTime ) = 0;

	}; // class IDynamicModule

} // namespace spad
