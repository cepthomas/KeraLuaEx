using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;


namespace KeraLuaEx.Test
{
    public class Common
    {
        /// <summary>Log message event.</summary>
        public static event EventHandler<string>? LogMessage;

        /// <summary>
        /// Log the message.
        /// </summary>
        /// <param name="msg"></param>
        public static void Log(string msg)
        {
            LogMessage?.Invoke(null, msg);
        }

        /// <summary>
        /// Sets package.path.
        /// </summary>
        /// <param name="l"></param>
        /// <param name="paths"></param>
        public static void SetLuaPath(Lua l, List<string> paths)
        {
            List<string> parts = new() { "?", "?.lua" };
            paths.ForEach(p => parts.Add(Path.Join(p, "?.lua").Replace('\\', '/')));
            string luapath = string.Join(';', parts);
            string s = $"package.path = \"{luapath}\"";
            l.DoString(s);
        }

        /// <summary>
        /// Get the dir name of the caller's source file.
        /// </summary>
        /// <param name="callerPath"></param>
        /// <returns>Caller source dir.</returns>
        public static string GetSourcePath([CallerFilePath] string callerPath = "")
        {
            var dir = Path.GetDirectoryName(callerPath)!;
            return dir;
        }

        /// <summary>
        /// Check the stack size.
        /// </summary>
        /// <param name="l"></param>
        /// <param name="expected"></param>
        /// <param name="file">Ignore - compiler use.</param>
        /// <param name="line">Ignore - compiler use.</param>
        /// <exception cref="LuaException"></exception>
        public static void EvalStackSize(Lua l, int expected, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
        {
            int num = l.GetTop();
            if (num != expected)
            {
                var s = $"{file}({line}): Expected {expected} stack but is {num}";
                Log(s);
                //throw new LuaException(s); TODO2
            }
        }

        /// <summary>
        /// Generic get a simple global value. Restores stack.
        /// </summary>
        /// <param name="l"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static object? GetGlobalValue(Lua l, string name) //TODOF
        {
            object? val = null;

            LuaType t = l.GetGlobal(name);
            switch (t)
            {
                case LuaType.String:
                    val = l.ToStringL(-1)!;
                    break;
                case LuaType.Boolean:
                    val = l.ToBoolean(-1);
                    break;
                case LuaType.Number:
                    // Ternary op doesn't work so...
                    if (l.IsInteger(-1)) { val = l.ToInteger(-1)!; }
                    else { val = l.ToNumber(-1)!; }
                    break;
                case LuaType.Table:
                    val = l.ToDataTable();
                    break;
                default:
                    //throw new ArgumentException($"Unsupported type {t} for {name}");
                    val = null;
                    break;
            }

            // Restore stack from GetGlobal().
            l.Pop(1);

            return val;
        }

        /// <summary>
        /// Dump the globals.
        /// </summary>
        /// <param name="l"></param>
        /// <returns></returns>
        public static List<string> DumpGlobals(Lua l)
        {
            l.PushGlobalTable();
            var sg3 = DumpRawTable(l, "globals", 0, true);
            l.Pop(1); // from PushGlobalTable()

            return sg3;
        }

        /// <summary>
        /// Dump the stack.
        /// </summary>
        /// <param name="l"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public static List<string> DumpStack(Lua l, string info = "") //TODOF
        {
            List<string> ls = new() { info, "Stack:" };

            int num = l.GetTop();

            if (num > 0)
            {
                for (int i = 1; i <= num; i++)
                {
                    LuaType t = l.Type(i);
                    string tinfo = $"[{i}]({t}):";
                    string s = t switch
                    {
                        LuaType.String => $"{tinfo}{l.ToStringL(i)}",
                        LuaType.Boolean => $"{tinfo}{l.ToBoolean(i)}",
                        LuaType.Number => $"{tinfo}{(l.IsInteger(i) ? l.ToInteger(i) : l.ToNumber(i))}",
                        LuaType.Nil => $"{tinfo}nil",
                        LuaType.Table => $"{tinfo}{l.ToStringL(i) ?? "null"}",
                        _ => $"{tinfo}{l.ToPointer(i):X}",
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="lsin"></param>
        /// <param name="indent"></param>
        /// <returns></returns>
        public static string FormatDump(string name, List<string> lsin, bool indent) //TODOF
        {
            string sindent = indent ? "    " : "";
            var lines = new List<string> { $"{name}:" };
            lsin.ForEach(s => lines.Add($"{sindent}{s}"));
            var s = string.Join(Environment.NewLine, lines);
            return s;
        }

        /// <summary>
        /// Dump the table at the top of the stack.
        /// </summary>
        /// <param name="l"></param>
        /// <param name="tableName"></param>
        /// <param name="indent"></param>
        /// <param name="all"></param>
        /// <returns></returns>
        public static List<string>? DumpRawTable(Lua l, string tableName, int indent, bool all) //TODOF
        {
            if (indent > 1)
            {
                return null;
            }

            var sindent = indent > 0 ? new(' ', 4 * indent) : "";
            List<string> ls = new() { $"{sindent}{tableName}(table):" };
            sindent += "    ";

            // Put a nil key on stack.
            l.PushNil();

            // Key(-1) is replaced by the next key(-1) in table(-2).
            while (l.Next(-2))
            {
                // Get key(-2) info.
                LuaType keyType = l.Type(-2);
                string key = l.ToStringL(-2)!;

                // Get type of value(-1).
                LuaType valType = l.Type(-1);
                object? val = null;

                switch (valType)
                {
                    case LuaType.String:
                        val = l.ToStringL(-1)!.Replace("\0", @"\0"); // fix embedded 0
                        break;
                    case LuaType.Boolean:
                        val = l.ToBoolean(-1);
                        break;
                    case LuaType.Number:
                        val = l.ToNumber(-1);
                        break;
                    case LuaType.Table:
                        var lsx = DumpRawTable(l, key, indent + 1, all); // recursion!
                        if (lsx is not null)
                        {
                            ls.AddRange(lsx);
                        }
                        break;
                    case LuaType.Function:
                        if (all)
                        {
                            val = l.ToStringL(-1)!;
                        }
                        break;
                    case LuaType.LightUserData:
                    case LuaType.UserData:
                    case LuaType.Thread:
                        // ignore
                        break;
                }

                if (val is not null)
                {
                    ls.Add($"{sindent}{key}:{val}");
                }

                // Remove value(-1), now key on top at(-1).
                l.Pop(1);
            }

            if (ls.Count <= 1)
            {
                ls.Add($"{sindent}Empty");
            }

            return ls;
        }


        /// <summary>
        /// Format value for display.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        /// <exception cref="SyntaxException"></exception>
        public static string FormatCsharpVal(string name, object? val) //TODOF used??
        {
            string s = "???";

            s = val switch
            {
                int _ => $"{name}(int):{val}",
                long _ => $"{name}(long):{val}",
                double _ => $"{name}(double):{val}",
                bool _ => $"{name}(bool):{val}",
                string _ => $"{name}(string):{val}",
                DataTable _ => $"{name}(table):{val}",
                null => $"{name}:null",
                _ => throw new SyntaxException($"Unsupported type:{val.GetType()} for {name}"),
            };

            return s;
        }

        
    }
}
