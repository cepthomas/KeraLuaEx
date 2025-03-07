using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;


namespace KeraLuaEx.Test
{
    /// <summary>An example of how to create a C# library that can be loaded by Lua.</summary>
    public partial class Interop
    {
        /// <summary>Main execution lua state.</summary>
        readonly Lua _l;

        /// <summary>Timer stuff.</summary>
        static readonly Stopwatch _sw = new();
        static long _startTicks = 0;

        #region Lifecycle
        /// <summary>
        /// Load the lua libs implemented in C#.
        /// </summary>
        public Interop(Lua l)
        {
            _l = l;

            // Load our lib stuff.
            LoadInterop();

            // Other inits.
            _startTicks = 0;
            _sw.Start();
        }
        #endregion

        #region Bound lua work functions
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
