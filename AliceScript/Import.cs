using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        public List<ObjectBase> Classes = new List<ObjectBase>();
        public Dictionary<string, string> Enums = new Dictionary<string, string>();
        public void Add(FunctionBase func)
        {
            Functions.Add(func);
        }
        public void Add(ObjectBase obj)
        {
            Classes.Add(obj);
        }
        public void Add(string name,string val)
        {
            Enums.Add(name,val);
        }
        public void Remove(FunctionBase func)
        {
            Functions.Remove(func);
        }
        public void Clear()
        {
            Functions.Clear();
        }
        public event EventHandler<CancelEventArgs> Loading;
        public event EventHandler<CancelEventArgs> UnLoading;
        public virtual void Load()
        {
            int ecount = 0;
            CancelEventArgs e = new CancelEventArgs();
            e.Cancel = false;
            Loading?.Invoke(this,e);
            if (e.Cancel)
            {
                return;
            }
            foreach (FunctionBase func in Functions)
            {
                try
                {
                    FunctionBaseManerger.Add(func);
                }
                catch { ecount++; }
            }
            foreach(ObjectBase obj in Classes)
            {
                try
                {
                    ClassManerger.Add(obj);
                }
                catch { ecount++; }
            }
            foreach(string s in Enums.Keys)
            {
                try
                {
                    FunctionBase.RegisterEnum(s,Enums[s]);
                }
                catch { ecount++; }
            }
            
            if (ecount != 0) { throw new Exception("名前空間のロード中に" + ecount + "件の例外が発生しました。これらの例外は捕捉されませんでした"); }
        }
        public virtual void UnLoad()
        {
            int ecount = 0;
            CancelEventArgs e = new CancelEventArgs();
            e.Cancel = false;
            UnLoading?.Invoke(this, e);
            if (e.Cancel)
            {
                return;
            }
            foreach (FunctionBase func in Functions)
            {
                try
                {
                    FunctionBaseManerger.Remove(func);
                }
                catch { ecount++; }
            }
            foreach (ObjectBase obj in Classes)
            {
                try
                {
                    ClassManerger.Remove(obj);
                }
                catch { ecount++; }
            }
            foreach(string s in Enums.Keys)
            {
                try
                {
                    FunctionBase.UnregisterFunction(s);
                }
                catch { ecount++; }
            }
            if (ecount != 0) { throw new Exception("名前空間のアンロード中に" + ecount + "件の例外が発生しました。これらの例外は捕捉されませんでした"); }
        }
        public int Count
        {
            get
            {
                return Functions.Count+Classes.Count;
            }
        }

    }
    class DllImportFunc : FunctionBase
    {
        public DllImportFunc()
        {
            this.FunctionName = "Dllimport";
            this.MinimumArgCounts =1;
            this.Run += ImportFunc_Run;
        }

        private void ImportFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            string filename = e.Args[0].AsString();
            if (e.Script.Package != null&&e.Script.Package.ExistsEntry(filename))
            {
                Interop.NetLibraryLoader.LoadLibrary(AlicePackage.GetEntryData(e.Script.Package.archive.GetEntry(filename),filename));
                return;
            }
            if (File.Exists(filename))
            {
                Interop.NetLibraryLoader.LoadLibrary(filename);
            }
            else
            {
                ThrowErrorManerger.OnThrowError("ファイルが見つかりません",Exceptions.FILE_NOT_FOUND,e.Script);
            }
        }
    }
    class IceImportFunc : FunctionBase
    {
        public IceImportFunc()
        {
            this.FunctionName = "Iceimport";
            this.MinimumArgCounts = 1;
            this.Run += IceImportFunc_Run;
        }

        private void IceImportFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            string filename = e.Args[0].AsString();
            if (e.Script.Package != null && e.Script.Package.ExistsEntry(filename))
            {
                AlicePackage.LoadData(AlicePackage.GetEntryData(e.Script.Package.archive.GetEntry(filename),filename));
                return;
            }
            AlicePackage.Load(filename);
        }
    }
    class ImportFunc : FunctionBase
    {
        public ImportFunc(bool isunimport=false)
        {
            if (isunimport)
            {
                this.FunctionName = "unimport";
                unimport = true;
            }
            else
            {
                this.FunctionName = "import";
            }
            this.Attribute = FunctionAttribute.FUNCT_WITH_SPACE;
            this.MinimumArgCounts = 0;
            this.Run += ImportFunc_Run;
        }
        private bool unimport = false;
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
                        if (unimport)
                        {
                            if (NameSpaceManerger.NameSpaces.ContainsKey(file))
                            {
                                NameSpaceManerger.UnLoad(file);
                            }
                            else {
                                ThrowErrorManerger.OnThrowError("該当する名前空間は読み込まれていません",Exceptions.NAMESPACE_NOT_LOADED,e.Script);
                            }
                        }
                        else
                        {
                            NameSpaceManerger.Load(file);
                        }
                        return;
                    }
                    else
                    {
                        ThrowErrorManerger.OnThrowError("該当する名前空間がありません",Exceptions.NAMESPACE_NOT_FOUND,e.Script);
                    }

                }
            }
        }
    }
 
}
