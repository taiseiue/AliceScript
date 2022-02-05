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
        public List<ObjectClass> Classes = new List<ObjectClass>();
        public List<InterfaceBase> Interfaces = new List<InterfaceBase>();
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
        public void Add(InterfaceBase ib)
        {
            Interfaces.Add(ib);
        }
        public void Add(ObjectClass cls)
        {
            Classes.Add(cls);
        }
        public void Remove(InterfaceBase ib)
        {
            Interfaces.Remove(ib);
        }
       
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
        public static bool TryGetNameSpace(string name,out NameSpace space)
        {
            return NameSpaces.TryGetValue(name,out space);
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
            if (NameSpaceManerger.TryGetNameSpace(namespaceName,out NameSpace space))
            {
                e.Script.UsingNameSpaces.Add(space);
            }
            else
            {
                ThrowErrorManerger.OnThrowError("指定された名前空間が存在しません",Exceptions.NAMESPACE_NOT_FOUND,e.Script);
            }
        }
    }
}
