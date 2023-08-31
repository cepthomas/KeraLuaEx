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
    public partial class Lua : IDisposable
    {
        /// <summary>
        /// Make a TableEx frin the lua table on the top of the stack.
        /// </summary>
        /// <param name="depth"></param>
        /// <param name="inclFuncs"></param>
        /// <returns></returns>
        public TableEx ToTableEx(int depth, bool inclFuncs)
        {
            TableEx t = new();
            t.Create(this, depth, inclFuncs);
            return t;
        }

        /// <summary>
        /// Push typed list onto lua stack.
        /// </summary>
        /// <param name="list"></param>
        public void PushList<T>(List<T> list)
        {
            // Check for supported types: double int string.
            var tv = typeof(T);
            if ( !(tv.Equals(typeof(string)) || tv.Equals(typeof(double)) || tv.Equals(typeof(int))))
            {
                throw new InvalidOperationException($"Unsupported value type {tv}");
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
        /// Push a dictionary onto lua stack.
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
                    default: throw new InvalidOperationException($"Unsupported type {f.Value.GetType()} for {f.Key}"); // should never happen
                }
                SetTable(-3);
            }
        }

        /// <summary>
        /// Check lua status. If _throwOnError is true, throws an exception otherwise returns true for error.
        /// </summary>
        /// <param name="lstat"></param>
        /// <param name="file">Ignore - compiler use.</param>
        /// <param name="line">Ignore - compiler use.</param>
        /// <returns>True means error</returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="LuaException"></exception>
        public bool EvalLuaStatus(LuaStatus lstat, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
        {
            bool hasError = false;
            _serror = "";

            if (lstat >= LuaStatus.ErrRun)
            {
                hasError = true;
                List<string> ls = new() // TODO0 ??
                {
                    $"Error:{lstat}",
                    $"Caller:{Path.GetFileName(file)}({line})"
                };

                GetGlobal("debug"); // ensures the source file info.

                var st = DumpStack();
                _serror = string.Join(Environment.NewLine, st);

                SetTop(0); // clean up GetGlobal("debug").
                //Pop(1); // This cores for some reason... TODO1

                if (ThrowOnError)
                {
                    throw lstat == LuaStatus.ErrFile ? new FileNotFoundException(_serror) : new LuaException(_serror);
                }
            }

            return hasError;
        }

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
            //return IsInteger(index) ? ToInteger(index) : ToNumber(index); // ternary op doesn't work - some subtle typing thing?
            if (IsInteger(index)) { return ToInteger(index); }
            else if (IsNumber(index)) { return ToNumber(index); }
            else { return null; }
        }
    }
}
