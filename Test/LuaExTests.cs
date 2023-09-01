﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using NUnit.Framework;
using static KeraLuaEx.Test.TestUtils;


namespace KeraLuaEx.Test
{
    [TestFixture]
    public class LuaExTests
    {
        /// <summary>Lua context.</summary>
        Lua? _l;

        [SetUp]
        public void Setup()
        {
            _l?.Close();
            _l = new Lua();
            ApiLib.Load(_l);
        }

        [TearDown]
        public void TearDown()
        {
            _l?.Close();
            _l = null;
        }

        [Test]
        public void ScriptWithGlobal()
        {
            ///// Load everything.
            LoadScript("luaex.lua");

            // PCall loads the file.
            _l.PCall(0, Lua.LUA_MULTRET, 0);

            //// Reset stack.
            _l.SetTop(0);
            _l.CheckStackSize(0);

            ///// Look at globals.
            {
                LuaType t = _l.GetGlobal("g_number"); // push lua value onto stack
                Assert.AreEqual(LuaType.Number, t);
                var num = _l.ToNumber(-1);
                Assert.IsInstanceOf<double>(num);
                Assert.AreEqual(7.654, num);
                _l.Pop(1); // Clean up from GetGlobal().
            }

            _l.CheckStackSize(0);

            {
                LuaType t = _l.GetGlobal("g_int"); // push lua value onto stack
                Assert.AreEqual(LuaType.Number, t);
                var i = _l.ToInteger(-1);
                Assert.IsInstanceOf<int>(i);
                Assert.AreEqual(80808, i);
                _l.Pop(1); // Clean up from GetGlobal().
            }

            _l.CheckStackSize(0);

            {
                LuaType t = _l.GetGlobal("g_list_number"); // push lua value onto stack
                Assert.AreEqual(LuaType.Table, t);
                var tbl = _l.ToTableEx(99, true);
                var list = tbl.ToList<double>();
                Assert.AreEqual(4, list.Count);
                Assert.AreEqual(2.303, list[3]);
                _l.Pop(1); // Clean up from GetGlobal().
            }

            _l.CheckStackSize(0);

            {
                LuaType t = _l.GetGlobal("g_list_int"); // push lua value onto stack
                Assert.AreEqual(LuaType.Table, t);
                var tbl = _l.ToTableEx(99, true);
                var list = tbl.ToList<int>();
                Assert.AreEqual(4, list.Count);
                Assert.AreEqual(98, list[2]);
                //var ex = Assert.Throws<KeyNotFoundException>(() => { object _ = ls[22]; });
                _l.Pop(1); // Clean up from GetGlobal().
            }

            _l.CheckStackSize(0);

            {
                LuaType t = _l.GetGlobal("g_table"); // push lua value onto stack
                Assert.AreEqual(LuaType.Table, t);
                var tbl = _l.ToTableEx(99, false);
                Assert.AreEqual(3, tbl.Count);
                Assert.AreEqual("bing_bong", tbl["dev_type"]);
                _l.Pop(1); // Clean up from GetGlobal().
            }

            _l.CheckStackSize(0);

            {
                LuaType t = _l.GetGlobal("things"); // push lua value onto stack
                Assert.AreEqual(LuaType.Table, t);
                var tbl = _l.ToTableEx(99, false);
                Assert.AreEqual(4, tbl.Count);

                var whiz = tbl["whiz"] as TableEx;
                Assert.IsInstanceOf<TableEx>(whiz);
                Assert.AreEqual(3, whiz.Count);
                Assert.AreEqual(99, whiz["channel"]);
                //var ex = Assert.Throws<KeyNotFoundException>(() => { object _ = whiz["booga"]; });

                var dtbl = whiz["double_table"] as TableEx;
                Assert.IsInstanceOf<TableEx>(dtbl);
                var list = dtbl.ToList<double>();
                Assert.AreEqual(3, list.Count);
                Assert.AreEqual(909.555, list[2]);
                _l.Pop(1); // Clean up from GetGlobal().
            }

            _l.CheckStackSize(0);

            ///// Execute a lua function.
            {
                LuaType t = _l.GetGlobal("g_func");
                Assert.AreEqual(LuaType.Function, t);

                // Push the arguments.
                var s = "az9011 birdie";
                _l.PushString(s);

                // Do the call.
                _l.PCall(1, 1, 0);

                // Get result.
                var resi = _l.ToInteger(-1);
                Assert.AreEqual(s.Length + 3, resi);
                _l.Pop(1); // Clean up from GetGlobal().
            }

            _l.CheckStackSize(0);

            ///// Execute a more complex lua function.
            {
                LuaType t = _l.GetGlobal("calc");
                Assert.AreEqual(LuaType.Function, t);
                _l.CheckStackSize(1);

                // Push the arguments.
                var addends = new List<int>() { 3901, 488, 922, 1578, 2406 };
                _l.PushList(addends);
                var suffix = "__the_end__";
                _l.PushString(suffix);
                _l.CheckStackSize(3);

                // Do the call.
                _l.PCall(2, 1, 0); //attempt to call a number value
                _l.CheckStackSize(1);

                // Get the results from the stack.
                var tbl = _l.ToTableEx(2, false);
                Assert.IsInstanceOf<TableEx> (tbl);
                _l.Pop(1);
                Assert.AreEqual(2, tbl.Count);
                Assert.AreEqual(">>>9295___the_end__<<<", tbl["str"]);
                Assert.AreEqual(9295, tbl["sum"]);
            }

            _l.CheckStackSize(0);
        }

