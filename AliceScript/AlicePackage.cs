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
        /// <summary>
        /// パッケージの発行者
        /// </summary>
        public string Publisher { get; set; }
        /// <summary>
        /// ターゲットのインタプリタ名
        /// </summary>
        public string TargetInterpreter { get; set; }

        internal ZipArchive archive { get; set; }

        private const string ConfigFileName = "manifest.xml";

        public static void Load(string path)
        {
            if (!File.Exists(path))
            {
                ThrowErrorManerger.OnThrowError("パッケージが見つかりません", Exceptions.FILE_NOT_FOUND);
                return;
            }
            LoadArchive(ZipFile.OpenRead(path),path);
        }
        public static void LoadData(byte[] data,string filename="")
        {
            LoadArchive(new ZipArchive(new MemoryStream(data)),filename);
        }
        private static void LoadArchive(ZipArchive a,string filename="")
        {
            try
            {
                if (a == null)
                {
                    ThrowErrorManerger.OnThrowError("パッケージを展開できません", Exceptions.BAD_PACKAGE);
                    return;
                }
                ZipArchiveEntry e = a.GetEntry(ConfigFileName);
                if (e == null)
                {
                    ThrowErrorManerger.OnThrowError("パッケージ設定ファイル:[manifest.xml]が見つかりません", Exceptions.BAD_PACKAGE);
                }
                else
                {
                    //見つかった時は開く
                    AlicePackage package = new AlicePackage();
                    package.archive = a;
                    string xml = GetEntryScript(e, ConfigFileName);
                    if (xml == null)
                    {
                        return;
                    }
                    XMLConfig config = new XMLConfig();
                    config.XMLText = xml;
                    if (config.Exists("name") && config.Exists("script"))
                    {
                        package.Name = config.Read("name");
                        package.Version = config.Read("version");
                        package.Description = config.Read("description");
                        package.Publisher = config.Read("publisher");
                        string supportinterpreter = config.Read("supportinterpreter");
                        if (supportinterpreter != "" && supportinterpreter != "any")
                        {
                            List<string> suppports = new List<string>(supportinterpreter.Split(','));
                            if (!suppports.Contains(Interpreter.Instance.Name))
                            {
                                ThrowErrorManerger.OnThrowError("そのパッケージをこのインタプリタで実行することはできません",Exceptions.NOT_COMPATIBLE_PACKAGES);
                                return;
                            }
                        }
                        string script = config.Read("script");
                        string srcname = ConfigFileName;
                        if (config.ExistsAttribute("script", "path"))
                        {
                            string entrypoint = config.ReadAttribute("script","path");
                            srcname = entrypoint;
                            ZipArchiveEntry entry = a.GetEntry(entrypoint);
                            if (entry == null)
                            {
                                ThrowErrorManerger.OnThrowError("エントリポイント:[" + entrypoint + "]が見つかりません", Exceptions.BAD_PACKAGE);
                                return;
                            }
                            else
                            {
                                script = GetEntryScript(entry, entrypoint);
                            }
                        }
                        Interpreter.Instance.Process(script, filename + "\\" +srcname, true, null, package);
                    }
                    else
                    {
                        ThrowErrorManerger.OnThrowError("パッケージ設定ファイルは必要な項目が記述されていません", Exceptions.BAD_PACKAGE);
                        return;
                    }
                    
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
            return (archive.GetEntry(filename) != null);
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
        internal static string GetEntryScript(ZipArchiveEntry e, string filename)
        {
            try
            {
                if (e == null)
                {
                    ThrowErrorManerger.OnThrowError("パッケージ内のファイル[" + filename + "]が見つかりません", Exceptions.FILE_NOT_FOUND);
                    return null;
                }
                string temp = Path.GetTempFileName();
                WriteStreamToExitingFile(temp, e.Open());
                return SafeReader.ReadAllText(temp, out _);
            }
            catch (Exception ex)
            {
                ThrowErrorManerger.OnThrowError("パッケージ内のファイル[" + filename + "]を読み込めません。詳細:" + ex.Message, Exceptions.BAD_PACKAGE);
                return null;
            }
        }
        private static void WriteStreamToExitingFile(string filename, Stream stream)
        {
            using (FileStream fs = new FileStream(filename, FileMode.Open))
            {
                stream.CopyTo(fs);
            }
        }
        private static byte[] GetDataFromStream(Stream stream)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return ms.GetBuffer();
            }
        }

    }
   
}
