using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AliceScript
{

    public static class ClassManerger
    {
        public static void Add(ObjectClass cls, ParsingScript script = null)
        {
            if (script == null)
            {
                Classes.Add(cls.Name, cls);
                TypeClass tc = new TypeClass();
                TypeObject to = new TypeObject();
                to.InterfaceBase = cls;
                to.Init(tc,null);
                Constants.CONSTS.Add(cls.Name,new Variable(to));
            }
            else
            {
                script.Classes.Add(cls.Name, cls);

                TypeClass tc = new TypeClass();
                TypeObject to = new TypeObject();
                to.InterfaceBase = cls;
                to.Init(tc, null);
                Constants.CONSTS.Add(cls.Name, new Variable(to));
            }
        }
        public static void Add(InterfaceBase ib,ParsingScript script=null)
        {
            if (script == null)
            {
                Interfaces.Add(ib.Name, ib);

                TypeClass tc = new TypeClass();
                TypeObject to = new TypeObject();
                to.InterfaceBase = ib;
                to.Init(tc, null);
                Constants.CONSTS.Add(ib.Name, new Variable(to));
            }
            else
            {
                Interfaces.Add(ib.Name, ib);

                TypeClass tc = new TypeClass();
                TypeObject to = new TypeObject();
                to.InterfaceBase = ib;
                to.Init(tc, null);
                Constants.CONSTS.Add(ib.Name, new Variable(to));
            }
        }
        public static void Remove(InterfaceBase ib, ParsingScript script = null)
        {
            if (script == null)
            {
                Interfaces.Remove(ib.Name);
            }
            else
            {
                Interfaces.Remove(ib.Name);
            }
        }
        public static void Remove(ObjectClass cls, ParsingScript script=null)
        {
            if (script == null)
            {
                Classes.Remove(cls.Name);
            }
            else
            {
                script.Classes.Remove(cls.Name);
            }
        }
        public static Dictionary<string, InterfaceBase> Interfaces = new Dictionary<string, InterfaceBase>();
        public static Dictionary<string, ObjectClass> Classes = new Dictionary<string, ObjectClass>();
    }
    public class CustomClass : ObjectClass
    {
        public CustomClass() { }

        public CustomClass(string className)
        {
            Name = className;
            RegisterClass(className, this);
        }

        public CustomClass(string className, string[] baseClasses, ParsingScript script)
        {
            Name = className;
            RegisterClass(className, this);
            bool inheritanceFromClass = false;
            foreach (string baseClass in baseClasses)
            {
                var bc = GetClassOrInterface(baseClass, script);
                if (bc == null)
                {
                    throw new ArgumentException("継承元クラスである [" + baseClass + "] が存在しません");
                }
                foreach (var entry in bc.Properties)
                {
                    Properties.Add(entry);
                }
                foreach (var entry in bc.Functions)
                {
                    Functions.Add(entry);
                }
                if (bc is ObjectClass oc)
                {
                    if (inheritanceFromClass)
                    {
                        throw new ArgumentException("すでにこのクラスは継承しているため、新たにクラスを継承することはできません");
                    }
                    else
                    {
                        Parent = oc;
                        foreach (var entry in oc.m_properties)
                        {
                            m_properties[entry.Key] = entry.Value;
                        }
                        foreach (var entry in oc.m_functions)
                        {
                            m_functions[entry.Key] = entry.Value;
                        }
                        inheritanceFromClass = true;
                    }
                }
                else
                {
                    Implementations.Add(bc);
                }
            }
        }
        public override ObjectBase GetImplementation(List<Variable> args, ParsingScript script = null, string className = "")
        {
            ObjectBase parent = null;
            if (Parent != null)
            {
               parent = Parent.GetImplementation(args, script, Parent.Name);
            }
            return new ClassInstance(script.CurrentAssign, className, args, script, parent);
        }

        public void AddMethod(string name, string[] args, CustomFunction method)
        {
            if (name == Name)
            {
                Constructors[args.Length] = method;
                for (int i = 0; i < method.DefaultArgsCount && i < args.Length; i++)
                {
                    Constructors[args.Length - i - 1] = method;
                }
            }
            else
            {
                if (!Functions.Contains(name)) 
                { Functions.Add(name); }
                m_functions[name] = method;
            }
        }
        public void AddStaticMethod(string name,CustomFunction method)
        {
            Static_Functions[name]=method;
        }

        public void AddProperty(string name, PropertyBase property)
        {
            Properties.Add(name);
            m_properties[name] = property;
        }


        public ParsingScript ParentScript = null;
        public int ParentOffset = 0;


        public class ClassInstance : ObjectBase
        {
            public ClassInstance(string instanceName, string className, List<Variable> args,
                                 ParsingScript script = null, ObjectBase parent = null)
            {
                Name = instanceName;
                Parent = parent;
                m_cscsClass = CustomClass.GetClass(className, script);
                if (m_cscsClass == null)
                {
                    throw new ArgumentException("クラスインスタンス [" + className + "] が存在しません");
                }

                // すべてのプロパティをクラスからコピー
                foreach (var entry in m_cscsClass.Properties)
                {
                    if (m_cscsClass.TryGetProperty(entry, out PropertyBase prop))
                    {
                        SetProperty(entry,prop);
                    }
                }
                // すべてのメソッドもクラスからコピー
                foreach(var entry in m_cscsClass.Functions)
                {
                    if(m_cscsClass.TryGetFunction(entry,out FunctionBase func))
                    {
                        SetMethod(entry,func);
                    }
                }
                // 引数の個数に応じたコンストラクタを実行
                // ただし、コンストラクタがあるにもかかわらず引数の個数が異なる場合はエラー
                FunctionBase constructor = null;
                if (m_cscsClass.Constructors.TryGetValue(args.Count, out constructor))
                {
                    constructor.OnRun(args, script, this);
                }
                else if (m_cscsClass.Constructors.Count > 0)
                {
                    ThrowErrorManerger.OnThrowError("このオブジェクトに引数" + args.Count + "個を指定するコンストラクタは実装されていません", Exceptions.CONSTRUCTOR_NOT_IMPLEMENT, script);
                }
            }

            private ObjectClass m_cscsClass;

            public override string ToString()
            {
                FunctionBase customFunction = null;
                if (!m_cscsClass.TryGetFunction(Constants.PROP_TO_STRING.ToLower(),
                     out customFunction))
                {
                    return m_cscsClass.Name + "." + Name;
                }

                Variable result = customFunction.OnRun(null, null, this);
                return result.ToString();
            }

            public override Task<Variable> SetProperty(string name, Variable value)
            {
                name = name.ToLower();
                var prop = Properties[name];
                if (prop.NeedCallFromParent && Parent != null)
                {
                    prop.SetProperty(value, Parent);
                }
                else
                {
                    prop.SetProperty(value, this);
                }
                //m_propSet.Add(name);
                //m_propSetLower.Add(name.ToLower());
                return Task.FromResult(Variable.EmptyInstance);
            }
            public Task<Variable> SetProperty(string name, PropertyBase property)
            {
                name = name.ToLower();
                Properties[name] = property;
                //m_propSet.Add(name);
                //m_propSetLower.Add(name.ToLower());
                return Task.FromResult(Variable.EmptyInstance);
            }
            public Task<Variable> SetMethod(string name, FunctionBase func)
            {
                name = name.ToLower();
                Functions[name] = func;
                //m_propSet.Add(name);
                //m_propSetLower.Add(name.ToLower());
                return Task.FromResult(Variable.EmptyInstance);
            }

            public override async Task<Variable> GetProperty(string name, List<Variable> args = null, ParsingScript script = null)
            {
                name = name.ToLower();
                if (Properties.TryGetValue(name, out PropertyBase value))
                {
                    if (value.NeedCallFromParent && Parent != null)
                    {
                        return value.GetProperty(Parent);
                    }
                    else
                    {
                        return value.GetProperty(this);
                    }
                }

                if (!Functions.TryGetValue(name, out FunctionBase customFunction))
                {
                    return null;
                }
                if (args == null)
                {
                    return Variable.EmptyInstance;
                }

                foreach (var entry in m_cscsClass.Properties)
                {
                    if (m_cscsClass.TryGetProperty(entry, out PropertyBase prop))
                    {
                        args.Add(prop.GetProperty(this));
                    }
                }
                if (customFunction.NeedCallFromParent)
                {
                    return customFunction.OnRun(args, script, Parent);
                }
                else
                {
                    return customFunction.OnRun(args, script, this);
                }
            }
            public override List<string> GetProperties()
            {
                /*
                List<string> props = new List<string>(Properties.Keys);
                props.AddRange(m_cscsClass.Functions.Keys);

                return props;
                */
                return m_cscsClass.Properties;
            }
            public override bool PropertyExists(string name)
            {
                return Properties.ContainsKey(name.ToLower());
            }

            public override bool FunctionExists(string name)
            {
                if (!m_cscsClass.TryGetFunction(name, out FunctionBase customFunction))
                {
                    return false;
                }
                return true;
            }
        }
    }

    internal class TestClass : ObjectClass
    {
        public static void Init()
        {
            ClassManerger.Add(new ITestObject());
            ClassManerger.Add(new TestClass());
        }
        public TestClass()
        {
            this.Name = "TestClass";
            this.AddFunction(new TestFunc());
        }
        public override ObjectBase GetImplementation(List<Variable> args, ParsingScript script = null, string className = "")
        {
            var obj = new TestObject("Tag");
            obj.Init(this, args, script, className);
            return obj;
        }

        private class TestFunc : FunctionBase
        {
            public TestFunc()
            {
                this.Name = "Show";
                this.NeedCallFromParent = true;
                this.Run += TestFunc_Run;
            }

            private void TestFunc_Run(object sender, FunctionBaseEventArgs e)
            {
                var v = (TestObject)e.Instance;
                Interpreter.Instance.AppendOutput(v.Tag, true);
            }
        }

        private class TestObject : ObjectBase
        {
            public TestObject(string tag)
            {
                Tag = tag;
            }
            public string Tag = "";
        }
    }
    internal class ITestObject : InterfaceBase
    {
        public ITestObject()
        {
            this.Name = "ITest";
            this.AddFunction("Show");
        }
    }

}
