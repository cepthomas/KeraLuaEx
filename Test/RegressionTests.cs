using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml.Linq;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using NUnit.Framework;


namespace KeraLuaEx.Test
{
    // These are copied from the KeraLua tests as regression.
    [TestFixture]
    public class RegressionTests
    {
        static LuaHookFunction _funcHookCallback = HookCallback;

        static readonly StringBuilder _hookLog = new();

        // Get the dir name of the caller's source file.
        string GetSourcePath([CallerFilePath] string callerPath = "")
        {
            return Path.GetDirectoryName(callerPath)!;
        }

        [SetUp]
        public void SetUp()
        {
        }

        #region Core
        [Test]
        public void CF()
        {
            using Lua l = new();

            string srcPath = GetSourcePath();
            string scriptsPath = Path.Combine(srcPath, "scripts");
            l.SetLuaPath(new() { scriptsPath });
            LuaStatus lstat = l.LoadFile(Path.Combine(scriptsPath, "cf.lua"));
            Assert.AreEqual(LuaStatus.OK, lstat);
            lstat = l.PCall(0, -1, 0);
            Assert.AreEqual(LuaStatus.OK, lstat);
        }
        #endregion

        #region PushCFunction
        static readonly char UNICODE_CHAR = '\uE007';
        static string UnicodeString => Convert.ToString(UNICODE_CHAR, System.Globalization.CultureInfo.InvariantCulture);
        static readonly LuaFunction _funcTestUnicodeString = TestUnicodeString;
        static readonly LuaFunction _funcTestReferenceData = TestReferenceData;
        static readonly LuaFunction _funcTestValueData = TestValueData;
        static XDocument _tempDocument = new();

        [Test]
        public void TestUnicodeString()
        {
            using var l = new Lua { Encoding = Encoding.UTF8 };
            l.PushCFunction(_funcTestUnicodeString);
            l.SetGlobal("TestUnicodeString");
            l.PushString(UnicodeString);
            l.SetGlobal("unicodeString");
            AssertString("TestUnicodeString(unicodeString)", l);
        }
        private static int TestUnicodeString(IntPtr p)
        {
            var l = Lua.FromIntPtr(p);
            string param = l!.ToString(1, false)!;

            Assert.AreEqual(UnicodeString, param, "#1 ToString()");

            return 0;
        }

        [Test]
        public void TestPushReferenceData()
        {
            var document = XDocument.Parse(@"<users><user name=""John Doe"" age=""42"" /><user name=""Jane Doe"" age=""39"" /></users>");

            using Lua l = new();

            //l.PushObject<XDocument>(null);
            l.PushNil();
            l.SetGlobal("foo");

            l.PushObject(document);
            l.SetGlobal("bar");

            l.PushCFunction(_funcTestReferenceData);
            l.SetGlobal("TestReferenceData");

            _tempDocument = document;
            AssertString("TestReferenceData(foo, bar)", l);
        }
        private static int TestReferenceData(IntPtr p)
        {
            var l = Lua.FromIntPtr(p);

            XDocument param1 = l!.ToObject<XDocument>(1)!;
            XDocument param2 = l!.ToObject<XDocument>(2)!;

            Assert.IsNull(param1, "#1");
            Assert.AreEqual(param2, _tempDocument, "#2");

            return 0;
        }

        [Test]
        public void TestPushValueData()
        {
            using var l = new Lua();

            l.PushObject<Rectangle?>(null);
            l.SetGlobal("foo");

            l.PushObject(new Rectangle(10, 10, 100, 100));
            l.SetGlobal("bar");

            l.PushObject(new DateTime(2018, 10, 10, 0, 0, 0));
            l.SetGlobal("date");

            l.PushCFunction(_funcTestValueData);
            l.SetGlobal("TestValueData");

            AssertString("TestValueData(foo, bar, date)", l);
        }
        #endregion

        #region Debug hooks
        [Test]
        public void TestLuaHook()
        {
            using var l = new Lua();

            _hookLog.Clear();

            string srcPath = GetSourcePath();
            string scriptsPath = Path.Combine(srcPath, "scripts");
            l.SetLuaPath(new() { scriptsPath });

            l.SetHook(_funcHookCallback, LuaHookMask.Line, 0);

            LuaStatus lstat = l.LoadFile(Path.Combine(scriptsPath, "main.lua"));
            l.EvalLuaStatus(lstat);
            lstat = l.PCall(0, -1, 0);
            l.EvalLuaStatus(lstat);

            string output = _hookLog.ToString();

            string expected =
@"main.lua-main.lua:2 (main)
foo.lua-foo.lua:2 (main)
module1.lua-module1.lua:3 (main)
module1.lua-module1.lua:9 (main)
module1.lua-module1.lua:5 (main)
module1.lua-module1.lua:11 (main)
foo.lua-foo.lua:8 (main)
foo.lua-foo.lua:4 (main)
foo.lua-foo.lua:14 (main)
foo.lua-foo.lua:10 (main)
foo.lua-foo.lua:14 (main)
main.lua-main.lua:4 (main)
main.lua-main.lua:5 (main)
main.lua-main.lua:7 (main)
main.lua-main.lua:8 (main)
foo.lua-foo.lua:5 (Lua)
foo.lua-foo.lua:6 (Lua)
foo.lua-foo.lua:7 (Lua)
module1.lua-module1.lua:6 (Lua)
module1.lua-module1.lua:7 (Lua)
module1.lua-module1.lua:8 (Lua)
foo.lua-foo.lua:8 (Lua)
main.lua-main.lua:11 (main)
";
            expected = expected.Replace("\r", "");
            expected = expected.Replace('/', Path.DirectorySeparatorChar);
            output = output.Replace("\r", "");
            Assert.AreEqual(expected, output, "#1");

            Assert.IsNotNull(l.Hook());

            // disable
            l.SetHook(_funcHookCallback, LuaHookMask.Disabled, 0);
            //Assert.IsNull(l.Hook, "#3");
        }

