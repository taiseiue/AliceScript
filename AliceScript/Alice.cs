using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;

namespace AliceScript
{
   public static class Alice
    {
        public static Variable Execute(string code)
        {
           return Interpreter.Instance.Process(code);
        }
        public static event Exiting Exiting;
        internal static void OnExiting(int exitcode=0)
        {
            ExitingEventArgs e = new ExitingEventArgs();
            e.Cancel = false;
            e.ExitCode = exitcode;
            Exiting?.Invoke(null,e);
            if (e.Cancel)
            {
                return;
            }
            else
            {
                Environment.Exit(e.ExitCode);
            }
        }
        public static string Runtime_File_Path = Path.Combine(Assembly.GetExecutingAssembly().Location,"Alice.Runtime.dll");
    }
    public delegate void Exiting(object sender,ExitingEventArgs e);
    public class ExitingEventArgs : EventArgs
    {
        /// <summary>
        /// キャンセルする場合は、True
        /// </summary>
        public bool Cancel { get; set; }
        /// <summary>
        /// 要求されている終了コードを表します
        /// </summary>
        public int ExitCode { get; set; }
    }

}
