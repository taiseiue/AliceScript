using System;
using System.Collections.Generic;
using System.Text;

namespace AliceScript.NameSpaces
{
    static class Alice_Interpreter_Initer
    {
        public static void Init()
        {
            NameSpace space = new NameSpace("Alice.Interpreter");

            space.Add(new Interpreter_Reset_VariablesFunc());
            space.Add(new Interpreter_Append_OutputOrDataFunc());
            space.Add(new Interpreter_Append_OutputOrDataFunc(true));
            space.Add(new Interpreter_ProcessOrFileFunc());
            space.Add(new Interpreter_ProcessOrFileFunc(true));
            space.Add(new Interpreter_GetVariable());
            space.Add(new Interpreter_NamespacesFunc());
            space.Add(new Interpreter_FunctionsFunc());
            space.Add(new Interpreter_VariablesFunc());
            space.Add(new ScheduleRunFunction(true));
            space.Add(new ScheduleRunFunction(false));

            NameSpaceManerger.Add(space);
        }
    }
    class Interpreter_Reset_VariablesFunc : FunctionBase
    {
        public Interpreter_Reset_VariablesFunc()
        {
            this.Name = "Interpreter_Reset_Variables";
            this.Run += Interpreter_Reset_VariablesFunc_Run;
        }

        private void Interpreter_Reset_VariablesFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            ParserFunction.CleanUpVariables();
        }
    }
    class Interpreter_Append_OutputOrDataFunc : FunctionBase
    {
        public Interpreter_Append_OutputOrDataFunc(bool isdata = false)
        {
            m_isData = isdata;
            if (isdata)
            {
                this.Name = "Interpreter_Append_Data";
            }
            else
            {
                this.Name = "Interpreter_Append_Data";
            }
            this.MinimumArgCounts = 1;
            this.Run += Interpreter_Append_OutputOrDataFunc_Run;
        }

        private void Interpreter_Append_OutputOrDataFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (m_isData)
            {
                e.Return=new Variable(Interpreter.Instance.AppendData(e.Args[0].AsString(),(Utils.GetSafeInt(e.Args,1)!=0)));
            }
            else
            {
                Interpreter.Instance.AppendOutput(e.Args[0].AsString(), (Utils.GetSafeInt(e.Args, 1) != 0));
            }
        }

        private bool m_isData=false;
    }
    class Interpreter_ProcessOrFileFunc : FunctionBase
    {
        public Interpreter_ProcessOrFileFunc(bool isfile = false)
        {
            m_isFile = isfile;
            if (m_isFile) { this.Name = "Interpreter_ProcessFile"; } else { this.Name = "Interpreter_Process"; }
            this.MinimumArgCounts = 1;
            this.Run += Interpreter_ProcessOrFileFunc_Run;
        }

        private void Interpreter_ProcessOrFileFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (m_isFile)
            {
                Interpreter.Instance.ProcessFile(e.Args[0].AsString(),(Utils.GetSafeInt(e.Args,1)!=0));
            }
            else
            {
                Interpreter.Instance.Process(e.Args[0].AsString(),Utils.GetSafeString(e.Args,1), (Utils.GetSafeInt(e.Args, 2) != 0));
            }
        }

        private bool m_isFile = false;
    }
    class Interpreter_FunctionsFunc : FunctionBase
    {
        public Interpreter_FunctionsFunc()
        {
            this.Name = "Interpreter_Functions";
            this.Run += FunctionsFunc_Run;
        }

        private void FunctionsFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args.Count == 0)
            {
                Variable v = new Variable(Variable.VarType.ARRAY);
                foreach (string s in FunctionBaseManerger.Functions)
                {
                    v.Tuple.Add(new Variable(s));
                }
                e.Return = v;
            }
            else
            {
                string str = Utils.GetSafeString(e.Args, 0);
                if (NameSpaceManerger.Contains(str))
                {
                    Variable v = new Variable(Variable.VarType.ARRAY);
                    foreach (FunctionBase fb in NameSpaceManerger.NameSpaces[str].Functions)
                    {
                        v.Tuple.Add(new Variable(fb.Name));
                    }
                    e.Return = v;
                }
                else
                {
                    throw new System.Exception("指定された名前空間が見つかりませんでした");
                }
            }
        }
    }
    class Interpreter_NamespacesFunc : FunctionBase
    {
        public Interpreter_NamespacesFunc()
        {
            this.FunctionName = "Interpreter_Namespaces";
            this.Run += NamespacesFunc_Run;
        }

        private void NamespacesFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            Variable v = new Variable(Variable.VarType.ARRAY);
            foreach (string s in NameSpaceManerger.NameSpaces.Keys)
            {
                v.Tuple.Add(new Variable(s));
            }
            e.Return = v;
        }
    }
    class Interpreter_VariablesFunc : FunctionBase
    {
        public Interpreter_VariablesFunc()
        {
            this.Name = "Interpreter_Variables";
            this.Run += Interpreter_VariablesFunc_Run;
        }

        private void Interpreter_VariablesFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            Variable v = new Variable(Variable.VarType.ARRAY);
            foreach (string s in Debug.Variables.Keys)
            {
                v.Tuple.Add(new Variable(s));
            }
            e.Return = v;
        }
    }
    class Interpreter_GetVariable : FunctionBase
    {
        public Interpreter_GetVariable()
        {
            this.Name = "Interpreter_GetVariable";
            this.MinimumArgCounts = 1;
            this.Run += Interpreter_GetVariable_Run;
        }

        private void Interpreter_GetVariable_Run(object sender, FunctionBaseEventArgs e)
        {
            if (Debug.Variables.ContainsKey(e.Args[0].AsString()))
            {
                e.Return = Debug.Variables[e.Args[0].AsString()];
            }
            else
            {
                ThrowErrorManerger.OnThrowError("指定された変数名の変数は定義されていません",e.Script);
            }
        }
    }
    class ScheduleRunFunction : FunctionBase
    {
        static Dictionary<string, System.Timers.Timer> m_timers =
           new Dictionary<string, System.Timers.Timer>();

        bool m_startTimer;

        public ScheduleRunFunction(bool startTimer)
        {
            if (startTimer)
            {
                this.Name = "Interpreter_Scadule";
            }
            else { this.Name = "Interpreter_Scadule_Cancel"; }
            this.MinimumArgCounts = 4;
            m_startTimer = startTimer;
            this.Run += ScheduleRunFunction_Run;
        }

        private void ScheduleRunFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            if (!m_startTimer)
            {
                string cancelTimerId = Utils.GetSafeString(e.Args, 0);
                System.Timers.Timer cancelTimer;
                if (m_timers.TryGetValue(cancelTimerId, out cancelTimer))
                {
                    cancelTimer.Stop();
                    cancelTimer.Dispose();
                    m_timers.Remove(cancelTimerId);
                }
                e.Return = Variable.EmptyInstance;
            }
            int timeout = e.Args[0].AsInt();
            CustomFunction delAction = e.Args[3].AsDelegate();
            string timerId = Utils.GetSafeString(e.Args, 1);
            bool autoReset = Utils.GetSafeInt(e.Args, 2, 0) != 0;

            timerId = Utils.ProtectQuotes(timerId);
            List<Variable> args = new List<Variable>();
            if (e.Args.Count > 4)
            {
                //引数が登録されているとき
                args = e.Args.GetRange(4, e.Args.Count - 4);
            }

            System.Timers.Timer pauseTimer = new System.Timers.Timer(timeout);
            pauseTimer.Elapsed += (sender, ex) =>
            {
                if (!autoReset)
                {
                    pauseTimer.Stop();
                    pauseTimer.Dispose();
                    m_timers.Remove(timerId);
                }
                delAction.Run(args, e.Script);
            };
            pauseTimer.AutoReset = autoReset;
            m_timers[timerId] = pauseTimer;

            pauseTimer.Start();

            e.Return = Variable.EmptyInstance;
        }
    }

}
