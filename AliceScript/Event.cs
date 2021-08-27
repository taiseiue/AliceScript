using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AliceScript
{
   public class EventObject:ObjectBase
    {
        public static bool overDDerror = false;
        public EventObject()
        {
           
            this.Functions.Add("Invoke",new e_doFunc(this));
            
        }
        public List<CustomFunction> Event = new List<CustomFunction>();
        public void Invoke(List<Variable> args)
        {
            //とりあえずイベントポンプ中は変数無効エラーを抑制
            EventObject.overDDerror = true;
          foreach(CustomFunction cf in Event)
            {
                cf.Run(args);
            }
            EventObject.overDDerror = false;
        }
        public void BeginInvoke(List<Variable> args)
        {
            //とりあえずイベントポンプ中は変数無効エラーを抑制
            EventObject.overDDerror = true;
            foreach (CustomFunction cf in Event)
            {
                m_BeginInvokeMessanger mb = new m_BeginInvokeMessanger();
                mb.Args = args;
                mb.Delegate = cf;
                ThreadPool.QueueUserWorkItem(ThreadProc, mb);
            }
            EventObject.overDDerror = false;
        }
        static void ThreadProc(Object stateInfo)
        {
            m_BeginInvokeMessanger mb = (m_BeginInvokeMessanger)stateInfo;
            mb.Delegate.Run(mb.Args);
        }
        private class m_BeginInvokeMessanger
        {
            public CustomFunction Delegate { get; set; }
            public List<Variable> Args { get; set; }
        }
        public override void Operator(Variable left, Variable right, string action)
        {
            switch (action)
            {
                case "+=":
                    if (left == null || right==null) { return; }
            if (right.Type != Variable.VarType.DELEGATE) { return; }
                    
               Event.Add(right.AsDelegate());
                    break;
                case "-=":
                    if (left == null || right == null) { return; }
                    if (right.Type != Variable.VarType.DELEGATE) { return; }
                    if (Event.Contains(right.AsDelegate())) { Event.Remove(right.AsDelegate()); }
                   
                    break;
            }
           
            
        }
      
        private void AddVars(List<Variable> args)
        {
           
            if (sins.Count > args.Count) { ThrowErrorManerger.OnThrowError("引数が不足しています");return; }
            int pos = 0;
            foreach(string s in sins)
            {
                AliceScript.Diagnosis.Variables.Add(s,args[pos]);
                pos++;
            }
           
        }
        public List<string> sins = new List<string>();
        
    }
    class EventFunc : FunctionBase
    {
        public EventFunc()
        {
            this.FunctionName = "event";

        }
        protected override Variable Evaluate(ParsingScript script)
        {
            string[] sins = Utils.GetFunctionSignature(script);
            EventObject eo = new EventObject();
            eo.sins.AddRange(sins);
            return new Variable(eo);
        }
    }
   
   
    class e_doFunc : FunctionBase
    {
        public e_doFunc(EventObject host)
        {
            Host = host;
            FunctionName = "Invoke";
            this.Run += E_doFunc_Run;
        
        }

        private void E_doFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            Host.Invoke(e.Args);
        }

        public EventObject Host;
    }
    class e_do2Func : FunctionBase
    {
        public e_do2Func(EventObject host)
        {
            Host = host;
            FunctionName = "BeginInvoke";
            this.Run += E_doFunc_Run;

        }

        private void E_doFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            Host.BeginInvoke(e.Args);
        }

        public EventObject Host;
    }
}