        [Test]
        public void ScriptWithModule()
        {
            ///// Load everything.
            LoadScript("luaex_mod.lua");

            // PCall loads the file.
            _l.PCall(0, Lua.LUA_MULTRET, 0);

            // Top of the stack is the module itself. Saves it for later.
            _l.SetGlobal("luaex_mod");

            // Reset stack.
            _l.SetTop(0);

            _l.CheckStackSize(0);

            ///// Look at globals.
            {
                LuaType t = _l.GetGlobal("g_int"); // push lua value onto stack
                Assert.AreEqual(LuaType.Number, t);
                var i = _l.ToInteger(-1);
                Assert.IsInstanceOf<int>(i);
                Assert.AreEqual(71717, i);
                _l.Pop(1); // Clean up from GetGlobal().
            }

            ///// Look inside module.
            _l.GetGlobal("luaex_mod"); // push module onto stack

            {
                LuaType t = _l.GetField(-1, "m_string"); // push lua value onto stack
                Assert.AreEqual(LuaType.String, t);
                var s = _l.ToStringL(-1);// !, // assign, no pop
                _l.Pop(1); // Clean up from GetField().
                Assert.IsInstanceOf<string>(s);
                Assert.AreEqual("Here I am", s);
            }

            _l.CheckStackSize(1); // luaex_mod is on top of stack

            {
                LuaType t = _l.GetField(-1, "m_bool"); // push lua value onto stack
                Assert.AreEqual(LuaType.Boolean, t);
                var b = _l.ToBoolean(-1);// !, // assign, no pop
                _l.Pop(1); // Clean up from GetField().
                Assert.IsInstanceOf<bool>(b);
                Assert.AreEqual(false, b);
            }

            _l.CheckStackSize(1); // luaex_mod is on top of stack

            {
                LuaType t = _l.GetField(-1, "m_list_int"); // push lua value onto stack
                Assert.AreEqual(LuaType.Table, t);
                var tbl = _l.ToTableEx(99, true);
                var list = tbl.ToList<int>();
                _l.Pop(1); // Clean up from GetField().
                Assert.AreEqual(4, list.Count);
                Assert.AreEqual(98, list[2]);
                //var ex = Assert.Throws<KeyNotFoundException>(() => { object _ = ls[22]; });
            }

            _l.CheckStackSize(1); // luaex_mod is on top of stack

            {
                LuaType t = _l.GetField(-1, "m_table"); // push lua value onto stack
                Assert.AreEqual(LuaType.Table, t);
                var tbl = _l.ToTableEx(99, true);
                _l.Pop(1); // Clean up from GetField().
                Assert.AreEqual(3, tbl.Count);
                Assert.AreEqual("bing_bong", tbl["dev_type"]);
            }

            _l.CheckStackSize(1); // luaex_mod is on top of stack

            ///// Execute a module lua function.
            {
                _l.GetField(-1, "funcmod"); // push lua value onto stack

                // Push the arguments.
                var s = "az9011 birdie";
                _l.PushString(s);

                _l.CheckStackSize(3);

                // Do the call.
                _l.PCall(1, 1, 0);
                _l.CheckStackSize(2);

                // Get result.
                var resi = _l.ToInteger(-1);
                Assert.AreEqual(s.Length + 3, resi);
                _l.Pop(1); // Clean up returned value.
            }
            
            _l.CheckStackSize(1);

            ///// Execute a more complex lua function.
            {
                _l.GetField(-1, "calcmod");
                _l.CheckStackSize(2);

                // Push the arguments.
                var addends = new List<int>() { 3901, 488, 922, 1578, 2406 };
                var suffix = "__the_end__";
                _l.PushList(addends);
                _l.PushString(suffix);

                // Do the call.
                _l.PCall(2, 1, 0);

                // Get the results from the stack.
                var tbl = _l.ToTableEx(4, false);
                Assert.IsInstanceOf<TableEx>(tbl);
                Assert.AreEqual(2, tbl.Count);
                Assert.AreEqual(">>>9295___the_end__<<<", tbl["str"]);
                Assert.AreEqual(9295, tbl["sum"]);
                _l.Pop(1); // Clean up returned value.

                _l.CheckStackSize(1);
            }

            _l.Pop(1); // GetGlobal("luaex_mod")

            _l.CheckStackSize(0);
        }

