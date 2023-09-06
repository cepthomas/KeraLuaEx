using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using NUnit.Framework;



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

        /// <summary>Test script-as-globals.</summary>
        [Test]
        public void ScriptGlobal()
        {
            ///// Load everything.
            LoadTestScript("luaex.lua");

            // PCall loads the file.
            _l!.PCall(0, Lua.LUA_MULTRET, 0);

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
                var tbl = _l.ToTableEx(-1);
                Assert.IsInstanceOf<TableEx>(tbl);
                var list = tbl!.ToList<double>();
                Assert.AreEqual(4, list.Count);
                Assert.AreEqual(2.303, list[3]);
                _l.Pop(1); // Clean up from GetGlobal().
            }
            _l.CheckStackSize(0);

            {
                LuaType t = _l.GetGlobal("g_list_int"); // push lua value onto stack
                Assert.AreEqual(LuaType.Table, t);
                var tbl = _l.ToTableEx(-1);
                Assert.IsInstanceOf<TableEx>(tbl);
                var list = tbl!.ToList<int>();
                Assert.AreEqual(4, list.Count);
                Assert.AreEqual(98, list[2]);
                _l.Pop(1); // Clean up from GetGlobal().
            }
            _l.CheckStackSize(0);

            {
                LuaType t = _l.GetGlobal("g_table"); // push lua value onto stack
                Assert.AreEqual(LuaType.Table, t);
                var tbl = _l.ToTableEx(-1);
                Assert.IsInstanceOf<TableEx>(tbl);
                Assert.AreEqual(3, tbl!.Count);
                Assert.AreEqual("bing_bong", tbl["dev_type"]);
                _l.Pop(1); // Clean up from GetGlobal().
            }
            _l.CheckStackSize(0);

            {
                LuaType t = _l.GetGlobal("things"); // push lua value onto stack
                Assert.AreEqual(LuaType.Table, t);
                var tbl = _l.ToTableEx(-1);
                Assert.IsInstanceOf<TableEx>(tbl);
                Assert.AreEqual(4, tbl!.Count);

                var whiz = tbl["whiz"] as TableEx;
                Assert.IsInstanceOf<TableEx>(whiz);
                Assert.AreEqual(3, whiz!.Count);
                Assert.AreEqual(99, whiz["channel"]);

                var dtbl = whiz["double_table"] as TableEx;
                Assert.IsInstanceOf<TableEx>(dtbl);
                var list = dtbl!.ToList<double>();
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
                _l.Pop(1); // Clean up results.
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
                var tbl = _l.ToTableEx(-1);
                Assert.IsInstanceOf<TableEx> (tbl);
                Assert.AreEqual(2, tbl!.Count);
                Assert.AreEqual(">>>9295___the_end__<<<", tbl["str"]);
                Assert.AreEqual(9295, tbl["sum"]);

                _l.Pop(1); // Clean up results.
            }
            _l.CheckStackSize(0);
        }

        /// <summary>Test script-as-a-module.</summary>
        [Test]
        public void ScriptModule()
        {
            ///// Load everything.
            LoadTestScript("luaex_mod.lua");

            // PCall loads the file.
            _l!.PCall(0, Lua.LUA_MULTRET, 0);

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
                var tbl = _l.ToTableEx(-1);
                Assert.IsInstanceOf<TableEx>(tbl);
                var list = tbl!.ToList<int>();
                _l.Pop(1); // Clean up from GetField().
                Assert.AreEqual(4, list.Count);
                Assert.AreEqual(98, list[2]);
            }
            _l.CheckStackSize(1); // luaex_mod is on top of stack

            {
                LuaType t = _l.GetField(-1, "m_table"); // push lua value onto stack
                Assert.AreEqual(LuaType.Table, t);
                var tbl = _l.ToTableEx(-1);
                Assert.IsInstanceOf<TableEx>(tbl);
                _l.Pop(1); // Clean up from GetField().
                Assert.AreEqual(3, tbl!.Count);
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

                _l.Pop(1); // Clean up results.
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
                var tbl = _l.ToTableEx(-1);
                Assert.IsInstanceOf<TableEx>(tbl);
                Assert.AreEqual(2, tbl!.Count);
                Assert.AreEqual(">>>9295___the_end__<<<", tbl["str"]);
                Assert.AreEqual(9295, tbl["sum"]);

                _l.Pop(1); // Clean up results.
            }
            _l.CheckStackSize(1);

            _l.Pop(1); // GetGlobal("luaex_mod")

            _l.CheckStackSize(0);
        }

        /// <summary>Test generated errors.</summary>
        [Test]
        public void ScriptErrors()
        {
            // Test EvalLuaStatus().
            {
                LoadTestScript("luaex.lua");
                _l!.PCall(0, Lua.LUA_MULTRET, 0);

                // Reset stack.
                _l.SetTop(0);
                _l.CheckStackSize(0);

                // Simulate how lua processes internal errors.
                _l.PushString("Fake lua error message");
                var ex = Assert.Throws<LuaException>(() => { object _ = _l.EvalLuaStatus(LuaStatus.ErrRun); });
                Assert.That(ex.Message, Does.Contain("Fake lua error message"));
                _l!.CheckStackSize(0);
            }

            // Test LuaStatus error handling.
            {
                LoadTestScript("luaex.lua");
                _l!.PCall(0, Lua.LUA_MULTRET, 0);

                // Reset stack.
                _l.SetTop(0);
                _l.CheckStackSize(0);

                //TODOF can't do this:
                //_l.Error("Forced error");
            }

            // Force internal error from lua side. TODOF force ErrRun, ErrMem, ErrErr.
            {
                LoadTestScript("luaex.lua");
                _l!.PCall(0, Lua.LUA_MULTRET, 0);

                // Reset stack.
                _l.SetTop(0);
                _l.CheckStackSize(0);

                // Call a function that does a bad thing.
                _l.GetGlobal("force_error");

                // Push the arguments. none

                _l.CheckStackSize(1);

                // Do the call.
                _l.PCall(0, 0, 0);
                _l.CheckStackSize(0);
            }

            // Test load invalid file.
            {
                var ex = Assert.Throws<FileNotFoundException>(() => { LoadTestScript("xxxyyyyzzz.lua"); });
                Assert.That(ex.Message, Does.Contain("xxxyyyyzzz.lua: No such file or directory"));
                _l.CheckStackSize(0);
            }

            // Test load file with bad syntax
            {
                var ex = Assert.Throws<SyntaxException>(() => { LoadTestScript("luaex_syntax.lua"); });
                Assert.That(ex.Message, Does.Contain(" syntax error near "));
                //_l!.PCall(0, Lua.LUA_MULTRET, 0);

                _l.CheckStackSize(0);
            }
        }

        /// <summary>General playground for testing.</summary>
        [Test]
        public void Play()
        {
            ///// Load everything.
            LoadTestScript("luaex.lua");

            // PCall loads the file.
            _l!.PCall(0, Lua.LUA_MULTRET, 0);
            //The function results are pushed onto the stack in direct order (the first result is pushed first),
            //so that after the call the last result is on the top of the stack.
            // [1] is the luaex module, [2] is api_lib


            // TODO2 magic way to identify uninitialized variables?
            //Add a metatable to the tables where undefined access must not be allowed.
            //a = setmetatable({ ...}, { __index = function(i) error "undefined" end})
            //is all you need.
            //Now, you could totally stick a metatable on _G that caused some sort of exception when you tried to
            //get the value for a key with a nil value.


            //// Reset stack.
            _l.SetTop(0);
            _l.CheckStackSize(0);


            _l.GetGlobal("bad1"); //TODO2 ignores undefined variable like: dev_type=midi_out  http://lua-users.org/wiki/DetectingUndefinedVariables
            var ooo = _l.ToStringL(-1);
            _l.Pop(1);
            _l.CheckStackSize(0);


            //LuaType t = _l.GetGlobal("things"); // push lua value onto stack
            //Assert.AreEqual(LuaType.Table, t);
            //var tbl = _l.ToTableEx(-1);
            //var s = tbl.Dump("things");

            //_l.PushGlobalTable(); // Blows up because globals contains _G causing a stack overflow. Don't do this.
            _l.GetGlobal("_G");
            var gl = _l.ToTableEx(-1);
            _l.Pop(1);


            //// Dump globals.
            //_l.PushGlobalTable();
            //var gl = _l.DumpTable("globals", 0, false);
            //_l.Pop(1); // from PushGlobalTable()
            //Lua.Log(string.Join(Environment.NewLine, gl));

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
        void LoadTestScript(string fn)
        {
            string srcPath = GetSourcePath();
            string scriptsPath = Path.Combine(srcPath, "scripts");
            _l!.SetLuaPath(new() { scriptsPath });
            string scriptFile = Path.Combine(scriptsPath, fn);
            _l!.LoadFile(scriptFile);
        }

        // Get the dir name of the caller's source file.
        string GetSourcePath([CallerFilePath] string callerPath = "")
        {
            return Path.GetDirectoryName(callerPath)!;
        }
    }
}
