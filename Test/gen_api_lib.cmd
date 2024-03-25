cls
echo off

:: Convert spec into interop library.

if "%LBOT%"=="" (echo Fail: requires env var LBOT set to LuaBagOfTricks & exit 1)

set LUA_PATH=;;%LBOT%\?.lua;

:: Build the interop.
pushd "%LBOT%"
lua gen_interop.lua -cs %~dp0api_spec.lua %~dp0
popd
