﻿using System;
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
            space.Add(new Interpreter_NameExistsFunc());
            space.Add(new Interpreter_ProcessOrFileFunc());
            space.Add(new Interpreter_ProcessOrFileFunc(true));
            space.Add(new Interpreter_GetVariable());
            space.Add(new Interpreter_NamespacesFunc());
            space.Add(new Interpreter_FunctionsFunc());
            space.Add(new Interpreter_VariablesFunc());
            space.Add(new Interpreter_ConstsFunc());
            space.Add(new ScheduleRunFunction(true));
            space.Add(new ScheduleRunFunction(false));
            space.Add(new Interpreter_GetScriptFunc());
            space.Add(new gc_collectFunc());
            space.Add(new gc_gettotalmemoryFunc());
            space.Add(new gc_collectafterexecuteFunc());
            space.Add(new Function_ShowFunc());
            space.Add(new Debug_PrintFunction());
            space.Add(new Debug_PrintFunction(true));
            space.Add(new Debug_PrintFunction(true,true));
            space.Add(new Debug_PrintFunction(false,true));
            space.Add(new Debug_IndentFunction());
            space.Add(new Debug_IndentFunction(true));
            space.Add(new Debug_IndentLevelFunction());

            NameSpaceManerger.Add(space);
        }
    }
    class Debug_IndentFunction : FunctionBase
    {
        public Debug_IndentFunction(bool unindent = false)
        {
            m_UnIndent = unindent;
            if (this.m_UnIndent)
            {
                this.Name = "Debug_Unindent";
            }
            else
            {
                this.Name = "Debug_Indent";
            }
            this.Run += Debug_IndentFunction_Run;
        }

        private void Debug_IndentFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            if (m_UnIndent && IndentLevel > 0)
            {
                IndentLevel--;
            }else if (!m_UnIndent)
            {
                IndentLevel++;
            }
        }
        public static int IndentLevel = 0;
        private bool m_UnIndent = false;
    }
    class Debug_IndentLevelFunction : FunctionBase
    {
        public Debug_IndentLevelFunction()
        {
            this.Name = "Debug_IndentLevel";
            this.Run += Debug_IndentLevelFunction_Run;
        }

        private void Debug_IndentLevelFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args.Count > 0 && e.Args[0].Type == Variable.VarType.NUMBER)
            {
                Debug_IndentFunction.IndentLevel = e.Args[0].AsInt();
            }
            e.Return = new Variable(Debug_IndentFunction.IndentLevel);
        }
    }
    class Debug_PrintFunction : FunctionBase
    {
        public Debug_PrintFunction(bool iswrite=false,bool isif=false)
        {
            string name = "Debug_";
            if (iswrite)
            {
                name += "Write";
            }
            else
            {
                name += "Print";
            }
            if (isif)
            {
                name += "If";
            }
            if (isif)
            {
                this.MinimumArgCounts = 2;
            }
            else
            {
                this.MinimumArgCounts = 1;
            }
            m_isIf = isif;
            m_isWrite = iswrite;
            this.Name = name;
            this.Run += PrintFunction_Run;
        }
        private bool m_isIf = false;
        private bool m_isWrite = false;
        private void PrintFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            if (!m_isIf||e.Args[0].AsBool())
            {
                int countnum = 0;
                if (m_isIf) { countnum++; }
                if (e.Args.Count == countnum)
                {
                    AddDebugOutput("", e.Script, !m_isWrite);
                }
                else if (e.Args.Count == countnum + 1)
                {
                    AddDebugOutput(e.Args[countnum].AsString(), e.Script, !m_isWrite);
                }
                else
                {
                    string format = e.Args[countnum].AsString();
                    for (int i = 0; i < countnum+1; i++)
                    {
                        e.Args.RemoveAt(0);
                    }
                    AddDebugOutput(StringFormatFunction.Format(format, e.Args), e.Script, !m_isWrite);
                }
            }
        }

        public static void AddDebugOutput(string text, ParsingScript script = null,
                                     bool addLine = true, bool addSpace = true, string start = "",string indent =" ")
        {
            string indents = "";
            for (int i = 0; i < Debug_IndentFunction.IndentLevel; i++)
            {
                indents += indent;
            }
            string output = indents+text + (addLine ? Environment.NewLine : string.Empty);
            Interpreter.Instance.AppendDebug(output);
        }
    }
    class Interpreter_NameExistsFunc : FunctionBase
    {
        public Interpreter_NameExistsFunc()
        {
            this.Name = "Interpreter_NameExists";
            this.Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
            this.Run += Interpreter_NameExistsFunc_Run;
        }

        private void Interpreter_NameExistsFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            string varName = Utils.GetToken(e.Script, Constants.TOKEN_SEPARATION);
            varName = Constants.ConvertName(varName);

            bool result = ParserFunction.GetVariable(varName, e.Script) != null;
            e.Return= new Variable(result);
        }
    }
    class Function_ShowFunc : FunctionBase
    {
        public Function_ShowFunc()
        {
            this.Name = "function_show";
            this.MinimumArgCounts = 1;
            this.Run += Function_ShowFunc_Run;
        }

        private void Function_ShowFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            string functionName = e.Args[0].AsString();

            CustomFunction custFunc = ParserFunction.GetFunction(functionName, e.Script) as CustomFunction;
            Utils.CheckNotNull(functionName, custFunc, e.Script);



            string body = Utils.BeautifyScript(custFunc.Body, custFunc.Header);
            Utils.PrintScript(body, e.Script);
            e.Return = new Variable(body);
        }
    }
    class gc_collectFunc : FunctionBase
    {
        public gc_collectFunc()
        {
            this.Name = "gc_collect";
            this.MinimumArgCounts = 0;
            this.Run += Gc_collectFunc_Run;
        }

        private void Gc_collectFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            GC.Collect();
        }
    }
    class gc_gettotalmemoryFunc : FunctionBase
    {
        public gc_gettotalmemoryFunc()
        {
            this.Name = "gc_gettotalmemory";
            this.MinimumArgCounts = 1;
            this.Run += Gc_gettotalmemoryFunc_Run;
        }

        private void Gc_gettotalmemoryFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            GC.GetTotalMemory(e.Args[0].AsBool());
        }
    }
    class gc_collectafterexecuteFunc : FunctionBase
    {
        public gc_collectafterexecuteFunc()
        {
            this.Name = "gc_collectafterexecute";
            this.MinimumArgCounts = 0;
            this.Run += Gc_collectafterexecuteFunc_Run;
        }

        private void Gc_collectafterexecuteFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args.Count > 0)
            {
                AliceScript.Interop.GCManerger.CollectAfterExecute = e.Args[0].AsBool();
            }
            e.Return = new Variable(Interop.GCManerger.CollectAfterExecute);
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
                this.Name = "Interpreter_Append_Output";
            }
            this.MinimumArgCounts = 1;
            this.Run += Interpreter_Append_OutputOrDataFunc_Run;
        }

        private void Interpreter_Append_OutputOrDataFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (m_isData)
            {
                e.Return=new Variable(Interpreter.Instance.AppendData(e.Args[0].AsString(), (Utils.GetSafeBool(e.Args, 1))));
            }
            else
            {
                Interpreter.Instance.AppendOutput(e.Args[0].AsString(), (Utils.GetSafeBool(e.Args,1)));
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
                e.Return=Interpreter.Instance.ProcessFile(e.Args[0].AsString(), (Utils.GetSafeBool(e.Args, 1)));
            }
            else
            {
                e.Return=Interpreter.Instance.Process(e.Args[0].AsString(),Utils.GetSafeString(e.Args,1), (Utils.GetSafeBool(e.Args, 2)));
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
            foreach (string s in Diagnosis.Variables.Keys)
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
            if (Diagnosis.Variables.ContainsKey(e.Args[0].AsString()))
            {
                e.Return = Diagnosis.Variables[e.Args[0].AsString()];
            }
            else
            {
                ThrowErrorManerger.OnThrowError("指定された変数名の変数は定義されていません",Exceptions.COULDNT_FIND_VARIABLE,e.Script);
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
            DelegateObject delAction = e.Args[3].AsDelegate();
            string timerId = Utils.GetSafeString(e.Args, 1);
            bool autoReset = (Utils.GetSafeBool(e.Args, 2));

            timerId = Utils.ProtectQuotes(timerId);
            List<Variable> args = new List<Variable>();
            if (e.Args.Count > 4)
            {
                //引数が登録されているとき
                args = e.Args.GetRange(4, e.Args.Count - 4);
            }

            System.Timers.Timer pauseTimer = new System.Timers.Timer(timeout);
            pauseTimer.Elapsed += (senders, ex) =>
            {
                if (!autoReset)
                {
                    pauseTimer.Stop();
                    pauseTimer.Dispose();
                    m_timers.Remove(timerId);
                }
                delAction.Invoke(args, e.Script);
            };
            pauseTimer.AutoReset = autoReset;
            m_timers[timerId] = pauseTimer;

            pauseTimer.Start();

            e.Return = Variable.EmptyInstance;
        }
    }
    class Interpreter_GetScriptFunc : FunctionBase
    {
        public Interpreter_GetScriptFunc()
        {
            this.Name = "Interpreter_GetScript";
            this.Run += Interpreter_GetScriptFunc_Run;
        }

        private void Interpreter_GetScriptFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(new Interpreter_ScriptObject(e.Script));
        }
    }
    class Interpreter_ConstsFunc : FunctionBase
    {
        public Interpreter_ConstsFunc()
        {
            this.Name = "Interpreter_Consts";
            this.MinimumArgCounts = 0;
            this.Attribute = FunctionAttribute.GENERAL;
            this.Run += Interpreter_ConstsFunc_Run;
        }

        private void Interpreter_ConstsFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            Variable v = new Variable(Variable.VarType.ARRAY);
            foreach(string s in Constants.CONSTS.Keys)
            {
                v.Tuple.Add(Variable.FromText(s));
            }
            e.Return = v;
        }
    }
    class Interpreter_ScriptObject : ObjectBase
    {
        public Interpreter_ScriptObject(ParsingScript script)
        {
            this.Name = "Script";
            this.AddProperty(new Interpreter_ScriptObject_Property(Interpreter_ScriptObject_Property.Interpreter_ScriptObject_Property_Mode.IsMainFile,this));
            this.AddProperty(new Interpreter_ScriptObject_Property(Interpreter_ScriptObject_Property.Interpreter_ScriptObject_Property_Mode.FileName, this));
            this.AddProperty(new Interpreter_ScriptObject_Property(Interpreter_ScriptObject_Property.Interpreter_ScriptObject_Property_Mode.PWD, this));
            this.AddProperty(new Interpreter_ScriptObject_Property(Interpreter_ScriptObject_Property.Interpreter_ScriptObject_Property_Mode.OriginalScript, this));
            this.AddProperty(new Interpreter_ScriptObject_Property(Interpreter_ScriptObject_Property.Interpreter_ScriptObject_Property_Mode.FunctionName, this));
            this.AddProperty(new Interpreter_ScriptObject_Property(Interpreter_ScriptObject_Property.Interpreter_ScriptObject_Property_Mode.InTryBlock, this));
            this.AddProperty(new Interpreter_ScriptObject_Property(Interpreter_ScriptObject_Property.Interpreter_ScriptObject_Property_Mode.StillValid, this));
            this.AddProperty(new Interpreter_ScriptObject_Property(Interpreter_ScriptObject_Property.Interpreter_ScriptObject_Property_Mode.Size, this));
            this.AddProperty(new Interpreter_ScriptObject_Property(Interpreter_ScriptObject_Property.Interpreter_ScriptObject_Property_Mode.OriginalLineNumber, this));
            this.AddProperty(new Interpreter_ScriptObject_Property(Interpreter_ScriptObject_Property.Interpreter_ScriptObject_Property_Mode.OriginalLine, this));
            this.AddProperty(new Interpreter_ScriptObject_Property(Interpreter_ScriptObject_Property.Interpreter_ScriptObject_Property_Mode.Labels, this));
            this.AddProperty(new Interpreter_ScriptObject_Property(Interpreter_ScriptObject_Property.Interpreter_ScriptObject_Property_Mode.Generation, this));

            Script = script;
        }
        private ParsingScript Script;
        class Interpreter_ScriptObject_Property : PropertyBase
        {
            public Interpreter_ScriptObject_Property(Interpreter_ScriptObject_Property_Mode mode,Interpreter_ScriptObject host)
            {
                Mode = mode;
                Host = host;
                this.Name = Mode.ToString();
                this.CanSet = false;
                this.HandleEvents = true;
                this.Getting += Interpreter_ScriptObject_Property_Getting;
            }
            private Interpreter_ScriptObject_Property_Mode Mode;
            private Interpreter_ScriptObject Host;
            internal enum Interpreter_ScriptObject_Property_Mode
            {
                IsMainFile,FileName,PWD,OriginalScript,FunctionName,InTryBlock, StillValid, Size, OriginalLineNumber, OriginalLine, Labels,Generation
            }
            private void Interpreter_ScriptObject_Property_Getting(object sender, PropertyGettingEventArgs e)
            {
                switch (Mode)
                {
                    case Interpreter_ScriptObject_Property_Mode.IsMainFile:
                        {
                            bool isMain = !string.IsNullOrWhiteSpace(Host.Script.MainFilename) &&
                           Host.Script.MainFilename == Host.Script.Filename;
                            e.Value = new Variable(isMain);
                            break;
                    }
                    case Interpreter_ScriptObject_Property_Mode.FileName:
                        {
                                e.Value= new Variable(Host.Script.Filename);
                            break;
                        }
                    case Interpreter_ScriptObject_Property_Mode.PWD:
                        {
                            e.Value = new Variable(Host.Script.PWD);
                            break;
                        }
                    case Interpreter_ScriptObject_Property_Mode.OriginalScript:
                        {
                            e.Value = new Variable(Host.Script.OriginalScript);
                            break;
                        }
                    case Interpreter_ScriptObject_Property_Mode.FunctionName:
                        {
                            e.Value = new Variable(Host.Script.FunctionName);
                            break;
                        }
                    case Interpreter_ScriptObject_Property_Mode.InTryBlock:
                        {
                            e.Value = new Variable(Host.Script.InTryBlock);
                            break;
                        }
                    case Interpreter_ScriptObject_Property_Mode.StillValid:
                        {
                            e.Value = new Variable(Host.Script.StillValid());
                            break;
                        }
                    case Interpreter_ScriptObject_Property_Mode.Size:
                        {
                            e.Value = new Variable(Host.Script.Size());
                            break;
                        }
                    case Interpreter_ScriptObject_Property_Mode.OriginalLineNumber:
                        {
                            e.Value = new Variable(Host.Script.OriginalLineNumber);
                            break;
                        }
                    case Interpreter_ScriptObject_Property_Mode.OriginalLine:
                        {
                            e.Value = new Variable(Host.Script.OriginalLine);
                            break;
                        }
                    case Interpreter_ScriptObject_Property_Mode.Labels:
                        {
                            Variable v = new Variable(Variable.VarType.ARRAY);
                            if (Host.Script.AllLabels == null)
                            {
                                e.Value = Variable.EmptyInstance;
                                break;
                            }
                            foreach(string s in Host.Script.AllLabels.Keys)
                            {
                                v.Tuple.Add(new Variable(s));
                            }
                            e.Value = v;
                            break;
                        }
                    case Interpreter_ScriptObject_Property_Mode.Generation:
                        {
                            e.Value = new Variable(Host.Script.Generation);
                            break;
                        }
                }
            }
        }
    }

}
