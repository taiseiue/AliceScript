using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliceScript
{
   public class EventObject:ObjectBase
    {
        public static bool overDDerror = false;
        public EventObject()
        {
           
            this.Functions.Add("do",new e_doFunc(this));
            
        }
        public List<CustomFunction> Event = new List<CustomFunction>();
        public void Do(List<Variable> args)
        {
            //とりあえずイベントポンプ中は変数無効エラーを抑制
            EventObject.overDDerror = true;
          foreach(CustomFunction cf in Event)
            {
                cf.Run(args);
            }
            EventObject.overDDerror = false;
        }
        public override void Operator(Variable left, Variable right, string action)
        {
            switch (action)
            {
                case "+=":
                    if (left == null || right==null) { return; }
            if (right.Type != Variable.VarType.DELEGATE) { return; }
                    Console.WriteLine("DELEGATE_ADDR_TO_LEFT");
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
            FunctionName = "do";
            this.Run += E_doFunc_Run;
        
        }

        private void E_doFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            Host.Do(e.Args);
        }

        public EventObject Host;
    }
}
