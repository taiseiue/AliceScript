
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Hnx8.ReadJEnc;

namespace alice
{
    //この機能には、hnx8様のReadJEncプロジェクトの成果物を使用しています
    static class PowerEncodeReader
    {
        public static string ReadAllText(string filename)
        {
			FileInfo file = new FileInfo(filename);
			if (!file.Exists) throw new FileNotFoundException();

			using (Hnx8.ReadJEnc.FileReader reader = new FileReader(file))
			{
				reader.Read(file);
				return reader.Text;
			}
		}
    }
}
