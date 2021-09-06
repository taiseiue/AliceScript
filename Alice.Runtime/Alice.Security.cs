using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace AliceScript.NameSpaces
{
    static class Alice_Security_Initer
    {
        public static void Init()
        {
            NameSpace space = new NameSpace("Alice.Security");

            space.Add(new Password_Hash());
            space.Add(new Password_Salt());
            space.Add(new Password_Verify());

            NameSpaceManerger.Add(space);
        }
    }
    class PSS
    {
        public static int SALT_SIZE = 32;

        public static int HASH_SIZE = 32;

        public static int STRETCH_COUNT = 1000;
    }
    class Password_Hash : FunctionBase
    {

        public Password_Hash()
        {
            FunctionName = "password_hash";
            MinimumArgCounts = 2;
            Run += Class1_Run;
        }

        private void Class1_Run(object sender, FunctionBaseEventArgs e)
        {
            string password = e.Args[0].ToString();

            // ソルトを取得
            string salt = e.Args[1].AsString();

            // ハッシュ値を取得
            string hash = GetHash(password, salt, PSS.HASH_SIZE, PSS.STRETCH_COUNT);

            e.Return = new Variable(hash);
        }

        // ハッシュ値を取得
        private static string GetHash(string password, string salt, int size, int cnt)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(salt);
            byte[] bytesSalt;

            using (var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, bytes, cnt))
            {
                bytesSalt = rfc2898DeriveBytes.GetBytes(size);
            }

            string hash = Convert.ToBase64String(bytesSalt);



            return hash;
        }

        // ハッシュ値を比較

    }
    class Password_Salt : FunctionBase
    {
        public Password_Salt()
        {
            FunctionName = "password_salt";
            MinimumArgCounts = 0;
            Run += Class3_Run;
        }

        private void Class3_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(GetSalt(PSS.SALT_SIZE));
        }
        private static string GetSalt(int size)
        {
            var bytes = new byte[size];
            using (var rngCryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                rngCryptoServiceProvider.GetBytes(bytes);
            }

            string salt = Convert.ToBase64String(bytes);


            return salt;
        }
    }

    class Password_Verify : FunctionBase
    {

        public Password_Verify()
        {
            FunctionName = "password_verify";
            MinimumArgCounts = 3;
            Run += Class1_Run;
        }

        private void Class1_Run(object sender, FunctionBaseEventArgs e)
        {
            string password = e.Args[0].AsString();

            // ソルトを取得
            string salt = e.Args[2].AsString();

            // ハッシュ値を取得
            string hash = GetHash(password, salt, PSS.HASH_SIZE, PSS.STRETCH_COUNT);

            bool i = (e.Args[1].AsString() == hash);
            e.Return = new Variable(i);
        }

        // ハッシュ値を取得
        private static string GetHash(string password, string salt, int size, int cnt)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(salt);
            byte[] bytesSalt;

            using (var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, bytes, cnt))
            {
                bytesSalt = rfc2898DeriveBytes.GetBytes(size);
            }

            string hash = Convert.ToBase64String(bytesSalt);



            return hash;
        }

        // ハッシュ値を比較

    }
}
