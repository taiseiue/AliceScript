using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;

namespace AliceScript
{
   static class AlicePackage
    {
        public static void LoadPackage(string path)
        {
            if (!File.Exists(path)) { throw new FileNotFoundException(); }

            ZipArchive m_zipArchive = new ZipArchive(FileDecrypt(path, "BjBx9rdRtm6U"));
            ZipArchiveEntry e = m_zipArchive.GetEntry(@"config.xml");
            if (e == null)
            {
                throw new Exception("ファイルが壊れています");
            }
            else
            {
                //見つかった時は開く
                using (StreamReader sr = new StreamReader(e.Open(),
                    System.Text.Encoding.UTF8))
                {
                    //すべて読み込む
                    string s = sr.ReadToEnd();
                    XMLConfig xml = new XMLConfig();
                    xml.XMLText = s;
                    if (xml.Exists("config/autoload/dll"))
                    {
                        string files = xml.Read("config/autoload/dll");
                        //[,]カンマ区切りで入っています
                        string[] vs = files.Split(',');
                        foreach(string v in vs)
                        {
                            if (!string.IsNullOrEmpty(v))
                            {
                                ZipArchiveEntry em = m_zipArchive.GetEntry(v);
                                if (em == null)
                                {
                                    throw new Exception("パッケージの設定で" + v + "を開くように要求されましたが、そのファイルが見つかりませんでした");
                                }
                                else
                                {
                                    MemoryStream ms = new MemoryStream();
                                    em.Open().CopyTo(ms);
                                    Interop.NetLibraryLoader.LoadLibrary(ms.ToArray());
                                }
                            }
                        }
                    }
                    if (xml.Exists("config/autoload/script"))
                    {
                        string files = xml.Read("config/autoload/script");
                        //[,]カンマ区切りで入っています
                        string[] vs = files.Split(',');
                        foreach (string v in vs)
                        {
                            if (!string.IsNullOrEmpty(v))
                            {
                                ZipArchiveEntry em = m_zipArchive.GetEntry(v);
                                if (em == null)
                                {
                                    throw new Exception("パッケージの設定で" + v + "を開くように要求されましたが、そのファイルが見つかりませんでした");
                                }
                                else
                                {
                                    using (StreamReader sr2 = new StreamReader(em.Open(),
                  System.Text.Encoding.UTF8))
                                    {
                                        //すべて読み込む
                                        string s2 = sr2.ReadToEnd();
                                        Alice.Execute(s2);
                                    }
                                    }
                            }
                        }
                    }
                }
            }
        }
        private static MemoryStream FileDecrypt(string FilePath, string Password)
        {
            int i, len;
            byte[] buffer = new byte[4096];

         

           

            using (MemoryStream outfs = new MemoryStream())
            {
                using (FileStream fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read))
                {
                    using (AesManaged aes = new AesManaged())
                    {
                        aes.BlockSize = 128;              // BlockSize = 16bytes
                        aes.KeySize = 128;                // KeySize = 16bytes
                        aes.Mode = CipherMode.CBC;        // CBC mode
                        aes.Padding = PaddingMode.PKCS7;    // Padding mode is "PKCS7".

                        // salt
                        byte[] salt = new byte[16];
                        fs.Read(salt, 0, 16);

                        // Initilization Vector
                        byte[] iv = new byte[16];
                        fs.Read(iv, 0, 16);
                        aes.IV = iv;

                        /*
                        // パスワード文字列が大きい場合は、切り詰め、16バイトに満たない場合は0で埋めます
                        byte[] bufferKey = new byte[16];
                        byte[] bufferPassword = Encoding.UTF8.GetBytes(Password);
                        for (i = 0; i < bufferKey.Length; i++)
                        {
                            if (i < bufferPassword.Length)
                            {
                                bufferKey[i] = bufferPassword[i];
                            }
                            else
                            {
                                bufferKey[i] = 0;
                            }
                        */

                        // ivをsaltにしてパスワードを擬似乱数に変換
                        Rfc2898DeriveBytes deriveBytes = new Rfc2898DeriveBytes(Password, salt);
                        byte[] bufferKey = deriveBytes.GetBytes(16);    // 16バイトのsaltを切り出してパスワードに変換
                        aes.Key = bufferKey;

                        //Decryption interface.
                        ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                        using (CryptoStream cse = new CryptoStream(fs, decryptor, CryptoStreamMode.Read))
                        {
                            using (DeflateStream ds = new DeflateStream(cse, CompressionMode.Decompress))   //解凍
                            {
                                while ((len = ds.Read(buffer, 0, 4096)) > 0)
                                {
                                    outfs.Write(buffer, 0, len);
                                }
                            }
                        }
                    }
                }
                return outfs;
            }

        }
     

