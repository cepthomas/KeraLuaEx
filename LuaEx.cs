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

    /// <summary>
    /// Stuff added for KeraLuaEx.
    /// </summary>
    public partial class Lua
    {
        #region Properties
        /// <summary>On LuaStatus error either throw or return error code.</summary>
        public bool ThrowOnError { get; set; } = true;
        #endregion

        #region Simple logging
        public enum Category { DBG, INF, ERR };

        public class LogEventArgs : EventArgs
        {
            /// <summary>What it be.</summary>
            public Category Category { get; set; } = Category.ERR;
            /// <summary>The information.</summary>
            public string Message { get; set; } = "";
        }

        /// <summary>Client app can listen in.</summary>
        public static event EventHandler<LogEventArgs>? LogMessage;

        /// <summary>Log message event.</summary>
        public static void Log(Category cat, string msg)
        {
            LogMessage?.Invoke(null, new() { Category = cat, Message = msg });
        }
        #endregion

        #region Added API functions
        /// <summary>
        /// Make a TableEx from the lua table on the top of the stack.
        /// </summary>
        /// <returns>TableEx object or null if failed, check the log.</returns>
        public TableEx? ToTableEx()
        {
            TableEx? t = null;
            try
            {
                t = new(this);
            }
            catch (Exception ex)
            {
                // Stack is probably a mess so reset it. Implies that this is a fatal event.
                SetTop(0);
                Lua.Log(Lua.Category.ERR, ex.Message);
            }

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
            if (!(tv.Equals(typeof(string)) || tv.Equals(typeof(double)) || tv.Equals(typeof(int))))
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
        /// Check lua status and log an error. If ThrowOnError is true, throws an exception.
        /// </summary>
        /// <param name="lstat">Thing to look at.</param>
        /// <param name="file">Ignore - compiler use.</param>
        /// <param name="line">Ignore - compiler use.</param>
        /// <returns>True means error</returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="LuaException"></exception>
        public bool EvalLuaStatus(LuaStatus lstat, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
        {
            bool hasError = false;

            if (lstat >= LuaStatus.ErrRun)
            {
                hasError = true;

                // Get error message on stack.
                string s;
                if (GetTop() > 0)
                {
                    s = ToStringL(-1)!.Trim();
                    Pop(1); // remove
                }
                else
                {
                    s = "No error message!!!";
                }

                var serror = $"{lstat}:{s}";
                // serror = $"{file}({line}) [{lstat}]: {s}";

                if (ThrowOnError)
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
        /// Check the stack size and log if incorrect. If ThrowOnError is true, throws an exception.
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="file">Ignore - compiler use.</param>
        /// <param name="line">Ignore - compiler use.</param>
        /// <returns>True means error</returns>
        /// <exception cref="LuaException"></exception>
        public bool CheckStackSize(int expected, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
        {
            bool hasError = false;

            int num = GetTop();

            if (num != expected)
            {
                hasError = true;
                var serror = $"Stack size expected {expected} actual {num} at {file}({line})";

                Log(Category.ERR, serror);
                if (ThrowOnError)
                {
                    throw new LuaException(serror);
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
