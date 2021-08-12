using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace AliceScript
{
    static class VariableFunctionIniter
    {
        public static void Init()
        {
            //総合関数(VariableFunction.csに本体あり)
            Variable.AddFunc(new DisposeFunc());
            //String関数
            Variable.AddFunc(new string_TrimFunc(0), "Trim");
            Variable.AddFunc(new string_TrimFunc(1), "TrimStart");
            Variable.AddFunc(new string_TrimFunc(2), "TrimEnd");
            Variable.AddFunc(new str_SEWithFunc(false), "StartsWith");
            Variable.AddFunc(new str_SEWithFunc(true), "EndsWith");
            Variable.AddFunc(new str_PadFunc(false), "PadLeft");
            Variable.AddFunc(new str_PadFunc(true), "PadRight");
            Variable.AddFunc(new str_NormalizeFunc());
            Variable.AddFunc(new str_IsMatchFunc());
            Variable.AddFunc(new str_MatchesFunc());
            Variable.AddFunc(new str_ReplaceMatchesFunc());
            //String関数(終わり)
            //NUMBER(Double)関数
            Variable.AddFunc(new num_EpsilonFunc());
            Variable.AddFunc(new num_InfinityFunc(true), "NegativeInfinity");
            Variable.AddFunc(new num_InfinityFunc(false), "PositiveInfinity");
            Variable.AddFunc(new num_NANFunc());
            Variable.AddFunc(new num_MValueFunc(true), "MinValue");
            Variable.AddFunc(new num_MValueFunc(false), "MaxValue");
            //NUMBER(Double)関数(終わり)
            //List関数
            Variable.AddFunc(new list_flattenFunc());
            Variable.AddFunc(new list_marge2Func());
            //List関数(終わり)
            //BYTES系(Base64)
            Variable.AddFunc(new toBase64Func());
            Variable.AddFunc(new fromBase64Func());
            //BYTES系(終わり)
            //DELEGATE系(Delegate.csに本体あり)
            Variable.AddFunc(new InvokeFunc());
            Variable.AddFunc(new BeginInvokeFunc());
            //DELEGATE系(終わり)
        }
    }
    class InvokeFunc : FunctionBase
    {
        public InvokeFunc()
        {
            this.FunctionName = "Invoke";
            this.RequestType = Variable.VarType.DELEGATE;
            this.Run += InvokeFunc_Run;
        }

        private void InvokeFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.CurentVariable.Delegate != null)
            {
                e.Return = e.CurentVariable.Delegate.Run(e.Args);
            }

        }
       
    }
    class BeginInvokeFunc : FunctionBase
    {
        public BeginInvokeFunc()
        {
            this.Name = "BeginInvoke";
            this.RequestType = Variable.VarType.DELEGATE;
            this.Run += BeginInvokeFunc_Run;
        }

        private void BeginInvokeFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.CurentVariable.Delegate != null)
            {
                m_BeginInvokeMessanger mb = new m_BeginInvokeMessanger();
                mb.Delegate = e.CurentVariable.Delegate;
                mb.Args = e.Args;
                ThreadPool.QueueUserWorkItem(ThreadProc, mb);
            }
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
        
    }
    class DisposeFunc : FunctionBase
    {

        public DisposeFunc()
        {
            this.FunctionName = "Dispose";
            this.Run += DisposeFunc_Run;

        }

        private void DisposeFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.CurentVariable.Reset();

        }
    }
    //ここより下は変数(Variable)オブジェクトの関数です
    class toBase64Func : FunctionBase
    {
        public toBase64Func()
        {
            this.FunctionName = "ToBase64";
            this.RequestType = Variable.VarType.BYTE_ARRAY;
            this.Run += ToBase64Func_Run;
        }

        private void ToBase64Func_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = Variable.FromText(System.Convert.ToBase64String(e.CurentVariable.AsByteArray()));
        }
    }
    class fromBase64Func : FunctionBase
    {
        public fromBase64Func()
        {
            this.FunctionName = "FromBase64";
            this.RequestType = Variable.VarType.STRING;
            this.Run += FromBase64Func_Run;
        }

        private void FromBase64Func_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(System.Convert.FromBase64String(e.CurentVariable.AsString()));
        }
    }
    class string_TrimFunc : FunctionBase
    {
        public string_TrimFunc(int trimtype = 0)
        {
            this.TrimType = trimtype;
            this.FunctionName = "Trim";
            this.RequestType = Variable.VarType.STRING;
            this.Run += String_TrimFunc_Run;
        }
        int TrimType = 0;
        private void String_TrimFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            switch (TrimType)
            {
                case 0:
                    {
                        if (e.Args.Count == 0)
                        {
                            string baseStr = e.CurentVariable.AsString();
                            e.Return = new Variable(baseStr.Trim());
                        }
                        else
                        {
                            string baseStr = e.CurentVariable.AsString();

                            foreach (Variable v in e.Args)
                            {
                                if (v.Type.HasFlag(Variable.VarType.STRING))
                                {
                                    baseStr = baseStr.Trim(v.AsString().ToCharArray());
                                }
                            }
                            e.Return = new Variable(baseStr);
                        }
                        break;
                    }
                case 1:
                    {
                        if (e.Args.Count == 0)
                        {
                            string baseStr = e.CurentVariable.AsString();
                            e.Return = new Variable(baseStr.TrimStart());
                        }
                        else
                        {
                            string baseStr = e.CurentVariable.AsString();

                            foreach (Variable v in e.Args)
                            {
                                if (v.Type.HasFlag(Variable.VarType.STRING))
                                {
                                    baseStr = baseStr.TrimStart(v.AsString().ToCharArray());
                                }
                            }
                            e.Return = new Variable(baseStr);
                        }
                        break;
                    }
                case 2:
                    {
                        if (e.Args.Count == 0)
                        {
                            string baseStr = e.CurentVariable.AsString();
                            e.Return = new Variable(baseStr.TrimEnd());
                        }
                        else
                        {
                            string baseStr = e.CurentVariable.AsString();

                            foreach (Variable v in e.Args)
                            {
                                if (v.Type.HasFlag(Variable.VarType.STRING))
                                {
                                    baseStr = baseStr.TrimEnd(v.AsString().ToCharArray());
                                }
                            }
                            e.Return = new Variable(baseStr);
                        }
                        break;
                    }

            }
        }
    }
    class str_IsMatchFunc : FunctionBase
    {
        public str_IsMatchFunc()
        {
            this.FunctionName = "ismatch";
            this.MinimumArgCounts = 1;
            this.RequestType = Variable.VarType.STRING;
            this.Run += Str_IsMatchFunc_Run;
        }

        private void Str_IsMatchFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable((System.Text.RegularExpressions.Regex.IsMatch(e.CurentVariable.AsString(), e.Args[0].AsString())));
        }
    }
    class str_MatchesFunc : FunctionBase
    {
        public str_MatchesFunc()
        {
            this.FunctionName = "matches";
            this.MinimumArgCounts = 1;
            this.RequestType = Variable.VarType.STRING;
            this.Run += Str_IsMatchFunc_Run;
        }

        private void Str_IsMatchFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            System.Text.RegularExpressions.MatchCollection mc = System.Text.RegularExpressions.Regex.Matches(e.CurentVariable.AsString(), e.Args[0].AsString());
            Variable r = new Variable(Variable.VarType.ARRAY);
            foreach (System.Text.RegularExpressions.Match m in mc)
            {
                r.Tuple.Add(new Variable(m.Value));
            }
            e.Return = r;
        }
    }
    class str_ReplaceMatchesFunc : FunctionBase
    {
        public str_ReplaceMatchesFunc()
        {
            this.FunctionName = "replacematches";
            this.MinimumArgCounts = 2;
            this.RequestType = Variable.VarType.STRING;
            this.Run += Str_IsMatchFunc_Run;
        }

        private void Str_IsMatchFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable((System.Text.RegularExpressions.Regex.Replace(e.CurentVariable.AsString(), e.Args[0].AsString(), e.Args[1].AsString())));
        }
    }
    class str_SEWithFunc : FunctionBase
    {
        public str_SEWithFunc(bool endsWith = false)
        {
            this.EndWith = endsWith;
            this.MinimumArgCounts = 1;
            this.RequestType = Variable.VarType.STRING;
            this.Run += Str_SEWithFunc_Run;
        }

        private void Str_SEWithFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (EndWith)
            {
                if (e.CurentVariable.AsString().EndsWith(e.Args[0].AsString()))
                {
                    e.Return = Variable.True;
                }
                else
                {
                    e.Return = Variable.False;
                }
            }
            else
            {
                if (e.CurentVariable.AsString().StartsWith(e.Args[0].AsString()))
                {
                    e.Return = Variable.True;
                }
                else
                {
                    e.Return = Variable.False;
                }
            }
        }

        bool EndWith = false;
    }
    class str_PadFunc : FunctionBase
    {
        public str_PadFunc(bool right = false)
        {
            this.Right = right;
            this.MinimumArgCounts = 1;
            this.RequestType = Variable.VarType.STRING;
            this.Run += Str_PadFunc_Run;
        }

        private void Str_PadFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (Right)
            {
                if (e.Args.Count > 1)
                {
                    e.Return = new Variable(e.CurentVariable.AsString().PadRight(e.Args[0].AsInt(), e.Args[1].AsString().ToCharArray()[0]));
                }
                else
                {
                    e.Return = new Variable(e.CurentVariable.AsString().PadRight(e.Args[0].AsInt()));
                }
            }
            else
            {
                if (e.Args.Count > 1)
                {
                    e.Return = new Variable(e.CurentVariable.AsString().PadLeft(e.Args[0].AsInt(), e.Args[1].AsString().ToCharArray()[0]));
                }
                else
                {
                    e.Return = new Variable(e.CurentVariable.AsString().PadLeft(e.Args[0].AsInt()));
                }
            }
        }

        bool Right = false;
    }
    class str_NormalizeFunc : FunctionBase
    {
        public str_NormalizeFunc()
        {
            this.FunctionName = "Normalize";
            this.RequestType = Variable.VarType.STRING;
            this.Run += Str_NormalizeFunc_Run;
        }

        private void Str_NormalizeFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(e.CurentVariable.AsString().Normalize());
        }
    }
    class num_MValueFunc : FunctionBase
    {
        public num_MValueFunc(bool min)
        {
            Min = min;
            this.RequestType = Variable.VarType.NUMBER;
            this.Run += Num_MValueFunc_Run;
        }

        private void Num_MValueFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (Min)
            {
                e.Return = new Variable(double.MinValue);
            }
            else
            {
                e.Return = new Variable(double.MaxValue);
            }
        }

        bool Min = false;
    }
    class num_EpsilonFunc : FunctionBase
    {
        public num_EpsilonFunc()
        {
            this.FunctionName = "Epsilon";
            this.RequestType = Variable.VarType.NUMBER;
            this.Run += Num_MValueFunc_Run;
        }

        private void Num_MValueFunc_Run(object sender, FunctionBaseEventArgs e)
        {

            e.Return = new Variable(double.Epsilon);

        }


    }
    class num_NANFunc : FunctionBase
    {
        public num_NANFunc()
        {
            this.FunctionName = "NaN";
            this.RequestType = Variable.VarType.NUMBER;
            this.Run += Num_MValueFunc_Run;
        }

        private void Num_MValueFunc_Run(object sender, FunctionBaseEventArgs e)
        {

            e.Return = new Variable(double.NaN);

        }


    }
    class num_InfinityFunc : FunctionBase
    {
        public num_InfinityFunc(bool min)
        {
            Min = min;
            this.RequestType = Variable.VarType.NUMBER;
            this.Run += Num_MValueFunc_Run;
        }

        private void Num_MValueFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (Min)
            {
                e.Return = new Variable(double.NegativeInfinity);
            }
            else
            {
                e.Return = new Variable(double.PositiveInfinity);

            }
        }

        bool Min = false;
    }
    class list_flattenFunc : FunctionBase
    {
        public list_flattenFunc()
        {
            this.FunctionName = "Flatten";
            this.RequestType = Variable.VarType.ARRAY;
            this.Run += List_flattenFunc_Run;
        }

        private void List_flattenFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            Variable v = new Variable();
            foreach (var strLst in e.CurentVariable.Tuple)
            {
                if (strLst.Type == Variable.VarType.ARRAY)
                {
                    v.Tuple.AddRange(strLst.Tuple);
                }
                else
                {
                    v.Tuple.Add(strLst);
                }
            }
        }
    }
    class list_marge2Func : FunctionBase
    {
        public list_marge2Func()
        {

            this.FunctionName = "Merge";
            this.RequestType = Variable.VarType.ARRAY;
            this.Run += List_marge2Func_Run;
        }

        private void List_marge2Func_Run(object sender, FunctionBaseEventArgs e)
        {
            Variable r = new Variable(Variable.VarType.ARRAY);

            r.Tuple.AddRange(e.CurentVariable.Tuple);

            foreach (Variable v1 in e.Args)
            {
                if (v1.Type == Variable.VarType.ARRAY)
                {
                    r.Tuple.AddRange(v1.Tuple);
                }
                else
                {
                    r.Tuple.Add(v1);
                }
            }

            e.Return = r;
        }
    }
}
