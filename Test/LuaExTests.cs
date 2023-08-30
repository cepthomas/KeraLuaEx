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
            {
                var o = GetGlobalValue(_l, "g_number");
                Assert.IsInstanceOf<double>(o);
                Assert.AreEqual(7.654, o);
            }

            EvalStackSize(_l, 99);

            {
                var o = GetGlobalValue(_l, "g_int");
                Assert.IsInstanceOf<int>(o);
                Assert.AreEqual(80808, o);
            }

            EvalStackSize(_l, 99);

            {
                _l.GetGlobal("g_list_number"); // push lua value onto stack
                var tbl = _l.ToTableEx(99, true);
                var list = tbl.ToListDouble();
                //var list = _l.ToDictionary(99, true);
                //var o = _l.ToListDouble("g_list_number");
                //Assert.IsInstanceOf<List<double>>(o);
                //var t = o.GetType();
                //var list = o as List<double>;
                Assert.AreEqual(4, list.Count);
                Assert.AreEqual(2.303, list[3]);
            }

            EvalStackSize(_l, 99);

            {
                _l.GetGlobal("g_list_int"); // push lua value onto stack
                var tbl = _l.ToTableEx(99, true);
                var list = tbl.ToListInt();
                //var o = _l.ToList<int>("g_list_int");
                //Assert.IsInstanceOf<List<int>>(o);
                //var list = o as List<int>;
                Assert.AreEqual(4, list.Count);
                Assert.AreEqual(98, list[2]);
                //var ex = Assert.Throws<KeyNotFoundException>(() => { object _ = ls[22]; });
            }

            EvalStackSize(_l, 99);

            {
                _l.GetGlobal("g_table"); // push lua value onto stack
                var dict = _l.ToTableEx(99, false);



                //var o = _l.ToDictionary("g_table");
                //Assert.IsInstanceOf<Dictionary<string, object>>(o);
                //var dict = o as Dictionary<string, object>;
                Assert.AreEqual(3, dict.Count);
                Assert.AreEqual("bing_bong", dict["dev_type"]);
            }

            EvalStackSize(_l, 99);

            {
                _l.GetGlobal("things"); // push lua value onto stack
                var tbl = _l.ToTableEx(99, false);

                var s = tbl.ToString();

                

                //List<string> ls = DumpRawTable(_l, "tableName", 0, true);



                //var o = _l.ToDictionary("things");
                //Assert.IsInstanceOf<Dictionary<string, object>>(o);
                //var dict = o as Dictionary<string, object>;
                Assert.AreEqual(4, tbl.Count);

                var whiz = tbl["whiz"];
                //Assert.IsInstanceOf<Dictionary<string, object>>(o);
                Assert.IsInstanceOf<TableEx>(whiz);


                //var whiz = o as Dictionary<string, object>;
                //Assert.AreEqual(3, whiz.Count);
                //Assert.AreEqual(99, whiz["channel"]);
                //var ex = Assert.Throws<KeyNotFoundException>(() => { object _ = dict["booga"]; });

                //           var o = whiz["double_table"] as TableEx;

                //Assert.IsInstanceOf<List<double>>(o);
                //var list = o as List<double>;
                //var list = o.
                //Assert.AreEqual(3, list.Count);
                //Assert.AreEqual(909.555, list[2]);

            }

            EvalStackSize(_l, 0);


            ///// Execute a lua function.
            {
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
            }

            EvalStackSize(_l, 0);

            ///// Execute a more complex lua function.
            {
                _l.GetGlobal("calc");
                EvalStackSize(_l, 1);

                // Push the arguments.
                var addends = new List<int>() { 3901, 488, 922, 1578, 2406 };
                _l.PushList(addends);
                var suffix = "__the_end__";
                _l.PushString(suffix);
                EvalStackSize(_l, 3);

                // Do the call.
                _l.PCall(2, 1, 0); //attempt to call a number value
                EvalStackSize(_l, 1);

                // Get the results from the stack.
                var dict = _l.ToTableEx(2, false);
                Assert.IsInstanceOf<Dictionary<string, object>>(dict);
                _l.Pop(1);
                Assert.AreEqual(2, dict.Count);
                Assert.AreEqual(">>>9295___the_end__<<<", dict["str"]);
                Assert.AreEqual(9295, dict["sum"]);
            }

            EvalStackSize(_l, 0);
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

            EvalStackSize(_l, 0);

            ///// Look at globals.
            {
                var o = GetGlobalValue(_l, "g_int");
                Assert.IsInstanceOf<int>(o);
                Assert.AreEqual(71717, o);
            }

            ///// Look inside module.
            LuaType t = _l.GetGlobal("luaex_mod"); // push lua value onto stack
            {
                var o = GetTableValue(_l, "m_string");
                Assert.IsInstanceOf<string>(o);
                Assert.AreEqual("Here I am", o);
                // _l.GetField(-1, "m_string"); // push lua value onto stack
                // var sval = _l.ToStringL(-1); // assign, no pop
                // Assert.AreEqual("Here I am", sval);
                // _l.Pop(1); // balance stack
            }

            {
                var o = GetTableValue(_l, "m_bool");
                Assert.IsInstanceOf<bool>(o);
                Assert.AreEqual(false, o);
                // _l.GetField(-1, "m_bool");
                // var bval = _l.ToBoolean(-1);
                // Assert.AreEqual(false, bval);
                // _l.Pop(1);
            }

            EvalStackSize(_l, 1); // luaex_mod is on top of stack

            {
                GetTableValue(_l, "m_list_int"); // push lua value onto stack
                var tbl = _l.ToTableEx(99, true);
                var list = tbl.ToListInt();


                //var o = _l.ToList<int>("g_list_int");
                //Assert.IsInstanceOf<List<int>>(o);
                //var list = o as List<int>;
                Assert.AreEqual(4, list.Count);
                Assert.AreEqual(98, list[2]);
                //var ex = Assert.Throws<KeyNotFoundException>(() => { object _ = ls[22]; });
            }

            EvalStackSize(_l, 1); // luaex_mod is on top of stack

            {
                GetTableValue(_l, "m_table"); // push lua value onto stack
                var dict = _l.ToTableEx(99, false);
                //var o = _l.ToDictionary("g_table");
                //Assert.IsInstanceOf<Dictionary<string, object>>(o);
                //var dict = o as Dictionary<string, object>;
                Assert.AreEqual(3, dict.Count);
                Assert.AreEqual("bing_bong", dict["dev_type"]);
            }

            EvalStackSize(_l, 1); // luaex_mod is on top of stack

            ///// Execute a module lua function.
            {
                _l.GetField(-1, "funcmod"); // push lua value onto stack

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
            }
            
            EvalStackSize(_l, 1);

            ///// Execute a more complex lua function.
            {
                _l.GetField(-1, "calcmod");
                EvalStackSize(_l, 2);

                // Push the arguments.
                var addends = new List<int>() { 3901, 488, 922, 1578, 2406 };
                var suffix = "__the_end__";
                // var table = new DataTable(addends);
                // _l.PushDataTable(table);
                _l.PushList(addends);
                _l.PushString(suffix);

                // Do the call.
                _l.PCall(2, 1, 0);

                // Get the results from the stack.
                var dict = _l.ToTableEx(4, false);
                Assert.IsInstanceOf<Dictionary<string, object>>(dict);
                Assert.AreEqual(2, dict.Count);
                Assert.AreEqual(">>>9295___the_end__<<<", dict["str"]);
                Assert.AreEqual(9295, dict["sum"]);

                // table = _l.ToDataTable();
                // _l.Pop(1);
                // Assert.AreEqual(DataTable.TableType.Dictionary, table.Type);
                // Assert.AreEqual(2, table.Count);
                // Assert.AreEqual(">>>9295___the_end__<<<", table["str"]);
                // Assert.AreEqual(9295, table["sum"]);

                EvalStackSize(_l, 1);
            }

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
