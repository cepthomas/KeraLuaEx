using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.ComponentModel;
using KeraLuaEx.Test;


namespace KeraLuaEx.Host
{
    public partial class HostForm : Form
    {
        #region Types
        /// <summary>Log level.</summary>
        enum Level { ERR, INF, DBG, SCR };
        #endregion

        #region Fields
        /// <summary>Detect file edited externally.</summary>
        readonly FileSystemWatcher _watcher = new();

        /// <summary>Cosmetics.</summary>
        Dictionary<Level, Color> _logColors = new();

        /// <summary>File has been edited.</summary>
        bool _dirty = false;

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

            _logColors = new()
            {
                { Level.INF, rtbOutput.BackColor },
                { Level.ERR, Color.HotPink },
                { Level.DBG, Color.LightGreen },
                { Level.SCR, Color.LightBlue },
            };
            Lua.LogMessage += (object? _, Lua.LogEventArgs a) => Log(Level.SCR, $"{a.Category} {a.Message}");

            Log(Level.INF, "============================ Starting up ===========================");

            var font = new Font("Consolas", 10);
            rtbScript.Font = font;
            rtbOutput.Font = font;

            rtbScript.KeyDown += (_, __) => _dirty = true;
            rtbScript.MouseDown += Script_MouseDown;

            _watcher.EnableRaisingEvents = false;
            _watcher.NotifyFilter = NotifyFilters.LastWrite;
            _watcher.Changed += Watcher_Changed;

            btnRunGlobal.Click += (_, __) => RunTests("Global");
            btnRunModule.Click += (_, __) => RunTests("Module");
            btnRunPlay.Click += (_, __) => RunTests("Play");
            btnRunErrors.Click += (_, __) => RunTests("Errors");
            btnClearOnRun.Checked = true;

            base.OnLoad(e);
        }

        /// <summary>
        /// Where are we?
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Script_MouseDown(object? sender, MouseEventArgs e)
        {
            var index = rtbScript.SelectionStart;
            var row = rtbScript.GetLineFromCharIndex(index);

            // Get the column.
            int firstChar = rtbScript.GetFirstCharIndexFromLine(row);
            int col = index - firstChar;

            txtPos.Text = $"R:{row + 1} C:{col + 1}";
        }

        /// <summary>
        /// Clean up on shutdown.
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_dirty)
            {
                if (MessageBox.Show("File has been edited - do you want to save the changes?", "Hey you!",
                    MessageBoxButtons.OKCancel) == DialogResult.OK)
                {
                    _watcher.EnableRaisingEvents = false;
                    File.WriteAllText(_fn, rtbScript.Text);
                    _watcher.EnableRaisingEvents = true;
                    _dirty = false;
                }
                else
                {
                    e.Cancel = true;
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
                components?.Dispose();
            }

            base.Dispose(disposing);
        }
        #endregion

        #region File handling
        /// <summary>
        /// Allows the user to select a script file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Open_Click(object sender, EventArgs e)
        {
            if (_dirty)
            {
                if (MessageBox.Show("File has been edited - do you want to save the changes?",
                    "Hey you!", MessageBoxButtons.OKCancel) == DialogResult.OK)
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
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Save_Click(object sender, EventArgs e)
        {
            if (_dirty)
            {
                _watcher.EnableRaisingEvents = false;
                File.WriteAllText(_fn, rtbScript.Text);
                _watcher.EnableRaisingEvents = true;
                _dirty = false;
            }
        }

        /// <summary>
        /// Common script file opener.
        /// </summary>
        /// <param name="fn">The lua file to open.</param>
        /// <returns>Error string or empty if ok.</returns>
        string OpenScriptFile(string fn)
        {
            string ret = "";
            rtbScript.Clear();
            _watcher.EnableRaisingEvents = false;

            try
            {
                string s = File.ReadAllText(fn);
                rtbScript.Text = s;
                Text = $"Testing {fn}";
                Log(Level.INF, $"Opening {fn}");

                _watcher.Path = Path.GetDirectoryName(fn)!;
                _watcher.Filter = Path.GetFileName(fn);
                _watcher.EnableRaisingEvents = true;
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
            this.InvokeIfRequired(_ =>
            {
                Log(Level.DBG, $"Watcher_Changed");

                if (_dirty)
                {
                    if (MessageBox.Show("File has been edited externally and there are changes locally - do you want to save the changes?",
                        "Hey you!", MessageBoxButtons.OKCancel) == DialogResult.OK)
                    {
                        _watcher.EnableRaisingEvents = false;
                        File.WriteAllText(_fn, rtbScript.Text);
                        _watcher.EnableRaisingEvents = true;
                        _dirty = false;
                    }
                }
                else
                {
                    // Reload from file.
                    string s = File.ReadAllText(_fn);
                    rtbScript.Text = s;
                }
            });
        }
        #endregion

        #region Internal functions
        /// <summary>
        /// 
        /// </summary>
        /// <param name="which"></param>
        void RunTests(string which)
        {
            if (btnClearOnRun.Checked)
            {
                rtbOutput.Clear();
            }

            Log(Level.INF, $"Starting tests:{which}");
            LuaExTests tests = new();

            try
            {
                tests.Setup();

                //var srcPath = TestUtils.GetSourcePath();
                //var scriptsPath = Path.Combine(srcPath, "..\\", "Test", "scripts");
                //OpenScriptFile(Path.Combine(scriptsPath, "luaex_mod.lua"));

                switch (which)
                {
                    case "Module": tests.ScriptModule(); break;
                    case "Global": tests.ScriptGlobal(); break;
                    case "Errors": tests.ScriptErrors(); break;
                    case "Play": tests.Play(); break;
                }
            }
            catch (Exception ex)
            {
                Log(Level.ERR, $"{ex}");
            }
            finally
            {
                tests.TearDown();
            }

            Log(Level.INF, $"Finished tests:{which}");
        }

        /// <summary>
        /// Log it.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="msg"></param>
        void Log(Level level, string msg)
        {
            string text = $">{level} {msg}{Environment.NewLine}";
            int _maxText = 10000;

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
        #endregion
    }

    public static class Extensions
    {
        /// <summary>
        /// Invoke helper, maybe. Extension method.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="action"></param>
        public static void InvokeIfRequired<T>(this T obj, InvokeIfRequiredDelegate<T> action) where T : ISynchronizeInvoke
        {
            if (obj.InvokeRequired)
            {
                obj.Invoke(action, new object[] { obj });
            }
            else
            {
                action(obj);
            }
        }
        public delegate void InvokeIfRequiredDelegate<T>(T obj) where T : ISynchronizeInvoke;
    }
}
