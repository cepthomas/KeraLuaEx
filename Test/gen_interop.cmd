
echo off
cls

:: Convert spec into interop library.

set "ODIR=%cd%"
pushd ..\LBOT
set LUA_PATH=;;"%ODIR%\?.lua";?.lua;
lua gen_interop.lua -csh "%ODIR%\interop_spec.lua" "%ODIR%"
popd
