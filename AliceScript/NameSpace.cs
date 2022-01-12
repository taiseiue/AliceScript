using System;
using System.Collections.Generic;
using System.Text;

namespace AliceScript
{
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
        public List<ObjectClass> Classes = new List<ObjectClass>();
        public Dictionary<string, string> Enums = new Dictionary<string, string>();
        public NameSpace Parent { get; set; }
        public string FullName
        {
            get
            {
                var parent = Parent;
                string name = Name;
                while (true)
                {
                    if (parent != null)
                    {
                        name = parent.Name +Constants.NAMESPACE_SPLITER+ name;
                        parent = parent.Parent;
                    }
                    else
                    {
                        break;
                    }
                }
                return name;
            }
        }
        public void Add(FunctionBase func)
        {
            Functions.Add(func);
        }
        public void Add(ObjectClass cls)
        {
            Classes.Add(cls);
        }
        public void Add(string name, string val)
        {
            Enums.Add(name, val);
        }
        public void Remove(FunctionBase func)
        {
            Functions.Remove(func);
        }
        public void Clear()
        {
            Functions.Clear();
        }
        public event EventHandler<ImportEventArgs> Loading;
        public event EventHandler<ImportEventArgs> UnLoading;
        public virtual void Load(ParsingScript script)
        {
            int ecount = 0;
            ImportEventArgs e = new ImportEventArgs();
            e.Cancel = false;
            e.Script = script;
            Loading?.Invoke(this, e);
            if (e.Cancel)
            {
                return;
            }
            foreach (FunctionBase func in Functions)
            {
                try
                {
                    FunctionBaseManerger.Add(func, func.Name, script);
                }
                catch { ecount++; }
            }
            foreach (ObjectClass cls in Classes)
            {
                try
                {
                    ClassManerger.Add(cls, script);
                }
                catch { ecount++; }
            }
            foreach (string s in Enums.Keys)
            {
                try
                {
                    FunctionBase.RegisterEnum(s, Enums[s], script);
                }
                catch { ecount++; }
            }

            if (ecount != 0) { throw new Exception("名前空間のロード中に" + ecount + "件の例外が発生しました。これらの例外は捕捉されませんでした"); }
        }
        public virtual void UnLoad(ParsingScript script)
        {
            int ecount = 0;
            ImportEventArgs e = new ImportEventArgs();
            e.Cancel = false;
            e.Script = script;
            UnLoading?.Invoke(this, e);
            if (e.Cancel)
            {
                return;
            }
            foreach (FunctionBase func in Functions)
            {
                try
                {
                    FunctionBaseManerger.Remove(func, func.Name, script);
                }
                catch { ecount++; }
            }
            foreach (ObjectClass cls in Classes)
            {
                try
                {
                    ClassManerger.Remove(cls, script);
                }
                catch { ecount++; }
            }
            foreach (string s in Enums.Keys)
            {
                try
                {
                    FunctionBase.UnregisterScriptFunction(s, script);
                }
                catch { ecount++; }
            }
            if (ecount != 0) { throw new Exception("名前空間のアンロード中に" + ecount + "件の例外が発生しました。これらの例外は捕捉されませんでした"); }
        }
        public int Count
        {
            get
            {
                return Functions.Count + Classes.Count;
            }
        }

    }
    public class UsingDelective : FunctionBase
    {
        public UsingDelective()
        {
            this.Name = "using";
            this.Attribute = FunctionAttribute.LANGUAGE_STRUCTURE | FunctionAttribute.FUNCT_WITH_SPACE_ONC;
            this.Run += UsingDelective_Run;
        }

        private void UsingDelective_Run(object sender, FunctionBaseEventArgs e)
        {
            string namespaceName = Utils.GetToken(e.Script, Constants.NEXT_OR_END_ARRAY);
        }
    }
}
