#include "Util_pch.h"
#include "SettingsEditor.h"
#include "..\ZMQHubUtil.h"

#if defined(_MSC_VER) && defined(_DEBUG)
#define new _DEBUG_NEW
#endif

#ifndef SETTINGS_EDITOR_FROZEN

using namespace spad::ZMQHubUtil;

namespace spad
{
	//namespace SettingsEditor
	//{
		//struct _Impl;
		//extern _Impl* _gImpl;

		namespace SettingsEditorZMQ
		{
			void handleMessage( const IncommingMessage& msg )
			{
				bool ok = true;
				std::string cmd = msg.readString( ok );
				std::string settingsFile = msg.readString( ok );
				std::string groupame = msg.readString( ok );
				std::string presetName = msg.readString( ok );
				std::string paramName = msg.readString( ok );

				if (cmd == "setInt")
				{
					int nValues = msg.readInt( ok );
					int iVal = msg.readInt( ok );
					SettingsEditor::DontTouchIt::updateParam( settingsFile.c_str(), groupame.c_str(), presetName.c_str(), paramName.c_str(), &iVal, nValues );
				}
				else if (cmd == "setFloat")
				{
					int nValues = msg.readInt( ok );
					SPAD_ASSERT( nValues <= 4 );
					float f[4];
					for (int i = 0; i < nValues; ++i)
						f[i] = msg.readFloat( ok );
					SettingsEditor::DontTouchIt::updateParam( settingsFile.c_str(), groupame.c_str(), presetName.c_str(), paramName.c_str(), f, nValues );
				}
				else if (cmd == "setString")
				{
					std::string sVal = msg.readString( ok );
					SettingsEditor::DontTouchIt::updateParam( settingsFile.c_str(), groupame.c_str(), presetName.c_str(), paramName.c_str(), sVal.c_str() );
				}
				else
				{
					SPAD_NOT_IMPLEMENTED;
				}
			}

			void MessageHandler( const IncommingMessage& msg, void* /*userData*/, void* /*userData2*/ )
			{
				if (!msg.doTagsEqual( "settings" ))
					return;

				handleMessage( msg );
			}

			int startUp()
			{
				registerMessageHandler( MessageHandler, nullptr /*_gImpl*/ );
				return 0;
			}

			void shutDown()
			{
			}
		} // namespace SettingsEditorZMQ
	//} // namespace SettingsEditor
} // namespace spad

#endif //

