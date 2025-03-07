using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Linq;
using System.Diagnostics;


namespace KeraLuaEx
{
    /// <summary>Stuff added for KeraLuaEx.</summary>
    public partial class Lua
    {
        #region Properties
        /// <summary>On LuaStatus error either throw or return error code.</summary>
        public bool ThrowOnError { get; set; } = true;
        #endregion

        #region Simple logging
        public enum Category { DBG, INF, ERR };

        public class LogEventArgs : EventArgs
        {
            /// <summary>What it be.</summary>
            public Category Category { get; set; } = Category.ERR;
            /// <summary>The information.</summary>
            public string Message { get; set; } = "";
        }

        /// <summary>Client app can listen in.</summary>
        public static event EventHandler<LogEventArgs>? LogMessage;

        /// <summary>Log message event.</summary>
        public static void Log(Category cat, string msg)
        {
            LogMessage?.Invoke(null, new() { Category = cat, Message = msg });
        }
        #endregion

        #region Added API functions
        /// <summary>
        /// Make a TableEx from the lua table on the top of the stack.
        /// Like other "to" functions except also does the pop.
        /// </summary>
        /// <param name="index">FUTURE Support index other than top?</param>
        /// <returns>TableEx object or null if failed, check the log.</returns>
        public TableEx? ToTableEx(int index)
        {
            TableEx? t = null;
            try
            {
                t = new(this);
            }
            catch (Exception ex)
            {
                // Stack is probably a mess so reset it. Implies that this is a fatal event.
                SetTop(0);
                Log(Category.ERR, ex.Message);
            }

            return t;
        }

        /// <summary>
        /// Push a table onto lua stack.
        /// </summary>
        /// <param name="table"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void PushTableEx(TableEx table)
        {
            switch (table.Type)
            {
                case TableEx.TableType.Dictionary:
                    var dict = table.AsDict();
                    // Create a new empty table and push it onto the stack.
                    NewTable();
                    // Add the values from the source.
                    foreach (var f in dict)
                    {
                        PushString(f.Key);
                        switch (f.Value)
                        {
                            case string s: PushString(s); break;
                            case bool b: PushBoolean(b); break;
                            case int i: PushInteger(i); break;
                            case double d: PushNumber(d); break;
                            case TableEx t: PushTableEx(t); break; // recursion!
                            default: break; // ignore
                        }
                        SetTable(-3);
                    }
                    break;

                case TableEx.TableType.StringList:
                    var slist = table.AsList<string>();
                    // Create a new empty table and push it onto the stack.
                    NewTable();
                    // Add the values from the source.
                    for (int i = 0; i < slist.Count; i++)
                    {
                        PushInteger(i + 1);
                        PushString(slist[i]);
                        SetTable(-3);
                    }
                    break;


                case TableEx.TableType.IntList:
                    var ilist = table.AsList<int>();
                    // Create a new empty table and push it onto the stack.
                    NewTable();
                    // Add the values from the source.
                    for (int i = 0; i < ilist.Count; i++)
                    {
                        PushInteger(i + 1);
                        PushInteger(ilist[i]);
                        SetTable(-3);
                    }
                    break;

                case TableEx.TableType.DoubleList:
                    var dlist = table.AsList<double>();
                    // Create a new empty table and push it onto the stack.
                    NewTable();
                    // Add the values from the source.
                    for (int i = 0; i < dlist.Count; i++)
                    {
                        PushInteger(i + 1);
                        PushNumber(dlist[i]);
                        SetTable(-3);
                    }
                    break;

                default:
                    throw new InvalidOperationException($"Unsupported table type {table.Type}");
            }
        }

        /// <summary>
        /// Returns  if the value at the given index is a table. 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool IsTableEx(int index)
        {
            return IsTable(index);
        }
        #endregion

        #region Helper functions
        /// <summary>
        /// Sets package.path for the context.
        /// </summary>
        /// <param name="paths"></param>
        public void SetLuaPath(List<string> paths)
        {
            List<string> parts = ["?", "?.lua"];
            paths.ForEach(p => parts.Add(Path.Join(p, "?.lua").Replace('\\', '/')));
            string s = string.Join(';', parts);
            s = $"package.path = \"{s}\"";
            DoString(s);
        }

        /// <summary>
        /// Converts to integer or number.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public object? DetermineNumber(int index)
        {
            // return IsInteger(index) ? ToInteger(index) : ToNumber(index); // ternary op doesn't work - some subtle typing thing?
            if (IsInteger(index)) { return ToInteger(index); }
            else if (IsNumber(index)) { return ToNumber(index); }
            else { return null; }
        }
        #endregion

        #region Capture error stack trace
        /// <summary>
        /// Message handler used to run all chunks.
        /// </summary>
        /// <returns></returns>
        private static int MsgHandler(IntPtr p)
        {
            var l = FromIntPtr(p)!;
            string? msg = l.ToString(1); //, false)!;
            if (msg is null)  // is error object not a string?
            {
                // does it have a metamethod that produces a string?
                if (l.CallMetaMethod(1, "__tostring") &&   l.Type(-1) == LuaType.String)
                {
                    // that is the message
                    return 1;
                }
                else
                {
                    msg = $"(error object is a {l.Type(1)} value)";
                    l.PushString(msg);
                }
            }

            // append and return  a standard traceback
            //l.Traceback(l, msg, 1);
            
            return 1;
        }
        static readonly LuaFunction _funcMsgHandler = MsgHandler;

