using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace AliceScript
{
    public class AlicePackage
    {

        internal ZipArchive archive { get; set; }

        public PackageManifest Manifest { get; set; }

        public static void Load(string path)
        {
            if (!File.Exists(path))
            {
                ThrowErrorManerger.OnThrowError("パッケージが見つかりません", Exceptions.FILE_NOT_FOUND);
                return;
            }
            byte[] file = File.ReadAllBytes(path);
            LoadData(file, path);
        }
        public static void LoadData(byte[] data, string filename = "")
        {
            byte[] magic = data.Take(Constants.PACKAGE_MAGIC_NUMBER.Length).ToArray();
            if (magic.SequenceEqual(Constants.PACKAGE_MAGIC_NUMBER))
            {
                LoadEncodingPackage(data, filename);
            }
            else
            {
                LoadArchive(new ZipArchive(new MemoryStream(data)), filename);
            }
        }
        public static PackageManifest GetManifest(string xml)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(xml))
                {
                    return null;
                }
                XMLConfig config = new XMLConfig();
                config.XMLText = xml;
                if (!config.Exists("name") && !config.Exists("script"))
                {
                    return null;
                }
                PackageManifest manifest = new PackageManifest();
                manifest.Name = config.Read("name");
                manifest.Version = config.Read("version");
                manifest.Description = config.Read("description");
                manifest.Publisher = config.Read("publisher");

                string sip = config.Read("target");
                if (!string.IsNullOrEmpty(sip) && sip.ToLower() != "any")
                {
                    manifest.Target = new List<string>(sip.Split(','));
                }
                else
                {
                    manifest.Target = null;
                }
                string script = config.Read("script");
                string path = config.ReadAttribute("script", "path");
                if (string.IsNullOrEmpty(script) && !string.IsNullOrEmpty(path))
                {
                    //リダイレクト
                    manifest.ScriptPath = path;
                    manifest.UseInlineScript = false;
                }
                else
                {
                    //インライン
                    manifest.Script = script;
                    manifest.ScriptPath = path;
                    manifest.UseInlineScript = true;
                }
                return manifest;
            }
            catch
            {
                return null;
            }
        }
        private static void LoadArchive(ZipArchive a, string filename = "")
        {
            try
            {
                if (a == null)
                {
                    ThrowErrorManerger.OnThrowError("パッケージを展開できません", Exceptions.BAD_PACKAGE);
                    return;
                }
                ZipArchiveEntry e = a.GetEntry(Constants.PACKAGE_MANIFEST_FILENAME);
                if (e == null)
                {
                    ThrowErrorManerger.OnThrowError("パッケージ設定ファイル:[manifest.xml]が見つかりません", Exceptions.BAD_PACKAGE);
                }
                else
                {
                    //見つかった時は開く
                    AlicePackage package = new AlicePackage();
                    package.archive = a;
                    string xml = GetEntryScript(e, Constants.PACKAGE_MANIFEST_FILENAME);
                    if (xml == null)
                    {
                        return;
                    }
                    package.Manifest = GetManifest(xml);
                    if (package.Manifest != null)
                    {
                        if (package.Manifest.Target != null)
                        {
                            if (!package.Manifest.Target.Contains(Interpreter.Instance.Name))
                            {
                                ThrowErrorManerger.OnThrowError("そのパッケージをこのインタプリタで実行することはできません", Exceptions.NOT_COMPATIBLE_PACKAGES);
                                return;
                            }
                        }
                        string srcname = string.IsNullOrEmpty(package.Manifest.ScriptPath) ? Constants.PACKAGE_MANIFEST_FILENAME : package.Manifest.ScriptPath;
                        if (!package.Manifest.UseInlineScript)
                        {
                            ZipArchiveEntry entry = a.GetEntry(srcname);
                            if (entry == null)
                            {
                                ThrowErrorManerger.OnThrowError("エントリポイント:[" + srcname + "]が見つかりません", Exceptions.BAD_PACKAGE);
                                return;
                            }
                            else
                            {
                                package.Manifest.Script = GetEntryScript(entry, srcname);
                            }
                        }
                        Interpreter.Instance.Process(package.Manifest.Script, filename + "\\" + srcname, true, null, package);
                    }
                    else
                    {
                        ThrowErrorManerger.OnThrowError("パッケージマニフェストファイルが不正です", Exceptions.BAD_PACKAGE);
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
        public static void CreateEncodingPackage(string filepath, string outfilepath, byte[] controlCode = null,string pfxFile=null,string pfxPassword=null)
        {
            int i, len;
            byte[] buffer = new byte[4096];
            byte[] data = File.ReadAllBytes(filepath);
            /// PKGFlagは次の形式です(各1ビット)
            /// 番地|使用法
            ///    0|このパッケージが署名されているか
            ///    1|未使用
            ///    2|未使用
            ///    3|未使用
            ///    4|未使用
            ///    5|未使用
            ///    6|未使用
            ///    7|未使用

            BitArray pkgflag = new BitArray(8);
            byte[] optinal = new byte[3];
            if (pfxFile != null)
            {
                pkgflag[0] = true;
            }
            if (controlCode == null)
            {
                controlCode = new byte[16];
            }
            else if (controlCode.Length != 16)
            {
                //制御コードが16バイトに満たない場合は0で埋め、それより大きい場合は切り詰めます
                byte[] newCode=new byte[16];
                for (i = 0; i < newCode.Length; i++)
                {
                    if (i < controlCode.Length)
                    {
                        newCode[i] = controlCode[i];
                    }
                    else
                    {
                        newCode[i] = 0x00;
                    }
                }
                controlCode = newCode;
            }

            using (FileStream outfs = new FileStream(outfilepath, FileMode.Create, FileAccess.Write))
            {
                using (AesManaged aes = new AesManaged())
                {
                    aes.BlockSize = 128;              // BlockSize = 16bytes
                    aes.KeySize = 128;                // KeySize = 16bytes
                    aes.Mode = CipherMode.CBC;        // CBC mode
                    aes.Padding = PaddingMode.PKCS7;    // Padding mode is "PKCS7".

                    // KeyとIV ( Initilization Vector ) は、AesManagedにつくらせる
                    aes.GenerateKey();
                    aes.GenerateIV();

                    //Encryption interface.
                    ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                    using (CryptoStream cse = new CryptoStream(outfs, encryptor, CryptoStreamMode.Write))
                    {
                        outfs.Write(Constants.PACKAGE_MAGIC_NUMBER, 0, Constants.PACKAGE_MAGIC_NUMBER.Length);     // ファイルヘッダを先頭に埋め込む
                        outfs.Write(BitsToBytes(pkgflag),0,1);//パッケージフラグを埋め込む
                        outfs.Write(optinal,0,3);//予備領域を埋め込む
                        outfs.Write(controlCode,0,16);//制御コードをファイルに埋め込む
                        outfs.Write(aes.Key, 0, 16); //次にKeyをファイルに埋め込む
                        outfs.Write(aes.IV, 0, 16); // 続けてIVもファイルに埋め込む
                        using (DeflateStream ds = new DeflateStream(cse, CompressionMode.Compress)) //圧縮
                        {
                            double size = data.LongLength;
                            byte[] sum = BitConverter.GetBytes(size);
                            ds.Write(sum, 0, 8);//解凍後の実際の長さを書き込む(これを用いて解凍をチェックする)
                            if (pkgflag[0])
                            {
                                //パッケージを署名する場合
                                byte[] signature = Sign(data,out byte[] publicKey,pfxFile,pfxPassword);
                                byte[] publen = BitConverter.GetBytes(signature.Length);
                                ds.Write(publen,0,4);//まず署名長さを書き込む
                                publen = BitConverter.GetBytes(publicKey.Length);
                                ds.Write(publen, 0, 4);//公開鍵長さを書き込む
                                ds.Write(signature,0,signature.Length);//署名を書き込む
                                ds.Write(publicKey,0,publicKey.Length);//公開鍵を書き込む
                            }
                            using (MemoryStream fs = new MemoryStream(data))
                            {
                                while ((len = fs.Read(buffer, 0, 4096)) > 0)
                                {
                                    ds.Write(buffer, 0, len);
                                }
                            }
                        }

                    }

                }
            }
        }
        /// <summary>
        /// PXFファイルを使ってデータに署名します。同時にPFXファイルから公開鍵のみを取り出します。
        /// </summary>
        /// <param name="byteData">署名したいデータ</param>
        /// <param name="pfxfile">PFXファイル</param>
        /// <param name="password">PXFファイルのパスワード</param>
        /// <param name="publicKey">公開鍵</param>
        /// <returns>署名</returns>
        private static byte[] Sign(byte[] byteData, out byte[] publicKey, string pfxfile,string password=null)
        {
            //pfxファイルを読み込み
            X509Certificate2 cert;
            if (password == null)
            {
                cert=new X509Certificate2(pfxfile);
            }
            else
            {
                cert = new X509Certificate2(pfxfile, password);
            }
            //証明書検証
            //秘密鍵の取り出し
            var rsa = (RSA)cert.PrivateKey;
            //公開鍵の取り出し
            var pub = (RSA)cert.PublicKey.Key;
            // 署名実行
            var signature = rsa.SignData(byteData,0,byteData.Length,HashAlgorithmName.SHA256,RSASignaturePadding.Pkcs1);
            publicKey=pub.ExportRSAPublicKey();
            return signature;
        }
        /// <summary>
        /// 指定されたデータの署名を検証します。
        /// </summary>
        /// <param name="byteData">署名を検証したいデータ</param>
        /// <param name="signature">署名</param>
        /// <param name="publicKey">公開鍵</param>
        /// <returns>検証が承認(Accept)された場合はTrue、棄却(Reject)された場合はFalse</returns>
        private static bool Vertify(byte[] byteData,byte[] signature,byte[] publicKey)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.ImportRSAPublicKey(publicKey,out _);
            return rsa.VerifyData(byteData, 0, byteData.Length, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
        private static byte[] BitsToBytes(BitArray bits)
        {
                if (bits.Count != 8)
                {
                    throw new ArgumentException("bits");
                }
                byte[] bytes = new byte[1];
                bits.CopyTo(bytes, 0);
            return bytes;
        }
        
        public static void LoadEncodingPackage(byte[] data, string filename = "")
        {
            int len;
            byte[] buffer = new byte[4096];

            using (MemoryStream outfs = new MemoryStream())
            {

                using (MemoryStream fs = new MemoryStream(data))
                {
                    using (AesManaged aes = new AesManaged())
                    {
                        aes.BlockSize = 128;              // BlockSize = 16bytes
                        aes.KeySize = 128;                // KeySize = 16bytes
                        aes.Mode = CipherMode.CBC;        // CBC mode
                        aes.Padding = PaddingMode.PKCS7;    // Padding mode is "PKCS7".

                        int ml = Constants.PACKAGE_MAGIC_NUMBER.Length;
                        byte[] mark = new byte[ml];
                        fs.Read(mark, 0, ml);
                        if (!mark.SequenceEqual(Constants.PACKAGE_MAGIC_NUMBER))
                        {
                            ThrowErrorManerger.OnThrowError("エラー:有効なAlicePackageファイルではありません", Exceptions.BAD_PACKAGE);
                            return;
                        }
                        byte[] f = new byte[1];
                        fs.Read(f,0,1);
                        BitArray pkgflag = new BitArray(f);//パッケージフラグ
                        fs.Seek(3,SeekOrigin.Current);//予備領域分シーク
                        fs.Seek(16,SeekOrigin.Current);//制御コード分シーク
                        // Key
                        byte[] key = new byte[16];
                        fs.Read(key, 0, 16);
                        aes.Key = key;
                        // Initilization Vector
                        byte[] iv = new byte[16];
                        fs.Read(iv, 0, 16);
                        aes.IV = iv;
                        //Decryption interface.
                        ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                        using (CryptoStream cse = new CryptoStream(fs, decryptor, CryptoStreamMode.Read))
                        {
                            using (DeflateStream ds = new DeflateStream(cse, CompressionMode.Decompress))   //解凍
                            {
                                byte[] sum = new byte[8];
                                try
                                {
                                    ds.Read(sum, 0, 8);
                                }
                                catch
                                {
                                    ThrowErrorManerger.OnThrowError("エラー:AlicePackageが壊れています", Exceptions.BAD_PACKAGE);
                                    return;
                                }
                                double size = BitConverter.ToDouble(sum);
                                int sl = 0;
                                int l = 0;
                                byte[] publicKey=null;
                                byte[] signature = null;
                                if (pkgflag[0])
                                {
                                    //署名済みパッケージ
                                    byte[] ln = new byte[4];
                                    ds.Read(ln, 0, 4);
                                    sl = BitConverter.ToInt32(ln);
                                    ds.Read(ln,0,4);
                                    l = BitConverter.ToInt32(ln);
                                    signature = new byte[sl];
                                    ds.Read(signature,0,sl);
                                    publicKey= new byte[l];
                                    ds.Read(publicKey,0,l);
                                }
                                while ((len = ds.Read(buffer, 0, 4096)) > 0)
                                {
                                    outfs.Write(buffer, 0, len);
                                }
                                if (outfs.Length != size)
                                {
                                    ThrowErrorManerger.OnThrowError("エラー:AlicePackageが壊れています", Exceptions.BAD_PACKAGE);
                                    return;
                                }
                                if (pkgflag[0] && !Vertify(outfs.GetBuffer(), signature, publicKey))
                                {
                                    ThrowErrorManerger.OnThrowError("エラー:署名の検証に失敗しました",Exceptions.BAD_PACKAGE);
                                    outfs.Dispose();
                                    return;
                                }
                            }
                        }
                    }
                }
                try
                {
                    LoadArchive(new ZipArchive(outfs), filename);
                }
                catch
                {
                    ThrowErrorManerger.OnThrowError("エラー:AlicePackageが壊れています", Exceptions.BAD_PACKAGE);
                    return;
                }
            }
        }
    }
    public class PackageManifest
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
        /// ターゲット
        /// </summary>
        public List<string> Target { get; set; }
        /// <summary>
        /// インラインスクリプトの場合。それ以外の場合はnull。
        /// </summary>
        public string Script { get; set; }
        /// <summary>
        /// スクリプトファイルのパス
        /// </summary>
        public string ScriptPath { get; set; }
        /// <summary>
        /// インラインスクリプトを使用するかどうか
        /// </summary>
        public bool UseInlineScript { get; set; }
    }

}
