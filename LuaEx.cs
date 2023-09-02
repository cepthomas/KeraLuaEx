using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Linq;
using System.Diagnostics;


namespace KeraLuaEx
{
    /// <summary>
    /// Stuff added for KeraLuaEx.
    /// </summary>
    public partial class Lua
    {
        #region Exceptions
        /// <summary>Lua script syntax error.</summary>
        public class SyntaxException : Exception
        {
            public SyntaxException() : base() { }
            public SyntaxException(string message) : base(message) { }
            public SyntaxException(string message, Exception inner) : base(message, inner) { }
        }

        /// <summary>Internal error on lua side.</summary>
        public class LuaException : Exception
        {
            public LuaException() : base() { }
            public LuaException(string message) : base(message) { }
            public LuaException(string message, Exception inner) : base(message, inner) { }
        }
        #endregion

        #region Simple logging
        /// <summary>Log message event.</summary>
        public static event EventHandler<string>? LogMessage;

        /// <summary>Client app can listen in.</summary>
        /// <param name="msg"></param>
        public static void Log(string msg)
        {
            LogMessage?.Invoke(null, msg);
        }
        #endregion

        #region Added API functions
        /// <summary>
        /// Make a TableEx from the lua table on the top of the stack.
        /// </summary>
        /// <param name="indent">Where to start.</param>
        /// <returns></returns>
        public TableEx ToTableEx(int indent = 0)
        {
            TableEx t = new();
            t.Create(this, indent);
            return t;
        }

        /// <summary>
        /// Push typed list onto lua stack.
        /// </summary>
        /// <param name="list"></param>
        public void PushList<T>(List<T> list)
        {
            // Check for supported types: int, double, string.
            var tv = typeof(T);
            if ( !(tv.Equals(typeof(string)) || tv.Equals(typeof(double)) || tv.Equals(typeof(int))))
            {
                throw new InvalidOperationException($"Unsupported value type [{tv}]");
            }

            // Create a new empty table and push it onto the stack.
            NewTable();

            // Add the values from the source.
            for (int i = 0; i < list.Count; i++)
            {
                PushInteger(i + 1);
                switch (list[i])
                {
                    case string v: PushString(v); break;
                    case int v: PushInteger(v); break;
                    case double v: PushNumber(v); break;
                }
                SetTable(-3);
            }
        }

        /// <summary>
        /// Push a dictionary onto lua stack. Ignores unsupported value types.
        /// </summary>
        /// <param name="dict"></param>
        public void PushDictionary(Dictionary<string, object> dict)
        {
            // Create a new empty table and push it onto the stack.
            NewTable();

            // Add the values from the source.
            foreach (var f in dict)
            {
                PushString(f.Key);
                switch (f.Value)
                {
                    case null: PushNil(); break;
                    case string s: PushString(s); break;
                    case bool b: PushBoolean(b); break;
                    case int i: PushInteger(i); break;
                    case double d: PushNumber(d); break;
                    case Dictionary<string, object> t: PushDictionary(t); break; // recursion!
                    default: break; // ignore
                }
                SetTable(-3);
            }
        }
        #endregion

        #region Helper functions
        /// <summary>
        /// Sets package.path for the context.
        /// </summary>
        /// <param name="paths"></param>
        public void SetLuaPath(List<string> paths)
        {
            List<string> parts = new() { "?", "?.lua" };
            paths.ForEach(p => parts.Add(Path.Join(p, "?.lua").Replace('\\', '/')));
            string s = string.Join(';', parts);
            s = $"package.path = \"{s}\"";
            DoString(s);
        }

        /// <summary>
        /// Converts to integer or number.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public object? DetermineNumber(int index)
        {
            // return IsInteger(index) ? ToInteger(index) : ToNumber(index); // ternary op doesn't work - some subtle typing thing?
            if (IsInteger(index)) { return ToInteger(index); }
            else if (IsNumber(index)) { return ToNumber(index); }
            else { return null; }
        }
        #endregion

        #region Quality control
        /// <summary>
        /// Check lua status. If ThrowOnError is true, throws an exception otherwise returns true for error.
        /// </summary>
        /// <param name="lstat">Thing to look at.</param>
        /// <param name="clientHandle">If true don't throw and let caller process it.</param>
        /// <param name="file">Ignore - compiler use.</param>
        /// <param name="line">Ignore - compiler use.</param>
        /// <returns>True means error</returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="LuaException"></exception>
        public bool EvalLuaStatus(LuaStatus lstat, bool clientHandle = false, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
        {
            bool hasError = false;
            var serror = "???";

            if (lstat >= LuaStatus.ErrRun)
            {
                hasError = true;
                // Log the stack so user can determine bad script file.
                GetGlobal("debug");
                var st = DumpStack();
                var sts = string.Join(Environment.NewLine, st);
                serror = $"{file}({line}): Failed lua status [{lstat}]{Environment.NewLine}{sts}";
                Pop(1); // clean up GetGlobal("debug").

                if (!clientHandle)
                {
                    throw lstat switch
                    {
                        LuaStatus.ErrFile => new FileNotFoundException(serror),
                        LuaStatus.ErrSyntax => new SyntaxException(serror),
                        _ => new LuaException(serror),
                    };
                }
            }

            return hasError;
        }

        /// <summary>
        /// Check the stack size and throw/log if incorrect.
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="clientHandle">If true don't throw and let caller process it.</param>
        /// <param name="file">Ignore - compiler use.</param>
        /// <param name="line">Ignore - compiler use.</param>
        public bool CheckStackSize(int expected, bool clientHandle = false, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
        {
            bool hasError = false;

            int num = GetTop();

            if (num != expected)
            {
                hasError = true;
                var serror = $"{file}({line}): Stack size expected [{expected}] actual [{num}]";
                if (!clientHandle)
                {
                    throw new LuaException(serror);
                }
                else
                {
                    Lua.Log(serror);
                }
            }

            return hasError;
        }
        #endregion

        #region Diagnostics
        /// <summary>
        /// Dump the contents of the stack.
        /// </summary>
        /// <param name="info"></param>
        /// <returns>List of strings.</returns>
        public List<string> DumpStack(string info = "")
        {
            List<string> ls = new();
            if (info != "")
            {
                ls.Add(info);
            }

            int num = GetTop();

            if (num > 0)
            {
                for (int i = 1; i <= num; i++)
                {
                    LuaType t = Type(i);
                    string st = t.ToString().ToLower();
                    string tinfo = $"    [{i}]:";

                    string s = t switch
                    {
                        LuaType.String => $"{tinfo}{ToStringL(i)}({st})",
                        LuaType.Boolean => $"{tinfo}{ToBoolean(i)}({st})",
                        LuaType.Number => $"{tinfo}{DetermineNumber(i)}({st})",
                        LuaType.Nil => $"{tinfo}nil",
                        LuaType.Table => $"{tinfo}{ToStringL(i) ?? "null"}({st})",
                        _ => $"{tinfo}{ToPointer(i):X}({st})",
                    };
                    ls.Add(s);

                    //Pop(1); // remove the value from stack
                }
            }
            else
            {
                ls.Add("Empty");
            }

            return ls;
        }
        #endregion
    }
}
