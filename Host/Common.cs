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

    public class Future_TODO2
    {
        // Lua calls C# functions
        public static object? LuaCallCsharp(IntPtr p, int numResults, params Type[] argTypes)
        {
            //object? ret = null;

            //var l = Lua.FromIntPtr(p);
            //int numArgs = l.GetTop();

            //if (argTypes.Length != numArgs)
            //{
            //    throw new SyntaxException(string.Join("|",  DumpStack())); // also "invalid func" or such
            //}

            // var noteString = l.l.ToString(1);
            // // Do the work.
            // List<int> notes = MusicDefinitions.GetNotesFromString(noteString);
            // l.PushList(notes);

            return numResults;
        }

        /// <summary>
        /// C# calls lua functions
        /// </summary>
        /// <param name="func"></param>
        /// <param name="retType"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static object? CsharpCallLua(string func, Type retType, params object[] args)
        {
            object? ret = null;
            Lua l = new();

            // Get the function to be called. Check return.
            LuaType gtype = l.GetGlobal(func);
            if (gtype != LuaType.Function) // optional?
            {
                //throw new SyntaxException(string.Join("|", UtilsEx.DumpStack(l))); // also "invalid func" or such
            }

            // Push the arguments to the call.
            int numArgs = args.Length;
            for (int i = 0; i < numArgs; i++)
            {
                switch (args[i])
                {
                    case string x:  l.PushString(x);    break;
                    case bool x:    l.PushBoolean(x);   break;
                    case int x:     l.PushInteger(x);   break;
                    case double x:  l.PushNumber(x);    break;
                    case float x:   l.PushNumber(x);    break;
                    //case List<int> x:
                    //case List<double> d:
                    //case List<string> s:
                    //case List<Table> b:
                    //    // convert to table and push.
                    //    break;

                    //default: throw new ArgumentException(string.Join("|", UtilsEx.DumpStack(l)));// also "invalid func" or such
                }
            }

            // Do the actual call.
            LuaStatus lstat = l.PCall(numArgs, retType is null ? 0 : 1, 0);

            l.EvalLuaStatus(lstat);

            // Get the results from the stack. Make generic???
            //object val = retType switch
            //{
            //    null => null,
            //    int => l.ToInteger(-1),
            //    double => l.IsNumber(-1),
            //    string => l.ToString(-1),
            //    bool => l.ToBoolean(-1),
            //    // ? table
            //    // ?? LuaType.Function:
            //    _ => throw new SyntaxException($"Invalid type:{retType}")
            //};

            // Restore stack from get.
            l.Pop(1);

            return ret;
        }
    }
}
