using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Nodes;
using System.ComponentModel;
//using KeraLuaEx;


//TODO2 need line numbers

namespace KeraLuaEx.Host
{
    public partial class HostForm : Form
    {
        #region Types
        /// <summary>Log level.</summary>
        enum Level { ERR, INF, DBG, SCR };
        #endregion

        #region Fields
        /// <summary>lua context.</summary>
        Lua _l = new();

        /// <summary>Needed to bind static lua functions.</summary>
        static HostForm _mf;

        /// <summary>Bound lua function.</summary>
        readonly LuaFunction _funcPrint = PrintEx;
        /// <summary>Bound lua function.</summary>
        readonly LuaFunction _funcTimer = Timer;

        /// <summary>Detect file edited externally.</summary>
        readonly FileSystemWatcher _watcher = new();

        /// <summary>Cosmetics.</summary>
        Dictionary<Level, Color> _logColors = new();

        /// <summary>File has been edited.</summary>
        bool _dirty = false;

        /// <summary>Metrics.</summary>
        readonly Stopwatch _sw = new();
        long _startTicks = 0;

        /// <summary>Current file.</summary>
        string _fn = "";

        /// <summary>Where to look.</summary>
        string _scriptsPath = "";
        #endregion

        #region Lifecycle
        /// <summary>
        /// Constructor.
        /// </summary>
        public HostForm()
        {
            InitializeComponent();

            _l = new(); // Shut up compiler.
            _mf = this;
        }

        /// <summary>
        /// Post create init.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            StartPosition = FormStartPosition.Manual;
            Location = new(20, 20);
            ClientSize = new(1500, 950);

            rtbScript.Clear();
            rtbOutput.Clear();

            // Settings - fixed for now.
            _logColors = new()
            {
                { Level.ERR, Color.Pink },
                { Level.INF, rtbOutput.BackColor },
                { Level.DBG, Color.LightGreen },
                { Level.SCR, Color.Magenta },
            };

            var font = new Font("Consolas", 10);
            rtbScript.Font = font;
            rtbOutput.Font = font;
            rtbStack.Font = font;

            Log(Level.INF, "============================ Starting up ===========================");

            var args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                OpenScriptFile(args[1]);
                if (_fn != "")
                {
                    _scriptsPath = Path.GetDirectoryName(_fn)!;
                }
            }

            rtbScript.KeyDown += (object? _, KeyEventArgs __) => _dirty = true;

            _watcher.EnableRaisingEvents = true;
            _watcher.NotifyFilter = NotifyFilters.LastWrite;
            _watcher.Changed += Watcher_Changed;

            _sw.Start();

