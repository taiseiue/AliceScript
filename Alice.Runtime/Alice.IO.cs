using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace AliceScript.NameSpaces
{
    class Alice_IO_Intiter
    {
        public static void Init()
        {
            NameSpace space = new NameSpace("Alice.IO");

            space.Add(new file_existsFunc());
            space.Add(new file_moveFunc());
            space.Add(new file_copyFunc());
            space.Add(new file_deleteFunc());
            space.Add(new file_encryptFunc());
            space.Add(new file_deleteFunc());
            space.Add(new file_read_dataFunc());
            space.Add(new file_read_textFunc());
            space.Add(new file_write_dataFunc());
            space.Add(new file_write_textFunc());
            space.Add(new file_append_textFunc());

            
            space.Add(new directory_moveFunc());
            space.Add(new directory_deleteFunc());
            space.Add(new directory_existsFunc());
            space.Add(new directory_createFunc());
            space.Add(new directory_getfilesFunc());
            space.Add(new directory_getdirectoriesFunc());
            space.Add(new directory_currentdirectoryFunc());

            NameSpaceManerger.Add(space);
        }
    }
    class file_read_textFunc : FunctionBase
    {
        public file_read_textFunc()
        {
            this.Name = "file_read_text";
            this.MinimumArgCounts = 1;
            this.Run += File_read_textFunc_Run;
        }

        private void File_read_textFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args.Count < 2)
            {
                e.Return = new Variable(SafeReader.ReadAllText(e.Args[0].AsString()));
            }
            else
            {
                if (e.Args[1].Type == Variable.VarType.STRING)
                {
                    e.Return = new Variable(File.ReadAllText(e.Args[0].AsString(), Encoding.GetEncoding(e.Args[1].AsString())));
                }
                else if(e.Args[1].Type==Variable.VarType.NUMBER)
                {
                    e.Return = new Variable(File.ReadAllText(e.Args[0].AsString(), Encoding.GetEncoding(e.Args[1].AsInt())));
                }
            }
        }
    }
    class file_read_dataFunc : FunctionBase
    {
        public file_read_dataFunc()
        {
            this.Name = "file_read_data";
            this.MinimumArgCounts = 1;
            this.Run += File_read_textFunc_Run;
        }

        private void File_read_textFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(File.ReadAllBytes(e.Args[0].AsString()));
        }
    }
  
    class file_write_textFunc : FunctionBase
    {
        public file_write_textFunc()
        {
            this.Name = "file_write_text";
            this.MinimumArgCounts = 2;
            this.Run += File_write_textFunc_Run;
        }

        private void File_write_textFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args.Count < 3)
            {
                File.WriteAllText(e.Args[0].AsString(), e.Args[1].AsString());
            }
            else
            {
                if (e.Args[1].Type == Variable.VarType.STRING)
                {
                    File.WriteAllText(e.Args[0].AsString(), e.Args[1].AsString(), Encoding.GetEncoding(e.Args[2].AsString()));
                }else if (e.Args[1].Type == Variable.VarType.NUMBER)
                {
                    File.WriteAllText(e.Args[0].AsString(), e.Args[1].AsString(), Encoding.GetEncoding(e.Args[2].AsInt()));
                }
            }
        }
    }
    class file_append_textFunc : FunctionBase
    {
        public file_append_textFunc()
        {
            this.Name = "file_append_text";
            this.MinimumArgCounts = 2;
            this.Run += File_write_textFunc_Run;
        }

        private void File_write_textFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args.Count < 3)
            {
                File.AppendAllText(e.Args[0].AsString(), e.Args[1].AsString());
            }
            else
            {
                if (e.Args[1].Type == Variable.VarType.STRING)
                {
                    File.AppendAllText(e.Args[0].AsString(), e.Args[1].AsString(), Encoding.GetEncoding(e.Args[2].AsString()));
                }
                else if (e.Args[1].Type == Variable.VarType.NUMBER)
                {
                    File.AppendAllText(e.Args[0].AsString(), e.Args[1].AsString(), Encoding.GetEncoding(e.Args[2].AsInt()));
                }
            }
        }
    }
    class file_write_dataFunc : FunctionBase
    {
        public file_write_dataFunc()
        {
            this.Name = "file_write_data";
            this.MinimumArgCounts = 2;
            this.Run += File_write_textFunc_Run;
        }

        private void File_write_textFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            File.WriteAllBytes(e.Args[0].AsString(), e.Args[1].AsByteArray());
        }
    }
    class file_copyFunc : FunctionBase
    {
        public file_copyFunc()
        {
            this.Name = "file_copy";
            this.MinimumArgCounts = 2;
            this.Run += File_copyFunc_Run;
        }

        private void File_copyFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args.Count < 3)
            {
                File.Copy(e.Args[0].AsString(),e.Args[1].AsString());
            }
            else
            {
                File.Copy(e.Args[0].AsString(), e.Args[1].AsString(),e.Args[2].AsBool());
            }
        }
    }
    class file_moveFunc : FunctionBase
    {
        public file_moveFunc()
        {
            this.Name = "file_move";
            this.MinimumArgCounts = 2;
            this.Run += File_copyFunc_Run;
        }

        private void File_copyFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            
                File.Move(e.Args[0].AsString(), e.Args[1].AsString());
           
        }
    }
    class file_existsFunc : FunctionBase
    {
        public file_existsFunc()
        {
            this.Name = "file_exists";
            this.MinimumArgCounts = 1;
            this.Run += File_exists_Run;
        }

        private void File_exists_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(File.Exists(e.Args[0].AsString()));
        }
    }
    class file_deleteFunc : FunctionBase
    {
        public file_deleteFunc()
        {
            this.Name = "file_delete";
            this.MinimumArgCounts = 1;
            this.Run += File_copyFunc_Run;
        }

        private void File_copyFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            
                File.Delete(e.Args[0].AsString());
            
           
        }
    }
    class file_encryptFunc:FunctionBase
    {
        public file_encryptFunc()
        {
            this.Name = "file_encrypt";
            this.MinimumArgCounts = 2;
            this.Run += File_encrypt_Run;
        }

        private void File_encrypt_Run(object sender, FunctionBaseEventArgs e)
        {
            FileEncrypter.FileEncrypt(e.Args[0].AsString(),e.Args[1].AsString(),e.Args[2].AsString());
        }
    }
    class file_decrypt : FunctionBase
    {
        public file_decrypt()
        {
            this.Name = "file_decrypt";
            this.MinimumArgCounts = 2;
            this.Run += File_encrypt_Run;
        }

        private void File_encrypt_Run(object sender, FunctionBaseEventArgs e)
        {
            FileEncrypter.FileDecrypt(e.Args[0].AsString(), e.Args[1].AsString(), e.Args[2].AsString());
        }
    }
    internal static class FileEncrypter
    {
        internal static bool FileDecrypt(string FilePath,string OutFilePath, string Password)
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
    class directory_createFunc : FunctionBase
    {
        public directory_createFunc()
        {
            this.Name = "directory_create";
            this.MinimumArgCounts = 1;
            this.Run += Directory_create_Run;
        }

        private void Directory_create_Run(object sender, FunctionBaseEventArgs e)
        {
            Directory.CreateDirectory(e.Args[0].AsString());
        }
    }
    class directory_deleteFunc : FunctionBase
    {
        public directory_deleteFunc()
        {
            this.Name = "directory_delete";
            this.MinimumArgCounts = 1;
            this.Run += Directory_create_Run;
        }

        private void Directory_create_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args.Count > 1)
            {
                Directory.Delete(e.Args[0].AsString(),e.Args[1].AsBool());
            }
            else
            {
                Directory.Delete(e.Args[0].AsString());
            }
        }
    }
    class directory_moveFunc : FunctionBase
    {
        public directory_moveFunc()
        {
            this.Name = "directory_move";
            this.MinimumArgCounts = 2;
            this.Run += File_copyFunc_Run;
        }

        private void File_copyFunc_Run(object sender, FunctionBaseEventArgs e)
        {
           
                Directory.Move(e.Args[0].AsString(), e.Args[1].AsString());
           
        }
    }
    class directory_existsFunc : FunctionBase
    {
        public directory_existsFunc()
        {
            this.Name = "directory_exists";
            this.MinimumArgCounts = 1;
            this.Run += File_exists_Run;
        }

        private void File_exists_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Directory.Exists(e.Args[0].AsString()));
        }
    }
    class directory_currentdirectoryFunc : FunctionBase
    {
        public directory_currentdirectoryFunc()
        {
            this.Name = "directory_currentdirectory";
            this.MinimumArgCounts = 0;
            this.Run += File_exists_Run;
        }

        private void File_exists_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args.Count > 0)
            {
                Directory.SetCurrentDirectory(e.Args[0].AsString());
            }
            e.Return = new Variable(Directory.GetCurrentDirectory());
        }
    }
    class directory_getdirectoriesFunc : FunctionBase
    {
        public directory_getdirectoriesFunc()
        {
            this.Name = "directory_getdirectories";
            this.MinimumArgCounts = 1;
            this.Run += Directory_getdirectoriesFunc_Run;
        }

        private void Directory_getdirectoriesFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args.Count == 1)
            {
                Variable vb = new Variable(Variable.VarType.ARRAY_STR);
                foreach(string dn in Directory.GetDirectories(e.Args[0].AsString()))
                {
                    vb.Tuple.Add(new Variable(dn));
                }
                e.Return = vb;
            }else if (e.Args.Count == 2)
            {
                Variable vb = new Variable(Variable.VarType.ARRAY_STR);
                foreach (string dn in Directory.GetDirectories(e.Args[0].AsString(),e.Args[1].AsString()))
                {
                    vb.Tuple.Add(new Variable(dn));
                }
                e.Return = vb;
            }
            else if (e.Args.Count >= 3)
            {
                Variable vb = new Variable(Variable.VarType.ARRAY_STR);
                SearchOption so = SearchOption.TopDirectoryOnly;
                if (e.Args[2].AsBool())
                {
                    so = SearchOption.AllDirectories;
                }
                foreach (string dn in Directory.GetDirectories(e.Args[0].AsString(), e.Args[1].AsString(),so))
                {
                    vb.Tuple.Add(new Variable(dn));
                }
                e.Return = vb;
            }
        }
    }
    class directory_getfilesFunc : FunctionBase
    {
        public directory_getfilesFunc()
        {
            this.Name = "directory_getfiles";
            this.MinimumArgCounts = 1;
            this.Run += Directory_getdirectoriesFunc_Run;
        }

        private void Directory_getdirectoriesFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args.Count == 1)
            {
                Variable vb = new Variable(Variable.VarType.ARRAY_STR);
                foreach (string dn in Directory.GetFiles(e.Args[0].AsString()))
                {
                    vb.Tuple.Add(new Variable(dn));
                }
                e.Return = vb;
            }
            else if (e.Args.Count == 2)
            {
                Variable vb = new Variable(Variable.VarType.ARRAY_STR);
                foreach (string dn in Directory.GetFiles(e.Args[0].AsString(), e.Args[1].AsString()))
                {
                    vb.Tuple.Add(new Variable(dn));
                }
                e.Return = vb;
            }
            else if (e.Args.Count >= 3)
            {
                Variable vb = new Variable(Variable.VarType.ARRAY_STR);
                SearchOption so = SearchOption.TopDirectoryOnly;
                if (e.Args[2].AsBool())
                {
                    so = SearchOption.AllDirectories;
                }
                foreach (string dn in Directory.GetFiles(e.Args[0].AsString(), e.Args[1].AsString(), so))
                {
                    vb.Tuple.Add(new Variable(dn));
                }
                e.Return = vb;
            }
        }
    }
    class directory_getdirectoryrootFunc : FunctionBase
    {
        public directory_getdirectoryrootFunc()
        {
            this.Name = "directory_getdirectoryroot";
            this.MinimumArgCounts = 1;
            this.Run += File_exists_Run;
        }

        private void File_exists_Run(object sender, FunctionBaseEventArgs e)
        {
           
            e.Return = new Variable(Directory.GetDirectoryRoot(e.Args[0].AsString()));
        }
    }
}