        static void HookCallback(IntPtr p, IntPtr ar)
        {
            var l = Lua.FromIntPtr(p);
            var debug = LuaDebug.FromIntPtr(ar);

            Assert.NotNull(l, "#l shouldn't be null");
            Assert.NotNull(debug, "#debug shouldn't be null");

            if (debug.Event != LuaHookEvent.Line)
            {
                return;
            }

            l!.GetStack(0, ar);

            if (!l.GetInfo("Snlu", ar))
            {
                return;
            }

            debug = LuaDebug.FromIntPtr(ar);

            string source = debug.Source[1..];
            string shortSource = Path.GetFileName(debug.ShortSource);

            source = Path.GetFileName(source);
            _hookLog.AppendLine($"{shortSource}-{source}:{debug.CurrentLine} ({debug.What})");
        }

        [Test]
        public void TestLuaHookStruct()
        {
            _funcHookCallback = HookCalbackStruct;

            using var l = new Lua();

            _hookLog.Clear();

            string srcPath = GetSourcePath();
            string scriptsPath = Path.Combine(srcPath, "scripts");
            l.SetLuaPath(new() { scriptsPath });

            l.SetHook(_funcHookCallback, LuaHookMask.Line, 0);

            LuaStatus lstat = l.LoadFile(Path.Combine(scriptsPath, "main.lua"));
            l.EvalLuaStatus(lstat);
            lstat = l.PCall(0, -1, 0);
            l.EvalLuaStatus(lstat);

            string output = _hookLog.ToString();

            string expected =
@"main.lua-main.lua:2 (main)
foo.lua-foo.lua:2 (main)
module1.lua-module1.lua:3 (main)
module1.lua-module1.lua:9 (main)
module1.lua-module1.lua:5 (main)
module1.lua-module1.lua:11 (main)
foo.lua-foo.lua:8 (main)
foo.lua-foo.lua:4 (main)
foo.lua-foo.lua:14 (main)
foo.lua-foo.lua:10 (main)
foo.lua-foo.lua:14 (main)
main.lua-main.lua:4 (main)
main.lua-main.lua:5 (main)
main.lua-main.lua:7 (main)
main.lua-main.lua:8 (main)
foo.lua-foo.lua:5 (Lua)
foo.lua-foo.lua:6 (Lua)
foo.lua-foo.lua:7 (Lua)
module1.lua-module1.lua:6 (Lua)
module1.lua-module1.lua:7 (Lua)
module1.lua-module1.lua:8 (Lua)
foo.lua-foo.lua:8 (Lua)
main.lua-main.lua:11 (main)
";
            expected = expected.Replace('/', Path.DirectorySeparatorChar);
            expected = expected.Replace("\r", "");
            output = output.Replace("\r", "");

            Assert.AreEqual(expected, output, "#2");

            // disable
            l.SetHook(_funcHookCallback, LuaHookMask.Disabled, 0);

            Assert.IsNull(l.Hook(), "#3");
        }

        static void HookCalbackStruct(IntPtr p, IntPtr ar)
        {
            var l = Lua.FromIntPtr(p);
            var debug = new LuaDebug();

            l!.GetStack(0, ref debug);

            if (!l.GetInfo("Snlu", ref debug))
            {
                return;
            }

            string shortSource = Path.GetFileName(debug.ShortSource);
            string source = debug.Source[1..];
            source = Path.GetFileName(source);
            _hookLog.AppendLine($"{shortSource}-{source}:{debug.CurrentLine} ({debug.What})");
        }
        #endregion


        [Test]
        public void TypeNameReturn()
        {
            using var l = new Lua();

            l.PushInteger(28);
            string name = l.TypeName(-1)!;

            Assert.AreEqual("number", name, "#1");
        }

        [Test]
        public void SettingUpValueDoesntCrash()
        {
            using var l = new Lua();

            l.LoadString("hello = 1");
            l.NewTable();
            string result = l!.SetUpValue(-2, 1)!;

            Assert.AreEqual("_ENV", result, "#1");
        }

