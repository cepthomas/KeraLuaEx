using System.Runtime.InteropServices;

namespace KeraLuaEx
{
    /// <summary>
    /// LuaRegister store the name and the delegate to register a native function
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct LuaRegister(string? name, LuaFunction? function)
    {
        /// <summary>
        /// Function name
        /// </summary>
        public string? name = name;

        /// <summary>
        /// Function delegate
        /// </summary>
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public LuaFunction? function = function;
    }
}
