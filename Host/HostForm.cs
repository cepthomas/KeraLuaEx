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
        /// <summary>Lua context.</summary>
        readonly Lua _l = new();

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

            _l = new(); // Shut up compiler.
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
            Test.Common.LogMessage += (object? sender, string e) => Log(Level.SCR, e);

            Log(Level.INF, "============================ Starting up ===========================");

            var font = new Font("Consolas", 10);
            rtbScript.Font = font;
            rtbOutput.Font = font;
            rtbStack.Font = font;

            rtbScript.KeyDown += (object? _, KeyEventArgs __) => _dirty = true;
            rtbScript.MouseDown += Script_MouseDown;

            _watcher.EnableRaisingEvents = false;
            _watcher.NotifyFilter = NotifyFilters.LastWrite;
            _watcher.Changed += Watcher_Changed;

            var args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                OpenScriptFile(args[1]);
                if (_fn != "")
                {
                    _scriptsPath = Path.GetDirectoryName(_fn)!;
                }
            }

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
                    rtbScript.Text = s;
                }
            });
        }
        #endregion

        #region Run tests
        /// <summary>
        /// Do something.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void GoMain_Click(object sender, EventArgs e)
        {
            rtbOutput.Clear();

            LuaExTests tests = new();

            try
            {
                tests.Setup();
                tests.Basic();
            }
            catch (Exception ex)
            {
                Log(Level.ERR, $"{ex}");
            }
            finally
            {
                tests.TearDown();
            }
        }
        #endregion

        #region Internal functions
        /// <summary>
        /// Log it.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="msg"></param>
        void Log(Level level, string msg)
        {
            string text = $">{level} {msg}{Environment.NewLine}";
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
            var s = Common.DumpStack(_l);
            rtbStack.Text = s;
        }
        #endregion
    }
}
