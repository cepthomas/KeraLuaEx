cls
echo off

:: Convert spec into interop library.

:: Set the lua path.
set LUA_PATH=;;^
C:\Dev\repos\Lua\LuaBagOfTricks\?.lua;^
C:\Dev\repos\Lua\LuaBagOfTricks\Test\?.lua;

:: Build the interop. TODO need explicit paths - lua doesn't know file system.
pushd "C:\Dev\repos\Lua\LuaBagOfTricks"
lua gen_interop.lua -cs "C:\Dev\repos\Lua\KeraLuaEx\Test\api_spec.lua" "C:\Dev\repos\Lua\KeraLuaEx\Test"
popd
