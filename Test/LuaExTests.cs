using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

        // TODO1 deal with this.
        //if (script != "")
        //{
        //    _l!.LoadString(script);
        //}
        //else
        //{
        //    string srcPath = Common.GetSourcePath();
        //    string scriptsPath = Path.Combine(srcPath, "scripts");
        //    Common.SetLuaPath(_l!, new() { scriptsPath });
        //    string scriptFile = Path.Combine(scriptsPath, "luaex.lua");
        //    _l!.LoadFile(scriptFile);
        //}

        public string ScriptText { get; set; } = "";


        [Test]
        public void Play()
        {
            _l!.LoadString(ScriptText);

            // Call to load file. TODO1 should be a util... but requires module returns itself, otherwise thinngs need to be global to be visible i think.
            _l.PCall(0, Lua.LUA_MULTRET, 0);

            //// Top of the stack is the module itself. Save it for later.
            //_l.SetGlobal("luaex");
            //// Reset stack.
            //_l.SetTop(0);
            //return;


            var s = Common.DumpStack(_l, "PCall return stack");
            Common.Log(Environment.NewLine + s);
            // [1] is the luaex module, [2] is api_lib
            //The function results are pushed onto the stack in direct order (the first result is pushed first),
            //so that after the call the last result is on the top of the stack.

            //// Stick module in global.
            //_l.SetGlobal("luaex");

            var sg1 = Common.DumpRawTable(_l, "[1]", 0, true);
            Common.Log(Environment.NewLine + string.Join(Environment.NewLine, sg1));
            _l.Pop(1);

            //var sg2 = Common.DumpRawTable(_l, "[2]", 0, true);
            //Common.Log(Environment.NewLine + string.Join(Environment.NewLine, sg2));
            //_l.Pop(1);

            // Dump globals. TODOF
            _l.PushGlobalTable();
            var sg3 = Common.DumpRawTable(_l, "globals", 0, true);
            _l.Pop(1); // from PushGlobalTable()
            Common.Log(Environment.NewLine + string.Join(Environment.NewLine, sg3));
        }

        [Test]
        public void ScriptWithGlobal()
        {
            _l!.LoadString(ScriptText);

            // Call to load file.
            _l.PCall(0, Lua.LUA_MULTRET, 0);

            // Reset stack.
            _l.SetTop(0);


            // Look at globals.
            var x = Common.GetGlobalValue(_l, "g_number");
            Assert.AreEqual(7.654, x);

            x = Common.GetGlobalValue(_l, "g_int");
            Assert.AreEqual(80808, x);

            var table = Common.GetGlobalValue(_l, "g_list_number") as DataTable;
            Assert.AreEqual(2.303, table[3]);

            table = Common.GetGlobalValue(_l, "g_list_int") as DataTable;
            Assert.AreEqual(98, table[2]);
            var ls = table.AsList();
            Assert.AreEqual(4, ls.Count);
            Assert.AreEqual(98, ls[2]);
            //var ex = Assert.Throws<KeyNotFoundException>(() => { object _ = ls[22]; });

            table = Common.GetGlobalValue(_l, "g_table") as DataTable;
            Assert.AreEqual(DataTable.TableType.Dictionary, table.Type);
            Assert.AreEqual(3, table.Count);
            Assert.AreEqual("bing_bong", table["dev_type"]);

            table = Common.GetGlobalValue(_l, "things") as DataTable;
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


            ///// Execute a more complex lua function.
            _l.GetGlobal("calc");

            // Push the arguments.
            var addends = new List<long>() { 3901, 488, 922, 1578, 2406 };
            var suffix = "__the_end__";
            table = new DataTable(addends);
            _l.PushDataTable(table);
            _l.PushString(suffix);

            // Do the call.
            _l.PCall(2, 1, 0); //attempt to call a number value

            // Get the results from the stack.
            table = _l.ToDataTable();
            //_l.Pop(1); // Don't pop - ToDataTable() does that for you.
            Assert.AreEqual(DataTable.TableType.Dictionary, table.Type);
            Assert.AreEqual(2, table.Count);
            Assert.AreEqual(">>>9295___the_end__<<<", table["str"]);
            Assert.AreEqual(9295, table["sum"]);
        }


        [Test]
        public void ScriptWithModule()
        {
            _l!.LoadString(ScriptText);

            // Call to load file.
            _l.PCall(0, Lua.LUA_MULTRET, 0);

            // Top of the stack is the module itself. Save it for later.
            _l.SetGlobal("luaex_mod");

            // Reset stack.
            _l.SetTop(0);

            //// Dump globals. TODOF - make a utility for this.
            //_l.PushGlobalTable();
            //var sg3 = Common.DumpRawTable(_l, "globals", 0, true);
            //_l.Pop(1); // from PushGlobalTable()
            //Common.Log(Environment.NewLine + string.Join(Environment.NewLine, sg3));



            ///// Look at globals.

            var x = Common.GetGlobalValue(_l, "g_int");
            Assert.AreEqual(71717, x);



            ///// Look at module stuff.
            //var mod = Common.GetGlobalValue(_l, "luaex_mod") as DataTable;
            LuaType t = _l.GetGlobal("luaex_mod");
            //var sg1 = Common.DumpRawTable(_l, ">>>", 0, true);
            //Common.Log(Environment.NewLine + string.Join(Environment.NewLine, sg1));

            _l.GetField(-1, "m_string");
            var sval = _l.ToStringL(-1);
            Assert.AreEqual("Here I am", sval);
            _l.Pop(1);

            _l.GetField(-1, "m_bool");
            var bval = _l.ToBoolean(-1);
            Assert.AreEqual(false, bval);
            _l.Pop(1);



            ///// Execute a lua function.
            _l.GetField(-1, "funcmod");

            // Push the arguments.
            var s = "az9011 birdie";
            _l.PushString(s);

            // Do the call.
            _l.PCall(1, 1, 0);

            // Get result.
            var resi = _l.ToInteger(-1);
            Assert.AreEqual(s.Length + 3, resi);
            _l.Pop(1); // Clean up returned value.


            ///// Execute a more complex lua function.
            _l.GetField(-1, "calcmod");

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
            //_l.Pop(1); // Don't pop - ToDataTable() does that for you.
            Assert.AreEqual(DataTable.TableType.Dictionary, table.Type);
            Assert.AreEqual(2, table.Count);
            Assert.AreEqual(">>>9295___the_end__<<<", table["str"]);
            Assert.AreEqual(9295, table["sum"]);

            _l.Pop(1); // GetGlobal("luaex_mod")
        }
    }
}
