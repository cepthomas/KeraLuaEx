using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;


namespace KeraLuaEx
{
    /// <summary>Lua script syntax error.</summary>
    [Serializable]
    public class SyntaxException : Exception
    {
        // If new properties are added to the derived exception class, ToString() should be overridden to return the added information.
        public SyntaxException() : base() { }
        public SyntaxException(string message) : base(message) { }
        public SyntaxException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>Internal error on lua side.</summary>
    [Serializable]
    public class LuaException : Exception
    {
        public LuaException() : base() { }
        public LuaException(string message) : base(message) { }
        public LuaException(string message, Exception inner) : base(message, inner) { }
    }
}
