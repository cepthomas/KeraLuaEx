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
        public static (object? val, Type? type) GetGlobalValue(Lua l, string name)
        {
            object? val = null;
            Type? type = null;

            LuaType t = l.GetGlobal(name);
            switch (t)
            {
                case LuaType.Nil:
                    // Return defaults.
                    break;
                case LuaType.String:
                    val = l.ToString(-1);
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
                default:
                    throw new ArgumentException($"Unsupported type {t} for {name}");
            }

            // Restore stack from get.
            l.Pop(1);

            return (val, type);
        }
    }
}
