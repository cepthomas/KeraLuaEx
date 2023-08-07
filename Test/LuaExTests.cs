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

        /// <summary>Bound lua function.</summary>
        readonly LuaFunction _funcPrint = PrintEx;

        /// <summary>Bound lua function.</summary>
        readonly LuaFunction _funcTimer = Timer;

        /// <summary>Metrics.</summary>
        static readonly Stopwatch _sw = new();
        static long _startTicks = 0;

        /// <summary>Needed to bind static lua functions.</summary>
        static LuaExTests _tests;

        [SetUp]
        public void Setup()
        {
            _l?.Close();
            _l = new Lua();
            _l.Register("printex", _funcPrint);
            _l.Register("timer", _funcTimer);
            _startTicks = 0;
            _sw.Start();
            _tests = this;
        }

        [TearDown]
        public void TearDown()
        {
            _l?.Close();
            _l = null;
            _sw.Stop();
        }

        [Test]
        public void Basic()
        {
            Common.Log($"Starting test:Basic");

            string srcPath = Common.GetSourcePath();
            string scriptsPath = Path.Combine(srcPath, "scripts");
            Common.SetLuaPath(_l!, new() { scriptsPath });
            string scriptFile = Path.Combine(scriptsPath, "luaex.lua");
            _l!.LoadFile(scriptFile);
            _l.PCall(0, -1, 0);

            var x = Common.GetGlobalValue(_l, "g_table");
            var table = x.val as DataTable;
            Assert.AreEqual(DataTable.TableType.Dictionary, table.Type);
            Assert.AreEqual(3, table.Count);
            var dict = table.AsDict();
            Assert.AreEqual("bing_bong", dict["dev_type"]);

            x = Common.GetGlobalValue(_l, "g_number");
            Assert.AreEqual(7.654, x.val);

            x = Common.GetGlobalValue(_l, "g_int");
            Assert.AreEqual(80808, x.val);

            x = Common.GetGlobalValue(_l, "g_list_int");
            table = x.val as DataTable;
            var ls = table.AsList();
            Assert.AreEqual(98, ls[2]);

            x = Common.GetGlobalValue(_l, "things"); //TODO1 too much casting? impl []?
            table = x.val as DataTable;
            Assert.AreEqual(DataTable.TableType.Dictionary, table.Type);
            Assert.AreEqual(4, table.Count);

            dict = table.AsDict();
            var whiz = dict["whiz"] as DataTable;
            Assert.AreEqual(3, whiz.Count);

            dict = whiz.AsDict();
            var dblt = dict["double_table"] as DataTable;
            Assert.AreEqual(DataTable.TableType.List, dblt.Type);
            Assert.AreEqual(3, dblt.Count);

            var list = dblt.AsList();
            Assert.AreEqual(909.555, list[2]);

            ///// Execute a simple lua function.
            _l.GetGlobal("g_func");
            // Push the arguments to the call.
            var s = "az9011 birdie";
            _l.PushString(s);
            // Do the call.
            _l.PCall(1, 1, 0);
            // Get result.
            var res = _l.ToInteger(-1);
            Assert.AreEqual(s.Length + 3, res);
            _l.Pop(1); // Clean up returned value.

            ///// Execute a more complex lua function.
            //Debug.WriteLine(Common.DumpStack(_l, "1"));
            _l.GetGlobal("calc");
            //Debug.WriteLine(Common.DumpStack(_l, "2"));
            // Push the arguments to the call.
            var addends = new List<long>() { 3901, 488, 922, 1578, 2406 };
            var suffix = "__the_end__";
            table = new(addends);
            _l.PushDataTable(table);
            //Debug.WriteLine(Common.DumpStack(_l, "3"));
            _l.PushString(suffix);
            //Debug.WriteLine(Common.DumpStack(_l, "4"));
            // Do the call.
            _l.PCall(2, 1, 0); //attempt to call a number value
            // Get the results from the stack.
            table = _l.ToDataTable();
            //_l.Pop(1); // Clean up returned value. ... not for table
            Assert.AreEqual(DataTable.TableType.Dictionary, table.Type);
            Assert.AreEqual(2, table.Count);
            dict = table.AsDict();
            Assert.AreEqual(">>>9295___the_end__<<<", dict["str"]);
            Assert.AreEqual(9295, dict["sum"]);

            Common.Log($"Finished test:Basic");
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
            // args
            var s = l.ToString(-1);
            // work
            Common.Log($"printex:{s}");
            // return
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
            // args
            bool on = l.ToBoolean(-1);
            // work
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
            // return
            l.PushNumber(totalMsec);
            return 1;
        }
        #endregion
    }
}
