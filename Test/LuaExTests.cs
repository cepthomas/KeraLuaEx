using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml.Linq;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using NUnit.Framework;
using KeraLuaEx;


namespace KeraLuaEx.Test
{
    [TestFixture]
    public class LuaExTests
    {
        Lua? _lMain;
        static readonly LuaFunction _funcPrint = Print;
        readonly LuaFunction _funcTimer = Timer;

        [SetUp]
        public void Setup()
        {
            _lMain?.Close();
            _lMain = new Lua();
            _lMain.Register("printex", _funcPrint);
            _lMain.Register("timer", _funcTimer);
        }

        [TearDown]
        public void TearDown()
        {
            _lMain?.Close();
            _lMain = null;
        }

        [Test]
        public void Basic()
        {
            string srcPath = Common.GetSourcePath();
            string scriptsPath = Path.Combine(srcPath, "scripts");
            Common.SetLuaPath(_lMain!, new() { scriptsPath });
            string scriptFile = Path.Combine(scriptsPath, "luaex.lua");
            LuaStatus lstat = _lMain!.LoadFile(scriptFile);
            Assert.AreEqual(LuaStatus.OK, lstat);
            lstat = _lMain.PCall(0, -1, 0);
            Assert.AreEqual(LuaStatus.OK, lstat);

            //TODO2 populate from Host.
        }

        string FormatDump(string name, List<string> lsin, bool indent)
        {
            string sindent = indent ? "    " : "";
            var lines = new List<string> { $"{name}:" };
            lsin.ForEach(s => lines.Add($"{sindent}{s}"));
            var s = string.Join(Environment.NewLine, lines);
            return s;
        }

        static int Print(IntPtr p)
        {
            var l = Lua.FromIntPtr(p)!;
            Debug.WriteLine($"print:{l.ToString(-1)}");
            return 0;
        }

        static int Timer(IntPtr p)
        {
            var l = Lua.FromIntPtr(p)!;
            // args
            bool on = l.ToBoolean(-1);
            // work
            var msec = 1.234;
            // return
            l.PushNumber(msec);
            return 1;
        }
    }
}
