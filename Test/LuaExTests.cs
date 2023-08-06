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
            string srcPath = Common.GetSourcePath();
            string scriptsPath = Path.Combine(srcPath, "scripts");
            Common.SetLuaPath(_l!, new() { scriptsPath });
            string scriptFile = Path.Combine(scriptsPath, "luaex.lua");
            LuaStatus lstat = _l!.LoadFile(scriptFile);
            Assert.AreEqual(LuaStatus.OK, lstat);
            lstat = _l.PCall(0, -1, 0);
            Assert.AreEqual(LuaStatus.OK, lstat);

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

            x = Common.GetGlobalValue(_l, "things"); //TODO1 too much casting....
            table = x.val as DataTable;
            Assert.AreEqual(DataTable.TableType.Dictionary, table.Type);
            Assert.AreEqual(4, table.Count);
            dict = table.AsDict();
            var whiz = dict["whiz"] as DataTable;
            Assert.AreEqual(3, whiz.Count);
            var dblt = whiz.AsDict()["double_table"] as DataTable;
            Assert.AreEqual(DataTable.TableType.List, dblt.Type);
            Assert.AreEqual(3, dblt.Count);
            Assert.AreEqual(909.555, dblt.AsList()[2]);

            ///// Execute a lua function.
            _l.GetGlobal("g_func");
            // Push the arguments to the call.
            _l.PushString("az9011 birdie");
            // Do the actual call.
            _l.PCall(1, 1, 0);
            // Get result.
            var res = _l.ToInteger(-1);
            Assert.AreEqual(13, res);
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

        #region C# calls Lua function TODO1
        /// <summary>
        /// 
        /// </summary>
        /// <param name="addends"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public string Calculate(List<long> addends, string suffix)
        {
            // Get the function to be called.
            LuaType gtype = _l.GetGlobal("calc");
            // Push the arguments to the call.
            DataTable table = new(addends);
            _l.PushDataTable(table);
            _l.PushString(suffix);
            // Do the actual call.
            LuaStatus lstat = _l.PCall(2, 1, 0);
            // Get the results from the stack.
            var res = _l.ToString(-1);
            _l.Pop(1); // Clean up returned value.
            return res!;
        }
        #endregion
    }
}
