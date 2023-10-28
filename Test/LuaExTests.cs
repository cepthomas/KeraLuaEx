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
            LoadTestScript("luaex.lua");

            // Run it.
            _l!.PCall(0, Lua.LUA_MULTRET, 0);

            // Reset stack.
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
                var list = tbl!.AsList<double>();
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
                var list = tbl!.AsList<int>();
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
                Assert.AreEqual(5, tbl!.Count);
                //Debug.WriteLine(tbl!.Dump("things"));

                var whiz = tbl["whiz"] as TableEx;
                Assert.IsInstanceOf<TableEx>(whiz);
                Assert.AreEqual(3, whiz!.Count);
                Assert.AreEqual(99, whiz["channel"]);

                var dtbl = whiz["double_table"] as TableEx;
                Assert.IsInstanceOf<TableEx>(dtbl);
                var list = dtbl!.AsList<double>();
                Assert.AreEqual(3, list.Count);
                Assert.AreEqual(909.555, list[2]);
                _l.Pop(1); // Clean up from GetGlobal().
            }
            _l.CheckStackSize(0);

            ///// Execute a raw lua function.
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

            ///// Execute a more complex raw lua function.
            {
                LuaType t = _l.GetGlobal("calc");
                Assert.AreEqual(LuaType.Function, t);
                _l.CheckStackSize(1);

                // Push the arguments.
                var vals = new List<int>() { 3901, 488, 922, 1578, 2406 };
                var addends = new TableEx(vals);
                _l.PushTableEx(addends);
                var suffix = "__the_end__";
                _l.PushString(suffix);
                _l.CheckStackSize(3);

                // Do the call.
                _l.PCall(2, 1, 0);
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
            LoadTestScript("luaex_mod.lua");

            // Run it.
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
                var s = _l.ToString(-1);// !, // assign, no pop
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
                var list = tbl!.AsList<int>();
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

            ///// Execute a module raw lua function.
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

            ///// Execute a more complex raw lua function.
            {
                _l.GetField(-1, "calcmod");
                _l.CheckStackSize(2);

                // Push the arguments.
                var vals = new List<int>() { 3901, 488, 922, 1578, 2406 };
                var addends = new TableEx(vals);
                _l.PushTableEx(addends);
                var suffix = "__the_end__";
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

        /// <summary>Test script api.</summary>
        [Test]
        public void ScriptApi()
        {
            // Create api.
            var api = new LuaInterop(_l);

            LoadTestScript("luaex_api.lua");

            // Run it.
            _l!.PCall(0, Lua.LUA_MULTRET, 0);

            // Reset stack.
            _l.SetTop(0);

            var tbl = api!.DoOperation("a string", 9876);
            Assert.IsInstanceOf<TableEx>(tbl);
            //Debug.WriteLine(tbl!.Dump("api_ret"));
            Assert.AreEqual(2, tbl!.Count);
            Assert.AreEqual("gnirts a", tbl["sret"]);
            Assert.AreEqual(9876 / 2, tbl["iret"]);

            _l.CheckStackSize(0);
        }

        /// <summary>Test generated errors.</summary>
        [Test]
        public void ScriptErrors()
        {
            // Test EvalLuaStatus().
            {
                LoadTestScript("luaex.lua");

                // Run it.
                _l!.PCall(0, Lua.LUA_MULTRET, 0);

                // Reset stack.
                _l.SetTop(0);
                _l.CheckStackSize(0);

                // Simulate how lua processes internal errors.
                _l.PushString("Fake lua error message");
                var ex = Assert.Throws<LuaException>(() => { object _ = _l.EvalLuaStatus(LuaStatus.ErrRun); });
                Assert.That(ex!.Message, Does.Contain("Fake lua error message"));
                _l!.CheckStackSize(0);
            }

            // Test LuaStatus error handling.
            {
                LoadTestScript("luaex.lua");

                // Run it.
                _l!.PCall(0, Lua.LUA_MULTRET, 0);

                // Reset stack.
                _l.SetTop(0);
                _l.CheckStackSize(0);

                // Don't do this:
                //_l.Error("Forced error");
            }

            // Force internal error from lua side. FUTURE force ErrRun, ErrMem, ErrErr.
            {
                LoadTestScript("luaex.lua");

                // Run it.
                _l!.PCall(0, Lua.LUA_MULTRET, 0);

                // Reset stack.
                _l.SetTop(0);
                _l.CheckStackSize(0);

                // Call a function that does a bad thing. Capture stack trace.
                _l.GetGlobal("force_error");
                _l.CheckStackSize(1);

                // Push the arguments. none

                // Do the call.
                var ex = Assert.Throws<LuaException>(() => { _l.DoCall(0, 0); });
                Assert.That(ex!.Message, Does.Contain("attempt to concatenate a table value"));
                Assert.That(ex.Message, Does.Contain("in function 'inner_error'"));
                Assert.That(ex.Message, Does.Contain("in function 'force_error'"));
                _l.Pop(1); // SetGlobal()

                _l.CheckStackSize(0);
            }

            // Test load invalid file.
            {
                var ex = Assert.Throws<FileNotFoundException>(() => { LoadTestScript("xxxyyyyzzz.lua"); });
                Assert.That(ex!.Message, Does.Contain("xxxyyyyzzz.lua: No such file or directory"));
                _l.CheckStackSize(0);
            }

            // Test load file with bad syntax
            {
                var ex = Assert.Throws<SyntaxException>(() => { LoadTestScript("luaex_syntax.lua"); });
                Assert.That(ex!.Message, Does.Contain(" syntax error near "));
                //_l!.PCall(0, Lua.LUA_MULTRET, 0);

                _l.CheckStackSize(0);
            }
        }

        /// <summary>General playground for testing.</summary>
        [Test]
        public void Play()
        {
            LoadTestScript("luaex.lua");

            // Run it.
            _l!.PCall(0, Lua.LUA_MULTRET, 0);

            //// Reset stack.
            _l.SetTop(0);
            _l.CheckStackSize(0);
        }

        /// <summary>
        /// Helper.
        /// </summary>
        /// <param name="fn"></param>
        void LoadTestScript(string fn)
        {
            string srcPath = GetSourcePath();
            string scriptsPath = Path.Combine(srcPath, "scripts");
            _l!.SetLuaPath(new() { scriptsPath });
            string scriptFile = Path.Combine(scriptsPath, fn);
            var lstat = _l!.LoadFile(scriptFile);
        }

        /// <summary>
        /// Get the dir name of the caller's source file.
        /// </summary>
        /// <param name="callerPath"></param>
        /// <returns></returns>
        string GetSourcePath([CallerFilePath] string callerPath = "")
        {
            return Path.GetDirectoryName(callerPath)!;
        }
    }
}
