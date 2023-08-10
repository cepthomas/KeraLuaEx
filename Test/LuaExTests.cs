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


        [Test]
        public void Play(string script)
        {
            _l!.LoadString(script);

            // Call to load file.
            _l.PCall(0, -1, 0);

            _l.PushGlobalTable();

            var sg = Common.DumpRawTable(_l, "Globals", 0, true);

            Common.Log(Environment.NewLine + string.Join(Environment.NewLine, sg));
        }

        [Test]
        public void BasicInterop(string script = "")
        {
            if (script != "")
            {
                _l!.LoadString(script);
            }
            else
            {
                string srcPath = Common.GetSourcePath();
                string scriptsPath = Path.Combine(srcPath, "scripts");
                Common.SetLuaPath(_l!, new() { scriptsPath });
                string scriptFile = Path.Combine(scriptsPath, "luaex.lua");
                _l!.LoadFile(scriptFile);
            }

            // Call to load file.
            _l.PCall(0, -1, 0);

            var table = Common.GetGlobalValue(_l, "g_table") as DataTable;
            Assert.AreEqual(DataTable.TableType.Dictionary, table.Type);
            Assert.AreEqual(3, table.Count);
            Assert.AreEqual("bing_bong", table["dev_type"]);

            var x = Common.GetGlobalValue(_l, "g_number");
            Assert.AreEqual(7.654, x);

            x = Common.GetGlobalValue(_l, "g_int");
            Assert.AreEqual(80808, x);

            table = Common.GetGlobalValue(_l, "g_list_int") as DataTable;
            Assert.AreEqual(98, table[2]);

            var ls = table.AsList();
            Assert.AreEqual(4, ls.Count);
            Assert.AreEqual(98, ls[2]);
            //var ex = Assert.Throws<KeyNotFoundException>(() => { object _ = ls[22]; });

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

            ///// Execute a simple lua function.
            _l.GetGlobal("g_func");

            // Push the arguments.
            var s = "az9011 birdie";
            _l.PushString(s);

            // Do the call.
            _l.PCall(1, 1, 0);

            // Get result.
            var res = _l.ToInteger(-1);
            Assert.AreEqual(s.Length + 3, res);
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
    }
}
