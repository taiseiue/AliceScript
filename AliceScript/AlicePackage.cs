using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;

namespace AliceScript
{
    public class AlicePackage
    {
        /// <summary>
        /// パッケージの名前
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// パッケージのバージョン
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// パッケージの説明
        /// </summary>
        public string Description { get; set; }

        internal ZipArchive archive { get; set; }

        public static void Load(string path)
        {
            if (!File.Exists(path))
            {
                ThrowErrorManerger.OnThrowError("パッケージが見つかりません", Exceptions.FILE_NOT_FOUND);
                return;
            }
            LoadArchive(ZipFile.OpenRead(path));
        }
        public static void LoadData(byte[] data)
        {
            LoadArchive(new ZipArchive(new MemoryStream(data)));
        }
        private static void LoadArchive(ZipArchive a)
        {
            try
            {
                if (a == null)
                {
                    ThrowErrorManerger.OnThrowError("パッケージを展開できません", Exceptions.BAD_PACKAGE);
                    return;
                }
                ZipArchiveEntry e = a.GetEntry(@"main.alice");
                if (e == null)
                {
                    ThrowErrorManerger.OnThrowError("パッケージエントリポイント:[main.alice]が見つかりません", Exceptions.BAD_PACKAGE);
                }
                else
                {
                    //見つかった時は開く
                    AlicePackage package = new AlicePackage();
                    package.archive = a;
                    string script = GetEntryScript(e, "main.alice");
                    if (script == null)
                    {
                        return;
                    }
                    Interpreter.Instance.Process(script, "main.alice", true, null, package);
                }
            }
            catch (Exception ex)
            {
                ThrowErrorManerger.OnThrowError(ex.Message, Exceptions.BAD_PACKAGE);
            }
        }
        public Variable ExecuteEntry(string filename)
        {
            string script = GetEntryScript(archive.GetEntry(filename), filename);
            if (script == null)
            {
                return Variable.EmptyInstance;
            }
            return Interpreter.Instance.Process(script, "main.alice", true, null, this);
        }
        public bool ExistsEntry(string filename)
        {
            return (archive.GetEntry(filename) != null) ;
        }
        internal static byte[] GetEntryData(ZipArchiveEntry e, string filename)
        {
            try
            {
                if (e == null)
                {
                    ThrowErrorManerger.OnThrowError("パッケージ内のファイル[" + filename + "]が見つかりません", Exceptions.FILE_NOT_FOUND);
                    return null;
                }
                return GetDataFromStream(e.Open());
            }
            catch (Exception ex)
            {
                ThrowErrorManerger.OnThrowError("パッケージ内のファイル[" + filename + "]を読み込めません。詳細:" + ex.Message, Exceptions.BAD_PACKAGE);
                return null;
            }
        }
        internal static string GetEntryScript(ZipArchiveEntry e,string filename)
        {
            try
            {
                if (e == null)
                {
                    ThrowErrorManerger.OnThrowError("パッケージ内のファイル[" + filename + "]が見つかりません", Exceptions.FILE_NOT_FOUND);
                    return null;
                }
                string temp = Path.GetTempFileName();
                WriteStreamToExitingFile(temp,e.Open());
                return SafeReader.ReadAllText(temp, out _);
            }
            catch(Exception ex)
            {
                ThrowErrorManerger.OnThrowError("パッケージ内のファイル[" + filename + "]を読み込めません。詳細:"+ex.Message, Exceptions.BAD_PACKAGE);
                return null;
            }
        }
        private static void WriteStreamToExitingFile(string filename,Stream stream)
        {
            using (FileStream fs=new FileStream(filename, FileMode.Open))
            {
                stream.CopyTo(fs);
            }
        }
        private static byte[] GetDataFromStream(Stream stream)
        {
            using (MemoryStream ms=new MemoryStream())
            {
                stream.CopyTo(ms);
                return ms.GetBuffer();
            }
        }

    }
}
