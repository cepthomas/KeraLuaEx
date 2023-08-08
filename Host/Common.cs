using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;


namespace KeraLuaEx.Host
{
    public class Utils
    {
        /// <summary>
        /// Format value for display.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        /// <exception cref="SyntaxException"></exception>
        public static string FormatCsharpVal(string name, object? val)
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

    public static class Extensions
    {
        /// <summary>
        /// Invoke helper, maybe. Extension method.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="action"></param>
        public static void InvokeIfRequired<T>(this T obj, InvokeIfRequiredDelegate<T> action) where T : ISynchronizeInvoke
        {
            if (obj is not null)
            {
                if (obj.InvokeRequired)
                {
                    obj.Invoke(action, new object[] { obj });
                }
                else
                {
                    action(obj);
                }
            }
        }
        public delegate void InvokeIfRequiredDelegate<T>(T obj) where T : ISynchronizeInvoke;
    }
}
