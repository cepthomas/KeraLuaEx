# KeraLuaEx

KeraLuaEx is an evolved version of [KeraLua 1.3.4](https://github.com/NLua/KeraLua/tree/v1.3.4)
with new capabilities and limitations.

It's not a branch because it does not support the multitude of targets that the original does.
The core KeraLua code is cleaned up but structurally and functionally the same.

## Components
- `KeraLuaEx` is the core standalone library. It has no external dependencies.
- `Test` is the NUnit project; some of the original tests are carried over for regression.
- `Host` is a WinForms project which purports to make it easier for initial development
  and debugging because NUnit is a bit clumsy for that.

## Changes

### Innards
- Uses Lua 5.4.6.
- .NET6 Windows SDK project only.
- Turned on nullable.

### Functional
- Added a DataTable class to simplify passing data back and forth between C# and Lua. Limited to homogenous arrays
  and string-keyed dictionaries.
- ToNumberX() and ToIntegerX() are removed and plain ToNumber() and ToInteger() now return nullables.
- Error handling:
    - Option to throw exceptions (default) or return LuaStatus codes. Implemented in these functions:
        Load(), LoadBuffer(), LoadFile(), LoadString(), PCall(), PCallK(), Resume()
    - Error() is not used.
- Removed most arg checking - it's kind of random. Client will have to handle things like null argument exc.

### Cosmetic
- Original term "State" changed to "L".
- Removed lots of overloaded funcs, uses default args instead.
- Removed expression-bodied members because I prefer to reserve `=>` for lambdas only.

