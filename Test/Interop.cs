///// Warning - this file is created by gen_interop.lua - do not edit. /////

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using KeraLuaEx;
using System.Diagnostics;

namespace KeraLuaEx.Test
{
    public partial class Host
    {
        #region ============= C# => KeraLuaEx functions =============

        /// <summary>Lua export function: Host asks script to do something</summary>
        /// <param name="arg_one">a string</param>
        /// <param name="arg_two">an integer</param>
        /// <returns>TableEx some calculated values></returns>
        public TableEx? DoOperation(string arg_one, int arg_two)
        {
            int numArgs = 0;
            int numRet = 1;

            // Get function.
            LuaType ltype = _l.GetGlobal("do_operation");
            if (ltype != LuaType.Function) { throw new SyntaxException("", -1, $"Invalid lua function: do_operation"); }

            // Push arguments.
            _l.PushString(arg_one);
            numArgs++;
            _l.PushInteger(arg_two);
            numArgs++;

            // Do the actual call.
            LuaStatus lstat = _l.DoCall(numArgs, numRet);
            if (lstat >= LuaStatus.ErrRun) { throw new LuaException("", -1, lstat, "DoCall() failed"); }

            // Get the results from the stack.
            TableEx? ret = _l.ToTableEx(-1);
            if (ret is null) { throw new SyntaxException("", -1, "DoOperation return value is not a TableEx"); }
            _l.Pop(1);
            return ret;
        }

        #endregion

        #region ============= KeraLuaEx => C# callback functions =============s
        
        /// <summary>Host export function: Print something for the user
        /// Lua arg: "msg" What to tell
        /// Lua return: bool Status
        /// </summary>
        /// <param name="p">Internal lua state</param>
        /// <returns>Number of lua return values></returns>
        int PrintEx(IntPtr p)
        {
            Lua l = Lua.FromIntPtr(p)!;

            // Get arguments
            string? msg = null;
            if (l.IsString(1)) { msg = l.ToString(1); }
            else { throw new SyntaxException("", -1, "Invalid arg type: printex(msg)"); }

            // Do the work. Always one result.
            bool ret = PrintExCb(msg);
            l.PushBoolean(ret);
            return 1;
        }

        /// <summary>Host export function: Get current timer value
        /// Lua arg: "on" On or off
        /// Lua return: double Number of msec
        /// </summary>
        /// <param name="p">Internal lua state</param>
        /// <returns>Number of lua return values></returns>
        int Timer(IntPtr p)
        {
            Lua l = Lua.FromIntPtr(p)!;

            // Get arguments
            bool? on = null;
            if (l.IsBoolean(1)) { on = l.ToBoolean(1); }
            else { throw new SyntaxException("", -1, "Invalid arg type: timer(on)"); }

            // Do the work. Always one result.
            double ret = TimerCb(on);
            l.PushNumber(ret);
            return 1;
        }

        #endregion

        #region ============= Infrastructure =============

        readonly List<LuaRegister> _libFuncs = new();
        readonly Lua _l = new ();

        int OpenInterop(IntPtr p)
        {
            var l = Lua.FromIntPtr(p)!;
            l.NewLib(_libFuncs.ToArray());
            return 1;
        }

        void LoadInterop()
        {
            _libFuncs.Add(new LuaRegister("printex", PrintEx));
            _libFuncs.Add(new LuaRegister("timer", Timer));
            _libFuncs.Add(new LuaRegister(null, null));
            _l.RequireF("luainterop", OpenInterop, true);
        }

        void LoadScript(string scriptFn, List<string> lbotDirs)
        {
            _l.SetLuaPath(lbotDirs);
            LuaStatus lstat = _l.LoadFile(scriptFn);
            if (lstat >= LuaStatus.ErrRun) { throw new LuaException("", -1, lstat, "LoadScript() failed"); }
            // Run it.
            _l.PCall(0, Lua.LUA_MULTRET, 0);
            // Reset stack.
            _l.SetTop(0);
        }
        #endregion
    }
}
