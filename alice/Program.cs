using System;
using System.Collections.Generic;
using System.IO;
using WSOFT.ConfigManerger;
using AliceScript;

namespace alice
{
    class Program
    {
        /// <summary>
        /// アプリケーションのメインエントリポイントです
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            ParsedArguments pa = new ParsedArguments(args);
            if (pa.Flags.Contains("r") || pa.Flags.Contains("run"))
            {
                //実行モード
                if (pa.Values.ContainsKey("print"))
                {
                    if (pa.Values["print"].ToLower()=="off")
                    {
                        allow_print = false;
                    }
                    else
                    {
                        print_redirect_files.Add(pa.Values["print"]);
                    }
                }
                if (pa.Values.ContainsKey("throw"))
                {
                    if (pa.Values["throw"].ToLower() == "off")
                    {
                        allow_throw = false;
                    }
                    else
                    {
                        throw_redirect_files.Add(pa.Values["throw"]);
                    }
                }
                if (pa.Values.ContainsKey("runtime"))
                {
                    Alice.Runtime_File_Path = pa.Values["runtime"];
                }
                bool mainfile = pa.Flags.Contains("mainfile");
                ThrowErrorManerger.HandleError = true;
                ThrowErrorManerger.ThrowError += ThrowErrorManerger_ThrowError;
                Interpreter.Instance.OnOutput += Instance_OnOutput;
                foreach (string fn in pa.Files)
                {
                    Alice.ExecuteFile(Path.GetFileName(fn),mainfile);
                }
            }
            else
            {
                Shell.Do(args);
            }
        }
        private static bool allow_print = true;
        private static List<string> print_redirect_files = new List<string>();
        private static bool allow_throw = true;
        private static List<string> throw_redirect_files = new List<string>();
        private static void Instance_OnOutput(object sender, OutputAvailableEventArgs e)
        {
            if (allow_print)
            {
                Console.Write(e.Output);
            }
            if (print_redirect_files.Count > 0)
            {
                foreach(string fn in print_redirect_files)
                {
                    File.AppendAllText(fn,e.Output);
                }
            }
        }

        private static void ThrowErrorManerger_ThrowError(object sender, ThrowErrorEventArgs e)
        {
            if (e.Message != "")
            {
                if (allow_throw)
                {
                    AliceScript.Utils.PrintColor("エラー:" + e.Message + " 行" + e.Script.OriginalLineNumber + " コード:" + e.Script.OriginalLine + " ファイル名:" + Path.GetFileName(e.Script.Filename) + "\r\n", ConsoleColor.Red);
                    Dictionary<string, AliceScript.Variable> dic = AliceScript.Debug.Variables;
                    Console.WriteLine("変数の内容\r\n| 変数名 | 内容 |");
                    foreach (string s in dic.Keys)
                    {
                        Console.WriteLine("| " + s + " | " + dic[s].AsString() + " |");
                    }
                }
                if (throw_redirect_files.Count > 0)
                {
                    foreach (string fn in throw_redirect_files)
                    {
                        File.AppendAllText(fn, "エラー:" + e.Message + " 行" + e.Script.OriginalLineNumber + " コード:" + e.Script.OriginalLine + " ファイル名:" + Path.GetFileName(e.Script.Filename) + "\r\n");
                    }
                }
            }
        }
    }
}
