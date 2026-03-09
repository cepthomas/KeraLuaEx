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
    

namespace KeraLuaEx.Test
{
    internal class Program
    {
        static void Main() //string[] args)
        {
            KeraLuaExTests tests = new();
            
            try
            {
                tests.Setup();

                tests.ScriptModule();
                tests.ScriptGlobal();
                tests.ScriptErrors();
                //tests.ScriptApi();
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
    }
}
