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
        public static string GetSourcePath([CallerFilePath] string callerPath = "") //UT/ALL
        {
            var dir = Path.GetDirectoryName(callerPath)!;
            return dir;
        }
    }
}
