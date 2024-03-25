cls
echo off

:: Convert spec into interop library. TODO1 test

if "%LBOT%"=="" (echo Fail: requires env var LBOT set to LuaBagOfTricks & exit 1)

:: Set the lua path.
set LUA_PATH=;;%LBOT%\?.lua;
rem set LUA_PATH=;;%LBOT%\?.lua;%LBOT%\Test\?.lua;

echo %LUA_PATH%
echo %~dp0

:: Build the interop.
pushd "%LBOT%"
lua gen_interop.lua -cs %~dp0\api_spec.lua %~dp0
popd