        /// <summary>
        /// Interface to 'lua_pcall', which sets appropriate message function and C-signal handler. Used to run all chunks.
        /// </summary>
        /// <param name="narg"></param>
        /// <param name="nres"></param>
        /// <returns></returns>
        public LuaStatus DoCall(int narg, int nres)
        {
            LuaStatus lstat;
            int fbase = GetTop() - narg;  // function index
            PushCFunction(_funcMsgHandler);  // push message handler
            Insert(fbase);  // put it under function and args
            lstat = PCall(narg, nres, fbase);
            Remove(fbase);  // remove message handler from the stack
            return lstat;
        }
        #endregion

        #region Quality control
        /// <summary>
        /// Check lua status and log an error. If ThrowOnError is true, throws an exception.
        /// </summary>
        /// <param name="lstat">Thing to look at.</param>
        /// <param name="appFile">Ignore - compiler use.</param>
        /// <param name="appLine">Ignore - compiler use.</param>
        /// <returns>True means error</returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="LuaException"></exception>
        public bool EvalLuaStatus(LuaStatus lstat, [CallerFilePath] string appFile = "", [CallerLineNumber] int appLine = -1)
        {
            bool hasError = false;

            if (lstat >= LuaStatus.ErrRun)
            {
                hasError = true;

                // Get error message on stack.
                string luaError;
                if (GetTop() > 0)
                {
                    luaError = ToString(-1)!.Trim();
                    Pop(1); // remove
                }
                else
                {
                    luaError = "No error message!!!";
                }

                Exception ex = lstat switch
                {
                    LuaStatus.ErrFile => new FileException(appFile, appLine, luaError),
                    LuaStatus.ErrSyntax => new SyntaxException(appFile, appLine, luaError),
                    _ => new LuaException(appFile, appLine, lstat, luaError),
                };

                Log(Category.ERR, ex.Message);

                if (ThrowOnError)
                {
                    throw ex;
                }
            }

            return hasError;
        }

        /// <summary>
        /// Check the stack size and log if incorrect. If ThrowOnError is true, throws an exception.
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="file">Ignore - compiler use.</param>
        /// <param name="line">Ignore - compiler use.</param>
        /// <returns>True means error</returns>
        /// <exception cref="LuaException"></exception>
        public bool CheckStackSize(int expected, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
        {
            bool hasError = false;

            int num = GetTop();

            if (num != expected)
            {
                hasError = true;
                var serror = $"Stack size expected {expected} actual {num} at {file}({line})";

                Log(Category.ERR, serror);
                if (ThrowOnError)
                {
                    throw new LuaException(file, line, LuaStatus.ErrRun, serror);
                }
            }

            return hasError;
        }
        #endregion

        #region Diagnostics
        /// <summary>
        /// Dump the contents of the stack.
        /// </summary>
        /// <param name="info"></param>
        /// <returns>List of strings.</returns>
        public List<string> DumpStack(string info = "")
        {
            List<string> ls = [];
            if (info != "")
            {
                ls.Add(info);
            }

            int num = GetTop();

            if (num > 0)
            {
                for (int i = 1; i <= num; i++)
                {
                    LuaType t = Type(i);
                    string st = t.ToString().ToLower();
                    string tinfo = $"    [{i}]:";

                    string s = t switch
                    {
                        LuaType.String => $"{tinfo}{ToString(i)}({st})",
                        LuaType.Boolean => $"{tinfo}{ToBoolean(i)}({st})",
                        LuaType.Number => $"{tinfo}{DetermineNumber(i)}({st})",
                        LuaType.Nil => $"{tinfo}nil",
                        LuaType.Table => $"{tinfo}{ToString(i) ?? "null"}({st})",
                        _ => $"{tinfo}{ToPointer(i):X}({st})",
                    };
                    ls.Add(s);
                }
            }
            else
            {
                ls.Add("Stack is empty");
            }

            return ls;
        }
        #endregion
    }

    #region Exceptions
    /// <summary>Lua script syntax error.</summary>
    public class SyntaxException : LuaExException
    {
        public SyntaxException(string appFile, int appLine, string luaError) :
            base(appFile, appLine, LuaStatus.ErrSyntax, luaError)
        { }
    }

    /// <summary>Lua script file error.</summary>
    public class FileException : LuaExException
    {
        public FileException(string appFile, int appLine, string luaError) :
            base(appFile, appLine, LuaStatus.ErrFile, luaError)
        { }
    }

    /// <summary>Internal error on lua side.</summary>
    public class LuaException : LuaExException
    {
        public LuaException(string appFile, int appLine, LuaStatus lstat, string luaError) :
            base(appFile, appLine, lstat, luaError)
        { }
    }

    /// <summary>Base exception class.</summary>
    public class LuaExException : Exception
    {
        public string AppFile { get; private set; }
        public int AppLine { get; private set; }
        public LuaStatus Status { get; private set; }
        public string LuaError { get; private set; }
        public override string Message { get { return _message; } }
        readonly string _message = "???";

        public LuaExException(string appFile, int appLine, LuaStatus lstat, string luaError) : base("???")
        {
            List<string> ps = [];
            ps.Add($"[{lstat}]");
            if (luaError != "") ps.Add(luaError);
            if (appFile != "") ps.Add($"({appFile}:{appLine})");
            _message = string.Join(' ', ps);

            AppFile = appFile;
            AppLine = appLine;
            LuaError = luaError;
            Status = lstat;
        }
    }
    #endregion
}
