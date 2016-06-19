#include "AppBase_pch.h"

#include "Input.h"
#include <Util\Exceptions.h>

namespace spad
{

MouseState MouseState::prevState;
KeyboardState KeyboardState::prevState;
BYTE KeyboardState::currState[256];

MouseState::MouseState() :    X (0),
                            Y (0),
                            DX (0),
                            DY (0),
                            IsOverWindow (false)
{

}

MouseState MouseState::GetMouseState(HWND hwnd)
{
    POINT pos;
	if ( !GetCursorPos( &pos ) )
		THROW_WIN_EXCEPTION( "GetCursorPos failed.", GetLastError() );

    if (hwnd != NULL)
        if(!ScreenToClient(hwnd, &pos))
			THROW_WIN_EXCEPTION( "ScreenToClient failed.", GetLastError() );

    MouseState newState;
    newState.X = pos.x;
    newState.Y = pos.y;
    newState.DX = pos.x    - prevState.X;
    newState.DY = pos.y    - prevState.Y;

    newState.LButton.Pressed = (GetKeyState(VK_LBUTTON) & 0x8000) > 0;
    newState.LButton.RisingEdge = newState.LButton.Pressed && !prevState.LButton.Pressed;
    newState.LButton.RisingEdge = !newState.LButton.Pressed && prevState.LButton.Pressed;

    newState.MButton.Pressed = (GetKeyState(VK_MBUTTON) & 0x8000) > 0;
    newState.MButton.RisingEdge = newState.MButton.Pressed && !prevState.MButton.Pressed;
    newState.MButton.RisingEdge = !newState.MButton.Pressed && prevState.MButton.Pressed;

    newState.RButton.Pressed = (GetKeyState(VK_RBUTTON) & 0x8000) > 0;
    newState.RButton.RisingEdge = newState.RButton.Pressed && !prevState.RButton.Pressed;
    newState.RButton.RisingEdge = !newState.RButton.Pressed && prevState.RButton.Pressed;

    if (hwnd != NULL)
    {
        RECT clientRect;
        if (!::GetClientRect(hwnd, &clientRect))
			THROW_WIN_EXCEPTION( "GetClientRect failed.", GetLastError() );
		newState.IsOverWindow = ( pos.x >= 0 && pos.x < clientRect.right
                                    && pos.y >= 0 && pos.y < clientRect.bottom);
    }
    else
        newState.IsOverWindow = false;

    prevState = newState;
    return prevState;
}

void MouseState::SetCursorPos(int x, int y, HWND hwnd)
{
    POINT pos;
    pos.x = x;
    pos.y = y;

    if (hwnd != NULL)
        if (!ClientToScreen(hwnd, &pos))
			THROW_WIN_EXCEPTION( "ClientToScreen failed.", GetLastError() );

    if (!::SetCursorPos(pos.x, pos.y))
		THROW_WIN_EXCEPTION( "SetCursorPos failed.", GetLastError() );
}

KeyState KeyboardState::GetKeyState(Keys key) const
{
    return keyStates[key];
}

bool KeyboardState::IsKeyDown(Keys key) const
{
    return keyStates[key].Pressed;
}

bool KeyboardState::RisingEdge(Keys key) const
{
    return keyStates[key].RisingEdge;
}

KeyboardState KeyboardState::GetKeyboardState(HWND hwnd)
{
    if(GetForegroundWindow() != hwnd)
        return prevState;

    ::GetKeyboardState(currState);

    KeyState state;
    for (UINT i = 0; i < 256; ++i)
    {
        state.Pressed = KeyPressed(currState[i]);
        state.RisingEdge = state.Pressed && !prevState.keyStates[i].Pressed;
        state.FallingEdge = !state.Pressed && prevState.keyStates[i].Pressed;
        prevState.keyStates[i] = state;
    }

    return prevState;
}

KeyState::KeyState() :  Pressed(false),
                        RisingEdge(false),
                        FallingEdge(false)
{
}

}