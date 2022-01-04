using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AliceScript
{
    public class ObjectBase : ScriptObject
    {
        /// <summary>
        /// このオブジェクトの名前
        /// ただし、すべて小文字に変換されます
        /// 規定値はobject
        /// </summary>
        public string Name
        {
            get
            {
                return m_name;
            }
            set
            {
                m_name = value.ToLower();
            }
        }
        /// <summary>
        /// newキーワードを使ってこのクラスのインスタンスを新規に作成することができるかを表す値
        /// </summary>
        public bool CanCreateInstance
        {
            get
            {
                return m_cancreateInstance;
            }
            set
            {
                m_cancreateInstance = value;
            }
        }
        /// <summary>
        /// このオブジェクトの継承元
        /// </summary>
        public ObjectBase Parent { get; set; }
        /// <summary>
        /// このオブジェクトの実装の定義元
        /// </summary>
        public InterfaceBase Interface { get; set; }
        private bool m_cancreateInstance = false;
        private string m_name="object";

        public Dictionary<string, PropertyBase> Properties = new Dictionary<string, PropertyBase>();
        public Dictionary<string, FunctionBase> Functions = new Dictionary<string, FunctionBase>();
        public Dictionary<int, FunctionBase> Constructors = new Dictionary<int, FunctionBase>();

        public ObjectBase(string name = "")
        {
            Name = name;
        }


        public virtual List<string> GetProperties()
        {
            List<string> v = new List<string>(Properties.Keys);
            v.AddRange(new List<string>(Functions.Keys));
            return v;
        }

        public virtual Variable Operator(Variable other, string action, ParsingScript script)
        {
            //継承先によって定義されます
            throw new NotImplementedException();
        }
        public virtual void Init(ObjectClass cls,List<Variable> args=null,ParsingScript script=null,string className="")
        {
            Name=cls.Name;
            if (cls.Parent != null)
            {
                Parent = cls.Parent.GetImplementation(args, script, className);
            }
            foreach (var entry in cls.Properties)
            {
                if(cls.TryGetProperty(entry,out PropertyBase prop))
                {
                    Properties[entry] = prop;
                }
            }
            foreach (var entry in cls.Functions)
            {
                if (cls.TryGetFunction(entry, out FunctionBase prop))
                {
                    Functions[entry] = prop;
                }
            }
            if(args!=null&&cls.Constructors.TryGetValue(args.Count,out FunctionBase func))
            {
                func.OnRun(args,script,this);
            }else if (cls.Constructors.Count > 0)
            {
                ThrowErrorManerger.OnThrowError("このクラスには、引数"+args.Count+"を受け取るコンストラクタは実装されていません",Exceptions.CONSTRUCTOR_NOT_IMPLEMENT);
            }
        }
        public virtual Task<Variable> GetProperty(string sPropertyName, List<Variable> args = null, ParsingScript script = null)
        {
            sPropertyName = Variable.GetActualPropertyName(sPropertyName, GetProperties()).ToLower();
            if (Properties.ContainsKey(sPropertyName))
            {
               return Task.FromResult(Properties[sPropertyName].GetProperty(this));
            }
            else
            {
                if (Functions.ContainsKey(sPropertyName))
                {
                    Task<Variable> va = Task.FromResult(Functions[sPropertyName].OnRun(args,script,this));
                    return va;

                }
                else
                {
                    ThrowErrorManerger.OnThrowError("指定されたプロパティまたはメソッドは存在しません。", Exceptions.PROPERTY_OR_METHOD_NOT_FOUND, script);
                    return Task.FromResult(Variable.EmptyInstance);
                }
            }
        }

        public virtual Task<Variable> SetProperty(string sPropertyName, Variable argValue)
        {
           
                sPropertyName = Variable.GetActualPropertyName(sPropertyName, GetProperties());
                if (Properties.ContainsKey(sPropertyName))
                {
                    Properties[sPropertyName].SetProperty(argValue,this);
                }
                else
                {
                    ThrowErrorManerger.OnThrowError("指定されたプロパティは存在しません",Exceptions.COULDNT_FIND_VARIABLE);
                }
            
            return Task.FromResult(Variable.EmptyInstance);
        }
    
        public List<KeyValuePair<string, Variable>> GetPropList()
        {
            List<KeyValuePair<string, Variable>> props = new List<KeyValuePair<string, Variable>>();
            foreach (var entry in Properties)
            {
                props.Add(new KeyValuePair<string, Variable>(entry.Key, entry.Value.GetProperty(this)));
            }
            return props;
        }
        public virtual bool PropertyExists(string name)
        {
            return Properties.ContainsKey(name.ToLower());
        }

        public virtual bool FunctionExists(string name)
        {
            if (!Functions.TryGetValue(name, out FunctionBase customFunction))
            {
                return false;
            }
            return true;
        }

     
    }
    public class InterfaceBase
    {
        /// <summary>
        /// インタフェースの名前
        /// </summary>
        public string Name
        {
            get
            {
                return m_name;
            }
            set
            {
                m_name = value.ToLower();
            }
        }
        private string m_name = "objcet";
        /// <summary>
        /// インタフェースの継承元
        /// </summary>
        public ObjectClass Parent { get; set; }
        /// <summary>
        /// インタフェースの所属する名前空間
        /// </summary>
        public string NameSpace { get; set; }
        /// <summary>
        /// インタフェースに含まれるプロパティ
        /// </summary>
        public List<string> Properties = new List<string>();
        /// <summary>
        /// インタフェースに含まれる関数
        /// </summary>
        public List<string> Functions = new List<string>();
        /// <summary>
        /// このインタフェースが実装するインタフェース
        /// </summary>
        public List<InterfaceBase> Implementations = new List<InterfaceBase>();
        public void AddProperty(string name)
        {
            this.Properties.Add(name);
        }
        public void RemoveProperty(string name)
        {
            this.Properties.Remove(name);
        }
        public void AddFunction(string name)
        {
            this.Functions.Add(name);
        }
        public virtual void RemoveFunction(string name)
        {
            this.Functions.Remove(name);
        }
        public virtual List<string> GetProperties()
        {
            List<string> v = new List<string>(Properties);
            v.AddRange(new List<string>(Functions));
            return v;
        }
        public static InterfaceBase GetInterface(string name, ParsingScript script = null)
        {
            string currNamespace = ParserFunction.GetCurrentNamespace;
            if (!string.IsNullOrWhiteSpace(currNamespace))
            {
                bool namespacePresent = name.Contains(".");
                if (!namespacePresent)
                {
                    name = currNamespace + "." + name;
                }
            }

            InterfaceBase theClass = null;
            if (script != null && script.TryGetInterface(name, out theClass))
            {
                return theClass;
            }
            if (ClassManerger.Interfaces.TryGetValue(name, out theClass))
            {
                return theClass;
            }
            return null;
        }

    }
    public class ObjectClass:InterfaceBase
    {
        
        public Dictionary<string, PropertyBase> m_properties = new Dictionary<string, PropertyBase>();
        public Dictionary<string, FunctionBase> m_functions = new Dictionary<string, FunctionBase>();
        public Dictionary<int, FunctionBase> Constructors = new Dictionary<int, FunctionBase>();
        public PropertyBase GetPropertyBase(string name)
        {
            PropertyBase v;
            m_properties.TryGetValue(name,out v);
            return v;
        }
        public FunctionBase GetFunctionBase(string name)
        {
            FunctionBase f;
            m_functions.TryGetValue(name,out f);
            return f;
        }
        public void AddProperty(PropertyBase property)
        {
            this.Properties.Add(property.Name);
            this.m_properties.Add(property.Name, property);
        }
        public void RemoveProperty(PropertyBase property)
        {
            this.Properties.Remove(property.Name);
            this.m_properties.Remove(property.Name);
        }
        public void AddFunction(FunctionBase function, string name = "")
        {
            if (string.IsNullOrEmpty(name)) { name = function.Name; }
            this.m_functions.Add(name,function);
            this.Functions.Add(name);
        }
        public override void RemoveFunction(string name)
        {
            this.m_functions.Remove(name);
            this.Functions.Remove(name);
        }
        public void RemoveFunction(FunctionBase function)
        {
            this.m_functions.Remove(function.Name);
            this.Functions.Remove(function.Name);
        }
        public bool TryGetProperty(string name,out PropertyBase property)
        {
            property = null;
            if (Properties.Contains(name)&&m_properties.TryGetValue(name,out property))
            {
                return true;
            }
            return false;
        }
        public bool TryGetFunction(string name,out FunctionBase function)
        {
            function = null;
            if(Functions.Contains(name)&&m_functions.TryGetValue(name,out function))
            {
                return true;
            }
            return false;
        }
        public virtual ObjectBase GetImplementation(List<Variable> args,ParsingScript script=null,string className="")
        {
            //継承先で実装されます
            throw new NotImplementedException();
        }
        public static ObjectClass GetClass(string name, ParsingScript script = null)
        {
            string currNamespace = ParserFunction.GetCurrentNamespace;
            if (!string.IsNullOrWhiteSpace(currNamespace))
            {
                bool namespacePresent = name.Contains(".");
                if (!namespacePresent)
                {
                    name = currNamespace + "." + name;
                }
            }

            ObjectClass theClass = null;
            if (s_allClasses.TryGetValue(name, out theClass))
            {
                return theClass;
            }
            if (script != null && script.TryGetClass(name, out theClass))
            {
                return theClass;
            }
            if (ClassManerger.Classes.TryGetValue(name, out theClass))
            {
                return theClass;
            }
            return null;
        }
        public static InterfaceBase GetClassOrInterface(string name, ParsingScript script = null)
        {
            var v = GetClass(name,script);
            if (v != null)
            {
                return v;
            }
            return GetInterface(name,script);
        }


        public static void RegisterClass(string className, ObjectClass obj)
        {
            obj.NameSpace = ParserFunction.GetCurrentNamespace;
            if (!string.IsNullOrWhiteSpace(obj.NameSpace))
            {
                className = obj.NameSpace + "." + className;
            }

            obj.Name = className;
            className = Constants.ConvertName(className);
            AllClasses[className] = obj;
        }
        public static void UnRegisterClass(string className)
        {
            AllClasses.Remove(className);
        }
        public static Dictionary<string,ObjectClass> AllClasses
        {
            get { return s_allClasses; }
            set { s_allClasses = value; }
        }
        private static Dictionary<string, ObjectClass> s_allClasses =
            new Dictionary<string, ObjectClass>();
       
    }


    public class PropertySettingEventArgs : EventArgs
    {
        /// <summary>
        /// プロパティに代入されようとしている変数の内容
        /// </summary>
        public Variable Value { get; set; }
        /// <summary>
        /// プロパティへの代入がキャンセルされたか否かを表す値
        /// </summary>
        public bool Cancel { get; set; }
        /// <summary>
        /// 変更を要求したクラスインスタンス
        /// </summary>
        public ObjectBase Instance { get; set; }
    }
    public class PropertyGettingEventArgs : EventArgs
    {
        /// <summary>
        /// プロパティの変数の内容
        /// </summary>
        public Variable Value { get; set; }
        /// <summary>
        /// プロパティへの代入がキャンセルされたか否かを表す値
        /// </summary>
        public bool Cancel { get; set; }
        /// <summary>
        /// 変更を要求したクラスインスタンス
        /// </summary>
        public ObjectBase Instance { get; set; }
    }
    public delegate void PropertySettingEventHandler(object sender, PropertySettingEventArgs e);

    public delegate void PropertyGettingEventHandler(object sender, PropertyGettingEventArgs e);

    public class PropertyBase
    {
        /// <summary>
        /// このプロパティの名前
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// TrueにするとSettingイベントおよびGettingイベントが発生します
        /// </summary>

        public bool HandleEvents { get; set; }
        /// <summary>
        /// このプロパティが継承されたとき、このプロパティに継承元のクラスから呼び出しが必要かどうかを表す値を表します。
        /// </summary>
        public bool NeedCallFromParent { get; set; }
        /// <summary>
        /// プロパティに変数が代入されるときに発生するイベント。このイベントはHandleEventsがTrueの場合のみ発生します
        /// </summary>
        public event PropertySettingEventHandler Setting;
        /// <summary>
        /// プロパティから変数が読みだされるときに発生するイベント。このイベントはHandleEventsがTrueの場合のみ発生します
        /// </summary>

        public event PropertyGettingEventHandler Getting;
        /// <summary>
        /// プロパティに値を代入可能かを表す値。デフォルトではTrueです。
        /// </summary>
        public bool CanSet
        {
            get
            {
                return m_CanSet;
            }
            set
            {
                m_CanSet = value;
            }
        }
        /// <summary>
        /// プロパティから値を取得可能かを表す値。デフォルトではTrueです。
        /// </summary>
        public bool CanGet
        {
            get
            {
                return m_CanGet;
            }
            set
            {
                m_CanGet = value;
            }
        }
        /// <summary>
        /// このプロパティから初めて読みだされたときのデフォルトの値
        /// </summary>
        public Variable Default
        {
            get
            {
                return m_default;
            }
            set
            {
                m_default = value;
            }
        }
        private Variable m_default = Variable.EmptyInstance;
        private bool m_CanSet = true;
        private bool m_CanGet = true;
        private Dictionary<ObjectBase, Variable> m_Values = new Dictionary<ObjectBase, Variable>();
        /// <summary>
        /// このプロパティから値を取得します
        /// </summary>
        /// <param name="instance">取得を要求しているインスタンス</param>
        /// <returns>このプロパティの値。失敗した場合はEmpty。</returns>
        public Variable GetProperty(ObjectBase instance)
        {
            if (m_CanGet)
            {
                if (HandleEvents)
                {
                    PropertyGettingEventArgs e = new PropertyGettingEventArgs();
                    if(m_Values.TryGetValue(instance,out Variable value))
                    {
                        e.Value = value;
                    }
                    else
                    {
                        m_Values.Add(instance,Default.DeepClone());
                        e.Value = m_Values[instance];
                    }
                    e.Instance = instance;
                    Getting?.Invoke(this, e);
                    m_Values[instance] = e.Value;
                    return e.Value;
                }
                else
                {

                    if (m_Values.TryGetValue(instance, out Variable value))
                    {
                        return value;
                    }
                    else
                    {
                        m_Values.Add(instance, Default.DeepClone());
                        return m_Values[instance];
                    }
                }
            }
            else
            {
                ThrowErrorManerger.OnThrowError("このプロパティから値を取得することはできません", Exceptions.COULDNT_GET_THIS_PROPERTY);
                return Variable.EmptyInstance;
            }
        }
        /// <summary>
        /// このプロパティに値を代入します
        /// </summary>
        /// <param name="value">代入したいプロパティ</param>
        /// <param name="instance">代入を要求しているインスタンス</param>
        /// <returns>代入が成功したかどうかを表す値</returns>
        public bool SetProperty(Variable value , ObjectBase instance)
        {
            if (m_CanSet)
            {
                if (HandleEvents)
                {
                    PropertySettingEventArgs e = new PropertySettingEventArgs();
                    e.Instance = instance;e.Value = value;
                    Setting?.Invoke(this, e);
                    if (!e.Cancel)
                    {
                        m_Values[instance] = value;
                    }
                    return e.Cancel;
                }
                else
                {
                    m_Values[instance] = value;
                    return true;
                }
            }
            else
            {
                ThrowErrorManerger.OnThrowError("このプロパティに代入できません", Exceptions.COULDNT_ASSIGN_THIS_PROPERTY);
                return false;
            }
        }
        /// <summary>
        /// 新規にプロパティを作成します
        /// </summary>
        /// <param name="value">プロパティの値</param>
        /// <param name="name">プロパティの名前</param>
        /// <param name="canSet">値を代入可能かを表す値</param>
        /// <param name="canGet">値を取得可能かを表す値</param>
        /// <returns>作成されたプロパティ</returns>
        public static PropertyBase NewInstance(Variable value,string name="",bool canSet=true,bool canGet=true)
        {
            PropertyBase property = new PropertyBase();
            property.Default = value;
            property.Name = name;
            property.CanSet = canSet;
            property.CanGet = canGet;
            return property;
        }

    }
}
