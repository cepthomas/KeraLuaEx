using System;
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

        /// <summary>Explicit script.</summary>
        public string ScriptText { get; set; } = "";

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
        public void Play()
        {
            ///// Load everything.
            LoadScript("luaex_mod.lua");

            // PCall loads the file.
            _l.PCall(0, Lua.LUA_MULTRET, 0);

            var sg = _l.DumpStack("PCall return stack:");
            Log(Environment.NewLine + string.Join(Environment.NewLine, sg));
            // [1] is the luaex module, [2] is api_lib
            //The function results are pushed onto the stack in direct order (the first result is pushed first),
            //so that after the call the last result is on the top of the stack.
            // Stick module in global.
            //_l.SetGlobal("luaex");
            EvalStackSize(_l, 0);

            if (_l.GetTop() > 0)
            {
                var sg1 = DumpRawTable(_l, "[1]", 0, false);
                Log(Environment.NewLine + string.Join(Environment.NewLine, sg1));
                _l.Pop(1);
            }
            EvalStackSize(_l, 0);

            if (_l.GetTop() > 0)
            {
                var sg2 = DumpRawTable(_l, "[2]", 0, false);
                Log(Environment.NewLine + string.Join(Environment.NewLine, sg2));
                _l.Pop(1);
            }
            EvalStackSize(_l, 0);

            // Dump globals.
            var gl = DumpGlobals(_l);
            Log(Environment.NewLine + string.Join(Environment.NewLine, gl));

            EvalStackSize(_l, 0);
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
            EvalStackSize(_l, 0);

            ///// Look at globals.
            var x = GetGlobalValue(_l, "g_number");
            Assert.AreEqual(7.654, x);

            x = GetGlobalValue(_l, "g_int");
            Assert.AreEqual(80808, x);

            var table = GetGlobalValue(_l, "g_list_number") as DataTable;
            Assert.AreEqual(2.303, table[3]);

            table = GetGlobalValue(_l, "g_list_int") as DataTable;
            Assert.AreEqual(98, table[2]);
            var ls = table.AsList();
            Assert.AreEqual(4, ls.Count);
            Assert.AreEqual(98, ls[2]);
            //var ex = Assert.Throws<KeyNotFoundException>(() => { object _ = ls[22]; });

            table = GetGlobalValue(_l, "g_table") as DataTable;
            Assert.AreEqual(DataTable.TableType.Dictionary, table.Type);
            Assert.AreEqual(3, table.Count);
            Assert.AreEqual("bing_bong", table["dev_type"]);

            table = GetGlobalValue(_l, "things") as DataTable;
            Assert.AreEqual(DataTable.TableType.Dictionary, table.Type);
            Assert.AreEqual(4, table.Count);

            var whiz = table["whiz"] as DataTable;
            Assert.AreEqual(3, whiz.Count);

            var dict = whiz.AsDict();
            Assert.AreEqual(3, dict.Count);
            Assert.AreEqual(99, dict["channel"]);
            //var ex = Assert.Throws<KeyNotFoundException>(() => { object _ = dict["booga"]; });

            var dblt = whiz["double_table"] as DataTable;
            Assert.AreEqual(DataTable.TableType.List, dblt.Type);
            Assert.AreEqual(3, dblt.Count);

            var list = dblt.AsList();
            Assert.AreEqual(909.555, list[2]);

            EvalStackSize(_l, 0);


            ///// Execute a lua function.
            _l.GetGlobal("g_func");

            // Push the arguments.
            var s = "az9011 birdie";
            _l.PushString(s);

            // Do the call.
            _l.PCall(1, 1, 0);

            // Get result.
            var resi = _l.ToInteger(-1);
            Assert.AreEqual(s.Length + 3, resi);
            _l.Pop(1); // Clean up returned value.

            EvalStackSize(_l, 0);

            ///// Execute a more complex lua function.
            _l.GetGlobal("calc");
            EvalStackSize(_l, 1);

            // Push the arguments.
            var addends = new List<long>() { 3901, 488, 922, 1578, 2406 };
            var suffix = "__the_end__";
            table = new DataTable(addends);
            _l.PushDataTable(table);
            _l.PushString(suffix);
            EvalStackSize(_l, 3);

            // Do the call.
            _l.PCall(2, 1, 0); //attempt to call a number value
            EvalStackSize(_l, 1);

            // Get the results from the stack.
            table = _l.ToDataTable();
            _l.Pop(1);
            Assert.AreEqual(DataTable.TableType.Dictionary, table.Type);
            Assert.AreEqual(2, table.Count);
            Assert.AreEqual(">>>9295___the_end__<<<", table["str"]);
            Assert.AreEqual(9295, table["sum"]);

            EvalStackSize(_l, 0);
        }

        [Test]
        public void ScriptWithModule()
        {
            ///// Load everything.
            LoadScript("luaex_mod.lua");

            // PCall loads the file.
            _l.PCall(0, Lua.LUA_MULTRET, 0);

            // Top of the stack is the module itself. Save it for later.
            _l.SetGlobal("luaex_mod");

            // Reset stack.
            _l.SetTop(0);

            EvalStackSize(_l, 0);

            ///// Look at globals.
            // var gl = Common.DumpGlobals(_l);
            // Common.Log(Environment.NewLine + string.Join(Environment.NewLine, gl));
            var x = GetGlobalValue(_l, "g_int");
            Assert.AreEqual(71717, x);

            ///// Look at module.
            LuaType t = _l.GetGlobal("luaex_mod");

            _l.GetField(-1, "m_string");
            var sval = _l.ToStringL(-1);
            Assert.AreEqual("Here I am", sval);
            _l.Pop(1);

            _l.GetField(-1, "m_bool");
            var bval = _l.ToBoolean(-1);
            Assert.AreEqual(false, bval);
            _l.Pop(1);

            EvalStackSize(_l, 1); // GetGlobal("luaex_mod") 


            ///// Execute a module lua function.
            _l.GetField(-1, "funcmod");

            // Push the arguments.
            var s = "az9011 birdie";
            _l.PushString(s);

            EvalStackSize(_l, 3);

            // Do the call.
            _l.PCall(1, 1, 0);
            EvalStackSize(_l, 2);

            // Get result.
            var resi = _l.ToInteger(-1);
            Assert.AreEqual(s.Length + 3, resi);
            _l.Pop(1); // Clean up returned value.
            
            EvalStackSize(_l, 1);


            ///// Execute a more complex lua function.
            _l.GetField(-1, "calcmod");
            EvalStackSize(_l, 2);

            // Push the arguments.
            var addends = new List<long>() { 3901, 488, 922, 1578, 2406 };
            var suffix = "__the_end__";
            var table = new DataTable(addends);
            _l.PushDataTable(table);
            _l.PushString(suffix);

            // Do the call.
            _l.PCall(2, 1, 0); //attempt to call a number value

            // Get the results from the stack.
            table = _l.ToDataTable();
            _l.Pop(1);
            Assert.AreEqual(DataTable.TableType.Dictionary, table.Type);
            Assert.AreEqual(2, table.Count);
            Assert.AreEqual(">>>9295___the_end__<<<", table["str"]);
            Assert.AreEqual(9295, table["sum"]);
            EvalStackSize(_l, 1);

            _l.Pop(1); // GetGlobal("luaex_mod")

            EvalStackSize(_l, 0);
        }

        // Helper.
        void LoadScript(string fn)
        {
            if (ScriptText != "")
            {
                _l!.LoadString(ScriptText);
            }
            else
            {
                string srcPath = GetSourcePath();
                string scriptsPath = Path.Combine(srcPath, "scripts");
                _l.SetLuaPath(new() { scriptsPath });
                string scriptFile = Path.Combine(scriptsPath, fn);
                _l!.LoadFile(scriptFile);
            }
        }
    }
}
