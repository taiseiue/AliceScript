using System;
using System.Collections.Generic;
using System.IO;

namespace alice
{
 //これらのコードの一部にWSOFTScriptプロジェクトの成果物が含有されています
    class Program
    {
        static List<string> outpass = new List<string>();
        static List<string> errpass = new List<string>();
        static bool canoutput = true;
        static bool canerrput = true;
        static Mode mode = Mode.Shell;
        static List<string> Packages = new List<string>();
        static List<string> Scripts = new List<string>();
        static List<string> Embeddeds = new List<string>();
        static List<string> ress = new List<string>();
        static bool mainfile = false;
        static void Main(string[] args)
        {

            if (args.Length == 0 && File.Exists("args.txt"))
            {
                try { args = File.ReadAllText("args.txt").Split(' '); } catch { }
            }
            if (args.Length == 0)
            {
                alice.SecondProgram.Do(args);
            }
            else
            {

                foreach (string arg in args)
                {


                    if (arg.ToLower().StartsWith("-"))
                    {

                        if (arg.Contains(":"))
                        {
                            string code = arg;

                            ListAdd("out", code, outpass);
                            ListAdd("err", code, errpass);
                            ListAdd("error", code, errpass);

                            {
                                string str = GetConst("print", code).ToLower();
                                if (str == "off" || str == "no" || str == "false")
                                {
                                    canoutput = false;
                                }

                            }

                            {
                                string str = GetConst("throw", code).ToLower();
                                if (str == "off" || str == "no" || str == "false")
                                {
                                    canerrput = false;
                                }

                            }
                            
                        }
                        else
                        {

                            string code = arg.ToLower();

                            code = code.TrimStart('-');

                           if (code == "shell" || code == "sh" || code == "s")
                            {
                                mode = Mode.Shell;
                            }else if(code == "run" || code == "r")
                            {
                                mode = Mode.Running;
                            }
                           
                            else if (code == "version" || code == "ver" || code == "v")
                                mode = Mode.Version;
                        }




                    }
                   
                    else
                    {
                        Scripts.Add(arg);
                    }
                }

                switch (mode)
                {
                    case Mode.Shell:
                        {
                            alice.SecondProgram.Do(new string[] { });
                            break;
                        }
                    case Mode.Running:
                        {
                           
                            foreach (string s in Scripts)
                            {
                                if (canoutput)
                                {
                                    AliceScript.Interpreter.Instance.OnOutput += Instance_OnOutput;
                                }
                                if (canerrput)
                                {
                                    AliceScript.ThrowErrorManerger.HandleError = true;
                                    AliceScript.ThrowErrorManerger.ThrowError += ThrowErrorManerger_ThrowError;
                                }
                                AliceScript.Alice.Execute(File.ReadAllText(s));
                            }
                           
                            break;
                        }
                   
                    case Mode.Version:
                        {
                            Console.WriteLine(AliceScript.Alice.Execute("wsver").AsString());
                            break;
                        }
                }
            }



        }

        private static void ThrowErrorManerger_ThrowError(object sender, AliceScript.ThrowErrorEventArgs e)
        {

            AliceScript.Utils.PrintColor("実行中のエラー:" + e.Message + " 行" + e.Script.OriginalLineNumber + " コード:" + e.Script.OriginalLine + " ファイル名:" + e.Script.Filename + "\r\n", ConsoleColor.Red);
            Dictionary<string, AliceScript.Variable> dic = AliceScript.Debug.Variables;
            Console.WriteLine("変数の内容\r\n| 変数名 | 内容 |");
            foreach (string s in dic.Keys)
            {
                Console.WriteLine("| " + s + " | " + dic[s].AsString() + " |");
            }
        }

        private static void Instance_OnOutput(object sender, AliceScript.OutputAvailableEventArgs e)
        {
            Console.Write(e.Output);
        }

        enum Mode
        {
            Packaging, Running, Marging, Shell, Compile, Depackage, Version
        }

        private static void ListAdd(string label, string text, List<string> list)
        {

            if (text.StartsWith("-" + label + ":"))
            {
                list.Add(text.TrimStart(("-" + label + ":").ToCharArray()));
            }
        }
        private static string GetConst(string label, string text)
        {
            if (text.StartsWith("-" + label + ":"))
            {
                return text.TrimStart(("-" + label + ":").ToCharArray());
            }
            return "";
        }
        private static void DoOutPut(string fn, string output)
        {
            if (File.Exists(fn))
            {
                string cont = File.ReadAllText(fn);
                if (cont == "" || cont.EndsWith(Environment.NewLine))
                {
                    //改行済み
                    cont += output;
                }
                else
                {
                    //未改行
                    cont += Environment.NewLine + output;
                }
                File.WriteAllText(fn, cont);
            }
            else
            {
                File.WriteAllText(fn, output);
            }

        }
    }
}