        [Test]
        public void Play()
        {
            ///// Load everything.
            LoadScript("luaex.lua");

            // PCall loads the file.
            _l.PCall(0, Lua.LUA_MULTRET, 0);
            //The function results are pushed onto the stack in direct order (the first result is pushed first),
            //so that after the call the last result is on the top of the stack.
            // [1] is the luaex module, [2] is api_lib

            //// Reset stack.
            _l.SetTop(0);
            _l.CheckStackSize(0);

            //// Dump globals.
            //_l.PushGlobalTable();
            //var gl = DumpTable(_l, "globals", 0, false);
            //_l.Pop(1); // from PushGlobalTable()
            //Log(string.Join(Environment.NewLine, gl));

            try
            {
                _l.EvalLuaStatus(LuaStatus.ErrRun);
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }


            //_l.DumpStack();

            //if (_l.GetTop() > 0)
            //{
            //    var sg1 = DumpTable(_l, "[1]", 0, false);
            //    Log(Environment.NewLine + string.Join(Environment.NewLine, sg1));
            //    _l.Pop(1);
            //}
            //_l.CheckStackSize(0);

            //if (_l.GetTop() > 0)
            //{
            //    var sg2 = DumpTable(_l, "[2]", 0, false);
            //    Log(Environment.NewLine + string.Join(Environment.NewLine, sg2));
            //    _l.Pop(1);
            //}
            //_l.CheckStackSize(0);

            _l.CheckStackSize(0);
        }

        // Helper.
        void LoadScript(string fn)
        {
            string srcPath = GetSourcePath();
            string scriptsPath = Path.Combine(srcPath, "scripts");
            _l.SetLuaPath(new() { scriptsPath });
            string scriptFile = Path.Combine(scriptsPath, fn);
            _l!.LoadFile(scriptFile);
        }
    }
}