            base.OnLoad(e);
        }

        /// <summary>
        /// Clean up on shutdown.
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_dirty)
            {
                if (MessageBox.Show("File has been edited - do you want to save the changes?", "Hey you!", MessageBoxButtons.OKCancel) == DialogResult.OK)
                {
                    _watcher.EnableRaisingEvents = false;
                    File.WriteAllText(_fn, rtbScript.Text);
                    _watcher.EnableRaisingEvents = true;
                    _dirty = false;
                }
            }

            base.OnFormClosing(e);
        }

        /// <summary>
        /// Resource clean up.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _sw.Stop();
                components?.Dispose();
            }

            base.Dispose(disposing);
        }
        #endregion

        #region File handling
        /// <summary>
        /// Allows the user to select a script file.
        /// </summary>
        void Open_Click(object? sender, EventArgs e)
        {
            if (_dirty)
            {
                if (MessageBox.Show("File has been edited - do you want to save the changes?", "Hey you!", MessageBoxButtons.OKCancel) == DialogResult.OK)
                {
                    _watcher.EnableRaisingEvents = false;
                    File.WriteAllText(_fn, rtbScript.Text);
                    _watcher.EnableRaisingEvents = true;
                    _dirty = false;
                }
            }

            using OpenFileDialog openDlg = new()
            {
                Filter = "Lua files | *.lua",
                Title = "Select a Lua file",
                InitialDirectory = _scriptsPath,
            };

            if (openDlg.ShowDialog() == DialogResult.OK)
            {
                OpenScriptFile(openDlg.FileName);
            }
        }

        /// <summary>
        /// Common script file opener.
        /// </summary>
        /// <param name="fn">The np file to open.</param>
        /// <returns>Error string or empty if ok.</returns>
        string OpenScriptFile(string fn)
        {
            string ret = "";
            rtbScript.Clear();

            try
            {
                string s = File.ReadAllText(fn);
                rtbScript.AppendText(s);
                Text = $"Testing {fn}";
                Log(Level.INF, $"Opening {fn}");

                _watcher.Path = Path.GetDirectoryName(fn)!;
                _watcher.Filter = Path.GetFileName(fn);
                _fn = fn;
            }
            catch (Exception ex)
            {
                ret = $"Couldn't open the script file {fn} because {ex.Message}";
                Log(Level.ERR, ret);
                _fn = "";
            }

            return ret;
        }

        /// <summary>
        /// File changed externally.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            Log(Level.DBG, $"Watcher_Changed");

            if (_dirty)
            {
                if (MessageBox.Show("File has been edited externally and there are changes locally - do you want to save the changes?", "Hey you!", MessageBoxButtons.OKCancel) == DialogResult.OK)
                {
                    _watcher.EnableRaisingEvents = false;
                    File.WriteAllText(_fn, rtbScript.Text);
                    _watcher.EnableRaisingEvents = true;
                    _dirty = false;
                }
            }
            else
            {
                // Reload.
                string s = File.ReadAllText(_fn);
                rtbScript.AppendText(s);
            }
        }
        #endregion

        #region Lua calls C# function
        /// <summary>
        /// Called by lua script.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        static int PrintEx(IntPtr p)
        {
            var l = Lua.FromIntPtr(p)!;
            // args
            string msg = l.ToString(-1)!;
            // work
            _mf.Log(Level.SCR, $"printex:{msg}");
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
            var msec = _mf.Timer(on);
            // return
            l.PushNumber(msec);
            return 1;
        }
        #endregion

        #region C# calls Lua function

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void GoMain_Click(object sender, EventArgs e)
        {
            Setup();
            try
            {
                string s = rtbScript.Text;
                _l.LoadString(s);
                _l.PCall(0, -1, 0);

                List<string>? ls = new();

                ShowStack();

                //ls = Utils.DumpStack(_l);
                //Log(Level.INF, FormatDump("Stack", ls, true));

                var x = Utils.GetGlobalValue(_l, "g_table");
                var table = (DataTable)x.val!;
                Log(Level.INF, table.Format("g_table"));
                //g_table:
                //  dev_type(String):bing_bong
                //  abool(Boolean):true
                //  channel(Number):10

                x = Utils.GetGlobalValue(_l, "g_number");
                Log(Level.INF, Utils.FormatCsharpVal("g_number", x.val));

                x = Utils.GetGlobalValue(_l, "g_int");
                Log(Level.INF, Utils.FormatCsharpVal("g_int", x.val));

                //x = Utils.GetGlobalValue(_l, "_G");
                //table = x.val as Table;
                //Log(Level.INF, table.Format("_G"));
                //public static List<string> DumpGlobals(Lua l)
                //{
                //    // Get global table.
                //    l.PushGlobalTable();
                //    var ls = DumpTable(l);
                //    // Remove global table(-1).
                //    l.Pop(1);
                //    return ls;
                //}

                x = Utils.GetGlobalValue(_l, "g_list_int");
                table = (DataTable)x.val!;
                Log(Level.INF, table.Format("g_list_int"));
                //g_list_int:
                //  1(Number):2
                //  2(Number):56
                //  3(Number):98
                //  4(Number):2

                x = Utils.GetGlobalValue(_l, "things");
                table = (DataTable)x.val!;
                Log(Level.INF, table.Format("things"));


                ///// Execute a lua function.
                LuaType gtype = _l.GetGlobal("g_func");
                // Push the arguments to the call.
                _l.PushString("az9011 birdie");
                // Do the actual call.
                _l.PCall(1, 1, 0);
                // Get result.
                var res = _l.ToInteger(-1);
                Log(Level.DBG, $"Function returned {res} should be 13");


                // DataTable create and access tests TODO1


                // Json TODO1  From json.lua:
                // json.encode(value)
                // Returns a string representing value encoded in JSON.
                // json.encode({ 1, 2, 3, { x = 10 } }) -- Returns '[1,2,3,{"x":10}]'
                //
                // json.decode(str)
                // Returns a value representing the decoded JSON string.
                // json.decode('[1,2,3,{"x":10}]') -- Returns { 1, 2, 3, { x = 10 } }
                // {"TUNE":{"channel":1,"dev_type":"midi_in"},"WHIZ":{"channel":10,"abool":true,"dev_type":"bing_bong"},"TRIG":{"channel":2,"adouble":1.234,"dev_type":"virt_key"}}
            }
            catch (Exception ex)
            {
                Log(Level.ERR, $"{ex}");
            }

            TearDown();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void GoJson_Click(object sender, EventArgs e)
        {
            Setup();

            string s = rtbScript.Text;
            _l.LoadString(s);
            _l.PCall(0, -1, 0);

            //List<string>? ls = new();

            var sjson = @"{""TUNE"":{""channel"":1,""dev_type"":""midi_in""},""WHIZ"":{""channel"":10,""alist"":[2,56,98,2],""dev_type"":""bing_bong""},""TRIG"":{""channel"":2,""adouble"":1.234,""dev_type"":""virt_key""}}";
            var table = DataTable.FromJson(sjson);
            // Do some tests. Also ToJson().

            TearDown();
        }

        #region Internal functions
        /// <summary>
        /// Log it.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="msg"></param>
        void Log(Level level, string msg)
        {
            string text = $"> {msg}{Environment.NewLine}";
            int _maxText = 5000;

            // Trim buffer.
            if (_maxText > 0 && rtbOutput.TextLength > _maxText)
            {
                rtbOutput.Select(0, _maxText / 5);
                rtbOutput.SelectedText = "";
            }

            rtbOutput.SelectionBackColor = _logColors[level];
            rtbOutput.AppendText(text);
            rtbOutput.ScrollToCaret();
        }

        /// <summary>
        /// Show the contents of the stack.
        /// </summary>
        void ShowStack()
        {
            var s = Utils.DumpStack(_l);
            rtbStack.Text = s;
        }

        /// <summary>
        /// Stop the elapsed timer and return msec.
        /// </summary>
        /// <param name="on">Start or stop.</param>
        /// <returns></returns>
        double Timer(bool on)
        {
            double totalMsec = 0;
            if (on)
            {
                _startTicks = _sw.ElapsedTicks; // snap
            }
            else
            {
                if (_startTicks > 0)
                {
                    long t = _sw.ElapsedTicks; // snap
                    totalMsec = (t - _startTicks) * 1000D / Stopwatch.Frequency;
                }
            }
            return totalMsec;
        }

        /// <summary>
        /// Pretend unit test function.
        /// </summary>
        void Setup()
        {
            rtbOutput.Clear();

            _l.Close();
            _l = new Lua();
            _l.Register("printex", _funcPrint);
            _l.Register("timer", _funcTimer);

            Utils.SetLuaPath(_l, new() { _scriptsPath });
        }

        /// <summary>
        /// Pretend unit test function.
        /// </summary>
        void TearDown()
        {
            _l.Close();
        }
        #endregion
    }
}