        [Test]
        public void TestUnref()
        {
            using var l = new Lua();

            l.DoString("function f() end");
            LuaType type = l.GetGlobal("f");
            Assert.AreEqual(LuaType.Function, type, "#1");

            l.PushCopy(-1);
            l.Ref(LuaRegistry.Index);
            l.Close();
        }

        [Test]
        public void TestThreadFromToPtr()
        {
            using var l = new Lua();

            l.Register("func1", Func);

            Lua thread = l.NewThread();

            thread.DoString("func1(10,10)");
            thread.DoString("func1(10,10)");
        }

        [Test]
        public void TestCoroutineCallback()
        {
            using var l = new Lua();

            l.Register("func1", Func);

            string script = @"function yielder() 
                                a=1; 
                                coroutine.yield();
                                a = func1(3,2);
                                a = func1(4,2);
                                coroutine.yield();
                                a=2;
                                coroutine.yield();
                             end
                             co_routine = coroutine.create(yielder);
                             while coroutine.resume(co_routine) do end;";
            l.DoString(script);
            l.DoString(script);

            l.GetGlobal("a");
            int? a = l.ToInteger(-1);
            Assert.AreEqual(a, 2d);
        }

        [Test]
        public void TestToStringStack()
        {
            using var l = new Lua();

            l.PushNumber(3);
            l.PushInteger(4);

            int currentTop = l.GetTop();

            string four = l.ToString(-1)!;

            int newTop = l.GetTop();

            Assert.AreEqual("4", four, "#1.1");
            Assert.AreEqual(currentTop, newTop, "#1.2");
        }

        [Test]
        public void GettingUpValueDoesntCrash()
        {
            using var l = new Lua();

            l.LoadString("hello = 1");
            string result = l.GetUpValue(-1, 1)!;

            Assert.AreEqual("_ENV", result, "#1");
        }

        [Test]
        public void ResumeAcceptsNull()
        {
            using var l = new Lua();

            l.LoadString("hello = 1");
            LuaStatus result = l.Resume(null, 0, out int _);

            Assert.AreEqual(LuaStatus.OK, result);
        }

        [Test]
        public void TestWarning()
        {
            using var l = new Lua();

            LuaWarnFunction warnFunction = MyWarning;
            var sb = new StringBuilder();

            l.PushObject(sb);
            l.SetWarningFunction(warnFunction, l.LuaState);

            l.Warning("Ola um dois tres", false);

            Assert.AreEqual("Ola um dois tres", sb.ToString(), "#1");
        }

        [Test]
        public static void TestNewLib()
        {
            using var l = new Lua();

            l.RequireF("foobar", OpenFoo, true);

            l.DoString("s = foobar.foo()");

            l.GetGlobal("s");

            bool check = l.IsString(-1);
            string s = l.ToString(-1, false)!;

            Assert.IsTrue(check, "#1");
            Assert.AreEqual("bar", s, "#2");
        }

        static readonly LuaRegister[] fooReg =
        {
            new LuaRegister { name = "foo", function = Foo, },
            new LuaRegister { name = null,  function = null, }
        };

        static int Func(IntPtr p)
        {
            var l = Lua.FromIntPtr(p);
            int param1 = l!.CheckInteger(1)!;
            int param2 = l!.CheckInteger(2)!;

            l.PushInteger(param1 + param2);
            return 1;
        }

        static int TestValueData(IntPtr p)
        {
            var l = Lua.FromIntPtr(p);

            Rectangle? param1 = l!.ToObject<Rectangle?>(1)!;
            Rectangle param2 = l!.ToObject<Rectangle>(2)!;
            DateTime param3 = l!.ToObject<DateTime>(3)!;

            Assert.IsNull(param1, "#1");
            Assert.AreEqual(param2, new Rectangle(10, 10, 100, 100), "#2");
            Assert.AreEqual(param3, new DateTime(2018, 10, 10, 0, 0, 0), "#3");

            return 0;
        }

        static void MyWarning(IntPtr ud, IntPtr msg, int tocont)
        {
            var l = Lua.FromIntPtr(ud);
            StringBuilder sb = l!.ToObject<StringBuilder>(-1)!;
            string? message = Marshal.PtrToStringAnsi(msg);
            sb.Append(message!);
        }

        static int OpenFoo(IntPtr p)
        {
            var l = Lua.FromIntPtr(p);
            l!.NewLib(fooReg);
            return 1;
        }

        static int Foo(IntPtr p)
        {
            var l = Lua.FromIntPtr(p);
            l!.PushString("bar");
            return 1;
        }

        static void AssertString(string chunk, Lua l)
        {
            string error = string.Empty;

            LuaStatus result = l.LoadString(chunk);

            if (result != LuaStatus.OK)
            {
                error = l.ToString(1, false)!;
            }

            Assert.True(result == LuaStatus.OK, "Fail loading string: " + chunk + "ERROR:" + error);

            result = l.PCall(0, -1, 0);

            if (result != 0)
            {
                error = l.ToString(1, false)!;
            }

            Assert.True(result == 0, "Fail calling chunk: " + chunk + " ERROR: " + error);
        }
    }
}
