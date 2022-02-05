using System;
using System.Collections.Generic;
using System.IO;

namespace AliceScript
{
    public class ImportEventArgs : EventArgs
    {
        public bool Cancel { get; set; }
        public ParsingScript Script { get; set; }
    }

    internal class DllImportFunc : FunctionBase
    {
        public DllImportFunc()
        {
            this.FunctionName = "Dllimport";
            this.MinimumArgCounts = 1;
            this.Run += ImportFunc_Run;
        }

        private void ImportFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            string filename = e.Args[0].AsString();
            if (e.Script.Package != null && e.Script.Package.ExistsEntry(filename))
            {
                Interop.NetLibraryLoader.LoadLibrary(AlicePackage.GetEntryData(e.Script.Package.archive.GetEntry(filename), filename));
                return;
            }
            if (File.Exists(filename))
            {
                Interop.NetLibraryLoader.LoadLibrary(filename);
            }
            else
            {
                ThrowErrorManerger.OnThrowError("ファイルが見つかりません", Exceptions.FILE_NOT_FOUND, e.Script);
            }
        }
    }

    internal class IceImportFunc : FunctionBase
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
                AlicePackage.LoadData(AlicePackage.GetEntryData(e.Script.Package.archive.GetEntry(filename), filename));
                return;
            }
            AlicePackage.Load(filename);
        }
    }

    internal class ImportFunc : FunctionBase
    {
        public ImportFunc(bool isunimport = false)
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
                                //TODO:これ
                              //  NameSpaceManerger.UnLoad(file, e.Script);
                            }
                            else
                            {
                                ThrowErrorManerger.OnThrowError("該当する名前空間は読み込まれていません", Exceptions.NAMESPACE_NOT_LOADED, e.Script);
                            }
                        }
                        else
                        {
                           // NameSpaceManerger.Load(file, e.Script);
                        }
                        return;
                    }
                    else
                    {
                        ThrowErrorManerger.OnThrowError("該当する名前空間がありません", Exceptions.NAMESPACE_NOT_FOUND, e.Script);
                    }

                }
            }
        }
    }

}
