using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AliceScript
{
    public class ObjectBase : ScriptObject
    {


        public Dictionary<string, Variable> Properties = new Dictionary<string, Variable>();
        public Dictionary<string, FunctionBase> Functions = new Dictionary<string, FunctionBase>();
        public Dictionary<string, EventObject> Events = new Dictionary<string, EventObject>();

        public ObjectBase(string name = "", string color = "")
        {
            Properties.Add("Name", new Variable("ObjectBase"));

        }


        public virtual List<string> GetProperties()
        {
            List<string> v = new List<string>(Properties.Keys);
            v.AddRange(new List<string>(Functions.Keys));
            v.AddRange(new List<string>(Events.Keys));
            return v;
        }

        public event PropertyGettingEventHandler PropertyGetting;
        public event PropertySettingEventHandler PropertySetting;

        public static bool GETTING = false;
        public static List<Variable> LaskVariable;

        public virtual void Operator(Variable left, Variable right, string action)
        {
            //継承先によって定義されます
        }
        public virtual Task<Variable> GetProperty(string sPropertyName, List<Variable> args = null, ParsingScript script = null)
        {


            sPropertyName = Variable.GetActualPropertyName(sPropertyName, GetProperties());
            if (Properties.ContainsKey(sPropertyName))
            {

                Variable v = null;
                PropertyGettingEventArgs ev = new PropertyGettingEventArgs();
                ev.Variable = Properties[sPropertyName];
                ev.PropertyName = sPropertyName;
                PropertyGetting?.Invoke(this, ev);
                v = ev.Variable;
                return Task.FromResult(v);

            }
            else
            {

                if (Functions.ContainsKey(sPropertyName))
                {

                    //issue#1「ObjectBase内の関数で引数が認識されない」に対する対処
                    //原因:先に値検出関数にポインタが移動されているため正常に引数が認識できていない
                    //対処:値検出関数で拾った引数のリストをバックアップし、関数で使用する
                    //ただしこれは、根本的な解決にはなっていない可能性がある
                    GETTING = true;

                    Task<Variable> va = Task.FromResult(Functions[sPropertyName].GetValue(script));
                    GETTING = false;
                    return va;

                }
                else if (Events.ContainsKey(sPropertyName))
                {
                    return Task.FromResult(new Variable(Events[sPropertyName]));
                }
                else
                {
                    return Task.FromResult(Variable.EmptyInstance);
                }
            }
        }

        public virtual Task<Variable> SetProperty(string sPropertyName, Variable argValue)
        {

            sPropertyName = Variable.GetActualPropertyName(sPropertyName, GetProperties());
            if (Properties.ContainsKey(sPropertyName))
            {

                PropertySettingEventArgs ev = new PropertySettingEventArgs();
                ev.Cancel = false;
                ev.PropertyName = sPropertyName;
                ev.Variable = argValue;
                PropertySetting?.Invoke(this, ev);
                if (!ev.Cancel)
                {
                    Properties[sPropertyName] = ev.Variable;
                }

            }
            else if (Events.ContainsKey(sPropertyName))
            {
                if (argValue.Object is EventObject e)
                {
                    Events[sPropertyName] = e;
                }
            }

            return Task.FromResult(Variable.EmptyInstance);
        }
        public string ClassName = "ObjectBase";

    }


    public class ObjectBaseManerger
    {
        public static void AddObject(ObjectBase obj)
        {
            if (obj != null)
            {
                ParserFunction.RegisterFunction(obj.ClassName, new GetVarFunction(new Variable(obj)), true);
            }
        }
    }

    public class PropertySettingEventArgs : EventArgs
    {
        /// <summary>
        /// 変更されるプロパティの名前
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// 変更される内容
        /// </summary>
        public Variable Variable { get; set; }

        /// <summary>
        /// キャンセルするには、このプロパティをTrueにします
        /// </summary>
        public bool Cancel { get; set; }
    }
    public class PropertyGettingEventArgs : EventArgs
    {
        /// <summary>
        /// 読み取られるプロパティの名前
        /// </summary>
        public string PropertyName { get; set; }
        /// <summary>
        /// 送られるプロパティの内容
        /// </summary>
        public Variable Variable { get; set; }
    }
    public delegate void PropertySettingEventHandler(object sender, PropertySettingEventArgs e);

    public delegate void PropertyGettingEventHandler(object sender, PropertyGettingEventArgs e);
}
