# KeraLuaEx

KeraLuaEx is a modified version of [KeraLua 1.3.4](https://github.com/NLua/KeraLua/tree/v1.3.4)
with new capabilities and some limitations.

It's not a branch because it does not support the multitude of targets that the original does.
The core KeraLua code is cleaned up but structurally and functionally the same.

# Components
- `KeraLuaEx` is the core standalone library. It has no external dependencies.
- `Test` is the NUnit project; some of the original tests are carried over for regression.
  It also contains a WinForms project which makes it easier for initial development
  and debugging because NUnit is a bit clumsy for that.

# Significant Changes

## Innards
- Uses Lua 5.4.6 x64. Windows only right now.
- .NET6/C# SDK project.
- Turned on nullable.
- Integers fixed at 32 bit.

## Functional
- Added a `TableEx` class to simplify passing data back and forth between C# and Lua. Supports:
  - Homogenous arrays of ints, doubles, or strings.
  - Dictionaries of objects with string keys.
- `ToNumberX()` and `ToIntegerX()` are removed and plain `ToNumber()` and `ToInteger()` now return nullables.
- Added `DoCall()` similar to the lua C function `docall()`. It captures the stack trace on error so the client can process.
- 
## Error Handling
- Original lib (Lua.cs) does not throw. New ones (LuaEx, TableEx) do.
  - Option to throw exceptions (default) in class `Lua` or return `LuaStatus` codes. Checking is implemented in these functions:
        `Load()`, `LoadBuffer()`, `LoadFile()`, `LoadString()`, `PCall()`, `PCallK()`, `Resume()`.
  -  `ToTableEx()` always throws exceptions because the errors (usually script syntax) can be deep in the hierarchy.
     Capture them by handling `LogMessage` event in your client.
  - `Error()` is not used internally.
  - Removed most arg checking - it was kind of sparse. Client will have to handle things like `NullArgumentException` etc.

## Cosmetic
- Variables with `State` changed to `L`.
- Removed lots of overloaded funcs, using default args instead.
- Removed expression-bodied members because I prefer to reserve `=>` for lambdas only.

## Script Structure
Scripts can be structured as "everything is a global" or a modular model (see `LuaExTests.cs`):
- Global: see `ScriptWithGlobal()` in conjunction with the script `luaex.lua`.
- Modular: see `ScriptWithModule()` in conjunction with the script `luaex_mod.lua`.


In reality the simple global approach makes more sense for plain old scripts but the option is there to mix and match.

## Api Approach
The `ApiLib` class demonstrates how either approach integrates with the host application (in this case the test suites).
It also demonstrates the use of interop code generation using [LuaBagOfTricks](https://github.com/cepthomas/LuaBagOfTricks).
