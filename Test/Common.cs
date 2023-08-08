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
        /// Generic get a simple global value. Restores stack.
        /// </summary>
        /// <param name="l"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static (object val, Type type) GetGlobalValue(Lua l, string name)
        {
            object val;
            Type type;

            LuaType t = l.GetGlobal(name);
            switch (t)
            {
                case LuaType.String:
                    val = l.ToStringL(-1)!;
                    type = val!.GetType();
                    break;
                case LuaType.Boolean:
                    val = l.ToBoolean(-1);
                    type = val.GetType();
                    break;
                case LuaType.Number:
                    if (l.IsInteger(-1))
                    {
                        val = l.ToInteger(-1)!;
                        type = val.GetType();
                    }
                    else
                    {
                        val = l.ToNumber(-1)!;
                        type = val.GetType();
                    }
                    break;
                case LuaType.Table:
                    val = l.ToDataTable();
                    type = val.GetType();
                    break;
                case LuaType.Nil:
                default:
                    throw new ArgumentException($"Unsupported type {t} for {name}");
            }

            // Restore stack from get.
            l.Pop(1);

            return (val, type);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="l"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public static string DumpStack(Lua l, string info = "")
        {
            List<string> ls = new() { info, "Stack:" };

            int num = l.GetTop();

            if (num > 0)
            {
                for (int i = num; i >= 1; i--)
                {
                    LuaType t = l.Type(i);
                    string tinfo = $"[{i}]({t}):";
                    string s = t switch
                    {
                        LuaType.String => $"{tinfo}{l.ToStringL(i)}",
                        LuaType.Boolean => $"{tinfo}{l.ToBoolean(i)}",
                        LuaType.Number => $"{tinfo}{(l.IsInteger(i) ? l.ToInteger(i) : l.ToNumber(i))}",
                        LuaType.Nil => $"{tinfo}nil",
                        //LuaType.Table => $"{tinfo}{l.ToString(i) ?? "null"}",
                        _ => $"{tinfo}{l.ToPointer(i):X}",
                    };
                    ls.Add(s);
                }
            }
            else
            {
                ls.Add("Empty");
            }

            return string.Join("  ", ls);
            //return string.Join(Environment.NewLine, ls);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="lsin"></param>
        /// <param name="indent"></param>
        /// <returns></returns>
        public static string FormatDump(string name, List<string> lsin, bool indent)
        {
            string sindent = indent ? "    " : "";
            var lines = new List<string> { $"{name}:" };
            lsin.ForEach(s => lines.Add($"{sindent}{s}"));
            var s = string.Join(Environment.NewLine, lines);
            return s;
        }
    }
}
