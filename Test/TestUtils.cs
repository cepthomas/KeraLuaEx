using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;


namespace KeraLuaEx.Test
{
    public class TestUtils
    {
        #region Core stuff
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
        #endregion

        #region Test and debug helpers
        /// <summary>
        /// Generic get a simple global value. Restores stack.
        /// </summary>
        /// <param name="l"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static object? GetGlobalValue(Lua l, string name)
        {
            object? val = null;

            LuaType t = l.GetGlobal(name);

            val = t switch
            {
                LuaType.String => l.ToStringL(-1)!,
                LuaType.Boolean => l.ToBoolean(-1),
                LuaType.Number => l.DetermineNumber(-1),
                LuaType.Table => l.ToDataTable(),
                _ => throw new ArgumentException($"Unsupported type {t} for {name}"),
            };

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
            var gl = DumpRawTable(l, "globals", 0, false);
            l.Pop(1); // from PushGlobalTable()
            return gl;
        }

        /// <summary>
        /// Dump the table at the top of the stack.
        /// </summary>
        /// <param name="l"></param>
        /// <param name="tableName"></param>
        /// <param name="indent"></param>
        /// <param name="all"></param>
        /// <returns></returns>
        public static List<string> DumpRawTable(Lua l, string tableName, int indent, bool all)
        {
            List<string> ls = new();
            if (indent < 2)
            {
                var sindent = indent > 0 ? new(' ', 4 * indent) : "";
                ls.Add($"{sindent}{tableName}(table all={all}):");
                sindent += "    ";

                // Put a nil key on stack.
                l.PushNil();

                // Key(-1) is replaced by the next key(-1) in table(-2).
                while (l.Next(-2))
                {
                    // Get key(-2) info.
                    LuaType keyType = l.Type(-2);
                    //string key = l.ToStringL(-2)!;
                    object key = keyType switch
                    {
                        LuaType.String => l.ToStringL(-2)!,
                        LuaType.Number => l.DetermineNumber(-2)!,
                        _ => throw new SyntaxException($"Unsupported key type {keyType} for {l.ToStringL(-2)}")
                    };

                    // Get type of value(-1).
                    LuaType valType = l.Type(-1);
                    string st = valType.ToString().ToLower();
                    object? val = null;

                    val = valType switch
                    {
                        LuaType.String => l.ToStringL(-1)!.Replace("\0", @"\0"), // fix occasional embedded 0
                        LuaType.Boolean => l.ToBoolean(-1),
                        LuaType.Number => l.ToNumber(-1),
                        LuaType.Table => DumpRawTable(l, key.ToString()!, indent + 1, all), // recursion!
                        LuaType.Function => all ? l.ToPointer(-1) : null,
                        _ => all ? l.ToStringL(-1)! : null,
                    };

                    switch (val)
                    {
                        case List<string> lsx: ls.AddRange(lsx); break;
                        case null: break;
                        default: ls.Add($"{sindent}{key}:{val}({st})"); break;
                    }

                    // Remove value(-1), now key on top at(-1).
                    l.Pop(1);
                }

                if (ls.Count < 2) // always one line minimum
                {
                    ls.Add($"{sindent}Empty");
                }
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
        public static string FormatCsharpVal(string name, object? val)
        {
            string s = val switch
            {
                int => $"{name}:{val}(integer)",
                long => $"{name}:{val}(long)",
                double => $"{name}:{val}(double)",
                bool => $"{name}:{val}(bool)",
                string => $"{name}:{val}(string)",
                DataTable => $"{name}:{val}(table)",
                null => $"{name}:null",
                _ => throw new SyntaxException($"Unsupported type:{val.GetType()} for {name}"),
            };
            return s;
        }
        #endregion
    }
}
