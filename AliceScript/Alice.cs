using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AliceScript
{
   public static class Alice
    {
        public static Variable Execute(string code,string filename="",bool mainFile=false)
        {
           return Interpreter.Instance.Process(code,filename,mainFile);
        }
        public static Variable ExecuteFile(string filename,bool mainFile=false)
        {
            return Interpreter.Instance.ProcessFile(filename,mainFile);
        }
        public static Task<Variable> ExecuteAsync(string code,string filename="",bool mainFile = false)
        {
            return Interpreter.Instance.ProcessAsync(code,filename,mainFile);
        }
        public static Task<Variable> ExecuteFileAsync(string filename,bool mainFile = false)
        {
            return Interpreter.Instance.ProcessFileAsync(filename,mainFile);
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
        public static string Runtime_File_Path = Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),"Alice.Runtime.dll");
    }
    public delegate void Exiting(object sender,ExitingEventArgs e);
    public class ExitingEventArgs : EventArgs
    {
        /// <summary>
        /// キャンセルする場合は、True
        /// </summary>
        public bool Cancel { get; set; }
        /// <summary>
        /// 終了コードを表します
        /// </summary>
        public int ExitCode { get; set; }
    }

}
