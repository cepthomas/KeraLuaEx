using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using NUnit.Framework;


namespace KeraLuaEx.Test
{
    public class ApiLib
    {
        /// <summary>Bound lua function.</summary>
        readonly static LuaFunction _fPrint = PrintEx;

        /// <summary>Bound lua function.</summary>
        readonly static LuaFunction _fTimer = Timer;

        /// <summary>Bound lua function.</summary>
        readonly static LuaFunction _fLoad = Load;

        /// <summary>Metrics.</summary>
        static readonly Stopwatch _sw = new();
        static long _startTicks = 0;

        bool _disposed;

        #region Lifecycle
        public static void Init(Lua l)
        {
            // C:\Dev\repos\C\c_emb_lua\source\c\exec.c

            // Open std libs.
            l.OpenLibs();

            // Load app stuff. This table gets pushed on the stack and into globals.
            l.RequireF("api_test", _fLoad, true);

            // Pop the table off the stack as it interferes with calling the module function.
            l.Pop(1);

            _startTicks = 0;
            _sw.Start();
        }
        #endregion

        static int Load(IntPtr p)
        {
            var l = Lua.FromIntPtr(p)!;

            // Register our C <-> Lua functions.
            l.Register("printex", _fPrint);
            l.Register("timer", _fTimer);

            return 1;
        }

        #region Lua functions implemented in C#
        /// <summary>
        /// Called by lua script.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        static int PrintEx(IntPtr p)
        {
            var l = Lua.FromIntPtr(p)!;
            ///// Get arguments.
            var s = l.ToStringL(-1);
            ///// Do the work.
            Common.Log($"printex:{s}");
            ///// Return results.
            return 0;
        }

        /// <summary>
        /// Called by lua script.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        static int Timer(IntPtr p)
        {
            var l = Lua.FromIntPtr(p)!;
            ///// Get arguments.
            bool on = l.ToBoolean(-1);
            ///// Do the work.
            double totalMsec = 0;
            if (on)
            {
                _startTicks = _sw.ElapsedTicks; // snap
            }
            else if (_startTicks > 0)
            {
                long t = _sw.ElapsedTicks; // snap
                totalMsec = (t - _startTicks) * 1000D / Stopwatch.Frequency;
            }
            ///// Return results.
            l.PushNumber(totalMsec);
            return 1;
        }
        #endregion
    }
}
