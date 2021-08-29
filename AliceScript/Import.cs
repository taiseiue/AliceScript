using System;
using System.Collections.Generic;
using System.IO;

namespace AliceScript
{
    class Import
    {

    }
    public static class NameSpaceManerger
    {
        public static Dictionary<string, NameSpace> NameSpaces = new Dictionary<string, NameSpace>();
        public static void Add(NameSpace space, string name = "")
        {
            if (name == "") { name = space.Name; }
            NameSpaces.Add(name, space);
        }
        public static bool Contains(NameSpace name)
        {
            return NameSpaces.ContainsValue(name);
        }
        public static bool Contains(string name)
        {
            return NameSpaces.ContainsKey(name);
        }
        public static void Load(string name)
        {
            NameSpaces[name].Load();
        }
        public static void UnLoad(string name)
        {
            NameSpaces[name].UnLoad();
        }
    }
    public class NameSpace
    {
        public NameSpace()
        {

        }
        public NameSpace(string name)
        {
            Name = name;
        }
        public string Name { get; set; }
        public List<FunctionBase> Functions = new List<FunctionBase>();
        public void Add(FunctionBase func)
        {
            Functions.Add(func);
        }
        public void Remove(FunctionBase func)
        {
            Functions.Remove(func);
        }
        public void Clear()
        {
            Functions.Clear();
        }
        public virtual void Load()
        {
            int ecount = 0;
            foreach (FunctionBase func in Functions)
            {
                try
                {
                    FunctionBaseManerger.Add(func);
                }
                catch { ecount++; }
            }
            if (ecount != 0) { throw new Exception("名前空間のロード中に" + ecount + "件の例外が発生しました。これらの例外は捕捉されませんでした"); }
        }
        public virtual void UnLoad()
        {
            int ecount = 0;
            foreach (FunctionBase func in Functions)
            {
                try
                {
                    FunctionBaseManerger.Remove(func);
                }
                catch { ecount++; }
            }
            if (ecount != 0) { throw new Exception("名前空間のアンロード中に" + ecount + "件の例外が発生しました。これらの例外は捕捉されませんでした"); }
        }
        public int Count
        {
            get
            {
                return Functions.Count;
            }
        }

    }
    class LibImportFunc : FunctionBase
    {
        public LibImportFunc()
        {
            this.FunctionName = "libimport";
            this.Attribute = FunctionAttribute.FUNCT_WITH_SPACE;
            this.MinimumArgCounts = 0;
            this.Run += ImportFunc_Run;
        }

        private void ImportFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args.Count > 0)
            {
                if (e.Args[0].Type == Variable.VarType.STRING)
                {
                    string file = e.Args[0].AsString();
                    if (!file.EndsWith(".alp") && !file.EndsWith(".dll"))
                    {
                        //拡張子がありません


                        if (File.Exists(Path.ChangeExtension(file, ".alp")))
                        { //alp形式で存在
                            file = Path.ChangeExtension(file, ".alp");
                        }
                        else
                            if (File.Exists(Path.ChangeExtension(file, ".dll")))
                        {
                            file = Path.ChangeExtension(file, ".dll");
                            //dll形式で存在
                        }
                        else
                        {
                            //いずれでもない場合
                            ThrowErrorManerger.OnThrowError("該当するライブラリが見つかりません",e.Script);
                        }
                    }
                    else if (File.Exists(file))
                    {
                        try
                        {
                            switch (Path.GetExtension(file))
                            {
                                case ".alp":
                                    {
                                        //alp形式で存在
                                        AlicePackage.LoadPackage(file);
                                        break;
                                    }
                                case ".dll":
                                    {
                                        //dll形式で存在
                                        AliceScript.Interop.NetLibraryLoader.LoadLibrary(file);
                                        break;
                                    }
                            }
                        }
                        catch
                        {
                            throw;
                        }
                    }
                }
            }
        }
    }
    class ImportFunc : FunctionBase
    {
        public ImportFunc()
        {
            this.FunctionName = "import";
            this.Attribute = FunctionAttribute.FUNCT_WITH_SPACE;
            this.MinimumArgCounts = 0;
            this.Run += ImportFunc_Run;
        }

        private void ImportFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args.Count > 0)
            {
                if (e.Args[0].Type == Variable.VarType.STRING)
                {
                    string file = e.Args[0].AsString();
                    if (NameSpaceManerger.Contains(file))
                    {
                        //NameSpace形式で存在
                        NameSpaceManerger.Load(file);
                        return;
                    }
                    else
                    {
                        ThrowErrorManerger.OnThrowError("該当する名前空間がありません",e.Script);
                    }

                }
            }
        }
    }
}
