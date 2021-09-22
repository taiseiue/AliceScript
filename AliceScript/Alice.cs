﻿using System;
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
        /// <summary>
        /// AliceScriptのコードを実行します
        /// </summary>
        /// <param name="code">実行したいスクリプト</param>
        /// <param name="filename">スクリプトのファイル名</param>
        /// <param name="mainFile">メインファイルとして処理するか否か</param>
        /// <returns>スクリプトから返される戻り値</returns>
        public static Variable Execute(string code,string filename="",bool mainFile=false)
        {
           return Interpreter.Instance.Process(code,filename,mainFile);
        }
        /// <summary>
        /// AliceScriptファイルを実行します
        /// </summary>
        /// <param name="filename">スクリプトのファイル名</param>
        /// <param name="mainFile">メインファイルとして処理するか否か</param>
        /// <returns>スクリプトから返される戻り値</returns>
        public static Variable ExecuteFile(string filename,bool mainFile=false)
        {
            return Interpreter.Instance.ProcessFile(filename,mainFile);
        }
        /// <summary>
        /// AliceScriptのコードを非同期で実行します
        /// </summary>
        /// <param name="code">実行したいスクリプト</param>
        /// <param name="filename">スクリプトのファイル名</param>
        /// <param name="mainFile">メインファイルとして処理するか否か</param>
        /// <returns>スクリプトから返される戻り値</returns>
        public static Task<Variable> ExecuteAsync(string code,string filename="",bool mainFile = false)
        {
            return Interpreter.Instance.ProcessAsync(code,filename,mainFile);
        }
        /// <summary>
        /// AliceScriptファイルを非同期で実行します
        /// </summary>
        /// <param name="filename">スクリプトのファイル名</param>
        /// <param name="mainFile">メインファイルとして処理するか否か</param>
        /// <returns>スクリプトから返される戻り値</returns>
        public static Task<Variable> ExecuteFileAsync(string filename,bool mainFile = false)
        {
            return Interpreter.Instance.ProcessFileAsync(filename,mainFile);
        }
        /// <summary>
        /// プログラムが終了を求めているときに発生するイベントです
        /// </summary>
        public static event Exiting Exiting;
        /// <summary>
        /// Exitingイベントを発生させます
        /// </summary>
        /// <param name="exitcode">終了の理由を表す終了コード</param>
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
        /// <summary>
        /// Alice.Runtimeファイルの場所
        /// </summary>
        public static string Runtime_File_Path = Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),"Alice.Runtime.dll");
        /// <summary>
        /// AliceScriptのバージョン
        /// </summary>
        public static Version Version
        {
            get
            {
                //自分自身のAssemblyを取得
                System.Reflection.Assembly asm =
                    System.Reflection.Assembly.GetExecutingAssembly();
                //バージョンの取得
                return asm.GetName().Version;
                
            }
        }
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