        public static bool MakePackage(string ZipFilePath,string OutputFilePath)
        {
            //パッケージZIPの作り方
            //1.フォルダ作る
            //2.dllとかscriptとか配置
            //3.直下にconfig.xmlを作成
            //4.xmlにconfig/autoload/dllないしconfig/autoload/scriptを作成
            //5.4にカンマ区切りで読み込むパスを入力
            return FileEncrypter.FileEncrypt(ZipFilePath,OutputFilePath, "BjBx9rdRtm6U");
        }

    }
    internal static class FileEncrypter
    {
        internal static bool FileDecrypt(string FilePath, string OutFilePath, string Password)
        {
            int i, len;
            byte[] buffer = new byte[4096];



            using (FileStream outfs = new FileStream(OutFilePath, FileMode.Create, FileAccess.Write))
            {
                using (FileStream fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read))
                {
                    using (AesManaged aes = new AesManaged())
                    {
                        aes.BlockSize = 128;              // BlockSize = 16bytes
                        aes.KeySize = 128;                // KeySize = 16bytes
                        aes.Mode = CipherMode.CBC;        // CBC mode
                        aes.Padding = PaddingMode.PKCS7;    // Padding mode is "PKCS7".

                        // salt
                        byte[] salt = new byte[16];
                        fs.Read(salt, 0, 16);

                        // Initilization Vector
                        byte[] iv = new byte[16];
                        fs.Read(iv, 0, 16);
                        aes.IV = iv;

                        /*
                        // パスワード文字列が大きい場合は、切り詰め、16バイトに満たない場合は0で埋めます
                        byte[] bufferKey = new byte[16];
                        byte[] bufferPassword = Encoding.UTF8.GetBytes(Password);
                        for (i = 0; i < bufferKey.Length; i++)
                        {
                            if (i < bufferPassword.Length)
                            {
                                bufferKey[i] = bufferPassword[i];
                            }
                            else
                            {
                                bufferKey[i] = 0;
                            }
                        */

                        // ivをsaltにしてパスワードを擬似乱数に変換
                        Rfc2898DeriveBytes deriveBytes = new Rfc2898DeriveBytes(Password, salt);
                        byte[] bufferKey = deriveBytes.GetBytes(16);    // 16バイトのsaltを切り出してパスワードに変換
                        aes.Key = bufferKey;

                        //Decryption interface.
                        ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                        using (CryptoStream cse = new CryptoStream(fs, decryptor, CryptoStreamMode.Read))
                        {
                            using (DeflateStream ds = new DeflateStream(cse, CompressionMode.Decompress))   //解凍
                            {
                                while ((len = ds.Read(buffer, 0, 4096)) > 0)
                                {
                                    outfs.Write(buffer, 0, len);
                                }
                            }
                        }
                    }
                }
            }

            return (true);
        }

        internal static bool FileEncrypt(string FilePath, string OutFilePath, string Password)
        {

            int i, len;
            byte[] buffer = new byte[4096];



            using (FileStream outfs = new FileStream(OutFilePath, FileMode.Create, FileAccess.Write))
            {
                using (AesManaged aes = new AesManaged())
                {
                    aes.BlockSize = 128;              // BlockSize = 16bytes
                    aes.KeySize = 128;                // KeySize = 16bytes
                    aes.Mode = CipherMode.CBC;        // CBC mode
                    aes.Padding = PaddingMode.PKCS7;    // Padding mode is "PKCS7".

                    //入力されたパスワードをベースに擬似乱数を新たに生成
                    Rfc2898DeriveBytes deriveBytes = new Rfc2898DeriveBytes(Password, 16);
                    byte[] salt = new byte[16]; // Rfc2898DeriveBytesが内部生成したなソルトを取得
                    salt = deriveBytes.Salt;
                    // 生成した擬似乱数から16バイト切り出したデータをパスワードにする
                    byte[] bufferKey = deriveBytes.GetBytes(16);

                    /*
                    // パスワード文字列が大きい場合は、切り詰め、16バイトに満たない場合は0で埋めます
                    byte[] bufferKey = new byte[16];
                    byte[] bufferPassword = Encoding.UTF8.GetBytes(Password);
                    for (i = 0; i < bufferKey.Length; i++)
                    {
                        if (i < bufferPassword.Length)
                        {
                            bufferKey[i] = bufferPassword[i];
                        }
                        else
                        {
                            bufferKey[i] = 0;
                        }
                    */

                    aes.Key = bufferKey;
                    // IV ( Initilization Vector ) は、AesManagedにつくらせる
                    aes.GenerateIV();

                    //Encryption interface.
                    ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                    using (CryptoStream cse = new CryptoStream(outfs, encryptor, CryptoStreamMode.Write))
                    {
                        outfs.Write(salt, 0, 16);     // salt をファイル先頭に埋め込む
                        outfs.Write(aes.IV, 0, 16); // 次にIVもファイルに埋め込む
                        using (DeflateStream ds = new DeflateStream(cse, CompressionMode.Compress)) //圧縮
                        {
                            using (FileStream fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read))
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


            return (true);
        }
    }
}
