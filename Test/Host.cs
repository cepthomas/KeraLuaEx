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
    

// Entry.
var app = new Host();
app.Dispose();

namespace KeraLuaEx.Test
{
    public partial class Host : IDisposable
    {
        /// <summary>Timer stuff.</summary>
        static readonly Stopwatch _sw = new();
        static long _startTicks = 0;

        #region Lifecycle
        /// <summary>Constructor.</summary>
        public Host()
        {
            // Load luainterop lib.
            LoadInterop();
            // LoadScript(scriptFn, [thisDir, lbotDir]);

            // Other inits.
            _startTicks = 0;
            _sw.Start();

            Lua.LogMessage += (object? _, Lua.LogEventArgs a) => Console.WriteLine(a.Message);
            //Lua.LogMessage += (object? _, Lua.LogEventArgs a) => Log($"[{a.Category}] {a.Message}");

            LuaExTests tests = new();
            try
            {
                tests.Setup();

                tests.ScriptModule();
                tests.ScriptGlobal();
                tests.ScriptErrors();
                tests.ScriptApi();
                //tests.Play();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");

                //var st = "???";
                //if (ex.StackTrace is not null)
                //{
                //    var lst = ex.StackTrace.Split(Environment.NewLine);
                //    if (lst.Length >= 2)
                //    {
                //        st = lst[^2];
                //    }
                //}
                //Console.WriteLine($"{ex.Message} {st}");
            }
            finally
            {
                tests.TearDown();
            }
        }
        public void Dispose()
        {
            //throw new NotImplementedException();
        }
        #endregion

        #region Lua callback functions
        /// <summary>
        /// Interop error handler. Do something with this - log it or other.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        bool ErrorHandler(Exception e)
        {
            Lua.Log(Lua.Category.ERR, e.ToString());
            return false;
        }

        /// <summary>
        /// Replacement for lua print(), redirects to log.
        /// </summary>
        /// <param name="msg">What to log.</param>
        /// <returns>Status - required</returns>
        bool PrintExCb(string? msg)
        {
            // Do the work.
            Lua.Log(Lua.Category.INF, msg ?? "null msg??");
            return true;
        }

        /// <summary>
        /// Get current msec.
        /// </summary>
        /// <param name="on">On or off.</param>
        /// <returns>Msec</returns>
        double TimerCb(bool? on)
        {
            // Do the work.
            double totalMsec = 0;
            if ((bool)on!)
            {
                _startTicks = _sw.ElapsedTicks; // snap
            }
            else if (_startTicks > 0)
            {
                long t = _sw.ElapsedTicks; // snap
                totalMsec = (t - _startTicks) * 1000D / Stopwatch.Frequency;
            }

            // Return results.
            return totalMsec;
        }
        #endregion
       
    }
}
