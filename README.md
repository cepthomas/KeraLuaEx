# KeraLuaEx

KeraLuaEx is a modified version of [KeraLua 1.3.4](https://github.com/NLua/KeraLua/tree/v1.3.4)
with new capabilities and some limitations.

It's not a branch because it does not support the multitude of targets that the original does.
The core KeraLua code is cleaned up but structurally and functionally the same.

# Components
- `KeraLuaEx` is the core standalone library. It has no external dependencies.
- `Test` is the NUnit project; some of the original tests are carried over for regression.
- `Host` is a WinForms project which makes it easier for initial development
  and debugging because NUnit is a bit clumsy for that.

# Significant Changes

## Innards
- Uses Lua 5.4.6 x64.
- .NET6/C# Windows SDK project only.
- Turned on nullable.
- Integers fixed at 32 bit.

## Functional
- Added a `DataTable` class to simplify passing data back and forth between C# and Lua. Limited to homogenous arrays
  and string-keyed dictionaries.
- `ToNumberX()` and `ToIntegerX()` are removed and plain `ToNumber()` and `ToInteger()` now return nullables.
- Error handling:
  - Option to throw exceptions (default) or return LuaStatus codes. Checking is implemented in these functions:
        `Load()`, `LoadBuffer()`, `LoadFile()`, `LoadString()`, `PCall()`, `PCallK()`, `Resume()`.
  - `Error()` is not used internally.
  - Removed most arg checking - it was kind of sparse. Client will have to handle things like null argument exceptions etc.

## Cosmetic
- Original term `State` changed to `L`.
- Removed lots of overloaded funcs, useing default args instead.
- Removed expression-bodied members because I prefer to reserve `=>` for lambdas only.

## Script Structure
Scripts can be structured as "everything is a global" or a more modular approach (see LuaExTests.cs):
- Global: see `ScriptWithGlobal()` in conjunction with the script `luaex.lua`.
- Modular: see `ScriptWithModule()` in conjunction with the script `luaex_mod.lua`.

In reality the simple global approach makes more sense for plain old scripts but the option is there to mix and match.

The `ApiLib` class demonstrates how either approach integrates with the application (in this case the test suites).
