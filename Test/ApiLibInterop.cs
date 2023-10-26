///// Warning - this file is created by gen_interop.lua, do not edit. /////
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using KeraLuaEx;
using System.Diagnostics;

namespace KeraLuaEx.Test
{
    public partial class ApiLib
    {
        #region Functions exported from lua for execution by host
        /// <summary>Lua export function: Host calls lua</summary>
        /// <param name="arg_one">some strings</param>
        /// <param name="arg_two">a nice integer</param>
        /// <returns>TableEx a returned thing></returns>
        public TableEx? HostCallLua(string arg_one, int arg_two)
        {
            int numArgs = 0;
            int numRet = 1;

            // Get function.
            LuaType ltype = _l.GetGlobal("host_call_lua");
            if (ltype != LuaType.Function) { ErrorHandler(new SyntaxException($"Bad lua function: host_call_lua")); return null; }

            // Push arguments
            _l.PushString(arg_one);
            numArgs++;
            _l.PushInteger(arg_two);
            numArgs++;

            // Do the actual call.
            LuaStatus lstat = _l.DoCall(numArgs, numRet);
            if (lstat >= LuaStatus.ErrRun) { ErrorHandler(new SyntaxException("DoCall() failed")); return null; }

            // Get the results from the stack.
            TableEx? ret = _l.ToTableEx(-1);
            if (ret is null) { ErrorHandler(new SyntaxException("Return value is not a TableEx")); return null; }
            _l.Pop(1);
            return ret;
        }

        #endregion

        #region Functions exported from host for execution by lua
        /// <summary>Host export function: Print something for the user
        /// Lua arg: "msg">What to tell
        /// Lua return: bool Status>
        /// </summary>
        /// <param name="p">Internal lua state</param>
        /// <returns>Number of lua return values></returns>
        int PrintEx(IntPtr p)
        {
            Lua l = Lua.FromIntPtr(p)!;

            // Get arguments
            string? msg = null;
            if (l.IsString(1)) { msg = l.ToString(1); }
            else { ErrorHandler(new SyntaxException($"Bad arg type for {msg}")); return 0; }

            // Do the work. One result.
            bool ret = PrintExWork(msg);
            l.PushBoolean(ret);
            return 1;
        }

        /// <summary>Host export function: Get current timer value
        /// Lua arg: "on">On or off
        /// Lua return: double Number of msec>
        /// </summary>
        /// <param name="p">Internal lua state</param>
        /// <returns>Number of lua return values></returns>
        int Timer(IntPtr p)
        {
            Lua l = Lua.FromIntPtr(p)!;

            // Get arguments
            bool? on = null;
            if (l.IsBoolean(1)) { on = l.ToBoolean(1); }
            else { ErrorHandler(new SyntaxException($"Bad arg type for {on}")); return 0; }

            // Do the work. One result.
            double ret = TimerWork(on);
            l.PushNumber(ret);
            return 1;
        }

        #endregion

        #region Infrastructure
        // Bind functions to static instance.
        static ApiLib? _instance;
        // Bound functions.
        static LuaFunction? _PrintEx;
        static LuaFunction? _Timer;
        readonly List<LuaRegister> _libFuncs = new();

        int OpenInterop(IntPtr p)
        {
            var l = Lua.FromIntPtr(p)!;
            l.NewLib(_libFuncs.ToArray());
            return 1;
        }

        void LoadInterop()
        {
            _instance = this;
            _PrintEx = _instance!.PrintEx;
            _libFuncs.Add(new LuaRegister("printex", _PrintEx));
            _Timer = _instance!.Timer;
            _libFuncs.Add(new LuaRegister("timer", _Timer));

            _libFuncs.Add(new LuaRegister(null, null));
            _l.RequireF("api_lib", OpenInterop, true);
        }
        #endregion
    }
}
